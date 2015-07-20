using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;
using System.IO;
using System.Reflection;
using InterplanAddin.AddIns;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;
using InterplanAddin.Dialogs;
using Autodesk.Revit.DB.Events;

namespace BimLibraryAddin
{
    [Regeneration(RegenerationOption.Manual)]
    [Transaction(TransactionMode.Manual)]
    public class AddInApplication : IExternalApplication 
    {
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
            application.CreateRibbonTab("Interplan");
            _panel = application.CreateRibbonPanel("Interplan", "Nástroje myFM");
            _panel.Enabled = true;

            //get assembly of this class to set the right path to other objects in this assembly
            Assembly assembly = Assembly.GetAssembly(this.GetType());
            string assemblyPath = assembly.Location;
            string dirPath = Path.GetDirectoryName(assemblyPath);

            //-----------------------------BUTTONS FOR COMMANDS ----------------------------
            var btnSettings = _panel.AddItem(new PushButtonData(
                "InterplanSettings",
                "Nastavení",
                assemblyPath,
                "InterplanAddin.Dialogs.InterplanSettings"
                )) as PushButton;
            btnSettings.LargeImage = new BitmapImage(new Uri("pack://application:,,,/InterplanAddin;component/Icons/icon_settings.ico"));
            btnSettings.AvailabilityClassName = "InterplanAddin.AddIns.InitializedAvailability";
            btnSettings.ToolTip = "Nastavení parametrů pro myFM firmy Interplan";
            btnSettings.LongDescription = 
@"Tyto parametry jsou určeny pro zachování vazeb se systémem myFM firmy Interplan pro Facility Management.";

            var btnBatchFill = _panel.AddItem(new PushButtonData(
               "InterplanBatchFill",
               "Hromadné vyplnění \n parametrů",
               assemblyPath,
               "InterplanAddin.Dialogs.BatchFiller"
               )) as PushButton;
            btnBatchFill.LargeImage = new BitmapImage(new Uri("pack://application:,,,/InterplanAddin;component/Icons/icon_batch_fill.ico"));
            btnBatchFill.AvailabilityClassName = "InterplanAddin.AddIns.InitializedAvailability";
            btnBatchFill.ToolTip = "Hromadné vyplnění parametrů pro systém myFM.";
            btnBatchFill.LongDescription = 
@"Tento nástroj je vhodný zejména pro hromadné vyplnění parametrů u projektu, který nebyl dosud inicializován pro myFM. Zároveň je možné jej využít k hromadným opravám. Pokud je nástroj použit na výběr, budou přepsány existující hodnoty. Pokud je použit bez výběru, jsou pouze vyplněny chybějící hodnoty.";

                        

            application.ControlledApplication.DocumentOpened += new EventHandler<DocumentOpenedEventArgs>(ControlledApplication_DocumentOpened);
            application.ControlledApplication.DocumentCreated += new EventHandler<DocumentCreatedEventArgs>(ControlledApplication_DocumentCreating);
            application.ControlledApplication.DocumentClosing += new EventHandler<DocumentClosingEventArgs>(ControlledApplication_DocumentClosing);
            application.ControlledApplication.DocumentOpening += new EventHandler<DocumentOpeningEventArgs>(ControlledApplication_DocumentOpening);
            application.ControlledApplication.DocumentChanged += new EventHandler<DocumentChangedEventArgs>(GuidUpdater.OnDocumentChanged);

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
