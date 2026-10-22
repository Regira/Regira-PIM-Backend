using Microsoft.EntityFrameworkCore;

namespace PIM.Services.Entities.Validation;

public static class DbContextValidationExtensions
{
    /// <summary>
    /// The items of a saved collection that stay: a <c>Related()</c> sync marks a removed child <c>Deleted</c>, and
    /// EF's navigation fixup can put such a child back into the collection a validator reads.
    /// </summary>
    public static IEnumerable<T> Kept<T>(this DbContext dbContext, IEnumerable<T> items)
        where T : class
        => items.Where(x => dbContext.Entry(x).State != EntityState.Deleted);
}
