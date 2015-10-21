using Awesomium.Core;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Net.Http;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Linq;
using System.Linq;
using System.Xml;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;

namespace PageKeeper
{
    public partial class MainWindow : Window
    {
        public NotifyIcon TrayIcon;

        private Uri defaultUrl;
        public Uri DefaultUrl
        {   
            get
            {
                if (defaultUrl == null)
                {
                    try
                    {
                        defaultUrl = new Uri(ConfigurationManager.AppSettings["DefaultUrl"]);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Default URL is in the incorrect format", e);
                    }                    
                }
                return defaultUrl;
            }
        }

        public MainWindow()
        {
            WebCore.Initialize(new WebConfig(), true);
            InitializeComponent();

            txtUrl.Text = DefaultUrl.ToString();
            Navigate(DefaultUrl);
        }

        private void Navigate(Uri uri)
        {
            webMain.Source = new Uri(uri.ToString());
            SetTrayIcon(uri);
        }

        private void SetTrayIcon(Uri pageUri)
        {
            Icon icon = null;
            var html = GetHtml(pageUri);
            if (!string.IsNullOrWhiteSpace(html))
            {
                var uri = GetFavIconUri(pageUri, html);                

                if (uri != null)
                {
                    var tempFilename = Path.GetFileNameWithoutExtension(Path.GetTempFileName());
                    var tempPath = string.Format("{0}.ico", tempFilename);

                    new WebClient().DownloadFile(uri, tempPath);
                    icon = new Icon(tempPath);
                }
            }

            var restoreMenuItem = new MenuItem("Restore", (s, a) => Show(), Shortcut.None);
            var exitMenuItem = new MenuItem("Exit", (s, a) => CloseNoTray(), Shortcut.None);
            TrayIcon = new NotifyIcon()
            {
                Icon = icon != null ? icon : new Icon("default.ico"),
                Visible = true,
                ContextMenu = new ContextMenu(new MenuItem[] { restoreMenuItem, exitMenuItem }),
                Text = "PageKeeper"
            };
            TrayIcon.DoubleClick += (sender, args) => Show();
        }
         
        private string GetHtml(Uri uri)
        {
            string html = null;

            try
            {
                html = new HttpClient().GetStringAsync(uri).Result;
            }
            catch (Exception)
            {   
            }

            return html;
        }

        private Uri GetFavIconUri(Uri pageUri, string html)
        {
            Uri uri = null;
            try
            {                
                var linkRegex = new Regex(@"<link.*rel=""shortcut icon"".*>");
                var link = linkRegex.Match(html);
                if (link.Success)
                {
                    var hrefRegex = new Regex(@"(?<=href="").*(?="")");
                    var href = hrefRegex.Match(link.ToString());
                    if (href.Success)
                    {
                        var url = href.ToString().ToLower();
                        if (url[0] == '/')
                        {
                            var baseUri = new Uri(string.Format("{0}://{1}", pageUri.Scheme, pageUri.Host));
                            uri = new Uri(baseUri, url.Substring(1));
                        }
                        else if (url.Contains("http"))
                            uri = new Uri(url);
                    }
                }
            }
            catch (Exception)
            {
            }

            return uri;
        }

        private void CloseNoTray()
        {
            Environment.Exit(0);
        }

        private void textBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                try
                {
                    Navigate(new Uri(txtUrl.Text));
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(this, ex.Message, "Browser Error", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                }
                
                e.Handled = true;                
            }
        }        

        private void window_Closing(object sender, CancelEventArgs e)
        {                        
            e.Cancel = true;
            Hide();
        }
    }
}
