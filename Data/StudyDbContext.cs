using _10_project_webiste.Models;
using Microsoft.EntityFrameworkCore;

namespace _10_project_webiste.Data;

public class StudyDbContext(DbContextOptions<StudyDbContext> options) : DbContext(options)
{
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<StudyTask> StudyTasks => Set<StudyTask>();
    public DbSet<TaskAttachment> TaskAttachments => Set<TaskAttachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Subject>(entity =>
        {
            entity.ToTable("subjects");
            entity.HasKey(subject => subject.Id);
            entity.Property(subject => subject.Name).HasMaxLength(40).IsRequired();
            entity.Property(subject => subject.Color).HasMaxLength(7).IsRequired();
            entity.HasMany(subject => subject.Tasks)
                .WithOne(task => task.Subject)
                .HasForeignKey(task => task.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Navigation(subject => subject.Tasks)
                .HasField("_tasks")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<StudyTask>(entity =>
        {
            entity.ToTable("study_tasks");
            entity.HasKey(task => task.Id);
            entity.Property(task => task.Title).HasMaxLength(120).IsRequired();
            entity.Property(task => task.Priority).HasMaxLength(10).IsRequired();
            entity.Property(task => task.DueDate).HasColumnType("date");
            entity.Property(task => task.Notes).HasMaxLength(5000).HasDefaultValue("");
            entity.HasMany(task => task.Attachments)
                .WithOne(a => a.Task)
                .HasForeignKey(a => a.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Navigation(task => task.Attachments)
                .HasField("_attachments")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<TaskAttachment>(entity =>
        {
            entity.ToTable("task_attachments");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Content).HasMaxLength(5000).IsRequired();
            entity.Property(a => a.Label).HasMaxLength(200);
            entity.Property(a => a.Type).HasConversion<string>().HasMaxLength(10).IsRequired();
        });
    }
}
