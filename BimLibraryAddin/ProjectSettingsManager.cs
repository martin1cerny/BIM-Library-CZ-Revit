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
using BimLibraryAddin.Helpers;

namespace BimLibraryAddin
{
    public class ProjectSettingsManager
    {
        private readonly Guid _schemaGuid = new Guid("21AF21DE-157E-4EFE-9BFF-C4CDD4D27714");
        private readonly Element _element;
        private readonly Schema _schema;
        private readonly Entity _entity;
        private ProjectSettingsData _data;
        private static readonly Dictionary<Document, ProjectSettingsManager> Cache = new Dictionary<Document, ProjectSettingsManager>();

        public static ProjectSettingsData GetSettings(Document document)
        {
            ProjectSettingsManager sett = null;
            if (Cache.TryGetValue(document, out sett))
                return sett._data;
            
            var psm = new ProjectSettingsManager(document);
            return psm._data;
        }

        

        private ProjectSettingsManager(Document document)
        {
            var col = new FilteredElementCollector(document).OfClass(typeof(ProjectInfo));
            _element = col.ToElements().First();
            _schema = Schema.Lookup(_schemaGuid) ?? GetStorageSchema();

            //get entity from element if it exists in there already or create new otherwise
            _entity = _element.GetEntity(_schema);
            if (_entity == null || _entity.Schema == null)
                _entity = new Entity(_schema);

            LoadData(document);

            //static cache management
            Cache.Add(document, this);
            document.DocumentClosing += new EventHandler<DocumentClosingEventArgs>(OnDocumentClosing);
            document.Application.DocumentSynchronizedWithCentral += new EventHandler<DocumentSynchronizedWithCentralEventArgs>(OnDocumentSynchronized);
            document.Application.DocumentChanged += new EventHandler<DocumentChangedEventArgs>((sender, args) =>
                {
                    if (args.Operation == UndoOperation.TransactionUndone || args.Operation == UndoOperation.TransactionRedone)
                    {
                       
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
            if (Cache.Keys.Contains(document))
                Cache.Remove(document);
        }

        private const string FieldNameData = "Data";

        private void LoadData(Document document)
        {
            //get XML string from entity if if exists in there
            var field = _schema.GetField(FieldNameData);
            var xmlString = _entity.Get<String>(field);
            _data = String.IsNullOrEmpty(xmlString) ? 
                new ProjectSettingsData() : 
                SerializationHelper.Deserialize<ProjectSettingsData>(xmlString);
        }

        void OnDocumentClosing(object sender, DocumentClosingEventArgs e)
        {
            //serialize actual settings
            var xmlString = SerializationHelper.Serialize<ProjectSettingsData>(_data);

            //add resulting XML string to the entity
            var field = _schema.GetField(FieldNameData);
            _entity.Set(field, xmlString);

            //save actual settings
            _element.SetEntity(_entity);

            //remove this from the static cache
            Cache.Remove(_element.Document);
        }

        private Schema GetStorageSchema()
        {
            var bld = new SchemaBuilder(_schemaGuid);
            bld.SetSchemaName("BimLibraryData");
            bld.SetWriteAccessLevel(AccessLevel.Public);
            bld.SetReadAccessLevel(AccessLevel.Public);
            //bld.SetVendorId("ADSK");
            bld.SetDocumentation("This schema stores project specific application data of Czech BIM Library.");
            bld.AddSimpleField(FieldNameData, typeof(String)).SetDocumentation("Data field");

            return bld.Finish();
        }

       
    }

    public class ProjectSettingsData
	{

    }
}
