using System;
using Autodesk.Revit.ApplicationServices;
using Microsoft.Win32;
namespace Autodesk.Revit.DB
{
    public static class DocumentExtensions
    {
        private static DefinitionFile _definitionFile;
        private const string DefaultGroupName = "bim_library_parameters"; //this is a hack to avoid multiple parameters with the same name.

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
            

            if (categoriesForParam == null) return false;
            if (categoriesForParam.Size == 0) return false;
            foreach (Category cat in categoriesForParam)
            {
                if (cat == null) 
                    return false;
                if (!cat.AllowsBoundParameters) 
                    return false;
            }

            var application = document.Application;
            if (_definitionFile == null)
            {
                //ask for the location of the shared parameters file
                var dialog = new OpenFileDialog();
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

            var myDefinitionFile = _definitionFile;

            // Get parameter or create new one
            var myGroups = myDefinitionFile.Groups;
            Definition myDefinition = null;
            var found = false;

            foreach (var gr in myGroups)
            {
                foreach (var def in gr.Definitions)
                {
                    if (def.Name != parameterName || def.ParameterType != paramType || def.ParameterGroup != @group)
                        continue;
                    myDefinition = def;
                    found = true;
                    break;
                }
                if (found) break;
            }

            //if there is not such a parameter new one is created in default group
            if (myDefinition == null)
            {
                var myGroup = myGroups.get_Item(DefaultGroupName) ?? myGroups.Create(DefaultGroupName);

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
            // Get the BingdingMap of current document. 
            var bindingMap = document.ParameterBindings;
            var binding = bindingMap.get_Item(myDefinition);

            bool bindOk;
            if (type)
            {
                if (binding != null)
                {
                    TypeBinding typeBinding = binding as TypeBinding;
                    if (typeBinding == null)
                        throw new Exception("Parameter with this definition already exists and is bound to instances. It cannot be bound to the type at the same time");
                    foreach (Category cat in categoriesForParam) typeBinding.Categories.Insert(cat);
                    bindOk = bindingMap.ReInsert(myDefinition, binding, group);
                    return bindOk;
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
                    var instBinding = binding as InstanceBinding;
                    if (instBinding == null)
                        throw new Exception("Parameter with this definition already exists and is bound to types. It cannot be bound to the instance at the same time");
                    foreach (Category cat in categoriesForParam) instBinding.Categories.Insert(cat);
                    bindOk = bindingMap.ReInsert(myDefinition, binding, group);
                    return bindOk;
                }

                binding = application.Create.NewInstanceBinding(categoriesForParam);
            }


            // Bind the definitions to the document 
            bindOk = bindingMap.Insert(myDefinition, binding, group);
            return bindOk;
        }
    
        #if Revit2014 || Revit2015 || Revit2016
        public static bool RemoveSharedPropertiesBinding(this Document document, ParameterType paramType, CategorySet categoriesForParam, string parameterName, BuiltInParameterGroup group, bool type, bool visible, Guid id)
#else
        public static bool RemoveSharedPropertiesBinding(this Document document, ParameterType paramType, CategorySet categoriesForParam, string parameterName, BuiltInParameterGroup group, bool type)
#endif
        {
            if (categoriesForParam == null) return false;
            if (categoriesForParam.Size == 0) return false;
            foreach (Category cat in categoriesForParam)
            {
                if (cat == null) 
                    return false;
                if (!cat.AllowsBoundParameters) 
                    return false;
            }

            if (_definitionFile == null)
            {
                //ask for the location of the shared parameters file
                var dialog = new OpenFileDialog();
                dialog.CheckFileExists = false;
                dialog.Title = "Set shared parameter file...";
                dialog.ShowDialog();
                var shrFilePath = dialog.FileName;
                if (!String.IsNullOrEmpty(shrFilePath))
                {
                    document.SetDefinitionFile(shrFilePath);
                }
            }
            if (_definitionFile == null) 
                throw new Exception("Definition file must be set before creation or removeal of the new parameters.");

            var myDefinitionFile = _definitionFile;

            // Get parameter or create new one
            var myGroups = myDefinitionFile.Groups;
            Definition myDefinition = null;
            var found = false;

            foreach (var gr in myGroups)
            {
                foreach (var def in gr.Definitions)
                {
                    if (def.Name != parameterName || def.ParameterType != paramType || def.ParameterGroup != @group)
                        continue;
                    myDefinition = def;
                    found = true;
                    break;
                }
                if (found) break;
            }

            //if there is not such a parameter new one is created in default group
            if (myDefinition == null)
            {
                DefinitionGroup myGroup = myGroups.get_Item(DefaultGroupName) ?? myGroups.Create(DefaultGroupName);

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
            // Get the BingdingMap of current document. 
            var bindingMap = document.ParameterBindings;
            var binding = bindingMap.get_Item(myDefinition);

            var elementBinding = binding as ElementBinding;
            //no such binding exist so it doesn't have to be removed
            if (elementBinding == null)
                return true;
            
            foreach (Category cat in categoriesForParam) 
                elementBinding.Categories.Erase(cat);
            var bindOk = bindingMap.ReInsert(myDefinition, binding, group);
                    return bindOk;
        }
    
    }

}
