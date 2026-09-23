using System.ComponentModel.DataAnnotations;

public class LoginRequest
{
    private string email = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email
    {
        get
        {
            return email;
        }
        set
        {
            if (value == null)
            {
                email = string.Empty;
            }
            else
            {
                email = value.Trim();
            }
        }
    }

    [Required, StringLength(256)]
    public string Password { get; set; } = string.Empty;
}
