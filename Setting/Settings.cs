/*
 *      This file is part of SAPPRemote distribution (https://github.com/poqdavid/SAPPRemote or http://poqdavid.github.io/SAPPRemote/).
 *  	Copyright (c) 2023 POQDavid
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

using System.Runtime.Serialization;

namespace PotatoWall.Setting;

// <copyright file="Settings.cs" company="POQDavid">
// Copyright (c) POQDavid. All rights reserved.
// </copyright>
// <author>POQDavid</author>
// <summary>This is the settings class.</summary>
public class Settings : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    [JsonPropertyName("GUI")]
    public GUI GUI { get; set; } = new(new XTheme(5));

    [JsonPropertyName("GeoIP")]
    public GeoIP GeoIP { get; set; } = new();

    [JsonPropertyName("WinDivert")]
    public WinDivert WinDivert { get; set; } = new();

    [JsonPropertyName("Firewall")]
    public Firewall Firewall { get; set; } = new();

    /// <summary>
    /// Saves the App settings in selected path.
    /// </summary>
    public static void SaveSetting()
    {
        if (!Directory.Exists(PotatoWallClient.AppDataPath))
        {
            _ = Directory.CreateDirectory(PotatoWallClient.AppDataPath);
        }

        JsonSerializerOptions options = new()
        {
            WriteIndented = true,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };

        File.WriteAllText(PotatoWallClient.SettingPath, JsonSerializer.Serialize(PotatoWallUI.ISettings, options));
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
                JsonSerializerOptions options = new()
                {
                    WriteIndented = true,
                    Converters =
                    {
                        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                    }
                };

                PotatoWallUI.ISettings = JsonSerializer.Deserialize<Settings>(json_string, options);
            }
            else
            {
                SaveSetting();
                LoadSettings();
            }
        }
        catch (Exception ex)
        {
            PotatoWallClient.Logger.Error("Error loading settings", ex);
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

    public XTheme()
    {
        Color = defaultColor;
    }

    public XTheme(int color)
    {
        Color = color;
    }

    private int defaultColor = 5;

    [JsonPropertyName("Color")]
    public int Color
    { get => defaultColor; set { defaultColor = value; OnPropertyChanged(); } }

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
    public event PropertyChangedEventHandler PropertyChanged;

    [JsonIgnore]
    public List<string> Modes { get; set; } = new() { "Dark", "Light", "Auto" };

    private string defaultMode = "Dark";
    private XTheme defaultTheme = new(5);

    public GUI()
    {
        XTheme = new XTheme(5);
    }

    public GUI(XTheme theme)
    {
        XTheme = theme;
    }


    [JsonPropertyName("Theme")]
    public XTheme XTheme
    {
        get => defaultTheme;
        set
        {
            defaultTheme = value;
            OnPropertyChanged();
        }
    }

    [JsonPropertyName("Mode")]
    [DefaultValue("Dark")]
    public string Mode
    {
        get => defaultMode;
        set
        {
            defaultMode = value;
            OnPropertyChanged();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class GeoIP
{
    public event PropertyChangedEventHandler PropertyChanged;

    [JsonIgnore]
    public List<string> GeoIPDBProviders { get; set; } = new() { "DBIP", "MaxMind" };

    private string defaultGeoIPDBProvider = "DBIP";
    private string defaultLicenseKEY = "";

    public GeoIP()
    {
        GeoIPDBProvider = defaultGeoIPDBProvider;
        LicenseKEY = defaultLicenseKEY;
    }

    public GeoIP(string geoipdbprovider, string licensekey)
    {
        GeoIPDBProvider = geoipdbprovider;
        LicenseKEY = licensekey;
    }

    [JsonPropertyName("GeoIPDBProvider")]
    [DefaultValue("DBIP")]
    public string GeoIPDBProvider
    {
        get => defaultGeoIPDBProvider;
        set
        {
            defaultGeoIPDBProvider = value;
            OnPropertyChanged();
        }
    }

    [JsonPropertyName("LicenseKEY")]
    [DefaultValue("")]
    public string LicenseKEY
    {
        get => defaultLicenseKEY;
        set
        {
            defaultLicenseKEY = value;
            OnPropertyChanged();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
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
    [JsonPropertyName("Filter")]
    [DefaultValue("(udp.SrcPort == 6672 or udp.DstPort == 6672) and ip")]
    public string Filter
    {
        get => defaultFilter;
        set
        {
            defaultFilter = value; OnPropertyChanged();
        }
    }

    [JsonPropertyName("WinDivertRecvEx")]
    [DefaultValue(true)]
    public bool WinDivertRecvEx
    {
        get => defaultWinDivertRecvEx;
        set
        {
            defaultWinDivertRecvEx = value; OnPropertyChanged();
        }
    }

    [JsonPropertyName("WinDivertRecv")]
    [DefaultValue(false)]
    public bool WinDivertRecv
    {
        get => defaultWinDivertRecv;
        set
        {
            defaultWinDivertRecv = value; OnPropertyChanged();
        }
    }

    [JsonPropertyName("QueueLen")]
    [DefaultValue(16384)]
    public ulong QueueLen
    {
        get => defaultQueueLen;
        set
        {
            defaultQueueLen = value; OnPropertyChanged();
        }
    }

    [JsonPropertyName("QueueTime")]
    [DefaultValue(8000)]
    public ulong QueueTime
    {
        get => defaultQueueTime;
        set
        {
            defaultQueueTime = value; OnPropertyChanged();
        }
    }

    [JsonPropertyName("QueueSize")]
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

public class Firewall : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private string defaultRegEX = @"^(185\.56\.6[4-7]\.\d{1,3})$|^(104\.255\.10[4-7]\.\d{1,3})$|^(192\.81\.24[0-7]\.\d{1,3})$";

    private bool defaultEnableRegEX = false;

    private bool defaultEnablePacketSize = false;

    private int defaultIPActivityTime = 35;

    private int defaultIPSearchDuration = 10;

    private uint[] defaultHeartbeatSizes = new uint[] { 12, 18, 63 };

    private uint[] defaultMatchmakingSizes = new uint[] { 191, 207, 223, 239 };

    private int defaultMaxPacketsMMS = 20;

    public Firewall()
    {
        RegEX = defaultRegEX;
        EnableRegEX = defaultEnableRegEX;
        IPActivityTime = defaultIPActivityTime;
        IPSearchDuration = defaultIPSearchDuration;
        EnablePacketSize = defaultEnablePacketSize;
        HeartbeatSizes = defaultHeartbeatSizes;
        MatchmakingSizes = defaultMatchmakingSizes;
    }

    public Firewall(string regEX, bool enableRegEX, int iPActivityTime, int iPSearchDuration, bool enablePacketSize, uint[] heartbeatSizes, uint[] matchmakingSizes, int maxPacketsMMS)
    {
        RegEX = regEX;
        EnableRegEX = enableRegEX;
        IPActivityTime = iPActivityTime;
        IPSearchDuration = iPSearchDuration;
        EnablePacketSize = enablePacketSize;
        HeartbeatSizes = heartbeatSizes;
        MatchmakingSizes = matchmakingSizes;
        MaxPacketsMMS = maxPacketsMMS;
    }

    [JsonPropertyName("RegEX")]
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

    [JsonPropertyName("EnableRegEX")]
    [DefaultValue(true)]
    public bool EnableRegEX
    {
        get => defaultEnableRegEX;
        set
        {
            defaultEnableRegEX = value;
            OnPropertyChanged();
        }
    }

    [JsonPropertyName("IPActivityTime")]
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

    [JsonPropertyName("IPSearchDuration")]
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

    [JsonPropertyName("EnablePacketSize")]
    [DefaultValue(true)]
    public bool EnablePacketSize
    {
        get => defaultEnablePacketSize;
        set
        {
            defaultEnablePacketSize = value;
            OnPropertyChanged();
        }
    }

    [JsonPropertyName("HeartbeatSizes")]
    [DefaultValue(new uint[] { 12, 18, 63 })]
    public uint[] HeartbeatSizes
    {
        get => defaultHeartbeatSizes;
        set
        {
            defaultHeartbeatSizes = value;
            OnPropertyChanged();
        }
    }

    [JsonPropertyName("MatchmakingSizes")]
    [DefaultValue(new uint[] { 191, 207, 223, 239 })]
    public uint[] MatchmakingSizes
    {
        get => defaultMatchmakingSizes;
        set
        {
            defaultMatchmakingSizes = value;
            OnPropertyChanged();
        }
    }

    [JsonPropertyName("MaxPacketsMMS")]
    [DefaultValue(20)]
    public int MaxPacketsMMS
    {
        get => defaultMaxPacketsMMS;
        set
        {
            defaultMaxPacketsMMS = value;
            OnPropertyChanged();
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