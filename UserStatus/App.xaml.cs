using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SensioUserStatus
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("user32", CharSet = CharSet.Unicode)]
        static extern IntPtr FindWindow(string cls, string win);
        [DllImport("user32")]
        static extern IntPtr SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32")]
        static extern bool IsIconic(IntPtr hWnd);
        [DllImport("user32")]
        static extern bool OpenIcon(IntPtr hWnd);


        protected override void OnStartup(StartupEventArgs e)
        {
            bool isNew;
            var mutex = new Mutex(true, AppDomain.CurrentDomain.FriendlyName.Split('.')[0], out isNew);
            if (CheckProccess())
            {
                MessageBox.Show("Aplikace je již spuštìna.","Sensio User Status");
                Shutdown();
            }
        }

        private bool CheckProccess()
        {
            string[] propertiesToSelect = new[] { "Handle", "ProcessId" };
            SelectQuery processQuery = new SelectQuery("Win32_Process", "Name = '"+ AppDomain.CurrentDomain.FriendlyName + "'", propertiesToSelect);
            Process p = Process.GetCurrentProcess();
            

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(processQuery))
            using (ManagementObjectCollection processes = searcher.Get())
                foreach (ManagementObject process in processes)
                {
                    object[] outParameters = new object[2];
                    uint result = (uint)process.InvokeMethod("GetOwner", outParameters);

                    if (result == 0)
                    {
                        string user = (string)outParameters[0];
                        string domain = (string)outParameters[1];
                        uint processId = (uint)process["ProcessId"];

                        if (WindowsIdentity.GetCurrent().Name.Contains("\\"+user) && p.Id != processId)
                        {
                            return true;
                        }
                    }
                }
            return false;
        }

    }
}