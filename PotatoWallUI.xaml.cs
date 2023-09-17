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

using PotatoWall.MVVM.Model;

namespace PotatoWall;

/// <copyright file="MainWindow.xaml.cs" company="POQDavid">
/// Copyright (c) POQDavid. All rights reserved.
/// </copyright>
/// <author>POQDavid</author>
/// <summary>Interaction logic for MainWindow.xaml</summary>
public partial class PotatoWallUI : Window
{
    ///<summary>
    /// Gets or sets the iSettings property.
    ///</summary>
    ///<value>Plugin Settings.</value>
    public static Setting.Settings ISettings { get => PotatoWallClient.ISettings; set => PotatoWallClient.ISettings = value; }

    private static Brush DefaultFrogroundC = Brushes.White;

    public IPListCompact<BaseIPData> IpWhiteList { get; set; }

    public IPListCompact<BaseIPData> IpBlackList { get; set; }

    public IPListCompact<BaseIPData> IpAutoWhiteList { get; set; }

    public IPList<SrcIPData> ActiveIPList { get; set; }

    private Dictionary<string, Dictionary<int, int>> matchmaking_flow = new();

    private readonly PotatoTimer IPActivityCheck = new();

    private static IntPtr WinDFriesWallHandle = IntPtr.Zero;
    private static volatile bool WinDFriesWallRunning = true;

    private static IntPtr WinDFriesWallMonitorHandle = IntPtr.Zero;
    private static volatile bool WinDFriesWallMonitorRunning = true;

    private static volatile bool ModeBlockAll;
    private static volatile bool ModeWhiteList;
    private static volatile bool ModeBlackList;
    private static volatile bool ModeAuto;
    private static volatile bool AutoAddWhiteList;
    private static volatile bool allowpacket = true;

    private static readonly List<string> currentNICIPAddress = new();

    private readonly Regex RegEX;

    [GeneratedRegex("\\b[A-Fa-f0-9]{64}\\b")]
    private static partial Regex SHA256RegEX();

    [GeneratedRegex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")]
    private static partial Regex IPV4RegEX();

    private readonly List<string> hosts = new() { "http://whatismyip.akamai.com/", "http://checkip.amazonaws.com/", "http://icanhazip.com/" };

    private static string externalIP = "255.255.255.255";

    public string ExternalIP
    { get => externalIP; set { externalIP = value; Label_PublicIP.Dispatcher.InvokeOrExecute(() => { Label_PublicIP.GetBindingExpression(ContentProperty).UpdateTarget(); }); } }

    private static string localIP = "127.0.0.1";

    public string LocalIP
    { get => localIP; set { localIP = value; Label_LocalIP.Dispatcher.InvokeOrExecute(() => { Label_LocalIP.GetBindingExpression(ContentProperty).UpdateTarget(); }); } }

    public static HttpClient HttpClient { get => httpClient; set => httpClient = value; }

    private readonly AddIPViewModel addIPViewModel;
    private readonly SettingsViewModel settingsViewModel;

    private static HttpClient httpClient = new();
    private static PaletteHelper paletteHelper = new();

    public PotatoWallUI()
    {
        Thread.CurrentThread.Name = "PotatoWallUI";

        PotatoWallClient.Logger.Information("Initializing resources for PotatoWallUI");
        PotatoWallClient.Logger.Information("CurrentThread CurrentCulture: {CurrentCulture}", Thread.CurrentThread.CurrentCulture);
        PotatoWallClient.Logger.Information("CurrentThread CurrentUICulture: {CurrentUICulture}", Thread.CurrentThread.CurrentUICulture);

        ProductInfoHeaderValue productInfo = new("PotatoWall", Assembly.GetExecutingAssembly().GetName().Version.ToString());
        ProductInfoHeaderValue moreInfo = new("(+https://poqdavid.github.io/PotatoWall/)");

        httpClient.DefaultRequestHeaders.UserAgent.Add(productInfo);
        httpClient.DefaultRequestHeaders.UserAgent.Add(moreInfo);
        httpClient.Timeout = TimeSpan.FromSeconds(5);

        settingsViewModel = new SettingsViewModel();
        settingsViewModel.IColorData.Load();

        ActiveIPList = new IPList<SrcIPData>(Application.Current.Dispatcher);
        IpWhiteList = new IPListCompact<BaseIPData>(Application.Current.Dispatcher);
        IpBlackList = new IPListCompact<BaseIPData>(Application.Current.Dispatcher);
        IpAutoWhiteList = new IPListCompact<BaseIPData>(Application.Current.Dispatcher);

        IpWhiteList = Json.Read<IPListCompact<BaseIPData>>(PotatoWallClient.WhiteListPath, "[]");
        PotatoWallClient.Logger.Information("WhiteList Loaded");

        IpBlackList = Json.Read<IPListCompact<BaseIPData>>(PotatoWallClient.BlackListPath, "[]");
        PotatoWallClient.Logger.Information("BlackList Loaded");

        InitializeComponent();

        SetGUIMode(ISettings.GUI.Mode);

        SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;

        LocalIP = GetLocalIP();
        if (!currentNICIPAddress.Contains(LocalIP)) { currentNICIPAddress.Add(LocalIP); }

        PotatoWallClient.Logger.Information("Initialized resources for PotatoWallUI");
        RegEX = new Regex(PotatoWallClient.ISettings.Firewall.RegEX);
        PotatoWallClient.ISettings.PropertyChanged += ISettings_PropertyChanged;
        PotatoWallClient.ISettings.GUI.PropertyChanged += GUI_PropertyChanged;
        PotatoWallClient.ISettings.GUI.XTheme.PropertyChanged += ITheme_PropertyChanged;
        PotatoWallClient.ISettings.WinDivert.PropertyChanged += WinDivert_PropertyChanged;
        PotatoWallClient.ISettings.GeoIP.PropertyChanged += GeoIP_PropertyChanged;
        PotatoWallClient.ISettings.Firewall.PropertyChanged += Firewall_PropertyChanged;

        settingsViewModel.SelectionChangedEvent += Theme_P_SelectionChanged;
        addIPViewModel = new AddIPViewModel();
        addIPViewModel.AddIPBlackListEvent += Button_AddIPBlackList_Click;
        addIPViewModel.AddIPWhiteListEvent += Button_AddIPWhiteList_Click;
        addIPViewModel.AddIPAutoWhiteListEvent += Button_AddIPAutoWhiteList_Click;

        IpWhiteList.CollectionChanged += IpWhiteList_CollectionChanged;
        IpBlackList.CollectionChanged += IpBlackList_CollectionChanged;
    }

    private void IpWhiteList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        try
        {
            Json.Write(PotatoWallClient.WhiteListPath, IpWhiteList);
        }
        catch (Exception ex)
        {
            PotatoWallClient.Logger.Error(ex, "Error: Saving Whitelist");
        }
    }

