using Auth.Domain.Entities;

namespace Auth.Domain.Repositories;

public interface IActivityLogRepository
{
    Task CreateAsync(ActivityLog log, CancellationToken ct = default);
    Task<List<ActivityLog>> GetByUserIdAsync(string userId, int limit = 50, CancellationToken ct = default);
}