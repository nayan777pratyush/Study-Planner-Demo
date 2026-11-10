using _10_project_webiste.Models;

namespace _10_project_webiste.Services;

public interface IStudyPlannerService
{
    Task<StudyPlannerDashboard> GetDashboardAsync();
    Task AddTaskAsync(string title, int subjectId, DateOnly? dueDate, int estimatedMinutes, string priority);
    Task ToggleTaskAsync(int id);
    Task DeleteTaskAsync(int id);
    Task AddSubjectAsync(string subjectName, string color);
    Task<StudyTask?> GetTaskDetailAsync(int id);
    Task UpdateTaskNotesAsync(int taskId, string notes);
    Task AddLinkAttachmentAsync(int taskId, string url, string? label);
    Task AddFileAttachmentAsync(int taskId, string filePath, string? label, AttachmentType type);
    Task DeleteAttachmentAsync(int attachmentId, string uploadsDirectory);
}
