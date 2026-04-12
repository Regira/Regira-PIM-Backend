using Regira.Entities.Web.Models;
using Regira.TreeList;

namespace PIM.Web.Extensions;

public static class TreeListExtensions
{
    public static ListResult<TreeNode<T>> ToTreeNodeListResult<T>(this TreeList<T> tree, long? duration = null)
    {
        return new ListResult<TreeNode<T>>
        {
            Items = tree,
            Duration = duration
        };
    }
}