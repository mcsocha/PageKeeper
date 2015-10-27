using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PageKeeper
{
    /// <summary>
    /// Interaction logic for BrowserError.xaml
    /// </summary>
    public partial class BrowserError : Window
    {
        public BrowserError(string message)
        {
            InitializeComponent();
            lblError.Content = message;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}
