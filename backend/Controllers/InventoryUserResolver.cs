using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MongoDB.Bson;

namespace RocketLog.Api.Controllers;

internal static class InventoryUserResolver
{
    public static bool TryResolveUserId(ClaimsPrincipal user, out ObjectId userId)
    {
        var subject = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(subject))
        {
            userId = default;
            return false;
        }

        if (ObjectId.TryParse(subject, out userId))
        {
            return true;
        }

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(subject));
        userId = new ObjectId(hash.Take(12).ToArray());
        return true;
    }
}