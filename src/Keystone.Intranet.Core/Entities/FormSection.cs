namespace Keystone.Intranet.Core.Entities;

public sealed class FormSection
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Intro { get; set; }
    public List<FormField> Fields { get; set; } = [];
}
