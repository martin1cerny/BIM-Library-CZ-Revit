using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using BimLibraryAddin.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BimLibraryAddin.AddIns
{
    [Regeneration(RegenerationOption.Manual), Transaction(TransactionMode.Manual)]
    public class AboutAddIn : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            var dlg = new BimLibraryAddin.Dialogs.About();
            var handle = new RevitHandle();
            handle.SetAsOwnerTo(dlg);

            dlg.ShowDialog();

            return Result.Succeeded;
        }
    }
}
