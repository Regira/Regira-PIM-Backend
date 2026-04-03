namespace PIM.Core.Abstractions;

public interface IUserContext
{
    public string? UserId { get; }
    public ICollection<string> Permissions { get; }
}