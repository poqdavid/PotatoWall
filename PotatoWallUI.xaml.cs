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

using MaterialDesignColors;
using PotatoWall.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using WinDivertSharp;
using WinDivertSharp.WinAPI;

namespace PotatoWall
{
    // <copyright file="MainWindow.xaml.cs" company="POQDavid">
    // Copyright (c) POQDavid. All rights reserved.
    // </copyright>
    // <author>POQDavid</author>
    // <summary>Interaction logic for MainWindow.xaml</summary>
    public partial class PotatoWallUI : Window
    {
        ///<summary>
        /// Gets or sets the iSettings property.
        ///</summary>
        ///<value>Plugin Settings.</value>
        public static Config.Settings ISettings { get => PotatoWallClient.ISettings; set => PotatoWallClient.ISettings = value; }

        private static Brush DefaultFrogroundC = Brushes.White;

        private readonly List<PotatoParagraph> consoleQ = new();

        public IPList<SrcIPData> IpWhiteList { get; set; }

        public IPList<SrcIPData> IpBlackList { get; set; }

        public IPList<SrcIPData> IpAutoWhiteList { get; set; }

        public IPList<SrcIPData> ActiveIPList { get; set; }

        private readonly DispatcherTimer updater = new();
        private readonly DispatcherTimer IPActivityCheck = new();

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

        private static volatile bool ModifyActiveIPList = true;

        private static readonly List<string> currentNICIPAddress = new();

        private readonly Regex RegEX;

        private readonly List<string> hosts = new() { "http://whatismyip.akamai.com/", "http://checkip.amazonaws.com/", "http://icanhazip.com/" };

        private static string externalIP = "255.255.255.255";

        public string ExternalIP { get => externalIP; set { externalIP = value; Label_PublicIP.Dispatcher.InvokeOrExecute(() => { Label_PublicIP.GetBindingExpression(ContentProperty).UpdateTarget(); }); } }

        private static string localIP = "127.0.0.1";

        public string LocalIP { get => localIP; set { localIP = value; Label_LocalIP.Dispatcher.InvokeOrExecute(() => { Label_LocalIP.GetBindingExpression(ContentProperty).UpdateTarget(); }); } }

        private readonly AddIPViewModel addIPViewModel;
        private readonly SettingsViewModel settingsViewModel;

        public PotatoWallUI()
        {
            PotatoWallClient.logger.WriteLog("Initializing resources for PotatoWallUI", LogLevel.Info);
            PotatoWallClient.logger.WriteLog($"CurrentThread CurrentCulture: {Thread.CurrentThread.CurrentCulture}", LogLevel.Info);
            PotatoWallClient.logger.WriteLog($"CurrentThread CurrentUICulture: {Thread.CurrentThread.CurrentUICulture}", LogLevel.Info);

            settingsViewModel = new SettingsViewModel();
            settingsViewModel.IColorData.Load();

            ActiveIPList = new IPList<SrcIPData>(Application.Current.Dispatcher);
            IpWhiteList = new IPList<SrcIPData>(Application.Current.Dispatcher);
            IpBlackList = new IPList<SrcIPData>(Application.Current.Dispatcher);
            IpAutoWhiteList = new IPList<SrcIPData>(Application.Current.Dispatcher);

            InitializeComponent();

            LocalIP = GetLocalIP();
            if (!currentNICIPAddress.Contains(LocalIP)) { currentNICIPAddress.Add(LocalIP); }

            PotatoWallClient.logger.WriteLog("Initialized resources for PotatoWallUI", LogLevel.Info);
            RegEX = new Regex(PotatoWallClient.ISettings.RegEX);
            PotatoWallClient.ISettings.PropertyChanged += ISettings_PropertyChanged;
            PotatoWallClient.ISettings.GUI.XTheme.PropertyChanged += ITheme_PropertyChanged;
            PotatoWallClient.ISettings.WinDivert.PropertyChanged += WinDivert_PropertyChanged;

            settingsViewModel.SelectionChangedEvent += Theme_P_SelectionChanged;
            addIPViewModel = new AddIPViewModel();
            addIPViewModel.AddIPBlackListEvent += Button_AddIPBlackList_Click;
            addIPViewModel.AddIPWhiteListEvent += Button_AddIPWhiteList_Click;
            addIPViewModel.AddIPAutoWhiteListEvent += Button_AddIPAutoWhiteList_Click;
        }

