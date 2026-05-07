# Integrations
_Last updated: 2026-05-01_

## Summary

CtrlWiz is a dual-target Autodesk plugin (Navisworks and Revit) that bridges Xbox gamepad input into 3D navigation. The core integration chain is: native XInput DLL → `XInputDotNetPure` managed wrapper → `CmdViewpoint.cs` / `CmdActivateController.cs` → Navisworks or Revit managed API → camera transforms. Licensing is enforced at command execution time via the Paddle SDK. Several legacy dependencies (WatiN, CredentialManagement) are present in `packages.config` and compiled in but contain no active call sites.

---

## Navisworks API

### Referenced Assemblies

All assemblies are resolved from the installed Navisworks Manage directory at build time via MSBuild properties. They are referenced as `Private=False` (copy-local disabled) so the plugin relies on the host application's copies at runtime.

| Assembly | Role |
|---|---|
| `Autodesk.Navisworks.Api.dll` | Primary managed API — `Application`, `Document`, `Viewpoint`, `ModelItem`, `SavedViewpoint`, `Tool` |
| `Autodesk.Navisworks.Controls.dll` | UI controls support |
| `Autodesk.Navisworks.Clash.dll` | Clash detection types (referenced but not actively used) |
| `Autodesk.Navisworks.ComApi.dll` | COM-based legacy API — `InwNvViewPoint2`, `nwECameraMode`, `nwEParadigm`, `InwOpState10` |
| `Autodesk.Navisworks.Interop.ComApi.dll` | Managed interop wrapper for COM API; referenced with `EmbedInteropTypes=True` |
| `AdWindows.dll` | Autodesk ribbon/UI infrastructure (`Autodesk.Windows.ComponentManager`) |
| `navisworks.gui.roamer.dll` | Internal Navisworks GUI commands — `LcRmFrameworkInterface`, `LcUCIPExecutionContext` |
| `navisworks.gui.interface.dll` | Internal Navisworks GUI interface types |

### Plugin Loading Mechanism

