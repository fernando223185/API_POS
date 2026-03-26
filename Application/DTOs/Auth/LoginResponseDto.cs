namespace Application.DTOs.Auth
{
    public class LoginResponseDto
    {
        public string Message { get; set; }
        public int Error { get; set; }
        public string Token { get; set; }
        public string TokenType { get; set; }
        public DateTime ExpiresAt { get; set; }
        public UserInfoDto User { get; set; }
    }

    public class UserInfoDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool Active { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public int? BranchId { get; set; }
        public int? CompanyId { get; set; }
    }
}