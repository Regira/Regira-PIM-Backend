using System.Globalization;
using PIM.Core.Abstractions;

namespace PIM.Web.Models;

public class CultureContext : ICultureContext
{
    public static CultureInfo DefaultCultureInfo = CultureInfo.CurrentCulture;

    public CultureInfo Culture { get; set; } = DefaultCultureInfo;
    public string LangCode { get; set; } = DefaultCultureInfo.TwoLetterISOLanguageName;
    public string? CountryCode { get; set; } = DefaultCultureInfo.Name.Split('-').LastOrDefault();

    public Task Init(string? culture)
    {
        if (!string.IsNullOrWhiteSpace(culture))
        {
            try
            {
                Culture = new CultureInfo(culture);
                LangCode = Culture.TwoLetterISOLanguageName;
                CountryCode = Culture.Name.Split('-').LastOrDefault();
            }
            catch (CultureNotFoundException)
            {
                Culture = DefaultCultureInfo;
                LangCode = Culture.TwoLetterISOLanguageName;
                CountryCode = Culture.Name.Split('-').LastOrDefault();
            }
        }
        return Task.CompletedTask;
    }
}