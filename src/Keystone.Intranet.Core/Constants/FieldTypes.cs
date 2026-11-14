using System.Collections.Frozen;

namespace Keystone.Intranet.Core.Constants;

public static class FieldTypes
{
    public const string Text = "text";
    public const string TextArea = "textarea";
    public const string Number = "number";
    public const string Currency = "currency";
    public const string Date = "date";
    public const string Select = "select";
    public const string Checkbox = "checkbox";
    public const string File = "file";

    public static readonly FrozenSet<string> All =
        new[] { Text, TextArea, Number, Currency, Date, Select, Checkbox, File }.ToFrozenSet();
}
