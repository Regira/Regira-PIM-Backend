namespace PIM.Data;

/// <summary>
/// Recursive walks over the facet taxonomy, which spans three link tables: FacetLink (facet → facet),
/// FacetChildGroup (facet → group) and FacetParentGroup (group → facet).
/// </summary>
/// <remarks>
/// <para>Seed selection, unchanged from the single-function form:</para>
/// <list type="bullet">
///   <item>neither id list → every seed (the <c>...All</c> variant)</item>
///   <item>only <c>@facetIds</c> → only facet-started seeds that match</item>
///   <item>only <c>@groupIds</c> → only group-started seeds that match</item>
///   <item>both → matching facet-started seeds AND matching group-started seeds</item>
/// </list>
/// <para>
/// The two variants exist because a nullable-id predicate cannot produce a seekable plan; see
/// <see cref="ProductDbFunctions"/> for the measurements. In the <c>ByIds</c> variant a missing list
/// becomes an empty JSON array, so its seed branches simply contribute nothing — which is what the
/// third and fourth rules above already required.
/// </para>
/// </remarks>
public class FacetDbFunctions
{
    private const string FacetIdsJson = "OPENJSON(COALESCE(NULLIF(@facetIds, N''), N'[]'))";
    private const string GroupIdsJson = "OPENJSON(COALESCE(NULLIF(@groupIds, N''), N'[]'))";

    private const string FacetNotArchived = "NOT EXISTS (SELECT 1 FROM Facets f2 WHERE f2.Id = {0} AND f2.IsArchived = 1)";
    private const string GroupNotArchived = "NOT EXISTS (SELECT 1 FROM FacetGroups g2 WHERE g2.Id = {0} AND g2.IsArchived = 1)";

    /// <summary>The three recursive members, identical for both variants and for both directions of travel.</summary>
    private static string OffspringRecursion() => $"""
            UNION ALL
            -- Recursive: Facet → Facet (FacetLink)
            SELECT fl.ParentId, CAST(N'Facet' AS NVARCHAR(16)), fl.ChildId, CAST(N'Facet' AS NVARCHAR(16)), o.Level + 1, o.RootId, o.RootType
            FROM   FacetLink fl
            INNER JOIN offspring o ON o.ChildId = fl.ParentId AND o.ChildType = N'Facet'
            WHERE  (@max_level IS NULL OR o.Level < @max_level)
                   AND {string.Format(FacetNotArchived, "fl.ParentId")}
                   AND {string.Format(FacetNotArchived, "fl.ChildId")}
            UNION ALL
            -- Recursive: Facet → FacetGroup (FacetChildGroup)
            SELECT fcg.FacetId, CAST(N'Facet' AS NVARCHAR(16)), fcg.FacetGroupId, N'FacetGroup', o.Level + 1, o.RootId, o.RootType
            FROM   FacetChildGroup fcg
            INNER JOIN offspring o ON o.ChildId = fcg.FacetId AND o.ChildType = N'Facet'
            WHERE  (@max_level IS NULL OR o.Level < @max_level)
                   AND {string.Format(FacetNotArchived, "fcg.FacetId")}
                   AND {string.Format(GroupNotArchived, "fcg.FacetGroupId")}
            UNION ALL
            -- Recursive: FacetGroup → Facet (FacetParentGroup)
            SELECT fpg.FacetGroupId, N'FacetGroup', fpg.FacetId, CAST(N'Facet' AS NVARCHAR(16)), o.Level + 1, o.RootId, o.RootType
            FROM   FacetParentGroup fpg
            INNER JOIN offspring o ON o.ChildId = fpg.FacetGroupId AND o.ChildType = N'FacetGroup'
            WHERE  (@max_level IS NULL OR o.Level < @max_level)
                   AND {string.Format(GroupNotArchived, "fpg.FacetGroupId")}
                   AND {string.Format(FacetNotArchived, "fpg.FacetId")}
        )
        SELECT * FROM offspring
        """;

