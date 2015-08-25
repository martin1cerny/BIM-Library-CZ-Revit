using System;
using System.IO;

namespace BimLibraryAddin
{
    public static class Paths
    {

        public static string BimServiceEndpoint { get { return "http://www.narodni-bim-knihovna.cz/BIMservice.svc"; } }

        public static string WorkingPath
        {
            get 
            {
                var dir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                dir = Path.Combine(dir, "BimLibraryCZ");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                return dir;
            }
        }



        public static string ErrorFile
        {
            get
            {
                var file = Path.Combine(WorkingPath, "Error.txt");
                if (!File.Exists(file))
                    File.CreateText(file).Close();
                return file;
            }
        }

        public static string SharedParamFile
        {
            get
            {
                var file = Path.Combine(WorkingPath, "SharedParamsFile.txt");
                if (!File.Exists(file))
                    File.CreateText(file).Close();
                return file;
            }
        }
    }
}
