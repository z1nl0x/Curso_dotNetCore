using System.ComponentModel.DataAnnotations;

namespace APICatalogo.DTOs;

public class RegisterModelDTO
{
    [Required(ErrorMessage = "Username é requerido.")]
    public string? UserName { get; set; }
    
    [EmailAddress]
    [Required(ErrorMessage = "Email é requerido.")]
    public string? Email { get; set; }
    
    [Required(ErrorMessage = "Password é requerido.")]
    public string? Password { get; set; }
}