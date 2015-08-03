using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;

namespace BimLibraryAddin.AddIns
{
    public class ProductSearchAddIn: IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            //show UI

            //get search string

            //search for a Revit products
            var proxy = new BimLibraryService.BIMserviceClient(new BasicHttpBinding(BasicHttpSecurityMode.None),
                new EndpointAddress("http://www.narodni-bim-knihovna.cz/BIMservice.svc"));
            proxy.Open();
            var products = proxy.GetProductByName("test");

            //show search results with ability to import data into Revit

            throw new NotImplementedException();
        }
    }
}
