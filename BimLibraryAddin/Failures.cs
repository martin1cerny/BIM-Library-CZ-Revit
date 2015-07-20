using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BimLibraryAddin
{
    public static class Failures
    {

        #region Data validation failures
        public static FailureDefinitionId UserNotDefinedFailure;
        public static FailureDefinitionId CompanyNotDefinedFailure;
        public static FailureDefinitionId IlegalCharsFailure;
        public static FailureDefinitionId WrongDateTimeFailure;
        public static FailureDefinitionId WrongGuidFormatFailure;
        #endregion

        #region Other failures
        public static FailureDefinitionId GeneralFailure;

        #endregion

        public static void RegisterFailures()
        {
            // register the new warning using FailureDefinition
            UserNotDefinedFailure = new FailureDefinitionId(new Guid("8C2951DA-1718-4085-9C8D-4590297322AC"));
            FailureDefinition.CreateFailureDefinition(UserNotDefinedFailure, FailureSeverity.Error,
                "Uživatel musí být definován pro potřeby myFM firmy Interplan.");

            CompanyNotDefinedFailure = new FailureDefinitionId(new Guid("BCE7D8BD-B02E-4E70-9860-A5E329AAD34F"));
            FailureDefinition.CreateFailureDefinition(CompanyNotDefinedFailure, FailureSeverity.Error,
                "Společnost musí být definována pro potřeby myFM firmy Interplan.");

            IlegalCharsFailure = new FailureDefinitionId(new Guid("6F33DC76-365B-4258-A936-9DD35AE5B68B"));
            FailureDefinition.CreateFailureDefinition(IlegalCharsFailure, FailureSeverity.Error,
                "Parametr obsahuje nepovolené znaky.");

            WrongDateTimeFailure = new FailureDefinitionId(new Guid("0ED443E7-E792-4298-B8A5-D5F12F64A4C1"));
            FailureDefinition.CreateFailureDefinition(WrongDateTimeFailure, FailureSeverity.Error,
                "Neplatný formát data.");

            WrongGuidFormatFailure = new FailureDefinitionId(new Guid("58F45187-82D6-49EC-84AE-8F5428B1B78E"));
            FailureDefinition.CreateFailureDefinition(WrongGuidFormatFailure, FailureSeverity.Error,
                "Neplatný formát identifikátoru.");

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
