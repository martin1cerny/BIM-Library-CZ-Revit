using BimLibrarySample.BimLibraryService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace BimLibrarySample
{
    class Program
    {
        static void Main(string[] args)
        {
            //search for a Revit products
            var client = new BIMserviceClient(new BasicHttpBinding(BasicHttpSecurityMode.None) { MaxReceivedMessageSize = 1000000},
                new EndpointAddress("http://www.narodni-bim-knihovna.cz/BIMservice.svc"));

            client.Open();

            //product search
            var products = client.GetProductByName("test");
            Console.WriteLine("Number of products: " + products.Count());
            foreach (var product in products)
            {
                var name = product.Namek__BackingField;
                var categories = product._productCategories ?? new ProductCategory[0];
                var catNames = categories.Select(c => c.Categoryk__BackingField.Name);
                var catNamesShow = String.Join(", ", catNames); ;

                Console.WriteLine("Product: {0}, Categories: {1}", name, catNamesShow);
            }

            ShowAllCategories(client);

            Console.ReadKey();
        }

        private static void ShowAllCategories(BIMserviceClient client)
        {
            //get category hierarchy root
            var catRoots = client.GetAllCategoriesByParentCategoryId(0, true);

            foreach (var root in catRoots)
            {
                PrintCategory(root, client, 0);
            }
        }

        private static void PrintCategory(Category category, BIMserviceClient client, int depth)
        {
            //indent
            for (int i = 0; i < depth; i++)
                Console.Write("\t");

            //write name
            Console.WriteLine(category.Name);
            //write all children
            var children = client.GetAllCategoriesByParentCategoryId(category.Idk__BackingField, true);
            foreach (var child in children)
            {
                PrintCategory(child, client, depth + 1);
            }
        }
    }
}
