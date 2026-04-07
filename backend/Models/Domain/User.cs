using MongoDB.Bson.Serialization.Attributes;
using RocketLog.Api.Models.Common;

namespace RocketLog.Api.Models.Domain;

[BsonIgnoreExtraElements]
[BsonCollection("users")]
public sealed class User : BaseMongoEntity
{
    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [BsonElement("passwordHash")]
    public string? PasswordHash { get; set; }

    [BsonElement("oauthProvider")]
    public string? OAuthProvider { get; set; }

    [BsonElement("oauthSubject")]
    public string? OAuthSubject { get; set; }

    [BsonElement("roles")]
    public List<string> Roles { get; set; } = [];

    [BsonElement("preferences")]
    public UserPreferences Preferences { get; set; } = new();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class UserPreferences
{
    [BsonElement("units")]
    public string Units { get; set; } = "imperial";

    [BsonElement("defaultLaunchSite")]
    public string? DefaultLaunchSite { get; set; }
}