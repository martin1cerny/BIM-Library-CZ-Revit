using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.RevitAddIns;
using System.IO;
using BimLibraryAddin;
using System.Windows.Forms;

namespace AddInRegistration
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                //create a new addin manifest
                var manifest = new RevitAddInManifest();
#if Revit2015
                var revitProduct =
                    RevitProductUtility.GetAllInstalledRevitProducts()
                        .FirstOrDefault(p => p.Version == RevitVersion.Revit2015);
                const string revitVersion = "Revit 2015";
#elif Revit2016
                var revitProduct = RevitProductUtility.GetAllInstalledRevitProducts()
                    .FirstOrDefault(p => p.Version == RevitVersion.Revit2016);
                const string revitVersion = "Revit 2016";
#endif

                if (revitProduct != null)
                {
                    var addinPath = Path.Combine(revitProduct.AllUsersAddInFolder, "CzechBimLibrary.addin");

                    //uninstall option - delete addin file
                    if (args.FirstOrDefault() == "-u")
                    {
                        if (File.Exists(addinPath))
                            File.Delete(addinPath);
                    }
                    //install addin - create addin file and save it to register the addin
                    else
                    {
                        var location = typeof (DummyReference).Assembly.Location;
                        //create an external application
                        var id = new Guid("936A5434-1A11-484C-BFE3-A310658180B1");
                        var application = new RevitAddInApplication("BimLibraryApplication", location, id,
                            "BimLibraryAddin.AddInApplication", "ADSK");
                        //add both command(s) and application(s) into manifest
                        manifest.AddInApplications.Add(application);
                        //save manifest to a file
                        manifest.SaveAs(addinPath);

                        //delete shared parameter file for the case its structure has been changed
                        //if (File.Exists(Paths.SharedParamFile))
                        //    File.Delete(Paths.SharedParamFile);
                    }
                }
                else
                {
                    if (args.FirstOrDefault() != "-u")
                    {
                        MessageBox.Show(
                            "There is no " + revitVersion +
                            " installed on your machine. Run this again after you have Revit installed.",
                            "Message",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    "There was an error during registration of the Add-in. Tool functionality will be affected.",
                    "Message",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
#if DEBUG
                var log = new ExceptionLog();
                log.Add(e);
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                path = Path.Combine(path, "RevitExceptions.xml");
                log.SaveToFile(path);
#endif
            }
        }
    }
}