using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Numerics;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using XInputDotNetPure;
using CtrlWiz.RVT.Utility;
using CtrlWiz.RVT.States;
using CtrlWiz.RVT.Dialogs;

namespace CtrlWiz.RVT
{
    #region Attributes
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    #endregion

    public class CmdActivateController : IExternalCommand
    {
        private GamePadState state;
        private GamePadState prevState;
        private Stopwatch sw;
        private static bool isRun = false;
        private bool isMoving = false;
        private bool isRotating = false;
        private long timeDeltatime;
        private byte speed = 1;
        private const float constantUnit = 3.28084f;
        //private static Transaction transaction = null;
        //private static UIDocument uiDoc = null;
        private static UIApplication uIApplication = null;
        private View3D view3D = null;
        //private static int? shadowIntensity = 0;
        // private bool? isRendering = null;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                if (!ExAppCtrlWizRVT.IsLicenseActive()) { return Result.Cancelled; }

                //uiDoc = uIApplication.ActiveUIDocument;
                //uiDoc = commandData.Application.ActiveUIDocument;
                //view3D = (uiDoc.Document.ActiveView) as View3D;
                //commandData.Application.ApplicationClosing += Application_ApplicationClosing;

                if (!isRun)
                {
                    uIApplication = commandData.Application;

                    view3D = uIApplication.ActiveUIDocument.ActiveView as View3D;

                    //shadowIntensity = view3D?.ShadowIntensity;

                    uIApplication.ViewActivating += UIApplication_ViewActivating;

                    using (Transaction transaction = new Transaction(uIApplication.ActiveUIDocument.Document, "Unlock the view"))
                    {
                        if (transaction.Start("Transaction Get Started") == TransactionStatus.Started)
                        {
                            InitializeController();
                        }
                        transaction.Commit();
                    }
                }
                else
                {
                    //PerspectiveViewNotActiveDialogBox.ShowDialog();

                    return Result.Cancelled;
                }
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                TaskDialog.Show("Error", e.Message);
                return Result.Failed;
            }
        }

        private void UIApplication_ViewActivating(object sender, Autodesk.Revit.UI.Events.ViewActivatingEventArgs e)
        {
            view3D = e.NewActiveView as View3D;
            //shadowIntensity = view3D.ShadowIntensity;
        }

        private void Application_ApplicationClosing(object sender, Autodesk.Revit.UI.Events.ApplicationClosingEventArgs e)
        {
            isRun = false;
        }

        private async void InitializeController()
        {
            isRun = true;
            await VibrateOnStart();
            await Update();
        }

        private async Task VibrateOnStart(int delay = 1, float p = 0.35f)
        {
            GamePad.SetVibration(PlayerIndex.One, p, p);
            await Task.Delay(delay * 1000);
            GamePad.SetVibration(PlayerIndex.One, 0, 0);
        }

        private async void Vibrate(int delay, float p)
        {
            await VibrateOnStart(delay, p);
        }

        private async Task Update()
        {
            while (isRun)
            {
                if (uIApplication.ActiveUIDocument != null && view3D != null)
                {
                    if (view3D.Id != uIApplication.ActiveUIDocument.Document.ActiveView.Id)
                    {
                        using (View3D newView3D = (uIApplication.ActiveUIDocument.Document.ActiveView) as View3D)
                        {
                            if (newView3D == null)
                            {
                                await Task.Delay(10);
                                continue;
                            }
                            view3D = newView3D;
                        }

                    }
                    // Set previous/current buttons state.
                    prevState = state;
                    state = GamePad.GetState(PlayerIndex.One);

                    if (!view3D.IsPerspective)
                    {
                        if (Math.Abs(state.ThumbSticks.Left.X) >= 0.002 ||
                            Math.Abs(state.ThumbSticks.Left.Y) >= 0.002 ||
                            state.Triggers.Left >= 0.002 ||
                            state.Triggers.Right >= 0.002 ||
                            Math.Abs(state.ThumbSticks.Right.X) >= 0.002 ||
                            Math.Abs(state.ThumbSticks.Right.Y) >= 0.002)
                        {
                            PerspectiveViewNotActiveDialogBox.ShowDialog();
                        }
                        await Task.Delay(10);
                        continue;
                    }

                    if (view3D.IsLocked)
                    {
                        view3D.Unlock();
                    }

                    //if (view3D.ShadowIntensity>0)
                    //{
                    //    view3D.ShadowIntensity = 0;
                    //}

                    try
                    {
                        sw = Stopwatch.StartNew();

                        //if ((isMoving || isRotating))
                        //{
                        //    DisplayStyle displayStyle = view3D.DisplayStyle;

                        //    switch (displayStyle)
                        //    {
                        //        case DisplayStyle.Shading:
                        //            view3D.DisplayStyle = DisplayStyle.ShadingWithEdges;
                        //            break;
                        //        case DisplayStyle.ShadingWithEdges:
                        //            view3D.DisplayStyle = DisplayStyle.Shading;
                        //            break;
                        //        case DisplayStyle.Realistic:
                        //            view3D.DisplayStyle = DisplayStyle.RealisticWithEdges;
                        //            break;
                        //        case DisplayStyle.RealisticWithEdges:
                        //            view3D.DisplayStyle = DisplayStyle.Realistic;
                        //            break;
                        //    }
                        //}

                        DisplayStyle displayStyle = view3D.DisplayStyle;

                        if ((isMoving || isRotating) && displayStyle == DisplayStyle.RealisticWithEdges)
                        {
                            //view3D.ShadowIntensity = 0;
                            view3D.DisplayStyle = DisplayStyle.Realistic;
                        }

                        if (!isMoving && !isRotating && displayStyle == DisplayStyle.Realistic)
                        {
                            //view3D.ShadowIntensity = (int)shadowIntensity;
                            view3D.DisplayStyle = DisplayStyle.RealisticWithEdges;
                        }

                        if ((isMoving || isRotating) && displayStyle == DisplayStyle.ShadingWithEdges)
                        {
                            //view3D.ShadowIntensity = 0;
                            view3D.DisplayStyle = DisplayStyle.Shading;
                        }

                        if (!isMoving && !isRotating && displayStyle == DisplayStyle.Shading)
                        {
                            //view3D.ShadowIntensity = (int)shadowIntensity;
                            view3D.DisplayStyle = DisplayStyle.ShadingWithEdges;
                        }
                        //// Set previous/current buttons state.
                        //prevState = state;
                        //state = GamePad.GetState(PlayerIndex.One);

                        //if Back button Pressed, exit the loop.
                        if (prevState.Buttons.Back == ButtonState.Released && state.Buttons.Back == ButtonState.Pressed)
                        {
                            Vibrate(1, 0.35f);
                            isRun = false;
                            //transaction = null;
                            break;
                        }

                        // Move Camera.
                        if (Math.Abs(state.ThumbSticks.Left.X) >= 0.002 ||
                            Math.Abs(state.ThumbSticks.Left.Y) >= 0.002 ||
                            state.Triggers.Left >= 0.002 || state.Triggers.Right >= 0.002)
                        {

                            float currentZ;
                            if (state.Triggers.Left >= 0.002)
                            {
                                currentZ = -0.55f * state.Triggers.Left;
                            }
                            else if (state.Triggers.Right >= 0.002)
                                currentZ = 0.55f * state.Triggers.Right;
                            else
                                currentZ = 0;

                            MoveCamera(state.ThumbSticks.Left.X, state.ThumbSticks.Left.Y, currentZ);
                        }
                        else
                        {
                            isMoving = false;
                        }

                        // Rotate Camera.
                        if (Math.Abs(state.ThumbSticks.Right.X) >= 0.002 || Math.Abs(state.ThumbSticks.Right.Y) >= 0.002)
                        {
                            RotateCamera(-state.ThumbSticks.Right.X, state.ThumbSticks.Right.Y);
                        }
                        else
                        {
                            isRotating = false;
                        }

                        //// Refresh the view with a lower display style on each iteration.
                        //// If no moving/rotating occur after the last iteration, mark this with flag. 
                        //if (isMoving || isRotating)
                        //{
                        //    RefreshActiveView();
                        //    isRendering = true;
                        //}
                        //else if (isRendering == true)
                        //{
                        //    isRendering = false;
                        //}

                        // Left Stick button Pressed.
                        if (prevState.Buttons.LeftStick == ButtonState.Released && state.Buttons.LeftStick == ButtonState.Pressed)
                        {
                            speed = 3;
                        }

                        // Left Stick button Released.
                        if (prevState.Buttons.LeftStick == ButtonState.Pressed && state.Buttons.LeftStick == ButtonState.Released)
                        {
                            speed = 1;
                        }
                        //if (!isMoving || !isRotating)
                        //{
                        uIApplication.ActiveUIDocument.RefreshActiveView();
                        //}

                        timeDeltatime = sw.ElapsedTicks * 1000 / Stopwatch.Frequency;

                        sw.Stop();

                        //// Refresh the view to apply the original DisplayStyle swapped in RefreshActiveView() method.
                        //if (!isMoving && !isRotating && isRendering == false)
                        //{
                        //    uIApplication.ActiveUIDocument.RefreshActiveView();

                        //    isRendering = null;
                        //}
                    }
                    catch (Exception ex)
                    {
                        ExAppCtrlWizRVT.MessageService.ShowError($"Error occurred:\n{ex.Message}");
                        throw;
                    }
                }
                await Task.Delay(10);
            }
        }

        private void MoveCamera(float x, float y, float z)
        {
            if (!isMoving)
            {
                isMoving = true;
            }

            ViewOrientation3D viewOrientation3D = view3D.GetOrientation();
            Vector3 upDirection = viewOrientation3D.UpDirection.ConvertToVector3();
            Vector3 forwardDirection = viewOrientation3D.ForwardDirection.ConvertToVector3();
            Vector3 rightDirection = view3D.RightDirection.ConvertToVector3();
            Vector3 eyePosition = viewOrientation3D.EyePosition.ConvertToVector3();

            eyePosition += (y * (Vector3.Add(forwardDirection, upDirection) * (Vector3.UnitY + Vector3.UnitX))
                            + x * (rightDirection * (Vector3.UnitY + Vector3.UnitX)))
                            * (0.003f * speed * ((float)SpeedSettings.LinearSpeed / SpeedSettings.LinearSpeedFactor) * constantUnit * timeDeltatime);

            eyePosition += z * Vector3.UnitZ * (0.003f * speed * ((float)SpeedSettings.LinearSpeed / SpeedSettings.LinearSpeedFactor) * constantUnit * timeDeltatime);

            using (ViewOrientation3D newViewOrientation3D = new ViewOrientation3D(
                eyePosition.ConvertToXYZ(),
                viewOrientation3D.UpDirection,
                viewOrientation3D.ForwardDirection))
            {
                view3D.SetOrientation(newViewOrientation3D);
            }
        }

        private void RotateCamera(double XAxis, double YAxis)
        {
            if (!isRotating)
            {
                isRotating = true;
            }

            ViewOrientation3D viewOrientation3D = view3D.GetOrientation();

            // Rotate Left-Right.
            double radians = 0.045 * XAxis * speed * SpeedSettings.AngularSpeed * timeDeltatime / 1000;
            XYZ eyePosition = viewOrientation3D.EyePosition;
            XYZ upDirection = viewOrientation3D.UpDirection.RotateByAxis(XYZ.BasisZ, radians);
            XYZ rightDirection = view3D.RightDirection.RotateByAxis(XYZ.BasisZ, radians);
            XYZ forwardDirection = viewOrientation3D.ForwardDirection.RotateByAxis(XYZ.BasisZ, radians);

            // Rotate Up-Down.
            radians = 0.045 * YAxis * speed * SpeedSettings.AngularSpeed * timeDeltatime / 1000;
            XYZ upDirectionTemp = upDirection.RotateByAxis(rightDirection, radians);

            if (upDirectionTemp.Z > 0)
            {
                upDirection = upDirectionTemp;
                forwardDirection = forwardDirection.RotateByAxis(rightDirection, radians);
            }

            using (ViewOrientation3D newViewOrientation3D = new ViewOrientation3D(eyePosition, upDirection, forwardDirection))
            {
                view3D.SetOrientation(newViewOrientation3D);
            }
        }

        //private void RefreshActiveView(View3D view3D)
        //{
        //    DisplayStyle displayStyle = view3D.DisplayStyle;

        //    // Changing the DisplayStyle to boost rendering.
        //    if (displayStyle == DisplayStyle.RealisticWithEdges)
        //    {
        //        view3D.DisplayStyle = DisplayStyle.Realistic;
        //    }
        //    using (UIDocument uIDocument = uIApplication.ActiveUIDocument)
        //    {
        //        uIDocument.RefreshActiveView();
        //    }

        //    // Set the original DisplayStyle.
        //    view3D.DisplayStyle = displayStyle;
        //}
    }
}