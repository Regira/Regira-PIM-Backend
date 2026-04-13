using Regira.Entities.Web.Models;
using Regira.TreeList;

namespace PIM.Web.Extensions;

public static class TreeListExtensions
{
    public static ListResult<T> ToTreeViewListResult<T>(this TreeList<T> tree, long? duration = null)
    {
        return new ListResult<T>
        {
            Items = tree.ToTreeView(),
            Duration = duration
        };
    }
}