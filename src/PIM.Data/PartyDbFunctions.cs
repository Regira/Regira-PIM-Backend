namespace PIM.Data;

/// <summary>
/// Recursive walks over the PartyRelationship graph.
/// </summary>
/// <remarks>
/// Split into <c>...ByIds</c> and <c>...All</c> variants for the same reason as
/// <see cref="ProductDbFunctions"/>: one function serving both modes cannot produce a seekable plan.
/// </remarks>
public class PartyDbFunctions
{
    private static string Recursion(string cte, string joinOn) => $"""
            UNION ALL
            SELECT        sc.ParentId, sc.ChildId, sc.RelationshipTypeId, {cte}.Level + 1, {cte}.RootId
            FROM          PartyRelationship sc
            INNER JOIN    {cte} ON {joinOn}
            WHERE         (@max_level IS NULL OR {cte}.Level < @max_level)
                          AND NOT EXISTS (SELECT 1 FROM Parties p WHERE p.Id = sc.ParentId AND p.IsArchived = 1)
                          AND NOT EXISTS (SELECT 1 FROM Parties p WHERE p.Id = sc.ChildId  AND p.IsArchived = 1)
        )
        SELECT * FROM {cte}
        """;

    private const string NotArchived = """
                          NOT EXISTS (SELECT 1 FROM Parties p WHERE p.Id = r.ParentId AND p.IsArchived = 1)
                          AND NOT EXISTS (SELECT 1 FROM Parties p WHERE p.Id = r.ChildId  AND p.IsArchived = 1)
        """;

    private static string OffspringSql(bool byIds) => $"""
        WITH offspring (ParentId, ChildId, RelationshipTypeId, Level, RootId) AS (
            SELECT        r.ParentId, r.ChildId, r.RelationshipTypeId, 0, r.ParentId
            {(byIds
                ? "FROM          OPENJSON(COALESCE(NULLIF(@ids, N''), N'[]')) j\n            INNER JOIN    PartyRelationship r ON r.ParentId = CAST(j.value AS INT)\n            WHERE"
                : "FROM          PartyRelationship r\n            WHERE")}
            {NotArchived}
        {Recursion("offspring", "offspring.ChildId = sc.ParentId")}
        """;

    private static string AncestorsSql(bool byIds) => $"""
        WITH ancestors (ParentId, ChildId, RelationshipTypeId, Level, RootId) AS (
            SELECT        r.ParentId, r.ChildId, r.RelationshipTypeId, 0, r.ChildId
            {(byIds
                ? "FROM          OPENJSON(COALESCE(NULLIF(@ids, N''), N'[]')) j\n            INNER JOIN    PartyRelationship r ON r.ChildId = CAST(j.value AS INT)\n            WHERE"
                : "FROM          PartyRelationship r\n            WHERE")}
            {NotArchived}
        {Recursion("ancestors", "ancestors.ParentId = sc.ChildId")}
        """;

    public static readonly string CREATE_GetPartyOffspringByIds = $"""
        CREATE OR ALTER FUNCTION [dbo].[GetPartyOffspringByIds] (@ids NVARCHAR(MAX), @max_level INT = 9)
        RETURNS TABLE AS RETURN
            {OffspringSql(byIds: true)};
        """;

    public static readonly string CREATE_GetPartyOffspringAll = $"""
        CREATE OR ALTER FUNCTION [dbo].[GetPartyOffspringAll] (@max_level INT = 9)
        RETURNS TABLE AS RETURN
            {OffspringSql(byIds: false)};
        """;

    public static readonly string CREATE_GetPartyAncestorsByIds = $"""
        CREATE OR ALTER FUNCTION [dbo].[GetPartyAncestorsByIds] (@ids NVARCHAR(MAX), @max_level INT = 9)
        RETURNS TABLE AS RETURN
            {AncestorsSql(byIds: true)};
        """;

    public static readonly string CREATE_GetPartyAncestorsAll = $"""
        CREATE OR ALTER FUNCTION [dbo].[GetPartyAncestorsAll] (@max_level INT = 9)
        RETURNS TABLE AS RETURN
            {AncestorsSql(byIds: false)};
        """;

    public static readonly string CREATE_GetPartyFamilyByIds = """
        CREATE OR ALTER FUNCTION [dbo].[GetPartyFamilyByIds] (@ids NVARCHAR(MAX), @max_level INT = 9)
        RETURNS TABLE AS RETURN
        (
            SELECT ParentId, ChildId, RelationshipTypeId, -(Level + 1) AS Level, RootId
            FROM   [dbo].[GetPartyAncestorsByIds](@ids, @max_level)
            UNION ALL
            SELECT ParentId, ChildId, RelationshipTypeId, Level + 1 AS Level, RootId
            FROM   [dbo].[GetPartyOffspringByIds](@ids, @max_level)
        );
        """;

    public static readonly string CREATE_GetPartyFamilyAll = """
        CREATE OR ALTER FUNCTION [dbo].[GetPartyFamilyAll] (@max_level INT = 9)
        RETURNS TABLE AS RETURN
        (
            SELECT ParentId, ChildId, RelationshipTypeId, -(Level + 1) AS Level, RootId
            FROM   [dbo].[GetPartyAncestorsAll](@max_level)
            UNION ALL
            SELECT ParentId, ChildId, RelationshipTypeId, Level + 1 AS Level, RootId
            FROM   [dbo].[GetPartyOffspringAll](@max_level)
        );
        """;

    public static readonly string DROP_Superseded = """
        DROP FUNCTION IF EXISTS [dbo].[GetPartyFamily];
        DROP FUNCTION IF EXISTS [dbo].[GetPartyOffspring];
        DROP FUNCTION IF EXISTS [dbo].[GetPartyAncestors];
        """;

    public static string[] CREATE_ALL =>
    [
        CREATE_GetPartyOffspringByIds, CREATE_GetPartyOffspringAll,
        CREATE_GetPartyAncestorsByIds, CREATE_GetPartyAncestorsAll,
        CREATE_GetPartyFamilyByIds, CREATE_GetPartyFamilyAll,
        DROP_Superseded
    ];
}
