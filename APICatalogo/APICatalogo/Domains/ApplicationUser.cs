using Microsoft.AspNetCore.Identity;

namespace APICatalogo.Domains;

public class ApplicationUser : IdentityUser
{
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
}