namespace PIM.Core.Abstractions;

public interface IOrderContext
{
    int? CustomerId { get; }
    DateTime? OrderDate { get; }
}
