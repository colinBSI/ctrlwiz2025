using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CtrlWiz.Forms.Presenters;

namespace CtrlWiz.RVT
{
    #region Attributes
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    #endregion

    public class CmdHelp : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                if (!ExAppCtrlWizRVT.IsLicenseActive()) { return Result.Cancelled; }

                HelpPresenterRVT helpPresenter = new HelpPresenterRVT();
                helpPresenter.ShowDialog();

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
