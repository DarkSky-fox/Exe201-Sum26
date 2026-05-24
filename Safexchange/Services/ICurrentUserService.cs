namespace Safexchange.Services;

public interface ICurrentUserService
{
    int GetUserId();
    void SetUserId(int userId);
    Task<bool> IsUserVerifiedAsync();
}
