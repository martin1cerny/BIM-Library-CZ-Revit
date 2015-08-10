using BimLibraryAddin.Dialogs.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BimLibraryAddin.Dialogs
{
    /// <summary>
    /// Interaction logic for SearchResultsDiaolog.xaml
    /// </summary>
    public partial class SearchResultsDialog : Window
    {
        public SearchResultsDialog()
        {
            InitializeComponent();
        }

        public IEnumerable<ProductViewModel> Products
        {
            get { return (IEnumerable<ProductViewModel>)GetValue(ProductsProperty); }
            set { SetValue(ProductsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Products.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProductsProperty =
            DependencyProperty.Register("Products", typeof(IEnumerable<ProductViewModel>), typeof(SearchResultsDialog), new PropertyMetadata(null));

        
    }
}
