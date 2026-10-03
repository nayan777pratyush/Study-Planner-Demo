namespace _10_project_webiste.Models;

public abstract class StudyPlannerException(string message) : Exception(message);

public sealed class StudyPlannerRuleException(string message) : StudyPlannerException(message);