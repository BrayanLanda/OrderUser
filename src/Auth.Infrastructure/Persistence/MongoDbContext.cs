using Auth.Domain.Entities;
using MongoDB.Driver;

namespace Auth.Infrastructure.Persistence;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        _database = client.GetDatabase(settings.DatabaseName);
    }

    public IMongoCollection<User> Users =>
        _database.GetCollection<User>("users");

    public IMongoCollection<RefreshToken> RefreshTokens =>
        _database.GetCollection<RefreshToken>("refreshTokens");

    public IMongoCollection<ActivityLog> ActivityLogs =>
        _database.GetCollection<ActivityLog>("activityLogs");
}