    private static string AncestorsRecursion() => $"""
            UNION ALL
            -- Recursive: further up from Facet via FacetLink
            SELECT fl.ParentId, CAST(N'Facet' AS NVARCHAR(16)), fl.ChildId, CAST(N'Facet' AS NVARCHAR(16)), a.Level + 1, a.RootId, a.RootType
            FROM   FacetLink fl
            INNER JOIN ancestors a ON a.ParentId = fl.ChildId AND a.ParentType = N'Facet'
            WHERE  (@max_level IS NULL OR a.Level < @max_level)
                   AND {string.Format(FacetNotArchived, "fl.ParentId")}
                   AND {string.Format(FacetNotArchived, "fl.ChildId")}
            UNION ALL
            -- Recursive: further up from Facet via FacetParentGroup (facet is child → group is parent)
            SELECT fpg.FacetGroupId, N'FacetGroup', fpg.FacetId, CAST(N'Facet' AS NVARCHAR(16)), a.Level + 1, a.RootId, a.RootType
            FROM   FacetParentGroup fpg
            INNER JOIN ancestors a ON a.ParentId = fpg.FacetId AND a.ParentType = N'Facet'
            WHERE  (@max_level IS NULL OR a.Level < @max_level)
                   AND {string.Format(FacetNotArchived, "fpg.FacetId")}
                   AND {string.Format(GroupNotArchived, "fpg.FacetGroupId")}
            UNION ALL
            -- Recursive: further up from FacetGroup via FacetChildGroup (group is child → facet is parent)
            SELECT fcg.FacetId, CAST(N'Facet' AS NVARCHAR(16)), fcg.FacetGroupId, N'FacetGroup', a.Level + 1, a.RootId, a.RootType
            FROM   FacetChildGroup fcg
            INNER JOIN ancestors a ON a.ParentId = fcg.FacetGroupId AND a.ParentType = N'FacetGroup'
            WHERE  (@max_level IS NULL OR a.Level < @max_level)
                   AND {string.Format(GroupNotArchived, "fcg.FacetGroupId")}
                   AND {string.Format(FacetNotArchived, "fcg.FacetId")}
        )
        SELECT * FROM ancestors
        """;

    private static string OffspringSql(bool byIds) => $"""
        WITH offspring (ParentId, ParentType, ChildId, ChildType, Level, RootId, RootType) AS (
            -- Seed: Facet → Facet (FacetLink)
            SELECT fl.ParentId, CAST(N'Facet' AS NVARCHAR(16)), fl.ChildId, CAST(N'Facet' AS NVARCHAR(16)), 0, fl.ParentId, CAST(N'Facet' AS NVARCHAR(16))
            {(byIds
                ? $"FROM   {FacetIdsJson} jf\n            INNER JOIN FacetLink fl ON fl.ParentId = CAST(jf.value AS INT)"
                : "FROM   FacetLink fl")}
            WHERE  {string.Format(FacetNotArchived, "fl.ParentId")}
                   AND {string.Format(FacetNotArchived, "fl.ChildId")}
            UNION ALL
            -- Seed: Facet → FacetGroup (FacetChildGroup)
            SELECT fcg.FacetId, CAST(N'Facet' AS NVARCHAR(16)), fcg.FacetGroupId, N'FacetGroup', 0, fcg.FacetId, CAST(N'Facet' AS NVARCHAR(16))
            {(byIds
                ? $"FROM   {FacetIdsJson} jf\n            INNER JOIN FacetChildGroup fcg ON fcg.FacetId = CAST(jf.value AS INT)"
                : "FROM   FacetChildGroup fcg")}
            WHERE  {string.Format(FacetNotArchived, "fcg.FacetId")}
                   AND {string.Format(GroupNotArchived, "fcg.FacetGroupId")}
            UNION ALL
            -- Seed: FacetGroup → Facet (FacetParentGroup)
            SELECT fpg.FacetGroupId, N'FacetGroup', fpg.FacetId, CAST(N'Facet' AS NVARCHAR(16)), 0, fpg.FacetGroupId, N'FacetGroup'
            {(byIds
                ? $"FROM   {GroupIdsJson} jg\n            INNER JOIN FacetParentGroup fpg ON fpg.FacetGroupId = CAST(jg.value AS INT)"
                : "FROM   FacetParentGroup fpg")}
            WHERE  {string.Format(GroupNotArchived, "fpg.FacetGroupId")}
                   AND {string.Format(FacetNotArchived, "fpg.FacetId")}
        {OffspringRecursion()}
        """;

