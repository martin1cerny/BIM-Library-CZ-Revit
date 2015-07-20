using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodesk.Revit.DB
{
    public static class DocumentExtensions
    {
        private static DefinitionFile _definitionFile = null;

        public static DefinitionFile SetDefinitionFile(this Document document, string sharedParameterFile)
        {
            Application application = document.Application;
            //if it is already open it is returned from the cashed static field
            if (_definitionFile != null && application.SharedParametersFilename == sharedParameterFile)
                return _definitionFile;
            else
            {
                _definitionFile = application.GetSharedParamDefinitionFile(sharedParameterFile);
                return _definitionFile;
            }
        }
#if Revit2015 || Revit2016
        public static bool SetSharedPropertiesBinding(this Document document, ParameterType paramType, CategorySet categoriesForParam, string parameterName, BuiltInParameterGroup group, bool type, bool visible, bool editable, Guid id)
#elif Revit2014
        public static bool SetSharedPropertiesBinding(this Document document, ParameterType paramType, CategorySet categoriesForParam, string parameterName, BuiltInParameterGroup group, bool type, bool visible, Guid id)
#else
        public static bool SetSharedPropertiesBinding(this Document document, ParameterType paramType, CategorySet categoriesForParam, string parameterName, BuiltInParameterGroup group, bool type)
#endif
        {
            string defaultGroupName = "interplan_parameters"; //this is a hack to avoid multiple parameters with the same name.

            if (categoriesForParam == null) return false;
            if (categoriesForParam.Size == 0) return false;
            foreach (Category cat in categoriesForParam)
            {
                if (cat == null) 
                    return false;
                if (!cat.AllowsBoundParameters) 
                    return false;
            }

            Application application = document.Application;
            if (_definitionFile == null)
            {
                //ask for the location of the shared parameters file
                var dialog = new Microsoft.Win32.OpenFileDialog();
                dialog.CheckFileExists = false;
                dialog.Title = "Set shared parameter file...";
                dialog.ShowDialog();
                string shrFilePath = dialog.FileName;
                if (!String.IsNullOrEmpty(shrFilePath))
                {
                    document.SetDefinitionFile(shrFilePath);
                }
            }
            if (_definitionFile == null) 
                throw new Exception("Definition file must be set before creation of the new parameters.");

            DefinitionFile myDefinitionFile = _definitionFile;

            // Get parameter or create new one
            DefinitionGroups myGroups = myDefinitionFile.Groups;
            Definition myDefinition = null;
            bool found = false;

            foreach (DefinitionGroup gr in myGroups)
            {
                foreach (Definition def in gr.Definitions)
                {
                    if (def.Name == parameterName && def.ParameterType == paramType && def.ParameterGroup == group)
                    {
                        myDefinition = def;
                        found = true;
                        break;
                    }
                }
                if (found) break;
            }

            //if there is not such a parameter new one is created in default group
            if (myDefinition == null)
            {
                DefinitionGroup myGroup = myGroups.get_Item(defaultGroupName);
                if (myGroup == null)
                {
                    myGroup = myGroups.Create(defaultGroupName);
                }

                // Create a type definition 
                myDefinition = myGroup.Definitions.get_Item(parameterName);
                if (myDefinition == null)
                {
#if Revit2015 || Revit2016
                    var options = new ExternalDefinitonCreationOptions(parameterName, paramType) { 
                        UserModifiable = editable, 
                        Visible = visible, 
                        GUID = id
                    };
                    myDefinition = myGroup.Definitions.Create(options);
#elif Revit2014 
                    myDefinition = myGroup.Definitions.Create(parameterName, paramType, visible, ref id);
#else
                    myDefinition = myGroup.Definitions.Create(parameterName, paramType);
#endif
                }
            }


            //Create an object of TypeBinding or InstanceBinding according to the Categories and "typeBinding" variable
            Binding binding = null;
            // Get the BingdingMap of current document. 
            BindingMap bindingMap = document.ParameterBindings;
            binding = bindingMap.get_Item(myDefinition);

            bool bindOK = false;
            if (type)
            {
                if (binding != null)
                {
                    TypeBinding typeBinding = binding as TypeBinding;
                    if (typeBinding == null)
                        throw new Exception("Parameter with this definition already exists and is bound to instances. It cannot be bound to the type at the same time");
                    foreach (Category cat in categoriesForParam) typeBinding.Categories.Insert(cat);
                    bindOK = bindingMap.ReInsert(myDefinition, binding, group);
                    return bindOK;
                }
                else
                {
                    binding = application.Create.NewTypeBinding(categoriesForParam);
                }
            }
            else
            {
                if (binding != null)
                {
                    InstanceBinding instBinding = binding as InstanceBinding;
                    if (instBinding == null)
                        throw new Exception("Parameter with this definition already exists and is bound to types. It cannot be bound to the instance at the same time");
                    foreach (Category cat in categoriesForParam) instBinding.Categories.Insert(cat);
                    bindOK = bindingMap.ReInsert(myDefinition, binding, group);
                    return bindOK;
                }
                else
                {
                    binding = application.Create.NewInstanceBinding(categoriesForParam);
                }
            }



            // Bind the definitions to the document 
            bindOK = bindingMap.Insert(myDefinition, binding, group);
            return bindOK;
        }
    
        #if Revit2014 || Revit2015 || Revit2016
        public static bool RemoveSharedPropertiesBinding(this Document document, ParameterType paramType, CategorySet categoriesForParam, string parameterName, BuiltInParameterGroup group, bool type, bool visible, Guid id)
#else
        public static bool RemoveSharedPropertiesBinding(this Document document, ParameterType paramType, CategorySet categoriesForParam, string parameterName, BuiltInParameterGroup group, bool type)
#endif
        {
            string defaultGroupName = "interplan_parameters"; //this is a hack to avoid multiple parameters with the same name.

            if (categoriesForParam == null) return false;
            if (categoriesForParam.Size == 0) return false;
            foreach (Category cat in categoriesForParam)
            {
                if (cat == null) 
                    return false;
                if (!cat.AllowsBoundParameters) 
                    return false;
            }

            Application application = document.Application;
            if (_definitionFile == null)
            {
                //ask for the location of the shared parameters file
                var dialog = new Microsoft.Win32.OpenFileDialog();
                dialog.CheckFileExists = false;
                dialog.Title = "Set shared parameter file...";
                dialog.ShowDialog();
                string shrFilePath = dialog.FileName;
                if (!String.IsNullOrEmpty(shrFilePath))
                {
                    document.SetDefinitionFile(shrFilePath);
                }
            }
            if (_definitionFile == null) 
                throw new Exception("Definition file must be set before creation or removeal of the new parameters.");

            DefinitionFile myDefinitionFile = _definitionFile;

            // Get parameter or create new one
            DefinitionGroups myGroups = myDefinitionFile.Groups;
            Definition myDefinition = null;
            bool found = false;

            foreach (DefinitionGroup gr in myGroups)
            {
                foreach (Definition def in gr.Definitions)
                {
                    if (def.Name == parameterName && def.ParameterType == paramType && def.ParameterGroup == group)
                    {
                        myDefinition = def;
                        found = true;
                        break;
                    }
                }
                if (found) break;
            }

            //if there is not such a parameter new one is created in default group
            if (myDefinition == null)
            {
                DefinitionGroup myGroup = myGroups.get_Item(defaultGroupName);
                if (myGroup == null)
                {
                    myGroup = myGroups.Create(defaultGroupName);
                }

                // Create a type definition 
                myDefinition = myGroup.Definitions.get_Item(parameterName);
                if (myDefinition == null)
                {
#if Revit2014 || Revit2015 || Revit2016
                    myDefinition = myGroup.Definitions.Create(parameterName, paramType, visible, ref id);
#else
                    myDefinition = myGroup.Definitions.Create(parameterName, paramType);
#endif
                }
            }

            //Create an object of TypeBinding or InstanceBinding according to the Categories and "typeBinding" variable
            Binding binding = null;
            // Get the BingdingMap of current document. 
            BindingMap bindingMap = document.ParameterBindings;
            binding = bindingMap.get_Item(myDefinition);

            var elementBinding = binding as ElementBinding;
            //no such binding exist so it doesn't have to be removed
            if (elementBinding == null)
                return true;
            
            foreach (Category cat in categoriesForParam) 
                elementBinding.Categories.Erase(cat);
            var bindOK = bindingMap.ReInsert(myDefinition, binding, group);
                    return bindOK;
        }
    
    }

}
