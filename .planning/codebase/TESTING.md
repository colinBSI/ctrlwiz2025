# Testing Patterns
_Last updated: 2026-05-01_

## Summary

There are no automated test projects in this repository. The only project with "test" in its name is `FullPermissionTest`, which is a console executable (`OutputType: Exe`) used to manually verify that Windows filesystem ACL permissions can be granted to the `CtrlWizLicense` and `System` directories under `%ProgramData%`. It is not a unit test project and contains no test framework references. All validation of plugin behavior is done manually by running the plugin inside Navisworks or Revit.

---

## Test Infrastructure

### FullPermissionTest (`FullPermissionTest/Program.cs`)

**Type:** Console application (manual run tool, not a test framework)
**Framework:** .NET Framework 4.8, no NuGet test packages
**Purpose:** Verify that `DirectoryInfo.SetAccessControl()` correctly sets `BuiltinUsersSid` `FullControl` ACLs on installation directories.

What it does:
1. Calls `GrantAccess(Path.Combine(programDataDirectory, "CtrlWizLicense"))` — sets ACL on the license data directory.
2. Calls `GrantAccess(Path.Combine(programDataDirectory, "System"))` — sets ACL on the system data directory.

The `GrantAccess` method does two `ModifyAccessRule` calls — one for the directory itself (`NoPropagateInherit`) and one for inherited children (`InheritOnly`). It returns `bool` to indicate success. There are no assertions, no test output, and no pass/fail reporting — the developer must inspect behavior manually.

There is also a dead-code alternative `GrantAccess1` method (different approach to the same ACL grant) left in the file for reference.

**How to run:**
```
# From Visual Studio: Set FullPermissionTest as startup project and press F5.
# From command line (must run as Administrator for ACL changes):
cd C:\Users\cemch-wei\Documents\GitHub\ctrlwiz2025\FullPermissionTest\bin\Debug
FullPermissionTest.exe
```

---

## No Automated Tests

### Test Frameworks
None installed. There are no references to:
- MSTest (`Microsoft.VisualStudio.TestTools.UnitTesting`)
- NUnit (`NUnit`, `NUnit3TestAdapter`)
- xUnit (`xunit`, `xunit.runner.visualstudio`)
- FluentAssertions or any assertion library

No `*.Test.csproj` or `*.Specs.csproj` project files exist. No files matching `*.test.cs` or `*.spec.cs` exist.

---

## What Is and Is Not Tested

### Tested (manually, by running the plugin)
| Area | How verified |
|---|---|
| Navisworks controller input loop (`CmdViewpoint.cs`) | Load plugin in Navisworks, connect Xbox controller, navigate |
| Revit camera movement (`CmdActivateController.cs`) | Load plugin in Revit, connect controller, navigate a 3D view |
| Speed settings form | Open settings dialog in-plugin, adjust sliders |
| Controller map UI (`HelpView.xaml`) | Open help/map window, change button assignments |
| License activation (`InAppCheckout.cs`) | Activate via Paddle SDK checkout flow |
| Installer ACL setup | Run `FullPermissionTest.exe` as admin |

### Not Tested (no coverage at all)
| Area | Risk |
|---|---|
| `CtrlWiz.Logging/Logger.cs` — log file path construction and file writes | Log silently fails if `_logFilePath` is null (SetUpLogger not called) |
| `CtrlWizNW.UI/ViewModels/HelpViewModel.cs` — all controller mapping logic | Settings corruption, wrong button mapped |
| `CtrlWizNW.UI/Properties/GetControllerElement.cs` — settings property lookup | Wrong controller element returned for a function |
| `CtrlWizForms/Presenters/SpeedSettingPresenter.cs` | Speed values not propagated correctly |
| `NavisUtils.cs` — math helpers (`MultiplyRotation3D`, `VLerp`, `getViewDir`) | Silent calculation errors in navigation |
| `CtrlWizRVT/Utility/ExtensionMethods.cs` — geometry converters | Rotation/translation errors in Revit camera |
| `CtrlWizLicense/InAppCheckout.cs` — all license check logic | Silent license failures |
| `CtrlWizForms.CustomControls/RoundedCornersButton.cs` | Visual regression |
| `CtrlWizRVT/States/SpeedSettings.cs` | Speed factor misapplication |

