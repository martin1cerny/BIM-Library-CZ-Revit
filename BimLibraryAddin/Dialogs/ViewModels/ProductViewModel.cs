using BimLibraryAddin.BimLibraryService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BimLibraryAddin.Dialogs.ViewModels
{
    public class ProductViewModel
    {
        Product _product;

        public ProductViewModel(Product product)
        {
            _product = product;
        }

        public string Name { get { return _product.Namek__BackingField; } }
    }
}
