using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace BimLibraryAddin
{
    public static class Failures
    {

        #region Other failures
        public static FailureDefinitionId GeneralFailureId;
        public static FailureDefinition GeneralFailure;

        #endregion

        public static void RegisterFailures()
        {

            GeneralFailureId = new FailureDefinitionId(new Guid("14281B4C-0AD0-4654-88DD-68E2021F4131"));
            GeneralFailure = FailureDefinition.CreateFailureDefinition(GeneralFailureId, FailureSeverity.Error,
                "Jejda, někde se stala chyba. Snad se to již nebude opakovat. Podívejte se prosím do souboru "+ Paths.ErrorFile);
        }

        public static void PostFailure(Document document, Exception exception)
        {
            PostFailure(document, new Exception[] { exception});
        }

        public static void PostFailure(Document document, IEnumerable<Exception> exceptions)
        {
            var log = new ExceptionLog();
            log.Add(exceptions);
            log.SaveToFile(Paths.ErrorFile);

            var msg = new FailureMessage(GeneralFailureId);
            document.PostFailure(msg);
        }

        public static void PostFailure(Document document, FailureDefinitionId failureId)
        {
            var msg = new FailureMessage(failureId);
            document.PostFailure(msg);
        }

        public static void PostFailure(Document document, FailureDefinitionId failureId, ElementId elementId)
        {
            var msg = new FailureMessage(failureId);
            msg.SetFailingElement(elementId);
            document.PostFailure(msg);
        }
    }
}
