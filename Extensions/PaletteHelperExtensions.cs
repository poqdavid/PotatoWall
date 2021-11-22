namespace PotatoWall.Extensions;

public static class PaletteHelperExtensions
{
    public static void ChangePrimaryColor(this PaletteHelper paletteHelper, Color color)
    {
        ITheme theme = paletteHelper.GetTheme();

        theme.PrimaryLight = new ColorPair(color.Lighten(), theme.PrimaryLight.ForegroundColor);
        theme.PrimaryMid = new ColorPair(color, theme.PrimaryMid.ForegroundColor);
        theme.PrimaryDark = new ColorPair(color.Darken(), theme.PrimaryDark.ForegroundColor);

        paletteHelper.SetTheme(theme);
    }

    public static void ChangeSecondaryColor(this PaletteHelper paletteHelper, Color color)
    {
        ITheme theme = paletteHelper.GetTheme();

        theme.SecondaryLight = new ColorPair(color.Lighten(), theme.SecondaryLight.ForegroundColor);
        theme.SecondaryMid = new ColorPair(color, theme.SecondaryMid.ForegroundColor);
        theme.SecondaryDark = new ColorPair(color.Darken(), theme.SecondaryDark.ForegroundColor);

        paletteHelper.SetTheme(theme);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
    private static void ModifyTheme(this PaletteHelper paletteHelper, bool isDarkTheme)
    {
        ITheme theme = paletteHelper.GetTheme();

        theme.SetBaseTheme(isDarkTheme ? Theme.Dark : Theme.Light);

        paletteHelper.SetTheme(theme);
    }

    public static void ReplacePrimaryColor(this PaletteHelper paletteHelper, Swatch swatch)
    {
        if (paletteHelper is null) { throw new ArgumentNullException(nameof(paletteHelper)); }

        if (swatch == null) { throw new ArgumentNullException(nameof(swatch)); }

        List<Hue> list = swatch.PrimaryHues.ToList();
        Hue light = list[2];
        Hue mid = swatch.ExemplarHue;
        Hue dark = list[7];

        foreach (Hue color in swatch.PrimaryHues)
        {
            ReplaceEntry(color.Name, color.Color);
            ReplaceEntry(color.Name + "Foreground", color.Foreground);
        }

        ReplaceEntry("PrimaryHueLightBrush", new SolidColorBrush(light.Color));
        ReplaceEntry("PrimaryHueLightForegroundBrush", new SolidColorBrush(light.Foreground));
        ReplaceEntry("PrimaryHueMidBrush", new SolidColorBrush(mid.Color));
        ReplaceEntry("PrimaryHueMidForegroundBrush", new SolidColorBrush(mid.Foreground));
        ReplaceEntry("PrimaryHueDarkBrush", new SolidColorBrush(dark.Color));
        ReplaceEntry("PrimaryHueDarkForegroundBrush", new SolidColorBrush(dark.Foreground));

        //mahapps brushes
        ReplaceEntry("HighlightBrush", new SolidColorBrush(dark.Color));
        ReplaceEntry("AccentColorBrush", new SolidColorBrush(list[5].Color));
        ReplaceEntry("AccentColorBrush2", new SolidColorBrush(list[4].Color));
        ReplaceEntry("AccentColorBrush3", new SolidColorBrush(list[3].Color));
        ReplaceEntry("AccentColorBrush4", new SolidColorBrush(list[2].Color));
        ReplaceEntry("WindowTitleColorBrush", new SolidColorBrush(dark.Color));
        ReplaceEntry("AccentSelectedColorBrush", new SolidColorBrush(list[5].Foreground));
        ReplaceEntry("ProgressBrush", new LinearGradientBrush(dark.Color, list[3].Color, 90.0));
        ReplaceEntry("CheckmarkFill", new SolidColorBrush(list[5].Color));
        ReplaceEntry("RightArrowFill", new SolidColorBrush(list[5].Color));
        ReplaceEntry("IdealForegroundColorBrush", new SolidColorBrush(list[5].Foreground));
        ReplaceEntry("IdealForegroundDisabledBrush", new SolidColorBrush(dark.Color) { Opacity = .4 });
    }

    public static void ReplacePrimaryColor(this PaletteHelper paletteHelper, string name)
    {
        if (name == null) { throw new ArgumentNullException(nameof(name)); }

        Swatch swatch = new SwatchesProvider().Swatches.FirstOrDefault(s => string.Compare(s.Name, name, StringComparison.OrdinalIgnoreCase) == 0);

        if (swatch == null) { throw new ArgumentException($"No such swatch '{name}'", nameof(name)); }

        ReplacePrimaryColor(paletteHelper, swatch);
    }

    public static void ReplaceAccentColor(this PaletteHelper paletteHelper, Swatch swatch)
    {
        if (paletteHelper is null) { throw new ArgumentNullException(nameof(paletteHelper)); }

        if (swatch == null) { throw new ArgumentNullException(nameof(swatch)); }

        foreach (Hue color in swatch.AccentHues)
        {
            ReplaceEntry(color.Name, color.Color);
            ReplaceEntry(color.Name + "Foreground", color.Foreground);
        }

        ReplaceEntry("SecondaryAccentBrush", new SolidColorBrush(swatch.AccentExemplarHue.Color));
        ReplaceEntry("SecondaryAccentForegroundBrush", new SolidColorBrush(swatch.AccentExemplarHue.Foreground));
    }

    public static void ReplaceAccentColor(this PaletteHelper paletteHelper, string name)
    {
        if (name == null) { throw new ArgumentNullException(nameof(name)); }

        Swatch swatch = new SwatchesProvider().Swatches.FirstOrDefault(s => string.Compare(s.Name, name, StringComparison.OrdinalIgnoreCase) == 0 && s.IsAccented);

        if (swatch == null) { throw new ArgumentException($"No such accented swatch '{name}'", nameof(name)); }

        ReplaceAccentColor(paletteHelper, swatch);
    }

    /// <summary>
    /// Replaces a certain entry anywhere in the parent dictionary and its merged dictionaries
    /// </summary>
    /// <param name="entryName">The entry to replace</param>
    /// <param name="newValue">The new entry value</param>
    /// <param name="parentDictionary">The root dictionary to start searching at. Null means using Application.Current.Resources</param>
    private static void ReplaceEntry(object entryName, object newValue, ResourceDictionary parentDictionary = null)
    {
        if (parentDictionary == null) { parentDictionary = Application.Current.Resources; }

        if (parentDictionary.Contains(entryName))
        {
            if (parentDictionary[entryName] is SolidColorBrush brush && !brush.IsFrozen)
            {
                ColorAnimation animation = new()
                {
                    From = ((SolidColorBrush)parentDictionary[entryName]).Color,
                    To = ((SolidColorBrush)newValue).Color,
                    Duration = new Duration(TimeSpan.FromMilliseconds(300))
                };
                brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
            }
            else
            {
                parentDictionary[entryName] = newValue; //Set value normally
            }
        }

        foreach (ResourceDictionary dictionary in parentDictionary.MergedDictionaries) { ReplaceEntry(entryName, newValue, dictionary); }
    }
}