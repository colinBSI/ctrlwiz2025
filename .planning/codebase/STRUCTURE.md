# Codebase Structure
_Last updated: 2026-05-01_

## Summary

CtrlWiz is a 12-project .NET Framework 4.8 Visual Studio solution containing two Autodesk plugin targets (Navisworks and Revit), shared UI and infrastructure libraries, and installer projects. Projects are grouped into plugin layers, shared libraries, and deployment tooling. All builds target x64 Windows; release output collects to `CtrlWiz.Release.Assemblies\`.

---

## Directory Layout

```
ctrlwiz2025/
├── CtrlWiz.sln                             Solution file (VS 2019, format 12.00)
├── LICENSE                                 GNU GPL v3.0
├── README.md
│
├── CtrlWizNW/                              Navisworks plugin (main)
├── CtrlWizNW.UI/                           WPF/MVVM UI layer for NW plugin
├── CtrlWizRVT/                             Revit plugin
│
├── CtrlWizForms/                           Shared WinForms UI (MVP pattern)
├── CtrlWizForms.CustomControls/            Shared WinForms custom controls
│
├── CtrlWizLicense/                         License management (Paddle SDK)
├── CtrlWiz.Logging/                        Shared file-based logger
├── XInputDotNetPure/                       Xbox gamepad P/Invoke wrapper
│
├── CtrlWizNWSetup/                         NW MSI installer (.vdproj)
├── CtrlWizRVTSetup/                        RVT MSI installer (.vdproj)
├── SetupNwXboxWix/                         NW installer (WiX alternative)
├── CtrlWizInstallCustomAction/             MSI custom action: install-time checks
├── CtrlWizDirectoryPermissionCustomAction/ MSI custom action: directory ACLs
│
├── FullPermissionTest/                     Dev test harness for ACL logic
├── Views/                                  Loose root-level XAML (working copies)
│
├── CtrlWiz.Release.Assemblies/             Shared release output directory
│   ├── Navisworks\NW17\ ... NW25\          Per-version NW release binaries
│   └── Revit\RVT19\ ... RVT22\            Per-version RVT release binaries
│
└── packages/                               NuGet package restore cache
```

---

## Project-by-Project Breakdown

### CtrlWizNW — Navisworks Plugin
- **Purpose:** Main Navisworks plugin assembly. Registers the CtrlWiz ribbon tab, handles all Navisworks API calls, runs the gamepad polling loop, and mutates the Navisworks document viewpoint.
- **Output type:** Class library (`CtrlWizNW.dll`)
- **Namespace:** `CtrlWiz.NW`
- **Target framework:** .NET 4.8, x64
- **Key files:**
  - `CtrlWizNW\CtrlWiz.NW\CmdViewpoint.cs` — plugin entry point; `CommandHandlerPlugin` subclass; defines all ribbon commands via attributes; owns the controller polling loop and viewpoint math
  - `CtrlWizNW\CtrlWiz.NW\CmdTool.cs` — `CmdTargetEnable` / `CmdTargetDisable` (`ToolPlugin`), overlay crosshair rendering
  - `CtrlWizNW\CtrlWiz.NW\NavisUtils.cs` — `Singleton<NavisUtils>` with quaternion multiply, view-direction extraction, vector lerp, angle calculation
  - `CtrlWizNW\Singleton.cs` — generic thread-safe lazy singleton base class (used by `NavisUtils`)
  - `CtrlWizNW\CustomRibbon\PackageContents.xml` — Autodesk Application Package manifest; declares the bundle, supported NW versions, and DLL module path
  - `CtrlWizNW\CustomRibbon\CustomRibbon.xaml` — WPF XAML ribbon layout compiled as a `Page` resource
- **Build configurations:** `NW17|x64` through `NW25|x64`; each sets `$(AutodeskNavisworksApiPath)` and a `NW20xx` compile constant
- **Debug output:** `%ProgramData%\Autodesk\ApplicationPlugins\CtrlWiz.bundle\Contents\v18\`
- **Release output:** `CtrlWiz.Release.Assemblies\Navisworks\$(Configuration)\`
- **Project references:** `CtrlWiz.Logging`, `CtrlWizForms`, `CtrlWizLicense`, `CtrlWizNW.UI`, `XInputDotNetPure`

---

### CtrlWizNW.UI — WPF/MVVM UI Layer for NW
- **Purpose:** WPF class library providing the controller-mapping settings window (Help/Map view) for the NW plugin. Contains all MVVM models, the `HelpViewModel`, and the `GetControllerElement` settings bridge consumed by `CmdViewpoint`.
- **Output type:** Class library (`CtrlWizNW.UI.dll`)
- **Namespace:** `CtrlWiz.NW.UI`
- **Target framework:** .NET 4.8 (WPF project type GUID `{60dc8134-...}`)
- **Key directories:**
  - `CtrlWizNW.UI\Models\` — `Function` enum, `ControllerButton/Trigger/ThumbStick` enums, `ControllerOption` base class and `*Option` concrete subclasses
  - `CtrlWizNW.UI\ViewModels\HelpViewModel.cs` — full MVVM ViewModel; persists button-to-function mappings to `Settings.Default`; implements drone-controls coupling logic
  - `CtrlWizNW.UI\Views\HelpView.xaml` — WPF window with combo-boxes for each controller element
  - `CtrlWizNW.UI\Properties\GetControllerElement.cs` — static bridge between `Settings.Default` string keys and typed controller element enums; raises `SettingsPropertyChanged` event
  - `CtrlWizNW.UI\Properties\Settings.settings` — per-user roaming settings storing `NV_AButton`, `NV_BButton`, `NV_XButton`, `NV_YButton`, `NV_RBumber`, `NV_StartButton`, `NV_LTrigger`, `NV_RTrigger`, `NV_LStick`, `NV_RStick`
- **NuGet dependency:** `MvvmLightLibs 5.4.1` (GalaSoft.MvvmLight + Extras + Platform, System.Windows.Interactivity)

---

### CtrlWizRVT — Revit Plugin
- **Purpose:** Main Revit plugin assembly. Implements `IExternalApplication` for startup/ribbon setup, and separate `IExternalCommand` classes for each ribbon button. Runs an async gamepad loop that mutates `View3D` orientation directly via the Revit API.
- **Output type:** Class library (`CtrlWizRVT.dll`)
- **Namespace:** `CtrlWiz.RVT`
- **Target framework:** .NET 4.8, AnyCPU
- **Key files:**
  - `CtrlWizRVT\CtrlWiz.RVT\ExAppCtrlWizRVT.cs` — `IExternalApplication` entry point; builds ribbon; initializes `InAppCheckout`; calls `Logger.SetUpLogger(LoggedApp.RVT)`
  - `CtrlWizRVT\CtrlWiz.RVT\CmdActivateController.cs` — `IExternalCommand`; async `Update()` loop; reads `GamePad.GetState()`, calls `MoveCamera()` / `RotateCamera()` using `ViewOrientation3D`
  - `CtrlWizRVT\CtrlWiz.RVT\CmdSettings.cs` — opens `SpeedSettingPresenter` dialog; writes result to `SpeedSettings` static state
  - `CtrlWizRVT\States\SpeedSettings.cs` — static `LinearSpeed` / `AngularSpeed` in-memory settings (reset each Revit session)
  - `CtrlWizRVT\Utility\ExtensionMethods.cs` — RVT-specific helpers (e.g., `ConvertToVector3()` for Revit `XYZ`, `RotateByAxis()`)
  - `CtrlWizRVT\Dialogs\PerspectiveViewNotActiveDialogBox.cs` — WinForms dialog shown when controller input received but active view is not a 3D perspective view
  - `CtrlWizRVT\Manifest\{year}\CtrlWiz.RVT.addin` — Revit add-in manifest (2019–2022); registers `ExAppCtrlWizRVT` as `Application` type
- **Build configurations:** `RVT19|AnyCPU` through `RVT22|AnyCPU`; each sets `$(RevitAPIPath)` and a `RVTxx` compile constant
- **Release output:** `CtrlWiz.Release.Assemblies\Revit\$(Configuration)\`
- **Project references:** `CtrlWiz.Logging`, `CtrlWizForms`, `CtrlWizLicense`, `XInputDotNetPure`

---

### CtrlWizForms — Shared WinForms UI (MVP)
- **Purpose:** WinForms class library shared by both NW and RVT plugins. Provides the Speed Settings dialog, Help/Controller-map dialogs (separate NW and RVT variants), and message service abstraction.
- **Output type:** Class library
- **Namespace:** `CtrlWiz.Forms`
- **Key directories:**
  - `CtrlWizForms\Views\` — `IHelpForm`, `ISpeedSettingForm` interfaces; `HelpFormNW`, `HelpFormRVT`, `SpeedSettingForm` concrete WinForms classes; `SettingEventArgs`
  - `CtrlWizForms\Presenters\` — `SpeedSettingPresenter`, `HelpPresenterNW`, `HelpPresenterRVT`
  - `CtrlWizForms\Models\` — `ISpeedSettingModel`, `SpeedSettingModel`
  - `CtrlWizForms\MessageServices\` — `IMessageService`, `MessageService` (wraps `MessageBox.Show`)

---

### CtrlWizForms.CustomControls — Shared WinForms Controls
- **Purpose:** Reusable WinForms control library.
- **Key files:**
  - `CtrlWizForms.CustomControls\RoundedCornersButton.cs` — `Button` subclass with rounded-corner paint override
  - `CtrlWizForms.CustomControls\Utility\ExtensionMethods.cs` — control-layer helpers

---

### CtrlWizLicense — License Management
- **Purpose:** Encapsulates Paddle SDK integration for in-app license checkout and activation verification. Used by both plugin hosts.
- **Key files:**
  - `CtrlWizLicense\InAppCheckout.cs` — main class; `Paddle.Configure()` initialization; `StartPaddle(out string message)` runs verification; `IsProductActivated` property; `ActivationChanged` event
  - `CtrlWizLicense\Configs\ProductVersion.cs` — `enum ProductVersion { NW = 0, RVT = 1 }` selects the correct Paddle product ID
  - `CtrlWizLicense\Properties\Resources.resx` — embeds `ApiKey`, `VendorId`, `ProductIdNW`, `ProductIdRVT`, `TempNW`, `TempRVT` (activation cache file names)
- **NuGet dependency:** Paddle SDK (referenced via `packages\`)

---

### CtrlWiz.Logging — Shared Logger
- **Purpose:** Static file-based exception logger used by both plugins.
- **Key files:**
  - `CtrlWiz.Logging\Logger.cs` — `SetUpLogger(LoggedApp)` sets path; `Exception.LogException()` extension method appends timestamped entries to `%TEMP%\CtrlWizNW.log` or `%TEMP%\..\CtrlWizRVT.log`
  - `CtrlWiz.Logging\Utility\LoggedApp.cs` — `enum LoggedApp { NW, RVT }`
  - `CtrlWiz.Logging\Utility\ExtensionMethods.cs` — `GetExceptionInfo()` for formatted stack traces

---

### XInputDotNetPure — Xbox Gamepad Wrapper
- **Purpose:** Pure-managed C# P/Invoke wrapper over `xinput1_3.dll`. Vendored into the solution (not a NuGet package).
- **Key files:**
  - `XInputDotNetPure\XInputDotNetPure\GamePad.cs` — `GamePad.GetState(PlayerIndex)` and `SetVibration()`
  - `XInputDotNetPure\XInputDotNetPure\Imports.cs` — `DllImport` declarations for `xinput1_3.dll`
  - `XInputDotNetPure\XInputDotNetPure\GamePadState.cs` — state struct aggregating buttons, thumbsticks, triggers, dpad
  - `XInputDotNetPure\XInputDotNetPure\Utils.cs` — dead-zone math

---

### CtrlWizNWSetup / CtrlWizRVTSetup — VS Installer Projects
- **Purpose:** Visual Studio Deployment Project (`.vdproj`) MSI installers for NW and RVT respectively.
- **Relation to plugin projects:** Reference the built output DLLs from `CtrlWiz.Release.Assemblies\`; depend on `CtrlWizInstallCustomAction` and `CtrlWizDirectoryPermissionCustomAction` for custom installer behaviors.
- **Key files:**
  - `CtrlWizNWSetup\CtrlWizNWSetup.vdproj`
  - `CtrlWizRVTSetup\CtrlWizRVTSetup.vdproj`

---

### CtrlWizInstallCustomAction — MSI Custom Action
- **Purpose:** Runs during MSI installation to prompt the user to confirm administrative privileges are in place (required for Paddle licensing to function correctly).
- **Key file:** `CtrlWizInstallCustomAction\InstallCustomAction.cs` — `Installer` subclass; `OnBeforeInstall` shows a confirmation `MessageBox`; throws `InstallException` if cancelled

---

### CtrlWizDirectoryPermissionCustomAction — MSI Custom Action
- **Purpose:** Runs during MSI installation to create and grant `FullControl` ACLs to `%ProgramData%\CtrlWizLicense`, `%ProgramData%\System`, and `%ProgramData%\Roamer` so the Paddle license cache is writable by non-admin users at runtime.
- **Key file:** `CtrlWizDirectoryPermissionCustomAction\DirectoryPermissionCustomAction.cs` — `Installer` subclass; `OnBeforeInstall` calls `ProcessDirectory()` which creates missing directories and calls `GrantAccess()`

---

### FullPermissionTest — Developer Test Harness
- **Purpose:** Console application used during development to test the ACL-granting logic that `CtrlWizDirectoryPermissionCustomAction` will run in production. Not included in any installer.
- **Key file:** `FullPermissionTest\Program.cs`

---

## Key File Locations (Quick Reference)

| What | Path |
|------|------|
| NW plugin entry point | `CtrlWizNW\CtrlWiz.NW\CmdViewpoint.cs` |
| NW ribbon layout | `CtrlWizNW\CustomRibbon\CustomRibbon.xaml` |
| NW bundle manifest | `CtrlWizNW\CustomRibbon\PackageContents.xml` |
| RVT plugin entry point | `CtrlWizRVT\CtrlWiz.RVT\ExAppCtrlWizRVT.cs` |
| RVT controller loop | `CtrlWizRVT\CtrlWiz.RVT\CmdActivateController.cs` |
| RVT add-in manifests | `CtrlWizRVT\Manifest\{2019–2022}\CtrlWiz.RVT.addin` |
| Controller-to-function mapping UI | `CtrlWizNW.UI\ViewModels\HelpViewModel.cs` |
| Controller-to-function settings bridge | `CtrlWizNW.UI\Properties\GetControllerElement.cs` |
| User settings keys | `CtrlWizNW.UI\Properties\Settings.settings` |
| Speed settings (RVT in-memory) | `CtrlWizRVT\States\SpeedSettings.cs` |
| Speed settings dialog | `CtrlWizForms\Views\SpeedSettingForm.cs` |
| Licensing | `CtrlWizLicense\InAppCheckout.cs` |
| Logger | `CtrlWiz.Logging\Logger.cs` |
| Gamepad state | `XInputDotNetPure\XInputDotNetPure\GamePad.cs` |
| Singleton base | `CtrlWizNW\Singleton.cs` |

---

## Where to Add New Code

**New Navisworks command (ribbon button):**
- Add `[Command("ID_Button_N", ...)]` attribute to `CmdViewpoint` in `CtrlWizNW\CtrlWiz.NW\CmdViewpoint.cs`
- Handle it in `CmdViewpoint.ExecuteCommand(string name)`
- Add button definition to `CtrlWizNW\CustomRibbon\CustomRibbon.xaml`

**New mappable function (NW controller input):**
- Add value to `Function` enum in `CtrlWizNW.UI\Models\Function.cs`
- Add `ControllerButtonOption` / `ControllerTriggerOption` / `ControllerThumbStickOption` entry in `HelpViewModel.CreateEmptyControllerElementOptions()`
- Handle the new function in `CmdViewpoint`'s polling loop

**New RVT command:**
- Create a new `IExternalCommand` class in `CtrlWizRVT\CtrlWiz.RVT\`
- Register the ribbon button in `ExAppCtrlWizRVT.OnStartup()`

**New shared WinForms dialog:**
- Define an interface in `CtrlWizForms\Views\`
- Implement the WinForms form in `CtrlWizForms\Views\`
- Create a presenter in `CtrlWizForms\Presenters\`

**New reusable WinForms control:**
- Add to `CtrlWizForms.CustomControls\`

---

## Build Output Conventions

- **Debug (NW):** outputs directly to `%ProgramData%\Autodesk\ApplicationPlugins\CtrlWiz.bundle\Contents\v18\` for live testing in NW 2022
- **Release (NW):** `CtrlWiz.Release.Assemblies\Navisworks\NW17\` ... `NW25\`
- **Release (RVT):** `CtrlWiz.Release.Assemblies\Revit\RVT19\` ... `RVT22\`
- **Installers:** consume the release output directories; built separately via the Setup projects

---

*Structure analysis: 2026-05-01*
