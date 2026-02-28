using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Auth.Domain.Entities;

public class RefreshToken
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public string Token { get; set; } = default!;
    public string UserId { get; private set; } = default!;
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; } = false;
    public DateTime CreatedAt { get; private set; } = DateTime.Now;
    
    private  RefreshToken()
    {
    }

    public static RefreshToken Create(string userId, int expirationDays = 7)
    {
        return new RefreshToken
        {
            Token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"),
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays)
        };
    }

    public bool IsExpired =>  DateTime.UtcNow >= ExpiresAt;
    public bool IsValid => !IsRevoked &&  !IsExpired;

    public void Revoke()
    {
        IsRevoked = true;
    }
}