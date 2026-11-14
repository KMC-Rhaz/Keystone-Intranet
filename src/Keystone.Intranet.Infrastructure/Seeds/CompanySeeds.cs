using Keystone.Intranet.Core.Entities;

namespace Keystone.Intranet.Infrastructure.Seeds;

// company tints live in tokens.css keyed by company key, so no colors are stored here
public static class CompanySeeds
{
    public static IReadOnlyList<Company> All() =>
    [
        Make("keystone", "Keystone Synergy Management Services Corporation", "Keystone", 0),
        Make("j3m", "J3M Fruits & Vegetables Wholesaling", "J3M", 1),
        Make("ktweens", "K-Tweens Travel & Tours", "K-Tweens", 2),
        Make("travellovers", "Travellovers Philippines Transport Inc.", "Travellovers", 3),
        Make("printingdistrict", "Printing District Philippines Inc.", "Printing District", 4),
        Make("infin8", "Infin8 Horizon Construction & General Services Inc.", "Infin8", 5),
        Make("bluearth", "Blue Earth Alternative Radiance Inc.", "Blue Earth", 6),
    ];

    private static Company Make(string key, string name, string shortName, int order) =>
        new() { Key = key, Name = name, ShortName = shortName, SortOrder = order, CreatedBy = "seed" };
}
