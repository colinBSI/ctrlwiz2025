using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace CtrlWiz.RVT
{
    #region Attributes
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    #endregion

    public class CmdLicense : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                if (!ExAppCtrlWizRVT.IsLicenseActive()) { return Result.Cancelled; }

                ExAppCtrlWizRVT.InAppCheckout.ShowLicenseDetailsWindow(out string msg);
                if (msg != null) ExAppCtrlWizRVT.MessageService.ShowError(msg);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                TaskDialog.Show("Error", e.Message);

                return Result.Failed;
            }
        }
    }
}
