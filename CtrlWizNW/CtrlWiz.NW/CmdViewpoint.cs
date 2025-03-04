using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.ComApi;
using Autodesk.Navisworks.Api.DocumentParts;
using Autodesk.Navisworks.Api.Interop;
using Autodesk.Navisworks.Api.Interop.ComApi;
using Autodesk.Navisworks.Api.Plugins;
using Autodesk.Windows;
using CtrlWiz.Forms.MessageServices;
using CtrlWiz.Forms.Presenters;
using CtrlWiz.License;
using CtrlWiz.License.Configs;
using CtrlWiz.Logging;
using CtrlWiz.Logging.Utility;
using CtrlWiz.NW.UI.Models;
using CtrlWiz.NW.UI.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;
using XInputDotNetPure;

namespace CtrlWiz.NW
{
    #region Create Ribbon
    [Plugin("NVSXBOX.CmdViewpoint", "VIATechnik", DisplayName = "CtrlWiz")]
    [Strings("CustomRibbon.name")]
    [RibbonLayout("CustomRibbon.xaml")]
    [RibbonTab("ID_CustomTab_1", DisplayName = "CtrlWiz")]
    [Command("ID_Button_1", CanToggle = true, DisplayName = " Activate\n Controller ", ExtendedToolTip = "", Icon = "ActivateController.ico", LargeIcon = "ActivateController.ico", ToolTip = "")]
    [Command("ID_Button_2", DisplayName = " Controller\n Map ", ExtendedToolTip = "", Icon = "Help28.png", LargeIcon = "Help28.png", ToolTip = "")]
    [Command("ID_Button_3", DisplayName = " Feature\n  Request  ", ExtendedToolTip = "", Icon = "Idea.png", LargeIcon = "Idea.png", ToolTip = "")]
    [Command("ID_Button_4", DisplayName = " Speed\n  Setting  ", ExtendedToolTip = "", Icon = "Settings.ico", LargeIcon = "Settings.ico", ToolTip = "")]
    [Command("ID_Button_5", DisplayName = " License ", ExtendedToolTip = "", Icon = "License.ico", LargeIcon = "License.ico", ToolTip = "", LoadForCanExecute = true)]

    #endregion

    public class CmdViewpoint : CommandHandlerPlugin, IDisposable
    {
        private InputSimulator _inputSimulator = new InputSimulator();
        private IntPtr _activeAppProcessId;
        //
        private ControllerButton _selectDeselectButton = ControllerButton.Unset;
        private ControllerButton _hideUnhideButton = ControllerButton.Unset;
        private ControllerButton _unhideAllButton = ControllerButton.Unset;
        private ControllerButton _deselectAllButton = ControllerButton.Unset;
        private ControllerButton _shortestDistanceButton = ControllerButton.Unset;
        private ControllerButton _nextViewpointButton = ControllerButton.Unset;
        private ControllerButton _enableTextMarkupButton = ControllerButton.Unset;
        private ControllerButton _createViewpointButton = ControllerButton.Unset;
        private ControllerButton _showHidePropertiesButton = ControllerButton.Unset;
        private ControllerButton _showHideSelectionTreeButton = ControllerButton.Unset;
        private ControllerButton _showHideViewpointsButton = ControllerButton.Unset;
        private ControllerButton _personView3rdButton = ControllerButton.Unset;
        private ControllerButton _collisionGravityButton = ControllerButton.Unset;
        private ControllerButton _moveLeftInTreeButton = ControllerButton.Unset;
        private ControllerTrigger _elevationUpTrigger = ControllerTrigger.Unset;
        private ControllerTrigger _elevationDownTrigger = ControllerTrigger.Unset;
        private ControllerThumbStick _moveSprintThumbStick = ControllerThumbStick.Unset;
        private ControllerThumbStick _lookThumbStick = ControllerThumbStick.Unset;
        private ControllerThumbStick _lookInvertedYThumbStick = ControllerThumbStick.Unset;
        private ControllerThumbStick _leftDroneControlsThumbStick = ControllerThumbStick.Unset;
        private ControllerThumbStick _rightDroneControlsThumbStick = ControllerThumbStick.Unset;
        //
        private static bool isProductActivated = false;
        //private static InAppCheckout inAppCheckout = null;
        public static MessageService MessageService => new MessageService();
        private bool isShortestDistanceShown = false;
        private List<ModelItem> previousSelectedItems = new List<ModelItem>();

        protected static DateTime MaxDate;
        protected static int KeyLock;
        //protected static bool isConnectInternet = false;

        private GamePadState state;
        private GamePadState prevState;
        private bool isrun = false;
        private double currentZ;

        public static Document oDoc;

        Viewpoint oCurrentViewCopy;
        //Document FPCdoc;

        private bool isMoving = false;
        private bool isRotating = false;
        private bool? isThumbStickValueXReleased = null;
        private bool? isThumbStickValueYReleased = null;

        //System.Numerics.Vector2 mv;
        //FormDebug formDebug;
        //COMApi.InwOpState10 m_state = null;
        //COMApi.InwOpClashElement m_clash = null;

        //public static string textDebug="abc";
        //FormDebug formDebug;
        //private string FPCPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)+"XRVinhSphere.fbx";

        #region declare FPS 
        private static long lastTime = System.Environment.TickCount;
        private static int fps = 1;
        private static int frames;
        private static float deltaTime = 0.005f;

        private static Stopwatch sw;
        private static long TimeDeltatime;
        private double Speed = 1;
        private static double ConstantUnit;
        private readonly static int LinearSpeedFactor = 10;
        public static int SpeedSetting = 20;
        public static int AngularSetting = 45;

        private bool firsttime = false;

        #endregion

        private int IDCurrentVpts;
        private int CurrCountAllVPts;
        public IList<Viewpoint> oAllVPts;
        private SavedItem CurrentSavedItem { get; set; }

        //#region Tool Plugin
        //private static readonly bool isTool = false;
        //#endregion

        //System.Diagnostics.Stopwatch stopWatch;
        //COMApi.InwNvViewPoint2 oV;

        //public override CommandState CanExecuteCommand(string name)
        //{
        //    if (!isProductActivated)
        //    {
        //        inAppCheckout = new InAppCheckout();

        //        inAppCheckout.StartPaddle();

        //        isProductActivated = inAppCheckout.IsProductActivated;
        //    }

        //    if (name == "ID_Button_5" && !isProductActivated)
        //    {
        //        return new CommandState(false) { IsVisible = false };
        //    }
        //    else
        //    {
        //        return new CommandState();
        //    }
        //    //RibbonTabCollection oTabs = Autodesk.Windows.ComponentManager.Ribbon.Tabs;
        //    //RibbonTab ribbonTab= oTabs.FirstOrDefault(t => t.AutomationName == "CtrlWiz");
        //    //RibbonPanel ribbonPanel= ribbonTab.Panels.FirstOrDefault();
        //    //RibbonItem ribbonItem= ribbonPanel.Source.FindItem("NVSXBOX.CmdViewpoint.VIATechnik.ID_Button_5");
        //    //ribbonItem.IsVisible = false;
        //}

        private void SetControllerButtonsLayout()
        {
            _selectDeselectButton = UI.Properties.GetControllerElement.GetControllerButtonByFunction(Function.SelectDeselect);
            _hideUnhideButton = UI.Properties.GetControllerElement.GetControllerButtonByFunction(Function.HideUnhide);
            _unhideAllButton = UI.Properties.GetControllerElement.GetControllerButtonByFunction(Function.UnhideAll);
            _deselectAllButton = UI.Properties.GetControllerElement.GetControllerButtonByFunction(Function.DeselectAll);
            _shortestDistanceButton = UI.Properties.GetControllerElement.GetControllerButtonByFunction(Function.ShortestDistance);
            _nextViewpointButton = UI.Properties.GetControllerElement.GetControllerButtonByFunction(Function.NextViewpoint);
            _enableTextMarkupButton = UI.Properties.GetControllerElement.GetControllerButtonByFunction(Function.EnableTextMarkup);
            _createViewpointButton = UI.Properties.GetControllerElement.GetControllerButtonByFunction(Function.CreateViewpoint);
            _showHidePropertiesButton = UI.Properties.GetControllerElement.GetControllerButtonByFunction(Function.ShowHideProperties);
            _showHideSelectionTreeButton = UI.Properties.GetControllerElement.GetControllerButtonByFunction(Function.ShowHideSelectionTree);
            _showHideViewpointsButton = UI.Properties.GetControllerElement.GetControllerButtonByFunction(Function.ShowHideViewpoints);
            _personView3rdButton = UI.Properties.GetControllerElement.GetControllerButtonByFunction(Function.PersonView3rd);
            _collisionGravityButton = UI.Properties.GetControllerElement.GetControllerButtonByFunction(Function.CollisionGravity);
            _moveLeftInTreeButton = UI.Properties.GetControllerElement.GetControllerButtonByFunction(Function.MoveLeftInTree);
        }
        private void SetControllerTriggersLayout()
        {
            _elevationUpTrigger = UI.Properties.GetControllerElement.GetControllerTriggerByFunction(Function.ElevationUp);
            _elevationDownTrigger = UI.Properties.GetControllerElement.GetControllerTriggerByFunction(Function.ElevationDown);
        }
        private void SetControllerThumbSticksLayout()
        {
            _moveSprintThumbStick = UI.Properties.GetControllerElement.GetControllerThumbStickByFunction(Function.MoveSprint);
            _lookThumbStick = UI.Properties.GetControllerElement.GetControllerThumbStickByFunction(Function.Look);
            _lookInvertedYThumbStick = UI.Properties.GetControllerElement.GetControllerThumbStickByFunction(Function.LookInvertedY);
            _leftDroneControlsThumbStick = UI.Properties.GetControllerElement.GetControllerThumbStickByFunction(Function.LeftDroneControls);
            _rightDroneControlsThumbStick = UI.Properties.GetControllerElement.GetControllerThumbStickByFunction(Function.RigthDroneControls);
        }

        private static ButtonState GetButtonState(GamePadState state, ControllerButton button)
        {
            if (button == ControllerButton.X)
                return state.Buttons.X;
            else if (button == ControllerButton.Y)
                return state.Buttons.Y;
            else if (button == ControllerButton.A)
                return state.Buttons.A;
            else if (button == ControllerButton.B)
                return state.Buttons.B;
            else if (button == ControllerButton.RBumber)
                return state.Buttons.RightShoulder;
            else if (button == ControllerButton.Start)
                return state.Buttons.Start;
            else
                return ButtonState.Released;
        }
        private static float GetTriggerStateValues(GamePadState state, ControllerTrigger trigger)
        {
            if (trigger == ControllerTrigger.LT)
                return state.Triggers.Left;
            else if (trigger == ControllerTrigger.RT)
                return state.Triggers.Right;
            else
                return 0;
        }
        private static (float, float) GetThumbStickStateValues(GamePadState state, ControllerThumbStick thumbStick)
        {
            if (thumbStick == ControllerThumbStick.L)
                return (state.ThumbSticks.Left.X, state.ThumbSticks.Left.Y);
            else if (thumbStick == ControllerThumbStick.R)
                return (state.ThumbSticks.Right.X, state.ThumbSticks.Right.Y);
            else
                return (0, 0);
        }
        private static ButtonState GetThumbStickState(GamePadState state, ControllerThumbStick thumbStick)
        {
            if (thumbStick == ControllerThumbStick.L)
                return state.Buttons.LeftStick;
            else if (thumbStick == ControllerThumbStick.R)
                return state.Buttons.RightStick;
            else
                return ButtonState.Released;
        }

        private void GetControllerElement_SettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            object controllerElementObj = UI.Properties.GetControllerElement.GetControllerElementByPropertyName(e.PropertyName);
            //Function controllerElementFunction = Forms.Properties.GetControllerElement.GetControllerElementFunctionByPropertyNameNV(e.PropertyName);

            if (controllerElementObj is ControllerButton)
            {
                this.SetControllerButtonsLayout();
            }
            else if (controllerElementObj is ControllerTrigger)
            {
                this.SetControllerTriggersLayout();
            }
            else if (controllerElementObj is ControllerThumbStick)
            {
                this.SetControllerThumbSticksLayout();
            }
        }

        public void Dispose()
        {
            UI.Properties.GetControllerElement.SettingsPropertyChanged -= GetControllerElement_SettingsPropertyChanged;
        }

