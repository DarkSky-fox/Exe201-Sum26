namespace Safexchange.Services;

public interface ICurrentUserService
{
    int GetUserId();
    void SetUserId(int userId);
    void SignIn(int userId, string email, string role);
    void SignOut();
    bool IsAuthenticated();
    string? GetUserEmail();
}
