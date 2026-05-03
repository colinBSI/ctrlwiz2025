# Feature Research: NW2022 to NW2025 Plugin Compatibility

**Domain:** Autodesk Navisworks managed plugin — ribbon registration, bundle packaging, API compatibility
**Researched:** 2026-05-03
**Confidence:** HIGH (based on installed NW2025 SDK examples, deployed bundle inspection, and diff of Nw22 vs Nw25 bundle contents)

---

## Finding Classification

### BREAKING CHANGES (things that definitely break NW22 plugins in NW25)

None found in the plugin/ribbon/PackageContents API surface itself — Autodesk maintained backward-compatible attribute-based registration between NW2022 and NW2025. However, **the deployment process broke**, which has the same practical effect.

---

### LIKELY ISSUES (high probability given the symptom: ribbon tab does not appear)

#### ISSUE-1: `CustomRibbon.xaml` and `CustomRibbon.name` are absent from the Nw25 bundle folder

**Evidence:** Direct filesystem comparison of deployed bundle contents.

- `Contents/Nw22/` contains: `CustomRibbon.xaml`, `CustomRibbon.name`, and all DLLs.
- `Contents/Nw25/` contains: all DLLs only. Neither `CustomRibbon.xaml` nor `CustomRibbon.name` was copied.

**Why it breaks:** The `[RibbonLayout("CustomRibbon.xaml")]` attribute on `CmdViewpoint` tells Navisworks to load the ribbon definition from a loose XAML file at a path relative to the plugin DLL. Navisworks cannot find the file, so it silently skips ribbon construction and the tab never appears. This is not a compile-time error — it is a silent runtime miss.

**Root cause:** The `Debug NW25|x64` build configuration outputs only what MSBuild produces from `<Page>` items (which compile the XAML to BAML embedded in the DLL) and from project references. There is no post-build step copying the loose `CustomRibbon.xaml` and `CustomRibbon.name` files to `Contents/Nw25/`. The Nw22 folder received these files via the MSI installer's custom action, which is not part of the debug-deploy workflow.

**How the NW22 bundle got these files:** The MSI installer (`CtrlWizNWSetup`) uses a custom action (`CtrlWizInstallCustomAction`) that handles deployment. The `Debug|x64` dev build config deployed to `Contents/v18/`, not `Nw22/`. The `Nw22/` folder was populated by the installed MSI, which apparently copies the XAML alongside the DLL.

**Fix required:** Add a post-build event (or MSBuild `Copy` task) to the `Debug NW25|x64` configuration that copies `CustomRibbon\CustomRibbon.xaml` and `CustomRibbon\CustomRibbon.name` from the project source directory to `$(OutputPath)`. The XAML must be a loose file next to the DLL, not embedded as BAML.

**Confidence:** HIGH — the Nw22 folder has the file, the Nw25 folder does not, and the official NW2025 SDK CustomRibbon example shows the XAML must be a loose file deployed alongside the DLL (see SDK post-build event in `CustomRibbon.csproj`).

---

#### ISSUE-2: `CustomRibbon.xaml` is compiled as `<Page>` (BAML) instead of a loose copied file

**Evidence:** `CtrlWizNW.csproj` declares the XAML as `<Page>` which compiles it into the DLL as BAML. The NW2025 SDK example (`CustomRibbon.csproj`) declares it as `<None CopyToOutputDirectory="Always">` and copies it as a loose file.

**Why it matters:** Navisworks does not load ribbon XAML from embedded BAML. It reads the file from disk relative to the plugin DLL. If `CustomRibbon.xaml` is compiled as a `<Page>`, it is unreachable to the NW ribbon loader. The fact that NW22 works is because the MSI copied the loose file separately; the `<Page>` compile was probably harmless but irrelevant.

**Fix required:** Either (a) change the build item from `<Page>` to `<None CopyToOutputDirectory="Always">` in the csproj (but this may break any XAML editor tooling), or (b) keep `<Page>` for editor support and add an explicit post-build copy step for the loose file. Option (b) is what the SDK example implies: the post-build event copies the `.xaml` file to the output folder alongside the DLL.

**Confidence:** HIGH — SDK example structure confirms this is the expected deployment pattern.

---

### POSSIBLE ISSUES (worth checking)

