using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Safexchange.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class VietnamesePhoneAttribute : ValidationAttribute
{
    private static readonly Regex PhonePattern = new(
        @"^(?:\+?84|0)(?:3[2-9]|5[2689]|7[06-9]|8[1-9]|9\d)\d{7}$",
        RegexOptions.Compiled);

    public VietnamesePhoneAttribute()
    {
        ErrorMessage = "Số điện thoại không hợp lệ. Vui lòng nhập số Việt Nam (vd: 0912345678).";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        var phone = value.ToString()?.Trim();
        if (string.IsNullOrWhiteSpace(phone))
        {
            return ValidationResult.Success;
        }

        var normalized = Regex.Replace(phone, @"[\s\-\.\(\)]", string.Empty);
        return PhonePattern.IsMatch(normalized)
            ? ValidationResult.Success
            : new ValidationResult(ErrorMessage);
    }

    public static string? Normalize(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return null;
        }

        var normalized = Regex.Replace(phone.Trim(), @"[\s\-\.\(\)]", string.Empty);
        if (!PhonePattern.IsMatch(normalized))
        {
            return null;
        }

        if (normalized.StartsWith("+84", StringComparison.Ordinal))
        {
            return "0" + normalized[3..];
        }

        if (normalized.StartsWith("84", StringComparison.Ordinal) && normalized.Length == 11)
        {
            return "0" + normalized[2..];
        }

        return normalized;
    }
}
