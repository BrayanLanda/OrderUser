using Auth.Domain.Entities.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Auth.Domain.Entities;

public class ActivityLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public string UserId { get; private set; } = default!;
    public ActivityType Type { get; private set; }
    public string Description { get; private set; } = default!;
    public Dictionary<string, object> Metadata { get; private set; } = [];
    public DateTime CreatedAt { get; private set; } =  DateTime.UtcNow;
    
    public ActivityLog() {}
    
    public static ActivityLog Create(string userId, ActivityType type, string description,
        Dictionary<string, object>? metadata = null)
    {
        return new ActivityLog
        {
            UserId = userId,
            Type = type,
            Description = description,
            Metadata = metadata ?? [],
        };
    }
}
