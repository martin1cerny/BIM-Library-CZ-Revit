using System;
using System.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            var client = new BimLibraryService.BIMserviceClient(new BasicHttpBinding(BasicHttpSecurityMode.None),
                new EndpointAddress("http://www.narodni-bim-knihovna.cz/BIMservice.svc"));
            client.Open();
            var products = client.GetProductByName("test");

            foreach (var product in products)
            {
                var description = product.FullDescription;
                var name = product.Name;
                var isPublished = product.Published;
                var usDeleted = product.Deleted;
                var downloadId = product.DownloadId;
            }

        }
    }
}
