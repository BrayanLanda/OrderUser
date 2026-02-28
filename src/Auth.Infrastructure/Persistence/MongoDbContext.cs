using Auth.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Auth.Infrastructure.Persistence;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    static MongoDbContext()
    {
        BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

        var conventions = new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new IgnoreExtraElementsConvention(true),
        };
        ConventionRegistry.Register("GlobalConventions", conventions, _ => true);
    }

    public MongoDbContext(MongoDbSettings settings)
    {
        var clientSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);
        var client = new MongoClient(clientSettings);
        _database = client.GetDatabase(settings.DatabaseName);
    }

    public IMongoCollection<User> Users =>
        _database.GetCollection<User>("users");

    public IMongoCollection<RefreshToken> RefreshTokens =>
        _database.GetCollection<RefreshToken>("refreshTokens");

    public IMongoCollection<ActivityLog> ActivityLogs =>
        _database.GetCollection<ActivityLog>("activityLogs");
}