# Architecture
_Last updated: 2026-05-01_

## Summary

CtrlWiz is a dual-target Autodesk plugin solution (Navisworks + Revit) that maps Xbox gamepad input to 3D viewport navigation and model interaction commands. The solution uses a layered plugin architecture where shared libraries (logging, licensing, forms) are consumed by two separate host-specific plugin projects. The NW plugin uses a richer MVVM UI layer backed by WPF/XAML; the RVT plugin uses simpler WinForms dialogs and the Revit IExternalApplication/IExternalCommand pattern.

---

## Overall Architecture Pattern

**Layered plugin architecture** with two independent plugin targets (NW, RVT) sharing a common infrastructure tier:

```
┌──────────────────────────────────────────────────────────────────┐
│                        Plugin Hosts                              │
│   CtrlWizNW (Navisworks plugin)   CtrlWizRVT (Revit plugin)     │
├──────────────────────────────────────────────────────────────────┤
│                    Host-Specific UI                              │
│   CtrlWizNW.UI (WPF/MVVM)         CtrlWizForms (WinForms/MVP)   │
├──────────────────────────────────────────────────────────────────┤
│                    Shared Infrastructure                         │
│   CtrlWizLicense   CtrlWiz.Logging   CtrlWizForms.CustomControls│
├──────────────────────────────────────────────────────────────────┤
│                    Hardware Abstraction                          │
│                    XInputDotNetPure (Xbox gamepad P/Invoke)      │
└──────────────────────────────────────────────────────────────────┘
```

---

## Navisworks Plugin (CtrlWizNW)

**Entry point:** `CtrlWizNW\CtrlWiz.NW\CmdViewpoint.cs` — `CmdViewpoint : CommandHandlerPlugin, IDisposable`

The Navisworks plugin uses the Autodesk Navisworks Plugin API (`Autodesk.Navisworks.Api.Plugins`). Plugin registration is entirely via C# attributes on `CmdViewpoint`:

```csharp
[Plugin("NVSXBOX.CmdViewpoint", "VIATechnik", DisplayName = "CtrlWiz")]
[RibbonLayout("CustomRibbon.xaml")]
[RibbonTab("ID_CustomTab_1", DisplayName = "CtrlWiz")]
[Command("ID_Button_1", CanToggle = true, DisplayName = " Activate\n Controller ", ...)]
```

