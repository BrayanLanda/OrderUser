using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using MongoDB.Driver;

namespace Auth.Infrastructure.Persistence.Repositories;

public class UserRepository(MongoDbContext context) : IUserRepository
{
    public async Task<User?> GetByIdAsync(string id, CancellationToken ct = default) =>
        await context.Users.Find(u => u.Id == id).FirstOrDefaultAsync(ct);
    
    public async Task<User?> GetByExternalIdAsync(Guid externalId, CancellationToken ct = default) =>
        await context.Users.Find(u => u.ExternalId == externalId).FirstOrDefaultAsync(ct);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await context.Users.Find(u => u.Email == email.ToLowerInvariant()).FirstOrDefaultAsync(ct);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default) =>
        await context.Users.Find(u => u.Email == email.ToLowerInvariant()).AnyAsync(ct);

    public async Task CreateAsync(User user, CancellationToken ct = default) =>
        await context.Users.InsertOneAsync(user, cancellationToken: ct);

    public async Task UpdateAsync(User user, CancellationToken ct = default) =>
        await context.Users.ReplaceOneAsync(u => u.Id == user.Id, user, cancellationToken: ct);
}