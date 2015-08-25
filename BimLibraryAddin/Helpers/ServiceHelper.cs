using BimLibraryAddin.BimLibraryService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace BimLibraryAddin.Helpers
{
    public class ServiceHelper
    {
        private static List<Category> _categoryCache = new List<Category>();

        public static BIMserviceClient GetNewClient()
        {
            return new BIMserviceClient(new BasicHttpBinding(BasicHttpSecurityMode.None) { MaxReceivedMessageSize = 2147483647 },
                new EndpointAddress(Paths.BimServiceEndpoint));
        }

        public static List<Category> GetParents(Category child, BIMserviceClient client)
        {
            var result = new List<Category>();
            
            //child is the last in the list
            result.Insert(0, child);

            var node = child;
            while (node.ParentCategoryId > 0)
            {
                //var parent = client.GetCategoryById(node.ParentCategoryId);
            }

            return result;
        }
    }
}
