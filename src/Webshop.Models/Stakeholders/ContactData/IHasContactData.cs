namespace Webshop.Models.Stakeholders.ContactData;

public interface IHasContactData
{
    ICollection<IContactDetails>? ContactData { get; set; }
}

public interface IHasContactData<T>
    where T : class, IContactDetails, new()
{
    ICollection<T>? ContactData { get; set; }
}