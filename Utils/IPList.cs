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

namespace PotatoWall.Utils;

// <copyright file="IPList.cs" company="POQDavid">
// Copyright (c) POQDavid. All rights reserved.
// </copyright>
// <author>POQDavid</author>
// <summary>This is the IPList class.</summary>

public class SrcIPData : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private string defaultIP = "";

    private string defaultPort = "";

    private string defaultDirection = "N/A";

    private Dictionary<string, HashSet<string>> defaultIPPorts = new();

    private DstIPData defaultDstIPData = new();

    public SrcIPData()
    {
        IP = defaultIP;
        Port = defaultPort;
    }

    public SrcIPData(string ip, string port)
    {
        IP = ip;

        Port = port;
    }

    public SrcIPData(string srcip, string srcport, string dstip, string dstport)
    {
        IP = srcip;

        Port = srcport;

        DstIPData.IP = dstip;
        DstIPData.Port = dstport;
    }

    public SrcIPData(string ip, string port, string direction)
    {
        IP = ip;

        Port = port;
        Direction = direction;
    }

    public SrcIPData(string srcip, string srcport, string dstip, string dstport, string direction)
    {
        IP = srcip;

        Port = srcport;

        Direction = direction;

        DstIPData.IP = dstip;
        DstIPData.Port = dstport;
    }

    [JsonProperty("IP")]
    public string IP
    {
        get => defaultIP;
        set
        {
            bool valchanged = false; if (defaultPort != value) { valchanged = true; }
            defaultIP = value;
            LastActivity = DateTime.Now;
            if (valchanged) { OnPropertyChanged(); OnPropertyChanged(nameof(IPPort)); OnPropertyChanged(nameof(ImageSource)); OnPropertyChanged(nameof(ASN)); }
        }
    }

    [JsonProperty("Port")]
    public string Port
    {
        get => defaultPort;
        set
        {
            bool valchanged = false;
            if (IPPorts.ContainsKey(IP))
            {
                _ = IPPorts[IP].Add(value);
            }
            else
            {
                IPPorts.Add(IP, new HashSet<string> { value });
            }

            if (defaultPort != value)
            {
                valchanged = true;
            }

            defaultPort = value;
            if (valchanged) { OnPropertyChanged(); OnPropertyChanged(nameof(IPPort)); }
        }
    }

    [JsonProperty("Direction")]
    public string Direction
    {
        get => defaultDirection;
        set
        {
            bool valchanged = false;
            if (defaultDirection != value) { valchanged = true; }

            defaultDirection = value;
            if (valchanged) { OnPropertyChanged(); OnPropertyChanged(nameof(IPPort)); }
        }
    }

    [JsonProperty("IPPort")]
    public string IPPort => $"{IP}:{Port}";

    [JsonProperty("IPPorts")]
    public Dictionary<string, HashSet<string>> IPPorts
    { get => defaultIPPorts; set { defaultIPPorts = value; OnPropertyChanged(); } }

    [JsonProperty("DstIPData")]
    public DstIPData DstIPData
    { get => defaultDstIPData; set { defaultDstIPData = value; OnPropertyChanged(); } }

    [JsonProperty("ImageSource")]
    public Uri ImageSource
    {
        get
        {
            string path = Path.Combine(Environment.CurrentDirectory, $"flags/{Country}.png");
            return File.Exists(path) ? new Uri(path) : new Uri(Path.Combine(Environment.CurrentDirectory, $"flags/XX.png"));
        }
    }

    [JsonProperty("Country")]
    public string Country => GetCountry();

    [JsonProperty("ASN")]
    public string ASN => GetASN();

    [JsonProperty("LastActivity")]
    public DateTime LastActivity { get; private set; } = DateTime.Now;

    public override bool Equals(object obj)
    {
        return obj.GetType() == typeof(string) ? IP == ((string)obj) : obj is SrcIPData iPData && IP.Equals(iPData.IP, global::System.StringComparison.Ordinal);
    }

    public IPAddress ToIPAddress()
    {
        return IPAddress.Parse(IP);
    }

    public InjectableValues ToInjectables()
    {
        InjectableValues injectables = new();
        injectables.AddValue("ip_address", IP);
        return injectables;
    }

    public override int GetHashCode()
    {
        return IP.GetHashCode();
    }

    public string GetCountry()
    {
        string temp = "XX";

        try
        {
            if (PotatoWallClient.CityMMDBReader != null)
            {
                CityResponse data = PotatoWallClient.CityMMDBReader.Find<CityResponse>(ToIPAddress(), ToInjectables());

                if (data != null)
                {
                    temp = data.Country.IsoCode;
                }
            }

            return temp;
        }
        catch (Exception)
        {
            return temp;
        }
    }

    public string GetASN()
    {
        string temp = "ASN: ERROR";
        string asn = "N/A";
        string asnorg = "N/A";

        try
        {
            if (PotatoWallClient.ASNMMDBReader != null)
            {
                AsnResponse data = PotatoWallClient.ASNMMDBReader.Find<AsnResponse>(ToIPAddress(), ToInjectables());

                if (data != null)
                {
                    if (data.AutonomousSystemNumber is not null and not 0)
                    {
                        asn = $"{data.AutonomousSystemNumber}";
                    }

                    if (data.AutonomousSystemOrganization != null && data.AutonomousSystemOrganization != string.Empty)
                    {
                        asnorg = data.AutonomousSystemOrganization;
                    }
                }

                temp = $"ASN: {asn}, ASNOrg: {asnorg}";
            }

            return temp;
        }
        catch (Exception)
        {
            return temp;
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

public class DstIPData : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private string defaultIP = "";

    private string defaultPort = "";

    private Dictionary<string, HashSet<string>> defaultIPPorts = new();

    public DstIPData()
    {
        IP = defaultIP;
        Port = defaultPort;
    }

    public DstIPData(string ip, string port)
    {
        IP = ip;
        Port = port;
    }

    [JsonProperty("IP")]
    public string IP
    {
        get => defaultIP;
        set
        {
            bool valchanged = false; if (defaultPort != value) { valchanged = true; }
            defaultIP = value;
            if (valchanged) { OnPropertyChanged(); OnPropertyChanged(nameof(IPPort)); OnPropertyChanged(nameof(ImageSource)); OnPropertyChanged(nameof(ASN)); }
        }
    }

    [JsonProperty("Port")]
    public string Port
    {
        get => defaultPort;
        set
        {
            bool valchanged = false;
            if (IPPorts.ContainsKey(IP))
            {
                _ = IPPorts[IP].Add(value);
            }
            else
            {
                IPPorts.Add(IP, new HashSet<string> { value });
            }

            if (defaultPort != value)
            {
                valchanged = true;
            }

            defaultPort = value;
            if (valchanged) { OnPropertyChanged(); OnPropertyChanged(nameof(IPPort)); }
        }
    }

    [JsonProperty("IPPort")]
    public string IPPort => $"{IP}:{Port}";

    [JsonProperty("IPPorts")]
    public Dictionary<string, HashSet<string>> IPPorts
    { get => defaultIPPorts; set { defaultIPPorts = value; OnPropertyChanged(); } }

    [JsonProperty("ImageSource")]
    public Uri ImageSource
    {
        get
        {
            string path = Path.Combine(Environment.CurrentDirectory, $"flags/{Country}.png");
            return File.Exists(path) ? new Uri(path) : new Uri(Path.Combine(Environment.CurrentDirectory, $"flags/XX.png"));
        }
    }

    [JsonProperty("Country")]
    public string Country => GetCountry();

    [JsonProperty("ASN")]
    public string ASN => GetASN();

    public override bool Equals(object obj)
    {
        return obj.GetType() == typeof(string)
            ? IP == ((string)obj)
            : obj is SrcIPData iPData && IP.Equals(iPData.IP, StringComparison.Ordinal);
    }

    public IPAddress ToIPAddress()
    {
        return IPAddress.Parse(IP);
    }

    public InjectableValues ToInjectables()
    {
        InjectableValues injectables = new();
        injectables.AddValue("ip_address", IP);
        return injectables;
    }

    public override int GetHashCode()
    {
        return IP.GetHashCode();
    }

    public string GetCountry()
    {
        string temp = "XX";

        try
        {
            if (PotatoWallClient.CityMMDBReader != null)
            {
                CityResponse data = PotatoWallClient.CityMMDBReader.Find<CityResponse>(ToIPAddress(), ToInjectables());

                if (data != null)
                {
                    temp = data.Country.IsoCode;
                }
            }

            return temp;
        }
        catch (Exception)
        {
            return temp;
        }
    }

    public string GetASN()
    {
        string temp = "ASN: ERROR";
        string asn = "N/A";
        string asnorg = "N/A";

        try
        {
            if (PotatoWallClient.ASNMMDBReader != null)
            {
                AsnResponse data = PotatoWallClient.ASNMMDBReader.Find<AsnResponse>(ToIPAddress(), ToInjectables());

                if (data != null)
                {
                    if (data.AutonomousSystemNumber is not null and not 0)
                    {
                        asn = $"{data.AutonomousSystemNumber}";
                    }

                    if (data.AutonomousSystemOrganization != null && data.AutonomousSystemOrganization != string.Empty)
                    {
                        asnorg = data.AutonomousSystemOrganization;
                    }
                }

                temp = $"ASN: {asn}, ASNOrg: {asnorg}";
            }

            return temp;
        }
        catch (Exception)
        {
            return temp;
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

public class IPList<T> : ObservableCollection<SrcIPData>
{
    private delegate void SetItemCallback(int index, SrcIPData item);

    private delegate void RemoveItemCallback(int index);

    private delegate void ClearItemsCallback();

    private delegate void InsertItemCallback(int index, SrcIPData item);

    private delegate void MoveItemCallback(int oldIndex, int newIndex);

    public Dispatcher Dispatcher { get; }

    public IPList(Dispatcher dispatcher)
    {
        Dispatcher = dispatcher;
    }

    public IPList()
    {
        Dispatcher = Dispatcher.CurrentDispatcher;
    }

    public bool AddContains(SrcIPData iPData)
    {
        if (!base.Contains(iPData))
        {
            Add(iPData);
            return false;
        }

        return true;
    }

    protected override void SetItem(int index, SrcIPData item)
    {
        if (Dispatcher.CheckAccess())
        {
            base.SetItem(index, item);
        }
        else
        {
            _ = Dispatcher.Invoke(DispatcherPriority.Send, new SetItemCallback(SetItem), index, new object[] { item });
        }
    }

    protected override void RemoveItem(int index)
    {
        if (Dispatcher.CheckAccess())
        {
            base.RemoveItem(index);
        }
        else
        {
            _ = Dispatcher.Invoke(DispatcherPriority.Send, new RemoveItemCallback(RemoveItem), index);
        }
    }

    protected override void ClearItems()
    {
        if (Dispatcher.CheckAccess())
        {
            base.ClearItems();
        }
        else
        {
            _ = Dispatcher.Invoke(DispatcherPriority.Send, new ClearItemsCallback(ClearItems));
        }
    }

    protected override void InsertItem(int index, SrcIPData item)
    {
        try
        {
            if (Dispatcher == null)
            {
                base.InsertItem(index, item);
            }
            else if (Dispatcher.CheckAccess())
            {
                base.InsertItem(index, item);
            }
            else
            {
                _ = Dispatcher.Invoke(DispatcherPriority.Send, new InsertItemCallback(InsertItem), index, new object[] { item });
            }
        }
        catch (Exception) { }
    }

    protected override void MoveItem(int oldIndex, int newIndex)
    {
        if (Dispatcher.CheckAccess())
        {
            base.MoveItem(oldIndex, newIndex);
        }
        else
        {
            _ = Dispatcher.Invoke(DispatcherPriority.Send, new MoveItemCallback(MoveItem), oldIndex, new object[] { newIndex });
        }
    }

    public bool Contains(string ip)
    {
        for (int i = 0; i < Count; i++)
        {
            if (Items[i].Equals(ip))
            {
                return true;
            }
        }
        return false;
    }

    public new bool Contains(SrcIPData iPData)
    {
        for (int i = 0; i < Count; i++)
        {
            if (Items[i].Equals(iPData))
            {
                return true;
            }
        }
        return false;
    }
}