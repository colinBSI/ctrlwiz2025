using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CtrlWiz.RVT.States;
using CtrlWiz.Forms.Presenters;
using System;

namespace CtrlWiz.RVT
{
    #region Attributes
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    #endregion

    public class CmdSettings : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                if (!ExAppCtrlWizRVT.IsLicenseActive()) { return Result.Cancelled; }

                SpeedSettingPresenter speedSettingPresenter = new SpeedSettingPresenter(SpeedSettings.LinearSpeed, SpeedSettings.AngularSpeed, SpeedSettings.LinearSpeedFactor);
                if (speedSettingPresenter.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                {
                    SpeedSettings.LinearSpeed = speedSettingPresenter.LinearSetting;
                    SpeedSettings.AngularSpeed = speedSettingPresenter.AngularSetting;
                }

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
