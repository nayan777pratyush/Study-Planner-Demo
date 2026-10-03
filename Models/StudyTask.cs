namespace _10_project_webiste.Models;

public class StudyTask
{
    private StudyTask() { }

    internal StudyTask(string title, DateOnly? dueDate, int estimatedMinutes, string priority, Subject subject)
    {
        Title = NormalizeTitle(title);
        DueDate = dueDate;
        EstimatedMinutes = ValidateEstimatedMinutes(estimatedMinutes);
        Priority = ValidatePriority(priority);
        Subject = subject ?? throw new StudyPlannerRuleException("Choose a subject for this task.");
        SubjectId = subject.Id;
    }

    public int Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public int SubjectId { get; private set; }
    public Subject Subject { get; private set; } = null!;
    public DateOnly? DueDate { get; private set; }
    public int EstimatedMinutes { get; private set; }
    public string Priority { get; private set; } = "Medium";
    public bool IsComplete { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    public void ToggleCompletion()
    {
        IsComplete = !IsComplete;
    }

    internal void Rename(string title)
    {
        Title = NormalizeTitle(title);
    }

    private static string NormalizeTitle(string title)
    {
        var normalizedTitle = title?.Trim() ?? string.Empty;
        if (normalizedTitle.Length is < 1 or > 120)
        {
            throw new StudyPlannerRuleException("Task titles must be between 1 and 120 characters.");
        }

        return normalizedTitle;
    }

    private static int ValidateEstimatedMinutes(int minutes)
    {
        if (minutes is < 5 or > 600)
        {
            throw new StudyPlannerRuleException("Study time must be between 5 and 600 minutes.");
        }

        return minutes;
    }

    private static string ValidatePriority(string priority)
    {
        if (priority is not ("Low" or "Medium" or "High"))
        {
            throw new StudyPlannerRuleException("Choose Low, Medium, or High priority.");
        }

        return priority;
    }
}