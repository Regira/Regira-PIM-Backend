using Regira.Entities.EFcore.Normalizing.Abstractions;
using Webshop.Models.Entities.Stakeholders.Parties;
using Webshop.Services.Entities.Stakeholders.Addresses;
using Webshop.Services.Entities.Stakeholders.ContactData;

namespace Webshop.Services.Entities.Stakeholders.Parties;

public class PartyNormalizer(ContactDataNormalizer contactDataNormalizer, AddressNormalizer addressNormalizer) : EntityNormalizerBase<Party>
{
    public override async Task HandleNormalize(Party item)
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