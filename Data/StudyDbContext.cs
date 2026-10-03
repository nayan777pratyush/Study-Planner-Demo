using _10_project_webiste.Models;
using Microsoft.EntityFrameworkCore;

namespace _10_project_webiste.Data;

public class StudyDbContext(DbContextOptions<StudyDbContext> options) : DbContext(options)
{
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<StudyTask> StudyTasks => Set<StudyTask>();

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
        });
    }
}