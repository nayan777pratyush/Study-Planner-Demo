using System.Collections.ObjectModel;

namespace _10_project_webiste.Models;

public class Subject
{
    private static readonly string[] AllowedColors = ["#237a57", "#d66c4f", "#4e73a1", "#b7862b"];
    private readonly List<StudyTask> _tasks = [];
    private readonly ReadOnlyCollection<StudyTask> _taskView;

    private Subject()
    {
        _taskView = _tasks.AsReadOnly();
    }

    private Subject(string name, string color) : this()
    {
        Name = ValidateName(name);
        Color = ValidateColor(color);
    }

    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Color { get; private set; } = "#237a57";
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public IReadOnlyCollection<StudyTask> Tasks => _taskView;

    public static Subject Create(string name, string color) => new(name, color);

    public StudyTask AddTask(string title, DateOnly? dueDate, int estimatedMinutes, string priority)
    {
        var task = new StudyTask(title, dueDate, estimatedMinutes, priority, this);
        _tasks.Add(task);
        return task;
    }

    private static string ValidateName(string name)
    {
        var normalizedName = name?.Trim() ?? string.Empty;
        if (normalizedName.Length is < 2 or > 40)
        {
            throw new StudyPlannerRuleException("Subject names must be between 2 and 40 characters.");
        }

        return normalizedName;
    }

    private static string ValidateColor(string color)
    {
        if (!AllowedColors.Contains(color))
        {
            throw new StudyPlannerRuleException("Choose one of the available subject colors.");
        }

        return color;
    }
}