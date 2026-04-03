using System.Globalization;

namespace PIM.Core.Abstractions;

public interface ICultureContext
{
    CultureInfo Culture { get; }
    string? LangCode { get; }
    string? CountryCode { get; }

    Task Init(string? culture);
}
