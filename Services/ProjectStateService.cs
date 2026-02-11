using Projektverktyg.Models;

namespace Projektverktyg.Services;

public class ProjectStateService
{
    public ProjectData Project { get; set; } = new();
    public int CurrentStep { get; set; } = 0;

    public event Action? OnChange;

    public void NotifyStateChanged() => OnChange?.Invoke();

    public void Reset()
    {
        Project = new ProjectData();
        CurrentStep = 0;
        NotifyStateChanged();
    }

    public bool CanGoNext()
    {
        if (CurrentStep == 0)
            return !string.IsNullOrWhiteSpace(Project.CustomerName) &&
                   !string.IsNullOrWhiteSpace(Project.ProjectTitle);
        return true;
    }

    public void NextStep()
    {
        if (CurrentStep < 5 && CanGoNext())
        {
            CurrentStep++;
            NotifyStateChanged();
        }
    }

    public void PreviousStep()
    {
        if (CurrentStep > 0)
        {
            CurrentStep--;
            NotifyStateChanged();
        }
    }

    public void GoToStep(int step)
    {
        if (step >= 0 && step <= 5)
        {
            CurrentStep = step;
            NotifyStateChanged();
        }
    }
}
