using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BimLibraryAddin.Dialogs.ViewModels;
using BimLibraryAddin.Helpers;
using System;
using System.Collections.Generic;
using System.IO.Compression;
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
    public partial class ImportDialog : Window
    {
        private readonly Document _document;

        public ImportDialog(Document document)
        {
            _document = document;
            InitializeComponent();
        }

        public IEnumerable<ProductViewModel> Products
        {
            get { return (IEnumerable<ProductViewModel>)GetValue(ProductsProperty); }
            set { SetValue(ProductsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Products.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProductsProperty =
            DependencyProperty.Register("Products", typeof(IEnumerable<ProductViewModel>), typeof(ImportDialog), new PropertyMetadata(null));

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        internal FamilySymbol LastSymbol;

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgProducts.SelectedItems;

            if (selected.Count == 0)
            {
                TaskDialog.Show("Nebyl vybrán žádný výrobek", "Vyberte některý z nalezených výrobků");
                return;
            }

            var products = selected.Cast<ProductViewModel>();
            string errMsg = "";
            var errLog = new ExceptionLog();

            //use one client for all ZIP retrievals
            var client = ServiceHelper.GetNewClient();

            foreach (var product in products)
            {
                var tempDir = System.IO.Path.GetTempPath();
                string tempFile = null;
                

                try
                {
                    //get zipped data from the client
                    using (var zipStream = client.GetZipById(product.Id, RevitVariants.Variant))
                    {
                        using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Read))
                        {
                            var familyEntry = zip.Entries.FirstOrDefault(entry =>
                                //family file
                                entry.Name.EndsWith(".rfa", StringComparison.InvariantCultureIgnoreCase) ||
                                //system file
                                entry.Name.EndsWith(".rvt", StringComparison.InvariantCultureIgnoreCase));
                            if (familyEntry == null)
                            {
                                errMsg += String.Format("Soubor pro rodinu {0} v knihovně není \n", product.Name);
                                continue;
                            }

                            //extract to temp file
                            var ext = System.IO.Path.GetExtension(familyEntry.Name);
                            tempFile = System.IO.Path.Combine(tempDir, product.Name + ext);
                            using (var entryStream = familyEntry.Open())
                            {
                                using (var tempStream = System.IO.File.Create(tempFile))
                                {
                                    entryStream.CopyTo(tempStream);
                                    tempStream.Close();
                                }
                            }
                        }
                    }


                    if (!FileExists(tempFile))
                    {
                        errMsg += String.Format("Žádný soubor pro Revit není v BIM knihovně pro {0} \n", product.Name);
                        continue;
                    }

                    var extension = System.IO.Path.GetExtension(tempFile).ToLower();
                    switch (extension)
                    {
                        case ".rfa":
                            //if it is family we can load it straight away
                            //check if family doesn't exist in the document already
                            var name = System.IO.Path.GetFileNameWithoutExtension(tempFile);
                            if (ExistingFamilies.Any(f => f.Name == name))
                            {
                                errMsg += String.Format("{0} již v projektu existuje. \n", product.Name);
                                continue;
                            }

                            //load family
                            Family family;
                            if (!_document.LoadFamily(tempFile, out family))
                            {
                                errMsg += String.Format("{0} se nepodařilo importovat, i když sobor v knihovně existuje. \n", product.Name);
                                continue;
                            }

                            var symbolId = family.GetFamilySymbolIds().FirstOrDefault();
                            LastSymbol = _document.GetElement(symbolId) as FamilySymbol;
                            break;
                        case ".rvt":
                            //if it is family file containing system family it is up to user to copy it over using cut+paste
                            //but we can open it for them
                            _document.Application.OpenDocumentFile(tempFile);
                            break;
                        default:
                            throw new Exception("Unexpected file extension " + extension);
                    }
                }
                catch (Exception ex)
                {
                    errMsg += String.Format("Chyba při stahování a importu produktu: {0} \n", product.Name);
                    errLog.Add(ex);
                }
                finally
                {
                    //delete the temp file if it was created to avoid poluting the system
                    if(FileExists(tempFile))
                        System.IO.File.Delete(tempFile);
                }
            }

            if (!String.IsNullOrWhiteSpace(errMsg))
            {
                var result = TaskDialog.Show(
                    "Některé produkty se nepodařilo importovat...", 
                    "Některé produkty se nepodařilo importovat. Chcete zrušit celý import? \n" + errMsg, 
                    TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No, TaskDialogResult.No);
                if(result == TaskDialogResult.Yes)
                {
                    DialogResult = false;
                    Close();
                    return;
                }
            }

            DialogResult = true;
            Close();
        }

        private IEnumerable<Family> ExistingFamilies
        {
            get
            {
                var filter = new ElementClassFilter(typeof(Family));
                var collector = new FilteredElementCollector(_document).WherePasses(filter);
                foreach (var item in collector)
                    yield return item as Family;
            }
        }

        private bool FileExists(string path)
        {
            return !String.IsNullOrWhiteSpace(path) && System.IO.File.Exists(path);

        }
    }
}