    private static string AncestorsSql(bool byIds) => $"""
        WITH ancestors (ParentId, ParentType, ChildId, ChildType, Level, RootId, RootType) AS (
            -- Seed: Facet ← Facet (FacetLink, start from ChildId)
            SELECT fl.ParentId, CAST(N'Facet' AS NVARCHAR(16)), fl.ChildId, CAST(N'Facet' AS NVARCHAR(16)), 0, fl.ChildId, CAST(N'Facet' AS NVARCHAR(16))
            {(byIds
                ? $"FROM   {FacetIdsJson} jf\n            INNER JOIN FacetLink fl ON fl.ChildId = CAST(jf.value AS INT)"
                : "FROM   FacetLink fl")}
            WHERE  {string.Format(FacetNotArchived, "fl.ParentId")}
                   AND {string.Format(FacetNotArchived, "fl.ChildId")}
            UNION ALL
            -- Seed: Facet ← FacetGroup (FacetParentGroup, start from FacetId)
            SELECT fpg.FacetGroupId, N'FacetGroup', fpg.FacetId, CAST(N'Facet' AS NVARCHAR(16)), 0, fpg.FacetId, CAST(N'Facet' AS NVARCHAR(16))
            {(byIds
                ? $"FROM   {FacetIdsJson} jf\n            INNER JOIN FacetParentGroup fpg ON fpg.FacetId = CAST(jf.value AS INT)"
                : "FROM   FacetParentGroup fpg")}
            WHERE  {string.Format(FacetNotArchived, "fpg.FacetId")}
                   AND {string.Format(GroupNotArchived, "fpg.FacetGroupId")}
            UNION ALL
            -- Seed: FacetGroup ← Facet (FacetChildGroup, start from FacetGroupId)
            SELECT fcg.FacetId, CAST(N'Facet' AS NVARCHAR(16)), fcg.FacetGroupId, N'FacetGroup', 0, fcg.FacetGroupId, N'FacetGroup'
            {(byIds
                ? $"FROM   {GroupIdsJson} jg\n            INNER JOIN FacetChildGroup fcg ON fcg.FacetGroupId = CAST(jg.value AS INT)"
                : "FROM   FacetChildGroup fcg")}
            WHERE  {string.Format(GroupNotArchived, "fcg.FacetGroupId")}
                   AND {string.Format(FacetNotArchived, "fcg.FacetId")}
        {AncestorsRecursion()}
        """;

    public static readonly string CREATE_GetFacetOffspringByIds = $"""
        CREATE OR ALTER FUNCTION [dbo].[GetFacetOffspringByIds] (@facetIds NVARCHAR(MAX) = NULL, @groupIds NVARCHAR(MAX) = NULL, @max_level INT = 9)
        RETURNS TABLE AS RETURN
            {OffspringSql(byIds: true)};
        """;

    public static readonly string CREATE_GetFacetOffspringAll = $"""
        CREATE OR ALTER FUNCTION [dbo].[GetFacetOffspringAll] (@max_level INT = 9)
        RETURNS TABLE AS RETURN
            {OffspringSql(byIds: false)};
        """;

    public static readonly string CREATE_GetFacetAncestorsByIds = $"""
        CREATE OR ALTER FUNCTION [dbo].[GetFacetAncestorsByIds] (@facetIds NVARCHAR(MAX) = NULL, @groupIds NVARCHAR(MAX) = NULL, @max_level INT = 9)
        RETURNS TABLE AS RETURN
            {AncestorsSql(byIds: true)};
        """;

    public static readonly string CREATE_GetFacetAncestorsAll = $"""
        CREATE OR ALTER FUNCTION [dbo].[GetFacetAncestorsAll] (@max_level INT = 9)
        RETURNS TABLE AS RETURN
            {AncestorsSql(byIds: false)};
        """;

    public static readonly string CREATE_GetFacetFamilyByIds = """
        CREATE OR ALTER FUNCTION [dbo].[GetFacetFamilyByIds] (@facetIds NVARCHAR(MAX) = NULL, @groupIds NVARCHAR(MAX) = NULL, @max_level INT = 9)
        RETURNS TABLE AS RETURN
        (
            SELECT ParentId, ParentType, ChildId, ChildType, -(Level + 1) AS Level, RootId, RootType
            FROM   [dbo].[GetFacetAncestorsByIds](@facetIds, @groupIds, @max_level)
            UNION ALL
            SELECT ParentId, ParentType, ChildId, ChildType, Level + 1 AS Level, RootId, RootType
            FROM   [dbo].[GetFacetOffspringByIds](@facetIds, @groupIds, @max_level)
        );
        """;

    public static readonly string CREATE_GetFacetFamilyAll = """
        CREATE OR ALTER FUNCTION [dbo].[GetFacetFamilyAll] (@max_level INT = 9)
        RETURNS TABLE AS RETURN
        (
            SELECT ParentId, ParentType, ChildId, ChildType, -(Level + 1) AS Level, RootId, RootType
            FROM   [dbo].[GetFacetAncestorsAll](@max_level)
            UNION ALL
            SELECT ParentId, ParentType, ChildId, ChildType, Level + 1 AS Level, RootId, RootType
            FROM   [dbo].[GetFacetOffspringAll](@max_level)
        );
        """;

    public static readonly string DROP_Superseded = """
        DROP FUNCTION IF EXISTS [dbo].[GetFacetFamily];
        DROP FUNCTION IF EXISTS [dbo].[GetFacetOffspring];
        DROP FUNCTION IF EXISTS [dbo].[GetFacetAncestors];
        """;

    public static string[] CREATE_ALL =>
    [
        CREATE_GetFacetOffspringByIds, CREATE_GetFacetOffspringAll,
        CREATE_GetFacetAncestorsByIds, CREATE_GetFacetAncestorsAll,
        CREATE_GetFacetFamilyByIds, CREATE_GetFacetFamilyAll,
        DROP_Superseded
    ];
}
