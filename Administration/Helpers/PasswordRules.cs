using System.Text.RegularExpressions;

namespace Administration.Helpers;

/// <summary>Shared rules for strong passwords.</summary>
public static class PasswordRules
{
    public const int MinLength = 8;

    public static bool TryValidate(string password, out string? errorMessage)
    {
        errorMessage = null;
        if (string.IsNullOrEmpty(password))
        {
            errorMessage = $"Le mot de passe doit contenir au moins {MinLength} caractères, une majuscule, une minuscule, un chiffre et un caractère spécial.";
            return false;
        }

        if (password.Length < MinLength)
        {
            errorMessage = $"Le mot de passe doit contenir au moins {MinLength} caractères.";
            return false;
        }

        if (!Regex.IsMatch(password, @"[A-Z]"))
        {
            errorMessage = "Le mot de passe doit contenir au moins une lettre majuscule (A-Z).";
            return false;
        }

        if (!Regex.IsMatch(password, @"[a-z]"))
        {
            errorMessage = "Le mot de passe doit contenir au moins une lettre minuscule (a-z).";
            return false;
        }

        if (!Regex.IsMatch(password, @"[0-9]"))
        {
            errorMessage = "Le mot de passe doit contenir au moins un chiffre (0-9).";
            return false;
        }

        if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
        {
            errorMessage = "Le mot de passe doit contenir au moins un caractère spécial (@, #, $, etc.).";
            return false;
        }

        return true;
    }
}