The plugin is loaded by Navisworks via the `PackageContents.xml` manifest at:
- `CtrlWizNW\CustomRibbon\PackageContents.xml`
- Deployed to: `%ProgramData%\Autodesk\ApplicationPlugins\CtrlWiz.bundle\`

The manifest specifies `AppType="ManagedPlugin"` and points to the versioned DLL under `./Contents/Nw22/CtrlWizNW.dll`. The manifest file currently only declares the NW22 component entry; NW17-NW25 build variants exist but the manifest itself targets NW22 only and needs updating for other versions.

The plugin entry class is declared with Navisworks plugin attributes:
```csharp
[Plugin("NVSXBOX.CmdViewpoint", "VIATechnik", DisplayName = "CtrlWiz")]
[RibbonLayout("CustomRibbon.xaml")]
[RibbonTab("ID_CustomTab_1", DisplayName = "CtrlWiz")]
[Command("ID_Button_1", CanToggle = true, DisplayName = " Activate\n Controller ", ...)]
```
Source: `CtrlWizNW\CtrlWiz.NW\CmdViewpoint.cs`

### Navisworks API Calls in Active Use

**Managed API (`Autodesk.Navisworks.Api`):**
- `Application.ActiveDocument` — retrieve current document
- `Document.CurrentViewpoint.CreateCopy()` / `CopyFrom()` — read and write camera position/speed
- `Viewpoint.LinearSpeed`, `Viewpoint.AngularSpeed` — set navigation speed
- `Viewpoint.Position` (Point3D) — direct camera position manipulation
- `Document.Units` — unit conversion factor for movement scaling
- `Document.SavedViewpoints` — enumerate, navigate, and create saved viewpoints
- `Document.CurrentSelection` — select/deselect/clear model items
- `Application.ActiveDocument.Tool.Value` — set active tool (e.g., `Tool.RedlineText`)
- `Application.Plugins.FindPlugin(...)` / `TryLoadPlugin()` — load sibling tool plugins (target cursor)
- `Application.MainDocument.Tool.SetCustomToolPlugin(...)` — activate a tool plugin

**COM API via `ComApiBridge` (`Autodesk.Navisworks.Api.ComApi` + Interop):**
- `ComApiBridge.State.CurrentView.ViewPoint as InwNvViewPoint2` — cast current view to legacy COM viewport
- `InwNvViewPoint2.Viewer.CameraMode` — toggle first/third person (`nwECameraMode.eCameraMode_ThirdPerson`)
- `InwNvViewPoint2.Viewer.Gravity` / `.CollisionDetection` — toggle physics
- `InwNvViewPoint2.Paradigm` — set navigation paradigm (`nwEParadigm.eParadigm_WALK`)

The COM API bridge is used specifically for camera mode and collision/gravity toggles because these properties are not exposed in the managed API.

**Internal GUI command dispatch (`navisworks.gui.roamer.dll`):**
```csharp
LcRmFrameworkInterface.ExecuteCommand("RoamerGUI_OM_ATTRIB_BAR", LcUCIPExecutionContext.eTOOLBAR);
LcRmFrameworkInterface.ExecuteCommand("RoamerGUI_OM_VIEW_TREE", LcUCIPExecutionContext.eTOOLBAR);
LcRmFrameworkInterface.ExecuteCommand("RoamerGUI_OM_VP_ORG", LcUCIPExecutionContext.eTOOLBAR);
LcRmFrameworkInterface.ExecuteCommand("RoamerGUI_MEASURE_SHORTESTDIST", LcUCIPExecutionContext.eTOOLBAR);
LcRmFrameworkInterface.ExecuteCommand("RoamerGUI_OM_MEASURE_NAV", LcUCIPExecutionContext.eTOOLBAR);
```
These are undocumented internal command IDs that show/hide panels and trigger measurement tools. They are fragile and may change across Navisworks versions.

### Multi-Version Build Strategy

The project defines one MSBuild configuration per supported NW version. Each configuration sets MSBuild properties pointing to the installed SDK directory for that version and defines a corresponding compile-time constant:

| Build Config | NW Install Path Used | Compile Constant | Output Path |
|---|---|---|---|
| `NW17\|x64` | `Navisworks Manage 2017\` | `NW2017` | `...\Navisworks\NW17\` |
| `NW18\|x64` | `Navisworks Manage 2018\` | `NW2018` | `...\Navisworks\NW18\` |
| `NW19\|x64` | `Navisworks Manage 2019\` | `NW2019` | `...\Navisworks\NW19\` |
| `NW20\|x64` | `Navisworks Manage 2020\` | `NW2020` | `...\Navisworks\NW20\` |
| `NW21\|x64` | `Navisworks Manage 2021\` | `NW2021` | `...\Navisworks\NW21\` |
| `NW22\|x64` | `Navisworks Manage 2022\` | `NW2022` | `...\Navisworks\NW22\` |
| `NW25\|x64` | `Navisworks Manage 2025\` | `NW2025` | `...\Navisworks\NW25\` |

The `Debug\|x64` config resolves against NW2022 and deploys directly to `%ProgramData%\...\Contents\v18\`.

There are no `#if NW2017` conditional compilation blocks in the source — the codebase compiles identically for all NW versions with no version-specific code paths. Source: `CtrlWizNW\CtrlWizNW.csproj`

---

## Revit API

### Referenced Assemblies

Assemblies are resolved from the Revit install directory at build time via `$(RevitAPIPath)` and `$(RevitAPIUIPath)` properties. Both are `Private=False`.

| Assembly | Role |
|---|---|
| `RevitAPI.dll` | Core Revit API — `Document`, `View3D`, `ViewOrientation3D`, `Transaction`, `XYZ` |
| `RevitAPIUI.dll` | Revit UI API — `UIApplication`, `UIControlledApplication`, `IExternalApplication`, `IExternalCommand`, `RibbonPanel`, `TaskDialog` |

