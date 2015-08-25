using BimLibraryAddin.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BimLibraryAddin.Dialogs
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
        }

        public static void ShowFor(int seconds)
        {
            var handle = new RevitHandle();
            var dlg = new About { WindowStyle = WindowStyle.None};
            handle.SetAsOwnerTo(dlg);

            dlg.Show();
            Thread.Sleep(seconds * 1000);
            dlg.Close();
        }
    }
}
