using Regira.Entities.EFcore.Normalizing.Abstractions;
using Regira.Globalization.LibPhoneNumber;
using Regira.Normalizing.Abstractions;
using Webshop.Models.Entities.Clients.ContactData;

namespace Webshop.Services.Entities.Clients.ContactData;

public class ContactDataNormalizer(INormalizer defaultNormalizer, PhoneNumberFormatter phoneNumberNormalizer)
    : EntityNormalizerBase<CustomerContactData>(defaultNormalizer)
{
    public override Task HandleNormalizeMany(IEnumerable<CustomerContactData> items)
    {
        foreach (var item in items)
        {
            HandleNormalize(item);
        }
        return Task.CompletedTask;
    }
    public override Task HandleNormalize(CustomerContactData item)
    {
        item.NormalizedValue = Normalize(item);
        return Task.CompletedTask;
    }

    public string? Normalize(string? input, ContactDataTypes dataType)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        switch (dataType)
        {
            case ContactDataTypes.Phone:
                try
                {
                    return phoneNumberNormalizer.Normalize(input);
                }
                catch
                {
                    return null;
                }
            case ContactDataTypes.Email:
                return input.ToUpper();
            case ContactDataTypes.Website:
                return input.ToUpper();
            default: //case ContactDataTypes.Other
                return DefaultPropertyNormalizer.Normalize(input);
        }
    }
    public string? Normalize(CustomerContactData data)
    {
        data.NormalizedValue = Normalize(data.Value, data.DataType);
        return data.NormalizedValue;
    }
}