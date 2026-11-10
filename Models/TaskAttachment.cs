namespace _10_project_webiste.Models;

public enum AttachmentType { Note, Link, Image, File }

public class TaskAttachment
{
    private TaskAttachment() { }

    internal TaskAttachment(int taskId, AttachmentType type, string content, string? label)
    {
        TaskId = taskId;
        Type = type;
        Content = content.Trim();
        Label = label?.Trim();
    }

    public int Id { get; private set; }
    public int TaskId { get; private set; }
    public StudyTask Task { get; private set; } = null!;
    public AttachmentType Type { get; private set; }
    // For Note: the note text. For Link: the URL. For Image/File: relative path like /uploads/filename
    public string Content { get; private set; } = string.Empty;
    public string? Label { get; private set; } // display label
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
}
