using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensioUserStatusService
{
    class Api_Format
    {
        public string token { get; set; }
        public string pcname { get; set; }
        public string username { get; set; }
        public string user_id { get; set; }
        public int status { get; set; }
    }

    public class App_Settings
    {
        public string serverIP { get; set; }
        public string apiUrl { get; set; }
        public string regUrl { get; set; }
        public string getUrl { get; set; }
        public string updSource { get; set; }
        public string settingsUrl { get; set; }
        public string dashboardUrl { get; set; }
        public bool writeToLog { get; set; }
    }

    public class GetToken
    {
        public string token { get; set; }
        public string user_id { get; set; }
        public string installed_version { get; set; }
        public string wifissid { get; set; }
    }

    class Get_Result
    {
        public string message { get; set; }
        public Int32 result { get; set; }
        public string name { get; set; }
        public string user_id { get; set; }
        //public DateTime timestamp { get; set; }
        public string lastversion { get; set; }
        public Int32 offline_time { get; set; }
        public Int32 dnd_time { get; set; }


    }

    class Get_BasicResult
    {
        public string message { get; set; }
        public Int32 result { get; set; }
        public string lastversion { get; set; }
    }

    public class UserName
    {
        public string userName { get; set; }
        public string Token { get; set; }
        public string user_id { get; set; }
        public string PcName { get; set; }

    }

    class Api_Result
    {
        public string message { get; set; }
        public string name { get; set; }
        public string user_id { get; set; }
        public DateTime timestamp { get; set; }

    }
}

