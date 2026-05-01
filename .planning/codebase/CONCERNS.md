# Concerns
_Last updated: 2026-05-01_

## Summary

CtrlWiz is a mature but organically-grown plugin codebase targeting multiple Navisworks (NW17–NW25) and Revit (RVT19–RVT22) versions through per-version MSBuild solution configurations rather than abstraction. The core Navisworks command handler (`CmdViewpoint.cs`, 2,496 lines) is a monolith that concentrates business logic, state, UI interaction, game loop, licensing, and COM interop in one class. Several third-party dependencies are significantly out-of-date, the `.pfx` signing certificate is committed to source control across five projects, and the `PackageContents.xml` plugin manifest hard-codes `SeriesMin="Nw22" SeriesMax="Nw22"` even though NW25 support was recently added.

---

## Critical Risks

### PackageContents.xml Does Not Declare NW25
- **Issue:** `CtrlWizNW\CustomRibbon\PackageContents.xml` declares `SeriesMin="Nw22" SeriesMax="Nw22"`. Autodesk's Application Manager uses this file to decide whether to load the plugin. Despite NW25 build support being added, the manifest has not been updated.
- **Files:** `CtrlWizNW\CustomRibbon\PackageContents.xml`
- **Impact:** The plugin will silently fail to load in Navisworks 2025 for users who install via the bundle directory rather than the installer. Any automated deployment relying on the manifest will also fail.
- **Fix approach:** Update `SeriesMin="Nw22" SeriesMax="Nw25"` (or the appropriate range). Verify with Autodesk bundle schema docs for the correct version token for NW25.

### PFX Certificate Committed to Source Control
- **Issue:** `VIATechnik_CS_cert.pfx` is present in five project directories: `CtrlWizForms\`, `CtrlWizLicense\`, `CtrlWizNW\`, `CtrlWizRVT\`, and `XInputDotNetPure\`. All `.csproj` files reference it as `<AssemblyOriginatorKeyFile>` even though `<SignAssembly>false</SignAssembly>` is set, meaning the file is not currently used for signing — but its presence in the repo represents a credential leak if the file contains a private key.
- **Files:** `CtrlWizForms\VIATechnik_CS_cert.pfx`, `CtrlWizLicense\VIATechnik_CS_cert.pfx`, `CtrlWizNW\VIATechnik_CS_cert.pfx`, `CtrlWizRVT\VIATechnik_CS_cert.pfx`, `XInputDotNetPure\VIATechnik_CS_cert.pfx`
- **Impact:** Private key exposure if the certificate is not password-protected. If signing is ever re-enabled (`<SignAssembly>true</SignAssembly>`), all five projects will attempt to sign with the same shared key.
- **Fix approach:** Remove `.pfx` files from the repository, add `*.pfx` to `.gitignore`, and store the certificate in a secrets manager or per-developer local path. Use a build secret or environment variable to supply the path at build time.

### `throw ex` Destroys Stack Traces
- **Issue:** Three `catch` blocks in `CmdViewpoint.cs` use `throw ex;` instead of bare `throw;`, destroying the original exception stack trace before it reaches the logger.
- **Files:** `CtrlWizNW\CtrlWiz.NW\CmdViewpoint.cs` lines 621, 746, 783
- **Impact:** When `GetListSaveVPts` or `recurse` fails, the logged exception will show the catch site rather than the root cause, making crashes in production difficult to diagnose.
- **Fix approach:** Replace `throw ex;` with `throw;` in each location.

---

## Technical Debt

### CmdViewpoint.cs Monolith (2,496 lines)
- **Issue:** The entire Navisworks plugin lives in a single class: game loop (`Update`), camera math, button mapping, license checking, viewpoint management, COM interop, UI invocation, speed settings, drone controls, inertia, vibration, and async coordination.
- **Files:** `CtrlWizNW\CtrlWiz.NW\CmdViewpoint.cs`
- **Impact:** High cognitive load for any change. Adding a new controller function, fixing a camera bug, or updating license logic all touch the same file. Unit testing any individual behavior is impractical.
- **Fix approach:** Extract at minimum: camera movement logic, button state dispatch, viewpoint cycle logic, and the game loop into separate classes. The `async void` game loop pattern (`Button1`, `TargetEnable`, `Vibration`, `Inertia`) should be converted to a properly cancellable `Task`-based pattern.

### Massive Volume of Commented-Out Dead Code
- **Issue:** `CmdViewpoint.cs` contains hundreds of lines of commented-out code: the abandoned `CmdInput` class, old `CanExecuteCommand` implementations, previous `PostMessage`-based keyboard injection, old momentum/inertia code, old viewpoint cycling logic, and debug form references.
- **Files:** `CtrlWizNW\CtrlWiz.NW\CmdInput.cs` (entire file commented out), `CtrlWizNW\CtrlWiz.NW\CmdViewpoint.cs` (approx. lines 1–160, 330–383, 547–565, 620–790, 987–1011, 1051–1100, 2035–2095, 2335–2496)
- **Impact:** Confuses readers about what code is active, inflates file size, and risks accidentally uncommenting stale logic.
- **Fix approach:** Delete dead code. Use git history to recover if needed. The `CmdInput.cs` file should be removed entirely or replaced with a clean empty class skeleton.

### `#region DO NOT USE` Block Still Exists in Production Code
- **Issue:** `CmdViewpoint.cs` lines 366–383 contain a `#region DO NOT USE` block with four private static methods (`CollisionView`, `GravityView`, `WalkMode`, `LookAroundMode`) that are never called.
- **Files:** `CtrlWizNW\CtrlWiz.NW\CmdViewpoint.cs`
- **Impact:** These methods call into the Navisworks UI command system by string name, making them an implicit dependency on internal Navisworks command IDs that may change between versions.
- **Fix approach:** Delete the region; the methods are superseded by the COM API approach used in `ToogleCollisionGravity`.

