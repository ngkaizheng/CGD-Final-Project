using System.Text.RegularExpressions;

public static class LoginUtils
{
    /// <summary>
    /// Checks if the email is in a valid format.
    /// </summary>
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        // Simple email regex pattern
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }

    /// <summary>
    /// Checks if the password meets the minimum length requirement.
    /// </summary>
    public static bool IsValidPassword(string password, int minLength)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        return password.Length >= minLength;
    }

    //Check character length of string
    public static bool IsValidStringLength(string input, int minLength, int maxLength)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        return input.Length >= minLength && input.Length <= maxLength;
    }
}