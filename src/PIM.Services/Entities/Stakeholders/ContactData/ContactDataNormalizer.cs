using PIM.Models.Stakeholders.ContactData;
using Regira.Entities.EFcore.Normalizing.Abstractions;
using Regira.Globalization.LibPhoneNumber;
using Regira.Normalizing.Abstractions;

namespace PIM.Services.Entities.Stakeholders.ContactData;

public class ContactDataNormalizer(INormalizer defaultNormalizer, PhoneNumberFormatter phoneNumberNormalizer)
    : EntityNormalizerBase<IContactDetails>(defaultNormalizer)
{
    public override Task HandleNormalizeMany(IEnumerable<IContactDetails> items)
    {
        foreach (var item in items)
        {
            HandleNormalize(item);
        }
        return Task.CompletedTask;
    }
    public override Task HandleNormalize(IContactDetails item)
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
    public string? Normalize(IContactDetails data)
    {
        data.NormalizedValue = Normalize(data.Value, data.DataType);
        return data.NormalizedValue;
    }
}