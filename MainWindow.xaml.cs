using Awesomium.Core;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
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

        public bool Exit { get; set; }

        public MainWindow(Uri startUri = null)
        {
            WebCore.Initialize(new WebConfig(), true);
            InitializeComponent();

            Exit = false;
            txtUrl.KeyUp += TxtUrl_KeyUp;
            winMain.Closing += WinMain_Closing;
            Uri link = startUri == null ? DefaultUrl : startUri;

            txtUrl.Text = link.ToString(); 
            Navigate(link);
            
        }

        private void WinMain_Closing(object sender, CancelEventArgs e)
        {
            if (!Exit)
            {
                e.Cancel = true;
                Hide();
            }
        }
        
        private void TxtUrl_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    e.Handled = true;
                    var url = txtUrl.Text.ToLower();
                    if (!url.Contains("http://") && !url.Contains("https://"))
                        url = string.Format("http://{0}", url);
                    txtUrl.Text = url;
                    Navigate(new Uri(url));
                }
                catch (Exception ex)
                {
                    var winError = new BrowserError(ex.Message);
                    winError.Owner = this;
                    winError.ShowDialog();
                }                
            }
        }

        private void Navigate(Uri uri)
        {
            webMain.Source = new Uri(uri.ToString());
            Title = uri.Host.Replace("www.", "").Replace("/", "") + " - PageKeeper";
            SetTrayIcon(uri);
        }

        private void SetTrayIcon(Uri pageUri)
        {
            Icon icon = null;
            var uri = GetFavIconUri(pageUri);
            if (uri != null)
            {
                var tempFilePath = string.Format("{0}.ico", Path.GetTempFileName());

                new WebClient().DownloadFile(uri, tempFilePath);
                try
                {
                    icon = new Icon(tempFilePath);
                }
                catch (Exception)
                {
                }
            }

            var restoreMenuItem = new MenuItem("Restore", (s, a) => Show(), Shortcut.None);
            var exitMenuItem = new MenuItem("Exit", (s, a) => { Exit = true; Close(); }, Shortcut.None);
            var title = pageUri.ToString() + " - PageKeeper";
            if (title.Length > 63)
                title = title.Substring(0, 63);
            if (TrayIcon == null)
            {
                TrayIcon = new NotifyIcon()
                {
                    Icon = icon != null ? icon : new Icon("default.ico"),
                    Visible = true,
                    ContextMenu = new ContextMenu(new MenuItem[] { restoreMenuItem, exitMenuItem }),
                    Text = title,
                };
                TrayIcon.DoubleClick += (sender, args) => Show();
            }
            else
                TrayIcon.Text = Title;
        }

        private Uri GetFavIconUri(Uri pageUri)
        {
            Uri uri = null;
            try
            {
                uri = new Uri(string.Format("http://www.google.com/s2/favicons?domain={0}", pageUri.ToString()));
            }
            catch (Exception)
            {
            }

            return uri;
        }
    }
}
