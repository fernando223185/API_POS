namespace Application.Abstractions.Security
{
    public interface IJwtTokenService
    {
        string GenerateToken(int userId, string userCode, string userName, string userEmail, int roleId, string roleName);
        Task<int?> GetUserIdFromTokenAsync(string token);
        bool ValidateToken(string token);
    }
}