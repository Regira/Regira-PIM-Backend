using System.Globalization;

namespace Webshop.Core.Abstractions;

public interface ICultureContext
{
    CultureInfo Culture { get; }
    string? LangCode { get; }
    string? CountryCode { get; }

    void Load(string? culture = null);
}
