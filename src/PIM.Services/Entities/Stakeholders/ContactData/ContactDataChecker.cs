using System.ComponentModel.DataAnnotations;
using PIM.Core.Constants;
using PIM.Models.Stakeholders.ContactData;
using Regira.Globalization.LibPhoneNumber;

namespace PIM.Services.Entities.Stakeholders.ContactData;

/// <summary>
/// Contact details that cannot be stored as typed: a phone number <see cref="ContactDataNormalizer"/> cannot parse —
/// it would be stored without a normalized value, so no search finds it — or an email address that is not one.
/// </summary>
public class ContactDataChecker(PhoneNumberFormatter phoneNumberFormatter)
{
    static readonly EmailAddressAttribute EmailAddress = new();

    /// <returns>A message key and the refused value for each detail that cannot be stored as typed.</returns>
    public IEnumerable<(string Message, string Value)> Check(IEnumerable<IContactDetails>? items)
    {
        foreach (var item in items ?? [])
        {
            if (string.IsNullOrWhiteSpace(item.Value))
            {
                continue;
            }
            if (item.DataType == ContactDataTypes.Phone && !IsPhoneNumber(item.Value))
            {
                yield return (ValidationMessages.InvalidPhone, item.Value);
            }
            else if (item.DataType == ContactDataTypes.Email && !EmailAddress.IsValid(item.Value))
            {
                yield return (ValidationMessages.InvalidEmail, item.Value);
            }
        }
    }

    // the same parse the normalizer runs: what it cannot parse, it stores unsearchable
    bool IsPhoneNumber(string value)
    {
        try
        {
            return phoneNumberFormatter.Normalize(value) != null;
        }
        catch
        {
            return false;
        }
    }
}
