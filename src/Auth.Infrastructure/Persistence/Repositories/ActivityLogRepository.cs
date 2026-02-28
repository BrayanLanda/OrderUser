using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using MongoDB.Driver;

namespace Auth.Infrastructure.Persistence.Repositories;

public class ActivityLogRepository(MongoDbContext context) : IActivityLogRepository
{
    public async Task CreateAsync(ActivityLog log, CancellationToken ct = default) =>
        await context.ActivityLogs.InsertOneAsync(log, cancellationToken: ct);

    public async Task<List<ActivityLog>> GetByUserIdAsync(string userId, int limit = 50, CancellationToken ct = default) =>
        await context.ActivityLogs
            .Find(l => l.UserId == userId)
            .SortByDescending(l => l.CreatedAt)
            .Limit(limit)
            .ToListAsync(ct);
}