### `CanExecuteCommand` Instantiates and Runs a Full Command on Every Query
- **Issue:** `CmdViewpoint.CanExecuteCommand` (lines 413–436) creates a new `CmdViewpoint` instance and calls `ExecuteCommand()` on it. `CanExecuteCommand` is called by Navisworks to check enabled state on UI repaint; this triggers license verification and Paddle SDK calls on every UI refresh cycle.
- **Files:** `CtrlWizNW\CtrlWiz.NW\CmdViewpoint.cs`
- **Impact:** Potential performance degradation, unexpected side effects from license checks being triggered repeatedly, and possible errors silently suppressed into a log file.
- **Fix approach:** `CanExecuteCommand` should return a lightweight `CommandState` based on cached state (e.g., `isProductActivated`) without triggering checkout flows.

### Multiple Overloads of `ShowLicenseDetailsWindow` with Divergent Logic
- **Issue:** `InAppCheckout` has two public `ShowLicenseDetailsWindow` methods — one with an `out string message` parameter and one without — with slightly different `ActivationChanged` event firing behavior.
- **Files:** `CtrlWizLicense\InAppCheckout.cs` lines 209–246 and 303–324
- **Impact:** Divergent behavior depending on which overload is called. The first always fires `ActivationChanged` after refresh; the second does not. The Navisworks caller uses the parameterized form; the other appears unused.
- **Fix approach:** Consolidate into a single method.

### `oDoc` is a Public Static Field on CmdViewpoint
- **Issue:** `public static Document oDoc;` on `CmdViewpoint` is mutated from multiple code paths including `Button1`, `SetInitSettingUIAS`, `SetRuntimeSettingUIAS`, `SetConstantUnit`, and event handlers.
- **Files:** `CtrlWizNW\CtrlWiz.NW\CmdViewpoint.cs` line 88
- **Impact:** Navisworks can host multiple documents. If a user switches documents, `oDoc` may reference a closed document. As a static field on an instance class, all `CmdViewpoint` instances share it, which is a design error.
- **Fix approach:** Make `oDoc` an instance field resolved per-invocation via `Application.ActiveDocument`, as recommended by the Navisworks API.

