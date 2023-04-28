using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using SensioUserStatus.Classes;
using System.Web.Script.Serialization;
using System.Security.Principal;
using System.Management;
using System.Timers;
using System.Security.AccessControl;
using SensioUserStatus;
using System.Reflection;
using NativeWifi;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class Functions
{

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("Kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.U4)]
    public static extern int WTSGetActiveConsoleSessionId();

    [DllImport("Wtsapi32.dll")]
    public static extern bool WTSQuerySessionInformation(IntPtr hServer, int sessionId, WtsInfoClass wtsInfoClass, out System.IntPtr ppBuffer, out int pBytesReturned);
    [DllImport("Wtsapi32.dll")]
    public static extern void WTSFreeMemory(IntPtr pointer);

    public enum WtsInfoClass
    {
        WTSInitialProgram,
        WTSApplicationName,
        WTSWorkingDirectory,
        WTSOEMId,
        WTSSessionId,
        WTSUserName,
        WTSWinStationName,
        WTSDomainName,
        WTSConnectState,
        WTSClientBuildNumber,
        WTSClientName,
        WTSClientDirectory,
        WTSClientProductId,
        WTSClientHardwareId,
        WTSClientAddress,
        WTSClientDisplay,
        WTSClientProtocolType,
        WTSIdleTime,
        WTSLogonTime,
        WTSIncomingBytes,
        WTSOutgoingBytes,
        WTSIncomingFrames,
        WTSOutgoingFrames,
        WTSClientInfo,
        WTSSessionInfo,
    }


    private static string lastVersion = "";
    public static string setting_folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), AppDomain.CurrentDomain.FriendlyName.Split('.')[0]);
    public static string userConfig = "Application.json";
    private static string logFile = "Application.log";
    public static string settingFile = "Service.json";
    private static Object _classLocker = new Object();
    private static Functions _machine;
    public static Timer sendApiTimer = new Timer() { Enabled = false, Interval = MainNotifyWindow.interval };
    public static Timer getApiTimer = new Timer() { Enabled = true, Interval = 1000 };
    protected const int MIN_MAC_ADDR_LENGTH = 12;
    public static App_Settings settings = new App_Settings();
    public static bool offlineStatus = false;
   

    //private Functions()
    //{
    //}

    //public static Functions getInstance()
    //{
    //    if (_machine == null)
    //    {
    //        lock (_classLocker)
    //        {
    //            if (_machine == null)
    //            {
    //                _machine = new Functions();
    //            }
    //        }
    //    }
    //    return _machine;
    //}


    public static string GetMacAddress()
    {
        const int MIN_MAC_ADDR_LENGTH = 12;
        string macAddress = string.Empty;
        long maxSpeed = -1;

        foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
        {

            string tempMac = nic.GetPhysicalAddress().ToString();
            if (nic.Speed > maxSpeed &&
                !string.IsNullOrEmpty(tempMac) &&
                tempMac.Length >= MIN_MAC_ADDR_LENGTH)
            {
                maxSpeed = nic.Speed;
                macAddress = tempMac;
            }
        }

        return macAddress;
    }


    public static String MD5(string s)
    {
        using (var provider = System.Security.Cryptography.MD5.Create())
        {
            StringBuilder builder = new StringBuilder();

            foreach (byte b in provider.ComputeHash(Encoding.UTF8.GetBytes(s)))
                builder.Append(b.ToString("x2").ToLower());

            return builder.ToString();
        }
    }


    private static string GetUsernameBySessionId(int sessionId, bool prependDomain)
    {
        IntPtr buffer;
        int strLen;
        string username = "SYSTEM";
        if (WTSQuerySessionInformation(IntPtr.Zero, sessionId, WtsInfoClass.WTSUserName, out buffer, out strLen) && strLen > 1)
        {
            username = Marshal.PtrToStringAnsi(buffer);
            WTSFreeMemory(buffer);
            if (prependDomain)
            {
                if (WTSQuerySessionInformation(IntPtr.Zero, sessionId, WtsInfoClass.WTSDomainName, out buffer, out strLen) && strLen > 1)
                {
                    username = Marshal.PtrToStringAnsi(buffer) + "\\" + username;
                    WTSFreeMemory(buffer);
                }
            }
        }
        return username;
    }


    private static String getUsername()
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


    private static string GetMacAddressList()
    {
        string macAddressList = string.Empty;
        long maxSpeed = -1;

        foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            string tempMac = nic.GetPhysicalAddress().ToString();
            if (nic.Speed > maxSpeed &&
                !string.IsNullOrEmpty(tempMac) &&
                tempMac.Length >= MIN_MAC_ADDR_LENGTH)
            {
                if (String.IsNullOrEmpty(macAddressList))
                {
                    macAddressList = tempMac;
                }
                else
                {
                    macAddressList += "," + tempMac;
                }
            }
        }
        return macAddressList;
    }


    private static string EncodeTo64(string toEncode)
    {

        //byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);
        byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes(toEncode);
        string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
        return returnValue;
    }

    private static string DecodetoUtf8(string toUtf8)
    {
        byte[] bytes = Encoding.Default.GetBytes(toUtf8);
        return Encoding.UTF8.GetString(bytes);
    }


    public static void OpenDasboard()
    {
        System.Diagnostics.Process.Start(settings.dashboardUrl);
    }

    public static void OpenBrowser()
    {
        System.Diagnostics.Process.Start(settings.regUrl + EncodeTo64(Environment.MachineName + ";" + MainNotifyWindow.userName.userName) + "&submit=1");
    }

    public static void RunUpdate()
    {
        System.Diagnostics.Process.Start(settings.updSource+"/"+ AppDomain.CurrentDomain.FriendlyName.Split('.')[0]+ lastVersion + ".msi");
    }

    public static void CheckTokenExist()
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
        GetToken();
    }


    public static void GetToken()
    {
        MainNotifyWindow.userName = ReadUserName();
        if (String.IsNullOrEmpty(MainNotifyWindow.userName.userName))
        {
            Api_Format sendData = new Api_Format();
            sendData.username = getUsername();
            sendData.token = MD5(Environment.MachineName + getUsername());
            sendData.pcname = Environment.MachineName;
            sendData.status = 2;
            sendData.user_id = MainNotifyWindow.user_id;

            if (!sendAPI(settings.apiUrl, sendData))
            {
                sendApiTimer.Enabled = true;
                MainNotifyWindow.SetNotifyIconStat("Unlogged", 0);
                MainNotifyWindow.enableChangeStatus();
            }
            else
            {
                sendApiTimer.Enabled = false;
                MainNotifyWindow.userName.userName = sendData.username;
                MainNotifyWindow.userName.Token = sendData.token;
                MainNotifyWindow.userName.user_id = MainNotifyWindow.user_id;
                MainNotifyWindow.userName.PcName = sendData.pcname;
                writeUserToken(MainNotifyWindow.userName);
                MainNotifyWindow.SetNotifyIconStat("Green", 2);
                MainNotifyWindow.enableChangeStatus();
            }

        }
        else
        {
            MainNotifyWindow.SetNotifyIconStat("Green", 2);
            MainNotifyWindow.enableChangeStatus();
        }
    }


    private static void WriteToFile(string file, string Message)
    {
        if (!File.Exists(file))
        {
            using (StreamWriter sw = File.CreateText(file))
            {
                sw.WriteLine(Message);
                sw.Close();
            }
        }
        else
        {
            using (StreamWriter sw = File.AppendText(file))
            {
                sw.WriteLine(Message);
                sw.Close();
            }
        }
    }


    private static UserName ReadUserName()
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
                        MainNotifyWindow.user_id = new JavaScriptSerializer().Deserialize<UserName>(line).user_id;
                        return new JavaScriptSerializer().Deserialize<UserName>(line);
                    }
                }
                sr.Close();
            }
        }
        return tempUser;
    }


    public static bool sendAPI(string apiUrl, Api_Format data)
    {
        if (data.username != "SYSTEM")
        {
            try
            {
                Ping ping = new Ping();
                PingReply pingReply = ping.Send(settings.serverIP);

                if (pingReply.Status == IPStatus.Success)
                {
                    offlineStatus = false;
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
                        streamReader.Close();
                        if (settings.writeToLog)
                        {
                            WriteToFile(Path.Combine(setting_folder, logFile), "resultOfPost: " + new JavaScriptSerializer().Serialize(data) + new JavaScriptSerializer().Deserialize<Api_Result>(result).message);
                        }
                        if (String.IsNullOrEmpty(new JavaScriptSerializer().Deserialize<Api_Result>(result).message))
                            return false;
                        else
                        {
                            MainNotifyWindow.user_id = new JavaScriptSerializer().Deserialize<Api_Result>(result).user_id;
                            MainNotifyWindow.userName.user_id = new JavaScriptSerializer().Deserialize<Api_Result>(result).user_id;
                            MainNotifyWindow.fullUserName = new JavaScriptSerializer().Deserialize<Api_Result>(result).name;
                            return true;
                        }
                    }
                }
                else offlineStatus = true;
            }
            catch (Exception e)
            {
                offlineStatus = true;
                return false;
            }
        }
        return false;
    }

    public static void loadSettings()
    {
        if (File.Exists(Path.Combine(setting_folder, settingFile)))
        {
            using (StreamReader sr = new StreamReader(Path.Combine(setting_folder, settingFile)))
            {
                string line = sr.ReadToEnd();
                settings = new JavaScriptSerializer().Deserialize<App_Settings>(line);
                sr.Close();
            }
        }
    }

    public static void OnElapsedTime(object source, ElapsedEventArgs e)
    {
        Api_Format sendData = new Api_Format();
        sendData.username = getUsername();
        sendData.token = MD5(Environment.MachineName + getUsername());
        sendData.pcname = Environment.MachineName;
        sendData.status = 2;
        sendData.user_id = MainNotifyWindow.user_id;
        UserName tempUsername = new UserName() { userName = sendData.username, user_id = sendData.user_id, Token = sendData.token, PcName = sendData.pcname };

        if (sendAPI(settings.apiUrl, sendData))
        {
            sendApiTimer.Enabled = false;
            tempUsername.user_id = MainNotifyWindow.user_id;
            writeUserToken(tempUsername);
            MainNotifyWindow.userName = tempUsername;
            MainNotifyWindow.enableChangeStatus();
            MainNotifyWindow.SetNotifyIconStat("Green", 2);

        }
        else
        {
            sendApiTimer.Enabled = true;
            MainNotifyWindow.user_id = null;
            MainNotifyWindow.userName.userName = null;
            MainNotifyWindow.enableChangeStatus();
            MainNotifyWindow.SetNotifyIconStat("Unlogged", 0);

        }
    }
    public static void GetElapsedTime(object source, ElapsedEventArgs e)
    { 
        try
        {
            getApiTimer.Interval = MainNotifyWindow.interval;

            Ping ping = new Ping();
            PingReply pingReply = ping.Send(settings.serverIP);

        if (pingReply.Status == IPStatus.Success)
        {
            if (MainNotifyWindow.settingChanged) { sendSetting(); }

                offlineStatus = false;
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

                GetToken tempToken = new GetToken() { token = MainNotifyWindow.userName.Token, user_id = MainNotifyWindow.userName.user_id, wifissid = getWifiSsid(), installed_version = Assembly.GetEntryAssembly().GetName().Version.ToString().Substring(0, Assembly.GetEntryAssembly().GetName().Version.ToString().Length - 2) };
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

                    if (new JavaScriptSerializer().Deserialize<Get_BasicResult>(result).result != 0)
                    {
                        string IconPrefix = "";
                        //int IconStatus = 0;
                        switch ((new JavaScriptSerializer().Deserialize<Get_Result>(result).result))
                        {
                            case 0:
                                IconPrefix = "Unlogged";
                                //IconStatus = 0;
                                break;
                            case 1:
                                IconPrefix = "Red";
                                //IconStatus = 1;
                                break;
                            case 2:
                                IconPrefix = "Green";
                                // IconStatus = 2;
                                break;
                            case 3:
                                IconPrefix = "Amber";
                                // IconStatus = 3;
                                break;
                            case 4:
                                IconPrefix = "Purple";
                                // IconStatus = 3;
                                break;
                            default:
                                break;
                        }
                        MainNotifyWindow.fullUserName = new JavaScriptSerializer().Deserialize<Get_Result>(result).name;

                        if (MainNotifyWindow.RunButtonVisibilityStat == "Visible") { 
                            MainNotifyWindow.SetDndValueStat = new JavaScriptSerializer().Deserialize<Get_Result>(result).dnd_time.ToString();
                            MainNotifyWindow.SetOfflineValueStat = new JavaScriptSerializer().Deserialize<Get_Result>(result).offline_time.ToString();
                        }
                        MainNotifyWindow.SetNotifyIconStat(IconPrefix, new JavaScriptSerializer().Deserialize<Get_Result>(result).result);
                        MainNotifyWindow.enableChangeStatus();
                    } else
                    {
                        MainNotifyWindow.user_id = null;
                        MainNotifyWindow.fullUserName = "Neregistrovaný Uživatel";
                        MainNotifyWindow.SetNotifyIconStat("Unlogged",0);
                        MainNotifyWindow.enableChangeStatus();
                    }

                    if (Convert.ToInt32(Assembly.GetEntryAssembly().GetName().Version.ToString().Substring(0, Assembly.GetEntryAssembly().GetName().Version.ToString().Length - 2).Replace(".", "")) < Convert.ToInt32(new JavaScriptSerializer().Deserialize<Get_Result>(result).lastversion.Replace(".", "")))
                    {
                        MainNotifyWindow.UpdateButtonToolTip = "Je dostupná nová verze: " + new JavaScriptSerializer().Deserialize<Get_Result>(result).lastversion;
                        lastVersion = new JavaScriptSerializer().Deserialize<Get_Result>(result).lastversion;
                        MainNotifyWindow.UpdateButtonStat = true;
                    }
                    else
                    {
                        MainNotifyWindow.UpdateButtonToolTip = "";
                        MainNotifyWindow.UpdateButtonStat = false;
                    }
                }

                    
            }
        }
        catch (Exception ex)
        {
            MainNotifyWindow.user_id = MainNotifyWindow.userName.user_id;
            MainNotifyWindow.fullUserName = MainNotifyWindow.userName.userName;
            offlineStatus = true;
            MainNotifyWindow.SetNotifyIconStat("Unlogged", 0);
            MainNotifyWindow.enableChangeStatus();
        }
    }

    private static void sendSetting()
    {
        if (!String.IsNullOrWhiteSpace(MainNotifyWindow.setDndValueStat) && !String.IsNullOrWhiteSpace(MainNotifyWindow.setOfflineValueStat)) {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls;
            ServicePointManager.ServerCertificateValidationCallback =
            delegate (Object obj, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
            {
                return true;
            };

            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(settings.settingsUrl);
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "POST";
            httpRequest.Credentials = CredentialCache.DefaultCredentials;

            SettingToken tempToken = new SettingToken() { token = MainNotifyWindow.userName.Token,user_id = MainNotifyWindow.userName.user_id, dnd_time = Convert.ToInt32(MainNotifyWindow.setDndValueStat), offline_time = Convert.ToInt32(MainNotifyWindow.setOfflineValueStat) };
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

                if (new JavaScriptSerializer().Deserialize<Get_BasicResult>(result).message == "success")
                {
                    MainNotifyWindow.settingChanged = false;
                }
            }
        }
    }

    private static bool writeUserToken(UserName tempUsername)
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


    public static string getWifiSsid()
    {
        string ret = "aha";

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