        private void WinDivert_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Config.Settings.SaveSetting();
        }

        private void ITheme_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Config.Settings.SaveSetting();
        }

        private void ISettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Button_RegEX_Icon.Foreground = PotatoWallClient.ISettings.EnableRegEX ? Brushes.LimeGreen : DefaultFrogroundC;
            Config.Settings.SaveSetting();
        }

        private void Window_Client_Loaded(object sender, RoutedEventArgs e)
        {
            Button_RegEX_Icon.Foreground = PotatoWallClient.ISettings.EnableRegEX ? Brushes.LimeGreen : DefaultFrogroundC;

            DefaultFrogroundC = Button_Setting_Icon.Foreground;
            updater.Tick += Updater_Tick;
            updater.Interval = new TimeSpan(0, 0, 0, 0, 1);
            updater.Start();

            IPActivityCheck.Tick += IPActivityCheck_Tick;
            IPActivityCheck.Interval = new TimeSpan(0, 0, 0, 1, 0);
            IPActivityCheck.Start();
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

        protected string GetExtIP()
        {
            using WebClient client = new();
            string extip = "0.0.0.0";

            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            foreach (string hosturl in hosts)
            {
                try
                {
                    WriteToConsole($"Checking public IP by {hosturl}...", Brushes.Yellow, LogLevel.Info);
                    string strdata = client.DownloadString(hosturl);
                    extip = new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}").Matches(strdata)[0].ToString();
                    break;
                }
                catch (WebException ex)
                {
                    WriteToConsole($"Error: URL: {hosturl}...", Brushes.Yellow, LogLevel.Info, ex);
                    return extip;
                }
            }

            return extip;
        }

        private void IPActivityCheck_Tick(object sender, EventArgs e)
        {
            foreach (SrcIPData iPData in ActiveIPList.ToArray())
            {
                if (DateTime.Now.Subtract(iPData.LastActivity).Seconds >= PotatoWallClient.ISettings.IPActivityTime)
                {
                    if (ModifyActiveIPList) { _ = ActiveIPList.Remove(iPData); }
                }
            }
        }

        private void Updater_Tick(object sender, EventArgs e)
        {
            Action append = () =>
            {
                if (consoleQ.Count != 0)
                {
                    richtextBox_console.CheckAppendText(consoleQ[0].ToParagraph());
                    consoleQ.RemoveAt(0);
                }
            };

            if (richtextBox_console.CheckAccess())
            {
                append();
            }
            else
            {
                _ = richtextBox_console.Dispatcher.BeginInvoke(append);
            }
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
                    WriteToConsole("Please wait checking connection...", Brushes.Yellow, LogLevel.Info);

                    LocalIP = GetLocalIP();

                    if (!currentNICIPAddress.Contains(LocalIP)) { currentNICIPAddress.Add(LocalIP); }

                    WriteToConsole("Local IP: " + LocalIP, Brushes.Red);

                    ExternalIP = GetExtIP();

                    WriteToConsole("Public IP: " + ExternalIP, Brushes.Red);

                    WinDFriesWallRunning = true;
                    WinDivertInit("PotatoFriesWall", ref WinDFriesWallHandle, WinDivertOpenFlags.None);
                    RunBackgroundThread(PotatoFriesWall, "PotatoFriesWall", ThreadPriority.Highest);

                    WinDFriesWallMonitorRunning = true;
                    WinDivertInit("PotatoFriesWallMonitor", ref WinDFriesWallMonitorHandle, WinDivertOpenFlags.Sniff);
                    RunBackgroundThread(PotatoFriesWallMonitor, "PotatoFriesWallMonitor", ThreadPriority.Highest);

                    _ = Button_Enable.Dispatcher.BeginInvoke((Action)delegate ()
                    {
                        Button_Enable.IsEnabled = true;
                    });
                })
                { IsBackground = true }.Start();
            }
            else
            {
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
            if (PotatoWallClient.ISettings.EnableRegEX)
            {
                PotatoWallClient.ISettings.EnableRegEX = false;
                Button_RegEX_Icon.Foreground = DefaultFrogroundC;
            }
            else
            {
                PotatoWallClient.ISettings.EnableRegEX = true;
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

                WriteToConsole($"Please wait {PotatoWallClient.ISettings.IPSearchDuration} seconds adding IPs...", Brushes.Yellow);
                Button_AutoList.IsEnabled = false;

                new Thread(() =>
                {
                    AutoAddWhiteList = true;
                    for (int i = 1; i < (PotatoWallClient.ISettings.IPSearchDuration + 1); i++)
                    {
                        Thread.Sleep(1000);
                        if (i == 10)
                        {
                            AutoAddWhiteList = false;
                            Thread.Sleep(1000);

                            ModeAuto = true;
                            _ = Button_AutoList.Dispatcher.BeginInvoke((Action)delegate ()
                              {
                                  Button_AutoList.IsEnabled = true;
                              });

                            WriteToConsole("Done finished adding IPs.", Brushes.Yellow);
                        }
                    }
                })
                { IsBackground = true }.Start();

                Button_AutoList_Icon.Foreground = Brushes.LimeGreen;
            }
        }

        private void RichtextBox_Console_TextChanged(object sender, TextChangedEventArgs e)
        {
            richtextBox_console.ScrollToEnd();
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
                        Color cx = (Color)ColorConverter.ConvertFromString(cdata);
                        SetTheme(cx);
                    }
                }
            }
        }

        public static void SetTheme(string cname)
        {
            MaterialDesignThemes.Wpf.PaletteHelper paletteHelper = new();
            SwatchesProvider swatchesProvider = new();
            Swatch color = swatchesProvider.Swatches.FirstOrDefault(a => a.Name == cname);
            paletteHelper.ReplacePrimaryColor(color);
        }

        public static void SetTheme(Color c)
        {
            MaterialDesignThemes.Wpf.PaletteHelper paletteHelper = new();

            paletteHelper.ChangePrimaryColor(c);
        }

        private void Window_Client_ContentRendered(object sender, EventArgs e)
        {
            PotatoWallClient.logger.WriteLog("Finished rendering content for PotatoWallUI", LogLevel.Info);
            if (settingsViewModel.IColorData.ColorDataList[PotatoWallClient.ISettings.GUI.XTheme.Color] is ColorDataList list)
            {
                if (list.ColorMetadata == "REC")
                {
                    SetTheme(list.ColorName);
                }
                else if (list.ColorMetadata != "SEP")
                {
                    string cdata = list.ColorMetadata;
                    Color cx = (Color)ColorConverter.ConvertFromString(cdata);
                    SetTheme(cx);
                }
            }
            grid_loading.Visibility = Visibility.Hidden;
        }

        private void Button_Setting_Click(object sender, RoutedEventArgs e)
        {
            _ = MaterialDesignThemes.Wpf.DialogHost.Show(settingsViewModel, "RootDialog");
        }

        private void Button_AddIP_Click(object sender, RoutedEventArgs e)
        {
            _ = MaterialDesignThemes.Wpf.DialogHost.Show(addIPViewModel, "RootDialog");
        }

        private void WriteToConsole(string text, Brush br, LogLevel logLevel, Exception ex)
        {
            consoleQ.Add(new PotatoParagraph(text, Brushes.Transparent, br, true, false, false));
            PotatoWallClient.logger.WriteLog(ex, text, logLevel);
        }

        private void WriteToConsole(string text, Brush br, LogLevel logLevel)
        {
            consoleQ.Add(new PotatoParagraph(text, Brushes.Transparent, br, true, false, false));
            PotatoWallClient.logger.WriteLog(text, logLevel);
        }

        private void WriteToConsole(string text, Brush br)
        {
            consoleQ.Add(new PotatoParagraph(text, Brushes.Transparent, br, true, false, false));
        }

        private void WriteToConsole(string text)
        {
            WriteToConsole(text, Brushes.Green);
        }

        private void WinDivert_CheckFilter()
        {
            uint errorPos = 0;

            if (!WinDivert.WinDivertHelperCheckFilter(PotatoWallClient.ISettings.WinDivert.Filter, WinDivertLayer.Network, out string errorMessage, ref errorPos))
            {
                WriteToConsole(string.Format(Thread.CurrentThread.CurrentCulture, "Filter string is invalid at position {0}.\nError Message:\n{1}", errorPos, errorMessage), Brushes.Red);
            }
        }

        private void PotatoFriesWall()
        {
            string name = "PotatoFriesWall";
            WinDivertBuffer packet = new();

            WinDivertAddress addr = new();

            uint readLen = 0;

            IntPtr recvEvent = IntPtr.Zero;

            NativeOverlapped recvOverlapped;

            uint recvAsyncIoLen = 0;

            do
            {
                if (WinDFriesWallRunning)
                {
                    readLen = 0;

                    recvEvent = Kernel32.CreateEvent(IntPtr.Zero, false, false, IntPtr.Zero);

                    string SrcIPAddress = "0.0.0.0";
                    ushort SrcPort = 0;

                    string DstIPAddress = "0.0.0.0";
                    ushort DstPort = 0;

                    allowpacket = true;

                    if (recvEvent == IntPtr.Zero)
                    {
                        WriteToConsole($"{name}: Failed to initialize receive IO event.", Brushes.Red);
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

                        if (!WinDivert.WinDivertRecvEx(WinDFriesWallHandle, packet, 0, ref addr, ref readLen, ref recvOverlapped))
                        {
                            int error = Marshal.GetLastWin32Error();

                            // 997 == ERROR_IO_PENDING
                            if (error != 997)
                            {
                                WriteToConsole($"{name}: Unknown IO error ID {error} while awaiting result.", Brushes.Red);

                                _ = Kernel32.CloseHandle(recvEvent);
                                continue;
                            }

                            while (Kernel32.WaitForSingleObject(recvEvent, 1000) == (uint)WaitForSingleObjectResult.WaitTimeout) { }

                            if (!Kernel32.GetOverlappedResult(WinDFriesWallHandle, ref recvOverlapped, ref recvAsyncIoLen, false))
                            {
                                WriteToConsole($"{name}: Failed to get overlapped result.", Brushes.Red);

                                _ = Kernel32.CloseHandle(recvEvent);
                                continue;
                            }

                            readLen = recvAsyncIoLen;
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
                                WriteToConsole($"{name}: Unknown IO error ID {error} while awaiting result.", Brushes.Red);
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

                    if (ModeBlockAll) { allowpacket = false || (RegEX.IsMatch(ChkIPAddress) && PotatoWallClient.ISettings.EnableRegEX); }

                    if (ModeBlackList)
                    {
                        allowpacket = !IpBlackList.Contains(ChkIPAddress) || IpBlackList.Count == 0;
                    }

                    if (ModeWhiteList)
                    {
                        allowpacket = IpWhiteList.Contains(ChkIPAddress) || (RegEX.IsMatch(ChkIPAddress) && PotatoWallClient.ISettings.EnableRegEX);
                    }

                    if (ModeAuto)
                    {
                        allowpacket = IpAutoWhiteList.Contains(ChkIPAddress) || (RegEX.IsMatch(ChkIPAddress) && PotatoWallClient.ISettings.EnableRegEX);
                    }

                    if (!ModeBlockAll && !ModeBlackList && !ModeWhiteList && !ModeAuto)
                    {
                        allowpacket = true;
                    }

                    if (ChkIPAddress == LocalIP) { allowpacket = true; }
                    if (ChkIPAddress == ExternalIP) { allowpacket = true; }

                    if (allowpacket)
                    {
                        if (PotatoWallClient.ISettings.WinDivert.WinDivertRecvEx)
                        {
                            if (!WinDivert.WinDivertSendEx(WinDFriesWallHandle, packet, readLen, 0, ref addr))
                            {
                                WriteToConsole($"{name}: Write Err: {Marshal.GetLastWin32Error()}", Brushes.Red);
                            }
                        }
                        else if (PotatoWallClient.ISettings.WinDivert.WinDivertRecv)
                        {
                            if (!WinDivert.WinDivertSend(WinDFriesWallHandle, packet, readLen, ref addr))
                            {
                                WriteToConsole($"{name}: Write Err: {Marshal.GetLastWin32Error()}", Brushes.Red);
                            }
                        }
                    }
                }
            }
            while (WinDFriesWallRunning);
        }

        private void PotatoFriesWallMonitor()
        {
            string name = "PotatoFriesWallMonitor";

            WinDivertBuffer packet = new();

            WinDivertAddress addr = new();

            uint readLen = 0;

            IntPtr recvEvent = IntPtr.Zero;

            NativeOverlapped recvOverlapped;

            uint recvAsyncIoLen = 0;

            do
            {
                if (WinDFriesWallMonitorRunning)
                {
                    readLen = 0;

                    recvEvent = Kernel32.CreateEvent(IntPtr.Zero, false, false, IntPtr.Zero);

                    string SrcIPAddress = "0.0.0.0";
                    ushort SrcPort = 0;

                    string DstIPAddress = "0.0.0.0";
                    ushort DstPort = 0;

                    if (recvEvent == IntPtr.Zero)
                    {
                        WriteToConsole($"{name}: Failed to initialize receive IO event.", Brushes.Red);
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

                        if (!WinDivert.WinDivertRecvEx(WinDFriesWallMonitorHandle, packet, 0, ref addr, ref readLen, ref recvOverlapped))
                        {
                            int error = Marshal.GetLastWin32Error();

                            // 997 == ERROR_IO_PENDING
                            if (error != 997)
                            {
                                WriteToConsole($"{name}: Unknown IO error ID {0} while awaiting result.", Brushes.Red);

                                _ = Kernel32.CloseHandle(recvEvent);
                                continue;
                            }

                            while (Kernel32.WaitForSingleObject(recvEvent, 1000) == (uint)WaitForSingleObjectResult.WaitTimeout) { }

                            if (!Kernel32.GetOverlappedResult(WinDFriesWallMonitorHandle, ref recvOverlapped, ref recvAsyncIoLen, false))
                            {
                                WriteToConsole($"{name}: Failed to get overlapped result.", Brushes.Red);

                                _ = Kernel32.CloseHandle(recvEvent);
                                continue;
                            }

                            readLen = recvAsyncIoLen;
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
                                WriteToConsole($"{name}: Unknown IO error ID {0} while awaiting result.", Brushes.Red);
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
                        _ = addr.Direction == WinDivertDirection.Inbound
                            ? IpAutoWhiteList.AddContains(new SrcIPData(SrcIPAddress, $"{SrcPort.SWPOrder()}", DstIPAddress, $"{DstPort.SWPOrder()}"))
                            : IpAutoWhiteList.AddContains(new SrcIPData(DstIPAddress, $"{DstPort.SWPOrder()}", SrcIPAddress, $"{SrcPort.SWPOrder()}"));
                    }

                    if (!currentNICIPAddress.Contains(SrcIPAddress))
                    {
                        ModifyActiveIPList = false;
                        Dictionary<string, SrcIPData> datasrc = ActiveIPList.ToDictionary(x => x.IP, x => x);

                        if (ActiveIPList.AddContains(new SrcIPData(SrcIPAddress, $"{SrcPort.SWPOrder()}", DstIPAddress, $"{DstPort.SWPOrder()}", Direction)))
                        {
                            datasrc[SrcIPAddress].Port = $"{SrcPort.SWPOrder()}";
                            datasrc[SrcIPAddress].Direction = Direction;

                            datasrc[SrcIPAddress].DstIPData.IP = DstIPAddress;
                            datasrc[SrcIPAddress].DstIPData.Port = $"{DstPort.SWPOrder()}";
                        }
                        ModifyActiveIPList = true;
                    }
                }
            }
            while (WinDFriesWallMonitorRunning);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WinDivertInit(string name, ref IntPtr WinDHandle, WinDivertOpenFlags flags)
        {
            WriteToConsole($"{name}: Starting WinDivertInit()!");

            string filter = PotatoWallClient.ISettings.WinDivert.Filter;
            WriteToConsole($"{name}: WinDivert Filter= \"{filter}\"");

            WinDivert_CheckFilter();

            WinDHandle = WinDivert.WinDivertOpen(filter, WinDivertLayer.Network, 0, flags);

            WriteToConsole($"{name}: WinDivert Handle = {WinDHandle}");

            _ = WinDivert.WinDivertSetParam(WinDHandle, WinDivertParam.QueueLen, PotatoWallClient.ISettings.WinDivert.QueueLen);
            _ = WinDivert.WinDivertSetParam(WinDHandle, WinDivertParam.QueueTime, PotatoWallClient.ISettings.WinDivert.QueueTime);
            _ = WinDivert.WinDivertSetParam(WinDHandle, WinDivertParam.QueueSize, PotatoWallClient.ISettings.WinDivert.QueueSize);

            WriteToConsole($"{name}: Started WinDivertInit()!");
        }

        private void RunBackgroundThread(ThreadStart start, string thname, ThreadPriority tp)
        {
            WriteToConsole($"{thname}: Starting background thread!");
            Thread background = new(start)
            {
                IsBackground = true,
                Name = thname,
                Priority = tp
            };

            background.Start();
            WriteToConsole($"{thname}: Started background thread!");
        }

        private void MenuItem_BlacklistAdd_Click(object sender, RoutedEventArgs e)
        {
            _ = IpBlackList.AddContains((SrcIPData)listBox_activeiplist.SelectedItem);
        }

        private void MenuItem_WhitelistAdd_Click(object sender, RoutedEventArgs e)
        {
            _ = IpWhiteList.AddContains((SrcIPData)listBox_activeiplist.SelectedItem);
        }

        private void MenuItem_AutoWhitelistAdd_Click(object sender, RoutedEventArgs e)
        {
            _ = IpAutoWhiteList.AddContains((SrcIPData)listBox_activeiplist.SelectedItem);
        }

        private void MenuItem_WhitelistRemove_Click(object sender, RoutedEventArgs e)
        {
            _ = IpWhiteList.Remove((SrcIPData)listBox_whitelist.SelectedItem);
        }

        private void MenuItem_BlacklistRemove_Click(object sender, RoutedEventArgs e)
        {
            _ = IpBlackList.Remove((SrcIPData)listBox_blacklist.SelectedItem);
        }

        private void MenuItem_AutoWhitelistRemove_Click(object sender, RoutedEventArgs e)
        {
            _ = IpAutoWhiteList.Remove((SrcIPData)listBox_autowhitelist.SelectedItem);
        }

        private void MenuItem_CopySrcIP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SrcIPData srcIPData = (SrcIPData)listBox_activeiplist.SelectedItem;
                if (srcIPData.IP != null) { Clipboard.SetText(srcIPData.IP); }
            }
            catch (Exception ex) { PotatoWallClient.logger.WriteLog(ex, "Error: CopySrcIP", LogLevel.Error); }
        }

        private void MenuItem_CopyDstIP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SrcIPData srcIPData = (SrcIPData)listBox_activeiplist.SelectedItem;
                if (srcIPData.DstIPData != null && srcIPData.DstIPData.IP != null) { Clipboard.SetText(srcIPData.IP); }
            }
            catch (Exception ex) { PotatoWallClient.logger.WriteLog(ex, "Error: CopyDstIP", LogLevel.Error); }
        }

        private void MenuItem_CopyAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SrcIPData srcIPData = (SrcIPData)listBox_activeiplist.SelectedItem;
                if (srcIPData != null)
                {
                    string data = $"Con: {srcIPData.Direction} \n" +
                                  $"Src: {srcIPData.Country} {srcIPData.IP}:{srcIPData.Port} {srcIPData.ASN} \n" +
                                  $"Dst: {srcIPData.DstIPData.Country} {srcIPData.DstIPData.IP}:{srcIPData.DstIPData.Port} {srcIPData.DstIPData.ASN} \n";
                    Clipboard.SetText(data);
                }
            }
            catch (Exception ex) { PotatoWallClient.logger.WriteLog(ex, "Error: CopyAll", LogLevel.Error); }
        }

        private void DownloadFile(Uri url, string fileName, string newFileName)
        {
            long urlSize = GetFileSize(url);
            if (urlSize != 0)
            {
                FileInfo fi = new(fileName);
                if (fi.Exists)
                {
                    if (fi.Length == urlSize)
                    {
                        GZDecompress(fi, newFileName);
                    }
                    else
                    {
                        WClient(url, fileName);
                        GZDecompress(fi, newFileName);
                    }
                }
                else
                {
                    WClient(url, fileName);
                    GZDecompress(fi, newFileName);
                }
            }
        }

        private void WClient(Uri url, string fileName)
        {
            WriteToConsole($"Downloading {url}...", Brushes.Yellow);
            using WebClient webc = new();
            webc.DownloadProgressChanged += WebClient_DownloadProgressChanged;
            webc.DownloadFileTaskAsync(url, fileName).Wait();
            WriteToConsole("Finished Downloading.", Brushes.Yellow);
        }

        private static long GetFileSize(Uri url)
        {
            try
            {
                WebClient client = new();
                using Stream sr = client.OpenRead(url); return Convert.ToInt64(client.ResponseHeaders["Content-Length"], Thread.CurrentThread.CurrentCulture);
            }
            catch (Exception) { return 0; }
        }

        public void GZDecompress(FileInfo fileToDecompress, string newFileName)
        {
            WriteToConsole($"Decompressing {fileToDecompress}...", Brushes.Yellow);

            using FileStream originalFileStream = fileToDecompress.OpenRead();
            string currentFileName = fileToDecompress.FullName;

            if (newFileName.Contains(@"data\asn.mmdb")) { if (PotatoWallClient.ASNMMDBReader != null) { PotatoWallClient.ASNMMDBReader.Dispose(); } }
            if (newFileName.Contains(@"data\city.mmdb")) { if (PotatoWallClient.CityMMDBReader != null) { PotatoWallClient.CityMMDBReader.Dispose(); } }

            using (FileStream decompressedFileStream = File.Create(newFileName))
            {
                using GZipStream decompressionStream = new(originalFileStream, CompressionMode.Decompress);
                decompressionStream.CopyTo(decompressedFileStream);
            }

            if (newFileName.Contains(@"data\asn.mmdb")) { PotatoWallClient.ASNMMDBReader = new(newFileName); }
            if (newFileName.Contains(@"data\city.mmdb")) { PotatoWallClient.CityMMDBReader = new(newFileName); }

            WriteToConsole("Finished Decompressing.", Brushes.Yellow);
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 100) { Button_DownloadDataBase_Text.Dispatcher.InvokeOrExecute(() => { Button_DownloadDataBase_Text.Text = "Download Database"; }); }
            else { Button_DownloadDataBase_Text.Dispatcher.InvokeOrExecute(() => { Button_DownloadDataBase_Text.Text = $"Downloading Database %{e.ProgressPercentage}"; }); }
        }

        private void Button_DownloadDataBase_Click(object sender, RoutedEventArgs e)
        {
            new Thread(() =>
            {
                DownloadFile(PotatoWallClient.ASNDBurl, PotatoWallClient.ASNDBSavePath, PotatoWallClient.ASNDBPath);
                DownloadFile(PotatoWallClient.CityDBurl, PotatoWallClient.CityDBSavePath, PotatoWallClient.CityDBPath);
            })
            { IsBackground = true }.Start();
        }

        private void Button_AddIPBlackList_Click(object sender, EventArgs e)
        {
            if (addIPViewModel.IP != null) { _ = IpBlackList.AddContains(new SrcIPData(addIPViewModel.IP, "0000")); }
        }

        private void Button_AddIPWhiteList_Click(object sender, EventArgs e)
        {
            if (addIPViewModel.IP != null) { _ = IpWhiteList.AddContains(new SrcIPData(addIPViewModel.IP, "0000")); }
        }

        private void Button_AddIPAutoWhiteList_Click(object sender, EventArgs e)
        {
            if (addIPViewModel.IP != null) { _ = IpAutoWhiteList.AddContains(new SrcIPData(addIPViewModel.IP, "0000")); }
        }
    }
}