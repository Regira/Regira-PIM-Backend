namespace PIM.Data;

public class FacetDbFunctions
{
    // Filter logic per seed:
    //   both null/empty  → include ALL seeds (no filter)
    //   only @facetIds   → include only Facet-starting seeds that match
    //   only @groupIds   → include only FacetGroup-starting seeds that match
    //   both provided    → include matching Facet-starting AND matching FacetGroup-starting seeds

    private const string FacetSeedCondition =
        "((@facetIds IS NULL OR @facetIds = N'') AND (@groupIds IS NULL OR @groupIds = N'')" +
        " OR @facetIds IS NOT NULL AND @facetIds <> N'' AND";

    private const string GroupSeedCondition =
        "((@facetIds IS NULL OR @facetIds = N'') AND (@groupIds IS NULL OR @groupIds = N'')" +
        " OR @groupIds IS NOT NULL AND @groupIds <> N'' AND";

    private const string FacetOffspringSQL = $"""
        WITH offspring (ParentId, ParentType, ChildId, ChildType, Level, RootId, RootType) AS (
            -- Seed: Facet → Facet (FacetLink)
            SELECT fl.ParentId, CAST(N'Facet' AS NVARCHAR(16)), fl.ChildId, CAST(N'Facet' AS NVARCHAR(16)), 0, fl.ParentId, CAST(N'Facet' AS NVARCHAR(16))
            FROM   FacetLink fl
            WHERE  {FacetSeedCondition} fl.ParentId IN (SELECT CAST(value AS INT) FROM OPENJSON(@facetIds)))
                   AND NOT EXISTS (SELECT 1 FROM Facets f WHERE f.Id IN (fl.ParentId, fl.ChildId) AND f.IsArchived = 1)
            UNION ALL
            -- Seed: Facet → FacetGroup (FacetChildGroup: FacetId=parent, FacetGroupId=child)
            SELECT fcg.FacetId, N'Facet', fcg.FacetGroupId, N'FacetGroup', 0, fcg.FacetId, N'Facet'
            FROM   FacetChildGroup fcg
            WHERE  {FacetSeedCondition} fcg.FacetId IN (SELECT CAST(value AS INT) FROM OPENJSON(@facetIds)))
                   AND NOT EXISTS (SELECT 1 FROM Facets f WHERE f.Id = fcg.FacetId AND f.IsArchived = 1)
                   AND NOT EXISTS (SELECT 1 FROM FacetGroups g WHERE g.Id = fcg.FacetGroupId AND g.IsArchived = 1)
            UNION ALL
            -- Seed: FacetGroup → Facet (FacetParentGroup: FacetGroupId=parent, FacetId=child)
            SELECT fpg.FacetGroupId, N'FacetGroup', fpg.FacetId, N'Facet', 0, fpg.FacetGroupId, N'FacetGroup'
            FROM   FacetParentGroup fpg
            WHERE  {GroupSeedCondition} fpg.FacetGroupId IN (SELECT CAST(value AS INT) FROM OPENJSON(@groupIds)))
                   AND NOT EXISTS (SELECT 1 FROM FacetGroups g WHERE g.Id = fpg.FacetGroupId AND g.IsArchived = 1)
                   AND NOT EXISTS (SELECT 1 FROM Facets f WHERE f.Id = fpg.FacetId AND f.IsArchived = 1)
            UNION ALL
            -- Recursive: Facet → Facet (FacetLink)
            SELECT fl.ParentId, N'Facet', fl.ChildId, N'Facet', o.Level + 1, o.RootId, o.RootType
            FROM   FacetLink fl
            INNER JOIN offspring o ON o.ChildId = fl.ParentId AND o.ChildType = N'Facet'
            WHERE  (@max_level IS NULL OR o.Level < @max_level)
                   AND NOT EXISTS (SELECT 1 FROM Facets f WHERE f.Id IN (fl.ParentId, fl.ChildId) AND f.IsArchived = 1)
            UNION ALL
            -- Recursive: Facet → FacetGroup (FacetChildGroup)
            SELECT fcg.FacetId, N'Facet', fcg.FacetGroupId, N'FacetGroup', o.Level + 1, o.RootId, o.RootType
            FROM   FacetChildGroup fcg
            INNER JOIN offspring o ON o.ChildId = fcg.FacetId AND o.ChildType = N'Facet'
            WHERE  (@max_level IS NULL OR o.Level < @max_level)
                   AND NOT EXISTS (SELECT 1 FROM Facets f WHERE f.Id = fcg.FacetId AND f.IsArchived = 1)
                   AND NOT EXISTS (SELECT 1 FROM FacetGroups g WHERE g.Id = fcg.FacetGroupId AND g.IsArchived = 1)
            UNION ALL
            -- Recursive: FacetGroup → Facet (FacetParentGroup)
            SELECT fpg.FacetGroupId, N'FacetGroup', fpg.FacetId, N'Facet', o.Level + 1, o.RootId, o.RootType
            FROM   FacetParentGroup fpg
            INNER JOIN offspring o ON o.ChildId = fpg.FacetGroupId AND o.ChildType = N'FacetGroup'
            WHERE  (@max_level IS NULL OR o.Level < @max_level)
                   AND NOT EXISTS (SELECT 1 FROM FacetGroups g WHERE g.Id = fpg.FacetGroupId AND g.IsArchived = 1)
                   AND NOT EXISTS (SELECT 1 FROM Facets f WHERE f.Id = fpg.FacetId AND f.IsArchived = 1)
        )
        SELECT * FROM offspring
        """;

