namespace login.Models;

using System.ComponentModel.DataAnnotations;
using Utils.Helpers;

public class UserModel
{
    public UserModel(long id, string email, string name, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("Email, name and password are required");
        }

        if (id <= 0)
        {
            throw new ArgumentException("Id must be greater than 0");
        }

        Id = id;
        _email = email;
        _name = name;
        _password = password;
    }
    
    [Key]
    public long Id { get; private set; }

    private string _email;
    public string Email
    {
        get => _email;
        private set
        {
            _email = Helpers.ValidateEmail(value) ? value : throw new ArgumentException(nameof(Email), "Invalid email");
        }
    }

    private string _name;
    public string Name
    {
        get => _name;
        private set
        {
            _name = (value.Length > 50) ?
            throw new ArgumentOutOfRangeException(nameof(Name),
                "Name cannot be longer than 50 characters") : value;
        }
    }

    private string _password;
    public string Password
    {
        get => _password;
        private set
        {
            _password = Helpers.ValidatePassword(value) ?
                value : throw new ArgumentException(nameof(Password), "Invalid password");
        }
    }
}