### Plugin Loading Mechanism

Loaded via `.addin` XML manifest files placed in the Revit addins folder. The `PackageContents.xml` at `CtrlWizRVT\PackageContents\PackageContents.xml` handles multi-version dispatch by pointing to per-version `.addin` files. Separate per-version manifests exist under `CtrlWizRVT\Manifest\20{19,20,21,22}\CtrlWiz.RVT.addin`.

Entry point: `CtrlWiz.RVT.ExAppCtrlWizRVT` implements `IExternalApplication`. The `OnStartup` handler creates the `InAppCheckout` instance, wires the `ActivationChanged` event, and builds the ribbon UI using `UIControlledApplication`.

### Supported Versions

| Build Config | Revit Install Path Used | Compile Constant |
|---|---|---|
| `RVT19\|AnyCPU` | `Revit 2019\` | `RVT19` |
| `RVT20\|AnyCPU` | `Revit 2020\` | `RVT20` |
| `RVT21\|AnyCPU` | `Revit 2021\` | `RVT21` |
| `RVT22\|AnyCPU` | `Revit 2022\` | `RVT22` |

`PackageContents.xml` also references an RVT18 component entry, but no `RVT18` MSBuild configuration exists. Source: `CtrlWizRVT\CtrlWizRVT.csproj`

### Revit API Calls in Active Use

Source: `CtrlWizRVT\CtrlWiz.RVT\CmdActivateController.cs`

- `commandData.Application` (`UIApplication`) — access to the running session
- `UIApplication.ActiveUIDocument.ActiveView as View3D` — get current 3D view
- `View3D.GetOrientation()` / `SetOrientation(ViewOrientation3D)` — read/write camera eye position, up/forward vectors
- `View3D.IsPerspective` — guard: stops input processing in orthographic views
- `View3D.IsLocked` / `Unlock()` — automatically unlock locked views before camera changes
- `View3D.DisplayStyle` — temporarily downgrade display quality during movement (e.g., `RealisticWithEdges` to `Realistic`) to reduce lag
- `UIApplication.ActiveUIDocument.RefreshActiveView()` — force render update after each frame
- `UIApplication.ViewActivating` event — update `view3D` reference when the user switches views
- `new Transaction(doc, ...).Start(...)` / `.Commit()` — wrap the controller initialization in a Revit transaction

---

## Paddle Licensing

### SDK

- Package: `Paddle.PaddleSDK` version `2.0.5`
- Assembly: `PaddleSDK.dll` (net35 build)
- NuGet source: `CtrlWizLicense\packages.config`
- Hint path: `..\packages\Paddle.PaddleSDK.2.0.5\lib\net35\PaddleSDK.dll`

### Configuration

Credentials (API key, vendor ID, product IDs, vendor name) are stored as compiled-in string resources in `CtrlWizLicense\Properties\Resources.resx`. They are never stored in config files or environment variables.

```csharp
Paddle.Configure(Resources.ApiKey, Resources.VendorId, this.ProductId, this.ProductInfo);
```

Two products are supported, selected by the `ProductVersion` enum at construction time:
- `ProductVersion.NW` — uses `Resources.ProductIdNW`, temp file `Resources.TempNW` under `Path.GetTempPath()`
- `ProductVersion.RVT` — uses `Resources.ProductIdRVT`, temp file `Resources.TempRVT` one level above `Path.GetTempPath()`

### Checkout and Activation Flow

Source: `CtrlWizLicense\InAppCheckout.cs`

**NW flow (event-driven, used by `CmdViewpoint.cs`):**
1. `new InAppCheckout(ProductVersion.NW, commandId)` — configures Paddle singleton, wires transaction events
2. `.Start()` — calls `Product.Refresh(callback)` asynchronously
3. In the refresh callback:
   - If `Product.Activated` — calls `Product.VerifyActivation(callback)`, fires `CheckoutCompleted` with `Allowed=true/false`
   - If not activated — shows `Paddle.Instance.ShowCheckoutWindowForProduct(Product)`, fires `CheckoutCompleted` with `Allowed=false`
4. `CheckoutCompleted` event carries the originating `commandId` so `CmdViewpoint` routes to the correct ribbon action

**RVT flow (synchronous-style, used by `ExAppCtrlWizRVT.cs`):**
1. `new InAppCheckout(ProductVersion.RVT)` created in `OnStartup`
2. `InAppCheckout.StartPaddle(out string message)` called on each command execution via `IsLicenseActive()`
3. If `Product.Activated` — calls `VerifyActivation`; on `VerificationState.Unverified` calls `Product.Deactivate`
4. If not activated — shows `ShowCheckoutWindowForProduct`

**License details window:** `Paddle.Instance.ShowLicenseActivationWindowForProduct(Product)` — opened from ribbon button ID_Button_5 (NW) or `CmdLicense` (RVT).

### Offline Fallback

When the Paddle server is unreachable (`StartPaddle` catches the exception), the code reads a binary temp file written on the last successful connection:
```csharp
// Write on success:
binaryWriter.Write(Product.Activated);