        private void EnableTextMarkup()
        {
            if (Application.ActiveDocument.Tool.Value != Tool.RedlineText)
            {
                Application.ActiveDocument.Tool.Value = Tool.RedlineText;
            }
            else
            {
                Application.ActiveDocument.Tool.Value = Tool.None;
            }
        }
        private void CreateViewpoint()
        {
            Viewpoint newViewpoint = Application.ActiveDocument.ActiveView.CreateViewpointCopy();
            SavedViewpoint newSavedViewpoint = new SavedViewpoint(newViewpoint);
            DocumentSavedViewpoints savedViewpoints = Application.ActiveDocument.SavedViewpoints;
            Collection<SavedItem> savedViewpointsCopy = savedViewpoints.CreateCopy();

            ICollection<string> savedViewpointNames = Application.ActiveDocument.SavedViewpoints.ToSavedItemCollection().Select(si => si.DisplayName).ToList();

            Regex namePattern = new Regex(@"^View \((\d+)\)$");

            ICollection<int> viewpointNamesMatchNumber = savedViewpointNames.Where(n =>
               namePattern.IsMatch(n)).Select(n =>
               {
                   return Convert.ToInt32(namePattern.Match(n).Groups[1].Value);
               }).ToList();

            if (viewpointNamesMatchNumber.Count() > 0)
                newSavedViewpoint.DisplayName = $"View ({viewpointNamesMatchNumber.Max() + 1})";
            else if (savedViewpointNames.Contains("View"))
                newSavedViewpoint.DisplayName = "View (1)";
            else
                newSavedViewpoint.DisplayName = "View";

            savedViewpointsCopy.Add(newSavedViewpoint);
            savedViewpoints.CopyFrom(savedViewpointsCopy);
        }
        private void ShowHideProperties()
        {
            LcRmFrameworkInterface.ExecuteCommand("RoamerGUI_OM_ATTRIB_BAR", LcUCIPExecutionContext.eTOOLBAR);
        }
        private void ShowHideSelectionTree()
        {
            LcRmFrameworkInterface.ExecuteCommand("RoamerGUI_OM_VIEW_TREE", LcUCIPExecutionContext.eTOOLBAR);
        }
        private void ShowHideViewpoints()
        {
            LcRmFrameworkInterface.ExecuteCommand("RoamerGUI_OM_VP_ORG", LcUCIPExecutionContext.eTOOLBAR);
        }
        private void MoveLeftInTree()
        {
            Document document = Application.ActiveDocument;
            ModelItemCollection selectedItems = document.CurrentSelection.SelectedItems;

            if (selectedItems.Count == 1)
            {
                ModelItem modelItem = selectedItems.First;
                ModelItemEnumerableCollection ancestors = modelItem.Ancestors;
                ModelItem parentModelItem = ancestors.First;

                Application.ActiveDocument.CurrentSelection.Clear();

                if (parentModelItem != null)
                    Application.ActiveDocument.CurrentSelection.Add(parentModelItem);
            }
        }
        private void ToogleThirdPerson()
        {
            //LcRmFrameworkInterface.ExecuteCommand("RoamerGUI_OM_THIRD_PERSON", LcUCIPExecutionContext.eTOOLBAR);
            InwNvViewPoint2 oV = ComApiBridge.State.CurrentView.ViewPoint as InwNvViewPoint2;
            if (oV.Viewer.CameraMode == nwECameraMode.eCameraMode_ThirdPerson)
            {
                oV.Viewer.CameraMode = nwECameraMode.eCameraMode_FirstPerson;
            }
            else
            {
                oV.Viewer.CameraMode = nwECameraMode.eCameraMode_ThirdPerson;
            }
        }
        private void ToogleCollisionGravity()
        {
            if ((isThumbStickValueXReleased == null || isThumbStickValueXReleased == true)
                && (isThumbStickValueYReleased == null || isThumbStickValueYReleased == true) &&
                isMoving == false && isRotating == false)
            {
                InwNvViewPoint2 oV = ComApiBridge.State.CurrentView.ViewPoint as InwNvViewPoint2;
                oV.Paradigm = nwEParadigm.eParadigm_WALK;
                if (oV.Viewer.Gravity || oV.Viewer.CollisionDetection)
                {
                    oV.Viewer.Gravity = false;
                    oV.Viewer.CollisionDetection = false;
                    oV.Paradigm = nwEParadigm.eParadigm_APPLICATION;
                }
                else
                {
                    oV.Viewer.Gravity = true;
                    oV.Viewer.CollisionDetection = true;
                }
            }
        }


        #region DO NOT USE
        private static void CollisionView()
        {
            LcRmFrameworkInterface.ExecuteCommand("RoamerGUI_OM_COLLISION_DETECTION", LcUCIPExecutionContext.eTOOLBAR);
        }
        private static void GravityView()
        {
            LcRmFrameworkInterface.ExecuteCommand("RoamerGUI_OM_GRAVITY", LcUCIPExecutionContext.eTOOLBAR);
        }
        private static void WalkMode()
        {
            LcRmFrameworkInterface.ExecuteCommand("RoamerGUI_OM_MODE_WALK", LcUCIPExecutionContext.eTOOLBAR);
        }
        private static void LookAroundMode()
        {
            LcRmFrameworkInterface.ExecuteCommand("RoamerGUI_OM_MODE_SWIVEL", LcUCIPExecutionContext.eTOOLBAR);
        }
        #endregion
        public override int ExecuteCommand(string commandId = "default", params string[] parameters)
        {
            try
            {
                if (commandId == "default")
                    return 0;

                Logger.SetUpLogger(LoggedApp.NW);

                this.SetControllerButtonsLayout();
                this.SetControllerTriggersLayout();
                this.SetControllerThumbSticksLayout();

                UI.Properties.GetControllerElement.SettingsPropertyChanged += GetControllerElement_SettingsPropertyChanged;
                //
                InAppCheckout inAppCheckout = new InAppCheckout(ProductVersion.NW, commandId);
                inAppCheckout.CheckoutCompleted += InAppCheckout_CheckoutCompleted;
                inAppCheckout.Start();
            }
            catch (Exception ex)
            {
                ex.LogException();

                System.Windows.Forms.MessageBox.Show(ex.Message);
            }

            return 0;
        }

        public override CommandState CanExecuteCommand(string name)
        {
            try
            {
                CmdViewpoint cmdViewpoint = new CmdViewpoint();

                cmdViewpoint.ExecuteCommand();

                return new CommandState(true);
            }
            catch (Exception ex)
            {
                string logFilePath = Path.Combine(Path.GetTempPath(), "CtrlWizNWExecuteFail.log");

                string lastLine = null;
                if (File.Exists(logFilePath))
                    lastLine = File.ReadAllLines(logFilePath).LastOrDefault();

                if (lastLine != ex.Message)
                    File.AppendAllLines(logFilePath, new List<string>() { ex.Message });

                return new CommandState(false);
            }
        }

