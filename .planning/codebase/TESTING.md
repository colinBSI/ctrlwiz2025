# CtrlWiz Testing Guide

## Current State

There are no automated test projects in this repository. The only project with "test" in
its name is FullPermissionTest/, which is a manual console harness for verifying that the
installer's directory-permission custom action can grant FullControl ACLs to BuiltinUsers
on ProgramData subdirectories. It is not a unit test runner and has no assertions.

All validation of plugin behaviour currently relies on manual, in-process testing inside a
live Autodesk Revit or Navisworks session with a physical Xbox controller attached.

---

## Constraints That Make Testing Hard

Understanding these constraints is necessary before adding any automated tests.

### In-process host dependency

Both plugins run inside the Autodesk host process. All host API types
(Autodesk.Revit.DB.View3D, Autodesk.Navisworks.Api.Document, etc.) are sealed or internal
classes loaded from the host's own DLLs. They cannot be instantiated in isolation.
Any test that exercises code touching host API objects must either:
  (a) run inside the host process (integration test), or
  (b) use an abstraction layer that does not exist yet.

### Async controller loop

InitializeController (RVT) and the equivalent NW loop are async void methods driven by
await Task.Delay. There is no hook to pause, advance, or cancel the loop from outside. A
test that starts the loop will run indefinitely unless isRun / isrun is set to false via
reflection or a new cancellation mechanism is introduced.

### Settings singleton

CtrlWizNW.UI.Properties.Settings.Default is a process-wide singleton. Tests that mutate
Settings.Default state will interfere with each other unless Settings.Default.Reset() is
called between tests.

### Paddle SDK

The Paddle SDK opens UI windows (checkout, activation dialogs) synchronously on the calling
thread. Any test that calls InAppCheckout.StartPaddle will block waiting for user
interaction unless the SDK is mocked.

---

## What Can Be Tested Today Without Host APIs

The following units have no hard dependency on host API types and can be tested in a plain
xUnit or MSTest project that references only the assembly under test plus XInputDotNetPure
and CtrlWiz.Logging:

### NavisUtils math (CtrlWizNW)

  Methods: getViewDir, MultiplyRotation3D, VLerp, AngleBetween
  Point3DtoV3, Rot3DtoQuaternion, V3toPoint3D, etc.

  These methods take Autodesk.Navisworks.Api.Rotation3D, Point3D, Vector3D, and
  System.Numerics.Vector3 values. Rotation3D and Point3D are value types defined in the
  Navisworks API DLL. Testing requires the Navisworks API DLL on disk (available from any
  NW SDK installation) but does not require a running Navisworks instance.

  Suggested test cases:
  - MultiplyRotation3D(identity, r) == r for any quaternion r
  - MultiplyRotation3D(r, r.Invert()) == identity (within floating-point tolerance)
  - VLerp(from, to, 0.0) == from
  - VLerp(from, to, 1.0) == to
  - VLerp(from, to, 0.5) == midpoint
  - AngleBetween(v, v) == 0
  - AngleBetween(unitX, unitY) == 90 degrees

### SpeedSettingPresenter (CtrlWizForms)

  The private constructor accepts (ISpeedSettingForm, ISpeedSettingModel, IMessageService).
  All three are interfaces. Create test doubles (mocks or stubs) for each.

  Suggested test cases:
  - After constructing with initialLinear=20 and initialAngular=45, LinearSetting==20 and
    AngularSetting==45.
  - Raising ISpeedSettingForm.LinearTrackBarValueChanged with SettingValue=30 updates
    LinearSetting to 30.
  - Raising ISpeedSettingForm.AngularTrackBarValueChanged with SettingValue=60 updates
    AngularSetting to 60.
  - ShowDialog delegates to ISpeedSettingForm.ShowDialog and returns its result.

  To access the private constructor use reflection or add an internal-visible-to attribute
  to CtrlWizForms.

