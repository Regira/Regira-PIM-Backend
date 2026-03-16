namespace Webshop.Models.Contexts;

public interface IOrderContext
{
    int? CustomerId { get; }
    int? OrganizationId { get; }
    DateTime? OrderDate { get; }
}