The ribbon layout is defined in `CtrlWizNW\CustomRibbon\CustomRibbon.xaml` (a WPF XAML resource compiled into the assembly). Plugin discovery is declared in `CtrlWizNW\CustomRibbon\PackageContents.xml`, which is the Autodesk Application Package manifest. The bundle is deployed to `%ProgramData%\Autodesk\ApplicationPlugins\CtrlWiz.bundle\Contents\v{NW-version}\`.

**Version targeting:** Each Navisworks version (NW17–NW25) is a separate MSBuild configuration (`NW17|x64` through `NW25|x64`). The configuration sets `$(AutodeskNavisworksApiPath)` to the correct installation directory and defines a compile-time constant (`NW2017`, `NW2022`, `NW2025`, etc.) for version-conditional code. Output goes to `CtrlWiz.Release.Assemblies\Navisworks\$(Configuration)\`.

**Controller loop pattern (NW):** `CmdViewpoint` maintains the main gamepad polling loop. On activation it calls `SetControllerButtonsLayout()`, `SetControllerTriggersLayout()`, and `SetControllerThumbSticksLayout()` to read the user's button mapping from `CtrlWizNW.UI`'s `Settings.Default`, mapping each `ControllerButton`/`ControllerTrigger`/`ControllerThumbStick` enum to a `Function` enum value. The polling loop then dispatches to Navisworks viewpoint navigation and model interaction APIs based on gamepad state deltas.

**ToolPlugin overlays:** `CmdTargetEnable` and `CmdTargetDisable` in `CtrlWizNW\CtrlWiz.NW\CmdTool.cs` are `ToolPlugin` subclasses that draw a crosshair overlay via the Navisworks `Graphics` API.

---

## Revit Plugin (CtrlWizRVT)

**Entry point:** `CtrlWizRVT\CtrlWiz.RVT\ExAppCtrlWizRVT.cs` — `ExAppCtrlWizRVT : IExternalApplication`

The Revit plugin uses the standard `IExternalApplication` / `IExternalCommand` pattern. `ExAppCtrlWizRVT.OnStartup()` builds the ribbon tab and panel programmatically using `UIControlledApplication`. Each ribbon button maps to a separate `IExternalCommand` class:

| Button | Class | File |
|--------|-------|------|
| Activate Controller | `CmdActivateController` | `CtrlWizRVT\CtrlWiz.RVT\CmdActivateController.cs` |
| Controller Map (Help) | `CmdHelp` | `CtrlWizRVT\CtrlWiz.RVT\CmdHelp.cs` |
| Feature Request | `CmdFeatureRequest` | `CtrlWizRVT\CtrlWiz.RVT\CmdFeatureRequest.cs` |
| Speed Settings | `CmdSettings` | `CtrlWizRVT\CtrlWiz.RVT\CmdSettings.cs` |
| License | `CmdLicense` | `CtrlWizRVT\CtrlWiz.RVT\CmdLicense.cs` |

Plugin discovery is via `.addin` manifest files in `CtrlWizRVT\Manifest\{year}\CtrlWiz.RVT.addin` (one per supported Revit year: 2019–2022). The manifest registers `ExAppCtrlWizRVT` as an `Application` type add-in.

**Controller loop pattern (RVT):** `CmdActivateController.Execute()` initiates an async loop (`InitializeController` → `Update`) using `async/await Task.Delay(10)`. The loop reads `GamePad.GetState(PlayerIndex.One)` each tick, moves/rotates the active `View3D` via `ViewOrientation3D.SetOrientation()`, and calls `UIDocument.RefreshActiveView()`. Speed is managed through `SpeedSettings.LinearSpeed` and `SpeedSettings.AngularSpeed` static state in `CtrlWizRVT\States\SpeedSettings.cs`. The `Back` button exits the loop.

**Version targeting (RVT):** Separate MSBuild configurations (`RVT19|AnyCPU` through `RVT22|AnyCPU`) set `$(RevitAPIPath)` and `$(RevitAPIUIPath)`, and define compile-time constants (`RVT19`, `RVT20`, etc.). Output goes to `CtrlWiz.Release.Assemblies\Revit\$(Configuration)\`.

---

## Shared UI Layer — CtrlWizForms (WinForms / MVP)

Both plugins share a WinForms-based forms library that follows the **Model-View-Presenter (MVP)** pattern.

- **Views** defined by interfaces: `IHelpForm` in `CtrlWizForms\Views\IHelpForm.cs`, `ISpeedSettingForm` in `CtrlWizForms\Views\ISpeedSettingForm.cs`
- **Concrete views**: `HelpFormNW`, `HelpFormRVT`, `SpeedSettingForm` — all in `CtrlWizForms\Views\`
- **Presenters**: `HelpPresenterNW`, `HelpPresenterRVT`, `SpeedSettingPresenter` — all in `CtrlWizForms\Presenters\`
- **Models**: `SpeedSettingModel` behind `ISpeedSettingModel` in `CtrlWizForms\Models\`
- **Message services**: `IMessageService` / `MessageService` in `CtrlWizForms\MessageServices\` abstract dialog display

The `SpeedSettingPresenter` wires `LinearTrackBarValueChanged` and `AngularTrackBarValueChanged` events from the view, and exposes `LinearSetting` / `AngularSetting` properties for the caller to read after `ShowDialog()` returns.

---

## NW-Specific UI Layer — CtrlWizNW.UI (WPF / MVVM)

`CtrlWizNW.UI` is a WPF class library targeting .NET 4.8 with **MVVM via MvvmLight** (`GalaSoft.MvvmLight 5.4.1`).

- **Models** (`CtrlWizNW.UI\Models\`): `ControllerButton`, `ControllerTrigger`, `ControllerThumbStick` enums; `ControllerOption`, `ControllerButtonOption`, `ControllerTriggerOption`, `ControllerThumbStickOption` classes; `Function` enum enumerating all mappable Navisworks actions
- **ViewModel** (`CtrlWizNW.UI\ViewModels\HelpViewModel.cs`): implements `INotifyPropertyChanged`; exposes `ObservableCollection<ControllerButtonOption>` etc. for data binding; persists user selections to `Settings.Default` (per-user roaming settings); uses `RelayCommand` for the Restore Default command
- **View** (`CtrlWizNW.UI\Views\HelpView.xaml` + `.xaml.cs`): WPF UserControl bound to `HelpViewModel`
- **Settings bridge** (`CtrlWizNW.UI\Properties\GetControllerElement.cs`): translates between `Settings.Default` property names (e.g., `NV_AButton`) and strongly-typed `ControllerButton`/`ControllerTrigger`/`ControllerThumbStick` enum values; raises `SettingsPropertyChanged` so `CmdViewpoint` can reload button mappings at runtime

---

## Hardware Abstraction — XInputDotNetPure

`XInputDotNetPure` is a pure-C# P/Invoke wrapper over `xinput1_3.dll`. It exposes:

- `GamePad.GetState(PlayerIndex)` → `GamePadState` (buttons, thumbsticks, triggers, dpad) in `XInputDotNetPure\XInputDotNetPure\GamePad.cs`
- `GamePad.SetVibration(PlayerIndex, leftMotor, rightMotor)`

Both `CtrlWizNW` and `CtrlWizRVT` reference this project directly.

---

## Licensing — CtrlWizLicense

`CtrlWizLicense` wraps the **Paddle SDK** (`PaddleSDK`) for in-app checkout and license verification.

- `InAppCheckout` in `CtrlWizLicense\InAppCheckout.cs`: initializes `Paddle.Configure(apiKey, vendorId, productId, ...)`, registers transaction events, exposes `StartPaddle(out string message)` / `IsProductActivated`
- `ProductVersion` enum in `CtrlWizLicense\Configs\ProductVersion.cs`: `NW = 0`, `RVT = 1` — selects the correct Paddle product ID
- `ActivationChangedArgs` / `ActivationChanged` event: emitted when activation state changes; wired in both `ExAppCtrlWizRVT.OnStartup()` and `CmdViewpoint`
- Resources (`CtrlWizLicense\Properties\Resources.resx`): stores `ApiKey`, `VendorId`, `ProductIdNW`, `ProductIdRVT` as embedded resources

---

## Logging — CtrlWiz.Logging

`CtrlWiz.Logging\Logger.cs` is a static file-based logger. `Logger.SetUpLogger(LoggedApp loggedApp)` sets the log file path in `%TEMP%` (`CtrlWizNW.log` or `CtrlWizRVT.log`). `Exception.LogException()` is an extension method that appends timestamped exception messages. The `LoggedApp` enum in `CtrlWiz.Logging\Utility\LoggedApp.cs` identifies the calling plugin host.

---

## Dependency Flow Between Projects

Arrows indicate "depends on":

```
CtrlWizNW ──► CtrlWiz.Logging
CtrlWizNW ──► CtrlWizForms
CtrlWizNW ──► CtrlWizLicense
CtrlWizNW ──► CtrlWizNW.UI
CtrlWizNW ──► XInputDotNetPure

