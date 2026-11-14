namespace Keystone.Intranet.Core.Entities;

public sealed class FormField
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool Required { get; set; }
    public string? HelpText { get; set; }
    public int? MaxLength { get; set; }
    public decimal? Min { get; set; }
    public decimal? Max { get; set; }
    public string? MinFieldRef { get; set; }
    public List<FieldOption> Options { get; set; } = [];
}
