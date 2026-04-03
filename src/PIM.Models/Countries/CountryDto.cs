namespace PIM.Models.Countries;

public class CountryDto
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Code { get; set; }
    public bool IsDefault { get; set; }
    public bool IsArchived { get; set; }
}
