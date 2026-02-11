namespace Projektverktyg.Models;

public class ProjectData
{
    // Steg 1: Grundinfo
    public string CustomerName { get; set; } = "";
    public string TeamName { get; set; } = "Applikation och Integration";
    public string ProjectTitle { get; set; } = "";
    public string ProjectDescription { get; set; } = "";
    public string SystemA { get; set; } = "";
    public string SystemB { get; set; } = "";

    // Steg 2: Projektgrupp
    public List<TeamMember> ExsitecMembers { get; set; } = new()
    {
        new() { Role = "Projektledare", Group = "PG" }
    };
    public List<TeamMember> CustomerMembers { get; set; } = new()
    {
        new()
    };
    public List<TeamMember> OtherContacts { get; set; } = new();

    // Steg 3: Planering
    public DateOnly? ProjectStart { get; set; }
    public DateOnly? GoLive { get; set; }
    public int BudgetHours { get; set; } = 270;
    public int WeeklyPace { get; set; } = 20;
    public List<Milestone> Milestones { get; set; } = new();

    // Steg 4: Risker
    public List<Risk> Risks { get; set; } = DefaultData.GetDefaultRisks();

    // Steg 5: DevOps-struktur
    public DevOpsStructure DevOpsStructure { get; set; } = DefaultData.GetDefaultDevOpsStructure();

    // DevOps connection
    public string DevOpsOrg { get; set; } = "";
    public string DevOpsProject { get; set; } = "";
    public string DevOpsPat { get; set; } = "";

    // DevOps setup config
    public string ProcessTemplate { get; set; } = "CMMI";
    public int SprintCount { get; set; } = 10;
    public int SprintLengthWeeks { get; set; } = 2;
    public bool SetupCreateProject { get; set; } = true;
    public bool SetupCreateAreas { get; set; } = true;
    public bool SetupCreateIterations { get; set; } = true;
    public bool SetupCreateTeam { get; set; } = true;
    public bool SetupUploadFiles { get; set; } = true;
    public bool SetupCreateWorkItems { get; set; } = true;

    public string AreaPath => $"{CustomerName}\\{TeamName}";
    public string MainTitle => !string.IsNullOrWhiteSpace(ProjectTitle)
        ? ProjectTitle
        : $"Integration {(string.IsNullOrWhiteSpace(SystemA) ? "System A" : SystemA)} - {(string.IsNullOrWhiteSpace(SystemB) ? "System B" : SystemB)}";
    public int EstimatedWeeks => WeeklyPace > 0 ? (int)Math.Ceiling((double)BudgetHours / WeeklyPace) : 0;
}

public class TeamMember
{
    public string Name { get; set; } = "";
    public string Role { get; set; } = "";
    public string Group { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Comment { get; set; } = "";
}

public class Milestone
{
    public string Subproject { get; set; } = "";
    public string Activity { get; set; } = "";
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

public class Risk
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Cause { get; set; } = "";
    public string Consequence { get; set; } = "";
    public string Category { get; set; } = "";
    public int Probability { get; set; } = 1;
    public int Impact { get; set; } = 3;
    public bool Selected { get; set; }
    public string Owner { get; set; } = "Projektledare";
    public string Strategy { get; set; } = "Minska";
    public string Preventive { get; set; } = "";
    public string Fallback { get; set; } = "";
    public string Trigger { get; set; } = "";

    public int RiskValue => Probability * Impact;
    public string Priority => RiskValue >= 10 ? "Hög" : RiskValue >= 6 ? "Medel" : "Låg";
    public string PriorityClass => RiskValue >= 10 ? "high" : RiskValue >= 6 ? "medium" : "low";
}

public class DevOpsStructure
{
    public List<Epic> Epics { get; set; } = new();
}

public class Epic
{
    public string Title { get; set; } = "";
    public List<Feature> Features { get; set; } = new();
}

public class Feature
{
    public string Title { get; set; } = "";
    public int Effort { get; set; }
    public List<Requirement> Requirements { get; set; } = new();
}

public class Requirement
{
    public string Title { get; set; } = "";
    public int Estimate { get; set; } = 8;
    public List<DevOpsTask> Tasks { get; set; } = new();
}

public class DevOpsTask
{
    public string Title { get; set; } = "";
}
