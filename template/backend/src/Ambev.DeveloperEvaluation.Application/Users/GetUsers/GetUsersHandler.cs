using AutoMapper;
using MediatR;
using Ambev.DeveloperEvaluation.Application.Users.GetUser;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Users.GetUsers;

public class GetUsersHandler : IRequestHandler<GetUsersQuery, GetUsersResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetUsersHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<GetUsersResult> Handle(GetUsersQuery query, CancellationToken cancellationToken)
    {
        var (users, total) = await _userRepository.GetPagedAsync(query.Page, query.Size, query.Order, cancellationToken);
        return new GetUsersResult
        {
            Data = _mapper.Map<IEnumerable<GetUserResult>>(users),
            TotalItems = total,
            CurrentPage = query.Page,
            TotalPages = (int)Math.Ceiling((double)total / query.Size)
        };
    }
}
