namespace PIM.Data;

public class ProductDbFunctions
{
    private const string ProductOffspringSQL = """
        WITH offspring (ParentId, ChildId, Level, RootId) AS (
            SELECT        r.AssemblyId, r.ComponentId, 0, r.AssemblyId
            FROM          ProductComponent r
            WHERE         (@ids IS NULL OR @ids = '' OR r.AssemblyId IN (SELECT CAST(value AS INT) FROM OPENJSON(@ids)))
                          AND NOT EXISTS (SELECT 1 FROM Products s WHERE (s.Id = r.ComponentId OR s.Id = r.AssemblyId) AND s.IsArchived = 1)
            UNION ALL
            SELECT        sc.AssemblyId, sc.ComponentId, offspring.Level + 1, offspring.RootId
            FROM          ProductComponent sc
            INNER JOIN    offspring ON offspring.ChildId = sc.AssemblyId
            WHERE         (@max_level IS NULL OR offspring.Level < @max_level)
                          AND NOT EXISTS (SELECT 1 FROM Products s WHERE (s.Id = sc.ComponentId OR s.Id = sc.AssemblyId) AND s.IsArchived = 1)
        )
        SELECT * FROM offspring
        """;

    private const string ProductAncestorsSQL = """
        WITH ancestors (ParentId, ChildId, Level, RootId) AS (
            SELECT        r.AssemblyId, r.ComponentId, 0, r.ComponentId
            FROM          ProductComponent r
            WHERE         (@ids IS NULL OR @ids = '' OR r.ComponentId IN (SELECT CAST(value AS INT) FROM OPENJSON(@ids)))
                          AND NOT EXISTS (SELECT 1 FROM Products s WHERE (s.Id = r.ComponentId OR s.Id = r.AssemblyId) AND s.IsArchived = 1)
            UNION ALL
            SELECT        sc.AssemblyId, sc.ComponentId, ancestors.Level + 1, ancestors.RootId
            FROM          ProductComponent sc
            INNER JOIN    ancestors ON ancestors.ParentId = sc.ComponentId
            WHERE         (@max_level IS NULL OR ancestors.Level < @max_level)
                          AND NOT EXISTS (SELECT 1 FROM Products s WHERE (s.Id = sc.ComponentId OR s.Id = sc.AssemblyId) AND s.IsArchived = 1)
        )
        SELECT * FROM ancestors
        """;

    public static readonly string CREATE_GetProductOffspring = $@"
CREATE OR ALTER FUNCTION [dbo].[GetProductOffspring] (@ids NVARCHAR(MAX) = NULL, @max_level INT = 9)
RETURNS TABLE AS RETURN
    {ProductOffspringSQL};";

    public static readonly string CREATE_GetProductAncestors = $@"
CREATE OR ALTER FUNCTION [dbo].[GetProductAncestors] (@ids NVARCHAR(MAX) = NULL, @max_level INT = 9)
RETURNS TABLE AS RETURN
    {ProductAncestorsSQL};";

    public static readonly string CREATE_GetProductFamily = """
        CREATE OR ALTER FUNCTION [dbo].[GetProductFamily] (@ids NVARCHAR(MAX) = NULL, @max_level INT = 9)
        RETURNS TABLE AS RETURN
        (
            SELECT ParentId, ChildId, -(Level + 1) AS Level, RootId
            FROM   [dbo].[GetProductAncestors](@ids, @max_level)
            UNION ALL
            SELECT ParentId, ChildId, Level + 1 AS Level, RootId
            FROM   [dbo].[GetProductOffspring](@ids, @max_level)
        );
        """;

    public static string[] CREATE_ALL => [CREATE_GetProductOffspring, CREATE_GetProductAncestors, CREATE_GetProductFamily];
}
