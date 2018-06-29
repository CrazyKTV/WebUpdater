using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Threading;
using System.Xml;

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
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string CurVer = " v" + FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileMajorPart + "." +
                                   FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileMinorPart + "." +
                                   FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileBuildPart;

            if (FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FilePrivatePart > 0) CurVer += "." + FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FilePrivatePart;
            this.Title += CurVer;

            using (MemoryStream ms = Download(Global.WebUpdaterLogUrl, false))
            {
                if (ms.Length > 0)
                {
                    ms.Position = 0;
                    using (StreamReader sr = new StreamReader(ms))
                    {
                        string content = sr.ReadToEnd();
                        using (StringReader strReader = new StringReader(content))
                        {
                            using (XmlReader xr = XmlReader.Create(strReader))
                            {
                                FlowDocument fdoc = (FlowDocument)XamlReader.Load(xr);
                                CommonFunc.SubscribeToAllHyperlinks(fdoc);
                                FlowDocument1.Document = fdoc;
                            }
                        }
                    }
                }
                else
                {
                    label1.Content = "暫時無法取得網路上的更新記錄,請稍後再試。";
                }
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            bool RebuildFile = false;
            if (!File.Exists(Global.WebUpdaterFile)) RebuildFile = true;

            if (RebuildFile)
            {
                bool DownloadFile = false;
                string dir = AppDomain.CurrentDomain.BaseDirectory;

                if (Directory.GetFileSystemEntries(dir).Length > 1 || Directory.GetDirectories(dir).Length > 0)
                {
                    if (MessageBox.Show("這將會在目前資料夾下載並覆蓋 CrazyKTV 所有檔案！"+ Environment.NewLine　+ "你確定要下載 CrazyKTV 所有檔案嗎？", "偵測到未在空資料夾", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        DownloadFile = true;
                    }
                    else
                    {
                        label1.Content = "已取消下載 CrazyKTV 所有檔案。";
                    }
                }
                else
                {
                    if (MessageBox.Show("你確定要下載 CrazyKTV 所有檔案嗎?", "下載 CrazyKTV 所有檔案", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        DownloadFile = true;
                    }
                    else
                    {
                        label1.Content = "已取消下載 CrazyKTV 所有檔案。";
                    }
                }

                if (DownloadFile)
                {
                    if (!File.Exists(Global.WebUpdaterFile))
                    {
                        CommonFunc.CreateVersionXmlFile(Global.WebUpdaterFile);
                        CommonFunc.SaveVersionXmlFile(Global.WebUpdaterFile, "VersionInfo", "20150831001", "", "", "版本日期及資訊");
                    }
                    Global.LocaleVerList = CommonFunc.ScanVersionXmlFile(Global.WebUpdaterFile, null, false);

                    using (MemoryStream ms = Download(Global.WebUpdaterUrl, false))
                    {
                        if (ms.Length > 0)
                        {
                            Global.RemoteVerList = CommonFunc.ScanVersionXmlFile("", ms, true);
                        }
                        else
                        {
                            label1.Content = "暫時無法取得網路上的更新資料,請稍後再試。";
                        }
                    }
                    Task.Factory.StartNew(() => UpdateFileTask());
                }
            }
            else
            {
                Global.LocaleVerList = CommonFunc.ScanVersionXmlFile(Global.WebUpdaterFile, null, false);
                using (MemoryStream ms = Download(Global.WebUpdaterUrl, false))
                {
                    if (ms.Length > 0)
                    {
                        Global.RemoteVerList = CommonFunc.ScanVersionXmlFile("", ms, true);

                        if (Convert.ToInt64(Global.RemoteVerList[0][1]) > Convert.ToInt64(Global.LocaleVerList[0][1]))
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
                        else
                        {
                            label1.Content = "你的 CrazyKTV 已是最新版本。";
                        }
                    }
                    else
                    {
                        label1.Content = "暫時無法取得網路上的更新資料,請稍後再試。";
                    }
                }
            }
        }

        private void UpdateFileTask()
        {
            string UnFolderFileArguments = "-y";
            var unZipTasks = new List<Task>();

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
                                string url = (Environment.OSVersion.Version.Major >= 6) ? list[2] : Global.CodecXPUrl;
                                MemoryStream mStreamCodec = Download(url, true);
                                if (mStreamCodec.Length > 0)
                                {
                                    unZipTasks.Add(Task.Factory.StartNew(() => unZIP(mStreamCodec)));
                                }
                                break;
                            default:
                                if (list[3] == "")
                                {
                                    if (list[0].Contains(".zip"))
                                    {
                                        MemoryStream mStream = Download(list[2], true);
                                        if (mStream.Length > 0)
                                        {
                                            unZipTasks.Add(Task.Factory.StartNew(() => unZIP(mStream)));
                                        }
                                    }
                                    else
                                    {
                                        DownloadFile(list[0], list[2], true);
                                    }
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

            Dispatcher.Invoke(DispatcherPriority.Background, new Action<Label, string, string>(CommonFunc.UpdateLabel), label1, "Content", "正在等待解壓縮檔案完成, 請稍待...");

            if (unZipTasks.Count > 0)
            {
                Task.Factory.ContinueWhenAll(unZipTasks.ToArray(), EndTask =>
                {
                    Dispatcher.Invoke(DispatcherPriority.Background, new Action<Label, string, string>(CommonFunc.UpdateLabel), label1, "Content", "已完成檔案更新。");
                });
            }
            else
            {
                Dispatcher.Invoke(DispatcherPriority.Background, new Action<Label, string, string>(CommonFunc.UpdateLabel), label1, "Content", "已完成檔案更新。");
            }
        }

        private MemoryStream Download(string Url, bool UseProgBar)
        {
            HttpWebRequest Request = (HttpWebRequest)HttpWebRequest.Create(Url);
            MemoryStream mStream = new MemoryStream();

            try
            {
                using (HttpWebResponse Response = (HttpWebResponse)Request.GetResponse())
                {
                    long FileSize = Response.ContentLength;

                    if (UseProgBar)
                    {
                        Dispatcher.Invoke(DispatcherPriority.Background, new Action<ProgressBar, string, int>(CommonFunc.UpdateProgressBar), progressBar1, "Maximum", (int)FileSize);
                    }

                    using (Stream DataStream = Response.GetResponseStream())
                    {
                        byte[] Databuffer = new byte[8192];
                        int CompletedLength = 0;
                        long TotalDLByte = 0;

                        while ((CompletedLength = DataStream.Read(Databuffer, 0, 8192)) > 0)
                        {
                            TotalDLByte += CompletedLength;
                            mStream.Write(Databuffer, 0, CompletedLength);
                            if (UseProgBar)
                            {
                                Dispatcher.Invoke(DispatcherPriority.Background, new Action<ProgressBar, string, int>(CommonFunc.UpdateProgressBar), progressBar1, "Value", (int)TotalDLByte);
                            }
                        }
                    }
                }
            }
            catch
            {
                Dispatcher.Invoke(DispatcherPriority.Background, new Action<Label, string, string>(CommonFunc.UpdateLabel), label1, "Content", "檔案連結錯誤!");
            }
            return mStream;
        }

        private bool DownloadFile(string File, string Url, bool UseProgBar)
        {
            bool DownloadStatus = false;
            HttpWebRequest Request = (HttpWebRequest)HttpWebRequest.Create(Url);

            try
            {
                using (HttpWebResponse Response = (HttpWebResponse)Request.GetResponse())
                {
                    long FileSize = Response.ContentLength;

                    if (UseProgBar)
                    {
                        Dispatcher.Invoke(DispatcherPriority.Background, new Action<ProgressBar, string, int>(CommonFunc.UpdateProgressBar), progressBar1, "Maximum", (int)FileSize);
                    }

                    using (FileStream FStream = new FileStream(File, FileMode.Create))
                    {
                        using (Stream DataStream = Response.GetResponseStream())
                        {
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
                        }
                    }
                }
                DownloadStatus = true;
            }
            catch
            {
                Dispatcher.Invoke(DispatcherPriority.Background, new Action<Label, string, string>(CommonFunc.UpdateLabel), label1, "Content", "檔案連結錯誤!");
                DownloadStatus = false;
            }
            return DownloadStatus;
        }

        private void unZIP(MemoryStream mStream)
        {
            mStream.Position = 0;
            ReadOptions opt = new ReadOptions();
            opt.Encoding = Encoding.Default;

            using (var zip = ZipFile.Read(mStream, opt))
            {
                zip.ExtractExistingFile = ExtractExistingFileAction.OverwriteSilently;
                zip.ExtractAll(AppDomain.CurrentDomain.BaseDirectory);
            }
            mStream.Close();
        }
    }
}
