using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using BimLibraryAddin.Dialogs;
using BimLibraryAddin.Extensions;
using BimLibraryAddin.Dialogs.ViewModels;
using Autodesk.Revit.Attributes;

namespace BimLibraryAddin.AddIns
{
    [Regeneration(RegenerationOption.Manual), Transaction(TransactionMode.Manual)]
    public class ProductSearchAddIn : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
#if !DEBUG
            //show about information if this is the first time when the tool is used
            if (AddInApplication.Used == 0)
                About.ShowFor(3);
            AddInApplication.Used++;
#endif

            //show UI
            var dlg = new SearchDialog();
            var handle = new RevitHandle();
            handle.SetAsOwnerTo(dlg);

            if (dlg.ShowDialog() != true) return Result.Cancelled;

            //get search string
            var searchString = dlg.SearchText;

            //create proxy client for a BIM Library services
            var client = new BimLibraryService.BIMserviceClient(new BasicHttpBinding(BasicHttpSecurityMode.None) { MaxReceivedMessageSize = 2147483647 },
                new EndpointAddress(Paths.BimServiceEndpoint));

            //many exceptions may occure during service operation
            try
            {
                client.Open();
                //search for a Revit products
                var products = client.GetProductByName(searchString, true);

                if (products.Length == 0)
                {
                    TaskDialog.Show("Produkt nenalezen", "Nebyl nalezen žádný produkt odpovídající zadání.");
                    //call this again to try is again;
                    return Execute(commandData, ref message, elements);
                }

                //show result in a dialog where user can download the object into his project
                var resultsDlg = new SearchResultsDialog();
                handle.SetAsOwnerTo(resultsDlg);
                resultsDlg.Products = products.Select(p => new ProductViewModel(p));
                resultsDlg.ShowDialog();


            }
            catch (FaultException customFaultEx)
            {
                return FailGracefully(customFaultEx.Message);
            }
            // The following is typically thrown on the client when a channel is terminated due to the server closing the connection.
            catch (ChannelTerminatedException cte)
            {
                return FailGracefully(cte.Message);
            }
            // The following is thrown when a remote endpoint could not be found or reached.  The endpoint may not be found or 
            // reachable because the remote endpoint is down, the remote endpoint is unreachable, or because the remote network is unreachable.
            catch (EndpointNotFoundException enfe)
            {
                return FailGracefully(enfe.Message);
            }
            // The following exception that is thrown when a server is too busy to accept a message.
            catch (ServerTooBusyException stbe)
            {
                return FailGracefully(stbe.Message);
            }
            catch (TimeoutException timeoutEx)
            {
                return FailGracefully(timeoutEx.Message);
            }
            catch (CommunicationException comException)
            {
                return FailGracefully(comException.Message);
            }
            catch (Exception e)
            {
                // rethrow any other exception not defined here
                throw e;
            }

            return Result.Succeeded;
        }

        private static Result FailGracefully(string message)
        {
            TaskDialog.Show("Chyba: ", message);
            return Result.Failed;
        }
    }
}
