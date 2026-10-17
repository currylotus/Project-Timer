using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ProjectTimer.Core;

public static class Backdrop
{
    private const int DwmwaUseImmersiveDarkMode = 20;
    private const int DwmwaWindowCornerPreference = 33;
    private const int DwmwaSystemBackdropType = 38;
    private const int CornerPreferenceRound = 2;
    private const int SystemBackdropTypeTransientWindow = 3;
    private const int AccentEnableAcrylicBlurBehind = 4;
    private const int AccentFlagDrawAllBorders = 2;

    public static void Apply(Window window)
    {
        if (window.Content is FrameworkElement root)
            root.Loaded += (_, _) => Attach(window);
        else
            Attach(window);
    }

    private static void Attach(Window window)
    {
        nint handle = new WindowInteropHelper(window).Handle;

        if (handle == 0)
            return;

        int dark = 1;
        _ = DwmSetWindowAttribute(handle, DwmwaUseImmersiveDarkMode, ref dark, sizeof(int));

        int corner = CornerPreferenceRound;
        _ = DwmSetWindowAttribute(handle, DwmwaWindowCornerPreference, ref corner, sizeof(int));

        int backdrop = SystemBackdropTypeTransientWindow;
        if (DwmSetWindowAttribute(handle, DwmwaSystemBackdropType, ref backdrop, sizeof(int)) != 0)
            EnableAcrylicFallback(handle);
    }

    private static void EnableAcrylicFallback(nint handle)
    {
        AccentPolicy policy = new()
        {
            AccentState = AccentEnableAcrylicBlurBehind,
            AccentFlags = AccentFlagDrawAllBorders,
            GradientColor = unchecked((int)0xCC1A1D24),
            AnimationId = 0
        };

        int size = Marshal.SizeOf<AccentPolicy>();
        nint pointer = Marshal.AllocHGlobal(size);

        try
        {
            Marshal.StructureToPtr(policy, pointer, false);
            WindowCompositionAttributeData data = new()
            {
                Attribute = AccentEnableAcrylicBlurBehind,
                Data = pointer,
                SizeOfData = size
            };
            SetWindowCompositionAttribute(handle, ref data);
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AccentPolicy
    {
        public int AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowCompositionAttributeData
    {
        public int Attribute;
        public nint Data;
        public int SizeOfData;
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(nint hwnd, int attribute, ref int value, int size);

    [DllImport("user32.dll")]
    private static extern int SetWindowCompositionAttribute(nint hwnd, ref WindowCompositionAttributeData data);
}