using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Safexchange.Hubs;

/// <summary>
/// Custom UserIdProvider that uses the UserId claim instead of Name claim
/// </summary>
public class NameUserIdProvider : IUserIdProvider
{
    public string GetUserId(HubConnectionContext connection)
    {
        var userId = connection.User?.FindFirst("UserId")?.Value
            ?? connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        return userId ?? "anonymous";
    }
}
