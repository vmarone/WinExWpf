using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel;

namespace WinExWpf
{
    public delegate void MessageReceiveEventHandler(object sender,
        MessageReceiveEventArgs e);

    class NetworkCore
    {
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
        string[] sap = new string[4];
        string sep = "_:%:_";

        public NetworkCore()
        {
            UDPCl = new UdpClient(AddressFamily.InterNetwork);
            ipEPWB = new IPEndPoint(IPAddress.Parse(BroadcastIP), srvPort);
            nwCreateSocketListen(srvPort);
        }

        public void nwCreateSocketListen(int port)
        {
            try
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPEndPoint ipEP = new IPEndPoint(IPAddress.Any, port);
                serverSocket.Bind(ipEP);
                serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                IPEndPoint ipeSender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint epSender = (EndPoint)ipeSender;
                //Start receiving data
                serverSocket.BeginReceiveFrom(byteData, 0, byteData.Length,
                    SocketFlags.None, ref epSender, new AsyncCallback(OnReceive), epSender);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "ServerUDP",
                //MessageBoxButtons.OK, MessageBoxIcon.Error); 
                //ErrorDlg(ex, "ServerUDP");
            }
        }


        public void UDPBroadSendWindowText(ref CurrentInformationContainer cic)
        {
            Data msgToSend = new Data();
            msgToSend.cmdCommand = Command.WindowBroadcast;
            msgToSend.strName = cic.NickName;
            msgToSend.strMessage = cic.NickName + sep + DateTime.Now.ToString("h:mm:ss") + sep + cic.Window + sep +  cic.CPU.ToString("###");
            byte[] message = msgToSend.ToByte();
            UDPCl.Send(message, message.Length, ipEPWB);
        }

        public void UDPBroadSendExit(ref CurrentInformationContainer cic)
        {
            Data msgToSend = new Data();
            msgToSend.cmdCommand = Command.cmdlineInternal;
            msgToSend.strName = cic.NickName;
            msgToSend.strMessage = "MeExit";
            byte[] message = msgToSend.ToByte();
            UDPCl.Send(message, message.Length, ipEPWB);
        }

        public event MessageReceiveEventHandler MessageReceive;
        protected virtual void OnMessageReceive(MessageReceiveEventArgs e)
        {
            if (MessageReceive != null)
            {
                MessageReceive(this, e);//Raise the event
            }
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                IPEndPoint ipeSender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint epSender = (EndPoint)ipeSender;
                serverSocket.EndReceiveFrom(ar, ref epSender);
                string SenderIp = ((IPEndPoint)epSender).Address.ToString();
                int SenderPort = ((IPEndPoint)epSender).Port;
                Data msgRecv = new Data(byteData);

                MessageReceiveEventArgs e = new MessageReceiveEventArgs(msgRecv);
                OnMessageReceive(e);

                //Start receiving data
                serverSocket.BeginReceiveFrom(byteData, 0, byteData.Length,
                    SocketFlags.None, ref epSender, new AsyncCallback(OnReceive), epSender);
            }
            catch (Exception ex)
            {
                string a = "";
                //MessageBox.Show(ex.Message, "ServerUDP",
                //MessageBoxButtons.OK, MessageBoxIcon.Error);
                //ErrorDlg(ex, "ServerUDP");
            }

        }



    }


    public class MessageReceiveEventArgs : EventArgs
    {
        private Data _msg;
        public MessageReceiveEventArgs(Data msg)
        {
            this._msg = msg;
        }
        public Data msg
        {
            get
            {
                return _msg;
            }
        }
    }


    public class CurrentInformationContainer : INotifyPropertyChanged
    {
        private string _NickName;
        private string _Window;
        private DateTime _UpdateDate;
        private float _CPU;
        public Exception ex { get; set; }
        public DateTime UpdateDate
        {
            get { return _UpdateDate; }
            set
            {
                if (_UpdateDate != value)
                {
                    _UpdateDate = value;
                    OnPropertyChanged("UpdateDate");
                }
            }
        }
        public float CPU
        {
            get { return _CPU; }
            set
            {
                if (_CPU != value)
                {
                    _CPU = value;
                    OnPropertyChanged("CPU");
                }
            }
        }
        public string NickName
        {
            get { return _NickName; }
            set
            {
                if (_NickName != value)
                {
                    _NickName = value;
                    OnPropertyChanged("NickName");
                }
            }
        }
        public string Window
        {
            get { return _Window; }
            set
            {
                if (_Window != value)
                {
                    _Window = value;
                    OnPropertyChanged("Window");
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string info)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(info));
            }
        }
    }

    //The data structure by which the server and the client interact with 
    //each other
    public class Data
    {
        //Default constructor
        public Data()
        {
            this.cmdCommand = Command.Null;
            this.strMessage = null;
            this.strName = null;
        }

        //Converts the bytes into an object of type Data
        public Data(byte[] data)
        {
            //The first four bytes are for the Command
            this.cmdCommand = (Command)BitConverter.ToInt32(data, 0);

            //The next four store the length of the name
            int nameLen = BitConverter.ToInt32(data, 4);

            //The next four store the length of the message
            int msgLen = BitConverter.ToInt32(data, 8);

            //This check makes sure that strName has been passed in the array of bytes
            if (nameLen > 0)
                this.strName = Encoding.Default.GetString(data, 12, nameLen);
            else
                this.strName = null;

            //This checks for a null message field
            if (msgLen > 0)
                this.strMessage = Encoding.Default.GetString(data, 12 + nameLen, msgLen);
            else
                this.strMessage = null;
        }

        //Converts the Data structure into an array of bytes
        public byte[] ToByte()
        {
            List<byte> result = new List<byte>();

            //First four are for the Command
            result.AddRange(BitConverter.GetBytes((int)cmdCommand));

            //Add the length of the name
            if (strName != null)
                result.AddRange(BitConverter.GetBytes(strName.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));

            //Length of the message
            if (strMessage != null)
                result.AddRange(BitConverter.GetBytes(strMessage.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));

            //Add the name
            if (strName != null)
                result.AddRange(Encoding.Default.GetBytes(strName));

            //And, lastly we add the message text to our array of bytes
            if (strMessage != null)
                result.AddRange(Encoding.Default.GetBytes(strMessage));

            return result.ToArray();
        }

        public string strName;      //Name by which the client logs into the room
        public string strMessage;   //Message text
        public Command cmdCommand;  //Command type (login, logout, send message, etcetera)
    }

    public enum Command
    {
        Message, //Show message
        WindowBroadcast, //ActiveWindow Broadcast
        cmdlineInternal,
        cmdline,
        Terminal,
        Update,
        Null
    }
}
