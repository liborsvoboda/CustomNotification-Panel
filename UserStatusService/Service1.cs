using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Management;
using System.Management.Instrumentation;
using System.Web;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;
using System.Net.NetworkInformation;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Threading;
using System.Web.SessionState;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Reflection;
using NativeWifi;

namespace SensioUserStatusService
{



    public partial class Service1 : ServiceBase
    {

        [DllImport("Kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U4)]
        public static extern int WTSGetActiveConsoleSessionId();


        private static System.Timers.Timer timer = new System.Timers.Timer() { Enabled = false, Interval = 10000 };
        private ServiceController controller = new ServiceController("SensioUserStatusService");
        static HttpClient client = new HttpClient();
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly CancellationToken cancellationToken;
        private readonly List<ManualResetEvent> resetEvents;
        public static string setting_folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), AppDomain.CurrentDomain.FriendlyName.Split('.')[0].Replace("Service", ""));
        public static string loggedUserName = null;
        public static string logFile = "Service.log";
        public static string settingFile = "Service.json";
        public static string userConfig = "Application.json";
        public static App_Settings settings = new App_Settings();
        private static Api_Format waitingRecord = new Api_Format();
        private static UserName userName = new UserName();
        private static string loggeduser_id = null;


        public Service1()
        {
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
            resetEvents = new List<ManualResetEvent>();
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);

            InitializeComponent();
            base.CanHandleSessionChangeEvent = true;
        }


        protected override void OnStart(string[] args)
        {


            timer.Enabled = true;

            CheckTokenExist();

            Functions.loadSettings();
            userName = ReadUserName();

            Api_Format sendData = new Api_Format();
            sendData.username = userName.userName;
            sendData.status = 2;
            sendData.token = userName.Token;
            sendData.pcname = userName.PcName;
            sendData.user_id = userName.user_id;

            loggedUserName = userName.userName;
            loggeduser_id = userName.user_id;
            sendAPI(settings.apiUrl, sendData);

            if (settings.writeToLog)
            {
                Functions.WriteToFile(Path.Combine(setting_folder, logFile), "Service is started at " + DateTime.Now);
            }
        }


        protected override void OnStop()
        {
            timer.Enabled = false;
            if (settings.writeToLog)
            {
                Functions.WriteToFile(Path.Combine(setting_folder, logFile), "Service is stopped at " + DateTime.Now);
            }
        }


        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            if (waitingRecord.username != null) {
                userName = ReadUserName();
                loggedUserName = userName.userName;
                loggeduser_id = userName.user_id;

                waitingRecord.username = userName.userName;
                waitingRecord.token = userName.Token;
                waitingRecord.pcname = userName.PcName;
                waitingRecord.status = 2;
                waitingRecord.user_id = userName.user_id;

                sendAPI(settings.apiUrl, waitingRecord);
                if (settings.writeToLog)
                {
                    Functions.WriteToFile(Path.Combine(setting_folder, logFile), "Service is recall at " + DateTime.Now);
                }
                timer.Enabled = true;
            }

