using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyKTV_WebUpdater
{
    class Global
    {
        public static string WebUpdaterFile = AppDomain.CurrentDomain.BaseDirectory + @"\CrazyKTV_WebUpdater.ver";
        public static string WebUpdaterTempFile = AppDomain.CurrentDomain.BaseDirectory + @"\CrazyKTV_WebUpdater.tmp";
        public static string WebUpdaterUrl = "https://raw.githubusercontent.com/KenLuoTW/CrazyKTVSongMgr/master/CrazyKTV_WebUpdater/UpdateFile/CrazyKTV_WebUpdater.ver";
        public static string WebUpdaterCSS = "https://raw.githubusercontent.com/KenLuoTW/CrazyKTVSongMgr/master/CrazyKTV_WebUpdater/UpdateFile/CrazyKTV_WebUpdater.css";
        public static string WebUpdaterLogUrl = "https://raw.githubusercontent.com/KenLuoTW/CrazyKTVSongMgr/master/CrazyKTV_WebUpdater/UpdateFile/CrazyKTV_WebUpdater.md";
        public static string WebUpdaterHtmlFile = AppDomain.CurrentDomain.BaseDirectory + @"\CrazyKTV_WebUpdater.html";
        public static string WebUpdaterMDFile = AppDomain.CurrentDomain.BaseDirectory + @"\CrazyKTV_WebUpdater.md";

        public static List<List<string>> LocaleVerList = new List<List<string>>();
        public static List<List<string>> RemoteVerList = new List<List<string>>();
    }
}
