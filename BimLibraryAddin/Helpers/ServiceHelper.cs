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
        public static BIMserviceClient GetNewClient()
        {
            return new BIMserviceClient(new BasicHttpBinding(BasicHttpSecurityMode.None) { MaxReceivedMessageSize = 2147483647 },
                new EndpointAddress(Paths.BimServiceEndpoint));
        }
    }
}