// Read on failure:
IsProductActivated = binaryReader.ReadBoolean();
```
The temp file is a single boolean byte. If the file is absent and the server is unreachable, the product is treated as unlicensed and the user sees an error message.

### Verification States

`VerificationState.UnableToVerify` is treated as **allowed** (graceful degradation) in the NW flow, while `Unverified` is treated as denied. Source: `CtrlWizLicense\InAppCheckout.cs`, method `InAppCheckout_VerifyActivationCompleted`.

---

## XInput / Gamepad

### Architecture

`XInputDotNetPure` is an in-tree managed wrapper project (`XInputDotNetPure\XInputDotNetPure.csproj`) that P/Invokes a companion native DLL named `XInputInterface`. It provides a pure C# surface over the raw XInput API.

The native DLL name is hardcoded:
```csharp
// XInputDotNetPure\XInputDotNetPure\Imports.cs
internal const string DLLName = "XInputInterface";

[DllImport(DLLName)]
public static extern uint XInputGamePadGetState(uint playerIndex, out GamePadState.RawState state);

[DllImport(DLLName)]
public static extern void XInputGamePadSetState(uint playerIndex, float leftMotor, float rightMotor);
```

`XInputInterface.dll` must be present alongside `XInputDotNetPure.dll` at runtime. It is a native C++ shim that calls into the system `xinput1_4.dll` (or similar).

### Public API Surface

- `GamePad.GetState(PlayerIndex.One)` — returns a `GamePadState` snapshot
- `GamePad.SetVibration(PlayerIndex.One, leftMotor, rightMotor)` — trigger haptic feedback (0.0-1.0 float)
- `GamePadState` exposes:
  - `.Buttons` — `A`, `B`, `X`, `Y`, `Start`, `Back`, `LeftStick`, `RightStick`, `LeftShoulder`, `RightShoulder`, `DPad.*`
  - `.ThumbSticks.Left.X/.Y`, `.ThumbSticks.Right.X/.Y` — float -1.0 to 1.0
  - `.Triggers.Left`, `.Triggers.Right` — float 0.0 to 1.0
- Dead zone handling via `GamePadDeadZone.IndependentAxes` (default) or `None`

### Controller Input to NW Camera Mapping

The `Update()` loop in `CmdViewpoint.cs` polls continuously with no fixed sleep in the primary path (`await Task.Delay(10)` only triggers when Navisworks loses focus). Mapping is configurable by the user via the Settings UI stored in `CtrlWizNW.UI` properties:

| Controller Input | Default Function | NW API Effect |
|---|---|---|
| Left stick X/Y | Move/Sprint | `MoveCamera(x, y, z)` — translate `Viewpoint.Position` |
| Right stick X/Y | Look | `EulerAngleCamera(pitch, yaw)` — rotate `Viewpoint.Rotation` |
| LT / RT | Elevation down/up | Z component of `MoveCamera` |
| Left stick click | Sprint | Speed multiplier |
| Back button | Deactivate plugin | Exits `Update()` loop |
| A/B/X/Y/RB/Start | Configurable | Select, hide, viewpoints, markup, properties |

### Collision/Gravity Mode: InputSimulator Fallback

When `IsCollisionAndGravityEnabled()` returns true, the plugin switches from direct camera API calls to keyboard simulation via InputSimulator (see InputSimulator section). This is a deliberate workaround because direct `Viewpoint` position manipulation does not respect Navisworks collision/gravity physics.

### Revit Usage

`CmdActivateController.cs` uses the same `XInputDotNetPure` assembly directly. Thumb sticks drive `View3D.SetOrientation`, triggers provide elevation, and `Back` button exits the loop. The Revit plugin does not use the configurable button-mapping system; mappings are hardcoded.

---

## InputSimulator

### Package

- NuGet ID: `InputSimulator` version `1.0.4.0`
- Assembly: `WindowsInput.dll` (net20 build)
- Reference: `CtrlWizNW\CtrlWizNW.csproj`
- Declared in: `CtrlWizNW\packages.config`

### What It Does

`InputSimulator` (`WindowsInput.InputSimulator`) provides managed wrappers around the Win32 `SendInput` API to synthesize keyboard and mouse events. The plugin uses only the keyboard component.

### Active Usage

Source: `CtrlWizNW\CtrlWiz.NW\CmdViewpoint.cs`

`InputSimulator` is **actively used but only in a narrow scenario**: when Navisworks is in Walk mode with collision detection and gravity enabled. In that mode, direct `Viewpoint` position manipulation breaks physics, so the plugin falls back to simulating arrow key presses:

```csharp
private InputSimulator _inputSimulator = new InputSimulator();

