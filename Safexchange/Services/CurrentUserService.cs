using System.Security.Claims;

namespace Safexchange.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    private ISession Session => _httpContextAccessor.HttpContext!.Session;

    private HttpContext HttpContext => _httpContextAccessor.HttpContext!;

    public int GetUserId()
    {
        var claimId = HttpContext.User?.FindFirst("UserId")?.Value
            ?? HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (int.TryParse(claimId, out var fromClaim))
        {
            Session.SetInt32(SessionKeys.CurrentUserId, fromClaim);
            return fromClaim;
        }

        var stored = Session.GetInt32(SessionKeys.CurrentUserId);
        if (stored.HasValue)
        {
            return stored.Value;
        }

        var demoId = _configuration.GetValue<int>("AppSettings:DemoBuyerId", 1);
        Session.SetInt32(SessionKeys.CurrentUserId, demoId);
        return demoId;
    }

    public void SetUserId(int userId)
    {
        Session.SetInt32(SessionKeys.CurrentUserId, userId);
    }
}
