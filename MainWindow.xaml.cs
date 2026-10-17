using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using ProjectTimer.Core;

namespace ProjectTimer;

public partial class MainWindow : Window
{
    private readonly TimerService _timer = new();
    private readonly ObservableCollection<Project> _projects = [];
    private readonly DispatcherTimer _uiTick = new() { Interval = TimeSpan.FromMilliseconds(250) };
    private readonly DispatcherTimer _saveTick = new() { Interval = TimeSpan.FromSeconds(5) };

    private Color _lampIdle = Color.FromRgb(0xFF, 0x5A, 0x5F);
    private Color _lampActive = Color.FromRgb(0xFF, 0x95, 0x40);
    private bool _lampRunning;
    private bool _breatheRunning;

    public MainWindow()
    {
        InitializeComponent();
        Backdrop.Apply(this);
        Loaded += OnLoaded;
        Closing += (_, _) => Store.Save(_timer);
        _projects.CollectionChanged += OnProjectsChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        AnchorRight();
        ReadPalette();

        foreach (Project project in Store.Load(_timer))
            _projects.Add(project);

        List.ItemsSource = _projects;
        _uiTick.Tick += (_, _) => RefreshFooter();
        _uiTick.Start();
        _saveTick.Tick += (_, _) => Store.Save(_timer);
        _saveTick.Start();

        RefreshFooter();
        SyncActive();
        PlayIntro();
    }

    private void ReadPalette()
    {
        if (FindResource("LampIdle") is SolidColorBrush idle)
            _lampIdle = idle.Color;

        if (FindResource("LampActive") is SolidColorBrush active)
            _lampActive = active.Color;

        Lamp.Fill = new SolidColorBrush(_lampIdle);
    }

    private void AnchorRight()
    {
        Rect area = new(
            SystemParameters.VirtualScreenLeft,
            SystemParameters.VirtualScreenTop,
            SystemParameters.VirtualScreenWidth,
            SystemParameters.VirtualScreenHeight);

        Left = Math.Max(area.Left, area.Right - Width - 28);
        Top = area.Top + 44;
    }

