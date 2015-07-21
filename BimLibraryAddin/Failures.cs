using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace BimLibraryAddin
{
    public static class Failures
    {

        #region Data validation failures
        #endregion

        #region Other failures
        public static FailureDefinitionId GeneralFailure;

        #endregion

        public static void RegisterFailures()
        {

            GeneralFailure = new FailureDefinitionId(new Guid("2AE7F8A8-34A4-4DAC-A753-6218DBCC9806"));
            FailureDefinition.CreateFailureDefinition(GeneralFailure, FailureSeverity.Error,
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

            var msg = new FailureMessage(GeneralFailure);
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
