namespace Conversey.BL.Administration;

public static class YouthEmailHelper
{
    public static bool ShouldReplaceEmail(string currentEmail, string newEmail)
    {
        if (IsPlaceholderEmail(newEmail)) return false;

        var normalizedCurrent = currentEmail?.Trim() ?? string.Empty;
        if (normalizedCurrent.Length == 0) return true;

        return normalizedCurrent.EndsWith("@local.invalid", StringComparison.OrdinalIgnoreCase) ||
               !string.Equals(normalizedCurrent, newEmail, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsPlaceholderEmail(string email)
    {
        var normalized = email?.Trim() ?? string.Empty;
        return normalized.Length == 0 || normalized.EndsWith("@local.invalid", StringComparison.OrdinalIgnoreCase);
    }
}