using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace BimLibraryAddin.Helpers
{
    public static class SerializationHelper
    {
        public static string Serialize<T>(T value)
        {

            if (value == null)
            {
                return null;
            }

            XmlSerializer serializer = new XmlSerializer(typeof(T));

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8; 
            settings.Indent = true;
            settings.OmitXmlDeclaration = false;

            

            using (var ms = new MemoryStream())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(ms, settings))
                {
                    serializer.Serialize(xmlWriter, value);
                }
                ms.Position = 0;
                var reader = new StreamReader(ms);
                return reader.ReadToEnd();
            }
        }

        public static T Deserialize<T>(string xml)
        {

            if (string.IsNullOrEmpty(xml))
            {
                return default(T);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(T));

            XmlReaderSettings settings = new XmlReaderSettings();
            // No settings need modifying here

            using (StringReader textReader = new StringReader(xml))
            {
                using (XmlReader xmlReader = XmlReader.Create(textReader, settings))
                {
                    return (T)serializer.Deserialize(xmlReader);
                }
            }
        }
    }
}