// During collision/gravity walk mode:
_inputSimulator.Keyboard.KeyDown(VirtualKeyCode.RIGHT);   // left stick X > 0
_inputSimulator.Keyboard.KeyUp(VirtualKeyCode.RIGHT);
_inputSimulator.Keyboard.KeyDown(VirtualKeyCode.UP);      // left stick Y > 0
_inputSimulator.Keyboard.KeyDown(VirtualKeyCode.DOWN);    // left stick Y < 0
```

Key-up events are sent on thumb stick release, tracked via `isThumbStickValueXReleased` / `isThumbStickValueYReleased` flags. This path only activates when `IsCollisionAndGravityEnabled()` returns true.

The `_inputSimulator` instance is declared at class level. It is not used in the Revit plugin.

---

## CredentialManagement

### Package

- NuGet ID: `CredentialManagement` version `1.0.2`
- Assembly: `CredentialManagement.dll` (net35 build)
- Reference: `CtrlWizLicense\CtrlWizLicense.csproj`
- Declared in: `CtrlWizLicense\packages.config`

### Actual Usage

**Zero active call sites.** A search across all `.cs` files confirms that `CredentialManagement` types are never instantiated or called anywhere in the codebase. The DLL is pulled in as a dependency and compiled into the output, but provides no runtime behavior.

### What It Could Do

The `CredentialManagement` library wraps the Windows Credential Manager (`advapi32.dll` DPAPI functions), allowing applications to store and retrieve named credentials from the Windows credential store. It was presumably added with the intent to persist license activation state or user credentials securely, but this was never implemented. The offline license fallback uses a plain binary temp file instead.

---

## WatiN (Legacy)

### Package

- NuGet ID: `WatiN` version `2.1.0`
- Assemblies: `WatiN.Core.dll`, `Interop.SHDocVw.dll`, `Microsoft.mshtml.dll`
- All three referenced in `CtrlWizLicense\CtrlWizLicense.csproj` with `EmbedInteropTypes=True`
- Declared in: `CtrlWizLicense\packages.config`

### Original Purpose

WatiN (Web Application Testing In .NET) was a UI automation library that drove Internet Explorer via the `SHDocVw` COM interface. It was likely used to automate a web-based checkout or license verification flow — an early implementation of the Paddle checkout before the native Paddle SDK was integrated.

### Current Status

**Zero active call sites.** No `.cs` file in the repository references `WatiN.Core`, `SHDocVw`, or `Microsoft.mshtml` types. The packages are present and the DLLs are compiled in, but WatiN is entirely dead code.

**Windows 11 compatibility:** WatiN 2.1.0 (last released 2011) requires Internet Explorer. IE is disabled by default on Windows 11 and fully removed in later feature updates. Even if call sites existed, WatiN would fail to launch on a modern Windows 11 machine. These three package references should be removed from `CtrlWizLicense\packages.config` and `CtrlWizLicense\CtrlWizLicense.csproj`.

---

## COM & Windows APIs

### P/Invoke in CmdViewpoint.cs

Source: `CtrlWizNW\CtrlWiz.NW\CmdViewpoint.cs` lines 1263-1276

```csharp
[DllImport("user32.dll")]
private static extern IntPtr GetForegroundWindow();

