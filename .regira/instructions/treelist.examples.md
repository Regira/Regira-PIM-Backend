# TreeList — Example: Product Category Tree

> Context: A webshop displays categories in a hierarchical menu. Categories can have subcategories (one parent). The back-office shows a flat sorted list with depth indentation.

## Build the tree from a flat list

```csharp
using Regira.TreeList;

// Each Category has a nullable ParentId and a reference to its Parent
var tree = categories.ToTreeList(c => c.Parent!);
```

## Render a hierarchical menu

```csharp
var ordered = tree.OrderByHierarchy(n => n.Value.SortOrder);
foreach (var node in ordered)
{
    var indent = new string(' ', node.Level * 2);
    Console.WriteLine($"{indent}{node.Value.Title}");
}
```

## Find all subcategories of "Electronics"

```csharp
var electronicsNode = tree.First(n => n.Value.Slug == "electronics");
var allSubs         = electronicsNode.GetOffspring().Select(n => n.Value);
```

## Get breadcrumb for a category page

```csharp
public IEnumerable<Category> GetBreadcrumb(Category category)
{
    var node = tree.First(n => n.Value.Id == category.Id);
    return node.GetAncestors()        // root → direct parent
               .Append(node)
               .Select(n => n.Value);
}
```

## Build tree top-down (best performance when children are navigable)

```csharp
var rootCategories = categories.Where(c => c.ParentId == null);

var tree = categories.ToTreeList(
    rootCategories,
    node => categories.Where(c => c.ParentId == node.Value.Id));
```

## Get a flat, depth-first view of raw values

```csharp
TreeView<Category> view = tree.ToTreeView();
// view[0] is the first root category; view.Tree is the full TreeList
```
