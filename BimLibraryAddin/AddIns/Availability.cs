using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BimLibraryAddin.AddIns
{
    class DocumentAvailability : Autodesk.Revit.UI.IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication a, CategorySet b)
        {
            if (a.ActiveUIDocument == null) return false;
            if (a.ActiveUIDocument.Document == null) return false;
            if (a.ActiveUIDocument.Document.IsFamilyDocument) return false;
            return true;
        }
    }
}
