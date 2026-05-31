using Ambev.DeveloperEvaluation.Application.Users.CreateUser;

namespace Ambev.DeveloperEvaluation.Application.Users.GetUsers;

public class GetUsersResult
{
    public IEnumerable<CreateUserResult> Data { get; set; } = Enumerable.Empty<CreateUserResult>();
    public int TotalItems { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}
