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
    class Program
    {
        static void Main(string[] args)
        {
            try
            {

                //create a new addin manifest
                RevitAddInManifest manifest = new RevitAddInManifest();
#if Revit2013
                //save manifest to a file
                RevitProduct revitProduct = RevitProductUtility.GetAllInstalledRevitProducts()
                    .Where<RevitProduct>(p => p.Version == RevitVersion.Revit2013)
                    .FirstOrDefault();
                string revitVersion = "Revit 2013";
#elif Revit2014
                //save manifest to a file
                RevitProduct revitProduct = RevitProductUtility.GetAllInstalledRevitProducts()
                    .Where<RevitProduct>(p => p.Version == RevitVersion.Revit2014)
                    .FirstOrDefault();
                string revitVersion = "Revit 2014";
#elif Revit2015
                //save manifest to a file
                RevitProduct revitProduct = RevitProductUtility.GetAllInstalledRevitProducts()
                    .Where<RevitProduct>(p => p.Version == RevitVersion.Revit2015)
                    .FirstOrDefault();
                string revitVersion = "Revit 2015";
#elif Revit2016
                //save manifest to a file
                RevitProduct revitProduct = RevitProductUtility.GetAllInstalledRevitProducts()
                    .Where<RevitProduct>(p => p.Version == RevitVersion.Revit2016)
                    .FirstOrDefault();
                string revitVersion = "Revit 2016";
#endif

                if (revitProduct != null)
                {
                    string addinPath = Path.Combine(revitProduct.AllUsersAddInFolder, "CzechBimLibrary.addin");

                    //uninstall option - delete addin file
                    if (args.FirstOrDefault() == "-u")
                    {
                        if (File.Exists(addinPath))
                            File.Delete(addinPath);
                    }
                    //install addin - create addin file and save it to register the addin
                    else
                    {
                        string location = typeof(DummyReference).Assembly.Location;
                        //create an external application
                        var id = new Guid("936A5434-1A11-484C-BFE3-A310658180B1");
                        RevitAddInApplication application = new RevitAddInApplication("BimLibraryApplication", location, id, "BimLibraryAddin.AddInApplication", "ADSK");
                        //add both command(s) and application(s) into manifest
                        manifest.AddInApplications.Add(application);
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
                            "There is no " + revitVersion + " installed on your machine. Run this again after you have Revit installed.",
                            "Message",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }


            }
            catch (Exception e)
            {
                MessageBox.Show( "There was an error during registration of the Add-in. Tool functionality will be affected.",
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
