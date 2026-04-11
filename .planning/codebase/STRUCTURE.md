# CtrlWiz Repository Structure

Root: ctrlwiz2025/
│
├── CtrlWiz.sln                        Visual Studio 2019 solution (all projects)
├── LICENSE                            GNU GPL v3.0
├── README.md                          Project overview, screenshots, controller maps
├── UpgradeLog.htm                     VS project-upgrade log (auto-generated)
│
├── CtrlWizNW/                         *** Navisworks plug-in ***
│   ├── CtrlWizNW.csproj               Target: .NET Framework, AnyCPU
│   ├── app.config
│   ├── Singleton.cs                   Generic thread-safe lazy singleton base class
│   ├── VIATechnik_CS_cert.pfx         Code-signing certificate
│   ├── CtrlWiz.NW/                    Source namespace CtrlWiz.NW
│   │   ├── CmdViewpoint.cs            Main CommandHandlerPlugin; controller loop,
│   │   │                              ribbon commands (ID_Button_1-5), viewpoint math
│   │   ├── CmdTool.cs                 ToolPlugin (CmdTargetEnable/Disable crosshair overlay)
│   │   ├── CmdInput.cs                InputPlugin stub (commented out)
│   │   └── NavisUtils.cs              Singleton math helpers: quaternion multiply,
│   │                                  view-direction extraction, vector lerp, angle
│   ├── CustomRibbon/
│   │   ├── CustomRibbon.xaml          Navisworks ribbon layout definition
│   │   ├── CustomRibbon.name          Ribbon name resource
│   │   └── PackageContents.xml        NW AppStore manifest (Nw14-Nw19 / 2017-2022)
│   ├── Images/                        Icon + image assets for ribbon buttons
│   └── Properties/                    AssemblyInfo, app settings
│
├── CtrlWizNW.UI/                      *** NW settings/help WPF window (MVVM) ***
│   ├── CtrlWizNW.UI.csproj
│   ├── Models/
│   │   ├── Function.cs                Enum of all mappable functions (21 values)
│   │   ├── ControllerButton.cs        Enum: A B X Y RBumber Start None Unset
│   │   ├── ControllerButtonOption.cs  Option model for a button drop-down entry
│   │   ├── ControllerTrigger.cs       Enum: LT RT None Unset
│   │   ├── ControllerTriggerOption.cs Option model for a trigger drop-down entry
│   │   ├── ControllerThumbStick.cs    Enum: L R None Unset
│   │   ├── ControllerThumbStickOption.cs
│   │   └── ControllerOption.cs        Abstract base for all option models
│   ├── ViewModels/
│   │   └── HelpViewModel.cs           INotifyPropertyChanged VM; manages per-element
│   │                                  option lists, selection state, Settings persistence,
│   │                                  drone-controls coupling, RestoreDefault command
│   └── Views/
│       ├── HelpView.xaml              WPF window: combo-boxes for each controller element
│       └── HelpView.xaml.cs           Code-behind
│
├── CtrlWizRVT/                        *** Revit plug-in ***
│   ├── CtrlWizRVT.csproj              Target: .NET Framework, AnyCPU
│   ├── app.config
│   ├── VIATechnik_CS_cert.pfx
│   ├── CtrlWiz.RVT/                   Source namespace CtrlWiz.RVT
│   │   ├── ExAppCtrlWizRVT.cs         IExternalApplication: OnStartup ribbon builder,
│   │   │                              license init, logger setup
│   │   ├── CmdActivateController.cs   IExternalCommand: async controller poll loop,
│   │   │                              camera/view3D mutation, vibration feedback
│   │   ├── CmdSettings.cs             IExternalCommand: opens SpeedSettingPresenter dialog
│   │   ├── CmdHelp.cs                 IExternalCommand: opens HelpFormRVT
│   │   ├── CmdFeatureRequest.cs       IExternalCommand: opens feature-request URL
│   │   └── CmdLicense.cs              IExternalCommand: opens license management window
│   ├── Dialogs/
│   │   ├── PerspectiveViewNotActiveDialogBox.cs   WinForms dialog shown when active view
│   │   │                                          is not a 3-D perspective view
│   │   ├── PerspectiveViewNotActiveDialogBox.Designer.cs
│   │   └── PerspectiveViewNotActiveDialogBox.resx
│   ├── States/
│   │   └── SpeedSettings.cs           Static class holding LinearSpeed / AngularSpeed
│   │                                  in-memory state (not persisted between sessions)
│   ├── Utility/
│   │   └── ExtensionMethods.cs        RVT-specific extension helpers
│   ├── Resources/                     Embedded icons (ActivateController, Help, Settings,
│   │                                  License, Idea) + string resources for ribbon labels
│   ├── PackageContents/
│   │   └── PackageContents.xml        Revit AppStore manifest (R2018-R2022)
│   └── Manifest/                      Per-year .addin manifest files (2019-2022)
│
├── CtrlWizForms/                      *** Shared WinForms UI (MVP) ***
│   ├── CtrlWizForms.csproj
│   ├── Views/
│   │   ├── IHelpForm.cs               Interface: ShowDialog
│   │   ├── HelpFormNW.cs/.Designer/.resx    Help window (NW variant)
│   │   ├── HelpFormRVT.cs/.Designer/.resx   Help window (RVT variant)
│   │   ├── ISpeedSettingForm.cs       Interface: LinearTrackBarValueChanged,
│   │   │                              AngularTrackBarValueChanged, ShowDialog
│   │   ├── SpeedSettingForm.cs/.Designer/.resx   Speed-setting sliders dialog
│   │   ├── SettingEventArgs.cs        EventArgs carrying int SettingValue
│   │   └── DialogWithLink.resx        Resx for hyperlink-capable dialog
│   ├── Presenters/
│   │   ├── SpeedSettingPresenter.cs   Mediates SpeedSettingForm ↔ SpeedSettingModel;
│   │   │                              exposes LinearSetting / AngularSetting after close
│   │   ├── HelpPresenterNW.cs         Presenter for NW help form
│   │   └── HelpPresenterRVT.cs        Presenter for RVT help form
│   ├── Models/
│   │   ├── ISpeedSettingModel.cs
│   │   └── SpeedSettingModel.cs
│   ├── MessageServices/
│   │   ├── IMessageService.cs         Interface: ShowError / ShowInfo
│   │   └── MessageService.cs          Implementation wrapping MessageBox.Show
│   └── Images/                        Shared image assets
│
├── CtrlWizForms.CustomControls/       *** Shared WinForms custom controls ***
│   ├── CtrlWizForms.CustomControls.csproj
│   ├── RoundedCornersButton.cs        Button with rounded-corner paint override
│   └── Utility/
│       └── ExtensionMethods.cs        Control-layer helpers
│
├── CtrlWizLicense/                    *** License management (Paddle SDK) ***
│   ├── CtrlWizLicense.csproj
│   ├── InAppCheckout.cs               Core class: Paddle.Configure, Product.Refresh,
│   │                                  ShowCheckoutWindow, VerifyActivation, offline
│   │                                  binary cache in %TEMP%
│   ├── CtrlWizPaddle.cs               IProductInfoWindow stub (unused/placeholder)
│   ├── InAppCheckoutArgs.cs           Event args for checkout flow
│   ├── CheckoutCompletedArgs.cs
│   ├── ActivationChangedArgs.cs
│   ├── VerifyActivationCompletedArgs.cs
│   ├── InAppCheckoutStatus.cs         Enum: Default / Pending
│   ├── Configs/
│   │   └── ProductVersion.cs          Enum: NW / RVT
│   └── license.rtf                    EULA text shown in installer
│
├── CtrlWiz.LicenseAgreement/          *** License agreement displayed during install ***
│   └── (bin/obj artefacts only — no tracked source files)
│
├── CtrlWiz.Logging/                   *** Shared logger ***
│   ├── CtrlWiz.Logging.csproj
│   ├── Logger.cs                      Static class: SetUpLogger(LoggedApp),
│   │                                  LogException(Exception) extension method,
│   │                                  appends to %TEMP%/CtrlWizNW.log or CtrlWizRVT.log
│   └── Utility/
│       ├── ExtensionMethods.cs        GetExceptionInfo helper for stack-trace formatting
│       └── LoggedApp.cs               Enum: NW / RVT
│
├── XInputDotNetPure/                  *** Pure-managed XInput wrapper (vendored) ***
│   ├── XInputDotNetPure.csproj
│   └── XInputDotNetPure/
│       ├── GamePad.cs                 Static: GetState(PlayerIndex), SetVibration(...)
│       ├── GamePadState.cs            Struct returned by GetState
│       ├── GamePadButtons.cs          Per-button ButtonState fields
│       ├── GamePadThumbSticks.cs      Left/Right StickState (X, Y floats)
│       ├── GamePadTriggers.cs         Left/Right float values
│       ├── GamePadDPad.cs             D-pad ButtonState fields
│       ├── ButtonState.cs             Enum: Pressed / Released
│       ├── GamePadDeadZone.cs         Enum: None / IndependentAxes / Circular
│       ├── PlayerIndex.cs             Enum: One / Two / Three / Four
│       ├── Imports.cs                 P/Invoke into xinput1_3.dll
│       └── Utils.cs                   Dead-zone application math
│
├── CtrlWizNWSetup/                    *** NW installer (Visual Studio .vdproj) ***
│   └── CtrlWizNWSetup.vdproj
│
├── CtrlWizRVTSetup/                   *** RVT installer (Visual Studio .vdproj) ***
│   └── CtrlWizRVTSetup.vdproj
│
├── SetupNwXboxWix/                    *** NW installer (WiX MSI) ***
│   ├── SetupNwXboxWix.wixproj
│   └── Product.wxs                    WiX product definition
│
├── CtrlWizInstallCustomAction/        *** MSI custom action DLL ***
│   └── InstallCustomAction.cs         Runs at install time (e.g. registry writes)
│
├── CtrlWizDirectoryPermissionCustomAction/   *** MSI custom action DLL ***
│   └── DirectoryPermissionCustomAction.cs    Sets ACLs on install directory
│
├── FullPermissionTest/                *** Dev/test project for ACL testing ***
│   └── (test harness for directory permission custom action)
│
└── Views/                             *** Loose XAML view files (root level) ***
    ├── HelpView.xaml                  Duplicate / working copy of NW help view
    └── HelpView.xaml.cs