    private const string FacetAncestorsSQL = $"""
        WITH ancestors (ParentId, ParentType, ChildId, ChildType, Level, RootId, RootType) AS (
            -- Seed: Facet ← Facet (FacetLink, start from ChildId)
            SELECT fl.ParentId, CAST(N'Facet' AS NVARCHAR(16)), fl.ChildId, CAST(N'Facet' AS NVARCHAR(16)), 0, fl.ChildId, CAST(N'Facet' AS NVARCHAR(16))
            FROM   FacetLink fl
            WHERE  {FacetSeedCondition} fl.ChildId IN (SELECT CAST(value AS INT) FROM OPENJSON(@facetIds)))
                   AND NOT EXISTS (SELECT 1 FROM Facets f WHERE f.Id IN (fl.ParentId, fl.ChildId) AND f.IsArchived = 1)
            UNION ALL
            -- Seed: Facet ← FacetGroup (FacetParentGroup: FacetGroupId=parent, FacetId=child; start from FacetId)
            SELECT fpg.FacetGroupId, N'FacetGroup', fpg.FacetId, N'Facet', 0, fpg.FacetId, N'Facet'
            FROM   FacetParentGroup fpg
            WHERE  {FacetSeedCondition} fpg.FacetId IN (SELECT CAST(value AS INT) FROM OPENJSON(@facetIds)))
                   AND NOT EXISTS (SELECT 1 FROM Facets f WHERE f.Id = fpg.FacetId AND f.IsArchived = 1)
                   AND NOT EXISTS (SELECT 1 FROM FacetGroups g WHERE g.Id = fpg.FacetGroupId AND g.IsArchived = 1)
            UNION ALL
            -- Seed: FacetGroup ← Facet (FacetChildGroup: FacetId=parent, FacetGroupId=child; start from FacetGroupId)
            SELECT fcg.FacetId, N'Facet', fcg.FacetGroupId, N'FacetGroup', 0, fcg.FacetGroupId, N'FacetGroup'
            FROM   FacetChildGroup fcg
            WHERE  {GroupSeedCondition} fcg.FacetGroupId IN (SELECT CAST(value AS INT) FROM OPENJSON(@groupIds)))
                   AND NOT EXISTS (SELECT 1 FROM FacetGroups g WHERE g.Id = fcg.FacetGroupId AND g.IsArchived = 1)
                   AND NOT EXISTS (SELECT 1 FROM Facets f WHERE f.Id = fcg.FacetId AND f.IsArchived = 1)
            UNION ALL
            -- Recursive: go further up from Facet via FacetLink
            SELECT fl.ParentId, N'Facet', fl.ChildId, N'Facet', a.Level + 1, a.RootId, a.RootType
            FROM   FacetLink fl
            INNER JOIN ancestors a ON a.ParentId = fl.ChildId AND a.ParentType = N'Facet'
            WHERE  (@max_level IS NULL OR a.Level < @max_level)
                   AND NOT EXISTS (SELECT 1 FROM Facets f WHERE f.Id IN (fl.ParentId, fl.ChildId) AND f.IsArchived = 1)
            UNION ALL
            -- Recursive: go further up from Facet via FacetParentGroup (Facet is child → FacetGroup is parent)
            SELECT fpg.FacetGroupId, N'FacetGroup', fpg.FacetId, N'Facet', a.Level + 1, a.RootId, a.RootType
            FROM   FacetParentGroup fpg
            INNER JOIN ancestors a ON a.ParentId = fpg.FacetId AND a.ParentType = N'Facet'
            WHERE  (@max_level IS NULL OR a.Level < @max_level)
                   AND NOT EXISTS (SELECT 1 FROM Facets f WHERE f.Id = fpg.FacetId AND f.IsArchived = 1)
                   AND NOT EXISTS (SELECT 1 FROM FacetGroups g WHERE g.Id = fpg.FacetGroupId AND g.IsArchived = 1)
            UNION ALL
            -- Recursive: go further up from FacetGroup via FacetChildGroup (FacetGroup is child → Facet is parent)
            SELECT fcg.FacetId, N'Facet', fcg.FacetGroupId, N'FacetGroup', a.Level + 1, a.RootId, a.RootType
            FROM   FacetChildGroup fcg
            INNER JOIN ancestors a ON a.ParentId = fcg.FacetGroupId AND a.ParentType = N'FacetGroup'
            WHERE  (@max_level IS NULL OR a.Level < @max_level)
                   AND NOT EXISTS (SELECT 1 FROM FacetGroups g WHERE g.Id = fcg.FacetGroupId AND g.IsArchived = 1)
                   AND NOT EXISTS (SELECT 1 FROM Facets f WHERE f.Id = fcg.FacetId AND f.IsArchived = 1)
        )
        SELECT * FROM ancestors
        """;