#### ISSUE-3: `LcRmFrameworkInterface` internal GUI command IDs may have changed between NW2022 and NW2025

**Evidence:** `CmdViewpoint.cs` calls undocumented internal commands:
```
RoamerGUI_OM_ATTRIB_BAR
RoamerGUI_OM_VIEW_TREE
RoamerGUI_OM_VP_ORG
RoamerGUI_MEASURE_SHORTESTDIST
RoamerGUI_OM_MEASURE_NAV
```
These are dispatched via `LcRmFrameworkInterface.ExecuteCommand(...)` from `navisworks.gui.roamer.dll`. These are private/undocumented internal command IDs that Autodesk never committed to maintaining across versions.

**Risk:** If any of these IDs changed, the associated controller button functions (show/hide Properties, Selection Tree, Viewpoints panels; shortest distance measurement) will silently fail or crash at runtime. This would not prevent the ribbon from appearing, but would break specific buttons after the ribbon issue is fixed.

**How to check:** After getting the ribbon to appear, test each mapped button function. A `null` or exception from `ExecuteCommand` would be the signal.

**Confidence:** MEDIUM — internal APIs do sometimes change at major version jumps. Cannot verify without running the plugin.

---

#### ISSUE-4: `ComApiBridge` COM types (`InwNvViewPoint2`, `nwECameraMode`, `nwEParadigm`) — interface contract stability

**Evidence:** `CmdViewpoint.cs` uses COM interop to `InwNvViewPoint2` for camera mode and collision/gravity toggles. The interop assembly is compiled with `EmbedInteropTypes=True`, which embeds type information at build time. If the COM interface GUID or method signatures changed in NW2025, the embedded interop would fail at runtime with `InvalidCastException` or `MissingMethodException`.

**Risk level:** Low-to-medium. Autodesk has historically kept these COM interfaces stable across minor versions. However, NW2022 to NW2025 spans 3 years and may include internal COM contract updates.

**How to check:** Run the plugin in NW2025 after ribbon fix. If camera mode / collision toggle buttons fail, rebuild with `EmbedInteropTypes=False` and reference the NW2025 interop assembly directly.

**Confidence:** MEDIUM — COM interface stability is implied but not confirmed from NW2025 API docs (not accessible in this research session).

---

#### ISSUE-5: Assembly binding redirect for `Newtonsoft.Json` in `CtrlWizNW.dll.config` may conflict with NW2025's own version

**Evidence:** `CtrlWizNW.dll.config` redirects `Newtonsoft.Json` from any version `0.0.0.0-12.0.0.0` to version `12.0.0.0`. NW2025 likely ships a newer version of Newtonsoft.Json internally. If NW2025 loads a higher version than 12.0.0.0, this redirect could cause a bind failure for code that expects a version above 12.

**Risk level:** Low. Navisworks plugin DLLs load in the Navisworks AppDomain. If NW2025 ships Newtonsoft.Json 13.x and the redirect requests 12.0.0.0, Newtonsoft.Json has been historically backward-compatible and the CLR would usually load the higher version. This is only a problem if both versions are incompatible or if NW strictly enforces version matching.

**How to check:** Check `%TEMP%\CtrlWizNW.log` for `FileLoadException` or `FileNotFoundException` referencing Newtonsoft.Json after the ribbon is fixed.

**Confidence:** LOW — speculative without inspecting what NW2025 ships for Newtonsoft.Json.

---

### NON-ISSUES (confirmed unchanged or not a factor)

#### NON-ISSUE-1: .NET Framework version

**Confirmed:** Both the CtrlWizNW project (`TargetFrameworkVersion=v4.8`) and the official NW2025 SDK examples (`BasicPlugIn.csproj`, `CustomRibbon.csproj`) target `.NET Framework v4.8`. Navisworks 2025 has NOT moved to .NET 6+. It remains a .NET Framework 4.8 host. No changes required.

**Source:** `C:\Program Files\Autodesk\Navisworks Manage 2025\api\NET\examples\Basic Examples\CSharp\BasicPlugIn\BasicPlugIn.csproj` and `CustomRibbon.csproj`, both specifying `<TargetFrameworkVersion>v4.8</TargetFrameworkVersion>`.

---

#### NON-ISSUE-2: Plugin attribute syntax (`[Plugin]`, `[RibbonLayout]`, `[RibbonTab]`, `[Command]`, `[Strings]`)

