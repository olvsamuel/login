namespace login.Utils.Helpers;

using System.Text.RegularExpressions;

public class Helpers
{
    public static bool ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        // the password must contain at least 1 uppercase letter, 1 lowercase letter, 1 digit, 1 special character and 
        // the length must be at least 6 characters long
        Regex regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\p{P}\p{S}]).{6,}$");
        return regex.IsMatch(password);
    }

    public static bool ValidateEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return false;
        }

        string pattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
        Regex regex = new Regex(pattern);

        return regex.IsMatch(email);
    }

}