---

## Testability Assessment

### What Makes Testing Hard Today

1. **No dependency injection at plugin entry points.** `CmdViewpoint` and `CmdActivateController` instantiate their dependencies directly and depend on static Autodesk API objects (`Application.ActiveDocument`, `UIApplication`). These cannot be instantiated outside the host application.

2. **Autodesk API not mockable without the host.** `Autodesk.Navisworks.Api` and `Autodesk.Revit.DB` types are sealed or require a running host process. Unit testing any code that touches these APIs requires the full Navisworks/Revit process.

3. **MVP presenter has a testability backdoor that is unused.** `SpeedSettingPresenter` has a private constructor accepting interfaces (`ISpeedSettingForm`, `ISpeedSettingModel`, `IMessageService`) — the intended seam for unit testing. It is `private`, preventing use from a test project. Changing to `internal` + `[InternalsVisibleTo]` would enable testing.

4. **MVVM ViewModel is testable in isolation.** `HelpViewModel` depends only on `Settings.Default` (a static) and the model types. The `INotifyPropertyChanged` behavior, command execution, and option selection logic could be tested with a test settings shim. This is the highest-value, lowest-effort testing opportunity in the codebase.

5. **`NavisUtils` and `CtrlWizRVT/Utility/ExtensionMethods.cs` contain pure math.** These have no Autodesk API dependencies and are straightforwardly unit-testable. `MultiplyRotation3D`, `VLerp`, `AngleBetween`, `RotateByAxis` are all deterministic functions.

---

## Testing Gaps and Risks

**High Risk — no coverage:**
- `HelpViewModel` controller mapping logic: wrong button assignments would silently persist to user settings. The `UpdateRelatedThumbStickInDroneControlsMode` method is particularly complex with no tests.
- `InAppCheckout.cs` license validation: all 19 `ex.LogException()` calls indicate error paths that are exercised only in production.
- `GetControllerElement.GetControllerButtonByFunction` / settings property lookups: a typo in a settings key string (e.g., `"NV_RBumber"` vs `"NV_RBumper"`) would silently return `Unset` with no error.

**Medium Risk:**
- `NavisUtils.MultiplyRotation3D` and `getViewDir` — math errors would cause wrong camera behavior only discoverable by testing in the running application.
- `SpeedSettingPresenter` — the model is an empty stub (`SpeedSettingModel` has no members), so speed values bypass the model layer entirely and are passed directly between view and presenter fields.

**Low Risk:**
- `Logger.SetUpLogger` path construction — simple string concatenation, easy to verify manually.
- `RoundedCornersButton` — pure rendering, no logic.

---

## How to Add Tests

### Recommended first step: unit-test HelpViewModel

1. Create a new project `CtrlWiz.Tests` targeting .NET Framework 4.8.
2. Add NuGet: `xunit`, `xunit.runner.visualstudio`, `FluentAssertions`.
3. Make `SpeedSettingPresenter`'s testability constructor `internal` and add `[assembly: InternalsVisibleTo("CtrlWiz.Tests")]` to `CtrlWizForms`.
4. Stub out `Settings.Default` or refactor `HelpViewModel` to accept an `ISettings` abstraction.

### Recommended second step: unit-test pure math utilities

`CtrlWizNW/CtrlWiz.NW/NavisUtils.cs` and `CtrlWizRVT/Utility/ExtensionMethods.cs` have zero external dependencies. Reference these projects from `CtrlWiz.Tests` and write data-driven tests for rotation and vector math.

### Integration / end-to-end tests

There is no practical path to automated integration testing without Navisworks or Revit installed. Manual test scripts run in the actual host application remain the only viable approach for API-dependent code.
