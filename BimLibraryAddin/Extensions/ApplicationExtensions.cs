using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;
using System.IO;

namespace Autodesk.Revit.DB
{
    public static class ApplicationExtensions
    {
        /// <summary>
        /// Get definition file of the shared parameters. If it does not exist it is created
        /// </summary>
        /// <param name="application">Revit applivation object</param>
        /// <param name="sharedParameterFile">Path to the shared parameter file</param>
        /// <returns></returns>
        public static DefinitionFile GetSharedParamDefinitionFile(this Application application, string sharedParameterFile)
        {
            //check for existence of the file. If it does not exist it is created
            if (!File.Exists(sharedParameterFile))
            {
                System.IO.FileStream fileStream = System.IO.File.Create(sharedParameterFile);
                fileStream.Close();
            }

            // set the path of shared parameter file to current Revit 
            application.SharedParametersFilename = sharedParameterFile;
            // open the file 
            return application.OpenSharedParameterFile();
        }
    }
}