### HelpViewModel state machine (CtrlWizNW.UI)

  HelpViewModel reads from and writes to Settings.Default. In tests, call
  Settings.Default.Reset() in the test fixture setup and teardown to isolate state.

  Suggested test cases:
  - After construction, SelectedAButtonIndex returns the index of the function stored in
    Settings.Default["NV_AButton"], or the index of Function.None if the setting is Unset.
  - Setting SelectedAButtonIndex = n calls Settings.Default.Save() (verify via spy or by
    reading Settings.Default["NV_AButton"] after the set).
  - Selecting Function.LeftDroneControls for the left stick also changes the right stick to
    Function.RigthDroneControls (drone-mode coupling).
  - Selecting Function.LeftDroneControls disables triggers (IsTriggerEnabled == false).
  - RestoreDefault command calls Settings.Default.Reset() and resets all SelectedXIndex
    properties to defaults.

### ControllerButton / ControllerTrigger / ControllerThumbStick / Function enums

  - Enum integer values are part of the persisted settings format. Verify that no values
    have changed from the expected literals:
      ControllerButton: X=0, Y=1, A=2, B=3, RBumber=4, Start=5, None=6, Unset=-1
      Function: EnableTextMarkup=0 ... RigthDroneControls=20, None=21, Unset=-1
    A regression test that asserts (int)ControllerButton.A == 2 etc. will catch accidental
    reordering that would silently corrupt user settings files.

### GetControllerElement mapping (CtrlWizNW.UI)

  GetControllerElement.GetControllerElementByPropertyName and GetPropertyNameByControllerElement
  are pure switch-style mappings. Test round-trip consistency:
  - For every property name in the mapping, GetPropertyNameByControllerElement(
      GetControllerElementByPropertyName(name)) == name.

### Logger (CtrlWiz.Logging)

  Logger.SetUpLogger and Logger.LogException use File.AppendAllText. In tests, initialize
  the logger with a temp file in Path.GetTempPath(), call LogException with a constructed
  exception, and assert that the file was written with the expected UTC timestamp prefix
  and exception message. Clean up the temp file in test teardown.

  Known issue to verify: If the exception's stack trace is null or has no frames,
  GetExceptionInfo (ExtensionMethods.cs, line 11) will throw a NullReferenceException.
  Write a test that calls LogException on a plain new Exception("msg") with no stack trace
  to confirm the crash, then write the fix.

### InAppCheckout offline path (CtrlWizLicense)

  The constructor and StartPaddle call Paddle.Configure which talks to the Paddle SDK. Mock
  or stub the Paddle SDK (the SDK uses a singleton Paddle.Instance; replacing it requires
  either an interface wrapper or subclassing PaddleProduct).

  Most testable without mocking:
  - If tempFileName exists with a BinaryWriter-written true value and Paddle.Configure
    throws, IsProductActivated is set to true from the file.
  - If tempFileName does not exist and Paddle.Configure throws, IsProductActivated remains
    false and the out message is set to the "Unable to verify license" string.

  These scenarios test the catch block in StartPaddle (lines 162-175 in InAppCheckout.cs).

### RoundedCornersButton (CtrlWizForms.CustomControls)

  The hover state and paint logic can be exercised by creating the control in a test (no
  form required), raising MouseEnter/MouseLeave events, and asserting that Invalidate was
  called. The OnPaint path requires a Graphics object; use a Bitmap with
  Graphics.FromImage to supply a real Graphics context in a headless test.

---

## Recommended Test Project Setup

### Project type

Create one MSTest or xUnit class library project per testable assembly, for example:
  CtrlWiz.Logging.Tests
  CtrlWizForms.Tests
  CtrlWizNW.UI.Tests

Target .NET Framework 4.8 to match the production assemblies. Use PackageReference-style
references if adding to the existing solution format requires converting .csproj files;
otherwise use the existing packages.config style.

### Mocking framework

No mocking framework is currently in use. Add Moq (4.x) via NuGet. It supports
.NET Framework 4.8 and handles interface mocking without requiring virtual methods.

### Floating-point comparisons

NavisUtils math uses float and double. Use an epsilon tolerance of 1e-5f for float
comparisons and 1e-10 for double. Do not use Assert.AreEqual for floating-point values.
Use a helper:

  static void AssertApprox(double expected, double actual, double eps = 1e-10)
  {
      Assert.IsTrue(Math.Abs(expected - actual) < eps,
          $"Expected {expected} but got {actual} (eps={eps})");
  }

