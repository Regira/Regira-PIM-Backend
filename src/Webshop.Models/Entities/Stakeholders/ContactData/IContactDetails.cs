using Regira.Entities.Models.Abstractions;

namespace Webshop.Models.Entities.Stakeholders.ContactData;

public interface IContactDetails : IEntity<int>, IHasTitle, ISortable, IHasDescription, IHasTimestamps
{
    string Value { get; set; }
    string? NormalizedValue { get; set; }
    ContactDataTypes DataType { get; set; }
}
