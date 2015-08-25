using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;
using System.IO;
using System.Reflection;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Events;

namespace BimLibraryAddin
{
    [Regeneration(RegenerationOption.Manual)]
    [Transaction(TransactionMode.Manual)]
    public class AddInApplication : IExternalApplication 
    {
        internal static int Used = 0;

        private RibbonPanel _panel;

        public Result OnShutdown(UIControlledApplication application)
        {
            //save actual user settings
            //Properties.Settings.Default.Save();

            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {

            // add new ribbon panel 
            application.CreateRibbonTab("BIM knihovna");
            _panel = application.CreateRibbonPanel("BIM knihovna", "Nástroje");
            _panel.Enabled = true;

            //get assembly of this class to set the right path to other objects in this assembly
            var assembly = Assembly.GetAssembly(this.GetType());
            var assemblyPath = assembly.Location;
            var dirPath = Path.GetDirectoryName(assemblyPath);

            //-----------------------------BUTTONS FOR COMMANDS ----------------------------
            var btnSearch = _panel.AddItem(new PushButtonData(
                "GetObject",
                "Vyhledávání",
                assemblyPath,
                "BimLibraryAddin.AddIns.ProductSearchAddIn"
                )) as PushButton;
            btnSearch.LargeImage = new BitmapImage(new Uri("pack://application:,,,/BimLibraryAddin;component/Icons/logo.ico"));
            btnSearch.AvailabilityClassName = "BimLibraryAddin.AddIns.DocumentAvailability";
            btnSearch.ToolTip = "Vyhledávání objektů v národní BIM knihovně";
            btnSearch.LongDescription = 
@"Vyhledané objekty je možné importovat do aktuálního projektu a okamžitě použít.";

            var btnAbout = _panel.AddItem(new PushButtonData(
                "About",
                "O aplikaci",
                assemblyPath,
                "BimLibraryAddin.AddIns.AboutAddIn"
                )) as PushButton;
            btnAbout.LargeImage = new BitmapImage(new Uri("pack://application:,,,/BimLibraryAddin;component/Icons/info.ico"));
            btnAbout.ToolTip = "Informace o tomto nástroji";

            //------------------------ EVENTS REGISTRATION ------------------------------------
            application.ControlledApplication.DocumentOpened += new EventHandler<DocumentOpenedEventArgs>(ControlledApplication_DocumentOpened);
            application.ControlledApplication.DocumentCreated += new EventHandler<DocumentCreatedEventArgs>(ControlledApplication_DocumentCreating);
            application.ControlledApplication.DocumentClosing += new EventHandler<DocumentClosingEventArgs>(ControlledApplication_DocumentClosing);
            application.ControlledApplication.DocumentOpening += new EventHandler<DocumentOpeningEventArgs>(ControlledApplication_DocumentOpening);
            //application.ControlledApplication.DocumentChanged += new EventHandler<DocumentChangedEventArgs>();

            //register failures
            Failures.RegisterFailures();


            return Result.Succeeded;
        }


        void ControlledApplication_DocumentOpening(object sender, Autodesk.Revit.DB.Events.DocumentOpeningEventArgs e)
        {
        }

        void ControlledApplication_DocumentCreating(object sender, Autodesk.Revit.DB.Events.DocumentCreatedEventArgs e)
        {
            var app = sender as Application;
            var document = e.Document;
        }

        //This function is called not only when document is being closed but also when 
        //anything changes in collaboration work sets. Unregistering updater is undesirable in that case
        void ControlledApplication_DocumentClosing(object sender, Autodesk.Revit.DB.Events.DocumentClosingEventArgs e)
        {

            ////unregister updaters
            //var app = sender as Application;
            //var updater = new GuidUpdater(app.ActiveAddInId);

            //if (UpdaterRegistry.IsUpdaterRegistered(updater.GetUpdaterId()))
            //    UpdaterRegistry.UnregisterUpdater(updater.GetUpdaterId());
        }

        void ControlledApplication_DocumentOpened(object sender, Autodesk.Revit.DB.Events.DocumentOpenedEventArgs e)
        {
        }


        private void RegisterUpdater(Document document, IUpdater updater)
        {
            //register updater
            UpdaterRegistry.RegisterUpdater(updater, document);

            //UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), GuidUpdater.GetFilter(document), Element.GetChangeTypeAny());
            //UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), GuidUpdater.GetFilter(document), Element.GetChangeTypeElementAddition());
            
        }
    }
}