[DllImport("user32.dll")]
private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out IntPtr lpdwProcessId);
```

These are called every loop iteration in `Update()` to check whether Navisworks still has foreground focus. If the active process changes (e.g., user Alt-Tabs), the update loop pauses (`await Task.Delay(10); continue`). This prevents the gamepad from controlling the camera when Navisworks is not the focused window.

### Navisworks COM API (In-Process)

`Autodesk.Navisworks.ComApi.dll` and `Autodesk.Navisworks.Interop.ComApi.dll` provide a COM-based API to Navisworks internals, accessed via the `ComApiBridge` managed bridge class. The interop assembly is compiled with `EmbedInteropTypes=True` in `CtrlWizNW.csproj`, embedding COM type information directly in the output assembly.

Actively used COM types:
- `InwNvViewPoint2` — extended viewpoint with gravity, collision, camera mode, and navigation paradigm
- `nwECameraMode` — enum: `eCameraMode_FirstPerson`, `eCameraMode_ThirdPerson`
- `nwEParadigm` — enum: `eParadigm_WALK`, `eParadigm_APPLICATION` (`eParadigm_FLY` appears only in commented-out code)

### Commented-Out P/Invoke (CmdTool.cs)

`CtrlWizNW\CtrlWiz.NW\CmdTool.cs` contains three commented-out `[DllImport("user32.dll")]` declarations. These are inactive leftover exploration code.

### Registry Access (NavisUtils.cs)

`CtrlWizNW\CtrlWiz.NW\NavisUtils.cs` contains two commented-out methods that would read/write a `CtrlWiz` key under `HKEY_LOCAL_MACHINE\SOFTWARE` via `Microsoft.Win32.Registry`. Never activated; dead code.

### Bitmap Interop (Revit)

`CtrlWizRVT\CtrlWiz.RVT\ExAppCtrlWizRVT.cs` uses `System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(IntPtr, ...)` to convert GDI `HBITMAP` handles (from `Bitmap.GetHbitmap()`) into WPF `BitmapSource` for ribbon button images. Standard WPF/GDI interop; no custom `DllImport` required.

---

## External URLs

| URL | Purpose |
|---|---|
| `https://feedback.ctrlwiz.com/ctrwiznav-feature-requests` | Feature request portal — opened by Button3 in NW plugin via `Process.Start` |
| `https://help.buildfore.com/ctrlwiz/ctrlwiz-for-navisworks/how-to-use-ctrlwiz-navisworks` | NW online documentation (PackageContents.xml) |
| `https://help.buildfore.com/ctrlwiz/ctrlwiz-for-revit/ctrlwiz-xbox-controller-for-revit` | Revit online documentation (contextual help) |
| `https://www.viatechnik.com/` | Vendor URL (manifests) |

These are opened via `System.Diagnostics.Process.Start(url)` or Revit's `ContextualHelp` API. There is no HTTP client code in the plugin itself beyond the Paddle SDK's internal calls.
