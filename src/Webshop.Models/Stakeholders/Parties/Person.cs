using Regira.Normalizing;
using System.ComponentModel.DataAnnotations;
using Webshop.Core.Constants;

namespace Webshop.Models.Stakeholders.Parties;

public class Person() : Party(PartyTypes.Person)
{
    [MaxLength(32)]
    public string? Salutation { get; set; }
    [MaxLength(128)]
    public string? GivenName { get; set; }
    [MaxLength(128)]
    public string? MiddleName { get; set; }
    [MaxLength(128)]
    public string? FamilyName { get; set; }
    [Normalized(SourceProperties = [nameof(FamilyName), nameof(GivenName)])]
    public override string? NormalizedTitle { get; set; }
    
    public override string Title => $"{GivenName} {FamilyName}".Trim();
}