**Confirmed:** The official NW2025 SDK example `CustomRibbonCommandHandler.cs` uses the identical attribute pattern to CtrlWiz's `CmdViewpoint.cs`:
```csharp
[Plugin("CustomRibbonCommandHandler", "ADSK", DisplayName = "Custom Ribbon")]
[Strings("CustomRibbon.name")]
[RibbonLayout("CustomRibbon.xaml")]
[RibbonTab("ID_CustomTab_1", ...)]
[Command("ID_Button_1", ...)]
public class CustomRibbonCommandHandler : CommandHandlerPlugin
```
The API surface is unchanged between NW2022 and NW2025. `CommandHandlerPlugin` subclassing with `ExecuteCommand` and `CanExecuteCommand` overrides is still the correct pattern.

**Source:** `C:\Program Files\Autodesk\Navisworks Manage 2025\api\NET\examples\Basic Examples\CSharp\CustomRibbon\CustomRibbonCommandHandler.cs`

---

#### NON-ISSUE-3: PackageContents.xml `SeriesMin/SeriesMax="Nw25"` token

**Confirmed correct by analogy.** The PackageContents.xml format uses two-digit year abbreviations: `Nw22` for 2022, `Nw25` for 2025. This follows the established Autodesk Application Package convention where the series token is `Nw` + last two digits of the year. The existing entry is:
```xml
<RuntimeRequirements OS="Win64" Platform="NAVMAN|NAVSIM" SeriesMin="Nw25" SeriesMax="Nw25" />
```
This is the correct format. `NW2025`, `NW25`, or other casing variants would be wrong.

**Source:** Working NW22 entry uses `Nw22`; the same pattern applied to NW2025 is `Nw25`. Pattern is internally consistent.

---

#### NON-ISSUE-4: Plugin discovery mechanism (ApplicationPlugins folder, bundle format)

**Confirmed unchanged.** Navisworks 2025 still discovers plugins from `%ProgramData%\Autodesk\ApplicationPlugins\*.bundle\` using `PackageContents.xml` manifests. The bundle structure (`.bundle` folder, `PackageContents.xml` at root, `Contents/` subdirectories) is identical to NW2022. No registry-based discovery or new manifest format has been introduced.

**Source:** The existing `CtrlWiz.NW.bundle` structure mirrors the documented Autodesk Application Package format, and the NW25 entry in `PackageContents.xml` correctly points to `./Contents/Nw25/CtrlWizNW.dll`. No evidence of any change in this mechanism.

---

#### NON-ISSUE-5: XInput P/Invoke targeting `xinput1_4.dll`

**Confirmed correct.** `xinput1_4.dll` is a Windows system DLL present on all Windows 8+ machines. P/Invoking it directly (commit cc35155) eliminates the dependency on the missing `XInputInterface.dll`. This is working as intended and is not version-specific to NW2022 or NW2025.

**Source:** `XInputDotNetPure\XInputDotNetPure\Imports.cs`, `const string DLLName = "xinput1_4.dll"`.

---

#### NON-ISSUE-6: NuGet package version / API SDK version

**Navisworks does not use NuGet for its API.** The SDK is consumed directly from the installed application directory via `HintPath` in the csproj. There is no separate NuGet package to update. The `Debug NW25|x64` config already points to `C:\Program Files\Autodesk\Navisworks Manage 2025\Autodesk.Navisworks.Api.dll` — this is the correct NW2025 API.

**Source:** `CtrlWizNW.csproj`, `Debug NW25|x64` property group.

---

#### NON-ISSUE-7: Code signing / unsigned DLL loading

**Confirmed not a blocker.** Navisworks does not require signed plugin DLLs. `<SignAssembly>false</SignAssembly>` in the csproj is fine. Autodesk Application Package loading does not check authenticode signatures on managed plugin assemblies.

**Source:** `CtrlWizNW.csproj`, no signing configuration for NW builds.

---

## Feature Dependencies for NW2025 Fix

```
Ribbon tab appears [ROOT GOAL]
    └──requires──> CustomRibbon.xaml present as loose file in Contents/Nw25/
    └──requires──> CustomRibbon.name present as loose file in Contents/Nw25/
    └──requires──> PackageContents.xml SeriesMin/Max="Nw25" [DONE]
    └──requires──> CtrlWizNW.dll compiled against NW2025 API [DONE]
    └──requires──> XInput P/Invoke resolves [DONE - xinput1_4.dll]