---

## Manual Testing Procedures

Until automated tests are in place, the following manual procedures must be completed
before any release build:

### RVT smoke test (per build configuration RVT19-RVT22)

1. Open Revit and confirm the "CtrlWiz" ribbon tab appears with five buttons:
   Activate Controller, Controller Map, Feature Request, Speed Setting, License.
2. Without an active license: click Activate Controller. Confirm the Paddle checkout
   window appears.
3. With an active license: open a 3D perspective view. Click Activate Controller.
   Confirm the controller vibrates briefly on startup.
4. Move the left thumbstick. Confirm the camera translates in the expected direction.
5. Move the right thumbstick. Confirm the camera rotates.
6. Press Left Trigger / Right Trigger. Confirm the camera moves down / up.
7. Press the Back button on the controller. Confirm the loop stops (camera no longer
   responds to input).
8. Open Speed Setting. Move the linear slider. Re-activate and verify that camera
   movement speed changed proportionally.
9. Switch to a non-perspective (orthographic) view. Move a thumbstick. Confirm the
   PerspectiveViewNotActiveDialogBox appears and movement does not crash Revit.
10. Click Help. Confirm the help image is displayed and the window closes cleanly.

### NW smoke test (per build configuration NW17-NW22)

1. Open Navisworks. Confirm the "CtrlWiz" ribbon tab appears with five buttons.
2. Click Activate Controller (ID_Button_1). Confirm the controller vibrates on startup.
3. Move the left thumbstick. Confirm the viewpoint translates.
4. Move the right thumbstick. Confirm the viewpoint rotates.
5. Press Left Trigger. Confirm the viewpoint moves downward (elevation down).
6. Press Right Trigger. Confirm the viewpoint moves upward (elevation up).
7. Open Controller Map (ID_Button_2). Verify that each controller element shows a
   drop-down with the correct function list. Change one assignment and close. Re-open:
   confirm the changed assignment persisted.
8. Click Restore Default in the Controller Map window. Confirm all assignments reset to
   factory defaults.
9. Open Speed Setting (ID_Button_4). Adjust the sliders and close. Re-activate and
   verify the speed changed.
10. Click License (ID_Button_5). Confirm the Paddle license window appears.
11. Click Feature Request (ID_Button_3). Confirm the feedback URL opens in the default
    browser.

### Installer smoke test

1. Run the MSI on a clean machine (no Autodesk software previously installed) and verify
   that the installer reports an error (not a crash) if Navisworks or Revit is absent.
2. On a machine with a supported Autodesk version installed, run the installer with a
   non-admin account. Confirm that the admin-privilege warning dialog appears and that
   declining the dialog cancels the install.
3. After install, verify that the bundle/Contents directory exists and that the DLL is
   code-signed (right-click > Properties > Digital Signatures).
4. After install, verify that ProgramData\CtrlWizLicense has the expected ACL
   (BuiltinUsers: FullControl).

---

## Known Gaps and Priority Order

The following are the highest-priority items to address when adding automated tests:

1. (Critical) License state machine in InAppCheckout.StartPaddle — the offline fallback
   path and the race between Product.Refresh callback and the post-callback IsProductActivated
   assignment have no test coverage. A bug here locks users out of the product.

2. (High) Button-mapping persistence round-trip — HelpViewModel <-> Settings.Default <->
   GetControllerElement. A regression here breaks all user-saved button layouts silently.

3. (High) Enum value stability — ControllerButton, Function integer values are persisted
   in user settings. Any reordering breaks deserialization without a visible error.

4. (Medium) NavisUtils quaternion math — getViewDir and MultiplyRotation3D are called on
   every controller frame; a subtle floating-point bug causes view jitter that is hard to
   diagnose.

5. (Medium) Logger null-safety — GetExceptionInfo crashes if the exception has no stack
   frames. This is the error handler; a crash here swallows the original exception entirely.

6. (Low) SpeedSettingPresenter event wiring — straightforward to test once interfaces are
   accessible, low risk of regression.
