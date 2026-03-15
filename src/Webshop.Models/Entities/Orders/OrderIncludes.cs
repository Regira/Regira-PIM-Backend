namespace Webshop.Models.Entities.Orders;

[Flags]
public enum OrderIncludes
{
    Default = 0,
    Customer = 1 << 0,
    OrderLines = 1 << 1,
    OrderLinesArticles = 1 << 2,
    OrderLinesArticleComponents = 1 << 3,
    OrderLinesArticlesWithComponents = OrderLinesArticles | OrderLinesArticleComponents,
    All = Customer | OrderLinesArticlesWithComponents
}