### `SetConstantUnit` Uses Chained `if` with No Default Case
- **Issue:** `SetConstantUnit` (lines 673–689) uses 11 consecutive `if` statements to map the `Units` enum to a conversion factor with no else/default.
- **Files:** `CtrlWizNW\CtrlWiz.NW\CmdViewpoint.cs`
- **Impact:** If a new `Units` value is returned by a future Navisworks version, `ConstantUnit` silently remains 0 and the camera will not move.
- **Fix approach:** Use a `switch` expression with a default case that throws `NotSupportedException`, or a static dictionary lookup.

---

## Known Issues

### `CtrlWizNW` Debug Configuration Deploys to NW22 Path / v18 Bundle Subdirectory
- **Issue:** The `Debug|x64` configuration in `CtrlWizNW.csproj` hard-codes API DLL paths to `Navisworks Manage 2022` and its `OutputPath` is `..\..\..\..\..\..\ProgramData\Autodesk\ApplicationPlugins\CtrlWiz.bundle\Contents\v18\` (NW2018 subdirectory).
- **Files:** `CtrlWizNW\CtrlWizNW.csproj` lines 32–48
- **Impact:** Debug builds targeting NW25 deploy to the wrong bundle directory and link against the wrong SDK DLLs. Developers must manually select the `NW25|x64` configuration for NW25 work.
- **Fix approach:** Add a `NW25` debug configuration or make the install path a parameterizable MSBuild property.

### NW23 and NW24 Are Not Supported
- **Issue:** The solution defines configurations for NW17–NW22 and NW25, with no NW23 or NW24 configurations.
- **Files:** `CtrlWiz.sln`
- **Impact:** Users on Navisworks 2023 or 2024 have no supported build. The plugin will either fail to load or run against mismatched API assemblies in those versions.
- **Fix approach:** Add NW23 and NW24 configurations mirroring the NW22/NW25 pattern in `CtrlWizNW.csproj`, `CtrlWizNWSetup.vdproj`, and `PackageContents.xml`.

### `async void` Pattern Throughout the Game Loop
- **Issue:** `Button1`, `TargetEnable`, `Vibration`, and `Inertia` are all `async void` methods. Exceptions thrown inside `async void` propagate to the synchronization context and cannot be caught by the calling code. There is no cancellation mechanism.
- **Files:** `CtrlWizNW\CtrlWiz.NW\CmdViewpoint.cs` lines 566, 824, 855, 869
- **Impact:** Unhandled exceptions in the game loop will crash Navisworks without a useful error. When `isrun` is set to `false`, the loop terminates only at the next `await Task.Delay` boundary — there is no clean or immediate shutdown.
- **Fix approach:** Convert `Update()` to use `CancellationToken` and `Task.Run`. Replace `async void` helper methods with `async Task` where possible.

### Offline License Fallback Reads Unencrypted Boolean from Temp File
- **Issue:** When Paddle servers are unreachable, `InAppCheckout.StartPaddle` reads a raw `bool` written by `BinaryWriter` from a predictable temp path. For RVT, the path is `Path.Combine(Path.GetTempPath(), "..")` (parent of temp directory), which is fragile.
- **Files:** `CtrlWizLicense\InAppCheckout.cs` lines 152–175
- **Impact:** The `..` path construction for RVT can produce an invalid path on some Windows configurations. The unencrypted boolean can be trivially forged by any local user.
- **Fix approach:** Use a consistent `%APPDATA%\CtrlWiz\` path. Protect the offline token with DPAPI (`CredentialManagement` is already a dependency) or a signed assertion.

---

## Dependency Risks

### MvvmLight (5.4.1.1) — Abandoned
- **Risk:** Last released in 2018, officially archived. No security patches or future maintenance.
- **Files:** `CtrlWizNW.UI\packages.config`, `CtrlWizNW.UI\CtrlWizNW.UI.csproj`
- **Impact:** Bundles `System.Windows.Interactivity` 4.5.0.0 (Blend SDK assembly with redistribution constraints). No path to .NET 5+ if migration is ever attempted.
- **Migration plan:** Replace with `CommunityToolkit.Mvvm` (Microsoft-supported, MIT-licensed).

### WatiN (2.1.0) — Abandoned IE-Based Browser Automation
- **Risk:** Automates Internet Explorer, which was removed from Windows 11 in June 2022.
- **Files:** `CtrlWizLicense\CtrlWizLicense.csproj`, `CtrlWizLicense\packages.config`
- **Impact:** Any license UI flow that launched an IE-hosted view is permanently broken on Windows 11.
- **Migration plan:** Audit all usages. If `WatiN.Core` is not referenced in active runtime paths, remove the dependency. If an embedded browser is required, use `WebView2`.

### Newtonsoft.Json (11.0.2) — Three Major Versions Behind
- **Risk:** Current stable is 13.x. Version 11 has known deserialization vulnerabilities depending on usage patterns.
- **Files:** `CtrlWizLicense\packages.config`
- **Migration plan:** Update to 13.x. No breaking changes in typical property-bag deserialization usage.

### StructureMap (2.6.3) — Extremely Old IoC Container
- **Risk:** Released circa 2010. The project itself is now archived (successor is Lamar). Referenced via binding redirect in `CtrlWizNW\app.config`.
- **Files:** `CtrlWizLicense\packages.config`, `CtrlWizNW\app.config`
- **Migration plan:** Audit whether StructureMap is exercised at runtime. If used only as a transitive dependency of PaddleSDK, pin and document. If used directly, migrate to `Microsoft.Extensions.DependencyInjection`.

### InputSimulator (1.0.4.0) — Targets net20, Last Updated ~2014
- **Risk:** References `lib\net20\WindowsInput.dll` from a NuGet package unchanged since ~2014.
- **Files:** `CtrlWizNW\packages.config`, `CtrlWizNW\CtrlWizNW.csproj` line 252
- **Impact:** The `_inputSimulator` field is declared in `CmdViewpoint` but most usage appears in commented-out code. If active paths do call it, the `net20` assembly running under .NET 4.8 may have subtle compatibility issues.
- **Migration plan:** Audit live usage. If found only in dead code, remove the dependency. If needed, replace with the actively-maintained `InputSimulator.Core` package.

### Paddle.PaddleSDK (2.0.5) — Outdated, Targets net35
- **Risk:** Paddle has released a new billing model (Paddle Billing) and the classic V1 SDK is end-of-life. If Paddle deprecates V1 API endpoints, license verification and checkout will fail silently.
- **Files:** `CtrlWizLicense\packages.config`, `CtrlWizLicense\CtrlWizLicense.csproj`
- **Migration plan:** Evaluate migration to Paddle Billing API or an alternative licensing provider. At minimum, implement alerting if Paddle API calls begin returning unexpected errors.

### XInputDotNetPure — Vendored, Native DLL Loaded by Name Only
- **Risk:** `XInputDotNetPure` is a vendored project that wraps a native `XInputInterface` DLL loaded via `DllImport("XInputInterface")` with no path qualifier.
- **Files:** `XInputDotNetPure\XInputDotNetPure\Imports.cs`
- **Impact:** `XInputInterface.dll` must be on the system PATH or co-located with the plugin. If missing or bitness-mismatched, the controller fails to initialize with a `DllNotFoundException` at runtime with no user-friendly error message.
- **Fix approach:** Ensure `XInputInterface.dll` is included as a `<Content>` item in each NW configuration's output. Add a graceful startup check that detects the missing native DLL and presents a clear error to the user.

---

## Build & Deployment Risks

### Installer Not Updated for NW25; NW23/NW24 Absent
- **Issue:** `CtrlWizNWSetup.vdproj` is a legacy Visual Studio Installer project. Its file deployment tables cover through NW22. NW25 assemblies are likely not packaged.
- **Files:** `CtrlWizNWSetup\CtrlWizNWSetup.vdproj`, `CtrlWizRVTSetup\CtrlWizRVTSetup.vdproj`
- **Impact:** NW25 plugin binaries may not be included in the distributed MSI. The `.vdproj` format requires the full Visual Studio IDE to build; it is not supported by `msbuild.exe` CLI alone.
- **Fix approach:** Add NW25 output group references to the installer project. Long-term, migrate fully to the started WiX project in `SetupNwXboxWix\`.

### `Prefer32Bit=true` on x64 Library Configurations
- **Issue:** All NW17–NW25 release configurations set `<Prefer32Bit>true</Prefer32Bit>` alongside `<PlatformTarget>x64</PlatformTarget>` on a `Library` output type. The flag is silently ignored in this combination.
- **Files:** `CtrlWizNW\CtrlWizNW.csproj` lines 66, 84, 102, 120, 138, 156, 174
- **Fix approach:** Remove `<Prefer32Bit>true</Prefer32Bit>` from all x64 `PropertyGroup` blocks to eliminate confusion.

### `Release` Solution Configuration Maps to `Debug` Build for CtrlWizNW
- **Issue:** In the `.sln` project configuration mappings, `CtrlWizNW` with `Release|Any CPU` and `Release|x64` map to `Debug|Any CPU` and `Debug|x64` respectively.
- **Files:** `CtrlWiz.sln` lines 113–115
- **Impact:** Any CI pipeline or developer selecting `Release` at the solution level will silently produce a debug-optimized, unoptimized binary for the core NW plugin.
- **Fix approach:** Correct the mappings so `Release|x64` for `CtrlWizNW` maps to `NW22|x64` or a proper release configuration.

### No Automated Tests
- **Issue:** No NUnit, xUnit, or MSTest projects exist in the solution. `FullPermissionTest` is a manual executable, not a test harness.
- **Files:** Solution-wide; `FullPermissionTest\FullPermissionTest.csproj`
- **Impact:** All correctness verification is manual. Regressions in camera math, controller mapping, license logic, or new version support cannot be automatically detected.
- **Fix approach:** Begin with unit tests for `NavisUtils` math methods and `GetControllerElement` settings mapping logic, which have no Navisworks API dependencies and can be tested in isolation.

---

## Security Considerations

### License Offline Cache Is Forgeable
- **Risk:** The offline license file is a raw boolean in a predictable temp path. Any local user can replace it to bypass offline license checks.
- **Files:** `CtrlWizLicense\InAppCheckout.cs`
- **Current mitigation:** None.
- **Recommendations:** Use DPAPI-protected storage via `CredentialManagement` (already a project dependency) or Windows Credential Manager instead of a plain binary file.

### `CredentialManagement` Package Is Present but Not Used for License Caching
- **Risk:** `CredentialManagement 1.0.2` is imported but the secure storage path is not used; the offline license cache bypasses it entirely.
- **Files:** `CtrlWizLicense\packages.config`
- **Recommendations:** Route offline license token storage through `CredentialManagement` to leverage Windows Credential Manager protection.

### `InputSimulator` Injects Global Keyboard Events
- **Risk:** `WindowsInput.dll` can inject keyboard events into any application that is in focus.
- **Files:** `CtrlWizNW\CtrlWiz.NW\CmdViewpoint.cs` line 48
- **Current mitigation:** The `Update` loop compares `GetActiveAppProcessId()` against the saved Navisworks process ID and skips processing when another app has focus.
- **Recommendations:** Confirm the active-window guard is evaluated before any live (non-commented-out) `_inputSimulator` calls to ensure keystrokes are never injected into sensitive windows (e.g., password dialogs).
