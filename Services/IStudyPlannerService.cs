using _10_project_webiste.Models;

namespace _10_project_webiste.Services;

public interface IStudyPlannerService
{
    Task<StudyPlannerDashboard> GetDashboardAsync();
    Task AddTaskAsync(string title, int subjectId, DateOnly? dueDate, int estimatedMinutes, string priority);
    Task ToggleTaskAsync(int id);
    Task DeleteTaskAsync(int id);
    Task AddSubjectAsync(string subjectName, string color);
}