    private void IpBlackList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        try
        {
            Json.Write(PotatoWallClient.BlackListPath, IpBlackList);
        }
        catch (Exception ex)
        {
            PotatoWallClient.Logger.Error(ex, "Error: Saving Blacklist");
        }
    }

    private void Firewall_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        Setting.Settings.SaveSetting();
    }

    private void GUI_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        SetGUIMode(ISettings.GUI.Mode);
        Setting.Settings.SaveSetting();
    }

    private void GeoIP_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        Setting.Settings.SaveSetting();
    }

    private void WinDivert_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        Setting.Settings.SaveSetting();
    }

    private void ITheme_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        Setting.Settings.SaveSetting();
    }

    private void ISettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        Button_RegEX_Icon.Foreground = PotatoWallClient.ISettings.Firewall.EnableRegEX ? Brushes.LimeGreen : DefaultFrogroundC;
        Setting.Settings.SaveSetting();
    }

    private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category == UserPreferenceCategory.General && ISettings.GUI.Mode.ToLower() == "auto")
        {
            ITheme itheme = paletteHelper.GetTheme();
            itheme.SetBaseTheme(GetSystemTheme());
            paletteHelper.SetTheme(itheme);
        }
    }

    private IBaseTheme GetSystemTheme()
    {
        string regPath = "Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize";
        string valName = "AppsUseLightTheme";

        using RegistryKey regKey = Registry.CurrentUser.OpenSubKey(regPath);

        return regKey.GetValue<uint>(valName, 0).ToTheme();
    }

    private void Window_Client_Loaded(object sender, RoutedEventArgs e)
    {
        PotatoWallClient.PotatoWriter = new TextBlockWriter(TextBlock_Console, ScrollViewer_Console);
        Console.SetOut(PotatoWallClient.PotatoWriter);
        SelfLog.Enable(PotatoWallClient.PotatoWriter);

        Button_RegEX_Icon.Foreground = PotatoWallClient.ISettings.Firewall.EnableRegEX ? Brushes.LimeGreen : DefaultFrogroundC;

        DefaultFrogroundC = Button_Setting_Icon.Foreground;

        IPActivityCheck.PotatoTimerEvent += IPActivityCheck_Event;
        IPActivityCheck.Start(TimeSpan.FromSeconds(1));

        PotatoWallClient.Logger.Information("Window Client Loaded");
    }

    private void Window_client_Closing(object sender, CancelEventArgs e)
    {
        StopWinDivert();
    }

    protected static string GetLocalIP()
    {
        try
        {
            using Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            socket.Connect("9.9.9.9", 53);
            IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
            return endPoint.Address.ToString();
        }
        catch (Exception)
        {
            return Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(address => address.AddressFamily == AddressFamily.InterNetwork).First().ToString();
        }
    }

    protected async Task<string> GetExtIPAsync()
    {
        string extip = "0.0.0.0";

        foreach (string hosturl in hosts)
        {
            try
            {
                PotatoWallClient.Logger.Warning("Checking public IP by {hosturl}...", hosturl);

                HttpResponseMessage response = await httpClient.GetAsync(hosturl);

                Thread.CurrentThread.Name = "PotatoWallUI-Enable";

                PotatoWallClient.Logger.Warning("Response status code: {statuscode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    extip = IPV4RegEX().Match(await response.Content.ReadAsStringAsync()).ToString();

                    if (extip != "0.0.0.0")
                    {
                        return extip;
                    }
                }

                Thread.CurrentThread.Name = "PotatoWallUI-Enable";
            }
            catch (WebException webex)
            {
                PotatoWallClient.Logger.Error(webex, "Error: URL: {hosturl}...", hosturl);
                PotatoWallClient.Logger.Information("");
            }
            catch (InvalidOperationException ioex)
            {
                PotatoWallClient.Logger.Error(ioex, "Error: URL: {hosturl}...", hosturl);
                PotatoWallClient.Logger.Information("");
            }
            catch (TaskCanceledException tcex)
            {
                PotatoWallClient.Logger.Error(tcex, "Error: URL: {hosturl}...", hosturl);
                PotatoWallClient.Logger.Information("");
            }
            catch (HttpRequestException httpex)
            {
                PotatoWallClient.Logger.Error(httpex, "Error: URL: {hosturl}...", hosturl);
                PotatoWallClient.Logger.Information("");
            }
            catch (SocketException socketex)
            {
                PotatoWallClient.Logger.Error(socketex, "Error: URL: {hosturl}...", hosturl);
                PotatoWallClient.Logger.Information("");
            }
            catch (AggregateException aex)
            {
                foreach (Exception exx in aex.InnerExceptions)
                {
                    PotatoWallClient.Logger.Error(exx, "Error: URL: {hosturl}...", hosturl);
                    PotatoWallClient.Logger.Information("");
                }
            }
        }

        return extip;
    }

    private void IPActivityCheck_Event(object sender, PotatoTimerEventEventArgs e)
    {
        try
        {
            if (e.TimerState == TimerStates.Running)
            {
                foreach (SrcIPData iPData in ActiveIPList.ToArray())
                {
                    if (DateTime.Now.Subtract(iPData.LastActivity).Seconds >= PotatoWallClient.ISettings.Firewall.IPActivityTime)
                    {
                        _ = ActiveIPList.Remove(iPData);
                    }
                }
            }
        }
        catch (Exception ex) { PotatoWallClient.Logger.Error(ex, ex.Message); }
    }

    private void DialogHost_Button_CLOSE_Click(object sender, RoutedEventArgs e)
    {
        WinDivert_CheckFilter();
    }

    private void Button_Enable_Click(object sender, RoutedEventArgs e)
    {
        if (Button_Enable.IsChecked == true)
        {
            Button_Enable.IsEnabled = false;
            new Thread(() =>
            {
                Thread.CurrentThread.Name = "PotatoWallUI-Enable";

                PotatoWallClient.Logger.Warning("Please wait checking connection...");

                LocalIP = GetLocalIP();

                if (!currentNICIPAddress.Contains(LocalIP)) { currentNICIPAddress.Add(LocalIP); }

                PotatoWallClient.Logger.Information("Local IP: {localip}", LocalIP);
                PotatoWallClient.Logger.Information("");

                ExternalIP = GetExtIPAsync().Result;

                PotatoWallClient.Logger.Information("Public IP: {externalip}", ExternalIP);
                PotatoWallClient.Logger.Information("");

                WinDFriesWallRunning = true;
                WinDivertInit("PotatoFriesWall", ref WinDFriesWallHandle, WinDivertOpenFlags.None);
                RunBackgroundThread(PotatoFriesWall, "PotatoFriesWall", ThreadPriority.Highest);

                WinDFriesWallMonitorRunning = true;
                WinDivertInit("PotatoFriesWallMonitor", ref WinDFriesWallMonitorHandle, WinDivertOpenFlags.Sniff);
                RunBackgroundThread(PotatoFriesWallMonitor, "PotatoFriesWallMonitor", ThreadPriority.Highest);

                PotatoWallClient.Logger.Information("");

                _ = Button_Enable.Dispatcher.BeginInvoke((Action)delegate ()
                {
                    Button_Enable.IsEnabled = true;
                });
            })
            { IsBackground = true }.Start();
        }
        else
        {
            Button_Enable.IsEnabled = false;

            new Thread(() =>
            {
                Thread.CurrentThread.Name = "PotatoWallUI-Disable";

                WinDFriesWallRunning = false;
                if (WinDFriesWallHandle != IntPtr.Zero)
                {
                    _ = WinDivert.WinDivertClose(WinDFriesWallHandle);
                }

                WinDFriesWallMonitorRunning = false;
                if (WinDFriesWallMonitorHandle != IntPtr.Zero)
                {
                    _ = WinDivert.WinDivertClose(WinDFriesWallMonitorHandle);
                }

                StopWinDivert();

                _ = Button_Enable.Dispatcher.BeginInvoke((Action)delegate ()
                {
                    Button_Enable.IsEnabled = true;
                });
            })
            { IsBackground = true }.Start();
        }
    }

    private void Button_BlockAll_Click(object sender, RoutedEventArgs e)
    {
        if (ModeBlockAll)
        {
            ModeBlockAll = false;
            Button_BlockAll_Icon.Foreground = DefaultFrogroundC;
        }
        else
        {
            ModeWhiteList = false;
            ModeBlackList = false;
            ModeAuto = false;
            IpAutoWhiteList.Clear();

            Button_WhiteList_Icon.Foreground = DefaultFrogroundC;
            Button_BlackList_Icon.Foreground = DefaultFrogroundC;
            Button_AutoList_Icon.Foreground = DefaultFrogroundC;

            ModeBlockAll = true;
            Button_BlockAll_Icon.Foreground = Brushes.LimeGreen;
        }
    }

    private void Button_RegEX_Click(object sender, RoutedEventArgs e)
    {
        if (PotatoWallClient.ISettings.Firewall.EnableRegEX)
        {
            PotatoWallClient.ISettings.Firewall.EnableRegEX = false;
            Button_RegEX_Icon.Foreground = DefaultFrogroundC;
        }
        else
        {
            PotatoWallClient.ISettings.Firewall.EnableRegEX = true;
            Button_RegEX_Icon.Foreground = Brushes.LimeGreen;
        }
    }

    private void Button_WhiteList_Click(object sender, RoutedEventArgs e)
    {
        if (ModeWhiteList)
        {
            ModeWhiteList = false;
            Button_WhiteList_Icon.Foreground = DefaultFrogroundC;
        }
        else
        {
            ModeBlockAll = false;
            ModeBlackList = false;
            ModeAuto = false;
            IpAutoWhiteList.Clear();

            Button_BlockAll_Icon.Foreground = DefaultFrogroundC;
            Button_BlackList_Icon.Foreground = DefaultFrogroundC;
            Button_AutoList_Icon.Foreground = DefaultFrogroundC;

            ModeWhiteList = true;
            Button_WhiteList_Icon.Foreground = Brushes.LimeGreen;
        }
    }

    private void Button_BlackList_Click(object sender, RoutedEventArgs e)
    {
        if (ModeBlackList)
        {
            ModeBlackList = false;
            Button_BlackList_Icon.Foreground = DefaultFrogroundC;
        }
        else
        {
            ModeWhiteList = false;
            ModeBlockAll = false;
            ModeAuto = false;

            IpAutoWhiteList.Clear();

            Button_WhiteList_Icon.Foreground = DefaultFrogroundC;
            Button_BlockAll_Icon.Foreground = DefaultFrogroundC;
            Button_AutoList_Icon.Foreground = DefaultFrogroundC;

            ModeBlackList = true;
            Button_BlackList_Icon.Foreground = Brushes.LimeGreen;
        }
    }

    private void Button_AutoList_Click(object sender, RoutedEventArgs e)
    {
        if (ModeAuto)
        {
            ModeAuto = false;
            Button_AutoList_Icon.Foreground = DefaultFrogroundC;
            IpAutoWhiteList.Clear();
        }
        else
        {
            ModeWhiteList = false;
            ModeBlackList = false;
            ModeBlockAll = false;
            Button_WhiteList_Icon.Foreground = DefaultFrogroundC;
            Button_BlackList_Icon.Foreground = DefaultFrogroundC;
            Button_BlockAll_Icon.Foreground = DefaultFrogroundC;

            RunIPScan();

            Button_AutoList_Icon.Foreground = Brushes.LimeGreen;
        }
    }

    private void Button_AutoScanList_Click(object sender, RoutedEventArgs e)
    {
        RunIPScan();
    }

    private void RunIPScan()
    {
        PotatoWallClient.Logger.Warning("Please wait {ipsearchduration} seconds adding IPs...", PotatoWallClient.ISettings.Firewall.IPSearchDuration);
        Button_AutoScanList.IsEnabled = false;

        new Thread(() =>
        {
            AutoAddWhiteList = true;
            for (int i = 1; i < (PotatoWallClient.ISettings.Firewall.IPSearchDuration + 1); i++)
            {
                Thread.Sleep(1000);
                if (i == 10)
                {
                    AutoAddWhiteList = false;
                    Thread.Sleep(1000);

                    ModeAuto = true;
                    _ = Button_AutoScanList.Dispatcher.BeginInvoke((Action)delegate ()
                    {
                        Button_AutoScanList.IsEnabled = true;
                    });

                    PotatoWallClient.Logger.Warning("Done finished adding IPs.");
                }
            }
        })
        { IsBackground = true }.Start();
    }

    private void Theme_P_SelectionChanged(object sender, ComboBoxSelectionChangedEventArgs e)
    {
        if (grid_loading.Visibility == Visibility.Hidden)
        {
            if (e.SelectedItem is ColorDataList list)
            {
                if (list.ColorMetadata == "REC")
                {
                    SetTheme(list.ColorName);
                }
                else if (list.ColorMetadata != "SEP")
                {
                    string cdata = list.ColorMetadata;
                    Color cx = ColorConverter.ConvertFromString(cdata).CastTo<Color>();
                    SetTheme(cx);
                }
            }
        }
    }

    private void SetGUIMode(string mode)
    {
        ITheme itheme = paletteHelper.GetTheme();

        switch (mode.ToLower())
        {
            case "dark":
                itheme.SetBaseTheme(Theme.Dark);
                break;

            case "light":
                itheme.SetBaseTheme(Theme.Light);
                break;

            case "auto":
                itheme.SetBaseTheme(GetSystemTheme());
                break;
        }

        paletteHelper.SetTheme(itheme);
    }

    public static void SetTheme(string cname)
    {
        SwatchesProvider swatchesProvider = new();
        Swatch color = swatchesProvider.Swatches.FirstOrDefault(a => a.Name == cname);
        paletteHelper.ReplacePrimaryColor(color);
    }

    public static void SetTheme(Color c)
    {
        PaletteHelper paletteHelper = new();

        paletteHelper.ChangePrimaryColor(c);
    }

    private void Window_Client_ContentRendered(object sender, EventArgs e)
    {
        PotatoWallClient.Logger.Information("Finished rendering content for PotatoWallUI");
        PotatoWallClient.Logger.Information("");

        if (settingsViewModel.IColorData.ColorDataList[PotatoWallClient.ISettings.GUI.XTheme.Color] is ColorDataList list)
        {
            if (list.ColorMetadata == "REC")
            {
                SetTheme(list.ColorName);
            }
            else if (list.ColorMetadata != "SEP")
            {
                string cdata = list.ColorMetadata;
                Color cx = ColorConverter.ConvertFromString(cdata).CastTo<Color>();
                SetTheme(cx);
            }
        }
        grid_loading.Visibility = Visibility.Hidden;
    }

    private void Button_Setting_Click(object sender, RoutedEventArgs e)
    {
        _ = DialogHost.Show(settingsViewModel, "RootDialog");
    }

    private void Button_AddIP_Click(object sender, RoutedEventArgs e)
    {
        _ = DialogHost.Show(addIPViewModel, "RootDialog");
    }

    private static void WinDivert_CheckFilter()
    {
        uint errorPos = 0;

        if (!WinDivert.WinDivertHelperCheckFilter(PotatoWallClient.ISettings.WinDivert.Filter, WinDivertLayer.Network, out string errorMessage, ref errorPos))
        {
            PotatoWallClient.Logger.Warning("Filter string is invalid at position {errorpos}.\nError Message:\n{errormessage}", errorPos, errorMessage);
        }
    }

    private void PotatoFriesWall()
    {
        WinDivertBuffer packet = new();

        WinDivertAddress addr = new();

        uint readLen = 0;

        IntPtr recvEvent = IntPtr.Zero;

        NativeOverlapped recvOverlapped;

        uint recvAsyncIoLen = 0;

        do
        {
            try
            {
                if (WinDFriesWallRunning)
                {
                    readLen = 0;

                    recvEvent = Kernel32.CreateEventW(IntPtr.Zero, false, false, IntPtr.Zero);

                    string SrcIPAddress = "0.0.0.0";
                    ushort SrcPort = 0;

                    string DstIPAddress = "0.0.0.0";
                    ushort DstPort = 0;

                    allowpacket = true;

                    if (recvEvent == IntPtr.Zero)
                    {
                        PotatoWallClient.Logger.Warning("Failed to initialize receive IO event.");
                        continue;
                    }

                    addr.Reset();

                    if (PotatoWallClient.ISettings.WinDivert.WinDivertRecvEx)
                    {
                        recvAsyncIoLen = 0;
                        recvOverlapped = new NativeOverlapped
                        {
                            EventHandle = recvEvent
                        };

                        unsafe
                        {
                            if (!WinDivert.WinDivertRecvEx(WinDFriesWallHandle, packet, 0, ref addr, ref readLen, &recvOverlapped))
                            {
                                int error = Marshal.GetLastWin32Error();

                                if (error != 997)
                                {
                                    PotatoWallClient.Logger.Warning("Unknown IO error ID {error} while awaiting result.", error);

                                    Debug.Assert(Kernel32.CloseHandle(recvEvent));
                                    continue;
                                }

                                while (Kernel32.WaitForSingleObject(recvEvent, 1000) == (uint)WaitForSingleObjectResult.WaitTimeout) { }

                                if (!Kernel32.GetOverlappedResult(WinDFriesWallHandle, &recvOverlapped, ref recvAsyncIoLen, false))
                                {
                                    PotatoWallClient.Logger.Warning("Failed to get overlapped result.");

                                    Debug.Assert(Kernel32.CloseHandle(recvEvent));
                                    continue;
                                }

                                readLen = recvAsyncIoLen;
                            }
                        }
                    }
                    else if (PotatoWallClient.ISettings.WinDivert.WinDivertRecv)
                    {
                        if (!WinDivert.WinDivertRecv(WinDFriesWallHandle, packet, ref addr, ref readLen))
                        {
                            int error = Marshal.GetLastWin32Error();

                            // 997 == ERROR_IO_PENDING
                            if (error != 997)
                            {
                                PotatoWallClient.Logger.Warning("Unknown IO error ID {error} while awaiting result.");
                                _ = Kernel32.CloseHandle(recvEvent);
                                continue;
                            }

                            while (Kernel32.WaitForSingleObject(recvEvent, 1000) == (uint)WaitForSingleObjectResult.WaitTimeout)
                            {
                            }
                        }
                    }

                    _ = Kernel32.CloseHandle(recvEvent);
                    WinDivertParseResult WD_PR = WinDivert.WinDivertHelperParsePacket(packet, readLen);

                    unsafe
                    {
                        if (WD_PR.IPv4Header != null && WD_PR.UdpHeader != null)
                        {
                            //WriteToConsole($"V4 UDP packet {addr.Direction} from {WD_PR.IPv4Header->SrcAddr}:{WD_PR.UdpHeader->SrcPort.SWPOrder()} to {WD_PR.IPv4Header->DstAddr}:{WD_PR.UdpHeader->DstPort.SWPOrder()}", Brushes.Yellow);

                            SrcIPAddress = $"{WD_PR.IPv4Header->SrcAddr}";
                            SrcPort = WD_PR.UdpHeader->SrcPort;

                            DstIPAddress = $"{WD_PR.IPv4Header->DstAddr}";
                            DstPort = WD_PR.UdpHeader->DstPort;
                        }
                        else if (WD_PR.IPv6Header != null && WD_PR.UdpHeader != null)
                        {
                            SrcIPAddress = $"{WD_PR.IPv6Header->SrcAddr}";
                            SrcPort = WD_PR.UdpHeader->SrcPort;

                            DstIPAddress = $"{WD_PR.IPv6Header->DstAddr}";
                            DstPort = WD_PR.UdpHeader->DstPort;
                        }

                        if (WD_PR.IPv4Header != null && WD_PR.TcpHeader != null)
                        {
                            //WriteToConsole($"V4 TCP packet {addr.Direction} from {WD_PR.IPv4Header->SrcAddr}:{WD_PR.TcpHeader->SrcPort.SWPOrder()} to {WD_PR.IPv4Header->DstAddr}:{WD_PR.TcpHeader->DstPort.SWPOrder()}", Brushes.Yellow);

                            SrcIPAddress = $"{WD_PR.IPv4Header->SrcAddr}";
                            SrcPort = WD_PR.TcpHeader->SrcPort;

                            DstIPAddress = $"{WD_PR.IPv4Header->DstAddr}";
                            DstPort = WD_PR.TcpHeader->DstPort;
                        }
                        else if (WD_PR.IPv6Header != null && WD_PR.TcpHeader != null)
                        {
                            SrcIPAddress = $"{WD_PR.IPv6Header->SrcAddr}";
                            SrcPort = WD_PR.TcpHeader->SrcPort;

                            DstIPAddress = $"{WD_PR.IPv6Header->DstAddr}";
                            DstPort = WD_PR.TcpHeader->DstPort;
                        }
                    }

                    //string ChkIPAddress = SrcIPAddress;
                    string Direction = addr.Direction.ToString();

                    if (ModeBlockAll) { allowpacket = false || ((RegEX.IsMatch(SrcIPAddress) && PotatoWallClient.ISettings.Firewall.EnableRegEX)); }

                    if (ModeBlackList)
                    {
                        allowpacket = !IpBlackList.Contains(SrcIPAddress) || IpBlackList.Count == 0 || IsPacketAllowed(SrcIPAddress, packet, WD_PR.PacketPayloadLength, "BlackList");
                    }

                    if (ModeWhiteList)
                    {
                        allowpacket = IpWhiteList.Contains(SrcIPAddress) || (RegEX.IsMatch(SrcIPAddress) && PotatoWallClient.ISettings.Firewall.EnableRegEX) || IsPacketAllowed(SrcIPAddress, packet, WD_PR.PacketPayloadLength, "WhiteList");
                    }

                    if (ModeAuto)
                    {
                        allowpacket = IpAutoWhiteList.Contains(SrcIPAddress) || (RegEX.IsMatch(SrcIPAddress) && PotatoWallClient.ISettings.Firewall.EnableRegEX) || IsPacketAllowed(SrcIPAddress, packet, WD_PR.PacketPayloadLength, "WhiteList");
                    }

                    if (!ModeBlockAll && !ModeBlackList && !ModeWhiteList && !ModeAuto)
                    {
                        allowpacket = true;
                    }

                    if (SrcIPAddress == LocalIP) { allowpacket = true; }
                    if (SrcIPAddress == ExternalIP) { allowpacket = true; }

                    if (allowpacket)
                    {
                        if (PotatoWallClient.ISettings.WinDivert.WinDivertRecvEx)
                        {
                            if (!WinDivert.WinDivertSendEx(WinDFriesWallHandle, packet, readLen, 0, ref addr))
                            {
                                PotatoWallClient.Logger.Warning("Write Err: {getlastwin32error}", Marshal.GetLastWin32Error());
                            }
                        }
                        else if (PotatoWallClient.ISettings.WinDivert.WinDivertRecv)
                        {
                            if (!WinDivert.WinDivertSend(WinDFriesWallHandle, packet, readLen, ref addr))
                            {
                                PotatoWallClient.Logger.Warning("Write Err: {getlastwin32error}", Marshal.GetLastWin32Error());
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { PotatoWallClient.Logger.Error(ex, ex.Message); }
        }
        while (WinDFriesWallRunning);
    }

    private void PotatoFriesWallMonitor()
    {
        WinDivertBuffer packet = new();

        WinDivertAddress addr = new();

        uint readLen = 0;

        IntPtr recvEvent = IntPtr.Zero;

        NativeOverlapped recvOverlapped;

        uint recvAsyncIoLen = 0;

        do
        {
            try
            {
                if (WinDFriesWallMonitorRunning)
                {
                    readLen = 0;

                    recvEvent = Kernel32.CreateEventW(IntPtr.Zero, false, false, IntPtr.Zero);

                    string SrcIPAddress = "0.0.0.0";
                    ushort SrcPort = 0;

                    string DstIPAddress = "0.0.0.0";
                    ushort DstPort = 0;

                    if (recvEvent == IntPtr.Zero)
                    {
                        PotatoWallClient.Logger.Warning("Failed to initialize receive IO event.");
                        continue;
                    }

                    addr.Reset();

                    if (PotatoWallClient.ISettings.WinDivert.WinDivertRecvEx)
                    {
                        recvAsyncIoLen = 0;
                        recvOverlapped = new NativeOverlapped
                        {
                            EventHandle = recvEvent
                        };

                        unsafe
                        {
                            if (!WinDivert.WinDivertRecvEx(WinDFriesWallMonitorHandle, packet, 0, ref addr, ref readLen, &recvOverlapped))
                            {
                                int error = Marshal.GetLastWin32Error();

                                if (error != 997)
                                {
                                    PotatoWallClient.Logger.Warning("Unknown IO error ID {error} while awaiting result.", error);

                                    Debug.Assert(Kernel32.CloseHandle(recvEvent));
                                    continue;
                                }

                                while (Kernel32.WaitForSingleObject(recvEvent, 1000) == (uint)WaitForSingleObjectResult.WaitTimeout) { }

                                if (!Kernel32.GetOverlappedResult(WinDFriesWallMonitorHandle, &recvOverlapped, ref recvAsyncIoLen, false))
                                {
                                    PotatoWallClient.Logger.Warning("Failed to get overlapped result.");

                                    Debug.Assert(Kernel32.CloseHandle(recvEvent));
                                    continue;
                                }

                                readLen = recvAsyncIoLen;
                            }
                        }
                    }
                    else if (PotatoWallClient.ISettings.WinDivert.WinDivertRecv)
                    {
                        if (!WinDivert.WinDivertRecv(WinDFriesWallMonitorHandle, packet, ref addr, ref readLen))
                        {
                            int error = Marshal.GetLastWin32Error();

                            // 997 == ERROR_IO_PENDING
                            if (error != 997)
                            {
                                PotatoWallClient.Logger.Warning("Unknown IO error ID {error} while awaiting result.", error);
                                _ = Kernel32.CloseHandle(recvEvent);
                                continue;
                            }

                            while (Kernel32.WaitForSingleObject(recvEvent, 1000) == (uint)WaitForSingleObjectResult.WaitTimeout)
                            {
                            }
                        }
                    }

                    _ = Kernel32.CloseHandle(recvEvent);
                    WinDivertParseResult WD_PR = WinDivert.WinDivertHelperParsePacket(packet, readLen);

                    unsafe
                    {
                        if (WD_PR.IPv4Header != null && WD_PR.UdpHeader != null)
                        {
                            //WriteToConsole($"V4 UDP packet {addr.Direction} from {WD_PR.IPv4Header->SrcAddr}:{WD_PR.UdpHeader->SrcPort.SWPOrder()} to {WD_PR.IPv4Header->DstAddr}:{WD_PR.UdpHeader->DstPort.SWPOrder()}", Brushes.Yellow);

                            SrcIPAddress = $"{WD_PR.IPv4Header->SrcAddr}";
                            SrcPort = WD_PR.UdpHeader->SrcPort;

                            DstIPAddress = $"{WD_PR.IPv4Header->DstAddr}";
                            DstPort = WD_PR.UdpHeader->DstPort;
                        }
                        else if (WD_PR.IPv6Header != null && WD_PR.UdpHeader != null)
                        {
                            SrcIPAddress = $"{WD_PR.IPv6Header->SrcAddr}";
                            SrcPort = WD_PR.UdpHeader->SrcPort;

                            DstIPAddress = $"{WD_PR.IPv6Header->DstAddr}";
                            DstPort = WD_PR.UdpHeader->DstPort;
                        }

                        if (WD_PR.IPv4Header != null && WD_PR.TcpHeader != null)
                        {
                            //WriteToConsole($"V4 TCP packet {addr.Direction} from {WD_PR.IPv4Header->SrcAddr}:{WD_PR.TcpHeader->SrcPort.SWPOrder()} to {WD_PR.IPv4Header->DstAddr}:{WD_PR.TcpHeader->DstPort.SWPOrder()}", Brushes.Yellow);

                            SrcIPAddress = $"{WD_PR.IPv4Header->SrcAddr}";
                            SrcPort = WD_PR.TcpHeader->SrcPort;

                            DstIPAddress = $"{WD_PR.IPv4Header->DstAddr}";
                            DstPort = WD_PR.TcpHeader->DstPort;
                        }
                        else if (WD_PR.IPv6Header != null && WD_PR.TcpHeader != null)
                        {
                            SrcIPAddress = $"{WD_PR.IPv6Header->SrcAddr}";
                            SrcPort = WD_PR.TcpHeader->SrcPort;

                            DstIPAddress = $"{WD_PR.IPv6Header->DstAddr}";
                            DstPort = WD_PR.TcpHeader->DstPort;
                        }
                    }

                    string ChkIPAddress = SrcIPAddress;
                    string Direction = addr.Direction.ToString();

                    if (AutoAddWhiteList)
                    {
                        if (!IpAutoWhiteList.Contains(SrcIPAddress))
                        {
                            if (addr.Direction == WinDivertDirection.Inbound)
                            {
                                PotatoWallClient.Logger.Information("Added IP {srcipaddress} to auto white list by scan", SrcIPAddress);
                            }
                            else
                            {
                                PotatoWallClient.Logger.Information("Added IP {srcipaddress} to auto white list by scan", DstIPAddress);
                            }
                        }

                        _ = addr.Direction == WinDivertDirection.Inbound
                            ? IpAutoWhiteList.AddContains(new BaseIPData(SrcIPAddress, $"{SrcPort.SWPOrder()}"))
                            : IpAutoWhiteList.AddContains(new BaseIPData(DstIPAddress, $"{DstPort.SWPOrder()}"));
                    }

                    if (!currentNICIPAddress.Contains(SrcIPAddress))
                    {
                        ActiveIPList.AddOrUpdate(new SrcIPData(SrcIPAddress, $"{SrcPort.SWPOrder()}", DstIPAddress, $"{DstPort.SWPOrder()}", Direction));
                    }
                }
            }
            catch (Exception ex) { PotatoWallClient.Logger.Error(ex, ex.Message); }
        }
        while (WinDFriesWallMonitorRunning);
    }

    private bool IsPacketAllowed(string ip, WinDivertBuffer payload, uint size, string mode)
    {
        switch (mode, PotatoWallClient.ISettings.Firewall.EnablePacketSize)
        {
            case ("WhiteList", true):

                if (payload[4] < 0 || payload[4] > 255 || payload[6] < 0 || payload[6] > 255)
                {
                    // Handle overflow, e.g. throw an exception or return an error code
                    // throw new OverflowException("The values in packet.Raw are outside the range of byte values.");
                    return true;
                }

                int identification = ((int)payload[4] << 8) | payload[6];

                if (PotatoWallClient.ISettings.Firewall.HeartbeatSizes.AsSpan().Contains(size))
                {
                    return true;
                }

                if (PotatoWallClient.ISettings.Firewall.MatchmakingSizes.AsSpan().Contains(size))
                {
                    if (matchmaking_flow.TryGetValue(ip, out Dictionary<int, int> value))
                    {
                        value[identification] = identification + PotatoWallClient.ISettings.Firewall.MaxPacketsMMS;
                    }
                    else
                    {
                        matchmaking_flow[ip] = new Dictionary<int, int>() { { identification, identification + PotatoWallClient.ISettings.Firewall.MaxPacketsMMS } };
                    }
                    return true;
                }

                if (matchmaking_flow[ip].TryGetValue(identification - 1, out int maxPackets))
                {
                    if (identification < maxPackets)
                    {
                        matchmaking_flow[ip].Remove(identification - 1);
                        matchmaking_flow[ip][identification] = maxPackets;
                        return true;
                    }
                }

                return false;

            case ("BlackList", true):
                return PotatoWallClient.ISettings.Firewall.MatchmakingSizes.AsSpan().Contains(size) || PotatoWallClient.ISettings.Firewall.HeartbeatSizes.AsSpan().Contains(size);

            default:
                return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WinDivertInit(string name, ref IntPtr WinDHandle, WinDivertOpenFlags flags)
    {
        PotatoWallClient.Logger.Information("{name} > Starting WinDivertInit()!", name);

        string filter = PotatoWallClient.ISettings.WinDivert.Filter;
        PotatoWallClient.Logger.Information("{name} > WinDivert Filter= \"{filter}\"", name, filter);

        WinDivert_CheckFilter();

        WinDHandle = WinDivert.WinDivertOpen(filter, WinDivertLayer.Network, 0, flags);

        PotatoWallClient.Logger.Information("{name} > WinDivert Handle = {WinDHandle}", name, WinDHandle);

        _ = WinDivert.WinDivertSetParam(WinDHandle, WinDivertParam.QueueLen, PotatoWallClient.ISettings.WinDivert.QueueLen);
        _ = WinDivert.WinDivertSetParam(WinDHandle, WinDivertParam.QueueTime, PotatoWallClient.ISettings.WinDivert.QueueTime);
        _ = WinDivert.WinDivertSetParam(WinDHandle, WinDivertParam.QueueSize, PotatoWallClient.ISettings.WinDivert.QueueSize);

        PotatoWallClient.Logger.Information("{name} > Started WinDivertInit()!", name);
    }

    private static void RunBackgroundThread(ThreadStart start, string thname, ThreadPriority tp)
    {
        PotatoWallClient.Logger.Information("{thname} > Starting background thread!", thname);
        Thread background = new(start)
        {
            IsBackground = true,
            Name = thname,
            Priority = tp
        };

        background.Start();

        PotatoWallClient.Logger.Information("{thname} > Started background thread!", thname);
    }

    private void MenuItem_BlacklistAdd_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _ = IpBlackList.AddContains(listBox_activeiplist.SelectedItem.CastTo<BaseIPData>());
        }
        catch (Exception ex)
        {
            PotatoWallClient.Logger.Error(ex, "Error: Adding to Whitelist");
        }
    }

    private void MenuItem_WhitelistAdd_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _ = IpWhiteList.AddContains(listBox_activeiplist.SelectedItem.CastTo<BaseIPData>());
        }
        catch (Exception ex)
        {
            PotatoWallClient.Logger.Error(ex, "Error: Adding to Whitelist");
        }
    }

    private void MenuItem_AutoWhitelistAdd_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _ = IpAutoWhiteList.AddContains(listBox_activeiplist.SelectedItem.CastTo<BaseIPData>());
        }
        catch (Exception ex)
        {
            PotatoWallClient.Logger.Error(ex, "Error: Adding to AutoWhitelist");
        }
    }

    private void MenuItem_WhitelistRemove_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _ = IpWhiteList.Remove(listBox_whitelist.SelectedItem.CastTo<BaseIPData>());
        }
        catch (Exception ex)
        {
            PotatoWallClient.Logger.Error(ex, "Error: Removing from Whitelist");
        }
    }

    private void MenuItem_BlacklistRemove_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _ = IpBlackList.Remove(listBox_blacklist.SelectedItem.CastTo<BaseIPData>());
        }
        catch (Exception ex)
        {
            PotatoWallClient.Logger.Error(ex, "Error: Removing from Blacklist");
        }
    }

    private void MenuItem_AutoWhitelistRemove_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _ = IpAutoWhiteList.Remove(listBox_autowhitelist.SelectedItem.CastTo<BaseIPData>());
        }
        catch (Exception ex)
        {
            PotatoWallClient.Logger.Error(ex, "Error: Removing from AutoWhitelist");
        }
    }

    private void MenuItem_CopySrcIP_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SrcIPData srcIPData = listBox_activeiplist.SelectedItem.CastTo<SrcIPData>();
            if (srcIPData.IP != null) { Clipboard.SetText(srcIPData.IP); }
        }
        catch (Exception ex) { PotatoWallClient.Logger.Error(ex, ex.Message); }
    }

    private void MenuItem_CopyDstIP_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SrcIPData srcIPData = listBox_activeiplist.SelectedItem.CastTo<SrcIPData>();
            if (srcIPData.DstIPData != null && srcIPData.DstIPData.IP != null) { Clipboard.SetText(srcIPData.IP); }
        }
        catch (Exception ex) { PotatoWallClient.Logger.Error(ex, ex.Message); }
    }

    private void MenuItem_CopyAll_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SrcIPData srcIPData = listBox_activeiplist.SelectedItem.CastTo<SrcIPData>();
            if (srcIPData != null)
            {
                string data = $"Con: {srcIPData.Direction} \n" +
                              $"Src: {srcIPData.Country} {srcIPData.IP}:{srcIPData.Port} {srcIPData.ASN} \n" +
                              $"Dst: {srcIPData.DstIPData.Country} {srcIPData.DstIPData.IP}:{srcIPData.DstIPData.Port} {srcIPData.DstIPData.ASN} \n";
                Clipboard.SetText(data);
            }
        }
        catch (Exception ex) { PotatoWallClient.Logger.Error(ex, ex.Message); }
    }

    private async Task<bool> DownloadFile(Uri url, string fileName, string newFileName, Uri hashfile, string hashtype)
    {
        if (hashtype == null)
        {
            long urlSize = await GetFileSizeAsync(url);
            if (urlSize != 0)
            {
                FileInfo fi = new(fileName);
                if (fi.Exists)
                {
                    if (fi.Length == urlSize)
                    {
                        GZDecompress(fi, newFileName);
                        PotatoWallClient.Logger.Information("");
                        return true;
                    }
                    else
                    {
                        File.Delete(fi.FullName);
                        await WClient(url, fileName, urlSize);
                        GZDecompress(fi, newFileName);
                        PotatoWallClient.Logger.Information("");
                        return true;
                    }
                }
                else
                {
                    await WClient(url, fileName, urlSize);
                    GZDecompress(fi, newFileName);
                    PotatoWallClient.Logger.Information("");
                    return true;
                }
            }
        }
        else
        {
            string urlhash = await CheckSum(hashfile, hashtype);
            FileInfo fi = new(fileName);

            if (fi.Exists)
            {
                string filehash = await CheckSum(fileName, hashtype);

                if (filehash.Equals(urlhash))
                {
                    PotatoWallClient.Logger.Information("File Hash Matched!!!");
                    GZDecompress(fi, newFileName);
                    PotatoWallClient.Logger.Information("");
                    return true;
                }
                else
                {
                    PotatoWallClient.Logger.Information("File Hash Didn't Matched!!!");
                    double urlSize = await GetFileSizeAsync(url);
                    File.Delete(fi.FullName);
                    await WClient(url, fileName, urlSize);
                    GZDecompress(fi, newFileName);
                    PotatoWallClient.Logger.Information("");
                    return true;
                }
            }
            else
            {
                double urlSize = await GetFileSizeAsync(url);
                await WClient(url, fileName, urlSize);
                GZDecompress(fi, newFileName);
                PotatoWallClient.Logger.Information("");
                return true;
            }
        }

        return true;
    }

    private async Task<bool> WClient(Uri url, string fileName, double totalSize)
    {
        PotatoWallClient.Logger.Information("Downloading {url}...", url);
        PotatoWallClient.Logger.Warning("File Size: {totalsize}MB", GetMB(totalSize));

        using (HttpResponseMessage HttpRM = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
        {
            using var stream = await HttpRM.Content.ReadAsStreamAsync();
            double totalBytesRead = 0L;
            var buffer = new byte[4096];
            bool readData = true;

            using var fStream = new FileStream(fileName, FileMode.Create);
            {
                do
                {
                    int bytesRead = await stream.ReadAsync(buffer);
                    if (bytesRead == 0)
                    {
                        readData = false;
                        DownloadProgress(Math.Round(totalBytesRead / totalSize * 100, 2));
                        continue;
                    }

                    await fStream.WriteAsync(buffer.AsMemory(0, bytesRead));

                    totalBytesRead += bytesRead;

                    DownloadProgress(Math.Round(totalBytesRead / totalSize * 100, 2));
                }
                while (readData);
            }
        }

        PotatoWallClient.Logger.Information("Finished Downloading.");
        return true;
    }

    private static async Task<string> WClient(Uri url)
    {
        string output = string.Empty;

        using (HttpResponseMessage HttpRM = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
        {
            using var stream = await HttpRM.Content.ReadAsStreamAsync();
            var buffer = new byte[4096];

            using var reader = new StreamReader(stream);
            output = reader.ReadToEnd();
        }

        return output;
    }

    private static async Task<long> GetFileSizeAsync(Uri url)
    {
        try
        {
            HttpResponseMessage hrm = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

            hrm.EnsureSuccessStatusCode();

            return Convert.ToInt64(hrm.Content.Headers.ContentLength, Thread.CurrentThread.CurrentCulture);
        }
        catch (Exception ex)
        {
            PotatoWallClient.Logger.Error(ex, ex.Message);

            return 0;
        }
    }

    public static void GZDecompress(FileInfo fileToDecompress, string newFileName)
    {
        PotatoWallClient.Logger.Warning("Decompressing {filetodecompress}...", fileToDecompress);

        using FileStream originalFileStream = fileToDecompress.OpenRead();

        string currentFileName = fileToDecompress.FullName;

        if (newFileName.Contains(@"asn.mmdb")) { PotatoWallClient.Logger.Information("Starting to reload {newFileName} database.", newFileName); PotatoWallClient.ASNMMDBReader?.Dispose(); }
        if (newFileName.Contains(@"city.mmdb")) { PotatoWallClient.Logger.Information("Starting to reload {newFileName} database.", newFileName); PotatoWallClient.CityMMDBReader?.Dispose(); }

        if (!fileToDecompress.FullName.Contains(".tar.gz"))
        {
            using FileStream decompressedFileStream = File.Create(newFileName);
            using GZipStream decompressionStream = new(originalFileStream, CompressionMode.Decompress);
            decompressionStream.CopyTo(decompressedFileStream);
        }
        else
        {
            using GZipStream decompressionStream = new(originalFileStream, CompressionMode.Decompress);
            using TarReader reader = new(decompressionStream, leaveOpen: true);
            TarEntry entry;

            while ((entry = reader.GetNextEntry(copyData: true)) != null)
            {
                if (entry.Name.Contains(".mmdb"))
                {
                    entry.ExtractToFile(newFileName, true);
                }
            }
        }
        PotatoWallClient.Logger.Warning("Finished Decompressing.");

        if (newFileName.Contains(@"asn.mmdb")) { PotatoWallClient.Logger.Information("Loading {newFileName} database.", newFileName); PotatoWallClient.ASNMMDBReader = new(newFileName); }
        if (newFileName.Contains(@"city.mmdb")) { PotatoWallClient.Logger.Information("Loading {newFileName} database.", newFileName); PotatoWallClient.CityMMDBReader = new(newFileName); }
    }

    private void DownloadProgress(double ProgressPercentage)
    {
        if (ProgressPercentage == 100) { Button_DownloadDataBase_Text.Dispatcher.InvokeOrExecute(() => { Button_DownloadDataBase_Text.Text = "Download Database"; }); }
        else { Button_DownloadDataBase_Text.Dispatcher.InvokeOrExecute(() => { Button_DownloadDataBase_Text.Text = $"Downloading Database %{ProgressPercentage}"; }); }
    }

    private static async Task<string> CheckSum(Uri uriPath, string hashtype)
    {
        return hashtype.RemoveWhitespace(true) switch
        {
            "sha256" => SHA256RegEX().Match(await WClient(uriPath)).Value,
            _ => throw new NotImplementedException($"Hash type {hashtype} is not supported"),
        };
    }

    private static async Task<string> CheckSum(string filePath, string hashtype)
    {
        switch (hashtype.RemoveWhitespace(true))
        {
            case "sha256":
                using (SHA256 SHA256 = SHA256.Create())
                {
                    using FileStream fileStream = File.OpenRead(filePath);
                    return Convert.ToHexString(await SHA256.ComputeHashAsync(fileStream)).ToLower();
                }
            default:
                throw new NotImplementedException($"Hash type {hashtype} is not supported");
        };
    }

    private void Button_DownloadDataBase_Click(object sender, RoutedEventArgs e)
    {
        Button_DownloadDataBase.IsEnabled = false;
        new Thread(async () =>
        {
            try
            {
                if (PotatoWallClient.ISettings.GeoIP.GeoIPDBProvider.Equals("DBIP"))
                {
                    await DownloadFile(PotatoWallClient.ASNDBIPurl, PotatoWallClient.ASNDBIPSavePath, PotatoWallClient.ASNDBIPPath, null, null);
                    await DownloadFile(PotatoWallClient.CityDBIPurl, PotatoWallClient.CityDBIPSavePath, PotatoWallClient.CityDBIPPath, null, null);
                }
                else
                {
                    await DownloadFile(PotatoWallClient.AsnMMurl, PotatoWallClient.AsnMMSavePath, PotatoWallClient.AsnMMPath, PotatoWallClient.AsnMMsha256url, "SHA256");
                    await DownloadFile(PotatoWallClient.CityMMurl, PotatoWallClient.CityMMSavePath, PotatoWallClient.CityMMPath, PotatoWallClient.CityMMsha256url, "SHA256");
                }
            }
            catch (Exception ex) { PotatoWallClient.Logger.Error(ex, "Error Downloading DataBase!!"); }

            Button_DownloadDataBase.Dispatcher.InvokeOrExecute(() => { Button_DownloadDataBase.IsEnabled = true; });
        })
        { IsBackground = true }.Start();
    }

    private void Button_AddIPBlackList_Click(object sender, EventArgs e)
    {
        if (addIPViewModel.IP != null) { _ = IpBlackList.AddContains(new BaseIPData(addIPViewModel.IP, "0000")); }
    }

    private void Button_AddIPWhiteList_Click(object sender, EventArgs e)
    {
        if (addIPViewModel.IP != null) { _ = IpWhiteList.AddContains(new BaseIPData(addIPViewModel.IP, "0000")); }
    }

    private void Button_AddIPAutoWhiteList_Click(object sender, EventArgs e)
    {
        if (addIPViewModel.IP != null) { _ = IpAutoWhiteList.AddContains(new BaseIPData(addIPViewModel.IP, "0000")); }
    }

    public static string GetMB(double bytes) => (bytes / 1024f / 1024f).ToString("0.##");

    public static void StopWinDivert()
    {
        try
        {
            if (Process.GetProcessesByName("PotatoWall").Length < 2)
            {
                foreach (ServiceController sc in ServiceController.GetDevices())
                {
                    if (sc.ServiceName.Contains("WinDivert") || sc.ServiceName.Contains("WinDivert1.4"))
                    {
                        sc.Stop();
                        PotatoWallClient.Logger.Warning("Stopped WinDivert service!!");
                    }
                }
            }
        }
        catch (Exception ex) { PotatoWallClient.Logger.Error(ex, ex.Message); }
    }
}