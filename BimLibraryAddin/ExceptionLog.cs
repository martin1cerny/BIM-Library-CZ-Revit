using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BimLibraryAddin
{
    /// <summary>
    /// This class stores exceptions in form of SimpleExceptions 
    /// which can be serialized to XML easily. It also contains functions
    /// to save exceptions to XML file and open them for later examination.
    /// </summary>
    public class ExceptionLog
    {
        private List<SimpleException> _exceptions = new List<SimpleException>();
        public IEnumerable<SimpleException> Exceptions
        {
            get
            {
                foreach (var item in _exceptions)
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Adds exception to the list and converts is to SimpleException
        /// </summary>
        /// <param name="e">Exception to be added</param>
        public void Add(Exception e)
        {
            _exceptions.Add( new SimpleException(e));
        }

        /// <summary>
        /// Adds group of exceptions
        /// </summary>
        /// <param name="exceptions">Exceptions</param>
        public void Add(IEnumerable<Exception> exceptions)
        {
            foreach (var e in exceptions)
            {
                _exceptions.Add(new SimpleException(e));
            }
        }

        public void Clear()
        {
            _exceptions.Clear();
        }

        /// <summary>
        /// Checks if there are any exceptions
        /// </summary>
        /// <returns></returns>
        public bool Any()
        {
            return _exceptions.Any();
        }

        /// <summary>
        /// Saves exceptions to XML file
        /// </summary>
        /// <param name="file">Path to the file. If the file exists it is truncated and overwritten.</param>
        public void SaveToFile(string file)
        {
            using (var writer = new System.Xml.XmlTextWriter(file, Encoding.UTF8) { Formatting = System.Xml.Formatting.Indented })
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(_exceptions.GetType());
                serializer.Serialize(writer, _exceptions);
                writer.Close();    
            }
        }

        /// <summary>
        /// Loads SimpleExceptions from the file specified
        /// </summary>
        /// <param name="file"></param>
        public void LoadFromFile(string file)
        {
            if (!File.Exists(file))
                throw new FileNotFoundException();
            using (var stream = File.Open(file, FileMode.Open))
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(_exceptions.GetType());
                _exceptions = serializer.Deserialize(stream) as List<SimpleException>;
                stream.Close();
            }
        }

    }

    /// <summary>
    /// This class is to be used for serialization of the exception to XML
    /// </summary>
    public class SimpleException
    {
        public string Message { get; set; }
        public string Source { get; set; }
        public string StackTrace { get; set; }
        public SimpleException InnerException { get; set; }
        public string Type { get; set; }

        public SimpleException()
        {

        }

        public SimpleException(Exception e)
        {
            Message = e.Message;
            Source = e.Source;
            StackTrace = e.StackTrace;
            Type = e.GetType().FullName;
            if (e.InnerException != null)
                InnerException = new SimpleException(e.InnerException);
        }
    }
}
