namespace PIM.Models.Stakeholders.Parties.Abstractions;

public interface IPartyCore
{
    int Id { get; set; }
    string? Code { get; set; }

    string? Title { get; set; }
    string? Description { get; set; }

    DateTime? StartDate { get; set; }
    DateTime? EndDate { get; set; }

    DateTime Created { get; set; }
    DateTime? LastModified { get; set; }
}