        private void InAppCheckout_CheckoutCompleted(object sender, CheckoutCompletedArgs e)
        {
            try
            {
                if (!e.Allowed)
                    return;

                switch (e.CommandId)
                {
                    case "ID_Button_1":
                        {
                            if (!isrun)
                            {
                                isrun = true;
                                Button1();
                                //CurrentSavedItem = oDoc.SavedViewpoints.CurrentSavedViewpoint;
                                oDoc.SavedViewpoints.Changed += SavedViewpoints_Changed;
                                oDoc.SavedViewpoints.CurrentSavedViewpointChanged += SavedViewpoints_CurrentSavedViewpointChanged;
                            }
                            break;
                        }
                    case "ID_Button_2":
                        {
                            Button2();
                            break;
                        }
                    case "ID_Button_3":
                        {
                            Button3();
                            break;
                        }
                    case "ID_Button_4":
                        {
                            try
                            {
                                SpeedSettingPresenter speedSettingPresenter = new SpeedSettingPresenter(SpeedSetting, AngularSetting, LinearSpeedFactor);
                                if (speedSettingPresenter.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                                {
                                    SpeedSetting = speedSettingPresenter.LinearSetting;
                                    AngularSetting = speedSettingPresenter.AngularSetting;
                                    oDoc = Application.ActiveDocument;
                                    if (oDoc != null)
                                    {
                                        var oVP = oDoc.CurrentViewpoint.CreateCopy();
                                        oVP.LinearSpeed = ((double)SpeedSetting / LinearSpeedFactor) * 1000;
                                        oVP.AngularSpeed = AngularSetting * Math.PI / 180;
                                        oDoc.CurrentViewpoint.CopyFrom(oVP);
                                        oVP.Dispose();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ex.LogException();

                                System.Windows.Forms.MessageBox.Show(ex.Message);
                            }
                            break;
                        }
                    case "ID_Button_5":
                        {
                            try
                            {
                                (sender as InAppCheckout).ShowLicenseDetailsWindow();
                                // if (msg != null) MessageService.ShowError(msg);
                            }
                            catch (Exception ex)
                            {
                                ex.LogException();
                            }
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                ex.LogException();
            }
        }

        private void SavedViewpoints_CurrentSavedViewpointChanged(object sender, EventArgs e)
        {
            Document document = sender as Document;
            if (document.SavedViewpoints.CurrentSavedViewpoint != null && document.SavedViewpoints.CurrentSavedViewpoint != CurrentSavedItem)
            {
                CurrentSavedItem = document.SavedViewpoints.CurrentSavedViewpoint;
            }
        }

        private void SavedViewpoints_Changed(object sender, SavedItemChangedEventArgs e)
        {
            Document document = sender as Document;
            if (e.Action == SavedItemChangedAction.Remove)
            {
                List<SavedItem> savedItems = GetSavedItems(document);
                if (savedItems.Count != 0)
                {
                    int index = e.OldIndex;
                    CurrentSavedItem = index < savedItems.Count ? savedItems[index] : savedItems[index - 1];
                    document.SavedViewpoints.CurrentSavedViewpoint = CurrentSavedItem;
                }
            }
        }

        private void InAppCheckout_ActivationChanged(object sender, ActivationChangedArgs e)
        {
            isProductActivated = e.IsActivated;
        }

        //public override CommandState CanExecuteCommand(String commandId)
        //{
        //    CommandState state = new CommandState();
        //    switch (commandId)
        //    {
        //        // Button 1 is only enabled when Button 2 is toggled on.
        //        case "ID_Button_1":
        //            {
        //                state.IsEnabled = false;
        //                break;
        //            }
        //        case "ID_Button_2":
        //            {

        //                break;
        //            }
        //    }
        //    return state;
        //}
        private async void Button1()
        {
            try
            {
                //Get Current Documents
                SetConstantUnit();

                #region Update 3
                SetInitSettingUIAS();
                #endregion

                //IDCurrentVpts = 0;

                await Start(1, 0.35f);
                //stopWatch = new System.Diagnostics.Stopwatch();
                GetListSaveVPts();
                CurrCountAllVPts = oAllVPts.Count;

                // Enable Target mark.
                TargetEnable();
                //Target();

                await Update();
            }
            catch (Exception ex)
            {
                ex.LogException();
            }
        }

        #region Update 3
        public void SetInitSettingUIAS()
        {
            //SpeedSetting = 4;
            //AngularSetting = 45;

            try
            {
                oDoc = Application.ActiveDocument;
                if (oDoc != null)
                {
                    var oVP = oDoc.CurrentViewpoint.CreateCopy();

                    //if (SpeedSetting >= 45) SpeedSetting = 45;
                    //if (AngularSetting >= 90) AngularSetting = 90;

                    oVP.LinearSpeed = ((double)SpeedSetting / LinearSpeedFactor) * 1000;
                    oVP.AngularSpeed = AngularSetting * Math.PI / 180; //Degree to Radian
                    oDoc.CurrentViewpoint.CopyFrom(oVP);
                    //oVP.Dispose();
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
                throw ex;
            }
        }
        #endregion

        #region Update 3
        public void SetRuntimeSettingUIAS()
        {
            try
            {
                oDoc = Application.ActiveDocument;
                if (oDoc != null)
                {
                    //
                    Viewpoint viewpoint = oDoc.CurrentViewpoint.CreateCopy();
                    viewpoint.LinearSpeed = ((double)SpeedSetting / LinearSpeedFactor) * 1000;
                    viewpoint.AngularSpeed = AngularSetting * Math.PI / 180; //Degree to Radian
                    oDoc.CurrentViewpoint.CopyFrom(viewpoint);
                    viewpoint.Dispose();
                    //
                    //SpeedSetting = (int)oDoc.CurrentViewpoint.ToViewpoint().LinearSpeed;
                    //AngularSetting = (int)oDoc.CurrentViewpoint.ToViewpoint().AngularSpeed;

                    //if (SpeedSetting >= 45)
                    // {
                    //     SpeedSetting = 45;
                    //var oVP = oDoc.CurrentViewpoint.CreateCopy();
                    //oVP.LinearSpeed = SpeedSetting;
                    //oDoc.CurrentViewpoint.CopyFrom(oVP);
                    //oVP.Dispose();

                    // }
                    // if (AngularSetting >= 90)
                    // {
                    //     AngularSetting = 90;
                    //oVP = oDoc.CurrentViewpoint.CreateCopy();
                    //var oVP = oDoc.CurrentViewpoint.CreateCopy();
                    //oVP.AngularSpeed = AngularSetting * Math.PI / 180; //Degree to Radian
                    //oDoc.CurrentViewpoint.CopyFrom(oVP);
                    //oVP.Dispose();
                    // }
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
                throw;
            }

        }
        #endregion

        private void SetConstantUnit()
        {
            oDoc = Autodesk.Navisworks.Api.Application.ActiveDocument;

            if (oDoc.Units == Units.Meters) ConstantUnit = 1;
            if (oDoc.Units == Units.Inches) ConstantUnit = 39.3701;
            if (oDoc.Units == Units.Centimeters) ConstantUnit = 100;
            if (oDoc.Units == Units.Feet) ConstantUnit = 3.28084;
            if (oDoc.Units == Units.Kilometers) ConstantUnit = Math.Pow(10, -3);
            if (oDoc.Units == Units.Microinches) ConstantUnit = 39.3701 * Math.Pow(10, 6);
            if (oDoc.Units == Units.Micrometers) ConstantUnit = Math.Pow(10, 6);
            if (oDoc.Units == Units.Miles) ConstantUnit = 0.621371 * Math.Pow(10, -3);
            if (oDoc.Units == Units.Millimeters) ConstantUnit = 1000;
            if (oDoc.Units == Units.Mils) ConstantUnit = 39370.1;
            if (oDoc.Units == Units.Yards) ConstantUnit = 1.093613888889;

        }

        private List<SavedItem> GetSavedItems(Document document)
        {
            List<SavedItem> savedItems = new List<SavedItem>();
            foreach (SavedItem savedItem in document.SavedViewpoints.Value)
            {
                GetContent(savedItem, savedItems);
            }
            return savedItems;
        }

        private void GetContent(SavedItem savedItem, List<SavedItem> savedItems)
        {
            if (savedItem.IsGroup)
            {
                foreach (SavedItem item in ((GroupItem)savedItem).Children)
                {
                    GetContent(item, savedItems);
                }
            }
            else
            {
                savedItems.Add(savedItem);
            }
        }

        #region Get all list Saved Viewpoint
        private void GetListSaveVPts()
        {
            //oDoc = Autodesk.Navisworks.Api.Application.ActiveDocument;
            oAllVPts = new List<Viewpoint>();

            foreach (SavedItem oSVP in oDoc.SavedViewpoints.Value)
            {
                // if it is a folder/animation 
                if (oSVP.IsGroup) recurse(oSVP);
                else
                {
                    try
                    {
                        if (oSVP is SavedViewpoint)
                        {

                            SavedViewpoint oThisSVP = oSVP as SavedViewpoint;
                            if (oThisSVP != null)
                            {
                                Viewpoint oVP = oThisSVP.Viewpoint;
                                if (!oAllVPts.Contains(oVP))
                                {
                                    oAllVPts.Add(oVP);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }

            //System.Windows.Forms.MessageBox.Show(string.Format($"All Views: {oDoc.Views.Count} \n  " +
            //    $"Save View point count: {oDoc.SavedViewpoints.Value.Count}\n" +
            //    $"List VP: {oAllVPts.Count}"));

        }
        #endregion
        #region Recurse
        void recurse(SavedItem oFolder)
        {
            foreach (SavedItem item in ((GroupItem)oFolder).Children)
            {
                if (item.IsGroup)
                {
                    recurse(item);
                }
                else if (item is SavedViewpoint)
                {
                    try
                    {
                        SavedViewpoint oThisSVP = item as SavedViewpoint;
                        if (oThisSVP != null)
                        {
                            Viewpoint oVP = oThisSVP.Viewpoint;
                            if (!oAllVPts.Contains(oVP))
                            {
                                oAllVPts.Add(oVP);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
        }
        #endregion

        private void Button2()
        {
            try
            {
                HelpView helpView = new HelpView();
                helpView.ShowDialog();
            }
            catch (Exception ex)
            {
                ex.LogException();
            }
        }

        private void Button3()
        {
            try
            {
                Process.Start("https://feedback.ctrlwiz.com/ctrwiznav-feature-requests");
            }
            catch (Exception ex)
            {
                ex.LogException();
            }
        }

        private async void Button5_Tool()
        {
            ToolPluginRecord toolPluginRecord = (ToolPluginRecord)Application.Plugins.FindPlugin("NVSXBOX.CmdTool.ATLAS");

            Application.MainDocument.Tool.SetCustomToolPlugin(toolPluginRecord.LoadPlugin());

            await Task.WhenAll();
        }

        private async void TargetEnable()
        {
            ToolPluginRecord toolPluginRecord = (ToolPluginRecord)Application.Plugins.FindPlugin("CtrlWiz.NW.CmdTargetEnable.VIATechnik");
            if (toolPluginRecord.IsEnabled)
            {
                Application.MainDocument.Tool.SetCustomToolPlugin(toolPluginRecord.TryLoadPlugin());
                RefreshView();
            }
            await Task.WhenAll();
        }

        private void TargetDisable()
        {
            ToolPluginRecord toolPluginRecord = (ToolPluginRecord)Application.Plugins.FindPlugin("CtrlWiz.NW.CmdTargetDisable.VIATechnik");
            Application.MainDocument.Tool.SetCustomToolPlugin(toolPluginRecord.TryLoadPlugin());
            RefreshView();
        }

        private void RefreshView()
        {
            if (oDoc != null)
            {
                oCurrentViewCopy = oDoc.CurrentViewpoint.CreateCopy();
                oCurrentViewCopy.Position = new Point3D(oCurrentViewCopy.Position.X - 1, oCurrentViewCopy.Position.Y, oCurrentViewCopy.Position.Z);
                oDoc.CurrentViewpoint.CopyFrom(oCurrentViewCopy);
                oCurrentViewCopy.Position = new Point3D(oCurrentViewCopy.Position.X + 1, oCurrentViewCopy.Position.Y, oCurrentViewCopy.Position.Z);
                oDoc.CurrentViewpoint.CopyFrom(oCurrentViewCopy);
            }
        }

        #region Virbration
        private async void Vibration(int delay, float p)
        {
            await Start(delay, p);
        }

        public async Task Start(int delay, float p)
        {
            GamePad.SetVibration(PlayerIndex.One, p, p);
            await Task.Delay(delay * 1000);
            GamePad.SetVibration(PlayerIndex.One, 0, 0);
        }
        #endregion

        #region Inertia
        private async void Inertia(int delay, double vX, double vY)
        {
            await threadInnertia(delay, vX, vY);
        }
        public async Task threadInnertia(int delay, double vX, double vY)
        {
            while (delay > 16)
            {
                MoveCamera(vX, vY, 0);
                await Task.Delay(16);
                delay -= 16;
            }
        }
        #endregion
        //-------------------------------------------------------------------------------------------------------------------------
        public async Task Update()
        {
            try
            {
                _activeAppProcessId = GetActiveAppProcessId();

                while (isrun)
                {
                    if (GetActiveAppProcessId() != _activeAppProcessId)
                    {
                        await Task.Delay(10);

                        continue;
                    }

                    if (isMoving || isRotating)
                    {
                        TargetEnable();
                        //Target();
                    }

                    //var currentTick = System.Environment.TickCount;

                    sw = Stopwatch.StartNew();

                    prevState = state;
                    state = GamePad.GetState(PlayerIndex.One);

                    ButtonState selectDeselectButtonState = GetButtonState(state, _selectDeselectButton);
                    ButtonState selectDeselectButtonPreviousState = GetButtonState(prevState, _selectDeselectButton);
                    ButtonState hideUnhideButtonState = GetButtonState(state, _hideUnhideButton);
                    ButtonState hideUnhideButtonPreviousState = GetButtonState(prevState, _hideUnhideButton);
                    ButtonState unhideAllButtonState = GetButtonState(state, _unhideAllButton);
                    ButtonState unhideAllButtonPreviousState = GetButtonState(prevState, _unhideAllButton);
                    ButtonState deselectAllButtonState = GetButtonState(state, _deselectAllButton);
                    ButtonState deselectAllButtonPreviousState = GetButtonState(prevState, _deselectAllButton);
                    ButtonState shortestDistanceButtonState = GetButtonState(state, _shortestDistanceButton);
                    ButtonState shortestDistanceButtonPreviousState = GetButtonState(prevState, _shortestDistanceButton);
                    ButtonState nextViewpointButtonState = GetButtonState(state, _nextViewpointButton);
                    ButtonState nextViewpointButtonPreviousState = GetButtonState(prevState, _nextViewpointButton);
                    ButtonState enableTextMarkupButtonState = GetButtonState(state, _enableTextMarkupButton);
                    ButtonState enableTextMarkupButtonPreviousState = GetButtonState(prevState, _enableTextMarkupButton);
                    ButtonState createViewpointButtonState = GetButtonState(state, _createViewpointButton);
                    ButtonState createViewpointButtonPreviousState = GetButtonState(prevState, _createViewpointButton);
                    ButtonState showHidePropertiesButtonState = GetButtonState(state, _showHidePropertiesButton);
                    ButtonState showHidePropertiesButtonPreviousState = GetButtonState(prevState, _showHidePropertiesButton);
                    ButtonState showHideSelectionTreeButtonState = GetButtonState(state, _showHideSelectionTreeButton);
                    ButtonState showHideSelectionTreeButtonPreviousState = GetButtonState(prevState, _showHideSelectionTreeButton);
                    ButtonState showHideViewpointsButtonState = GetButtonState(state, _showHideViewpointsButton);
                    ButtonState showHideViewpointsButtonPreviousState = GetButtonState(prevState, _showHideViewpointsButton);
                    ButtonState personView3rdButtonState = GetButtonState(state, _personView3rdButton);
                    ButtonState personView3rdButtonPreviousState = GetButtonState(prevState, _personView3rdButton);
                    ButtonState collisionGravityButtonState = GetButtonState(state, _collisionGravityButton);
                    ButtonState collisionGravityButtonPreviousState = GetButtonState(prevState, _collisionGravityButton);
                    ButtonState moveLeftInTreeButtonState = GetButtonState(state, _moveLeftInTreeButton);
                    ButtonState moveLeftInTreeButtonPreviousState = GetButtonState(prevState, _moveLeftInTreeButton);

                    float elevationDownTriggerValue = GetTriggerStateValues(state, _elevationDownTrigger);
                    float elevationUpTriggerValue = GetTriggerStateValues(state, _elevationUpTrigger);
                    (float moveSprintThumbStickValueX, float moveSprintThumbStickValueY) = GetThumbStickStateValues(state, _moveSprintThumbStick);
                    (float lookThumbStickValueX, float lookThumbStickValueY) = GetThumbStickStateValues(state, _lookThumbStick);
                    (float lookInvertedYThumbStickValueX, float lookInvertedYThumbStickValueY) = GetThumbStickStateValues(state, _lookInvertedYThumbStick);
                    (float leftDroneControlsThumbStickValueX, float leftDroneControlsThumbStickValueY) = GetThumbStickStateValues(state, _leftDroneControlsThumbStick);
                    (float rightDroneControlsThumbStickValueX, float rightDroneControlsThumbStickValueY) = GetThumbStickStateValues(state, _rightDroneControlsThumbStick);

                    ButtonState moveSprintThumbStickButtonState = GetThumbStickState(state, _moveSprintThumbStick);
                    ButtonState moveSprintThumbStickPreviousButtonState = GetThumbStickState(prevState, _moveSprintThumbStick);

                    if (_leftDroneControlsThumbStick == ControllerThumbStick.Unset && _rightDroneControlsThumbStick == ControllerThumbStick.Unset)
                    {
                        this.MoveAndRotateCamera(moveSprintThumbStickValueX, moveSprintThumbStickValueY,
                            elevationDownTriggerValue, elevationUpTriggerValue,
                            lookInvertedYThumbStickValueX, lookInvertedYThumbStickValueY,
                            lookThumbStickValueX, lookThumbStickValueY);

                        //Rotate Camera--------------------------------------------------------------------------------------
                        if (_lookInvertedYThumbStick != ControllerThumbStick.Unset)
                        {
                            if (Math.Abs(lookInvertedYThumbStickValueX) >= 0.002 || Math.Abs(lookInvertedYThumbStickValueY) >= 0.002)
                            {
                                EulerAngleCamera(lookInvertedYThumbStickValueY, -lookInvertedYThumbStickValueX);
                            }
                            else
                            {
                                isRotating = false;
                            }
                        }
                        else if (_lookThumbStick != ControllerThumbStick.Unset)
                        {
                            //if (Math.Abs(state.ThumbSticks.Right.X) >= 0.002 || Math.Abs(state.ThumbSticks.Right.Y) >= 0.002)
                            if (Math.Abs(lookThumbStickValueX) >= 0.002 || Math.Abs(lookThumbStickValueY) >= 0.002)
                            {
                                //EulerAngleCamera(-state.ThumbSticks.Right.Y, -state.ThumbSticks.Right.X);
                                EulerAngleCamera(-lookThumbStickValueY, -lookThumbStickValueX);
                            }
                            else
                            {
                                isRotating = false;
                            }
                        }
                    }
                    else
                    {
                        /*
                         * leftDroneControlsThumbStickValueY => Move Forward/Backward
                         * leftDroneControlsThumbStickValueX => Turn Left/Right
                         * rightDroneControlsThumbStickValueY < 0 && >= 0.002 => Elevation Down and rightDroneControlsThumbStickValueY > 0 && >= 0.002 => Elevation Up
                         * rightDroneControlsThumbStickValueX and *rightDroneControlsThumbStickValueY => Move Left/Right
                         */
                        this.DroneControlsNavigateCamera(leftDroneControlsThumbStickValueX, leftDroneControlsThumbStickValueY,
                            rightDroneControlsThumbStickValueX, rightDroneControlsThumbStickValueY);
                        //if (isMoving)
                        //{
                        //    //oDoc = Autodesk.Navisworks.Api.Application.ActiveDocument;
                        //    oCurrentViewCopy = oDoc.CurrentViewpoint.CreateCopy();

                        //    isMoving = false;
                        //}
                        //formDebug.textBox1.Text = mv.Length().ToString(); 

                        #region momentum removed
                        //if (mv.Length() > 0.01)
                        //{
                        //    mv = System.Numerics.Vector2.Lerp(mv, System.Numerics.Vector2.Zero, 2.2f * TimeDeltatime);
                        //    MoveCamera(mv.X / Speed, mv.Y / Speed, 0);
                        //}
                        //else mv = System.Numerics.Vector2.Zero; 
                        #endregion
                    }

                    //Button Back----------------------------------------------------------------------------------------
                    #region Button Back : Finish plugin
                    if (prevState.Buttons.Back == ButtonState.Released && state.Buttons.Back == ButtonState.Pressed)
                    {
                        Vibration(1, 0.35f);
                        isrun = false;

                        // Disable Target mark.
                        TargetDisable();
                    }
                    #endregion

                    //Button Start----------------------------------------------------------------------------------------
                    #region Button Start : change viewpoint
                    //if (prevState.Buttons.Start == ButtonState.Released && state.Buttons.Start == ButtonState.Pressed)
                    if (nextViewpointButtonPreviousState == ButtonState.Released && nextViewpointButtonState == ButtonState.Pressed)
                    {
                        List<SavedItem> savedItems = GetSavedItems(oDoc);
                        if (savedItems.Count != 0)
                        {
                            if (CurrentSavedItem == null)
                            {
                                CurrentSavedItem = savedItems[0];
                            }
                            else
                            {
                                int index = savedItems.IndexOf(CurrentSavedItem);
                                CurrentSavedItem = index + 1 < savedItems.Count ? savedItems[index + 1] : savedItems[0];
                            }
                            oDoc.SavedViewpoints.CurrentSavedViewpoint = CurrentSavedItem;
                        }

                        // Enable Target mark for CurrentSavedViewpoint.
                        //TargetEnable();

                        //
                        //                        GetListSaveVPts();
                        //#if NW2016 || NW2017
                        //                 if (oAllVPts.Count != 0)
                        //                {
                        //                    if(CurrCountAllVPts==oAllVPts.Count)
                        //                    {
                        //                        oDoc.CurrentViewpoint.CopyFrom(oAllVPts[IDCurrentVpts]);
                        //                        IDCurrentVpts++;
                        //                        if (IDCurrentVpts >= oAllVPts.Count) IDCurrentVpts = 0;

                        //                    } else
                        //                    {
                        //                        IDCurrentVpts = 0;
                        //                        oDoc.CurrentViewpoint.CopyFrom(oAllVPts[IDCurrentVpts]);
                        //                        CurrCountAllVPts = oAllVPts.Count;
                        //                    }

                        //                }
                        //                else
                        //                {
                        //                    System.Windows.Forms.MessageBox.Show("The current version is not supported");
                        //                }
                        //#else

                        //                        //For NW2018,NW2019
                        //                        if (oAllVPts.Count != 0)
                        //                        {

                        //                            if (oAllVPts.Count >= CurrCountAllVPts)
                        //                            {

                        //                                oDoc.CurrentViewpoint.CopyFrom(oAllVPts[IDCurrentVpts]);
                        //                                IDCurrentVpts++;

                        //                                if (oAllVPts.Count > CurrCountAllVPts) CurrCountAllVPts = oAllVPts.Count;
                        //                                if (IDCurrentVpts >= oAllVPts.Count) IDCurrentVpts = 0;

                        //                            }
                        //                            else
                        //                            {
                        //                                IDCurrentVpts = 0;
                        //                                oDoc.CurrentViewpoint.CopyFrom(oAllVPts[IDCurrentVpts]);
                        //                                CurrCountAllVPts = oAllVPts.Count;
                        //                            }

                        //                        }
                        //                        else
                        //                        {
                        //                            if (oDoc.HomeView != null) oDoc.CurrentViewpoint.CopyFrom(oDoc.HomeView);
                        //                            else
                        //                            {
                        //                                System.Windows.Forms.MessageBox.Show("Home view not found! \n Please set one");
                        //                                oCurrentViewCopy = oDoc.CurrentViewpoint.CreateCopy();
                        //                            }
                        //                        }
                        //#endif
                    }
                    #endregion

                    //Button LB-------------------------------------------------------------------------------------------
                    #region Button LB = Null
                    if (prevState.Buttons.LeftShoulder == ButtonState.Released && state.Buttons.LeftShoulder == ButtonState.Pressed)
                    {
                        //Speed = 2.2;
                    }

                    //Button LB Released----------------------------------------------------------------------------------------

                    if (prevState.Buttons.LeftShoulder == ButtonState.Pressed && state.Buttons.LeftShoulder == ButtonState.Released)
                    {
                        //Speed = 1;
                    }
                    #endregion

                    //Button RB-------------------------------------------------------------------------------------------
                    #region Button RB = Null
                    //if (prevState.Buttons.RightShoulder == ButtonState.Released && state.Buttons.RightShoulder == ButtonState.Pressed)
                    if (shortestDistanceButtonPreviousState == ButtonState.Released && shortestDistanceButtonState == ButtonState.Pressed)
                    {
                        TryShowShortestDistance();
                    }
                    //Button RB Released--------------------------------------------------------------------------------
                    if (prevState.Buttons.RightShoulder == ButtonState.Pressed && state.Buttons.RightShoulder == ButtonState.Released)
                    {
                        //Speed = 1;
                    }
                    #endregion

                    //Button Left Stick
                    #region Button LeftStick : Speed up
                    //if (prevState.Buttons.LeftStick == ButtonState.Released && state.Buttons.LeftStick == ButtonState.Pressed)
                    if (moveSprintThumbStickPreviousButtonState == ButtonState.Released && moveSprintThumbStickButtonState == ButtonState.Pressed)
                    {
                        Speed = 3;
                    }
                    //Button Left Stick Released
                    //if (prevState.Buttons.LeftStick == ButtonState.Pressed && state.Buttons.LeftStick == ButtonState.Released)
                    if (moveSprintThumbStickPreviousButtonState == ButtonState.Pressed && moveSprintThumbStickButtonState == ButtonState.Released)
                    {
                        Speed = 1;
                    }
                    #endregion

                    //Button A--------------------------------------------------------------------------------------------
                    #region Button A : Null
                    //if (prevState.Buttons.A == ButtonState.Released && state.Buttons.A == ButtonState.Pressed)
                    if (selectDeselectButtonPreviousState == ButtonState.Released && selectDeselectButtonState == ButtonState.Pressed)
                    {
                        SelectDeselect();
                    }
                    #endregion
                    //Button B-------------------------------------------------------------------------------------------
                    #region Button B : Null
                    //if (prevState.Buttons.B == ButtonState.Released && state.Buttons.B == ButtonState.Pressed)
                    if (deselectAllButtonPreviousState == ButtonState.Released && deselectAllButtonState == ButtonState.Pressed)
                    {
                        DeselectAll();
                    }
                    #endregion
                    //Button X------------------------------------------------------------------------------------------
                    #region Button X : Null
                    //if (prevState.Buttons.X == ButtonState.Released && state.Buttons.X == ButtonState.Pressed)
                    if (hideUnhideButtonPreviousState == ButtonState.Released && hideUnhideButtonState == ButtonState.Pressed)
                    {
                        HideSelected();
                    }
                    #endregion
                    //Button Y------------------------------------------------------------------------------------------
                    #region Button Y : Null
                    //if (prevState.Buttons.Y == ButtonState.Released && state.Buttons.Y == ButtonState.Pressed)
                    if (unhideAllButtonPreviousState == ButtonState.Released && unhideAllButtonState == ButtonState.Pressed)
                    {
                        UnhideAll();
                    }
                    #endregion
                    //
                    // Enable Text Markup function.
                    if (enableTextMarkupButtonPreviousState == ButtonState.Released && enableTextMarkupButtonState == ButtonState.Pressed)
                    {
                        EnableTextMarkup();
                    }
                    // Create Viewpoint function.
                    if (createViewpointButtonPreviousState == ButtonState.Released && createViewpointButtonState == ButtonState.Pressed)
                    {
                        CreateViewpoint();
                    }
                    // Show Hide Properties function.
                    if (showHidePropertiesButtonPreviousState == ButtonState.Released && showHidePropertiesButtonState == ButtonState.Pressed)
                    {
                        ShowHideProperties();
                    }
                    // Show Hide Selection Tree function.
                    if (showHideSelectionTreeButtonPreviousState == ButtonState.Released && showHideSelectionTreeButtonState == ButtonState.Pressed)
                    {
                        ShowHideSelectionTree();
                    }
                    // Show Hide Viewpoints function.
                    if (showHideViewpointsButtonPreviousState == ButtonState.Released && showHideViewpointsButtonState == ButtonState.Pressed)
                    {
                        ShowHideViewpoints();
                    }
                    // Person View 3rd function.
                    if (personView3rdButtonPreviousState == ButtonState.Released && personView3rdButtonState == ButtonState.Pressed)
                    {
                        ToogleThirdPerson();
                    }
                    // Collision Gravity function.
                    if (collisionGravityButtonPreviousState == ButtonState.Released && collisionGravityButtonState == ButtonState.Pressed)
                    {
                        ToogleCollisionGravity();
                    }
                    // Move Left In Tree function.
                    if (moveLeftInTreeButtonPreviousState == ButtonState.Released && moveLeftInTreeButtonState == ButtonState.Pressed)
                    {
                        MoveLeftInTree();
                    }
                    //

                    await Task.Delay(10);
                    #region Show FPS 

                    //if (currentTick - lastTime >= 1000)
                    //{
                    //    fps = frames;
                    //    frames = 0;
                    //    lastTime = currentTick;
                    //}

                    //frames++;

                    //deltaTime = (currentTick - lastTime) / 1000f;

                    //if (fps > 1)
                    //{

                    //    TimeDeltatime = (1 / (float)fps);
                    //    //if (TimeDeltatime >= 0.05) TimeDeltatime = 0.05f;
                    //    //if (TimeDeltatime <= 0.01) TimeDeltatime = 0.01f;
                    //}
                    #endregion
                    TimeDeltatime = sw.ElapsedTicks * 1000 / Stopwatch.Frequency;

                    sw.Stop();
                }
            }
            catch (Exception ex)
            {
                ex.LogException();

                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out IntPtr lpdwProcessId);

        private IntPtr GetActiveAppProcessId()
        {
            IntPtr activeAppHandle = GetForegroundWindow();

            GetWindowThreadProcessId(activeAppHandle, out IntPtr activeAppProcessId);

            return activeAppProcessId;
        }

        private bool IsCollisionAndGravityEnabled()
        {
            InwNvViewPoint2 oV = ComApiBridge.State.CurrentView.ViewPoint as InwNvViewPoint2;

            return oV.Viewer.CollisionDetection && oV.Viewer.Gravity;
        }
        private void TurnOnWalkMode()
        {
            InwNvViewPoint2 oV = ComApiBridge.State.CurrentView.ViewPoint as InwNvViewPoint2;
            oV.Paradigm = nwEParadigm.eParadigm_WALK;
        }

        private void MoveAndRotateCamera(float moveSprintThumbStickValueX, float moveSprintThumbStickValueY,
            float elevationDownTriggerValue, float elevationUpTriggerValue,
            float lookInvertedYThumbStickValueX, float lookInvertedYThumbStickValueY,
            float lookThumbStickValueX, float lookThumbStickValueY)
        {
            float xValueAbs = Math.Abs(moveSprintThumbStickValueX);
            float yValueAbs = Math.Abs(moveSprintThumbStickValueY);

            if (IsCollisionAndGravityEnabled())
            {
                this.TurnOnWalkMode();

                if (moveSprintThumbStickValueX > 0)
                {
                    _inputSimulator.Keyboard.KeyUp(VirtualKeyCode.LEFT);
                    _inputSimulator.Keyboard.KeyDown(VirtualKeyCode.RIGHT);

                    isThumbStickValueXReleased = false;
                }
                else if (moveSprintThumbStickValueX < 0)
                {
                    _inputSimulator.Keyboard.KeyUp(VirtualKeyCode.RIGHT);
                    _inputSimulator.Keyboard.KeyDown(VirtualKeyCode.LEFT);

                    isThumbStickValueXReleased = false;
                }
                else if (isThumbStickValueXReleased == false)
                {
                    _inputSimulator.Keyboard.KeyUp(VirtualKeyCode.LEFT);
                    _inputSimulator.Keyboard.KeyUp(VirtualKeyCode.RIGHT);

                    isThumbStickValueXReleased = true;
                }

                if (moveSprintThumbStickValueY > 0 && moveSprintThumbStickValueX == 0)
                {
                    _inputSimulator.Keyboard.KeyUp(VirtualKeyCode.DOWN);
                    _inputSimulator.Keyboard.KeyDown(VirtualKeyCode.UP);

                    isThumbStickValueYReleased = false;
                }
                else if (moveSprintThumbStickValueY < 0 && moveSprintThumbStickValueX == 0)
                {
                    _inputSimulator.Keyboard.KeyUp(VirtualKeyCode.UP);
                    _inputSimulator.Keyboard.KeyDown(VirtualKeyCode.DOWN);

                    isThumbStickValueYReleased = false;
                }
                else if (isThumbStickValueYReleased == false)
                {
                    _inputSimulator.Keyboard.KeyUp(VirtualKeyCode.DOWN);
                    _inputSimulator.Keyboard.KeyUp(VirtualKeyCode.UP);

                    isThumbStickValueYReleased = true;
                }
            }
            else
            {
                //Move Camera----------------------------------------------------------------------------------------
                //if (Math.Abs(state.ThumbSticks.Left.X) >= 0.002 ||
                //    Math.Abs(state.ThumbSticks.Left.Y) >= 0.002 ||
                //    state.Triggers.Left >= 0.002 || state.Triggers.Right >= 0.002)
                if (xValueAbs >= 0.002 || yValueAbs >= 0.002 ||
                elevationDownTriggerValue >= 0.002 || elevationUpTriggerValue >= 0.002)
                {
                    double mcurrentZ = currentZ;

                    //if (state.Triggers.Left >= 0.002)
                    if (elevationDownTriggerValue >= 0.002)
                    {
                        //currentZ = -0.55 * state.Triggers.Left;
                        currentZ = -0.55 * elevationDownTriggerValue;
                    }
                    //else if (state.Triggers.Right >= 0.002)
                    else if (elevationUpTriggerValue >= 0.002)
                        //currentZ = 0.55 * state.Triggers.Right;
                        currentZ = 0.55 * elevationUpTriggerValue;
                    else
                        currentZ = 0;
                    mcurrentZ = currentZ;

                    //MoveCamera(state.ThumbSticks.Left.X, state.ThumbSticks.Left.Y, mcurrentZ);
                    MoveCamera(moveSprintThumbStickValueX, moveSprintThumbStickValueY, mcurrentZ);
                }
                else
                {
                    if (isMoving)
                    {
                        //oDoc = Autodesk.Navisworks.Api.Application.ActiveDocument;
                        oCurrentViewCopy = oDoc.CurrentViewpoint.CreateCopy();

                        isMoving = false;
                    }
                    //formDebug.textBox1.Text = mv.Length().ToString(); 

                    #region momentum removed
                    //if (mv.Length() > 0.01)
                    //{
                    //    mv = System.Numerics.Vector2.Lerp(mv, System.Numerics.Vector2.Zero, 2.2f * TimeDeltatime);
                    //    MoveCamera(mv.X / Speed, mv.Y / Speed, 0);
                    //}
                    //else mv = System.Numerics.Vector2.Zero; 
                    #endregion
                }
            }

            //Rotate Camera--------------------------------------------------------------------------------------
            if (_lookInvertedYThumbStick != ControllerThumbStick.Unset)
            {
                if (Math.Abs(lookInvertedYThumbStickValueX) >= 0.002 || Math.Abs(lookInvertedYThumbStickValueY) >= 0.002)
                {
                    EulerAngleCamera(lookInvertedYThumbStickValueY, -lookInvertedYThumbStickValueX);
                }
                else
                {
                    isRotating = false;
                }
            }
            else
            {
                //if (Math.Abs(state.ThumbSticks.Right.X) >= 0.002 || Math.Abs(state.ThumbSticks.Right.Y) >= 0.002)
                if (Math.Abs(lookThumbStickValueX) >= 0.002 || Math.Abs(lookThumbStickValueY) >= 0.002)
                {
                    //EulerAngleCamera(-state.ThumbSticks.Right.Y, -state.ThumbSticks.Right.X);
                    EulerAngleCamera(-lookThumbStickValueY, -lookThumbStickValueX);
                }
                else
                {
                    isRotating = false;
                }
            }
        }

        private void DroneControlsNavigateCamera(float leftDroneControlsThumbStickValueX, float leftDroneControlsThumbStickValueY,
            float rightDroneControlsThumbStickValueX, float rightDroneControlsThumbStickValueY)
        {
            //this.LeftDroneControlNavigate(leftDroneControlsThumbStickValueX, leftDroneControlsThumbStickValueY);
            //this.RightDroneControlNavigate(rightDroneControlsThumbStickValueX, rightDroneControlsThumbStickValueY);

            // Left Drone Control navigation.
            if (Math.Abs(leftDroneControlsThumbStickValueY) >= 0.002)
            {
                this.MoveCamera(0, leftDroneControlsThumbStickValueY, 0);
            }
            else
            {
                if (isMoving)
                {
                    oCurrentViewCopy = oDoc.CurrentViewpoint.CreateCopy();

                    isMoving = false;
                }
            }

            if (Math.Abs(leftDroneControlsThumbStickValueX) >= 0.002)
            {
                this.EulerAngleCamera(0, -leftDroneControlsThumbStickValueX);
            }
            else
            {
                if (isRotating)
                {
                    oCurrentViewCopy = oDoc.CurrentViewpoint.CreateCopy();

                    isRotating = false;
                }
            }

            // Right Drone Control navigation.
            if (Math.Abs(rightDroneControlsThumbStickValueX) >= 0.002 || Math.Abs(rightDroneControlsThumbStickValueY) >= 0.002)
            {
                if (Math.Abs(rightDroneControlsThumbStickValueY) >= 0.002)
                    currentZ = 0.55 * rightDroneControlsThumbStickValueY;
                else
                    currentZ = 0;

                this.MoveCamera(rightDroneControlsThumbStickValueX, 0, currentZ);
            }
            else if (Math.Abs(leftDroneControlsThumbStickValueY) < 0.002)
            {
                if (isMoving)
                {
                    oCurrentViewCopy = oDoc.CurrentViewpoint.CreateCopy();

                    isMoving = false;
                }
            }

        }
        //private void LeftDroneControlNavigate(double x, double y)
        //{
        //    if (Math.Abs(y) >= 0.002)
        //    {
        //        this.MoveCamera(0, y, 0);
        //    }
        //    else
        //    {
        //        if (isMoving)
        //        {
        //            oCurrentViewCopy = oDoc.CurrentViewpoint.CreateCopy();

        //            isMoving = false;
        //        }
        //    }

        //    if (Math.Abs(x) >= 0.002)
        //    {
        //        this.EulerAngleCamera(0, -x);
        //    }
        //    else
        //    {
        //        if (isRotating)
        //        {
        //            oCurrentViewCopy = oDoc.CurrentViewpoint.CreateCopy();

        //            isRotating = false;
        //        }
        //    }
        //}
        //private void RightDroneControlNavigate(double x, double y)
        //{
        //    if (Math.Abs(x) >= 0.002 || Math.Abs(y) >= 0.002)
        //    {
        //        if (Math.Abs(y) >= 0.002)
        //            currentZ = 0.55 * y;
        //        else
        //            currentZ = 0;

        //        this.MoveCamera(x, 0, currentZ);
        //    }
        //    else
        //    {
        //        if (isMoving)
        //        {
        //            oCurrentViewCopy = oDoc.CurrentViewpoint.CreateCopy();

        //            isMoving = false;
        //        }
        //    }
        //}

        private void SelectDeselect()
        {
            View view = Application.ActiveDocument.ActiveView;
            int x = view.Width / 2;
            int y = view.Height / 2;

            PickItemResult itemResult = Application.ActiveDocument.ActiveView.PickItemFromPoint(x, y);

            if (itemResult != null)
            {
                ModelItem modelItem = itemResult.ModelItem;
                ModelItemCollection modelItems = new ModelItemCollection { modelItem };

                bool isSelectedModelItem = Application.ActiveDocument.CurrentSelection.SelectedItems.Contains(modelItem);
                bool isParentOfSelectedModelItem = Application.ActiveDocument.CurrentSelection.SelectedItems.Contains(modelItem.Parent);

                if (isParentOfSelectedModelItem)
                {
                    modelItem = modelItem.Parent;
                }

                if (isSelectedModelItem || isParentOfSelectedModelItem)
                {
                    Selection selectedItemsCopy = Application.ActiveDocument.CurrentSelection.CreateCopy();

                    Application.ActiveDocument.CurrentSelection.Clear();
                    selectedItemsCopy.ExplicitSelection.Remove(modelItem);
                    Application.ActiveDocument.CurrentSelection.CopyFrom(selectedItemsCopy);
                }
                else
                {
                    Application.ActiveDocument.CurrentSelection.Add(modelItem);
                }
            }

            //Vibration(1, 0.25f);
            //isFly = !isFly;
            //if (!isFly)
            //{
            //    if (oCurrentViewCopy.Tool != Tool.NavigateWalk) oCurrentViewCopy.Tool = Tool.NavigateWalk;
            //}
            //else if (oCurrentViewCopy.Tool != Tool.NavigateFly) oCurrentViewCopy.Tool = Tool.NavigateFly;

            //toogleRealism();
            //Speed = 3.0;
        }

        private void DeselectAll()
        {
            Application.ActiveDocument.CurrentSelection.Clear();
        }

        private void HideSelected()
        {
            Document document = Application.ActiveDocument;

            if (document.CurrentSelection.Value.GetSelectedItems().Count() > 0)
            {
                document.Models.SetHidden(document.CurrentSelection.Value.GetSelectedItems(), true);
                DeselectAll();
            }
            else
            {
                UnhideAll();
            }
        }

        private void HideUnselected(ModelItemCollection selModelItems)
        {
            //Create hidden collection
            ModelItemCollection hidden = new ModelItemCollection();

            //create a store for the visible items
            ModelItemCollection visible = new ModelItemCollection();

            //Add all the items that are visible to the visible collection            
            foreach (ModelItem item in selModelItems)
            {
                if (item.AncestorsAndSelf != null)
                    visible.AddRange(item.AncestorsAndSelf);
                if (item.Descendants != null)
                    visible.AddRange(item.Descendants);
            }

            //mark as invisible all the siblings of the visible items as well as the visible items
            foreach (ModelItem toShow in visible)
            {
                if (toShow.Parent != null)
                {
                    hidden.AddRange(toShow.Parent.Children);
                }
            }

            //remove the visible items from the collection
            foreach (ModelItem toShow in visible)
            {
                hidden.Remove(toShow);
            }

            //hide the remaining items
            Application.ActiveDocument.Models.
            SetHidden(hidden, true);
        }

        //private void Isolate()
        //{
        //    ModelItemCollection modelSelectedItems = Application.ActiveDocument.CurrentSelection.Value.GetSelectedItems();

        //    HideUnselected(modelSelectedItems);
        //}

        private void TryShowShortestDistance()
        {
            List<ModelItem> currentSelectedItemGuids = Application.ActiveDocument.CurrentSelection.Value.GetSelectedItems().Cast<ModelItem>().ToList();

            if (currentSelectedItemGuids.TrueForAll(g => previousSelectedItems.Contains(g)) && isShortestDistanceShown)
            {
                Autodesk.Navisworks.Api.Interop.LcRmFrameworkInterface.ExecuteCommand("RoamerGUI_OM_MEASURE_NAV", Autodesk.Navisworks.Api.Interop.LcUCIPExecutionContext.eTOOLBAR);
                isShortestDistanceShown = false;
            }
            else if (currentSelectedItemGuids.Count == 2)
            {
                Autodesk.Navisworks.Api.Interop.LcRmFrameworkInterface.ExecuteCommand("RoamerGUI_MEASURE_SHORTESTDIST", Autodesk.Navisworks.Api.Interop.LcUCIPExecutionContext.eTOOLBAR);
                previousSelectedItems = currentSelectedItemGuids;
                isShortestDistanceShown = true;
            }
        }

        private void UnhideAll()
        {
            Document document = Application.ActiveDocument;
            DocumentModels models = document.Models;

            foreach (Model model in models)
            {
                ModelItem rootItem = model.RootItem;
                ModelItemEnumerableCollection modelItems = rootItem.DescendantsAndSelf;
                document.Models.SetHidden(modelItems, false);
            }
        }

        #region Move Camera
        private void MoveCamera(double x, double y, double z)
        {
            // To move the camera, we can just directly manipulate the
            // Position property. Rotation will remain unchanged, so view direction
            // is not changed.
            //if (!isCOM)

            if (!isMoving)
            {

                SetConstantUnit();

                //oCurrentViewCopy = oDoc.CurrentViewpoint.CreateCopy();
                oDoc = Application.ActiveDocument;
                if (oDoc != null)
                {
                    oCurrentViewCopy = oDoc.CurrentViewpoint.CreateCopy();
                    oCurrentViewCopy.LinearSpeed = ((double)SpeedSetting / LinearSpeedFactor) * 1000;
                    oCurrentViewCopy.AngularSpeed = AngularSetting * Math.PI / 180;
                    oDoc.CurrentViewpoint.CopyFrom(oCurrentViewCopy);
                }
                #region Update 3
                SetRuntimeSettingUIAS();
                #endregion

                isMoving = true;
            }

            #region Move Mode 1
            // get view direction
            //Vector3D oViewDir = NavisUtils.Instance.getViewDir(oCurrentViewCopy);

            //Vector3 mV3 = NavisUtils.Instance.V3DtoV3(oViewDir);

            //Vector3 PDir = Vector3.Reflect(mV3, Vector3.UnitZ);

            //Vector3 PerpenPdir = new Vector3(PDir.Y, -PDir.X, PDir.Z);

            //Vector3 pos = NavisUtils.Instance.Point3DtoV3(oCurrentViewCopy.Position);

            //pos += (float)y * (PDir * (Vector3.UnitY + Vector3.UnitX));

            //pos += (float)x * (PerpenPdir * (Vector3.UnitX + Vector3.UnitY));

            //pos += (float)z * Vector3.UnitZ;

            //Point3D newPos = NavisUtils.Instance.V3toPoint3D(pos);
            //oCurrentViewCopy.Position = newPos;
            //oDoc.CurrentViewpoint.CopyFrom(oCurrentViewCopy);
            #endregion

            #region Move Mode 2
            // get view direction
            //Vector3D oViewDir = NavisUtils.Instance.getViewDir(oCurrentViewCopy);

            //Vector3D PerpenPdir = new Vector3D(oViewDir.Y, -oViewDir.X, oViewDir.Z);

            //Vector3D pos = NavisUtils.Instance.Point3DtoV3D(oCurrentViewCopy.Position);

            //pos += y * (oViewDir * (UnitVector3D.UnitY + UnitVector3D.UnitX));

            //pos += x * (PerpenPdir * (UnitVector3D.UnitY + UnitVector3D.UnitX));

            //pos += z * UnitVector3D.UnitZ;

            //Point3D newPos = NavisUtils.Instance.V3DtoPoint3D(pos);
            //oCurrentViewCopy.Position = newPos;
            //oDoc.CurrentViewpoint.CopyFrom(oCurrentViewCopy);
            #endregion

            #region Move Mode 3

            Vector3D oViewDir = NavisUtils.Instance.getViewDir(oCurrentViewCopy);

            Vector3D PerpenPdir = new Vector3D(oViewDir.Y, -oViewDir.X, oViewDir.Z);

            Vector3D pos = NavisUtils.Instance.Point3DtoV3D(oCurrentViewCopy.Position);

            pos += (y * (oViewDir * (UnitVector3D.UnitY + UnitVector3D.UnitX)) + x * (PerpenPdir * (UnitVector3D.UnitY + UnitVector3D.UnitX))) * 0.003 * Speed * ((double)SpeedSetting / LinearSpeedFactor) * ConstantUnit * TimeDeltatime;
            pos += z * UnitVector3D.UnitZ * 0.003 * Speed * ((double)SpeedSetting / LinearSpeedFactor) * ConstantUnit * TimeDeltatime;

            //oCurrentViewCopy.Tool = Tool.NavigateWalk;

            //oDoc.Units == Units.Millimeters;
            //if (CanContinueMotion(pos.ToPoint3D()))
            //{
            Point3D newPos = NavisUtils.Instance.V3DtoPoint3D(pos);
            oCurrentViewCopy.Position = newPos;
            oDoc.CurrentViewpoint.CopyFrom(oCurrentViewCopy);
            //}


            ////Test
            ////View view = Application.ActiveDocument.ActiveView;
            ////int xx = view.Width / 2;
            ////int yy = view.Height / 2;
            ////PickItemResult itemResult = Application.ActiveDocument.ActiveView.PickItemFromPoint(xx, xx);
            ////ModelItem modelItem = itemResult.ModelItem;
            ////Point3D point3D = modelItem.BoundingBox().Center;
            ////PickItemResult pickItemResult1 = new PickItemResult();
            ////pickItemResult1.Point = itemResult.Point;
            ////ModelItemCollection modelItems = new ModelItemCollection { modelItem };
            ////Selection selection = new Selection(modelItems);
            ////DocumentClash clash =Application.ActiveDocument.GetClash();
            ////ClashTest clashTest = new ClashTest();
            ////clashTest.SelectionA.Selection.CopyFrom(selection);
            ////clash.TestsData.TestsRunTest(clashTest);
            //InwOpState10 state = ComApiBridge.State;
            //OnCollisionEnter(state);
            //state.BeginEdit("Move Camera");

            //InwNvViewPoint2 oV = (InwNvViewPoint2)state.CurrentView.ViewPoint;
            //oV.Viewer.CameraMode = nwECameraMode.eCameraMode_ThirdPerson;
            //oV.Paradigm = nwEParadigm.eParadigm_WALK;//eParadigm_FLY
            //oV.Viewer.CollisionDetection = true;
            //oV.Viewer.Gravity = true;

            //InwLPos3f camPos = oV.Camera.Position;//cam.Position;
            //camPos.data1 += x * 0.003 * Speed * ((double)SpeedSetting / LinearSpeedFactor) * ConstantUnit * TimeDeltatime;
            //camPos.data2 += y * 0.003 * Speed * ((double)SpeedSetting / LinearSpeedFactor) * ConstantUnit * TimeDeltatime;
            //camPos.data3 += z * 0.003 * Speed * ((double)SpeedSetting / LinearSpeedFactor) * ConstantUnit * TimeDeltatime;

            //state.EndEdit();
            ////

            #endregion

            //  correct for Forward:
            //Point3D newPos = new Point3D(oCurrentViewCopy.Position.X + PDir.X * y /*+ PDir.Y * x*/,
            //                                oCurrentViewCopy.Position.Y + /*PDir.X * x +*/ PDir.Y * y,
            //                                oCurrentViewCopy.Position.Z + z);
            //double alpha;
            //if (Vector3.Dot(PDir, NavisUtils.Instance.V3DtoV3(UnitVector3D.UnitX)) > 0)
            //    alpha = NavisUtils.Instance.AngleBetween(PDir, NavisUtils.Instance.V3DtoV3(UnitVector3D.UnitX));
            //else
            //    alpha = 180 - NavisUtils.Instance.AngleBetween(PDir, NavisUtils.Instance.V3DtoV3(UnitVector3D.UnitX));

            //Point3D newPos = new Point3D(oCurrentViewCopy.Position.X + PDir.X * Math.Cos(alpha) + PDir.Y * Math.Sin(alpha),
            //                               oCurrentViewCopy.Position.Y + PDir.X * Math.Sin(alpha) + PDir.Y * Math.Cos(alpha),
            //                               oCurrentViewCopy.Position.Z + z);

            //Point3D newPos = new Point3D(oCurrentViewCopy.Position.X + x1,
            //                               oCurrentViewCopy.Position.Y + y1,
            //                               oCurrentViewCopy.Position.Z + z);

            //Matrix4x4 m0 = Matrix4x4.CreateWorld(Vector3.Zero,PDir,Vector3.UnitZ);

            //Quaternion q1 = NavisUtils.Instance.Rot3DtoQuaternion(EulerAngleCamera(-state.ThumbSticks.Right.Y * 0.02 * Speed, -state.ThumbSticks.Right.X * 0.02 * Speed));

            //Point3D Pos = oCurrentViewCopy.Position;
            //Rotation3D rot = oCurrentViewCopy.Rotation;

            //Matrix4x4 m0 = Matrix4x4.CreateWorld(NavisUtils.Instance.Point3DtoV3(Pos), pdir, Vector3.UnitZ);

            //Matrix4x4 m1 = Matrix4x4.Transform(m0, NavisUtils.Instance.Rot3DtoQuaternion(rot));

            //Vector3 movepos = new Vector3((float)(Pos.X ), (float)(Pos.Y), (float)(Pos.Z));

            //Vector3 ConvertToLocal = Vector3.Transform(movepos, m0);

            //Vector3 movelocal = new Vector3(ConvertToLocal.X+(float)x, ConvertToLocal.Y+ (float)y, ConvertToLocal.Z + (float)z);

            //Vector3 ConvertToGlobal = Vector3.Transform(movelocal, Matrix4x4.CreateWorld(Vector3.Zero, Vector3.UnitY, Vector3.UnitZ));

            //oCurrentViewCopy.Position = NavisUtils.Instance.V3toPoint3D( ConvertToGlobal);

            //textDebug = "false dsfsdfsdfsd";
            //formDebug.textBox1.Text = textDebug;

            //else
            //{

            //    m_state.BeginEdit("Move Camera");

            //    oV = (COMApi.InwNvViewPoint2)m_state.CurrentView.ViewPoint;
            //    oV.Viewer.CameraMode = COMApi.nwECameraMode.eCameraMode_FirstPerson;
            //    oV.Paradigm = COMApi.nwEParadigm.eParadigm_WALK;//eParadigm_FLY
            //    oV.Viewer.CollisionDetection = true;
            //    oV.Viewer.Gravity = true;

            //    COMApi.InwLPos3f pos = oV.Camera.Position;//cam.Position;
            //    pos.data1 += x;
            //    pos.data2 += y;
            //    pos.data3 += z;

            //    m_state.EndEdit();
            //    oV = (COMApi.InwNvViewPoint2)m_state.CurrentView.ViewPoint;
            //    //oV.Copy(); 
            //    //textDebug = OnCollisionEnter(oV.Camera).ToString();
            //    //formDebug.textBox1.Text = textDebug;

            //    //updatePos();

            //}
        }

        #endregion

        #region Rotation Camera By Quaternion
        private void RotateCamera(double angle, double rotX, double rotY, double rotZ) //angle 0.1-0.01
        {
            //Document oDoc = Autodesk.Navisworks.Api.Application.ActiveDocument;

            //Viewpoint currentVP = oDoc.CurrentViewpoint;

            //  Make a copy of current viewpoint	
            Viewpoint oCurrVCopy = oDoc.CurrentViewpoint.CreateCopy();

            //  set the axis we will rotate around （Ｚ：０,０,１）
            UnitVector3D odeltaA = new UnitVector3D(rotX, rotY, rotZ);
            // Create delta of Quaternion: 
            Rotation3D delta = new Rotation3D(odeltaA, angle);
            // multifly the current Quaternion with the delta , get the new Quaternion 

            oCurrVCopy.Rotation = Multiply(oCurrVCopy.Rotation, delta);

            // Update current viewpoint
            oDoc.CurrentViewpoint.CopyFrom(oCurrVCopy);

        }
        #endregion

        #region Rotation Camera By EulerAngle
        private void EulerAngleCamera(double XAxis, double YAxis)
        {
            if (!isRotating)
            {
                SetConstantUnit();
                oCurrentViewCopy = oDoc.CurrentViewpoint.CreateCopy();

                #region Update 3
                SetRuntimeSettingUIAS();
                #endregion

                isRotating = true;
            }

            Vector3D EulerAngel = toEulerAngle(oCurrentViewCopy.Rotation) + new Vector3D(0.045 * YAxis * Speed * AngularSetting * TimeDeltatime / 1000, 0, 0.045 * XAxis * Speed * AngularSetting * TimeDeltatime / 1000);

            if (EulerAngel.Z >= 0 && EulerAngel.Z <= Math.PI)
            {
                oCurrentViewCopy.Rotation = toQuaternion(EulerAngel);
            }
            else
            {
                EulerAngel = toEulerAngle(oCurrentViewCopy.Rotation);
                oCurrentViewCopy.Rotation = toQuaternion(EulerAngel);
            }

            // Update current viewpoint
            oDoc.CurrentViewpoint.CopyFrom(oCurrentViewCopy);
        }

        #endregion

        #region Rotation3D Multiply
        public static Rotation3D Multiply(Rotation3D r1, Rotation3D r2)
        {
            Rotation3D res = new Rotation3D(r2.D * r1.A + r2.A * r1.D + r2.B * r1.C - r2.C * r1.B,
                r2.D * r1.B + r2.B * r1.D + r2.C * r1.A - r2.A * r1.C,
                r2.D * r1.C + r2.C * r1.D + r2.A * r1.B - r2.B * r1.A,
                r2.D * r1.D - r2.A * r1.A - r2.B * r1.B - r2.C * r1.C);

            return res;
        }
        #endregion

        #region Quaternion To EulerAngle
        public Vector3D toEulerAngle(Rotation3D q)
        {
            double roll, pitch, yaw;
            // roll (x-axis rotation)
            double sinr_cosp = +2.0 * (q.A * q.B + q.C * q.D);
            double cosr_cosp = +1.0 - 2.0 * (q.B * q.B + q.C * q.C);
            roll = Math.Atan2(sinr_cosp, cosr_cosp);

            // pitch (y-axis rotation)
            double sinp = +2.0 * (q.A * q.C - q.D * q.B);
            if (Math.Abs(sinp) >= 1)
            {
                if (sinp >= 0) pitch = Math.PI / 2;
                else pitch = -Math.PI / 2;
                //pitch =copysign(Math.PI / 2, sinp); // use 90 degrees if out of range
            }
            else
                pitch = Math.Asin(sinp);

            // yaw (z-axis rotation)
            double siny_cosp = +2.0 * (q.A * q.D + q.B * q.C);
            double cosy_cosp = +1.0 - 2.0 * (q.C * q.C + q.D * q.D);
            yaw = Math.Atan2(siny_cosp, cosy_cosp);

            return new Vector3D(roll, pitch, yaw);
        }
        #endregion

        #region Rotation To Quaternion
        public Rotation3D toQuaternion(Vector3D mEulerAnger) // yaw (Z), pitch (Y), roll (X)
        {
            double yaw, pitch, roll;
            yaw = mEulerAnger.Z;
            pitch = mEulerAnger.Y;
            roll = mEulerAnger.X;
            // Abbreviations for the various angular functions
            double cy = Math.Cos(yaw * 0.5);
            double sy = Math.Sin(yaw * 0.5);
            double cp = Math.Cos(pitch * 0.5);
            double sp = Math.Sin(pitch * 0.5);
            double cr = Math.Cos(roll * 0.5);
            double sr = Math.Sin(roll * 0.5);

            Rotation3D q = new Rotation3D(cy * cp * cr + sy * sp * sr,
                cy * cp * sr - sy * sp * cr,
                sy * cp * sr + cy * sp * cr,
                sy * cp * cr - cy * sp * sr);
            //q.A = cy * cp * cr + sy * sp * sr;
            //q.B = cy * cp * sr - sy * sp * cr;
            //q.C = sy * cp * sr + cy * sp * cr;
            //q.D = sy * cp * cr - cy * sp * sr;
            return q;
        }

        #endregion

        #region Get Current Time Internet
        protected static DateTime GetCurrentTime()
        {
            try
            {
                using (var response = System.Net.WebRequest.Create("http://www.google.com").GetResponse())
                { //string todaysDates =  response.Headers["date"];
                    return DateTime.ParseExact(response.Headers["date"],
                        "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                        CultureInfo.InvariantCulture.DateTimeFormat,
                        DateTimeStyles.AssumeUniversal);

                }

            }
            catch (System.Net.WebException)
            {
                System.Windows.Forms.MessageBox.Show("Please connect to the internet!!!");
                return new DateTime(2018, 11, 11, 0, 0, 0); //In case something goes wrong. 
            }

        }
        #endregion

        #region Showing infor F1 for Help
        public override bool TryShowCommandHelp(String commandId)
        {
            //System.Windows.MessageBox.Show("Showing Help for command with the Id " + commandId);
            System.Diagnostics.Process.Start("https://help.buildfore.com/ctrlwiz/ctrlwiz-for-navisworks/how-to-use-ctrlwiz-navisworks");
            return true;
        }
        #endregion
    }
}

#region Old Code
//
//[DllImport("user32.dll")]
//public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
//const int VK_UP = 0x26; //up key
//const int VK_DOWN = 0x28;  //down key
//const int VK_LEFT = 0x25;
//const int VK_RIGHT = 0x27;
//const uint KEYEVENTF_KEYDOWN = 0x0000; // New definition
//const uint KEYEVENTF_KEYUP = 0x0002; // Key up flag
//const uint KEYEVENTF_EXTENDEDKEY = 0x0001;

//private string GetCurrentAppName()
//{
//    IntPtr activeAppHandle = GetForegroundWindow();

//    GetWindowThreadProcessId(activeAppHandle, out IntPtr activeAppProcessId);

//    Process currentAppProcess = Process.GetProcessById((int)activeAppProcessId);
//    string currentAppName = FileVersionInfo.GetVersionInfo(currentAppProcess.MainModule.FileName).FileDescription;

//    return currentAppName;
//}

//[DllImport("user32.dll")]
//private static extern bool PostMessage(
//    IntPtr hWnd, // handle to destination window 
//    UInt32 Msg, // message 
//    Int32 wParam, // first message parameter 
//    Int32 lParam // second message parameter
//    );
//const int WM_KEYDOWN = 0x100;

//private void ReleaseKeyboardKeysViaPostMessage()
//{
//    PostMessage(activeAppProcessId, WM_KEYDOWN, (int)WinForms.Keys.Up, 0);
//    PostMessage(activeAppProcessId, WM_KEYDOWN, (int)WinForms.Keys.Down, 0);
//    PostMessage(activeAppProcessId, WM_KEYDOWN, (int)WinForms.Keys.Right, 0);
//    PostMessage(activeAppProcessId, WM_KEYDOWN, (int)WinForms.Keys.Left, 0);
//}

//private void ReleaseKeyboardKeys()
//{
//    //if (Keyboard.IsKeyDown(Key.Up))
//    //{
//    //this.ReleaseKeyboardKey(KeyboardKeyCode.Up, BScanCode.Up);
//    _inputSimulator.Keyboard.KeyUp(VirtualKeyCode.UP);
//    //}
//    //if (Keyboard.IsKeyDown(Key.Down))
//    //{
//    //this.ReleaseKeyboardKey(KeyboardKeyCode.Down, BScanCode.Down);
//    _inputSimulator.Keyboard.KeyUp(VirtualKeyCode.DOWN);
//    //}
//    //if (Keyboard.IsKeyDown(Key.Right))
//    //{
//    //this.ReleaseKeyboardKey(KeyboardKeyCode.Right, BScanCode.Right);
//    _inputSimulator.Keyboard.KeyUp(VirtualKeyCode.RIGHT);
//    //}
//    //if (Keyboard.IsKeyDown(Key.Left))
//    //{
//    //this.ReleaseKeyboardKey(KeyboardKeyCode.Left, BScanCode.Left);
//    _inputSimulator.Keyboard.KeyUp(VirtualKeyCode.LEFT);
//    //}
//}
//if (xValueAbs >= 0.002 || yValueAbs >= 0.002)
//{
//    if (xValueAbs >= 0.002)
//    {
//        if (moveSprintThumbStickValueX > 0)
//            _inputSimulator.Keyboard.KeyDown(VirtualKeyCode.RIGHT);
//        //this.PressKeyboardKey(KeyboardKeyCode.Right, BScanCode.Right);
//        else
//            _inputSimulator.Keyboard.KeyDown(VirtualKeyCode.LEFT);
//            //this.PressKeyboardKey(KeyboardKeyCode.Left, BScanCode.Left);
//    }

//    if (yValueAbs >= 0.002)
//    {
//        if (moveSprintThumbStickValueY > 0)
//            _inputSimulator.Keyboard.KeyDown(VirtualKeyCode.UP);
//            //this.PressKeyboardKey(KeyboardKeyCode.Up, BScanCode.Up);
//        else
//            _inputSimulator.Keyboard.KeyDown(VirtualKeyCode.DOWN);
//            //this.PressKeyboardKey(KeyboardKeyCode.Down, BScanCode.Down);
//    }
//} else
//{
//    this.ReleaseKeyboardKeys();
//}
//
//private bool CanContinueMotion(Point3D position)
//{
//    foreach (ModelItem modelItem in modelItems)
//    {

//        BoundingBox3D box3D = modelItem.BoundingBox(true);
//        //Point3D closestPoint = box3D.ClosestPoint(position);
//        //double distance = closestPoint.DistanceTo(position);
//        if (box3D.Contains(position))
//        {
//            InwOpState oState = ComApiBridge.State;
//            InwOaPath path = ComApiBridge.ToInwOaPath(modelItem);

//            CallbackGeomListener callbkListener = new CallbackGeomListener();
//            foreach (InwOaFragment3 frag in path.Fragments())
//            {
//                InwLTransform3f3 localToWorld = (InwLTransform3f3)frag.GetLocalToWorldMatrix();
//                callbkListener.LCS2WCS = frag.GetLocalToWorldTransformMatrix();
//                Array array_v1 = (Array)(object)frag.Appearance;
//                float[] appearance = array_v1.Cast<float>().ToArray();

//                //generate the primitives
//                frag.GenerateSimplePrimitives(nwEVertexProperty.eNONE, callbkListener);
//                if (callbkListener.Intersects)
//                {
//                    return false;
//                }
//                //InwLBox3f box = frag.GetWorldBox();
//                //Point3D pointMin = new Point3D(box.min_pos.data1, box.min_pos.data2, box.min_pos.data3);
//                //Point3D pointMax = new Point3D(box.max_pos.data1, box.max_pos.data2, box.max_pos.data3);

//                //BoundingBox3D box3D1 = new BoundingBox3D(pointMin, pointMax);
//                //if (box3D1.Contains(position))
//                //{
//                //    return false;
//                //}
//            }
//        }
//    }
//    return true;
//}
////get fragments
//private ICollection<InwOaFragment3> GetModelFragments()
//{
//    InwOpState oState = ComApiBridge.State;
//    InwOpSelection oSel = ComApiBridge.ToInwOpSelection(GetItems());
//    ICollection<InwOaFragment3> fragments = new List<InwOaFragment3>();
//    foreach (InwOaPath3 path in oSel.Paths())
//    {
//        foreach (InwOaFragment3 frag in path.Fragments())
//        {
//            fragments.Add(frag);
//        }
//    }
//    return fragments;
//}
//// get model item collection
//private ModelItemCollection GetItems()
//{
//    // current document
//    Document doc = Application.ActiveDocument;
//    // model items collection-1
//    ModelItemCollection itemCollection = new ModelItemCollection();
//    // get current selected items
//    ModelItemCollection selectedItems = doc.CurrentSelection.SelectedItems;
//    // get appended models 
//    DocumentModels models = doc.Models;

//    // each model
//    foreach (Model model in models)
//    {
//        // collect all items from the mode1
//        // add to model item collection-1
//        itemCollection.AddRange(ItemsFromRoot(model));
//    }
//    return itemCollection;
//}
//// get model item collection
//private ICollection<ModelItem> GetModelItems()
//{
//    // current document
//    Document doc = Application.ActiveDocument;
//    // model items collection-1
//    ModelItemCollection itemCollection = new ModelItemCollection();
//    // get current selected items
//    ModelItemCollection selectedItems = doc.CurrentSelection.SelectedItems;
//    // get appended models 
//    DocumentModels models = doc.Models;

//    // each model
//    foreach (Model model in models)
//    {
//        // collect all items from the mode1
//        // add to model item collection-1
//        itemCollection.AddRange(ItemsFromRoot(model));
//    }
//    return itemCollection.ToList();
//}
//// get descendants from model item
//public IEnumerable<ModelItem> ItemsFromRoot(Model model)
//{
//    // collect all descendants geometric items from a model 
//    return model.RootItem.Descendants.Where(x => x.HasGeometry);
//}
//private void OnCollisionEnter(InwOpState10 oState)
//{
//    //find the clash detective plugin

//    InwOpClashElement m_clash = null;
//    ICollection<string> names = oState.Plugins().Cast<InwBase>().Select(e => e.ObjectName).ToList();
//    foreach (InwBase oPlugin in oState.Plugins())

//    {
//        if (oPlugin.ObjectName == "nwOpClashElement")

//        {

//            m_clash = (InwOpClashElement)oPlugin;

//            break;

//        }

//    }

//    if (m_clash == null)

//    {

//        System.Windows.Forms.MessageBox.Show(

//            "cannot find clash test plugin!");

//        return;

//    }
//}
//private string OnCollisionEnter(COMApi.InwOclClashTest test)
//{

//    //COMApi.InwOclClashTest test = inwNvCamera as COMApi.InwOclClashTest;

//    //m_clash = test as COMApi.InwOpClashElement;

//    //if (m_clash != null) return true;

//    //return false;
//    string rs = "";
//    foreach (COMApi.InwBase oPlugin in m_state.Plugins())
//    {
//        if (oPlugin.ObjectName == "nwOpClashElement")
//        {
//            m_clash = (COMApi.InwOpClashElement)oPlugin;
//            rs = "nwOpClashElement";
//        }
//    }
//    if (m_clash == null)
//    {
//       rs = "cannot find clash test plugin!";
//    }
//    return rs;
//}
//private void toogleRealism()
//{
//    isCOM = !isCOM;

//    //if (isCOM)
//    //{

//    //    oV.Paradigm = COMApi.nwEParadigm.eParadigm_WALK;
//    //    oV.Viewer.CollisionDetection = true;
//    //    oV.Viewer.Gravity = true;
//    //}

//    //var mParadigm = oV.Paradigm;

//    //IDMode++;
//    //if (IDMode > 3) IDMode = 0;

//    //if (IDMode == 1)
//    //{
//    //    oV.Paradigm = COMApi.nwEParadigm.eParadigm_WALK;
//    //    oV.Viewer.CollisionDetection = true;
//    //    oV.Viewer.Gravity = true;

//    //}
//    //else if (IDMode == 2)
//    //{
//    //    oV.Paradigm = COMApi.nwEParadigm.eParadigm_FLY;
//    //    oV.Viewer.CollisionDetection = true;
//    //}
//    //else if(IDMode==3)
//    //{
//    //    oV.Paradigm = COMApi.nwEParadigm.eParadigm_FLY;
//    //    oV.Viewer.CollisionDetection = false;
//    //} else
//    //    oV.Paradigm = mParadigm;
//    //third person
//    //oV.Viewer.CameraMode = COMApi.nwECameraMode.eCameraMode_ThirdPerson;

//    //if (oV.Paradigm == COMApi.nwEParadigm.eParadigm_WALK)
//    //{
//    //    // gravity, coollison or crouch can only take effect in Walk mode.

//    //    oV.Viewer.Gravity = true;//toogle gravity on

//    //    oV.Viewer.CollisionDetection = true;//toogle collision detection on

//    //    oV.Viewer.AutoCrouch = true; //toogle auto crouch on

//    //}

//}
//public static void AddTextToViewpoint(int savedViewpointIndex, string text, Point2D pixelCoordinates)
//{
//    var viewpoints = Application.ActiveDocument.SavedViewpoints;
//    var viewpointCopy = viewpoints.Value[savedViewpointIndex].CreateCopy() as SavedViewpoint;
//    var redlineText = new LcOpRedlineText(text, ScreenToCameraSpace(pixelCoordinates));
//    redlineText.SetLineThickness(5);
//    redlineText.SetLineColor(Color.Red);

//    // Note: the Redlines property is a part of the "hidden" api so it is not officially suppported. Use at own risk.
//    viewpointCopy.Redlines.Add(redlineText);
//    viewpoints.ReplaceWithCopy(savedViewpointIndex, viewpointCopy);
//}

//public static Point2D ScreenToCameraSpace(Point2D pointScreenSpace)
//{
//    // Note: the Viewer property is a part of the "hidden" api so it is not officially supported. Use at own risk.
//    LcOaViewer viewer = Application.ActiveDocument.ActiveView.Viewer;

//    var pointWindowSpace = LcOpRedline.ScreenToWindowSpace(viewer, pointScreenSpace);
//    var pointCameraSpace = LcOpRedline.WindowToCameraSpace(viewer, pointWindowSpace);

//    return pointCameraSpace;
//}
//private void GetControllerElement_SettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
//{
//    object controllerElementObj = Forms.Properties.GetControllerElement.GetControllerElementByPropertyNameNV(e.PropertyName);
//    Function controllerElementFunction = Forms.Properties.GetControllerElement.GetControllerElementFunctionByPropertyNameNV(e.PropertyName);

//    if (controllerElementObj is ControllerButton)
//    {
//        if (controllerElementFunction == Function.SelectDeselect)
//        {
//            _selectDeselectButton = (ControllerButton)controllerElementObj;

//            if (_hideUnhideButton == _selectDeselectButton)
//                _hideUnhideButton = ControllerButton.Unset;
//        }
//        else if (controllerElementFunction == Function.HideUnhide)
//        {
//            _hideUnhideButton = (ControllerButton)controllerElementObj;

//            if (_selectDeselectButton == _hideUnhideButton)
//                _selectDeselectButton = ControllerButton.Unset;
//        }
//        else
//        {
//            if ((ControllerButton)controllerElementObj == _selectDeselectButton)
//                _selectDeselectButton = ControllerButton.Unset;
//            else if ((ControllerButton)controllerElementObj == _hideUnhideButton)
//                _hideUnhideButton = ControllerButton.Unset;
//        }
//    }
//    else if (controllerElementObj is ControllerTrigger)
//    {
//        if (controllerElementFunction == Function.ElevationUp)
//        {
//            _elevationUpTrigger = (ControllerTrigger)controllerElementObj;

//            if (_elevationDownTrigger == _elevationUpTrigger)
//                _elevationDownTrigger = ControllerTrigger.Unset;
//        }
//        else if (controllerElementFunction == Function.ElevationDown)
//        {
//            _elevationDownTrigger = (ControllerTrigger)controllerElementObj;

//            if (_elevationUpTrigger == _elevationDownTrigger)
//                _elevationUpTrigger = ControllerTrigger.Unset;
//        }
//        else
//        {
//            if ((ControllerTrigger)controllerElementObj == _elevationUpTrigger)
//                _elevationUpTrigger = ControllerTrigger.Unset;
//            else if ((ControllerTrigger)controllerElementObj == _elevationDownTrigger)
//                _elevationDownTrigger = ControllerTrigger.Unset;
//        }
//    }
//    else if (controllerElementObj is ControllerThumbStick)
//    {
//        if (controllerElementFunction == Function.MoveSprint)
//        {
//            _moveSprintThumbStick = (ControllerThumbStick)controllerElementObj;

//            if (_lookThumbStick == _moveSprintThumbStick)
//                _lookThumbStick = ControllerThumbStick.Unset;
//        }
//        else if (controllerElementFunction == Function.Look)
//        {
//            _lookThumbStick = (ControllerThumbStick)controllerElementObj;

//            if (_moveSprintThumbStick == _lookThumbStick)
//                _moveSprintThumbStick = ControllerThumbStick.Unset;
//        }
//        else
//        {
//            if ((ControllerThumbStick)controllerElementObj == _moveSprintThumbStick)
//                _moveSprintThumbStick = ControllerThumbStick.Unset;
//            else if ((ControllerThumbStick)controllerElementObj == _lookThumbStick)
//                _lookThumbStick = ControllerThumbStick.Unset;
//        }
//    }
//}

//private void GetControllerElement_SettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
//{
//    object controllerElementObj = Forms.Properties.GetControllerElement.GetControllerElementByPropertyNameNV(e.PropertyName);
//    Function controllerElementFunction = Forms.Properties.GetControllerElement.GetControllerElementFunctionByPropertyNameNV(e.PropertyName);

//    if (controllerElementObj is ControllerButton)
//    {
//        if (controllerElementFunction == Function.None)
//        {
//            if ((ControllerButton)controllerElementObj == _selectDeselectButton)
//                _selectDeselectButton = ControllerButton.Unset;
//            else if ((ControllerButton)controllerElementObj == _hideUnhideButton)
//                _hideUnhideButton = ControllerButton.Unset;
//        }
//        else if (controllerElementFunction == Function.SelectDeselect)
//        {
//            _selectDeselectButton = (ControllerButton)controllerElementObj;
//        }
//        else if (controllerElementFunction == Function.HideUnhide)
//        {
//            _hideUnhideButton = (ControllerButton)controllerElementObj;
//        }
//    }
//    else if (controllerElementObj is ControllerTrigger)
//    {
//        if (controllerElementFunction == Function.None)
//        {
//            if ((ControllerTrigger)controllerElementObj == _elevationUpTrigger)
//                _elevationUpTrigger = ControllerTrigger.Unset;
//            else if ((ControllerTrigger)controllerElementObj == _elevationDownTrigger)
//                _elevationDownTrigger = ControllerTrigger.Unset;
//        }
//        else if (controllerElementFunction == Function.ElevationUp)
//        {
//            _elevationUpTrigger = (ControllerTrigger)controllerElementObj;
//        }
//        else if (controllerElementFunction == Function.ElevationDown)
//        {
//            _elevationDownTrigger = (ControllerTrigger)controllerElementObj;
//        }
//    }
//    else if (controllerElementObj is ControllerThumbStick)
//    {
//        if (controllerElementFunction == Function.None)
//        {
//            if ((ControllerThumbStick)controllerElementObj == _moveSprintThumbStick)
//                _moveSprintThumbStick = ControllerThumbStick.Unset;
//            else if ((ControllerThumbStick)controllerElementObj == _lookThumbStick)
//                _lookThumbStick = ControllerThumbStick.Unset;
//        }
//        else if (controllerElementFunction == Function.MoveSprint)
//        {
//            _moveSprintThumbStick = (ControllerThumbStick)controllerElementObj;
//        }
//        else if (controllerElementFunction == Function.Look)
//        {
//            _lookThumbStick = (ControllerThumbStick)controllerElementObj;
//        }
//    }
//}
#endregion