using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Hardcodet.Wpf.TaskbarNotification;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace WinExWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string NickName = "";
        string myTime = "";
        string myWindow = "";
        string myCPU = "";
        string[] sap = new string[4];
        string sep = "_:%:_";
        string[] seps = new string[4];
        //string[] ip_aray;
        int srvPort = 7857;
        string BroadcastIP = "192.168.1.255";
        bool CloseEqHide = true;
        bool StartMinimazed = true;
        bool EnableWinBroadcastOnStartup = true;
        bool EnableWinBrListerOnStartup = true;
        Socket serverSocket;
        //        Socket ClientSocket;
        UdpClient UDPCl;
        IPEndPoint ipEPWB;
        byte[] byteData = new byte[1024];
        private const int WM_QUERYENDSESSION = 0x11;
        private bool systemShutdown = false;
        public bool Loaded = false;
        bool UseSmallWindow = false;
        bool Dragging = false;
        int DraggingX0, DraggingY0;
        bool TrayCPU = false;
        float mCPU;
        bool WindowShowed = false;
        bool WindowCompacted = false;
        bool Debug = false;
        bool DebogLog = true;
        bool CompactOnShow = false;
        int FishDrive = 0;
        System.IO.DriveInfo[] DrInf;

        //Icons
        //Icon[] TrayIconArray = new Icon[6];

        //Color t_frmBackColor;
        //Color t_lstBackColor = Color.WhiteSmoke;
        //Color t_lstFontColor = Color.Black;
        //Color t_lstHighLigtBackColor1 = Color.LightCoral;
        string CurrentTheme = "default";
        string[,] RepArray = new string[20, 2];
        string AppDir = "";
        bool PathConverter = false;
        string lastMessage = "";
        bool Sounds = true;
        int ZeroSpace = 0;
        int LowSpace = 0;
        bool UpdateGen = false;
        string UpdateServer = "";
        int TrayIconsStyle = 0;


        public ObservableCollection<CurrentInformationContainer> NetworkUsers = new ObservableCollection<CurrentInformationContainer>();


        TaskbarIcon tbi;
        System.Windows.Threading.DispatcherTimer TOS_Timer;
        TaskScheduler UIScheduler;
        CurrentInfo CurrentInfoObj = new CurrentInfo();
        CurrentInformationContainer cic = new CurrentInformationContainer();
        NetworkCore NetworkExchange = new NetworkCore();

        public MainWindow()
        {
            InitializeComponent();

            System.Reflection.Assembly execAssem =
    System.Reflection.Assembly.GetExecutingAssembly();
            AppDir =
                System.IO.Path.GetDirectoryName(execAssem.GetModules()[0].FullyQualifiedName);
            // Make sure there is a path separator on the end.
            if (!AppDir.EndsWith(@"\"))
            {
                AppDir += @"\";
            }
            UIScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            //nwCreateSocketListen(srvPort);
            UDPCl = new UdpClient(AddressFamily.InterNetwork);
            sap[0] = "";
            for (int i = 0; i < 4; i++)
            {
                seps[i] = sep;
            }

           

            TrayIcon.TrayLeftMouseDown += new RoutedEventHandler(tbi_TrayLeftMouseDown);


            cic.NickName = "Test";

            TOS_Timer = new System.Windows.Threading.DispatcherTimer();
            TOS_Timer.Tick += new EventHandler(TOS_Timer_Tick);
            TOS_Timer.Interval = new TimeSpan(0, 0, 3);
            TOS_Timer.IsEnabled = true;
            NetworkExchange.MessageReceive += new MessageReceiveEventHandler(Net_MessageRecieve);
            listView1.ItemsSource = NetworkUsers;


        }

        void tbi_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                this.Visibility = Visibility.Hidden;
            }
            else
            {
                this.Visibility = Visibility.Visible;
            }
        }

        public void SortList()
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(listView1.ItemsSource);
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription("NickName", ListSortDirection.Ascending));  
        }


        private void Net_MessageRecieve(object sender, MessageReceiveEventArgs e)
        {
            //MessageBox.Show("Reached: " + e.msg.strMessage.ToString());
            System.Windows.Application.Current.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                (Action)delegate()
                {
                    if (e.msg.cmdCommand == Command.WindowBroadcast)
                    {
                        sap = e.msg.strMessage.Split(seps, 4, StringSplitOptions.None);
                        //sap array:
                        //0=Nick
                        //1=Time
                        //2=Window
                        //3=CPU
                        float cpu = 0;
                        float.TryParse(sap[3], out cpu);
                        bool found = false;
                        foreach (CurrentInformationContainer ci in NetworkUsers)
                        {
                            if (ci.NickName == e.msg.strName)
                            {
                                ci.UpdateDate = DateTime.Now;
                                ci.Window = sap[2];
                                ci.CPU = cpu;
                                found = true;
                            }
                        }
                        if (!found)
                        {
                            NetworkUsers.Add(new CurrentInformationContainer() { NickName = e.msg.strName, Window = sap[2], CPU = cpu, UpdateDate = DateTime.Now });
                            SortList();
                        }
                    }
                });
            
        }

        public void Net_MessageRecieveResult(MessageReceiveEventArgs e)
        {
        }

        void TOS_Timer_Tick(object sender, EventArgs e)
        {
            cic.CPU = CurrentInfoObj.getCurrentCpuUsage();
            cic.Window = CurrentInfoObj.GetActWinText();
            NetworkExchange.UDPBroadSendWindowText(ref cic);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            NetworkExchange.UDPBroadSendExit(ref cic);
        }

        private void Tray_Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

    }


    enum RecycleFlags : uint
    {
        SHERB_NOCONFIRMATION = 0x00000001,
        SHERB_NOPROGRESSUI = 0x00000002,
        SHERB_NOSOUND = 0x00000004
    }





}
