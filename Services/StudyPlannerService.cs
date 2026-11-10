using _10_project_webiste.Data;
using _10_project_webiste.Models;
using Microsoft.EntityFrameworkCore;

namespace _10_project_webiste.Services;

public sealed class StudyPlannerService(StudyDbContext db) : IStudyPlannerService
{
    private static readonly (string Title, int Minutes, string Priority)[] StarterTasks =
    [
        ("Review C# fundamentals", 25, "High"),
        ("Organize student progress by course", 30, "High"),
        ("Plan focused practice sessions", 25, "Medium"),
        ("Review upcoming course deadlines", 30, "High"),
        ("Check completed and pending work", 20, "Medium"),
        ("Prepare a weekly progress summary", 30, "Medium")
    ];

    private static readonly IReadOnlyDictionary<string, string> PreviousStarterTaskTitles =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["IA-1: Practice control statements"] = "Review C# fundamentals",
            ["IA-1: Build a student class and objects"] = "Organize student progress by course",
            ["IA-1: Encapsulate student records"] = "Plan focused practice sessions",
            ["IA-2: Model inheritance for student types"] = "Review upcoming course deadlines",
            ["IA-2: Handle invalid study input"] = "Check completed and pending work",
            ["IA-2: Add an interface-based report"] = "Prepare a weekly progress summary"
        };

    public async Task<StudyPlannerDashboard> GetDashboardAsync()
    {
        await EnsureDatabaseReadyAsync();

        var subjects = await db.Subjects
            .Include(subject => subject.Tasks)
            .OrderBy(subject => subject.Name)
            .ToListAsync();
        var tasks = await db.StudyTasks
            .Include(task => task.Subject)
            .OrderBy(task => task.IsComplete)
            .ThenBy(task => task.DueDate ?? DateOnly.MaxValue)
            .ThenBy(task => task.Title)
            .ToListAsync();

        var monday = DateTime.Today.AddDays(-(((int)DateTime.Today.DayOfWeek + 6) % 7));
        var weeklyPlan = Enumerable.Range(0, 7)
            .Select(offset => monday.AddDays(offset))
            .Select(date => new DailyStudyPlan(date, tasks
                .Where(task => !task.IsComplete && task.DueDate == DateOnly.FromDateTime(date))
                .Sum(task => task.EstimatedMinutes)))
            .ToList();

        return new StudyPlannerDashboard(tasks, subjects, weeklyPlan);
    }

    public async Task AddTaskAsync(string title, int subjectId, DateOnly? dueDate, int estimatedMinutes, string priority)
    {
        await EnsureDatabaseReadyAsync();
        var subject = await db.Subjects.FindAsync(subjectId);
        if (subject is null)
        {
            throw new StudyPlannerRuleException("Choose an existing subject for this task.");
        }

        subject.AddTask(title, dueDate, estimatedMinutes, priority);
        await db.SaveChangesAsync();
    }

    public async Task ToggleTaskAsync(int id)
    {
        await EnsureDatabaseReadyAsync();
        var task = await db.StudyTasks.FindAsync(id);
        if (task is null)
        {
            return;
        }

        task.ToggleCompletion();
        await db.SaveChangesAsync();
    }

    public async Task DeleteTaskAsync(int id)
    {
        await EnsureDatabaseReadyAsync();
        var task = await db.StudyTasks.FindAsync(id);
        if (task is null)
        {
            return;
        }

        db.StudyTasks.Remove(task);
        await db.SaveChangesAsync();
    }

    public async Task AddSubjectAsync(string subjectName, string color)
    {
        await EnsureDatabaseReadyAsync();
        var normalizedName = subjectName?.Trim() ?? string.Empty;
        if (await db.Subjects.AnyAsync(subject => subject.Name.ToLower() == normalizedName.ToLower()))
        {
            throw new StudyPlannerRuleException("That subject already exists.");
        }

        db.Subjects.Add(Subject.Create(normalizedName, color));
        await db.SaveChangesAsync();
    }

    public async Task<StudyTask?> GetTaskDetailAsync(int id)
    {
        await EnsureDatabaseReadyAsync();
        return await db.StudyTasks
            .Include(task => task.Subject)
            .Include(task => task.Attachments)
            .FirstOrDefaultAsync(task => task.Id == id);
    }

    public async Task UpdateTaskNotesAsync(int taskId, string notes)
    {
        await EnsureDatabaseReadyAsync();
        var task = await db.StudyTasks.FindAsync(taskId);
        if (task is null) return;
        task.UpdateNotes(notes);
        await db.SaveChangesAsync();
    }

    public async Task AddLinkAttachmentAsync(int taskId, string url, string? label)
    {
        await EnsureDatabaseReadyAsync();
        var task = await db.StudyTasks.Include(t => t.Attachments).FirstOrDefaultAsync(t => t.Id == taskId);
        if (task is null) throw new StudyPlannerRuleException("Task not found.");
        if (!Uri.TryCreate(url?.Trim(), UriKind.Absolute, out var uri) || (uri.Scheme != "http" && uri.Scheme != "https"))
            throw new StudyPlannerRuleException("Please enter a valid http or https URL.");
        task.AddAttachment(AttachmentType.Link, url!.Trim(), label);
        await db.SaveChangesAsync();
    }

    public async Task AddFileAttachmentAsync(int taskId, string filePath, string? label, AttachmentType type)
    {
        await EnsureDatabaseReadyAsync();
        var task = await db.StudyTasks.Include(t => t.Attachments).FirstOrDefaultAsync(t => t.Id == taskId);
        if (task is null) throw new StudyPlannerRuleException("Task not found.");
        task.AddAttachment(type, filePath, label);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAttachmentAsync(int attachmentId, string uploadsDirectory)
    {
        await EnsureDatabaseReadyAsync();
        var attachment = await db.TaskAttachments.FindAsync(attachmentId);
        if (attachment is null) return;
        // If it's a file/image, delete from disk
        if (attachment.Type is AttachmentType.Image or AttachmentType.File)
        {
            var fileName = Path.GetFileName(attachment.Content);
            var filePath = Path.Combine(uploadsDirectory, fileName);
            if (File.Exists(filePath)) File.Delete(filePath);
        }
        db.TaskAttachments.Remove(attachment);
        await db.SaveChangesAsync();
    }

    private async Task EnsureDatabaseReadyAsync()
    {
        await db.Database.MigrateAsync();

        var subject = await db.Subjects.FirstOrDefaultAsync(item => item.Name == "C# Programming");
        if (subject is null)
        {
            subject = Subject.Create("C# Programming", "#237a57");
            db.Subjects.Add(subject);
            await db.SaveChangesAsync();
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var starterTaskTitles = StarterTasks.Select(task => task.Title)
            .Concat(PreviousStarterTaskTitles.Keys)
            .ToArray();
        var existingStarterTasks = await db.StudyTasks
            .Where(task => task.SubjectId == subject.Id && starterTaskTitles.Contains(task.Title))
            .ToListAsync();
        var existingTitles = existingStarterTasks.Select(task => task.Title).ToHashSet(StringComparer.Ordinal);
        var changedStarterTasks = false;

        foreach (var task in existingStarterTasks)
        {
            if (!PreviousStarterTaskTitles.TryGetValue(task.Title, out var replacementTitle))
            {
                continue;
            }

            var previousTitle = task.Title;
            task.Rename(replacementTitle);
            existingTitles.Remove(previousTitle);
            existingTitles.Add(replacementTitle);
            changedStarterTasks = true;
        }

        for (var index = 0; index < StarterTasks.Length; index++)
        {
            var starterTask = StarterTasks[index];
            if (!existingTitles.Add(starterTask.Title))
            {
                continue;
            }

            subject.AddTask(
                starterTask.Title,
                today.AddDays(index + 1),
                starterTask.Minutes,
                starterTask.Priority);
            changedStarterTasks = true;
        }

        if (changedStarterTasks)
        {
            await db.SaveChangesAsync();
        }
    }
}
