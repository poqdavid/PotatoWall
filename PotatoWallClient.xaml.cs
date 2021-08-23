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

using MaxMind.Db;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

namespace PotatoWall
{
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
        public static Config.Settings ISettings { get; set; } = new();

        //private static bool appJustStarted = true;

        private static PotatoWallUI iPotatoWallUI;

        public static readonly PotatoWallLogger logger = new();

        public static Uri CityDBurl { get; set; } = new Uri("https://" + $"download.db-ip.com/free/dbip-city-lite-{DateTime.Now.Year}-{DateTime.Now.Month:d2}.mmdb.gz");
        public static string CityDBSavePath { get; set; } = Path.Combine(AppDataPath, $@"data\dbip-city-lite-{DateTime.Now.Year}-{DateTime.Now.Month:d2}.mmdb.gz");

        public static Uri ASNDBurl { get; set; } = new Uri("https://" + $"download.db-ip.com/free/dbip-asn-lite-{DateTime.Now.Year}-{DateTime.Now.Month:d2}.mmdb.gz");
        public static string ASNDBSavePath { get; set; } = Path.Combine(AppDataPath, $@"data\dbip-asn-lite-{DateTime.Now.Year}-{DateTime.Now.Month:d2}.mmdb.gz");

        public static string CityDBPath { get; set; } = Path.Combine(AppDataPath, @"data\city.mmdb");
        public static string ASNDBPath { get; set; } = Path.Combine(AppDataPath, @"data\asn.mmdb");
        public static Reader CityMMDBReader { get; set; }
        public static Reader ASNMMDBReader { get; set; }

        public void InitializeComponent()
        {
            logger.WriteLog("Initializing resources for Client", LogLevel.Info);
            Uri resourceLocater = new("/PotatoWall;V1.2.0.0;component/PotatoWallclient.xaml", UriKind.Relative);
            LoadComponent(this, resourceLocater);

            if (Directory.Exists(AppDataPath)) { logger.WriteLog($"AppData Path: {AppDataPath}", LogLevel.Info); }
            else { logger.WriteLog($"Creating AppData folder {AppDataPath}", LogLevel.Info); Directory.CreateDirectory(AppDataPath); }

            if (Directory.Exists(DataPath)) { logger.WriteLog($"Data Path: {DataPath}", LogLevel.Info); }
            else { logger.WriteLog($"Creating Data folder {DataPath}", LogLevel.Info); Directory.CreateDirectory(DataPath); }

            if (Directory.Exists(LogsPath)) { logger.WriteLog($"Logs Path: {LogsPath}", LogLevel.Info); }
            else { Directory.CreateDirectory(LogsPath); logger.WriteLog($"Creating Logs folder {LogsPath}", LogLevel.Info); }

            if (File.Exists(ASNDBPath)) { logger.WriteLog($"Loading {ASNDBPath} database.", LogLevel.Info); ASNMMDBReader = new(ASNDBPath); }
            else { logger.WriteLog($"{ASNDBPath} database does not Exist.", LogLevel.Info); }

            if (File.Exists(CityDBPath)) { logger.WriteLog($"Loading {CityDBPath} database.", LogLevel.Info); CityMMDBReader = new(CityDBPath); }
            else { logger.WriteLog($"{CityDBPath} database does not Exist.", LogLevel.Info); }

            logger.WriteLog("Initialized resources for Client", LogLevel.Info);

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
            PotatoWallClient app = new();

            app.InitializeComponent();
            _ = app.Run();
        }

        public PotatoWallClient()
        {
            ISettings.LoadSettings();
            logger.WriteLog("Settings loaded", LogLevel.Info);
        }
    }
}