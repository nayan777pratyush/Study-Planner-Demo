using _10_project_webiste.Models;
using _10_project_webiste.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace _10_project_webiste.Pages.Tasks;

public class DetailModel(
    IStudyPlannerService planner,
    IWebHostEnvironment env,
    IConfiguration configuration,
    ILogger<DetailModel> logger) : PageModel
{
    public StudyTask? Task { get; private set; }
    public bool IsDatabaseConfigured => !string.IsNullOrWhiteSpace(configuration.GetConnectionString("Neon"));
    public string? StatusMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        StatusMessage = TempData["StatusMessage"] as string;
        if (!IsDatabaseConfigured) return RedirectToPage("/Index");
        Task = await planner.GetTaskDetailAsync(id);
        if (Task is null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostSaveNotesAsync(int id, string notes)
    {
        if (!IsDatabaseConfigured) return RedirectToPage("/Index");
        try
        {
            await planner.UpdateTaskNotesAsync(id, notes ?? "");
            TempData["StatusMessage"] = "Notes saved.";
        }
        catch (StudyPlannerException ex) { TempData["StatusMessage"] = ex.Message; }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError("Error saving notes: {Type}", ex.GetType().Name);
            TempData["StatusMessage"] = "Couldn't save notes. Try again.";
        }
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostAddLinkAsync(int id, string url, string? linkLabel)
    {
        if (!IsDatabaseConfigured) return RedirectToPage("/Index");
        try
        {
            await planner.AddLinkAttachmentAsync(id, url, linkLabel);
            TempData["StatusMessage"] = "Link added.";
        }
        catch (StudyPlannerException ex) { TempData["StatusMessage"] = ex.Message; }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError("Error adding link: {Type}", ex.GetType().Name);
            TempData["StatusMessage"] = "Couldn't add link. Try again.";
        }
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostUploadFileAsync(int id, IFormFile? file, string? fileLabel)
    {
        if (!IsDatabaseConfigured) return RedirectToPage("/Index");
        if (file is null || file.Length == 0)
        {
            TempData["StatusMessage"] = "Please choose a file to upload.";
            return RedirectToPage(new { id });
        }
        if (file.Length > 10 * 1024 * 1024)
        {
            TempData["StatusMessage"] = "File must be under 10 MB.";
            return RedirectToPage(new { id });
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
        {
            TempData["StatusMessage"] = "Only JPG, PNG, GIF, WebP images and PDFs are allowed.";
            return RedirectToPage(new { id });
        }

        var uploadsDir = Path.Combine(env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);

        var uniqueFileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsDir, uniqueFileName);
        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        var virtualPath = $"/uploads/{uniqueFileName}";
        var attachmentType = ext == ".pdf" ? AttachmentType.File : AttachmentType.Image;

        try
        {
            await planner.AddFileAttachmentAsync(id, virtualPath, fileLabel ?? file.FileName, attachmentType);
            TempData["StatusMessage"] = "File uploaded successfully.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Clean up the uploaded file if DB save failed
            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            logger.LogError("Error saving file attachment: {Type}", ex.GetType().Name);
            TempData["StatusMessage"] = "Couldn't save the file. Try again.";
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteAttachmentAsync(int id, int attachmentId)
    {
        if (!IsDatabaseConfigured) return RedirectToPage("/Index");
        try
        {
            var uploadsDir = Path.Combine(env.WebRootPath, "uploads");
            await planner.DeleteAttachmentAsync(attachmentId, uploadsDir);
            TempData["StatusMessage"] = "Attachment removed.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError("Error deleting attachment: {Type}", ex.GetType().Name);
            TempData["StatusMessage"] = "Couldn't remove attachment. Try again.";
        }
        return RedirectToPage(new { id });
    }
}
