using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using System.IO;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System.Diagnostics;

namespace Autodesk.Revit.DB
{
    //shared parameters extensions:
    public static class ElementExtensions
    {
        private static DefinitionFile _definitionFile = null;
        
        public static void SetSharedParamFileInUserProgramFolder(Document rvtDocument)
        {
            string appTempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "4Projects", "SharedParameterFiles");
            if (!Directory.Exists(appTempPath)) Directory.CreateDirectory(appTempPath);
            string sharedParameterFile = Path.Combine(appTempPath, "4P_shared_parameters.txt");

            //set shared parameter file
            rvtDocument.Application.SharedParametersFilename = sharedParameterFile;
            _definitionFile = rvtDocument.Application.GetSharedParamDefinitionFile(sharedParameterFile);
        }

        public static DefinitionFile SetDefinitionFile(this Element element, string sharedParameterFile)
        {
            Application application = element.Document.Application;
            //if it is already open it is returned from the cashed static field
            if (_definitionFile != null && application.SharedParametersFilename == sharedParameterFile) return _definitionFile;
            _definitionFile = application.GetSharedParamDefinitionFile(sharedParameterFile);
            return _definitionFile;
        }

        /// <summary>
        ///  Creates new shared parameter if it does not exist and bind it to the type or instance of the objects 
        /// </summary>
        /// <param name="paramType">Type of the parameter</param>
        /// <param name="categoriesForParam">Category of elements to bind the parameter to</param>
        /// <param name="defaultGroupName">Group name of the parameters</param>
        /// <param name="parameterName">Name of the parameter</param>
        /// <returns>TRUE if shared parameter is created, FALSE otherwise</returns>
        public static bool SetNewSharedParameter(this Element element, ParameterType paramType, CategorySet categoriesForParam, string parameterName, BuiltInParameterGroup group)
        {
            string defaultGroupName = "4P_imported_parameters"; //this is a hack to avoid multiple parameters with the same name.

            if (categoriesForParam == null) return false;
            if (categoriesForParam.Size == 0) return false;
            foreach (Category cat in categoriesForParam)
            {
                if (cat == null) return false;
                if (!cat.AllowsBoundParameters) return false;
            }

            Application application = element.Document.Application;
            Document document = element.Document;
            if (_definitionFile == null)
            {
                //ask for the location of the shared parameters file
                var dialog = new Microsoft.Win32.OpenFileDialog();
                dialog.CheckFileExists = false;
                dialog.Title = "Set shared pID file...";
                dialog.ShowDialog();
                string shrFilePath = dialog.FileName;
                if (shrFilePath == null)
                {
                    _definitionFile = application.OpenSharedParameterFile();
                    if (_definitionFile == null) SetSharedParamFileInUserProgramFolder(document);
                }
                else
                {
                    SetDefinitionFile(element, shrFilePath);
                }
            }
            if (_definitionFile == null) throw new Exception("Definition file must be set before creation of the new parameters.");

            DefinitionFile myDefinitionFile = _definitionFile;

            // Get parameter or create new one
            DefinitionGroups myGroups = myDefinitionFile.Groups;
            Definition myDefinition = null;
            bool found = false;
            
            foreach (DefinitionGroup gr in myGroups)
            {
                foreach (Definition def in gr.Definitions)
                {
                    if (def.Name == parameterName)
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
                    myDefinition = myGroup.Definitions.Create(parameterName, paramType);
                    
                }
            }
            

            //Create an object of TypeBinding or InstanceBinding according to the Categories and "typeBinding" variable
            Binding binding = null;
            // Get the BingdingMap of current document. 
            BindingMap bindingMap = document.ParameterBindings;
            binding = bindingMap.get_Item(myDefinition);

            bool bindOK = false;
            if (!element.CanHaveTypeAssigned() && !(element is Material) && !(element is ProjectInfo))
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

        /// <summary>
        /// Returns shared or built-in parameter of the specified name. parameter.IsShared property identifies the type.
        /// If it does not exist it is created in the shared parameters file with specified name in the specified group. That must be defined in advance
        /// using SetDefinitionFile() function.
        /// </summary>
        /// <param name="paramType">Type of the parameter (simplified to base data types)</param>
        /// <param name="groupName">Name of the group of the shared parameters</param>
        /// <param name="parameterName">Name of the parameter</param>
        /// <returns>Existing or new parameter</returns>
        public static Parameter GetParameter(this Element element, ParameterType paramType, string groupName, string parameterName)
        {
            return GetParameter(element, paramType, parameterName, BuiltInParameterGroup.PG_IFC);
        }

        /// <summary>
        /// Returns shared or built-in parameter of the specified name. parameter.IsShared property identifies the type.
        /// If it does not exist it is created in the shared parameters file with specified name in the specified group. That must be defined in advance
        /// using SetDefinitionFile() function.
        /// </summary>
        /// <param name="paramType">Type of the parameter (simplified to base data types)</param>
        /// <param name="groupName">Name of the group of the shared parameters</param>
        /// <param name="parameterName">Name of the parameter</param>
        /// <param name="group">Revit built in parameter group where the parameter is to be created if it doesn't exist</param>
        /// <returns>Existing or new parameter</returns>
        public static Parameter GetParameter(this Element element, ParameterType paramType, string parameterName, BuiltInParameterGroup group)
        {
            Parameter parameter = element.get_Parameter(parameterName);
            if (parameter != null)
            {
                //check type and throw error if the type is different
                if (parameter.Definition.ParameterType == paramType)
                    return parameter; //returns existing parameter (it could be any type of parameter not only shared parameter)
                else
                    throw new Exception(String.Format("Parameter type mismatch for pID '{0}': existing type: {1}, desired type {2}.", parameterName, parameter.Definition.ParameterType, paramType));
            }

            // Create a category set and insert category of wall to it 
            CategorySet myCategories = element.Document.Application.Create.NewCategorySet();
            // Use BuiltInCategory to get category of wall 
            Category myCategory = element.Category;
            
            myCategories.Insert(myCategory);

            bool success = SetNewSharedParameter(element, paramType, myCategories, parameterName, group);

            if (!success)
            {
                Debug.WriteLine("Parameter creation not successfull.\nParameter name: " + parameterName + "\n Existing parameters:");
                foreach (Parameter par in element.Parameters)
                {
                    Debug.WriteLine("Parameter name: " + par.Definition.Name + ", group: " + Enum.GetName(typeof(BuiltInParameterGroup), par.Definition.ParameterGroup));
                }
                throw new Exception("Parameter creation failed.");
            }

            parameter = element.get_Parameter(parameterName);
            return parameter;
        }

        public static string GetParameter_string(this Element element, string parameterName)
        {
            Parameter parameter = element.get_Parameter(parameterName);
            if (parameter != null)
            {
                return parameter.AsString();
            }
            return null;
        }

        public static int? GetParameter_integer(this Element element, string parameterName)
        {
            Parameter parameter = element.get_Parameter(parameterName);
            if (parameter != null)
            {
                return parameter.AsInteger();
            }
            return null;
        }

        public static double? GetParameter_double(this Element element, string parameterName)
        {
            Parameter parameter = element.get_Parameter(parameterName);
            if (parameter != null)
            {
                return parameter.AsDouble();
            }
            return null;
        }

        public static bool? GetParameter_bool(this Element element, string parameterName)
        {
            Parameter parameter = element.get_Parameter(parameterName);
            if (parameter != null)
            {
                return parameter.AsInteger() == 0;
            }
            return null;
        }

     
     

     
        public static void SetDescription(this Element element, string description)
        {
            Parameter parameter = element.get_Parameter("Description");
            if (parameter != null)
            {
                parameter.Set(description);
            }
        }

        public static string GetDescription(this Element element)
        {
            Parameter parameter = element.get_Parameter("Description");
            if (parameter != null)
            {
                return parameter.AsString();
            }
            return null;
        }
    }
}
