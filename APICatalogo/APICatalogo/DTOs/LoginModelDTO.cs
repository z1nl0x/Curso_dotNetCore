using System.ComponentModel.DataAnnotations;

namespace APICatalogo.DTOs;

public class LoginModelDTO
{
    [Required(ErrorMessage ="Usuário é requerido.")]
    public string? Username { get; set; }
    
    [Required(ErrorMessage = "Password é requerido.")]
    public string? Password { get; set; }
}