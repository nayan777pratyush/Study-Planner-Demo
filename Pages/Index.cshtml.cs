using _10_project_webiste.Models;
using _10_project_webiste.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace _10_project_webiste.Pages;

public class IndexModel(
    IStudyPlannerService planner,
    IConfiguration configuration,
    ILogger<IndexModel> logger) : PageModel
{
    public List<StudyTask> Tasks { get; private set; } = [];
    public List<Subject> Subjects { get; private set; } = [];
    public List<DailyStudyPlan> WeeklyPlan { get; private set; } = [];
    public bool IsDatabaseConfigured => !string.IsNullOrWhiteSpace(configuration.GetConnectionString("Neon"));
    public string? DatabaseError { get; private set; }
    public string? StatusMessage { get; private set; }

    public int CompletedCount => Tasks.Count(task => task.IsComplete);
    public int PendingCount => Tasks.Count - CompletedCount;
    public int OverdueCount => Tasks.Count(task => !task.IsComplete && task.DueDate < DateOnly.FromDateTime(DateTime.Today));
    public int ProgressPercentage => Tasks.Count == 0 ? 0 : (int)Math.Round((double)CompletedCount / Tasks.Count * 100);
    public int PlannedMinutes => Tasks.Where(task => !task.IsComplete).Sum(task => task.EstimatedMinutes);

    public async Task OnGetAsync()
    {
        StatusMessage = TempData["StatusMessage"] as string;
        if (!IsDatabaseConfigured)
        {
            return;
        }

        try
        {
            var dashboard = await planner.GetDashboardAsync();
            Tasks = dashboard.Tasks.ToList();
            Subjects = dashboard.Subjects.ToList();
            WeeklyPlan = dashboard.WeeklyPlan.ToList();
        }
        catch (Exception exception)
        {
            logger.LogError(
                "Failed to load the study planner dashboard. Exception type: {ExceptionType}",
                exception.GetType().Name);
            DatabaseError = "Couldn't load your study plan. Check the database connection and try again.";
        }
    }

    public Task<IActionResult> OnPostAddTaskAsync(string title, int subjectId, DateOnly? dueDate, int estimatedMinutes, string priority) =>
        ExecuteMutationAsync(
            () => planner.AddTaskAsync(title, subjectId, dueDate, estimatedMinutes, priority),
            "Task added to your plan.");

    public Task<IActionResult> OnPostToggleAsync(int id) =>
        ExecuteMutationAsync(() => planner.ToggleTaskAsync(id));

    public Task<IActionResult> OnPostDeleteAsync(int id) =>
        ExecuteMutationAsync(() => planner.DeleteTaskAsync(id), "Task removed.");

    public Task<IActionResult> OnPostAddSubjectAsync(string subjectName, string color) =>
        ExecuteMutationAsync(() => planner.AddSubjectAsync(subjectName, color), "Subject added.");

    private async Task<IActionResult> ExecuteMutationAsync(Func<Task> mutation, string? successMessage = null)
    {
        if (!IsDatabaseConfigured)
        {
            return DatabaseSetupRedirect();
        }

        try
        {
            await mutation();
            if (successMessage is not null)
            {
                TempData["StatusMessage"] = successMessage;
            }
        }
        catch (StudyPlannerException exception)
        {
            TempData["StatusMessage"] = exception.Message;
        }
        catch (DbUpdateException exception)
        {
            logger.LogError(
                "Failed to save a study planner change. Exception type: {ExceptionType}",
                exception.GetType().Name);
            TempData["StatusMessage"] = "Couldn't save that change. Check the database and try again.";
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(
                "Unexpected error while updating the study planner. Exception type: {ExceptionType}",
                exception.GetType().Name);
            TempData["StatusMessage"] = "Couldn't complete that change. Please try again.";
        }

        return RedirectToPage();
    }

    private IActionResult DatabaseSetupRedirect()
    {
        TempData["StatusMessage"] = "Add your Neon connection string to ConnectionStrings__Neon first.";
        return RedirectToPage();
    }

}

