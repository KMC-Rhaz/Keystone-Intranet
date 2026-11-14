namespace Keystone.Intranet.Core.Entities;

public sealed class WorkflowDefinition : AuditedEntity
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Icon { get; set; } = "file";
    public int Version { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public List<string> CompanyScope { get; set; } = ["*"];
    public string ReferencePrefix { get; set; } = string.Empty;
    public string SubmitPermission { get; set; } = string.Empty;
    public string? LinkedDefinitionKey { get; set; }
    public string? PdfTemplateKey { get; set; }
    public int SortOrder { get; set; }
    public FormSchema Form { get; set; } = new();
    public List<WorkflowStep> Steps { get; set; } = [];
}
