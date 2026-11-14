using Keystone.Intranet.Core.Constants;
using Keystone.Intranet.Core.Entities;
using Keystone.Intranet.Core.Models;

namespace Keystone.Intranet.Web.Security;

// holds the verified user and active company resolved for the current http request
public sealed class CurrentActor
{
    public User? User { get; set; }
    public ActorContext? Actor { get; set; }
    public Company? ActiveCompany { get; set; }
    public IReadOnlyList<Company> AvailableCompanies { get; set; } = [];
    public string Language { get; set; } = Languages.English;

    public bool IsAuthenticated => User is not null;
    public bool IsApproved => User?.Status == UserStatuses.Approved && Actor is not null;
    public bool HasPermission(string code) => Actor?.Permissions.Has(code) ?? false;
}
