﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xml.Linq;
using System.Xml.XPath;

namespace CrazyKTV_WebUpdater
{
    class CommonFunc
    {
        public static void CreateVersionXmlFile(string VersionFile)
        {

            XDocument xmldoc = new XDocument
                (
                    new XDeclaration("1.0", "utf-16", null),
                    new XElement("Configeruation")
                );
            xmldoc.Save(VersionFile);
        }

        public static List<string> LoadVersionXmlFile(string VersionFile, string FileName)
        {
            List<string> Value = new List<string>();
            Value.Add(FileName);
            try
            {
                XElement rootElement = XElement.Load(VersionFile);
                var Query = from childNode in rootElement.Elements("File")
                            where (string)childNode.Attribute("Name") == FileName
                            select childNode;

                foreach (XElement childNode in Query)
                {
                    Value.Add(childNode.Element("Ver").Value);
                    Value.Add(childNode.Element("Url").Value);
                    Value.Add(childNode.Element("Path").Value);
                    Value.Add(childNode.Element("Desc").Value);
                }
            }
            catch
            {
                MessageBox.Show("【" + Path.GetFileName(VersionFile) + "】設定檔內容有錯誤,請刪除後再執行。");
            }
            return Value;
        }

        public static void SaveVersionXmlFile(string VersionFile, string FileName, string FileVer, string FileUrl, string FilePath, string FileDesc)
        {
            XDocument xmldoc = XDocument.Load(VersionFile);
            XElement rootElement = xmldoc.XPathSelectElement("Configeruation");

            var Query = from childNode in rootElement.Elements("File")
                        where (string)childNode.Attribute("Name") == FileName
                        select childNode;

            if (Query.ToList().Count > 0)
            {
                foreach (XElement childNode in Query)
                {
                    childNode.Element("Ver").Value = FileVer;
                    childNode.Element("Url").Value = FileUrl;
                    childNode.Element("Path").Value = FilePath;
                    childNode.Element("Desc").Value = FileDesc;
                }
            }
            else
            {
                XElement AddNode = new XElement("File", new XAttribute("Name", FileName), new XElement("Ver", FileVer), new XElement("Url", FileUrl), new XElement("Path", FilePath), new XElement("Desc", FileDesc));
                rootElement.Add(AddNode);
            }
            xmldoc.Save(VersionFile);
        }

        public static List<List<string>> ScanVersionXmlFile(string VersionFile, Stream stream, bool isStream)
        {
            List<List<string>> VerValueListList = new List<List<string>>();

            try
            {
                XElement rootElement;

                if (isStream)
                {
                    stream.Position = 0;
                    rootElement = XElement.Load(stream);
                }
                else
                {
                    rootElement = XElement.Load(VersionFile);
                }
                
                foreach (XElement childNode in rootElement.Elements("File"))
                {
                    List<string> list = new List<string>();
                    list.Add(childNode.Attribute("Name").Value);
                    list.Add(childNode.Element("Ver").Value);
                    list.Add(childNode.Element("Url").Value);
                    list.Add(childNode.Element("Path").Value);
                    list.Add(childNode.Element("Desc").Value);
                    VerValueListList.Add(list);
                }
            }
            catch
            {
                MessageBox.Show("【" + Path.GetFileName(VersionFile) + "】設定檔內容有錯誤,請刪除後再執行。");
                Application.Current.Shutdown();
            }
            return VerValueListList;
        }

        public static void UpdateProgressBar(ProgressBar pbar, string uitem, int value)
        {
            switch (uitem)
            {
                case "Maximum":
                    pbar.Maximum = value;
                    break;
                case "Value":
                    pbar.Value = value;
                    break;
            }
        }

        public static void UpdateLabel(Label lbl, string uitem, string value)
        {
            switch (uitem)
            {
                case "Content":
                    lbl.Content = value;
                    break;
            }
        }

        public static void SubscribeToAllHyperlinks(FlowDocument flowDocument)
        {
            var hyperlinks = GetVisuals(flowDocument).OfType<Hyperlink>();
            foreach (var link in hyperlinks)
                link.RequestNavigate += new System.Windows.Navigation.RequestNavigateEventHandler(Link_RequestNavigate);
        }

        public static IEnumerable<DependencyObject> GetVisuals(DependencyObject root)
        {
            foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
            {
                yield return child;
                foreach (var descendants in GetVisuals(child))
                    yield return descendants;
            }
        }

        public static void Link_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        public static void SetSecurityProtocol()
        {
            try
            { //try TLS 1.3
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)12288
                                                     | (SecurityProtocolType)3072
                                                     | (SecurityProtocolType)768
                                                     | SecurityProtocolType.Tls;
            }
            catch (NotSupportedException)
            {
                try
                { //try TLS 1.2
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072
                                                         | (SecurityProtocolType)768
                                                         | SecurityProtocolType.Tls;
                }
                catch (NotSupportedException)
                {
                    try
                    { //try TLS 1.1
                        ServicePointManager.SecurityProtocol = (SecurityProtocolType)768
                                                             | SecurityProtocolType.Tls;
                    }
                    catch (NotSupportedException)
                    { //TLS 1.0
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                    }
                }
            }
        }

    }
}
