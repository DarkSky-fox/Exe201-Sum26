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

    public int GetUserId()
    {
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
