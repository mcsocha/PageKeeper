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
            var config = new WebConfig { };
            WebCore.Initialize(config);
            InitializeComponent();

            textBox.Text = DefaultUrl.ToString();
            Navigate(DefaultUrl);
        }

        private void Navigate(Uri uri)
        {
            browser.Source = new Uri(uri.ToString());
            SetTrayFavicon(uri);
        }

        private void SetTrayFavicon(Uri pageUri)
        {            
            var html = GetHtml(pageUri);
            var uri = GetFavIconUri(pageUri, html);            
            var tempPath = Path.GetTempFileName() + ".ico";
            var client = new WebClient();
            client.DownloadFile(uri, tempPath);
            Icon icon = null;
            try
            {
                icon = new Icon(tempPath);
            }
            catch (Exception)
            {
                icon = new Icon("default.ico");
            }


            TrayIcon = new NotifyIcon()
            {
                Icon = icon,
                Visible = true,
                ContextMenu = new ContextMenu(new MenuItem[]
                {
                    new MenuItem("Restore", (s, a) => Show(), Shortcut.None),
                    new MenuItem("Exit", (s, a) => CloseNoTray(), Shortcut.None)
                }),
                Text = "PageKeeper"
            };
            TrayIcon.DoubleClick += (sender, args) =>
            {
                Show();
            };
        }
         
        private string GetHtml(Uri uri)
        {
            string html = null;
            var client = new HttpClient();
            html = client.GetStringAsync(uri).Result;
            return html;
        }

        private Uri GetFavIconUri(Uri pageUri, string html)
        {
            Uri uri = null;
            try
            {
                var linkRegex = new Regex(@"<link.*rel=""shortcut icon"".*>");
                var link = linkRegex.Match(html);
                var hrefRegex = new Regex(@"(?<=href="").*(?="")");
                var href = hrefRegex.Match(link.ToString());
                var url = href.ToString().ToLower();

                if (!string.IsNullOrWhiteSpace(url) && url[0] == '/')
                {
                    var baseUri = new Uri(string.Format("{0}://{1}", pageUri.Scheme, pageUri.Host));
                    uri = new Uri(baseUri, url.Substring(1));
                }
                else if (url.Contains("http"))
                    uri = new Uri(url);

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
                    Navigate(new Uri(textBox.Text));
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
        
        private void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            browser.Width = window.Width;
            browser.Height = window.Height - textBox.Height;
        }
    }
}
