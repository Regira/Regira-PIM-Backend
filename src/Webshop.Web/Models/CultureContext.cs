using System.Globalization;
using Webshop.Core.Abstractions;

namespace Webshop.Web.Models;

public class CultureContext : ICultureContext
{
    public CultureInfo Culture { get; set; } = CultureInfo.CurrentCulture;
    public string LangCode { get; set; } = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
    public string? CountryCode { get; set; } = CultureInfo.CurrentCulture.Name.Split('-').LastOrDefault();
}