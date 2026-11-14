namespace Keystone.Intranet.Application.Forms;

public sealed record ValidatedForm(Dictionary<string, string> Values, List<string> AttachmentIds);
