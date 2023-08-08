﻿using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IrdLibraryClient;
using Ps3DiscDumper;
using UI.Avalonia.Converters;
using UI.Avalonia.Utils.ColorPalette;

namespace UI.Avalonia.ViewModels;

public partial class ViewModelBase: ObservableObject
{
    public ViewModelBase() => OnAccentColorChanged(accentColor);
    
    [ObservableProperty] private static string tintColor = "#ffffff";
    [ObservableProperty] private static double tintOpacity = 1.0;
    [ObservableProperty] private static double materialOpacity = 0.69;
    [ObservableProperty] private static double luminosityOpacity = 1.0;
    [ObservableProperty] private static string accentColor = ThemeConsts.AccentColor;
    [ObservableProperty] private static string systemAccentColor1 = ThemeConsts.AccentColor;
    [ObservableProperty] private static string systemAccentColor2 = ThemeConsts.AccentColor;
    [ObservableProperty] private static string systemAccentColor3 = ThemeConsts.AccentColor;
    [ObservableProperty] private static bool hasSystemAccentColor = Application.Current?.PlatformSettings is not null;
    [ObservableProperty] private static bool micaEnabled = true;
    [ObservableProperty] private static bool acrylicEnabled = false;
    [ObservableProperty] private static bool enableTransparency = SettingsProvider.Settings.EnableTransparency;
    [ObservableProperty] private static bool preferSystemAccent = SettingsProvider.Settings.PreferSystemAccent;
    [ObservableProperty] private static bool stayOnTop = SettingsProvider.Settings.StayOnTop;
    
    [ObservableProperty] private static string dimTextColor = "#00ff00"; //ThemeConsts.LightThemeDimGray;
    [ObservableProperty] private static IPalette colorPalette = ThemeConsts.Debug;
    [ObservableProperty] private static string layer2BackgroundColor = "#ff0000"; //ThemeConsts.LightThemeLayerHover;
    [ObservableProperty] private static string layer2GroundedColor = "#7f0000"; //ThemeConsts.LightThemeLayerHover;
    [NotifyPropertyChangedFor(nameof(SettingsSymbol))]
    [ObservableProperty] private static FontFamily symbolFontFamily = new("avares://ps3-disc-dumper/Assets/Fonts#Font Awesome 6 Free");
    [ObservableProperty] private static FontFamily largeFontFamily = FontManager.Current.DefaultFontFamily;
    [ObservableProperty] private static FontFamily smallFontFamily = FontManager.Current.DefaultFontFamily;

    private bool UseSegoeIcons => SymbolFontFamily.Name is "Segoe Fluent Icons";
    public string SettingsSymbol => UseSegoeIcons ? "\ue713" : "\uf013";
    public string UpdateSymbol => UseSegoeIcons ? "\ue946" : "\uf05a"; //"\uf06a" // exclamation mark in circle
    public string BackSymbol => UseSegoeIcons ? "\ue72b" : "\uf060";
    public string FolderSymbol => UseSegoeIcons ? "\ue8b7" : "\uf07b";
    public string RenameSymbol => UseSegoeIcons ? "\ue8ac" : "\uf573";
    public string HelpSymbol => UseSegoeIcons ? "\ue8ac" : "\uf573";
    public string HomeSymbol => UseSegoeIcons ? "\ue80f" : "\uf015";
    public string FeedbackSymbol => UseSegoeIcons ? "\ue939" : "\uf086";
    public string TransparencySymbol => UseSegoeIcons ? "\uef20" : "\uf853"; //e8b3
    public string PinSymbol => UseSegoeIcons ? "\ue718" : "\uf08d";
    public string ValidationErrorSymbol => UseSegoeIcons ? "\ue783" : "\uf06a";
    public string ValidationWarningSymbol => UseSegoeIcons ? "\ue7ba" : "\uf071";
    public string DiagnosticsSymbol => UseSegoeIcons ? "\ue9d9" : "\uf478";
    public string ColorSymbol => UseSegoeIcons ? "\ue790" : "\uf53f";

    [ObservableProperty] protected string pageTitle = "PS3 Disc Dumper";// + Dumper.Version;
    [ObservableProperty] private bool canEditSettings = true;
    
    [RelayCommand]
    private static void OpenUrl(string url)
    {
        ProcessStartInfo psi = OperatingSystem.IsWindows()
            ? new() { FileName = url, UseShellExecute = true, }
            : new() { FileName = "open", Arguments = url, };
        psi.CreateNoWindow = true;
        try
        {
            using var _ = Process.Start(psi);
        }
        catch (Exception e)
        {
            Log.Warn(e, "Failed to open web URL");
        }
    }

    partial void OnEnableTransparencyChanged(bool value)
    {
        SettingsProvider.Settings = SettingsProvider.Settings with { EnableTransparency = value };
        /* // seems to be broken atm, only works at the time the window is created
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: Window w })
            RenderOptions.SetTextRenderingMode(w, value ? TextRenderingMode.Antialias : TextRenderingMode.SubpixelAntialias);
        */
    }

    partial void OnAccentColorChanged(string value)
    {
        if (Application.Current is not {} app)
            return;

        var c = ColorConverter.Parse(value);
        app.Resources["SystemAccentColor"] = c;
        app.Resources["SystemAccentColorDark1"] = ChangeColorLuminosity(c, -0.3);
        app.Resources["SystemAccentColorDark2"] = ChangeColorLuminosity(c, -0.5);
        app.Resources["SystemAccentColorDark3"] = ChangeColorLuminosity(c, -0.7);
        app.Resources["SystemAccentColorLight1"] = ChangeColorLuminosity(c, 0.3);
        app.Resources["SystemAccentColorLight2"] = ChangeColorLuminosity(c, 0.5);
        app.Resources["SystemAccentColorLight3"] = ChangeColorLuminosity(c, 0.7);
    }

    partial void OnPreferSystemAccentChanged(bool value)
    {
        SettingsProvider.Settings = SettingsProvider.Settings with { PreferSystemAccent = value };
        AccentColor = value ? SystemAccentColor1 : ThemeConsts.AccentColor;
    }

    partial void OnStayOnTopChanged(bool value)
        => SettingsProvider.Settings = SettingsProvider.Settings with { StayOnTop = value };
    
    static Color ChangeColorLuminosity(Color color, double luminosityFactor)
    {
        var red = (double)color.R;
        var green = (double)color.G;
        var blue = (double)color.B;

        if (luminosityFactor < 0)
        {
            luminosityFactor = 1 + luminosityFactor;
            red *= luminosityFactor;
            green *= luminosityFactor;
            blue *= luminosityFactor;
        }
        else if (luminosityFactor >= 0)
        {
            red = (255 - red) * luminosityFactor + red;
            green = (255 - green) * luminosityFactor + green;
            blue = (255 - blue) * luminosityFactor + blue;
        }

        return new Color(color.A, (byte)red, (byte)green, (byte)blue);
    }
}