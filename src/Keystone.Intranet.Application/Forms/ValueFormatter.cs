using System.Globalization;
using Keystone.Intranet.Core.Constants;
using Keystone.Intranet.Core.Entities;

namespace Keystone.Intranet.Application.Forms;

// turns stored normalized answers into text people can read
public sealed class ValueFormatter
{
    private static readonly CultureInfo Peso = CultureInfo.GetCultureInfo("en-PH");

    public string Format(FormField field, string? raw, string yes = "Yes", string no = "No")
    {
        if (string.IsNullOrEmpty(raw)) return string.Empty;
        return field.Type switch
        {
            FieldTypes.Currency when decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount)
                => amount.ToString("C2", Peso),
            FieldTypes.Number when decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var number)
                => number.ToString("#,##0.##", Peso),
            FieldTypes.Date when FieldValueParser.TryParseDate(raw, out var date)
                => date.ToString("d MMM yyyy", CultureInfo.InvariantCulture),
            FieldTypes.Select => field.Options.FirstOrDefault(o => o.Value == raw)?.Label ?? raw,
            FieldTypes.Checkbox => raw == "true" ? yes : no,
            _ => raw
        };
    }
}
