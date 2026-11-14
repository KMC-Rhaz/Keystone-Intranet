using Keystone.Intranet.Core.Constants;

namespace Keystone.Intranet.Web.Localization;

// plain, jargon-free strings in english and filipino
public sealed class Translator
{
    private static readonly Dictionary<string, (string En, string Fil)> Strings = new(StringComparer.Ordinal)
    {
        ["app.title"] = ("Keystone Intranet", "Keystone Intranet"),
        ["nav.hub"] = ("Company Hub", "Company Hub"),
        ["nav.requests"] = ("My Requests", "Mga Kahilingan Ko"),
        ["nav.approvals"] = ("Approvals", "Mga Pag-apruba"),
        ["nav.dashboard"] = ("Dashboard", "Dashboard"),
        ["nav.users"] = ("Users", "Mga Gumagamit"),
        ["nav.sign_out"] = ("Sign out", "Mag-sign out"),
        ["auth.sign_in_title"] = ("Sign in to Keystone", "Mag-sign in sa Keystone"),
        ["auth.sign_in_subtitle"] = ("Use your Google account to open the company hub.", "Gamitin ang iyong Google account para buksan ang company hub."),
        ["auth.sign_in_google"] = ("Continue with Google", "Magpatuloy gamit ang Google"),
        ["auth.waiting_title"] = ("Waiting for approval", "Naghihintay ng pag-apruba"),
        ["auth.waiting_body"] = ("Your account is signed in, but an admin still needs to set up your company and access.", "Naka-sign in ka na, pero kailangan pang ayusin ng admin ang iyong kumpanya at access."),
        ["auth.rejected_body"] = ("Your access request was not approved. Please contact HR or IT.", "Hindi naaprubahan ang iyong access. Mangyaring makipag-ugnayan sa HR o IT."),
        ["hub.title"] = ("What do you need today?", "Ano ang kailangan mo ngayon?"),
        ["hub.subtitle"] = ("Pick a form below to send a request.", "Pumili ng form sa ibaba para magpadala ng kahilingan."),
        ["hub.open_form"] = ("Open form", "Buksan ang form"),
        ["hub.choose_advance"] = ("Pick an approved cash advance", "Pumili ng aprubadong cash advance"),
        ["hub.no_advances"] = ("You have no approved cash advances to attach receipts to yet.", "Wala ka pang aprubadong cash advance na puwedeng lagyan ng resibo."),
        ["request.submit"] = ("Send request", "Ipadala ang kahilingan"),
        ["request.resubmit"] = ("Send updated request", "Ipadala ang na-update na kahilingan"),
        ["request.cancel"] = ("Cancel request", "Kanselahin ang kahilingan"),
        ["request.cancel_confirm"] = ("Are you sure you want to cancel this request?", "Sigurado ka bang gusto mong kanselahin ito?"),
        ["request.submitted"] = ("Your request was sent for approval.", "Naipadala na ang iyong kahilingan para sa pag-apruba."),
        ["request.resubmitted"] = ("Your updated request was sent for review.", "Naipadala na ang iyong na-update na kahilingan."),
        ["request.approved"] = ("Request approved.", "Aprubado na ang kahilingan."),
        ["request.rejected"] = ("Request rejected.", "Tinanggihan ang kahilingan."),
        ["request.returned"] = ("Request sent back for updates.", "Ibinalik ang kahilingan para sa pagbabago."),
        ["request.cancelled"] = ("Request cancelled.", "Nakansela na ang kahilingan."),
        ["user.approved"] = ("User access saved.", "Na-save na ang access ng gumagamit."),
        ["user.rejected"] = ("User access declined.", "Tinanggihan ang access ng gumagamit."),
        ["status.pending"] = ("Waiting", "Naghihintay"),
        ["status.approved"] = ("Approved", "Aprubado"),
        ["status.rejected"] = ("Rejected", "Tinanggihan"),
        ["status.returned"] = ("Needs update", "Kailangan baguhin"),
        ["status.cancelled"] = ("Cancelled", "Nakansela"),
        ["status.draft"] = ("Draft", "Draft"),
        ["error.not_found"] = ("We could not find that item.", "Hindi namin makita ang hinahanap mo."),
        ["error.forbidden"] = ("You do not have permission to do that.", "Wala kang pahintulot na gawin iyan."),
        ["error.stale"] = ("Someone else updated this just now. Please reload and try again.", "May ibang nag-update nito ngayon lang. Paki-reload at subukan muli."),
        ["error.self_approval"] = ("You cannot approve or decide on your own request.", "Hindi mo puwedeng aprubahan ang sarili mong kahilingan."),
        ["error.not_pending"] = ("This request is no longer waiting for a decision.", "Hindi na naghihintay ng desisyon ang kahilingang ito."),
        ["error.not_returned"] = ("Only returned requests can be edited here.", "Mga ibinalik na kahilingan lang ang puwedeng baguhin dito."),
        ["error.cannot_cancel"] = ("This request can no longer be cancelled.", "Hindi na puwedeng kanselahin ang kahilingang ito."),
        ["error.cannot_submit"] = ("You do not have access to submit this form.", "Wala kang access para magsumite ng form na ito."),
        ["error.company"] = ("You do not belong to the selected company.", "Hindi ka kabilang sa napiling kumpanya."),
        ["error.user_not_pending"] = ("This user is no longer waiting for approval.", "Hindi na naghihintay ng pag-apruba ang gumagamit na ito."),
        ["error.cannot_grant_super_admin"] = ("Only a super admin can grant super admin access.", "Super admin lang ang puwedeng magbigay ng super admin access."),
        ["error.domain_not_allowed"] = ("Your Google account domain is not allowed here.", "Hindi pinapayagan ang domain ng iyong Google account dito."),
        ["error.validation"] = ("Please check the highlighted fields.", "Pakisuri ang mga may markang patlang."),
        ["error.unexpected"] = ("Something went wrong. Please try again.", "May nangyaring mali. Pakisubukan muli."),
        ["validation.required"] = ("This field is required.", "Kailangan punan ito."),
        ["validation.too_long"] = ("This is too long.", "Masyadong mahaba ito."),
        ["validation.number"] = ("Please enter a valid number.", "Maglagay ng tamang numero."),
        ["validation.too_small"] = ("This number is too small.", "Masyadong maliit ang numerong ito."),
        ["validation.too_large"] = ("This number is too large.", "Masyadong malaki ang numerong ito."),
        ["validation.date"] = ("Please pick a valid date.", "Pumili ng tamang petsa."),
        ["validation.date_before"] = ("This date cannot be earlier than the start date.", "Hindi puwedeng mas maaga ito sa unang petsa."),
        ["validation.option"] = ("Please pick one of the choices.", "Pumili mula sa mga nakalista."),
        ["validation.file"] = ("Please upload a valid file.", "Mag-upload ng tamang file."),
        ["validation.file_required"] = ("Please attach at least one file.", "Maglakip ng kahit isang file."),
        ["validation.file_empty"] = ("The file is empty.", "Walang laman ang file."),
        ["validation.file_too_big"] = ("File is larger than 10 MB.", "Mas malaki sa 10 MB ang file."),
        ["validation.file_type"] = ("File type is not allowed.", "Hindi pinapayagan ang ganitong uri ng file."),
        ["validation.comment_required"] = ("Please add a short note explaining why.", "Maglagay ng maikling paliwanag."),
        ["validation.company_required"] = ("Pick at least one company.", "Pumili ng kahit isang kumpanya."),
        ["validation.linked_required"] = ("Please select the related cash advance.", "Piliin ang kaugnay na cash advance.")
    };

    public string Get(string key, string language = Languages.English)
    {
        if (!Strings.TryGetValue(key, out var pair)) return key;
        return language == Languages.Filipino ? pair.Fil : pair.En;
    }

    public string StatusLabel(string status, string language = Languages.English) =>
        Get($"status.{status}", language);
}
