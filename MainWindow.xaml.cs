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
        public bool Exit { get; set; }

        public MainWindow()
        {
            WebCore.Initialize(new WebConfig(), true);
            InitializeComponent();

            Exit = false;
            txtUrl.KeyUp += TxtUrl_KeyUp;
            winMain.Closing += WinMain_Closing;
            txtUrl.Text = DefaultUrl.ToString();
            Navigate(DefaultUrl);
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
                    Navigate(new Uri(txtUrl.Text));
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(this, ex.Message, "Browser Error", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                e.Handled = true;
            }
        }

        private void Navigate(Uri uri)
        {
            webMain.Source = new Uri(uri.ToString());
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

            TrayIcon = new NotifyIcon()
            {
                Icon = icon != null ? icon : new Icon("default.ico"),
                Visible = true,
                ContextMenu = new ContextMenu(new MenuItem[] { restoreMenuItem, exitMenuItem }),
                Text = title,
            };
            TrayIcon.DoubleClick += (sender, args) => Show();
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
