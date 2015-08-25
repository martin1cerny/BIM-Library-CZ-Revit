using BimLibraryAddin.BimLibraryService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BimLibraryAddin.Dialogs.ViewModels
{
    public class ProductViewModel
    {
        Product _product;

        public ProductViewModel(Product product)
        {
            _product = product;
        }

        public string Name { get { return _product.Namek__BackingField;} }

        public string ShortDescription { get { return _product.ShortDescriptionk__BackingField; } }
        public string Description { get { return _product.FullDescriptionk__BackingField; } }

        public int Id { get { return _product.Idk__BackingField; } }

        public IEnumerable<BitmapImage> Images
        {
            get
            {
                return _product._productPictures.Select(p => GetImage(p.Picturek__BackingField));
            }
        }

        public BitmapImage FirstImage { get { return Images.FirstOrDefault(); } }

        public string Classifications
        {
            get
            {
                return _product._productCategories.Select(c => c.Categoryk__BackingField.Name).Aggregate("", (s, c) => s + "\n" + c);
            }
        }

        private BitmapImage GetImage(Picture picture)
        {
            var type = picture.MimeType;
            var data = picture.PictureBinary;

            using (var ms = new System.IO.MemoryStream(data))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }
    }
}
