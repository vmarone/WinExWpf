using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

using HANDLE = System.IntPtr;
using HWND = System.IntPtr;
using HDC = System.IntPtr;

namespace WinExWpf
{
    class CurrentInfo
    {
        [DllImport("User32.dll")]
        public static extern HWND GetActiveWindow();
        [DllImport("User32.dll")]
        public static extern HWND GetForegroundWindow();
        [DllImport("User32.dll")]
        public static extern int GetWindowText(HWND hwnd,
            [MarshalAs(UnmanagedType.LPArray)] byte[] lpBuffer,
            int cch);
        [DllImport("User32.dll")]
        public static extern int GetWindowTextLength(HWND hwnd);
        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg,
                      IntPtr wParam, IntPtr lParam);
        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        static extern uint SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, RecycleFlags dwFlags);
        [DllImport("User32.dll")]
        private static extern bool
                GetLastInputInfo(ref LASTINPUTINFO plii);
        internal struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        PerformanceCounter cpuCounter;

        public CurrentInfo()
        {
            cpuCounter = new PerformanceCounter();
            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";
        }

        public string GetActWinText()
        {
            HWND hwndi = GetForegroundWindow();
            int WTL = GetWindowTextLength(hwndi);
            byte[] str = new byte[WTL];
            GetWindowText(hwndi, str, WTL + 1);
            return System.Text.Encoding.Default.GetString(str);
        }

        public float getCurrentCpuUsage()
        {
            return cpuCounter.NextValue();
        }

        public string GetVersionStr()
        {
            string s = "";
            string w = "";
            /*if (myWindow == "Zoom Player")
            {
                w += "Zoom Player:" + "???" + "\r\n";
            }*/
            bool GetVersionStr_ScanProc = true;
            if (GetVersionStr_ScanProc)
            {
                System.Diagnostics.Process[] proc = System.Diagnostics.Process.GetProcesses();
                foreach (System.Diagnostics.Process p in proc)
                {
                    //s += p.ProcessName + "\r\n";
                    if (p.ProcessName == "skynet0897b6sbf0_20a")
                        w += "_skynet0897b6sbf0_20a" + "\r\n";
                    if (p.ProcessName == "TOTALCMD")
                        w += "_Totalcmd" + "\r\n";
                    if (p.ProcessName == "egui")
                        w += "_NOD 32 antivirus" + "\r\n";
                    if (p.ProcessName == "Opera")
                        w += "_Opera" + "\r\n";
                    if (p.ProcessName == "AmpView")
                        w += "_AmpView" + "\r\n";
                    if (p.ProcessName == "winamp")
                    {
                        string title = p.MainWindowTitle;
                        w += "WinAmp (" + title + ")\r\n";
                    }
                    if (p.ProcessName == "foobar2000")
                        w += "_foobar2000" + "\r\n";
                    if (p.ProcessName == "KMPlayer")
                        w += "_KMPlayer" + "\r\n";
                    if (p.ProcessName == "zplayer")
                    {
                        //01.avi - Zoom Player WMV Professional
                        string title = p.MainWindowTitle;
                        title = title.Replace(" - Zoom Player WMV Professional", "");
                        w += "Zoom Player (" + title + ")\r\n";
                    }
                }
            }
            s = w;
            /*
            s = NickName +
                " Version " + Application.ProductVersion + "\r\n" +
                "Theme:" + CurrentTheme + "\r\n" +
                "WindowShowed:" + WindowShowed.ToString() + "\r\n" +
                "WindowCompacted:" + WindowCompacted.ToString() + "\r\n" +
                w +
                "Idle:" + GetIdleTimeStr() + "\r\n" +
                "Os:" + GetOsVersion();*/
            return s;

        }

        public string GetOsVersion()
        {
            string s = "";
            PlatformID pID = Environment.OSVersion.Platform;
            Version osVer = Environment.OSVersion.Version;
            string sp = Environment.OSVersion.ServicePack;
            switch (pID)
            {
                case PlatformID.Win32NT:
                    s = "Windows";

                    switch (osVer.Major)
                    {
                        case 5:

                            switch (osVer.Minor)
                            {
                                case 0:
                                    s += " 2000";
                                    break;
                                case 1:
                                    s += " XP";
                                    break;
                                default:
                                    s += osVer.Minor.ToString();
                                    break;
                            }
                            break;
                        case 6:
                            s += " Vista";
                            break;
                        case 7:
                            s += " Viena";
                            break;
                        default:
                            s += " NT";
                            break;
                    }

                    if (sp.Length > 1)
                    {
                        s += " " + sp;
                    }

                    break;
                case PlatformID.WinCE:
                    s = "WindowsCE";
                    break;
                default:
                    s = "Unknown";
                    break;
            }
            s += " " + osVer.ToString();

            return s;
        }

        public string GetIdleTimeStr()
        {
            uint idle = (uint)GetIdleTime() / 1000;
            string str = "";

            if (idle > 60)
            {
                str = ((uint)idle / 60).ToString() + " min.";
            }
            else
            {
                str = idle.ToString() + " sec.";
            }
            return str;
        }

        public static uint GetIdleTime()
        {
            LASTINPUTINFO lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
            GetLastInputInfo(ref lastInPut);
            return ((uint)Environment.TickCount - lastInPut.dwTime);
        }

        public static long GetTickCount()
        {
            return Environment.TickCount;
        }

        public static long GetLastInputTime()
        {
            LASTINPUTINFO lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
            if (!GetLastInputInfo(ref lastInPut))
            {
                //throw new Exception(GetLastError().ToString());
            }

            return lastInPut.dwTime;
        }



    }
}
