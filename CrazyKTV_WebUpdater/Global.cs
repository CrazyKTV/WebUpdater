using System;
using System.Collections.Generic;

namespace CrazyKTV_WebUpdater
{
    class Global
    {
        public static string WebUpdaterFile = AppDomain.CurrentDomain.BaseDirectory + @"\CrazyKTV_WebUpdater.ver";
        public static string WebUpdaterUrl = "https://raw.githubusercontent.com/CrazyKTV/WebUpdater/master/CrazyKTV_WebUpdater/UpdateFile/CrazyKTV_WebUpdater.106";
        public static string CodecXPUrl = "https://raw.githubusercontent.com/CrazyKTV/WebUpdater/master/CrazyKTV_WebUpdater/UpdateFile/Folder_Codec_XP.zip";

        public static string WebUpdaterHtml = "https://raw.githubusercontent.com/CrazyKTV/WebUpdater/master/CrazyKTV_WebUpdater/UpdateFile/CrazyKTV_WebUpdater.html";
        public static string WebUpdaterHtmlFile = AppDomain.CurrentDomain.BaseDirectory + @"\CrazyKTV_WebUpdater.html";
        public static string WebUpdaterCSS = "https://raw.githubusercontent.com/CrazyKTV/WebUpdater/master/CrazyKTV_WebUpdater/UpdateFile/CrazyKTV_WebUpdater.css";
        public static string WebUpdaterCSSFile = AppDomain.CurrentDomain.BaseDirectory + @"\CrazyKTV_WebUpdater.css";

        public static List<List<string>> LocaleVerList = new List<List<string>>();
        public static List<List<string>> RemoteVerList = new List<List<string>>();
    }
}
