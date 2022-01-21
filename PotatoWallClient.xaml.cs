/*
 *      This file is part of PotatoWall distribution (https://github.com/poqdavid/PotatoWall or http://poqdavid.github.io/PotatoWall/).
 *  	Copyright (c) 2021 POQDavid
 *      Copyright (c) contributors
 *
 *      PotatoWall is free software: you can redistribute it and/or modify
 *      it under the terms of the GNU General Public License as published by
 *      the Free Software Foundation, either version 3 of the License, or
 *      (at your option) any later version.
 *
 *      PotatoWall is distributed in the hope that it will be useful,
 *      but WITHOUT ANY WARRANTY; without even the implied warranty of
 *      MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *      GNU General Public License for more details.
 *
 *      You should have received a copy of the GNU General Public License
 *      along with PotatoWall.  If not, see <https://www.gnu.org/licenses/>.
 */

namespace PotatoWall;

// <copyright file="PotatoWallClient.xaml.cs" company="POQDavid">
// Copyright (c) POQDavid. All rights reserved.
// </copyright>
// <author>POQDavid</author>
// <summary>This is the PotatoWallClient class.</summary>
public partial class PotatoWallClient : Application
{
    public static string AppDataPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PotatoWall");
    public static string DataPath { get; set; } = Path.Combine(AppDataPath, @"data");
    public static string LogsPath { get; set; } = Path.Combine(AppDataPath, @"logs");
    public static string ColorDataPath { get; set; } = Path.Combine(AppDataPath, "Colors.data");

    ///<summary>
    /// Gets or sets the dataDir property.
    ///</summary>
    ///<value>Directory for the plugin's data and settings.</value>
    public static string SettingPath { get; set; } = Path.Combine(AppDataPath, "Default.json");

    ///<summary>
    /// Gets or sets the iSettings property.
    ///</summary>
    ///<value>Plugin Settings.</value>
    public static Setting.Settings ISettings { get; set; } = new();

    private static PotatoWallUI iPotatoWallUI;

    private static Logger logger;
    private static TextWriter potatoWriter;

    public static Uri CityDBurl { get; set; } = new Uri("https://" + $"download.db-ip.com/free/dbip-city-lite-{DateTime.Now.Year}-{DateTime.Now.Month:d2}.mmdb.gz");
    public static string CityDBSavePath { get; set; } = Path.Combine(AppDataPath, $@"data\dbip-city-lite-{DateTime.Now.Year}-{DateTime.Now.Month:d2}.mmdb.gz");

    public static Uri ASNDBurl { get; set; } = new Uri("https://" + $"download.db-ip.com/free/dbip-asn-lite-{DateTime.Now.Year}-{DateTime.Now.Month:d2}.mmdb.gz");
    public static string ASNDBSavePath { get; set; } = Path.Combine(AppDataPath, $@"data\dbip-asn-lite-{DateTime.Now.Year}-{DateTime.Now.Month:d2}.mmdb.gz");

    public static string CityDBPath { get; set; } = Path.Combine(AppDataPath, @"data\city.mmdb");
    public static string ASNDBPath { get; set; } = Path.Combine(AppDataPath, @"data\asn.mmdb");
    public static Reader CityMMDBReader { get; set; }
    public static Reader ASNMMDBReader { get; set; }
    public static Logger Logger { get => logger; set => logger = value; }
    public static TextWriter PotatoWriter { get => potatoWriter; set => potatoWriter = value; }

    public void InitializeComponent()
    {
        logger.Information("Initializing resources for Client");
        Uri resourceLocater = new("/PotatoWall;V1.5.1.0;component/PotatoWallclient.xaml", UriKind.Relative);
        LoadComponent(this, resourceLocater);

        if (Directory.Exists(AppDataPath))
        {
            logger.Information("AppData Path: {AppDataPath}", AppDataPath);
        }
        else
        {
            logger.Information("Creating AppData folder {AppDataPath}", AppDataPath); Directory.CreateDirectory(AppDataPath);
        }

        if (Directory.Exists(DataPath))
        {
            logger.Information("Data Path: {DataPath}", DataPath);
        }
        else
        {
            logger.Information("Creating Data folder {DataPath}", DataPath); Directory.CreateDirectory(DataPath);
        }

        if (Directory.Exists(LogsPath))
        {
            logger.Information("Logs Path: {LogsPath}", LogsPath);
        }
        else
        {
            Directory.CreateDirectory(LogsPath); logger.Information("Creating Logs folder {LogsPath}", LogsPath);
        }

        if (File.Exists(ASNDBPath))
        {
            logger.Information("Loading {ASNDBPath} database.", ASNDBPath); ASNMMDBReader = new(ASNDBPath);
        }
        else
        {
            logger.Information("{ASNDBPath} database does not Exist.", ASNDBPath);
        }

        if (File.Exists(CityDBPath))
        {
            logger.Information("Loading {CityDBPath} database.", CityDBPath); CityMMDBReader = new(CityDBPath);
        }
        else
        {
            logger.Information("{CityDBPath} database does not Exist.", CityDBPath);
        }

        logger.Information("Initialized resources for Client");

        iPotatoWallUI = new PotatoWallUI();
        iPotatoWallUI.Show();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

        FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

        base.OnStartup(e);
    }

    /// <summary>
    /// Application Entry Point.
    /// </summary>
    [STAThread()]
    public static void Main()
    {
        Thread.CurrentThread.Name = "MainThread";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        PotatoWallClient app = new();

        app.InitializeComponent();
        _ = app.Run();
    }

    public PotatoWallClient()
    {
        ISettings.LoadSettings();
        logger.Information("Settings loaded");
    }
}

[Flags]
public enum LogLevel
{
    Info,
    Error,
    Trace
}