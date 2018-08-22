using System;
using System.Collections.Generic;

namespace CrazyKTV_WebUpdater
{
    class Global
    {
        public static string WebUpdaterFile = AppDomain.CurrentDomain.BaseDirectory + @"\CrazyKTV_WebUpdater.ver";
        public static string WebUpdaterUrl = "https://raw.githubusercontent.com/CrazyKTV/WebUpdater/master/CrazyKTV_WebUpdater/UpdateFile/CrazyKTV_WebUpdater.ver";
        public static string CodecXPUrl = "https://raw.githubusercontent.com/CrazyKTV/WebUpdater/master/CrazyKTV_WebUpdater/UpdateFile/Folder_Codec_XP.zip";
        public static string FFmpegXPUrl = "https://raw.githubusercontent.com/CrazyKTV/WebUpdater/master/CrazyKTV_WebUpdater/UpdateFile/Folder_FFmpeg_XP.zip";
        public static string WebUpdaterLogUrl = "https://raw.githubusercontent.com/CrazyKTV/WebUpdater/master/CrazyKTV_WebUpdater/UpdateFile/CrazyKTV_WebUpdater.xaml";

        public static List<List<string>> LocaleVerList = new List<List<string>>();
        public static List<List<string>> RemoteVerList = new List<List<string>>();
    }
}
