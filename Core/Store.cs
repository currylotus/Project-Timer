using System.IO;
using System.Text.Json;

namespace ProjectTimer.Core;

public static class Store
{
    private static readonly string StateFolder =
        System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ProjectTimer");

    private static readonly string StateFile = System.IO.Path.Combine(StateFolder, "projects.json");

    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static List<Project> Load(TimerService timer)
    {
        List<Project> loaded = [];

        if (!File.Exists(StateFile))
            return loaded;

        try
        {
            List<Row>? rows =
                JsonSerializer.Deserialize<List<Row>>(File.ReadAllText(StateFile), Options);

            if (rows is null)
                return loaded;

            foreach (Row row in rows)
            {
                Project project = new()
                {
                    Id = string.IsNullOrWhiteSpace(row.Id) ? Guid.NewGuid().ToString("N") : row.Id,
                    Name = string.IsNullOrWhiteSpace(row.Name) ? "Projekt" : row.Name,
                    Base = TimeSpan.FromMilliseconds(Math.Max(0, row.ElapsedMs))
                };

                project.Refresh(TimeSpan.Zero);

                if (row.Running)
                {
                    project.Resume(TimeSpan.Zero);
                    timer.Start(project);
                }

                timer.Projects.Add(project);
                loaded.Add(project);
            }
        }
        catch
        {
            loaded.Clear();
        }

        return loaded;
    }

    public static void Save(TimerService timer)
    {
        try
        {
            Directory.CreateDirectory(StateFolder);

            List<Row> rows = timer.Projects
                .Select(p => new Row
                {
                    Id = p.Id,
                    Name = p.Name,
                    ElapsedMs = (long)p.Base.TotalMilliseconds,
                    Running = p.IsRunning
                })
                .ToList();

            string temp = StateFile + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(rows, Options));
            File.Move(temp, StateFile, true);
        }
        catch
        {
        }
    }

    private sealed class Row
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public long ElapsedMs { get; set; }
        public bool Running { get; set; }
    }
}