CtrlWizRVT ──► CtrlWiz.Logging
CtrlWizRVT ──► CtrlWizForms
CtrlWizRVT ──► CtrlWizLicense
CtrlWizRVT ──► XInputDotNetPure
(CtrlWizRVT does NOT reference CtrlWizNW.UI)

CtrlWizLicense ──► CtrlWiz.Logging

CtrlWizNWSetup ──► CtrlWizInstallCustomAction
CtrlWizNWSetup ──► CtrlWizDirectoryPermissionCustomAction
```

No circular dependencies exist. Shared libraries (`CtrlWiz.Logging`, `CtrlWizLicense`, `CtrlWizForms`) have no references back to plugin projects.

---

## Error Handling Strategy

- **NW plugin**: `try/catch` blocks in command handlers; exceptions logged via `Logger.LogException()` extension method; user-visible errors via `MessageService.ShowError()`
- **RVT plugin**: `try/catch` in `OnStartup`, `Execute`, and the `Update` async loop; `TaskDialog.Show("Error", e.Message)` for user-visible errors; exceptions re-thrown from the async loop to surface in Revit's error handler
- **Licensing**: `InAppCheckout.StartPaddle(out string message)` returns error messages via out-parameter rather than throwing; callers check and show via `MessageService`

---

## Key Design Patterns

| Pattern | Where Applied |
|---------|---------------|
| Plugin via attributes | `CmdViewpoint` (NW) — `[Plugin]`, `[Command]`, `[RibbonLayout]` attributes |
| IExternalApplication / IExternalCommand | `ExAppCtrlWizRVT`, `CmdActivateController`, all RVT `Cmd*` classes |
| MVVM (MvvmLight) | `CtrlWizNW.UI` — `HelpViewModel`, `HelpView.xaml`, `RelayCommand` |
| MVP (Model-View-Presenter) | `CtrlWizForms` — `SpeedSettingPresenter`, `ISpeedSettingForm`, `SpeedSettingModel` |
| Singleton (thread-safe lazy) | `NavisUtils : Singleton<NavisUtils>` via `CtrlWizNW\Singleton.cs` |
| Event-driven settings | `GetControllerElement.SettingsPropertyChanged`; `InAppCheckout.ActivationChanged` |
| Async polling loop | `CmdActivateController.Update()` (RVT) — `async Task` with `await Task.Delay(10)` |

---

*Architecture analysis: 2026-05-01*
