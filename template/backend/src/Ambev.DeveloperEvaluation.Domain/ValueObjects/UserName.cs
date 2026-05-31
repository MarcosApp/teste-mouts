namespace Ambev.DeveloperEvaluation.Domain.ValueObjects;

public class UserName
{
    public string Firstname { get; private set; } = string.Empty;
    public string Lastname { get; private set; } = string.Empty;

    protected UserName() { }

    public UserName(string firstname, string lastname)
    {
        Firstname = firstname;
        Lastname = lastname;
    }
}