    public static readonly string CREATE_GetFacetOffspring = $@"
CREATE OR ALTER FUNCTION [dbo].[GetFacetOffspring] (@facetIds NVARCHAR(MAX) = NULL, @groupIds NVARCHAR(MAX) = NULL, @max_level INT = 9)
RETURNS TABLE AS RETURN
    {FacetOffspringSQL};";

    public static readonly string CREATE_GetFacetAncestors = $@"
CREATE OR ALTER FUNCTION [dbo].[GetFacetAncestors] (@facetIds NVARCHAR(MAX) = NULL, @groupIds NVARCHAR(MAX) = NULL, @max_level INT = 9)
RETURNS TABLE AS RETURN
    {FacetAncestorsSQL};";

    public static readonly string CREATE_GetFacetFamily = """
        CREATE OR ALTER FUNCTION [dbo].[GetFacetFamily] (@facetIds NVARCHAR(MAX) = NULL, @groupIds NVARCHAR(MAX) = NULL, @max_level INT = 9)
        RETURNS TABLE AS RETURN
        (
            SELECT ParentId, ParentType, ChildId, ChildType, -(Level + 1) AS Level, RootId, RootType
            FROM   [dbo].[GetFacetAncestors](@facetIds, @groupIds, @max_level)
            UNION ALL
            SELECT ParentId, ParentType, ChildId, ChildType, Level + 1 AS Level, RootId, RootType
            FROM   [dbo].[GetFacetOffspring](@facetIds, @groupIds, @max_level)
        );
        """;

    public static string[] CREATE_ALL => [CREATE_GetFacetOffspring, CREATE_GetFacetAncestors, CREATE_GetFacetFamily];
}
