using MudBlazor;

namespace RevestAdmin.Theme;

public static class RevestTheme
{
    public static readonly MudTheme Theme = new()
    {
        Palette = new Palette
        {
            // Primary blue — oklch(0.5 0.134 242.749)
            Primary = "#1565C0",
            PrimaryContrastText = "#F0F7FF",

            // Background — oklch(0.984 0.003 247)
            Background = "#F7F8FA",

            // Surface (cards, table) — oklch(1 0 0)
            Surface = "#FFFFFF",

            // AppBar (used for mobile only)
            AppbarBackground = "#FAFBFC",
            AppbarText = "#1A1A1A",

            // Drawer = sidebar — oklch(0.99 0.002 247)
            DrawerBackground = "#FAFBFC",
            DrawerText = "#1A1A1A",
            DrawerIcon = "#4B5563",

            // Status colours
            Success = "#2E7D32",   // oklch(0.527 0.154 150)
            Warning = "#B45309",   // oklch(0.636 0.168 70) — amber
            Error = "#C62828",   // oklch(0.577 0.245 27)
            Info = "#1565C0",

            // Text
            TextPrimary = "#1A1A1A",   // oklch(0.145 0 0)
            TextSecondary = "#6B7280",   // oklch(0.556 0 0) muted-foreground
            TextDisabled = "#9CA3AF",

            // Lines & inputs
            ActionDefault = "#6B7280",
            LinesDefault = "#E5E8ED",   // oklch(0.912 0.006 247) --border
            LinesInputs = "#E5E8ED",
            TableLines = "#E5E8ED",
            TableHover = "#F3F4F6",   // muted/30

            // Overline
            Divider = "#E5E8ED",
            DividerLight = "#F3F4F6",

            OverlayDark = "rgba(0,0,0,0.5)",
        },

        Typography = new Typography
        {
            Default = new Default
            {
                FontFamily = ["Figtree", "Inter", "sans-serif"],
                FontSize = "0.875rem",
                FontWeight = 400,
                LineHeight = 1.5,
            },
            H1 = new H1 { FontFamily = ["Inter", "sans-serif"], FontSize = "1.875rem", FontWeight = 600 },
            H2 = new H2 { FontFamily = ["Inter", "sans-serif"], FontSize = "1.5rem", FontWeight = 600 },
            H3 = new H3 { FontFamily = ["Inter", "sans-serif"], FontSize = "1.25rem", FontWeight = 600 },
            Button = new Button { FontSize = "0.875rem", FontWeight = 500, TextTransform = "none" },
        },

        LayoutProperties = new LayoutProperties
        {
            DrawerWidthLeft = "256px",
            DrawerMiniWidthLeft = "56px",
            AppbarHeight = "56px",
            DefaultBorderRadius = "8px",
        },
    };
}
