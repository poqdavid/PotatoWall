using PotatoWall.MVVM.Model;

namespace PotatoWall.MVVM.ViewModel
{
    public class PotatoWallUIViewModel : ViewModelBase
    {
        public IPList<SrcIPData> IpWhiteList { get; set; }

        public IPList<SrcIPData> IpBlackList { get; set; }

        public IPList<SrcIPData> IpAutoWhiteList { get; set; }

        public IPList<SrcIPData> ActiveIPList { get; set; }

        private static string externalIP = "255.255.255.255";

        public string ExternalIP
        { get => externalIP; set { externalIP = value; } }

        private static string localIP = "127.0.0.1";

        public string LocalIP
        { get => localIP; set { localIP = value; } }

        public PotatoWallUIViewModel()
        {
            ActiveIPList = new IPList<SrcIPData>(Application.Current.Dispatcher);
            IpWhiteList = new IPList<SrcIPData>(Application.Current.Dispatcher);
            IpBlackList = new IPList<SrcIPData>(Application.Current.Dispatcher);
            IpAutoWhiteList = new IPList<SrcIPData>(Application.Current.Dispatcher);
        }
    }
}