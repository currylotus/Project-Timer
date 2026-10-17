using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProjectTimer.Core;

public sealed class Project : INotifyPropertyChanged
{
    private const string PlayGlyph = "\u25B6";
    private const string PauseGlyph = "\u275A\u275A";

    private string _name = string.Empty;
    private TimeSpan _base;
    private TimeSpan? _since;
    private TimeSpan _total;

    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string Name
    {
        get => _name;
        set => Set(ref _name, value);
    }

    public TimeSpan Base
    {
        get => _base;
        set
        {
            if (Set(ref _base, value))
                Changed(nameof(Total));
        }
    }

    public TimeSpan? Since
    {
        get => _since;
        set
        {
            if (Set(ref _since, value))
            {
                Changed(nameof(IsRunning));
                Changed(nameof(Glyph));
            }
        }
    }

    public bool IsRunning => _since.HasValue;

    public TimeSpan Total
    {
        get => _total;
        private set
        {
            if (Set(ref _total, value))
                Changed(nameof(TimeText));
        }
    }

    public string TimeText => Format.Hms(_total);

    public string CentisText => _total.ToString(@"\.ff");

    public string Glyph => IsRunning ? PauseGlyph : PlayGlyph;

    public void Resume(TimeSpan now)
    {
        if (_since is null)
            Since = now;
    }

    public void Hold(TimeSpan now)
    {
        if (_since is not null)
        {
            _base += now - _since.Value;
            Since = null;
            Changed(nameof(Total));
        }
    }

    public void Refresh(TimeSpan now)
    {
        Total = _since is null ? _base : _base + (now - _since.Value);
        Changed(nameof(IsRunning));
        Changed(nameof(Glyph));
        Changed(nameof(CentisText));
    }

    public void Reset(TimeSpan now)
    {
        _base = TimeSpan.Zero;
        Since = IsRunning ? now : null;
        Refresh(now);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        Changed(name);
        return true;
    }

    private void Changed([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}