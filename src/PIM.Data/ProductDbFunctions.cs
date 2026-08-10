namespace PIM.Data;

/// <summary>
/// Recursive walks over the ProductComponent graph.
/// </summary>
/// <remarks>
/// <para>
/// Each walk comes in two variants: <c>...ByIds</c>, which starts from a JSON array of product ids, and
/// <c>...All</c>, which starts from every edge. They are separate functions on purpose.
/// </para>
/// <para>
/// A single function taking a nullable <c>@ids</c> has to serve both modes from one cached plan, and the
/// <c>(@ids IS NULL OR @ids = '' OR AssemblyId IN (...))</c> predicate that expresses it is not sargable —
/// SQL Server cannot know at compile time which branch applies, so it scans ProductComponent at every level
/// of the recursion. Measured on 1M products / 7.2M components, that cost 11 s and 16,238 logical reads for
/// a single product's tree; the split version answers the same query in 34 ms and 91 reads.
/// </para>
/// </remarks>
public class ProductDbFunctions
{
    /// <summary>
    /// Recursive member and projection, shared by both variants of a walk.
    /// </summary>
    /// <param name="cte">CTE name.</param>
    /// <param name="joinOn">How a further level attaches to the rows found so far.</param>
    private static string Recursion(string cte, string joinOn) => $"""
            UNION ALL
            SELECT        sc.AssemblyId, sc.ComponentId, {cte}.Level + 1, {cte}.RootId
            FROM          ProductComponent sc
            INNER JOIN    {cte} ON {joinOn}
            WHERE         (@max_level IS NULL OR {cte}.Level < @max_level)
                          AND NOT EXISTS (SELECT 1 FROM Products p WHERE p.Id = sc.AssemblyId  AND p.IsArchived = 1)
                          AND NOT EXISTS (SELECT 1 FROM Products p WHERE p.Id = sc.ComponentId AND p.IsArchived = 1)
        )
        SELECT * FROM {cte}
        """;

    // The archived check is two seekable NOT EXISTS rather than one with an OR over both columns:
    // same meaning (neither end archived), but each can seek Products by its primary key.
    private const string NotArchived = """
                          NOT EXISTS (SELECT 1 FROM Products p WHERE p.Id = r.AssemblyId  AND p.IsArchived = 1)
                          AND NOT EXISTS (SELECT 1 FROM Products p WHERE p.Id = r.ComponentId AND p.IsArchived = 1)
        """;

    private static string OffspringSql(bool byIds) => $"""
        WITH offspring (ParentId, ChildId, Level, RootId) AS (
            SELECT        r.AssemblyId, r.ComponentId, 0, r.AssemblyId
            {(byIds
                ? "FROM          OPENJSON(COALESCE(NULLIF(@ids, N''), N'[]')) j\n            INNER JOIN    ProductComponent r ON r.AssemblyId = CAST(j.value AS INT)\n            WHERE"
                : "FROM          ProductComponent r\n            WHERE")}
            {NotArchived}
        {Recursion("offspring", "offspring.ChildId = sc.AssemblyId")}
        """;

    private static string AncestorsSql(bool byIds) => $"""
        WITH ancestors (ParentId, ChildId, Level, RootId) AS (
            SELECT        r.AssemblyId, r.ComponentId, 0, r.ComponentId
            {(byIds
                ? "FROM          OPENJSON(COALESCE(NULLIF(@ids, N''), N'[]')) j\n            INNER JOIN    ProductComponent r ON r.ComponentId = CAST(j.value AS INT)\n            WHERE"
                : "FROM          ProductComponent r\n            WHERE")}
            {NotArchived}
        {Recursion("ancestors", "ancestors.ParentId = sc.ComponentId")}
        """;

    public static readonly string CREATE_GetProductOffspringByIds = $"""
        CREATE OR ALTER FUNCTION [dbo].[GetProductOffspringByIds] (@ids NVARCHAR(MAX), @max_level INT = 9)
        RETURNS TABLE AS RETURN
            {OffspringSql(byIds: true)};
        """;

    public static readonly string CREATE_GetProductOffspringAll = $"""
        CREATE OR ALTER FUNCTION [dbo].[GetProductOffspringAll] (@max_level INT = 9)
        RETURNS TABLE AS RETURN
            {OffspringSql(byIds: false)};
        """;

    public static readonly string CREATE_GetProductAncestorsByIds = $"""
        CREATE OR ALTER FUNCTION [dbo].[GetProductAncestorsByIds] (@ids NVARCHAR(MAX), @max_level INT = 9)
        RETURNS TABLE AS RETURN
            {AncestorsSql(byIds: true)};
        """;

    public static readonly string CREATE_GetProductAncestorsAll = $"""
        CREATE OR ALTER FUNCTION [dbo].[GetProductAncestorsAll] (@max_level INT = 9)
        RETURNS TABLE AS RETURN
            {AncestorsSql(byIds: false)};
        """;

    public static readonly string CREATE_GetProductFamilyByIds = """
        CREATE OR ALTER FUNCTION [dbo].[GetProductFamilyByIds] (@ids NVARCHAR(MAX), @max_level INT = 9)
        RETURNS TABLE AS RETURN
        (
            SELECT ParentId, ChildId, -(Level + 1) AS Level, RootId
            FROM   [dbo].[GetProductAncestorsByIds](@ids, @max_level)
            UNION ALL
            SELECT ParentId, ChildId, Level + 1 AS Level, RootId
            FROM   [dbo].[GetProductOffspringByIds](@ids, @max_level)
        );
        """;

    public static readonly string CREATE_GetProductFamilyAll = """
        CREATE OR ALTER FUNCTION [dbo].[GetProductFamilyAll] (@max_level INT = 9)
        RETURNS TABLE AS RETURN
        (
            SELECT ParentId, ChildId, -(Level + 1) AS Level, RootId
            FROM   [dbo].[GetProductAncestorsAll](@max_level)
            UNION ALL
            SELECT ParentId, ChildId, Level + 1 AS Level, RootId
            FROM   [dbo].[GetProductOffspringAll](@max_level)
        );
        """;

    /// <summary>Removes the superseded single-function forms, so a database upgraded in place is not left with both.</summary>
    public static readonly string DROP_Superseded = """
        DROP FUNCTION IF EXISTS [dbo].[GetProductFamily];
        DROP FUNCTION IF EXISTS [dbo].[GetProductOffspring];
        DROP FUNCTION IF EXISTS [dbo].[GetProductAncestors];
        """;

    // Family depends on the other two, so it is created last.
    public static string[] CREATE_ALL =>
    [
        CREATE_GetProductOffspringByIds, CREATE_GetProductOffspringAll,
        CREATE_GetProductAncestorsByIds, CREATE_GetProductAncestorsAll,
        CREATE_GetProductFamilyByIds, CREATE_GetProductFamilyAll,
        DROP_Superseded
    ];
}
