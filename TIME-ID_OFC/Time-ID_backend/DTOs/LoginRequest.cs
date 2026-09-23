using System.ComponentModel.DataAnnotations;

public class LoginRequest
{
    private string email = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email
    {
        get => email;
        set => email = value?.Trim() ?? string.Empty;
    }

    [Required, StringLength(256)]
    public string Password { get; set; } = string.Empty;
}
