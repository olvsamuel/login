namespace login.DTOs;

public class UserDTO {
    public UserDTO(string email, string password) {
        Email = email;
        Password = password;
    }
    public string Email { get; set; }
    public string Password { get; set; }
}