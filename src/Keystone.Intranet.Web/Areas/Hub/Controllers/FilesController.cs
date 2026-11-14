using Keystone.Intranet.Application.Files;
using Keystone.Intranet.Application.Requests;
using Keystone.Intranet.Web.Models;
using Keystone.Intranet.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Keystone.Intranet.Web.Areas.Hub.Controllers;

[Area("Hub")]
[Authorize]
public sealed class FilesController(UploadService uploads, RequestQueryService queries, CurrentActor current) : Controller
{
    [EnableRateLimiting("upload")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(UploadService.MaxBytes + 65536)]
    public async Task<IActionResult> Upload(IFormFile? file, CancellationToken ct)
    {
        if (!current.IsApproved) return Forbid();
        if (file is null) return BadRequest(ApiResponse.Fail("Please choose a file."));

        await using var stream = file.OpenReadStream();
        var saved = await uploads.UploadAsync(file.FileName, file.Length, stream, current.Actor!, ct);
        return Json(ApiResponse.Ok("File attached.", new { id = saved.Id, name = saved.OriginalName, length = saved.Length }, "info"));
    }

    [HttpGet]
    public async Task<IActionResult> Download(string id, CancellationToken ct)
    {
        if (!current.IsApproved) return Forbid();
        var (meta, stream) = await queries.OpenAttachmentAsync(id, current.Actor!, ct);
        return File(stream, "application/octet-stream", meta.OriginalName);
    }
}
