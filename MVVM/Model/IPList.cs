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

namespace PotatoWall.MVVM.Model;

// <copyright file="IPList.cs" company="POQDavid">
// Copyright (c) POQDavid. All rights reserved.
// </copyright>
// <author>POQDavid</author>
// <summary>This is the IPList class.</summary>

public class BaseIPData : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    internal string defaultIP = "";

    internal string defaultPort = "";

    private string defaultDirection = "N/A";

    private Dictionary<string, HashSet<string>> defaultIPPorts = new();

    public BaseIPData()
    {
        IP = defaultIP;
        Port = defaultPort;
    }

    public BaseIPData(string ip, string port)
    {
        IP = ip;

        Port = port;
    }

    public BaseIPData(string ip, string port, string direction)
    {
        IP = ip;

        Port = port;
        Direction = direction;
    }

    [JsonPropertyName("IP")]
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

    [JsonPropertyName("Port")]
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

    [JsonPropertyName("Direction")]
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

    [JsonIgnore]
    [JsonPropertyName("IPPort")]
    public string IPPort => $"{IP}:{Port}";

    [JsonPropertyName("IPPorts")]
    public Dictionary<string, HashSet<string>> IPPorts
    { get => defaultIPPorts; set { defaultIPPorts = value; OnPropertyChanged(); } }

    [JsonIgnore]
    [JsonPropertyName("ImageSource")]
    public Uri ImageSource
    {
        get
        {
            string path = Path.Combine(Environment.CurrentDirectory, $"flags/{Country}.png");
            return File.Exists(path) ? new Uri(path) : new Uri(Path.Combine(Environment.CurrentDirectory, $"flags/XX.png"));
        }
    }

    [JsonPropertyName("Country")]
    public string Country => GetCountry();

    [JsonPropertyName("ASN")]
    public string ASN => GetASN();

    public override bool Equals(object obj)
    {
        return obj.GetType() == typeof(string) ? IP == obj.CastTo<string>() : obj is BaseIPData iPData && IP.Equals(iPData.IP, global::System.StringComparison.Ordinal);
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
    internal void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class SrcIPData : BaseIPData, INotifyPropertyChanged
{
    private DstIPData defaultDstIPData = new();

    public SrcIPData()
    {
        IP = base.defaultIP;
        Port = base.defaultPort;
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

    [JsonPropertyName("DstIPData")]
    public DstIPData DstIPData
    { get => defaultDstIPData; set { defaultDstIPData = value; OnPropertyChanged(); } }

    [JsonPropertyName("LastActivity")]
    public DateTime LastActivity { get; private set; } = DateTime.Now;

    public override bool Equals(object obj)
    {
        return obj.GetType() == typeof(string) ? IP == obj.CastTo<string>() : obj is SrcIPData iPData && IP.Equals(iPData.IP, global::System.StringComparison.Ordinal);
    }

    public override int GetHashCode()
    {
        return IP.GetHashCode();
    }
}

public class DstIPData : BaseIPData, INotifyPropertyChanged
{
    public DstIPData()
    {
        IP = base.defaultIP;
        Port = base.defaultPort;
    }

    public DstIPData(string ip, string port)
    {
        IP = ip;
        Port = port;
    }

    public override bool Equals(object obj)
    {
        return obj.GetType() == typeof(string)
            ? IP == obj.CastTo<string>()
            : obj is SrcIPData iPData && IP.Equals(iPData.IP, StringComparison.Ordinal);
    }

    public override int GetHashCode()
    {
        return IP.GetHashCode();
    }
}

public class IPListCompact<T> : ThreadSafeObservableCollection<BaseIPData>
{
    public IPListCompact(Dispatcher dispatcher) : base(dispatcher)
    {
        Dispatcher = dispatcher;
    }

    public IPListCompact() : base()
    {
        Dispatcher = Dispatcher.CurrentDispatcher;
    }

    public void AddOrUpdate(BaseIPData iPData)
    {
        if (AddContains(iPData))
        {
            try
            {
                ModifyIPList = false;
                BaseIPData item = this.FirstOrDefault(i => i.IP == iPData.IP);
                if (item != null)
                {
                    item = iPData;
                }
            }
            catch (Exception ex)
            {
                PotatoWallClient.Logger.Debug(ex, "AddOrUpdate: ");
            }
            finally
            {
                ModifyIPList = true;
            }
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

    public bool AddContains(string ip)
    {
        if (!Contains(ip))
        {
            Add(new BaseIPData(ip, "0000"));
            return false;
        }

        return true;
    }
}

public class IPListCompact<TKey, TValue> : ThreadSafeObservableDictionary<String, BaseIPData>
{
    public IPListCompact() : base()
    {
        _dispatcher = Dispatcher.CurrentDispatcher;
    }

    public IPListCompact(Dispatcher dispatcher) : base(dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public bool Remove(BaseIPData iPData)
    {
        return base.Remove(iPData.IP);
    }

    public bool AddContains(BaseIPData iPData)
    {
        return AddContains(iPData.IP, iPData);
    }

    public void AddOrUpdate(BaseIPData iPData)
    {
        AddOrUpdate(iPData.IP, iPData);
    }
}

public class IPList<T> : ThreadSafeObservableCollection<SrcIPData>
{
    public IPList(Dispatcher dispatcher) : base(dispatcher)
    {
        Dispatcher = dispatcher;
    }

    public IPList() : base()
    {
        Dispatcher = Dispatcher.CurrentDispatcher;
    }

    public void AddOrUpdate(SrcIPData iPData)
    {
        if (AddContains(iPData))
        {
            try
            {
                ModifyIPList = false;
                SrcIPData item = this.FirstOrDefault(i => i.IP == iPData.IP);
                if (item != null)
                {
                    item = iPData;
                }
            }
            catch (Exception ex)
            {
                PotatoWallClient.Logger.Debug(ex, "AddOrUpdate: ");
            }
            finally
            {
                ModifyIPList = true;
            }
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

    public bool AddContains(string ip)
    {
        if (!Contains(ip))
        {
            Add(new SrcIPData(ip, "0000"));
            return false;
        }

        return true;
    }
}

public class IPList<TKey, TValue> : ThreadSafeObservableDictionary<String, SrcIPData>
{
    public IPList() : base()
    {
        _dispatcher = Dispatcher.CurrentDispatcher;
    }

    public IPList(Dispatcher dispatcher) : base(dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public bool Remove(SrcIPData iPData)
    {
        return base.Remove(iPData.IP);
    }

    public bool AddContains(SrcIPData iPData)
    {
        return AddContains(iPData.IP, iPData);
    }

    public void AddOrUpdate(SrcIPData iPData)
    {
        AddOrUpdate(iPData.IP, iPData);
    }
}