    private void PlayIntro()
    {
        Shell.Opacity = 0;
        ShellScale.ScaleX = 0.94;
        ShellScale.ScaleY = 0.94;
        ShellSlide.Y = 18;

        CubicEase ease = new() { EasingMode = EasingMode.EaseOut };

        Animate(Shell, OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(260))
        {
            EasingFunction = ease
        });
        Animate(ShellScale, ScaleTransform.ScaleXProperty, new DoubleAnimation(0.94, 1, TimeSpan.FromMilliseconds(420))
        {
            EasingFunction = ease
        });
        Animate(ShellScale, ScaleTransform.ScaleYProperty, new DoubleAnimation(0.94, 1, TimeSpan.FromMilliseconds(420))
        {
            EasingFunction = ease
        });
        Animate(ShellSlide, TranslateTransform.YProperty, new DoubleAnimation(18, 0, TimeSpan.FromMilliseconds(460))
        {
            EasingFunction = ease
        });
    }

    private void OnProjectsChanged(object? sender, NotifyCollectionChangedEventArgs e) => RefreshFooter();

    private void RefreshFooter()
    {
        EmptyHint.Visibility = _projects.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        TotalText.Text = Format.Long(_timer.Total);
        CountText.Text = _projects.Count == 0 ? string.Empty
            : _projects.Count == 1 ? "1 Projekt"
            : $"{_projects.Count} Projekte";
    }

    private void SyncActive()
    {
        Project? active = _timer.Active;

        TimerBlock.DataContext = active;
        ActiveName.Visibility = active is null ? Visibility.Collapsed : Visibility.Visible;
        ActiveTime.Visibility = active is null ? Visibility.Collapsed : Visibility.Visible;
        ActiveCenti.Visibility = active is null ? Visibility.Collapsed : Visibility.Visible;
        IdleHint.Visibility = active is null ? Visibility.Visible : Visibility.Collapsed;

        TintLamp(active is not null);

        if (active is null)
        {
            StopLamp();
            Breathe(false);
        }
        else
        {
            StartLamp();
            Breathe(true);
        }
    }

    private void TintLamp(bool active)
    {
        if (Lamp.Fill is not SolidColorBrush brush)
            Lamp.Fill = brush = new SolidColorBrush(_lampIdle);

        Animate(brush, SolidColorBrush.ColorProperty, new ColorAnimation(
            active ? _lampActive : _lampIdle,
            TimeSpan.FromMilliseconds(340))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        });
    }

    private void StartLamp()
    {
        if (_lampRunning)
            return;

        _lampRunning = true;

        Animate(Lamp, OpacityProperty, new DoubleAnimation(1, 0.4, TimeSpan.FromMilliseconds(950))
        {
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        });
    }

    private void StopLamp()
    {
        if (!_lampRunning)
            return;

        _lampRunning = false;
        Lamp.BeginAnimation(OpacityProperty, null);
        Lamp.Opacity = 1;
    }

    private void Breathe(bool on)
    {
        if (on)
        {
            if (_breatheRunning)
                return;

            _breatheRunning = true;

            DoubleAnimation beat = new(1, 1.011, TimeSpan.FromSeconds(2.8))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            Animate(TimerScale, ScaleTransform.ScaleXProperty, beat);
            Animate(TimerScale, ScaleTransform.ScaleYProperty, beat);
            return;
        }

        if (!_breatheRunning)
            return;

        _breatheRunning = false;
        TimerScale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
        TimerScale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
        TimerScale.ScaleX = 1;
        TimerScale.ScaleY = 1;
    }

    private void OnAddClick(object sender, RoutedEventArgs e) => AddProject();

    private void OnNameKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            AddProject();
    }

    private void AddProject()
    {
        string name = NameBox.Text.Trim();

        if (name.Length == 0)
        {
            Pulse(NameBox);
            return;
        }

        Project project = new() { Name = name };
        _timer.Projects.Add(project);
        _projects.Add(project);
        project.Refresh(_timer.Now);

        NameBox.Clear();
        Store.Save(_timer);
        NameBox.Focus();

        List.UpdateLayout();

        if (List.ItemContainerGenerator.ContainerFromItem(project) is FrameworkElement element)
            element.BringIntoView();
    }

    private void OnToggleClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Project project })
            Toggle(project);
    }

    private void OnCardClick(object sender, MouseButtonEventArgs e)
    {
        if (FindAncestor<Button>(e.OriginalSource as DependencyObject) is not null)
            return;

        if (sender is FrameworkElement { DataContext: Project project })
            Toggle(project);
    }

    private void OnDeleteClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: Project project })
            return;

        if (List.ItemContainerGenerator.ContainerFromItem(project) is FrameworkElement container)
        {
            Animate(container, OpacityProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(170))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            });

            if (container.RenderTransform is ScaleTransform shrink)
            {
                Animate(shrink, ScaleTransform.ScaleXProperty, new DoubleAnimation(0.94, TimeSpan.FromMilliseconds(170))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                });
                Animate(shrink, ScaleTransform.ScaleYProperty, new DoubleAnimation(0.94, TimeSpan.FromMilliseconds(170))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                });
            }

            DoubleAnimation fade = new(1, 0, TimeSpan.FromMilliseconds(170));
            fade.Completed += (_, _) => DropProject(project);
            container.BeginAnimation(OpacityProperty, fade);
            container.IsHitTestVisible = false;
            return;
        }

        DropProject(project);
    }

    private void DropProject(Project project)
    {
        _timer.Remove(project);
        _projects.Remove(project);
        SyncActive();
        Store.Save(_timer);
    }

    private void Toggle(Project project)
    {
        if (project.IsRunning && ReferenceEquals(_timer.Active, project))
            _timer.Pause(project);
        else
            _timer.Start(project);

        SyncActive();
        Store.Save(_timer);
    }

    private void OnNameChanged(object sender, TextChangedEventArgs e)
    {
        Animate(Placeholder, OpacityProperty, new DoubleAnimation(
            NameBox.Text.Length == 0 ? 1 : 0,
            TimeSpan.FromMilliseconds(200))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        });
    }

    private void Pulse(FrameworkElement element)
    {
        element.RenderTransformOrigin = new Point(0.5, 0.5);

        if (element.RenderTransform is not TranslateTransform)
            element.RenderTransform = new TranslateTransform();

        Animate(element.RenderTransform, TranslateTransform.XProperty, new DoubleAnimation(0, 5, TimeSpan.FromMilliseconds(60))
        {
            AutoReverse = true,
            RepeatBehavior = new RepeatBehavior(2),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        });
    }

    private void OnDrag(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Store.Save(_timer);
        Close();
    }

    private static void Animate(UIElement target, DependencyProperty property, AnimationTimeline timeline)
        => target.BeginAnimation(property, timeline);

    private static void Animate(Animatable target, DependencyProperty property, AnimationTimeline timeline)
        => target.BeginAnimation(property, timeline);

    private static T? FindAncestor<T>(DependencyObject? node) where T : DependencyObject
    {
        while (node is not null)
        {
            if (node is T match)
                return match;

            if (node is not Visual)
                return null;

            node = VisualTreeHelper.GetParent(node);
        }

        return null;
    }
}