            if (!CheckProccess()) sendGetRequest();
        }

        //void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        //{
        //    if (e.Mode == PowerModes.Suspend)
        //    {
        //    }
        //}


        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            bool sendNow = false;
            Api_Format sendData = new Api_Format();

            switch (changeDescription.Reason)
            {
                case SessionChangeReason.SessionLogon:
                    sendData.status = 2;
                    sendNow = true;
                    break;

                case SessionChangeReason.SessionLogoff:
                    sendData.status = 1;
                    sendNow = true;
                    break;

                case SessionChangeReason.RemoteConnect:
                    sendData.status = 2;
                    sendNow = true;
                    break;

                case SessionChangeReason.RemoteDisconnect:
                    sendData.status = 1;
                    sendNow = true;
                    break;

                case SessionChangeReason.SessionLock:
                    sendData.status = 3;
                    sendNow = true;
                    break;

                case SessionChangeReason.SessionUnlock:
                    sendData.status = 2;
                    sendNow = true;
                    break;
                case SessionChangeReason.ConsoleConnect:
                    sendData.status = 2;
                    sendNow = true;
                    break;
                case SessionChangeReason.ConsoleDisconnect:
                    sendData.status = 1;
                    break;
                default:
                    break;
            }

            if (sendData.status == 2) {
                userName = ReadUserName();
                loggedUserName = userName.userName;
                loggeduser_id = userName.user_id;

                sendData.username = userName.userName;
                sendData.token = userName.Token;
                sendData.user_id = userName.user_id;
                sendData.pcname = userName.PcName;
            }
            else if (sendData.status == 1)
            {
                sendData.username = loggedUserName;
                sendData.user_id = loggeduser_id;
                sendData.token = Functions.getInstance().MD5(Environment.MachineName + sendData.username);
            } else
            {
                loggedUserName = userName.userName;
                loggeduser_id = userName.user_id;

                sendData.username = userName.userName;
                sendData.token = userName.Token;
                sendData.user_id = userName.user_id;
                sendData.pcname = userName.PcName;
            }

            if (sendNow)
            {
                sendAPI(settings.apiUrl, sendData);
                sendNow = false;
            }

            base.OnSessionChange(changeDescription);
        }


        private bool sendAPI(string apiUrl, Api_Format data)
        {
            if (data.username != "SYSTEM") {

                try
                {
                    Ping ping = new Ping();
                    PingReply pingReply = ping.Send(settings.serverIP);

                    if (pingReply.Status == IPStatus.Success)
                    {
                        waitingRecord = new Api_Format();

                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls;
                        ServicePointManager.ServerCertificateValidationCallback =
                        delegate (Object obj, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
                        {
                            return true;
                        };

                        HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(apiUrl);
                        httpRequest.ContentType = "application/json";
                        httpRequest.Method = "POST";
                        httpRequest.Credentials = CredentialCache.DefaultCredentials;

                        using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                        {
                            streamWriter.Write(new JavaScriptSerializer().Serialize(data));
                            streamWriter.Flush();
                            streamWriter.Close();
                        }
                        var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();
                            loggeduser_id = new JavaScriptSerializer().Deserialize<Api_Result>(result).user_id;
                            if (settings.writeToLog)
                            {
                                Functions.WriteToFile(Path.Combine(setting_folder, logFile), "resultOfPost: " + new JavaScriptSerializer().Serialize(data) + result);
                            }
                            UserName tempUsername = new UserName() { userName = data.username, user_id = loggeduser_id, Token = data.token, PcName = data.pcname };
                            writeUserToken(tempUsername);
                            return true;
                        }
                    }
                    else
                    {
                        waitingRecord = data;
                        return false;
                    }
                }
                catch (Exception e)
                {
                    waitingRecord = data;
                    return false;
                }
            }
            return false;
        }


        private String getUsername()
        {
            string username = null;
            try
            {
                //System.Security.Principal.WindowsIdentity.GetCurrent(False).Name
                ManagementScope ms = new ManagementScope("\\\\.\\root\\cimv2");
                ms.Connect();

                ObjectQuery query = new ObjectQuery
                        ("SELECT * FROM Win32_ComputerSystem");
                ManagementObjectSearcher searcher =
                        new ManagementObjectSearcher(ms, query);


                foreach (ManagementObject mo in searcher.Get())
                {
                    username = mo["UserName"].ToString();
                }
                string[] usernameParts = username.Split('\\');

                username = usernameParts[usernameParts.Length - 1];
            }
            catch (Exception)
            {

                username = WindowsIdentity.GetCurrent().Name.Split('\\')[1];
                return username;
            }
            return username;
        }


        private UserName ReadUserName()
        {
            UserName tempUser = new UserName() { userName = null };
            if (File.Exists(Path.Combine(setting_folder, userConfig)))
            {
                using (StreamReader sr = new StreamReader(Path.Combine(setting_folder, userConfig)))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (getUsername() == new JavaScriptSerializer().Deserialize<UserName>(line).userName)
                        {
                            loggeduser_id = new JavaScriptSerializer().Deserialize<UserName>(line).user_id;
                            return new JavaScriptSerializer().Deserialize<UserName>(line);
                        }
                    }
                    sr.Close();
                }
            }

            loggedUserName = getUsername();
            tempUser.userName = getUsername();
            tempUser.Token = Functions.getInstance().MD5(Environment.MachineName + getUsername());
            tempUser.PcName = Environment.MachineName;//Functions.getInstance().GetMacAddress();
            tempUser.user_id = loggeduser_id;
            return tempUser;
        }


        private bool writeUserToken(UserName tempUsername)
        {
            if (File.Exists(Path.Combine(setting_folder, userConfig)))
            {
                using (StreamReader sr = new StreamReader(Path.Combine(setting_folder, userConfig)))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (tempUsername.userName == new JavaScriptSerializer().Deserialize<UserName>(line).userName)
                        {
                            return true;
                        }
                    }
                    sr.Close();
                }
            }
            Functions.WriteToFile(Path.Combine(setting_folder, userConfig), new JavaScriptSerializer().Serialize(tempUsername));
            return true;
        }


        private bool CheckProccess()
        {
            string[] propertiesToSelect = new[] { "Handle", "ProcessId" };
            SelectQuery processQuery = new SelectQuery("Win32_Process", "Name = '" + AppDomain.CurrentDomain.FriendlyName.Replace("Service", "") + "'", propertiesToSelect);

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

                        if (loggedUserName == user)
                        {
                            return true;
                        }
                    }
                }
            return false;
        }


        private static void sendGetRequest()
        {
            try
            {
                Ping ping = new Ping();
                PingReply pingReply = ping.Send(settings.serverIP);

                if (pingReply.Status == IPStatus.Success)
                {
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls;
                    ServicePointManager.ServerCertificateValidationCallback =
                    delegate (Object obj, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
                    {
                        return true;
                    };

                    HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(settings.getUrl);
                    httpRequest.ContentType = "application/json";
                    httpRequest.Method = "POST";
                    httpRequest.Credentials = CredentialCache.DefaultCredentials;

                    GetToken tempToken = new GetToken() { token = userName.Token, user_id = loggeduser_id, wifissid = getWifiSsid(), installed_version = Assembly.GetEntryAssembly().GetName().Version.ToString().Substring(0, Assembly.GetEntryAssembly().GetName().Version.ToString().Length - 2) };
                    using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                    {
                        streamWriter.Write(new JavaScriptSerializer().Serialize(tempToken));
                        streamWriter.Flush();
                        streamWriter.Close();
                    }

                    var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        streamReader.Close();
                    }
                }
            }
            catch (Exception e)
            {

            }
        }


        private static void CheckTokenExist()
        {
            if (!Directory.Exists(setting_folder))
            {
                Directory.CreateDirectory(setting_folder);
                DirectoryInfo FolderInfo = new DirectoryInfo(setting_folder);
                DirectorySecurity FolderAcl = new DirectorySecurity();
                FolderAcl.AddAccessRule(new FileSystemAccessRule("Users", FileSystemRights.Modify, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                FolderInfo.SetAccessControl(FolderAcl);
            }

            if (!File.Exists(Path.Combine(setting_folder, userConfig)))
            {
                File.Create(Path.Combine(setting_folder, userConfig)).Close();
            }
        }


        private static string getWifiSsid()
        {
            string ret = null;

            try
            {
                using (WlanClient wlan = new WlanClient())
                {
                    foreach (WlanClient.WlanInterface wlanInterface in wlan.Interfaces)
                    {
                        if (wlanInterface.InterfaceState == Wlan.WlanInterfaceState.Connected)
                        {
                            Wlan.Dot11Ssid ssid = wlanInterface.CurrentConnection.wlanAssociationAttributes.dot11Ssid;
                            ret = Encoding.ASCII.GetString(ssid.SSID, 0, (int)ssid.SSIDLength);
                        }
                    }
                }
            }
            catch
            {
                ret = null;
            }
            return ret;
        }

    }
}


