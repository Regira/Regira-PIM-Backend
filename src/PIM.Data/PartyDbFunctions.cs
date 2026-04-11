namespace PIM.Data;

public class PartyDbFunctions
{
    private const string PartyOffspringSQL = """
        WITH offspring (ParentId, ChildId, RelationshipTypeId, Level, RootId) AS (
            SELECT        r.ParentId, r.ChildId, r.RelationshipTypeId, 0, r.ParentId
            FROM          PartyRelationship r
            WHERE         (@ids IS NULL OR @ids = '' OR r.ParentId IN (SELECT CAST(value AS INT) FROM OPENJSON(@ids)))
                  AND NOT EXISTS (SELECT 1 FROM Parties s WHERE (s.Id = r.ChildId OR s.Id = r.ParentId) AND s.IsArchived = 1)
            UNION ALL
            SELECT        sc.ParentId, sc.ChildId, sc.RelationshipTypeId, offspring.Level + 1, offspring.RootId
            FROM          PartyRelationship sc
            INNER JOIN    offspring ON offspring.ChildId = sc.ParentId
            WHERE         (@max_level IS NULL OR offspring.Level < @max_level)
                  AND NOT EXISTS (SELECT 1 FROM Parties s WHERE (s.Id = sc.ChildId OR s.Id = sc.ParentId) AND s.IsArchived = 1)
        )
        SELECT * FROM offspring
        """;

    private const string PartyAncestorsSQL = """
        WITH ancestors (ParentId, ChildId, RelationshipTypeId, Level, RootId) AS (
        
            SELECT        r.ParentId, r.ChildId, r.RelationshipTypeId, 0, r.ChildId
            FROM          PartyRelationship r
            WHERE         (@ids IS NULL OR @ids = '' OR r.ChildId IN (SELECT CAST(value AS INT) FROM OPENJSON(@ids)))
                          AND NOT EXISTS (SELECT 1 FROM Parties s WHERE (s.Id = r.ChildId OR s.Id = r.ParentId) AND s.IsArchived = 1)
            UNION ALL
            SELECT        sc.ParentId, sc.ChildId, sc.RelationshipTypeId, ancestors.Level + 1, ancestors.RootId
            FROM          PartyRelationship sc
            INNER JOIN    ancestors ON ancestors.ParentId = sc.ChildId
            WHERE         (@max_level IS NULL OR ancestors.Level < @max_level)
                          AND NOT EXISTS (SELECT 1 FROM Parties s WHERE (s.Id = sc.ChildId OR s.Id = sc.ParentId) AND s.IsArchived = 1)
        )
        SELECT * FROM ancestors
        """;

    public static readonly string CREATE_GetPartyOffspring = $@"
CREATE OR ALTER FUNCTION [dbo].[GetPartyOffspring] (@ids NVARCHAR(MAX) = NULL, @max_level INT = 9)
RETURNS TABLE AS RETURN
    {PartyOffspringSQL};";

    public static readonly string CREATE_GetPartyAncestors = $@"
CREATE OR ALTER FUNCTION [dbo].[GetPartyAncestors] (@ids NVARCHAR(MAX) = NULL, @max_level INT = 9)
RETURNS TABLE AS RETURN
    {PartyAncestorsSQL};";

    public static readonly string CREATE_GetPartyFamily = """
        CREATE OR ALTER FUNCTION [dbo].[GetPartyFamily] (@ids NVARCHAR(MAX) = NULL, @max_level INT = 9)
        RETURNS TABLE AS RETURN
        (
            SELECT ParentId, ChildId, RelationshipTypeId, -(Level + 1) AS Level, RootId
            FROM   [dbo].[GetPartyAncestors](@ids, @max_level)
            UNION ALL
            SELECT ParentId, ChildId, RelationshipTypeId, Level + 1 AS Level, RootId
            FROM   [dbo].[GetPartyOffspring](@ids, @max_level)
        );
        """;

    public static string[] CREATE_ALL => [CREATE_GetPartyOffspring, CREATE_GetPartyAncestors, CREATE_GetPartyFamily];
}