using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrazyKTV_WebUpdater
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            bool DownloadStatus = DownloadFile(Global.WebUpdaterHtmlFile, Global.WebUpdaterHtml, false);
            if (DownloadStatus)
            {
                if (File.Exists(Global.WebUpdaterHtmlFile))
                {
                    if (!File.Exists(Global.WebUpdaterCSSFile))
                    {
                        DownloadFile(Global.WebUpdaterCSSFile, Global.WebUpdaterCSS, false);
                    }
                    string curDir = Directory.GetCurrentDirectory();
                    WebBrowser1.Source = new Uri(string.Format("file:///{0}/CrazyKTV_WebUpdater.html", curDir));
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string CurVer = " v" + FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileMajorPart + "." +
                                   FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileMinorPart + "." +
                                   FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileBuildPart;

            if (FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FilePrivatePart > 0) CurVer += "." + FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FilePrivatePart;

            
            this.Title += CurVer;

            bool RebuildFile = false;
            if (!File.Exists(Global.WebUpdaterFile))
            {
                CommonFunc.CreateVersionXmlFile(Global.WebUpdaterFile);
                CommonFunc.SaveVersionXmlFile(Global.WebUpdaterFile, "VersionInfo", "20150831001", "", "", "版本日期及資訊");
                RebuildFile = true;
            }
            Global.LocaleVerList = CommonFunc.ScanVersionXmlFile(Global.WebUpdaterFile);

            bool DownloadStatus = DownloadFile(Global.WebUpdaterTempFile, Global.WebUpdaterUrl, false);
            if (DownloadStatus)
            {
                if (File.Exists(Global.WebUpdaterTempFile))
                {
                    Global.RemoteVerList = CommonFunc.ScanVersionXmlFile(Global.WebUpdaterTempFile);
                    File.Delete(Global.WebUpdaterTempFile);

                    if (Convert.ToInt64(Global.RemoteVerList[0][1]) > Convert.ToInt64(Global.LocaleVerList[0][1]) || RebuildFile)
                    {
                        if (RebuildFile)
                        {
                            Task.Factory.StartNew(() => UpdateFileTask());
                        }
                        else
                        {
                            if (MessageBox.Show("你確定要更新檔案嗎?", "偵測到 CrazyKTV 版本更新", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                Task.Factory.StartNew(() => UpdateFileTask());
                            }
                            else
                            {
                                label1.Content = "你的 CrazyKTV 還未更新至最新版本。";
                            }
                        }
                    }
                    else
                    {
                        label1.Content = "你的 CrazyKTV 已是最新版本。";
                    }
                }
            }
            else
            {
                File.Delete(Global.WebUpdaterTempFile);
                label1.Content = "暫時無法取得網路上的更新資料,請稍後再試。";
            }
        }

        private void UpdateFileTask()
        {
            string UnFolderFileArguments = "-y";

            Dispatcher.Invoke(DispatcherPriority.Background, new Action<ProgressBar, string, int>(CommonFunc.UpdateProgressBar), progressBar2, "Maximum", Global.RemoteVerList.Count);

            List<string> LocaleNameList = new List<string>();
            foreach (List<string> list in Global.LocaleVerList)
            {
                LocaleNameList.Add(list[0]);
            }

            foreach (List<string> list in Global.RemoteVerList)
            {
                Dispatcher.Invoke(DispatcherPriority.Background, new Action<Label, string, string>(CommonFunc.UpdateLabel), label1, "Content", "正在檢查更新檔案,請稍待...");
                Dispatcher.Invoke(DispatcherPriority.Background, new Action<ProgressBar, string, int>(CommonFunc.UpdateProgressBar), progressBar2, "Value", Global.RemoteVerList.IndexOf(list) + 1);

                bool UpdateFile = false;
                int LocaleListIndex = LocaleNameList.IndexOf(list[0]);

                if (LocaleListIndex >= 0)
                {
                    if (Convert.ToInt64(list[1]) > Convert.ToInt64(Global.LocaleVerList[LocaleListIndex][1]))
                    {
                        UpdateFile = true;
                    }
                }
                else
                {
                    UpdateFile = true;
                }

                if (UpdateFile)
                {
                    Dispatcher.Invoke(DispatcherPriority.Background, new Action<Label, string, string>(CommonFunc.UpdateLabel), label1, "Content", "正在下載 " + list[0] + " 更新檔案...");

                    CommonFunc.SaveVersionXmlFile(Global.WebUpdaterFile, list[0], list[1], list[2], list[3], list[4]);
                    if (list[0] != "VersionInfo")
                    {
                        switch (list[0])
                        {
                            case "CrazySong.mdb":
                                if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\CrazySong.mdb"))
                                {
                                    DownloadFile(list[0], list[2], true);
                                }
                                break;
                            case "Folder_Codec.zip":
                                if (Environment.OSVersion.Version.Major >= 6)
                                {
                                    DownloadFile(list[0], list[2], true);
                                }
                                else
                                {
                                    DownloadFile(list[0], Global.CodecXPUrl, true);
                                }
                                break;
                            default:
                                if (list[3] == "")
                                {
                                    DownloadFile(list[0], list[2], true);
                                }
                                else
                                {
                                    string FilePath = AppDomain.CurrentDomain.BaseDirectory + @"\" + list[3];
                                    if (!Directory.Exists(FilePath)) Directory.CreateDirectory(FilePath);
                                    DownloadFile(FilePath + @"\" + list[0], list[2], true);
                                }
                                break;
                        }
                    }
                    else
                    {
                        if (list[2] != "") UnFolderFileArguments = list[2];
                    }
                }
            }

            Dispatcher.Invoke(DispatcherPriority.Background, new Action<Label, string, string>(CommonFunc.UpdateLabel), label1, "Content", "正在解壓檔案,請稍待...");

            List<string> FolderFileList = new List<string>()
            {
                AppDomain.CurrentDomain.BaseDirectory + @"\Folder_BackGround.zip",
                AppDomain.CurrentDomain.BaseDirectory + @"\Folder_BMP.zip",
                AppDomain.CurrentDomain.BaseDirectory + @"\Folder_Codec.zip",
                AppDomain.CurrentDomain.BaseDirectory + @"\Folder_Favorite.zip",
                AppDomain.CurrentDomain.BaseDirectory + @"\Folder_Lang.zip",
                AppDomain.CurrentDomain.BaseDirectory + @"\Folder_SongMgr.zip",
                AppDomain.CurrentDomain.BaseDirectory + @"\Folder_Web.zip",
            };

            ReadOptions opt = new ReadOptions();
            opt.Encoding = Encoding.Default;

            foreach (string file in FolderFileList)
            {
                using (var zip = ZipFile.Read(file, opt))
                {
                    zip.ExtractExistingFile = ExtractExistingFileAction.OverwriteSilently;
                    zip.ExtractAll(AppDomain.CurrentDomain.BaseDirectory);
                }
                File.Delete(file);
            }

            Dispatcher.Invoke(DispatcherPriority.Background, new Action<Label, string, string>(CommonFunc.UpdateLabel), label1, "Content", "已完成檔案更新。");
        }

        private bool DownloadFile(string File, string Url, bool UseProgBar)
        {
            bool DownloadStatus = false;
            FileStream FStream = new FileStream(File, FileMode.Create);

            try
            {
                HttpWebRequest Request = (HttpWebRequest)HttpWebRequest.Create(Url);
                HttpWebResponse Response = (HttpWebResponse)Request.GetResponse();

                long FileSize = Response.ContentLength;

                if (UseProgBar)
                {
                    Dispatcher.Invoke(DispatcherPriority.Background, new Action<ProgressBar, string, int>(CommonFunc.UpdateProgressBar), progressBar1, "Maximum", (int)FileSize);
                }

                Stream DataStream = Response.GetResponseStream();
                byte[] Databuffer = new byte[8192];
                int CompletedLength = 0;
                long TotalDLByte = 0;

                while ((CompletedLength = DataStream.Read(Databuffer, 0, 8192)) > 0)
                {
                    TotalDLByte += CompletedLength;
                    FStream.Write(Databuffer, 0, CompletedLength);
                    if (UseProgBar)
                    {
                        Dispatcher.Invoke(DispatcherPriority.Background, new Action<ProgressBar, string, int>(CommonFunc.UpdateProgressBar), progressBar1, "Value", (int)TotalDLByte);
                    }
                }
                FStream.Close();
                DataStream.Close();
                Response.Close();
                DownloadStatus = true;
            }
            catch
            {
                FStream.Close();
                DownloadStatus = false;
            }
            return DownloadStatus;
        }
    }
}
