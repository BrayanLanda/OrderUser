using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using MongoDB.Driver;

namespace Auth.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository(MongoDbContext context) : IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default) =>
        await context.RefreshTokens.Find(t => t.Token == token).FirstOrDefaultAsync(ct);

    public async Task CreateAsync(RefreshToken refreshToken, CancellationToken ct = default) =>
        await context.RefreshTokens.InsertOneAsync(refreshToken, cancellationToken: ct);

    public async Task UpdateAsync(RefreshToken refreshToken, CancellationToken ct = default) =>
        await context.RefreshTokens.ReplaceOneAsync(t => t.Id == refreshToken.Id, refreshToken, cancellationToken: ct);

    public async Task RevokeAllForUserAsync(string userId, CancellationToken ct = default)
    {
        var update = Builders<RefreshToken>.Update.Set(t => t.IsRevoked, true);
        await context.RefreshTokens.UpdateManyAsync(t => t.UserId == userId && !t.IsRevoked, update, cancellationToken: ct);
    }
}