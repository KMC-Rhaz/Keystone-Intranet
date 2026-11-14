namespace Keystone.Intranet.Core.Entities;

public sealed class FormSchema
{
    public List<FormSection> Sections { get; set; } = [];

    public IEnumerable<FormField> AllFields() => Sections.SelectMany(s => s.Fields);
}
