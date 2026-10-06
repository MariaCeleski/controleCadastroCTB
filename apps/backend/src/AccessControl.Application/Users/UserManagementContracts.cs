using AccessControl.Domain.Entities;

namespace AccessControl.Application.Users;

public sealed record CreateUserRequest(string Name, string Email, string Password, UserRole Role);
public sealed record UserSummary(Guid Id, string Name, string Email, UserRole Role, bool IsActive);

public interface IUserManagementService
{
    Task<IReadOnlyCollection<UserSummary>> ListAsync(Guid organizationId, CancellationToken cancellationToken);
    Task<UserSummary> CreateAsync(Guid organizationId, Guid actorUserId, CreateUserRequest request, CancellationToken cancellationToken);
    Task<bool> SetActiveAsync(Guid organizationId, Guid actorUserId, Guid userId, bool isActive, CancellationToken cancellationToken);
}
