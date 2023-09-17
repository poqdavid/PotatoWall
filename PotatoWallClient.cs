/*
 *      This file is part of PotatoWall distribution (https://github.com/poqdavid/PotatoWall or http://poqdavid.github.io/PotatoWall/).
 *  	Copyright (c) 2023 POQDavid
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

/// <copyright file="PotatoWallClient.cs" company="POQDavid">
/// Copyright (c) POQDavid. All rights reserved.
/// </copyright>
/// <author>POQDavid</author>
/// <summary>This is the PotatoWallClient class.</summary>
public partial class PotatoWallClient
{
    public static string AppDataPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PotatoWall");
    public static string DataPath { get; set; } = Path.Combine(AppDataPath, @"data");
    public static string LogsPath { get; set; } = Path.Combine(AppDataPath, @"logs");
    public static string ListsPath { get; set; } = Path.Combine(AppDataPath, @"lists");
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

    public static Uri CityDBIPurl { get; set; } = new Uri("https://" + $"download.db-ip.com/free/dbip-city-lite-{DateTime.Now.Year}-{DateTime.Now.Month:d2}.mmdb.gz");
    public static string CityDBIPSavePath { get; set; } = Path.Combine(AppDataPath, $@"data\dbip-city-lite-{DateTime.Now.Year}-{DateTime.Now.Month:d2}.mmdb.gz");

    public static Uri ASNDBIPurl { get; set; } = new Uri("https://" + $"download.db-ip.com/free/dbip-asn-lite-{DateTime.Now.Year}-{DateTime.Now.Month:d2}.mmdb.gz");
    public static string ASNDBIPSavePath { get; set; } = Path.Combine(AppDataPath, $@"data\dbip-asn-lite-{DateTime.Now.Year}-{DateTime.Now.Month:d2}.mmdb.gz");

    public static Uri CityMMurl { get; set; }
    public static Uri CityMMsha256url { get; set; }
    public static string CityMMSavePath { get; set; } = Path.Combine(AppDataPath, $@"data\mm-city-{DateTime.Now.Year}-{DateTime.Now.Month:d2}.tar.gz");

    public static Uri AsnMMurl { get; set; }
    public static Uri AsnMMsha256url { get; set; }
    public static string AsnMMSavePath { get; set; } = Path.Combine(AppDataPath, $@"data\mm-asn-{DateTime.Now.Year}-{DateTime.Now.Month:d2}.tar.gz");

    public static string CityDBIPPath { get; set; } = Path.Combine(AppDataPath, @"data\dbip-city.mmdb");
    public static string ASNDBIPPath { get; set; } = Path.Combine(AppDataPath, @"data\dbip-asn.mmdb");

    public static string CityMMPath { get; set; } = Path.Combine(AppDataPath, @"data\mm-city.mmdb");
    public static string AsnMMPath { get; set; } = Path.Combine(AppDataPath, @"data\mm-asn.mmdb");

    public static Reader CityMMDBReader { get; set; }
    public static Reader ASNMMDBReader { get; set; }

    public static Logger Logger { get => logger; set => logger = value; }
    public static TextWriter PotatoWriter { get => potatoWriter; set => potatoWriter = value; }

    public static string WhiteListPath { get; set; } = Path.Combine(ListsPath, "WhiteList.json");
    public static string BlackListPath { get; set; } = Path.Combine(ListsPath, "BlackList.json");

    public static void InitializeComponent()
    {
        Thread.CurrentThread.Name = "MainThread";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        logger.Information("Initializing resources for Client");

        ISettings.LoadSettings();

        logger.Information("Settings loaded");

        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

        FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

        CityMMurl = new Uri($"https://download.maxmind.com/app/geoip_download?edition_id=GeoLite2-City&license_key={ISettings.GeoIP.LicenseKEY}&suffix=tar.gz");
        CityMMsha256url = new Uri($"https://download.maxmind.com/app/geoip_download?edition_id=GeoLite2-City&license_key={ISettings.GeoIP.LicenseKEY}&suffix=tar.gz.sha256");

        AsnMMurl = new Uri($"https://download.maxmind.com/app/geoip_download?edition_id=GeoLite2-ASN&license_key={ISettings.GeoIP.LicenseKEY}&suffix=tar.gz");
        AsnMMsha256url = new Uri($"https://download.maxmind.com/app/geoip_download?edition_id=GeoLite2-ASN&license_key={ISettings.GeoIP.LicenseKEY}&suffix=tar.gz.sha256");

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

        if (Directory.Exists(ListsPath))
        {
            logger.Information("Lists Path: {ListsPath}", ListsPath);
        }
        else
        {
            Directory.CreateDirectory(ListsPath); logger.Information("Creating Lists folder {ListsPath}", ListsPath);
        }

        LoadGeoIPDBs();

        iPotatoWallUI = new PotatoWallUI();
    }

    public static void LoadGeoIPDBs()
    {
        if (ISettings.GeoIP.GeoIPDBProvider == "DBIP")
        {
            if (File.Exists(ASNDBIPPath))
            {
                logger.Information("Loading {ASNDBPath} database.", ASNDBIPPath); ASNMMDBReader = new(ASNDBIPPath);
            }
            else
            {
                logger.Information("{ASNDBPath} database does not Exist.", ASNDBIPPath);
            }

            if (File.Exists(CityDBIPPath))
            {
                logger.Information("Loading {CityDBPath} database.", CityDBIPPath); CityMMDBReader = new(CityDBIPPath);
            }
            else
            {
                logger.Information("{CityDBPath} database does not Exist.", CityDBIPPath);
            }
        }
        else
        {
            if (File.Exists(AsnMMPath))
            {
                logger.Information("Loading {AsnMMPath} database.", AsnMMPath); ASNMMDBReader = new(AsnMMPath);
            }
            else
            {
                logger.Information("{AsnMMPath} database does not Exist.", AsnMMPath);
            }

            if (File.Exists(CityMMPath))
            {
                logger.Information("Loading {CityDBPath} database.", CityMMPath); CityMMDBReader = new(CityMMPath);
            }
            else
            {
                logger.Information("{CityDBPath} database does not Exist.", CityMMPath);
            }
        }
    }

    public static void Run()
    {
        logger.Information("Initialized resources for Client");

        iPotatoWallUI.Show();
    }
}

[Flags]
public enum LogLevel
{
    Info,
    Error,
    Trace
}