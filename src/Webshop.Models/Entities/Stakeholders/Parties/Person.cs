using Regira.Normalizing;
using System.ComponentModel.DataAnnotations;

namespace Webshop.Models.Entities.Stakeholders.Parties;

public class Person() : Party("PERSON")
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