using Regira.Entities.Models.Abstractions;
using Webshop.Models.Stakeholders.Parties;

namespace Webshop.Models.Catalog.Articles;

public class ArticleSupplier : IEntityWithSerial
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public int SupplierId { get; set; }

    public Article? Article { get; set; }
    public Party? Supplier { get; set; }
}