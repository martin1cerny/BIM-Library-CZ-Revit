using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.DB;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.ComponentModel;
using Autodesk.Revit.DB.Events;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using InterplanAddin.Dialogs;
using InterplanAddin.Helpers;

namespace BimLibraryAddin
{
    public class ProjectSettingsManager
    {
        private Guid _schemaGuid = new Guid("890F9BB5-3436-45AA-9480-E371125F9576");
        private Element _element;
        private Schema _schema;
        private Entity _entity;
        private ProjectSettingsData _data;
        private static Dictionary<Document, ProjectSettingsManager> _cache;

        static ProjectSettingsManager()
        {
            _cache = new Dictionary<Document, ProjectSettingsManager>();
        }

        public static ProjectSettingsData GetSettings(Document document)
        {
            ProjectSettingsManager sett = null;
            if (_cache.TryGetValue(document, out sett))
                return sett._data;
            else
            { 
                var psm = new ProjectSettingsManager(document);
                return psm._data;
            }
        }

        

        private ProjectSettingsManager(Document document)
        {
            FilteredElementCollector col = new FilteredElementCollector(document).OfClass(typeof(ProjectInfo));
            _element = col.ToElements().First();
            _schema = Schema.Lookup(_schemaGuid);
            if (_schema == null)
                _schema = GetStorageSchema();

            //get entity from element if it exists in there already or create new otherwise
            _entity = _element.GetEntity(_schema);
            if (_entity == null || _entity.Schema == null)
                _entity = new Entity(_schema);

            LoadData(document);

            //static cache management
            _cache.Add(document, this);
            _data.PropertyChanged += new PropertyChangedEventHandler(OnSettingsChanged);
            document.DocumentClosing += new EventHandler<DocumentClosingEventArgs>(OnDocumentClosing);
            document.Application.DocumentSynchronizedWithCentral += new EventHandler<DocumentSynchronizedWithCentralEventArgs>(OnDocumentSynchronized);
            document.Application.DocumentChanged += new EventHandler<DocumentChangedEventArgs>(delegate(object sender, DocumentChangedEventArgs args) 
                {
                    if (args.Operation == UndoOperation.TransactionUndone || args.Operation == UndoOperation.TransactionRedone)
                    {
                        //check for interplan transaction
                        var names = args.GetTransactionNames();
                        if (IsInterplanTransaction(names))
                            //reload settings for the case it is changed in the undo operation
                            LoadData(args.GetDocument());

                        if (args.Operation == UndoOperation.TransactionUndone && args.GetTransactionNames().Contains("Interplan init"))
                        {
                            //parameter binding is being undone
                            InterplanDialog.ShowDialog("Varování","Dokument nyní NEBUDE inicializován pro myFM.");
                        }
                    }

                    //check if the document has been initialized
                    var sett = ProjectSettingsManager.GetSettings(document);
                    var initialized = sett.IsInitialized;

                    //check if updater is active in this document
                    var updaterActive = InterplanAddin.AddIns.GuidUpdater.IsActiveInDocument(document);

                    if (initialized && !updaterActive)
                    {
                        InterplanDialog.ShowDialog("Varování", "Projekt byl inicializován pro myFM, ale parametry nejsou ověřovány. Pravděpodobně jste inicializovali projekt dodatečně. Je třeba projekt uložit, zavřít a znovu otevřít.");
                    }
                    if (!initialized && updaterActive)
                    {
                        InterplanDialog.ShowDialog("Varování", "Projekt není inicializován pro myFM, ale parametry jsou ověřovány. Pravděpodobně jste zrušili inicializaci v jednom z kroků 'Zpět'. Inicializujte projekt nástrojem z nabídky 'Interplan', nebo zavřete a znovu otevřete projekt. V tom případě nebudou parametry myFM dále sledovány.");
                    }
                });
        }

        /// <summary>
        /// It may happen during synchronization that ElementID changes and it's week reference to the
        /// underlying model is broken so it is better to remove it from cache and recreate again when needed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnDocumentSynchronized(object sender, DocumentSynchronizedWithCentralEventArgs args)
        {
            var document = args.Document;
            if (_cache.Keys.Contains(document))
                _cache.Remove(document);
        }

        private bool IsInterplanTransaction(IEnumerable<string> names)
        {
            foreach (var name in names)
            {
                if (name.ToLower().Contains("interplan"))
                    return true;
            }
            return false;
        }

        private const string fieldNameData = "Data";
        private const string fieldNameSystems = "Systems";

        private IPSystemCollection GetSystems()
        {
            //get XML string from entity if if exists in there
            var field = _schema.GetField(fieldNameSystems);
            var xmlString = _entity.Get<String>(field);
            if (String.IsNullOrEmpty(xmlString))
            {
                return new IPSystemCollection() { Document = _element.Document};
            }
            else
            {
                var systems = SerializationHelper.Deserialize<IPSystemCollection>(xmlString);
                systems.Document = _element.Document;
                foreach (var system in systems)
                {
                    foreach (var item in system.AllItems)
                    {
                        item.SetDocument(_element.Document);
                    }
                }
                return systems;
            }
        }

        private void SaveSystemsToDocument(IPSystemCollection systems)
        {
            //serialize collection
            var xmlString = SerializationHelper.Serialize<IPSystemCollection>(systems);

            //add resulting XML string to the entity
            var field = _schema.GetField(fieldNameSystems);
            _entity.Set<String>(field, xmlString);

            //save actual settings
            _element.SetEntity(_entity);

        }

        private void LoadData(Document document)
        {
            //get XML string from entity if if exists in there
            var field = _schema.GetField(fieldNameData);
            var xmlString = _entity.Get<String>(field);
            if (String.IsNullOrEmpty(xmlString))
            {
                //create new data if there is nothing specified yet
                _data = new ProjectSettingsData();
            }
            else
            {
                //parse data from the XML string
                _data = SerializationHelper.Deserialize<ProjectSettingsData>(xmlString);
            }
        }

        void OnDocumentClosing(object sender, DocumentClosingEventArgs e)
        {
            //remove this from the static cache
            _cache.Remove(_element.Document);
        }

        private Schema GetStorageSchema()
        {
            var bld = new SchemaBuilder(_schemaGuid);
            bld.SetSchemaName("BimLibraryData");
            bld.SetWriteAccessLevel(AccessLevel.Public);
            bld.SetReadAccessLevel(AccessLevel.Public);
            //bld.SetVendorId("ADSK");
            bld.SetDocumentation("This schema stores project specific application data of Czech BIM Library.");
            bld.AddSimpleField(fieldNameData, typeof(String)).SetDocumentation("Data field");

            return bld.Finish();
        }

       
    }

    public class Notifier : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        } 
    }

    public class ProjectSettingsData : Notifier
	{
        public ProjectSettingsData()
        {
        }

    }
}
