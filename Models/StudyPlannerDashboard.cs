namespace _10_project_webiste.Models;

public sealed record StudyPlannerDashboard(
    IReadOnlyList<StudyTask> Tasks,
    IReadOnlyList<Subject> Subjects,
    IReadOnlyList<DailyStudyPlan> WeeklyPlan);

public sealed record DailyStudyPlan(DateTime Date, int Minutes);