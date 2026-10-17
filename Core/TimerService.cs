using System.Diagnostics;
using System.Windows.Threading;

namespace ProjectTimer.Core;

public sealed class TimerService
{
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly DispatcherTimer _ticker;

    public TimerService()
    {
        _ticker = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(33)
        };
        _ticker.Tick += (_, _) => Push();
        _ticker.Start();
    }

    public List<Project> Projects { get; } = [];

    public Project? Active { get; private set; }

    public TimeSpan Total
    {
        get
        {
            TimeSpan sum = TimeSpan.Zero;
            foreach (Project p in Projects)
                sum += p.Total;
            return sum;
        }
    }

    public TimeSpan Now => _clock.Elapsed;

    public void Start(Project project)
    {
        if (project.IsRunning)
            return;

        project.Resume(Now);
        Active = project;
        Push();
    }

    public void Pause(Project project)
    {
        project.Hold(Now);

        if (ReferenceEquals(Active, project))
            Active = null;
    }

    public void Toggle(Project project)
    {
        if (project.IsRunning)
            Pause(project);
        else
            Start(project);
    }

    public void Remove(Project project)
    {
        if (project.IsRunning)
            Pause(project);

        Projects.Remove(project);

        if (ReferenceEquals(Active, project))
            Active = null;
    }

    public void Push()
    {
        TimeSpan now = _clock.Elapsed;
        foreach (Project p in Projects)
            p.Refresh(now);
    }

    public void Stop() => _ticker.Stop();
}