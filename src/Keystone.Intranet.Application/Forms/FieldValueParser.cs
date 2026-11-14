using System.Globalization;
using Keystone.Intranet.Core.Constants;
using Keystone.Intranet.Core.Entities;

namespace Keystone.Intranet.Application.Forms;

// checks one scalar answer against its field definition and returns the normalized value or an error key
public sealed class FieldValueParser
{
    public const int DefaultTextLength = 200;
    public const int DefaultTextAreaLength = 2000;
    public const string DateFormat = "yyyy-MM-dd";

    public (string? Value, string? Error) Parse(FormField field, string raw) => field.Type switch
    {
        FieldTypes.Text => Text(raw, field.MaxLength ?? DefaultTextLength),
        FieldTypes.TextArea => Text(raw, field.MaxLength ?? DefaultTextAreaLength),
        FieldTypes.Number => Number(field, raw, false),
        FieldTypes.Currency => Number(field, raw, true),
        FieldTypes.Date => Date(raw),
        FieldTypes.Select => Select(field, raw),
        FieldTypes.Checkbox => Checkbox(field, raw),
        _ => (null, "validation.invalid")
    };

    public static bool TryParseDate(string? raw, out DateOnly date) =>
        DateOnly.TryParseExact(raw, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

    private static (string?, string?) Text(string raw, int maxLength)
    {
        var normalized = raw.Replace("\r\n", "\n");
        return normalized.Length > maxLength ? (null, "validation.too_long") : (normalized, null);
    }

    private static (string?, string?) Number(FormField field, string raw, bool isCurrency)
    {
        var cleaned = raw.Replace(",", string.Empty);
        if (!decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out var number))
            return (null, "validation.number");
        if (field.Min is { } min && number < min) return (null, "validation.too_small");
        if (field.Max is { } max && number > max) return (null, "validation.too_large");
        var normalized = isCurrency
            ? decimal.Round(number, 2, MidpointRounding.AwayFromZero).ToString("F2", CultureInfo.InvariantCulture)
            : number.ToString(CultureInfo.InvariantCulture);
        return (normalized, null);
    }

    private static (string?, string?) Date(string raw) =>
        TryParseDate(raw, out var date)
            ? (date.ToString(DateFormat, CultureInfo.InvariantCulture), null)
            : (null, "validation.date");

    private static (string?, string?) Select(FormField field, string raw) =>
        field.Options.Any(o => o.Value == raw) ? (raw, null) : (null, "validation.option");

    private static (string?, string?) Checkbox(FormField field, string raw)
    {
        var isChecked = raw is "true" or "on" or "1";
        if (field.Required && !isChecked) return (null, "validation.required");
        return (isChecked ? "true" : "false", null);
    }
}
