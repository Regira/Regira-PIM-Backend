using Regira.Entities.EFcore.Normalizing.Abstractions;
using Webshop.Models.Entities.Clients.Customers;
using Webshop.Services.Entities.Clients.Addresses;
using Webshop.Services.Entities.Clients.ContactData;

namespace Webshop.Services.Entities.Clients.Customers;

public class CustomerNormalizer(ContactDataNormalizer contactDataNormalizer, AddressNormalizer addressNormalizer) : EntityNormalizerBase<Customer>
{
    public override async Task HandleNormalize(Customer item)
    {
        await base.HandleNormalize(item);

        var contentEntries = new List<string?> { item.NormalizedTitle };

        // ContactData
        if (item.ContactData?.Any() == true)
        {
            await contactDataNormalizer.HandleNormalizeMany(item.ContactData);
        }
        if (item.ContactData?.Any() == true)
        {
            contentEntries.AddRange(item.ContactData.Select(a => a.NormalizedValue));
        }

        // Address
        if (item.Addresses?.Any() == true)
        {
            foreach (var address in item.Addresses)
            {
                addressNormalizer.Normalize(address);
            }
            contentEntries.AddRange(item.Addresses.Select(a => a.NormalizedContent));
        }

        item.NormalizedContent = string.Join(' ', contentEntries.Where(x => !string.IsNullOrWhiteSpace(x)));

        await Task.CompletedTask;
    }
}