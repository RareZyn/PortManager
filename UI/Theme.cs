namespace PortManager.UI;

public static class Theme
{
    // Background gradient
    public static readonly Color BgDark = Color.FromArgb(26, 26, 46);       // #1a1a2e
    public static readonly Color BgLight = Color.FromArgb(22, 33, 62);      // #16213e

    // Card / Panel
    public static readonly Color CardBg = Color.FromArgb(40, 40, 70);
    public static readonly Color CardBgHover = Color.FromArgb(50, 50, 85);
    public static readonly Color CardBorder = Color.FromArgb(60, 60, 100);

    // Glass overlay
    public static readonly Color GlassOverlay = Color.FromArgb(30, 255, 255, 255);
    public static readonly Color GlassBorder = Color.FromArgb(50, 255, 255, 255);

    // Text
    public static readonly Color TextPrimary = Color.FromArgb(240, 240, 255);
    public static readonly Color TextSecondary = Color.FromArgb(160, 165, 190);
    public static readonly Color TextMuted = Color.FromArgb(100, 110, 140);

    // Accent
    public static readonly Color Accent = Color.FromArgb(0, 122, 255);     // #007AFF iOS blue
    public static readonly Color AccentHover = Color.FromArgb(30, 144, 255);

    // Status
    public static readonly Color StatusFree = Color.FromArgb(52, 199, 89);   // iOS green
    public static readonly Color StatusInUse = Color.FromArgb(255, 59, 48);  // iOS red

    // Kill button
    public static readonly Color KillBg = Color.FromArgb(80, 255, 59, 48);
    public static readonly Color KillBgHover = Color.FromArgb(255, 59, 48);
    public static readonly Color KillText = Color.FromArgb(255, 100, 90);
    public static readonly Color KillTextHover = Color.White;

    // Title bar
    public static readonly Color TitleBarBg = Color.FromArgb(18, 18, 38);
    public static readonly Color TitleBarButton = Color.FromArgb(160, 165, 190);
    public static readonly Color TitleBarClose = Color.FromArgb(255, 59, 48);

    // Search
    public static readonly Color SearchBg = Color.FromArgb(35, 35, 60);
    public static readonly Color SearchBorder = Color.FromArgb(55, 55, 90);
    public static readonly Color SearchBorderFocus = Accent;

    // Fonts
    public static readonly Font TitleFont = new("Segoe UI", 14f, FontStyle.Bold);
    public static readonly Font PortFont = new("Segoe UI", 18f, FontStyle.Bold);
    public static readonly Font PortNameFont = new("Segoe UI", 11f, FontStyle.Regular);
    public static readonly Font ProcessFont = new("Segoe UI", 9.5f, FontStyle.Regular);
    public static readonly Font ButtonFont = new("Segoe UI", 9.5f, FontStyle.Bold);
    public static readonly Font SearchFont = new("Segoe UI", 11f, FontStyle.Regular);
    public static readonly Font SmallFont = new("Segoe UI", 8.5f, FontStyle.Regular);

    // Dimensions
    public const int CardHeight = 68;
    public const int CardHeightInUse = 90;
    public const int CardRadius = 12;
    public const int CardSpacing = 8;
    public const int ButtonRadius = 8;
    public const int TitleBarHeight = 42;
    public const int ToolbarHeight = 56;
}