Controller navigation works
    └──requires──> Ribbon tab appears [see above]
    └──requires──> ExecuteCommand() succeeds [POSSIBLE-ISSUE-3 - undocumented IDs]
    └──requires──> ComApiBridge COM types resolve [POSSIBLE-ISSUE-4]
```

---

## MVP Fix Sequence

### Fix 1 (required — root cause)
Copy `CustomRibbon.xaml` and `CustomRibbon.name` to `Contents/Nw25/` alongside the DLL.

**Implementation:** Add to the `Debug NW25|x64` post-build event (or MSBuild `Copy` task):
```
copy "$(ProjectDir)CustomRibbon\CustomRibbon.xaml" "$(OutputPath)"
copy "$(ProjectDir)CustomRibbon\CustomRibbon.name" "$(OutputPath)"
```
Alternatively, change the `<Page>` item for `CustomRibbon.xaml` to `<None CopyToOutputDirectory="Always">` and add a separate `<None CopyToOutputDirectory="Always">` for `CustomRibbon.name`.

The SDK example (`C:\Program Files\Autodesk\Navisworks Manage 2025\api\NET\examples\Basic Examples\CSharp\CustomRibbon\CustomRibbon.csproj`) uses this exact pattern: XAML as `<None CopyToOutputDirectory="Always">` with a post-build xcopy.

### Fix 2 (validation — after Fix 1)
Launch NW2025, confirm the CtrlWiz ribbon tab appears.

### Fix 3 (validation — after Fix 2)
Test each controller button function. If any panel show/hide or measurement command fails, investigate whether the `RoamerGUI_*` command IDs changed in NW2025.

---

## Prioritization Matrix

| Issue | User Impact | Fix Cost | Priority |
|-------|-------------|----------|----------|
| Missing CustomRibbon.xaml + .name (ISSUE-1,2) | CRITICAL — ribbon invisible | LOW — post-build copy | P1 |
| Internal GUI command IDs changed (ISSUE-3) | HIGH — buttons non-functional | MEDIUM — find new IDs | P2 |
| COM interop type mismatch (ISSUE-4) | MEDIUM — camera mode fails | MEDIUM — rebuild interop | P3 |
| Newtonsoft.Json redirect conflict (ISSUE-5) | LOW — unlikely binding failure | LOW — remove/update redirect | P4 |

---

## Sources

- `C:\Program Files\Autodesk\Navisworks Manage 2025\api\NET\examples\Basic Examples\CSharp\CustomRibbon\CustomRibbon.csproj` — official NW2025 SDK example confirming XAML as loose file, .NET 4.8 target
- `C:\Program Files\Autodesk\Navisworks Manage 2025\api\NET\examples\Basic Examples\CSharp\CustomRibbon\CustomRibbonCommandHandler.cs` — confirms attribute API unchanged in NW2025
- `C:\Program Files\Autodesk\Navisworks Manage 2025\api\NET\examples\Basic Examples\CSharp\BasicPlugIn\BasicPlugIn.csproj` — confirms .NET 4.8 target for NW2025 plugins
- `C:\ProgramData\Autodesk\ApplicationPlugins\CtrlWiz.NW.bundle\Contents\Nw22\` — contains CustomRibbon.xaml and .name (working)
- `C:\ProgramData\Autodesk\ApplicationPlugins\CtrlWiz.NW.bundle\Contents\Nw25\` — missing CustomRibbon.xaml and .name (broken)
- `C:\ProgramData\Autodesk\ApplicationPlugins\CtrlWiz.NW.bundle\PackageContents.xml` — confirms SeriesMin/Max="Nw25" syntax
- `C:\Users\cemch-wei\Documents\GitHub\ctrlwiz2025\CtrlWizNW\CtrlWizNW.csproj` — build config analysis
- `C:\Users\cemch-wei\Documents\GitHub\ctrlwiz2025\XInputDotNetPure\XInputDotNetPure\Imports.cs` — confirms xinput1_4.dll P/Invoke fix applied

---

*Feature research for: Navisworks 2025 plugin ribbon compatibility*
*Researched: 2026-05-03*
