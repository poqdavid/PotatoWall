/*
 *      This file is part of SAPPRemote distribution (https://github.com/poqdavid/SAPPRemote or http://poqdavid.github.io/SAPPRemote/).
 *  	Copyright (c) 2016-2020 POQDavid
 *      Copyright (c) contributors
 *
 *      SAPPRemote is free software: you can redistribute it and/or modify
 *      it under the terms of the GNU General Public License as published by
 *      the Free Software Foundation, either version 3 of the License, or
 *      (at your option) any later version.
 *
 *      SAPPRemote is distributed in the hope that it will be useful,
 *      but WITHOUT ANY WARRANTY; without even the implied warranty of
 *      MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *      GNU General Public License for more details.
 *
 *      You should have received a copy of the GNU General Public License
 *      along with SAPPRemote.  If not, see <https://www.gnu.org/licenses/>.
 */

using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace PotatoWall.Config
{
    // <copyright file="Settings.cs" company="POQDavid">
    // Copyright (c) POQDavid. All rights reserved.
    // </copyright>
    // <author>POQDavid</author>
    // <summary>This is the settings class.</summary>
    public class Settings : INotifyPropertyChanged
    {
        private string defaultRegEX = @"^(185\.56\.6[4-7]\.\d{1,3})$|^(104\.255\.10[4-7]\.\d{1,3})$|^(192\.81\.24[0-7]\.\d{1,3})$";

        private bool defaultEnableRegEX;

        private int defaultIPActivityTime = 35;

        private int defaultIPSearchDuration = 10;

        public event PropertyChangedEventHandler PropertyChanged;

        [JsonProperty("RegEX")]
        [DefaultValue(@"^(185\.56\.6[4-7]\.\d{1,3})$|^(104\.255\.10[4-7]\.\d{1,3})$|^(192\.81\.24[0-7]\.\d{1,3})$")]
        public string RegEX
        {
            get => defaultRegEX;
            set
            {
                defaultRegEX = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("EnableRegEX")]
        [DefaultValue(false)]
        public bool EnableRegEX
        {
            get => defaultEnableRegEX;
            set
            {
                defaultEnableRegEX = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("IPActivityTime")]
        [DefaultValue(35)]
        public int IPActivityTime
        {
            get => defaultIPActivityTime;
            set
            {
                defaultIPActivityTime = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("IPSearchDuration")]
        [DefaultValue(10)]
        public int IPSearchDuration
        {
            get => defaultIPSearchDuration;
            set
            {
                defaultIPSearchDuration = value;
                OnPropertyChanged();
            }
        }

        public GUI GUI { get; set; } = new(new XTheme("bluegrey"));

        public WinDivert WinDivert { get; set; } = new();

        /// <summary>
        /// Saves the App settings in selected path.
        /// </summary>
        public static void SaveSetting()
        {
            if (!Directory.Exists(PotatoWallClient.AppDataPath))
            {
                _ = Directory.CreateDirectory(PotatoWallClient.AppDataPath);
            }

            JsonSerializerSettings s = new()
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace // without this, you end up with duplicates.
            };

            File.WriteAllText(PotatoWallClient.SettingPath, JsonConvert.SerializeObject(PotatoWallUI.ISettings, Formatting.Indented, s));
        }

        /// <summary>
        /// Loads the App settings from the selected path.
        /// </summary>
        public void LoadSettings()
        {
            try
            {
                string json_string = File.ReadAllText(PotatoWallClient.SettingPath);
                if (Json.IsValid(json_string))
                {
                    JsonSerializerSettings s = new()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        ObjectCreationHandling = ObjectCreationHandling.Replace // without this, you end up with duplicates.
                    };

                    PotatoWallUI.ISettings = JsonConvert.DeserializeObject<Settings>(json_string, s);
                }
                else
                {
                    SaveSetting();
                    LoadSettings();
                }
            }
            catch (Exception)
            {
                SaveSetting();
                LoadSettings();
            }
        }

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class XTheme : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public XTheme(string color)
        {
            Color = color;
        }

        private string defaultColor = "bluegrey";

        [JsonProperty("Color")]
        public string Color { get => defaultColor; set { defaultColor = value; OnPropertyChanged(); } }

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class GUI
    {
        public GUI(XTheme theme)
        {
            XTheme = theme;
        }

        [JsonProperty("Theme")]
        public XTheme XTheme { get; set; } = new("bluegrey");
    }

    public class WinDivert : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        ///<summary>
        /// Default value for WinDivertFilter.
        ///</summary>
        private string defaultFilter = "(udp.SrcPort == 6672 or udp.DstPort == 6672) and ip";

        private bool defaultWinDivertRecvEx = true;

        private bool defaultWinDivertRecv;

        ///<summary>
        /// Default value for QueueLen.
        ///</summary>
        private ulong defaultQueueLen = 2048;

        ///<summary>
        /// Default value for QueueTime.
        ///</summary>
        private ulong defaultQueueTime = 1000;

        ///<summary>
        /// Default value for QueueSize.
        ///</summary>
        private ulong defaultQueueSize = 4194304;

        public WinDivert()
        {
            Filter = defaultFilter;
            QueueLen = defaultQueueLen;
            QueueTime = defaultQueueTime;
            QueueSize = defaultQueueSize;
        }

        public WinDivert(string filter, ulong queuelen, ulong queuetime, ulong queuesize)
        {
            Filter = filter;
            QueueLen = queuelen;
            QueueTime = queuetime;
            QueueSize = queuesize;
        }

        ///<summary>
        /// Gets or sets the WinDivertFilter property.
        ///</summary>
        ///<value>Filter string.</value>
        [JsonProperty("Filter")]
        [DefaultValue("(udp.SrcPort == 6672 or udp.DstPort == 6672) and ip")]
        public string Filter
        {
            get => defaultFilter;
            set
            {
                defaultFilter = value; OnPropertyChanged();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CA1711: Identifiers should not have incorrect suffix", Justification = "WinDivertRecvEx")]
        [JsonProperty("WinDivertRecvEx")]
        [DefaultValue(true)]
        public bool WinDivertRecvEx
        {
            get => defaultWinDivertRecvEx;
            set
            {
                defaultWinDivertRecvEx = value; OnPropertyChanged();
            }
        }

        [JsonProperty("WinDivertRecv")]
        [DefaultValue(false)]
        public bool WinDivertRecv
        {
            get => defaultWinDivertRecv;
            set
            {
                defaultWinDivertRecv = value; OnPropertyChanged();
            }
        }

        [JsonProperty("QueueLen")]
        [DefaultValue(16384)]
        public ulong QueueLen
        {
            get => defaultQueueLen;
            set
            {
                defaultQueueLen = value; OnPropertyChanged();
            }
        }

        [JsonProperty("QueueTime")]
        [DefaultValue(8000)]
        public ulong QueueTime
        {
            get => defaultQueueTime;
            set
            {
                defaultQueueTime = value; OnPropertyChanged();
            }
        }

        [JsonProperty("QueueSize")]
        [DefaultValue(33554432)]
        public ulong QueueSize
        {
            get => defaultQueueSize;
            set
            {
                defaultQueueSize = value; OnPropertyChanged();
            }
        }

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}