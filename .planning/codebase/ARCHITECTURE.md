# CtrlWiz Architecture

## Overview

CtrlWiz is a pair of Autodesk plug-ins — one for Revit (CtrlWiz RVT) and one for Navisworks
Manage/Simulate (CtrlWiz NW) — that translate Xbox controller input into 3D navigation and
model-interaction commands inside each host application. The solution is a single Visual Studio
2019 `.sln` file that builds all assemblies in one pass.

---

## Runtime Boundary

Both plug-ins run entirely inside the host process (in-process DLL). Neither spawns a
separate server. Controller polling is implemented as a tight async Task loop driven by
`await Task.Delay(N)` ticks; there is no separate thread or background service.

---

## Layered Dependency Graph

    ┌──────────────────────────────────────────────────────┐
    │  Host processes                                       │
    │  Autodesk Revit (any year 2018-2022)                 │
    │  Autodesk Navisworks Manage / Simulate (Nw14-Nw19)   │
    └──────────┬───────────────────────┬───────────────────┘
               │                       │
    ┌──────────▼──────────┐ ┌──────────▼──────────────────┐
    │  CtrlWizRVT         │ │  CtrlWizNW                   │
    │  (IExternalApp +    │ │  (CommandHandlerPlugin +      │
    │   IExternalCommand) │ │   ToolPlugin / XAML ribbon)  │
    └──────────┬──────────┘ └──────────┬──────────────────┘
               │                       │
               │           ┌───────────▼──────────────────┐
               │           │  CtrlWizNW.UI                 │
               │           │  (WPF MVVM – button-mapping   │
               │           │   settings window)            │
               └─────┬─────┴──────────────────────────────┘
                     │
         ┌───────────┼────────────┐
         │           │            │
    ┌────▼────┐ ┌────▼────┐ ┌────▼─────────────────┐
    │CtrlWiz  │ │CtrlWiz  │ │CtrlWizForms           │
    │License  │ │Logging  │ │(WinForms MVP –        │
    │(Paddle) │ │         │ │ help + speed settings)│
    └────┬────┘ └─────────┘ └──────────────────────┘
         │
    ┌────▼────────────────────────────────┐
    │  CtrlWizForms.CustomControls        │
    │  (RoundedCornersButton)             │
    └─────────────────────────────────────┘

    Shared low-level library used by all layers:
    ┌─────────────────────────────────────┐
    │  XInputDotNetPure                   │
    │  (pure-managed XInput wrapper)      │
    └─────────────────────────────────────┘

---

## Key Architectural Decisions

### 1. Two parallel host integrations, maximal shared code

The Revit and Navisworks plug-ins share every concern except the host-API calls:
- License validation — CtrlWizLicense (Paddle SDK)
- Speed-settings dialog — CtrlWizForms (WinForms MVP)
- Log writing — CtrlWiz.Logging
- Controller state types — XInputDotNetPure

This means new features or bug fixes in shared libraries automatically apply to both hosts.

### 2. XInput via XInputDotNetPure

Rather than P/Invoking XInput directly, the solution vendors a pure-managed wrapper
(`XInputDotNetPure`) that exposes `GamePad.GetState(PlayerIndex)` and
`GamePad.SetVibration(PlayerIndex, l, r)`. This is the only mechanism used to read
button, trigger, and thumbstick state each frame.

### 3. Controller-loop patterns differ per host

- **Revit**: `CmdActivateController.InitializeController()` calls `await Update()`, a
  `while(isRun)` async Task that polls `GamePad.GetState` and calls Revit API methods
  directly under a `Transaction` opened once at startup. Revit's single-document/view
  model means view transforms are applied via `View3D.Unlock()` + direct camera mutation.

- **Navisworks**: `CmdViewpoint` (a `CommandHandlerPlugin`) manages a similar loop and
  mutates `Document.CurrentViewpoint` using the Navisworks API. The NW version also uses
  the `WindowsInput.InputSimulator` library to synthesize keyboard events for actions that
  have no direct API equivalent (e.g. text markup, tree navigation).

### 4. Button-mapping (NW only) — MVVM with persisted user settings

The NW plug-in ships a WPF window (`CtrlWizNW.UI`) that lets users re-assign which Xbox
button triggers which `Function` enum value. Assignments are persisted via the standard
.NET `Properties.Settings` (user-scoped) mechanism and loaded at controller-activation
time. The Revit plug-in uses fixed button assignments with no remapping UI.

### 5. License gate — Paddle SDK

`CtrlWizLicense.InAppCheckout` wraps the Paddle .NET SDK. Both plug-ins call
`InAppCheckout.StartPaddle()` before any command executes. The result is cached in a
binary temp file so the plug-in can function offline after the first successful activation.
Two separate Paddle product IDs exist — one for NW (`ProductVersion.NW`) and one for RVT
(`ProductVersion.RVT`).

### 6. MVP pattern in shared WinForms layer

`CtrlWizForms` follows classic Model-View-Presenter:
- `ISpeedSettingForm` / `IHelpForm` — view interfaces
- `SpeedSettingPresenter` / `HelpPresenterNW` / `HelpPresenterRVT` — presenters
- `SpeedSettingModel` / `ISpeedSettingModel` — models
- `MessageService` / `IMessageService` — thin abstraction over `MessageBox.Show`

### 7. Logging

`CtrlWiz.Logging.Logger` is a static class that appends timestamped exception lines to
a UTF-8 text file in the system temp directory (`CtrlWizNW.log` or `CtrlWizRVT.log`).
Exceptions are logged via the `LogException(this Exception)` extension method defined in
`CtrlWiz.Logging.Utility.ExtensionMethods`.

### 8. Installer variants

Three installer strategies coexist:
- **CtrlWizNWSetup / CtrlWizRVTSetup** — legacy Visual Studio Deployment Project (`.vdproj`)
- **SetupNwXboxWix** — WiX-based MSI (`Product.wxs`) for the NW flavour
- **PackageContents.xml** — Autodesk AppStore / in-product-manager manifest (one per host,
  versioned into year-specific component entries covering 2017-2022 / Revit 2018-2022)

---

## Data / Control Flow (per frame, NW example)

    Xbox controller hardware
          │  XInput (Win32)
          ▼
    XInputDotNetPure.GamePad.GetState(PlayerIndex.One)
          │  GamePadState struct
          ▼
    CmdViewpoint.Update() loop   ←── CtrlWizNW.UI.Properties.GetControllerElement
          │                           (reads persisted button-function mapping)
          │  reads thumbstick, trigger, button deltas
          ▼
    NavisUtils.getViewDir / MultiplyRotation3D / VLerp
          │  math helpers (quaternion × vector arithmetic)
          ▼
    Document.CurrentViewpoint.SetCurrentViewpoint(newViewpoint)
    — or —
    InputSimulator.Keyboard.KeyPress(VirtualKeyCode.XXX)   (keyboard-shortcut actions)
