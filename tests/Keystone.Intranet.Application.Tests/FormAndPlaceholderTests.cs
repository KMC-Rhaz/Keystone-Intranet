using Keystone.Intranet.Application.Forms;
using Keystone.Intranet.Core.Entities;
using Keystone.Intranet.Core.Exceptions;
using Keystone.Intranet.Infrastructure.InMemory;
using Keystone.Intranet.Infrastructure.Seeds;
using Xunit;

namespace Keystone.Intranet.Application.Tests;

public sealed class FormAndPlaceholderTests
{
    [Fact]
    public async Task RejectsOperatorInjectionKeys()
    {
        var validator = new FormValidator(new InMemoryAttachmentStore(), new FieldValueParser());
        var def = CashAdvanceDefinition.Create();

        await Assert.ThrowsAsync<ValidationFailedException>(() =>
            validator.ValidateAsync(def, new Dictionary<string, string?> { ["$set"] = "1" }, "u1", null, CancellationToken.None));
    }

    [Fact]
    public async Task RejectsEndDateBeforeStartDate()
    {
        var validator = new FormValidator(new InMemoryAttachmentStore(), new FieldValueParser());
        var def = LeaveRequestDefinition.Create();
        var input = new Dictionary<string, string?>
        {
            ["leaveType"] = "vacation",
            ["dateFrom"] = "2026-11-20",
            ["dateTo"] = "2026-11-18",
            ["reason"] = "Family trip"
        };

        var ex = await Assert.ThrowsAsync<ValidationFailedException>(() =>
            validator.ValidateAsync(def, input, "u1", null, CancellationToken.None));
        Assert.Contains(ex.Errors, e => e.Field == "dateTo" && e.Message == "validation.date_before");
    }

    [Fact]
    public void PlaceholderResolverFillsKnownTokensAndLeavesUnknownVisible()
    {
        var resolver = new PlaceholderResolver(new ValueFormatter());
        var caDef = CashAdvanceDefinition.Create();
        var ca = new RequestInstance
        {
            ReferenceNo = "CA-2026-000001",
            Requester = new PersonSnapshot { UserId = "u1", Name = "Maria Santos" },
            Values = new Dictionary<string, string> { ["amount"] = "1500.00", ["purpose"] = "Client lunch" }
        };

        var tokens = resolver.BuildTokens(ca, caDef);
        var resolved = resolver.Resolve("Receipts for {{REFERENCE}} ({{AMOUNT}}) {{MISSING}}", tokens);

        Assert.Contains("CA-2026-000001", resolved);
        Assert.Contains("1,500.00", resolved);
        Assert.Contains("{{MISSING}}", resolved);
    }

    [Fact]
    public void AllSeededDefinitionsPassValidation()
    {
        var defs = SeedRunner.Definitions();
        var keys = defs.Select(d => d.Key).ToHashSet();
        var validator = new DefinitionValidator();

        foreach (var def in defs)
        {
            var errors = validator.Validate(def, keys);
            Assert.Empty(errors);
        }
    }
}
