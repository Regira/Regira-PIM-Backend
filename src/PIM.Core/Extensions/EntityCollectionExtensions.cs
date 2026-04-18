using Regira.Entities.Extensions;
using Regira.Entities.Models.Abstractions;
using Regira.Utilities;

namespace PIM.Core.Extensions;

public static class EntityCollectionExtensions
{
    public static ICollection<T> Prepare<T>(this ICollection<T> items)
    {
        var entityType = typeof(T);
        if (TypeUtility.ImplementsInterface<IEntity<int>>(entityType))
        {
            items.Cast<IEntity<int>>().AdjustIdForEfCore();
        }

        if (items is ICollection<ISortable> sortableItems)
        {
            sortableItems.SetSortOrder();
        }
        if (TypeUtility.ImplementsInterface<ISortable>(entityType))
        {
            items.Cast<ISortable>().SetSortOrder();
        }

        return items;
    }
}