using System;
using System.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Media.Imaging;
using BimLibraryServiceTests.BimLibraryService;
using System.Collections.Generic;
using System.Linq;

namespace BimLibraryServiceTests
{
    [TestClass]
    public class ProductRetrievalTests
    {
        [TestMethod]
        public void EstablishingConnection()
        {
            var proxy = new BimLibraryService.BIMserviceClient(new BasicHttpBinding(BasicHttpSecurityMode.None),
                new EndpointAddress("http://www.narodni-bim-knihovna.cz/BIMservice.svc"));
            proxy.Open();
        }

        [TestMethod]
        public void ProductSearch()
        {
            var client = new BimLibraryService.BIMserviceClient(new BasicHttpBinding(BasicHttpSecurityMode.None) { MaxReceivedMessageSize = 2147483647},
                new EndpointAddress("http://www.narodni-bim-knihovna.cz/BIMservice.svc"));
            client.Open();
            var products = client.GetProductByName("martin", true);

            foreach (var product in products)
            {
                var description = product.ShortDescriptionk__BackingField;
                var name = product.Namek__BackingField;
                var isPublished = product.Publishedk__BackingField;
                var usDeleted = product.Deletedk__BackingField;
                var downloadId = product.DownloadIdk__BackingField;
                var image = GetImages(product).FirstOrDefault();
                var cats = product._productCategories;

                var variants = client.GetModelVariantsForProduct(product.Idk__BackingField);

            }


        }

        private IEnumerable<BitmapImage> GetImages(Product product)
        {
                return product._productPictures.Select(p => GetImage(p.Picturek__BackingField));
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
