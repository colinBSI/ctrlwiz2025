# Pitfalls Research

**Domain:** Navisworks Application Package Plugin — NW2025 version targeting
**Researched:** 2026-05-03
**Confidence:** HIGH (grounded in actual deployed files and csproj; web search unavailable but findings derived from direct code inspection and known Autodesk bundle specification behavior)

---

## Critical Pitfalls

Ordered by likelihood given the specific symptom: ribbon tab does not appear in NW2025, no crash,
DLLs are present in Contents/Nw25/.

---

### Pitfall 1: DLL Load Failure Due to Missing Dependency — Silent, No Ribbon

**What goes wrong:**
Navisworks loads the bundle manifest, resolves the `ManagedPlugin` entry for Nw25, attempts to
load `CtrlWizNW.dll`, and the CLR fails to resolve one of its dependencies. The plugin silently
disappears — no ribbon tab, no error dialog, no journal entry visible to the user. This is the
highest-probability cause of the current symptom.

**Why it happens:**
`CtrlWizNW.dll` has a broad dependency graph: `CtrlWiz.Logging`, `CtrlWizForms`,
`CtrlWizLicense`, `CtrlWizNW.UI`, `XInputDotNetPure`, `WindowsInput`, `PaddleSDK`,
`CredentialManagement`, `GalaSoft.MvvmLight`, and `WatiN.Core`. Every one of these must be
co-located in `Contents/Nw25/` because the plugin folder is not on the GAC or the process's
default probing path. Navisworks does not log dependency resolution failures to any user-visible
surface; the CLR's fusion log is the only place they appear.

The `Debug NW25|x64` build config copies outputs from the main project and its project
references, but NuGet package DLLs are only copied if their `CopyLocalLockFileAssemblies` or
`Private` flags allow it. If any NuGet-sourced DLL was missed by the build's copy-local
mechanism, it lands in the release assembly output but not in the bundle.

**How to avoid:**
1. Enable CLR Fusion logging before launching NW2025:

   Open registry: `HKLM\SOFTWARE\Microsoft\Fusion`
   Set `EnableLog = 1` (DWORD), `LogPath = C:\FusionLog\` (String), `ForceLog = 1` (DWORD),
   `LogFailures = 1` (DWORD). Launch NW2025, then inspect the fusion log folder for any
   `.htm` file showing a bind failure on a CtrlWiz-related assembly.

2. Cross-check the Nw25 folder against the Nw22 folder. Every DLL in Nw22 that is not a
   Navisworks API DLL (those are `Private=False` and provided by the host) must also be
   present in Nw25.

3. Current Nw25 DLL inventory (verified present):
   `CtrlWizNW.dll`, `CtrlWizNW.UI.dll`, `CtrlWizLicense.dll`, `CtrlWiz.Logging.dll`,
   `CtrlWizForms.dll`, `CtrlWizForms.CustomControls.dll`, `XInputDotNetPure.dll`,
   `WindowsInput.dll`, `PaddleSDK.dll`, `GalaSoft.MvvmLight.dll`, `CredentialManagement.dll`

   Missing from the above list that may be required: `WatiN.Core.dll`, `Interop.SHDocVw.dll`,
   `Microsoft.mshtml.dll`. These are dead code but are declared as references in
   `CtrlWizLicense.csproj` with `EmbedInteropTypes=True`. The interop types are embedded so
   no separate DLL is needed for those, but `WatiN.Core.dll` itself may still be expected.
   Verify this specific DLL is not causing a bind failure.

**Warning signs:**
- Ribbon absent, no crash, no error dialog — this is the canonical symptom of a CLR bind failure
- Nw22 ribbon works but Nw25 does not, despite the PackageContents.xml Nw25 entry being correct
- Fusion log shows `FileNotFoundException` or `FileLoadException` on any assembly

**Phase to address:** Phase 1 (Diagnosis) — enable fusion log before doing anything else.

---

### Pitfall 2: `navisworks.gui.roamer.dll` Internal Type Changes Between NW22 and NW25

**What goes wrong:**
`CustomRibbon.xaml` declares a namespace binding to
`Autodesk.Navisworks.Gui.Roamer.AIRLook;assembly=navisworks.gui.roamer` and uses
`local:NWRibbonButton` and `local:NWRibbonMenuButton`. These types are in an Autodesk-internal
assembly (`navisworks.gui.roamer.dll`) that is not part of the public API and changes between
NW versions without notice.

If `NWRibbonButton` was renamed, moved to a different namespace, or its constructor signature
changed in NW2025, the XAML parser throws a `XamlParseException` during ribbon construction.
This exception is caught by the Navisworks plugin host, which discards the plugin silently. The
ribbon tab never appears.

**Why it happens:**
The plugin uses `[RibbonLayout("CustomRibbon.xaml")]` which tells Navisworks to parse the XAML
file. The XAML file references internal Navisworks GUI types by CLR namespace. These types are
not versioned or guaranteed stable across years. NW22 to NW25 is a three-version jump.

**How to avoid:**
1. Check whether `navisworks.gui.roamer.dll` in NW2025's install directory exports
   `Autodesk.Navisworks.Gui.Roamer.AIRLook.NWRibbonButton`. Use ILSpy or:
   ```
   [System.Reflection.Assembly]::LoadFrom("C:\Program Files\Autodesk\Navisworks Manage 2025\navisworks.gui.roamer.dll").GetTypes() | Where-Object { $_.Name -like "*Ribbon*" }
   ```
2. If the type is absent or in a different namespace, update `CustomRibbon.xaml`'s namespace
   declaration to match NW2025's actual namespace.
3. Wrap the ribbon loading in a try-catch and log to `%TEMP%\CtrlWizNW.log` — currently the
   plugin's exception logging is only called from command handlers, not from the ribbon
   initialization path.

**Warning signs:**
- XAML namespace or type errors in the Navisworks journal file
  (`%LOCALAPPDATA%\Autodesk\Navisworks Manage 2025\*.log`)
- Event Viewer → Application log showing a `XamlParseException` from Navisworks
- The NW22 XAML works fine but NW25 XAML fails, despite identical file

**Phase to address:** Phase 1 (Diagnosis) — inspect navisworks.gui.roamer.dll types in NW2025;
Phase 2 (Fix) — update XAML namespace if types moved.

---

### Pitfall 3: `navisworks.gui.interface.dll` Command ID Changes — Internal API Breakage

**What goes wrong:**
`CmdViewpoint.cs` calls `LcRmFrameworkInterface.ExecuteCommand(...)` with hardcoded string
command IDs (`"RoamerGUI_OM_ATTRIB_BAR"`, `"RoamerGUI_OM_VIEW_TREE"`, etc.) from
`navisworks.gui.interface.dll`. These are undocumented internal IDs. If NW2025 renamed or
removed any of these commands, the call throws an exception inside the plugin's execution path.

Unlike a load-time failure (which prevents the ribbon from appearing at all), this failure
occurs at command-execution time. However, if the exception propagates during plugin
initialization before the ribbon is returned, the ribbon will not appear.

**Why it happens:**
The codebase uses internal, unsupported Navisworks command strings that Autodesk does not
version-stabilize. This is a known fragile pattern explicitly flagged in the codebase's own
INTEGRATIONS.md: "These are undocumented internal command IDs that show/hide panels and trigger
measurement tools. They are fragile and may change across Navisworks versions."

**How to avoid:**
Guard every `LcRmFrameworkInterface.ExecuteCommand(...)` call with a try-catch. If the command
ID is invalid, Navisworks throws — catch and log, then continue. The ribbon itself should not
be affected unless these calls happen in the constructor or static initializer.

**Warning signs:**
- Ribbon appears but specific buttons (Show Properties, Selection Tree, Shortest Distance) throw
  unhandled exceptions
- `%TEMP%\CtrlWizNW.log` shows exceptions from the internal command dispatch path

**Phase to address:** Phase 2 (Stabilization) — add defensive try-catch around all internal
command calls.

---

### Pitfall 4: `PackageContents.xml` `AppName` Collision — Duplicate Plugin ID Rejection

**What goes wrong:**
Both the Nw22 and Nw25 `ComponentEntry` elements use `AppName="CtrlWizNW"`. The Autodesk
Application Package loader may reject a manifest where two `ComponentEntry` elements share the
same `AppName` and both are candidates for the running host version (Nw25). If only one
`SeriesMin/Max` range matches the running NW version this is not a problem, but if the loader
performs name-uniqueness validation globally across all `Components` blocks, both entries would
conflict.

**Why it happens:**
The AppName was simply duplicated from the Nw22 entry without considering whether Autodesk's
manifest parser enforces uniqueness. The Autodesk bundle specification is not publicly precise
on this point.

**How to avoid:**
Change the Nw25 `ComponentEntry` `AppName` to something unique, e.g. `AppName="CtrlWizNW25"`,
to eliminate any possibility of a name collision. This is a low-risk one-attribute change.

**Warning signs:**
- No ribbon in NW25, but NW22 ribbon still works
- Removing the Nw22 `Components` block causes NW25 to load the plugin successfully (would
  confirm name collision as the cause)

**Phase to address:** Phase 1 (Diagnosis) — test by removing Nw22 component temporarily;
Phase 1 (Fix) — rename AppName to be unique per version.

---

### Pitfall 5: `Debug NW25|x64` Config Compiles Against NW2025 API But `PlatformTarget` Is x64 While Old `Debug|x64` Was AnyCPU

**What goes wrong:**
The old `Debug|x64` config has `PlatformTarget=AnyCPU`. The new `Debug NW25|x64` config has
`PlatformTarget=x64`. If any of the referenced managed DLLs in the dependency chain (e.g.,
`WindowsInput.dll` from InputSimulator 1.0.4, which declares `processorArchitecture=MSIL`)
have bitness constraints that conflict, the CLR will refuse to load the mixed assembly graph.

More subtly, `XInputDotNetPure` compiled as AnyCPU but calling `xinput1_4.dll` via P/Invoke
will work correctly in a 64-bit host process. Navisworks 2025 is 64-bit. This is not an issue
as long as the managed assemblies are AnyCPU or x64; an x86-only managed DLL in the chain
would hard-fail.

**How to avoid:**
Verify that every DLL in Contents/Nw25/ is either AnyCPU or x64 using:
```powershell
Get-ChildItem "C:\ProgramData\Autodesk\ApplicationPlugins\CtrlWiz.NW.bundle\Contents\Nw25\*.dll" |
  ForEach-Object { [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($_.FullName).GetName() } |
  Select-Object Name, ProcessorArchitecture
```
Any DLL reporting `X86` will cause a load failure in the 64-bit NW2025 process.

**Warning signs:**
- `BadImageFormatException` in the Navisworks journal or Windows Event Log
- Fusion log showing processor architecture mismatch

**Phase to address:** Phase 1 (Diagnosis) — bitness check all DLLs.

---

### Pitfall 6: Windows Zone-of-Origin Block ("Internet Zone" Mark-of-the-Web)

**What goes wrong:**
DLLs downloaded from a network share, the internet, or email attachments are tagged with an
Alternate Data Stream `Zone.Identifier` by Windows. When the CLR loads a DLL with this tag,
.NET Framework may throw a `FileLoadException` with "Operation is not supported" — the DLL is
treated as coming from the internet zone and cannot be loaded as a managed assembly in a
partially-trusted context.

**Why it happens:**
If the plugin DLLs were copied to `Contents/Nw25/` from a network share (e.g., GitHub clone
on a network drive, or downloaded ZIP), Windows attaches the zone identifier automatically.

**How to avoid:**
Unblock all DLLs before deployment:
```powershell
Get-ChildItem "C:\ProgramData\Autodesk\ApplicationPlugins\CtrlWiz.NW.bundle\Contents\Nw25\*.dll" |
  Unblock-File
```
This requires elevation (Admin By Request). The `Debug NW25|x64` build config builds locally
and copies via MSBuild post-build, so DLLs produced by a local build should not be blocked.
Only applies if DLLs were obtained by other means (e.g., extracted from a ZIP).

**Warning signs:**
- DLLs present and correct bitness, but CLR refuses to load them
- Right-click → Properties on a DLL in Windows Explorer shows "This file came from another
  computer" with an "Unblock" checkbox

**Phase to address:** Phase 1 (Diagnosis) — check file properties; unblock if needed.

---

### Pitfall 7: Bundle Folder Name Must End in `.bundle` — Loader Ignores Non-Compliant Names

**What goes wrong:**
The Autodesk Application Package loader scans `%ProgramData%\Autodesk\ApplicationPlugins\` for
directories whose name ends in `.bundle`. A folder without this suffix is silently ignored —
no `PackageContents.xml` inside it will ever be read.

**Why it happens:**
The deployed folder is `CtrlWiz.NW.bundle` which is correct. However, there are two bundle
name variants in the codebase: the old `Debug|x64` config deploys to
`CtrlWiz.bundle\Contents\v18\`, while the actual deployed path is
`CtrlWiz.NW.bundle\Contents\Nw25\`. If someone ever used the old `Debug|x64` config and looked
at the wrong bundle folder, they would see correct-seeming DLLs in a bundle Navisworks is not
scanning.

**How to avoid:**
Verify the bundle folder name in use is exactly `CtrlWiz.NW.bundle` and that
`PackageContents.xml` is at the root of that folder (not inside Contents/). The deployed file
at `C:\ProgramData\Autodesk\ApplicationPlugins\CtrlWiz.NW.bundle\PackageContents.xml` is
confirmed present and correct.

Also retire or rename the old `Debug|x64` build config to prevent confusion — it still deploys
to `CtrlWiz.bundle\Contents\v18\` which is a stale/wrong path.

**Warning signs:**
- Bundle folder exists but no plugin loads at all
- `CtrlWiz.bundle` folder exists alongside `CtrlWiz.NW.bundle` — stale deployment confusion

**Phase to address:** Phase 1 (Diagnosis) — confirm which bundle folder NW2025 is reading;
clean up stale `CtrlWiz.bundle` if it exists.

---

### Pitfall 8: `SeriesMin`/`SeriesMax` Token Exact Spelling — Case-Sensitive

**What goes wrong:**
The Autodesk bundle loader uses exact string matching for the `SeriesMin` and `SeriesMax`
attributes. The documented token for Navisworks 2025 is `Nw25`. Using `NW25`, `nw25`, `NW2025`,
or `Navisworks2025` causes the entry to never match, and the component is silently skipped.

**Why it happens:**
Developers often guess the token from the product year (e.g., typing `NW2025`) or copy from an
unofficial example. The official token strings are:
`Nw17`, `Nw18`, `Nw19`, `Nw20`, `Nw21`, `Nw22`, `Nw25` — note the gap from Nw22 to Nw25,
and the exact mixed-case `Nw` prefix with two-digit year suffix.

**Current state:** The deployed `PackageContents.xml` uses `SeriesMin="Nw25" SeriesMax="Nw25"`
which is the correct token. This pitfall is already correctly handled.

**Warning signs:**
- Component entry absent for the running NW version
- Navisworks loads the bundle but skips the component (no ribbon, no error)

**Phase to address:** Already addressed. Verify by confirming exact casing in the deployed file.

---

### Pitfall 9: API Version Mismatch — Compiled Against NW2022 DLLs, Running Under NW2025

**What goes wrong:**
`CtrlWizNW.dll` built with the `Debug NW25|x64` config correctly references NW2025 API DLLs.
However, if someone accidentally builds with `Debug|x64` (the old config, which resolves against
NW2022 API DLLs but outputs to `CtrlWiz.bundle\Contents\v18\`) and manually copies those DLLs
to `Contents/Nw25/`, the plugin would be compiled against the NW2022 API assembly version.
The NW2025 host provides its own API assemblies from its install directory; if the plugin was
compiled against NW2022 API DLLs whose public key token or assembly version differ from NW2025,
the CLR will reject the plugin at load time.

In practice, Navisworks API DLLs use `Private=False` (copy-local disabled) so the host's own
DLLs are used at runtime. The risk is specifically if the API surface changed in a way that
breaks binary compatibility (type forwarding, removed members), not just version number changes.

**How to avoid:**
Always build NW25 content using the `Debug NW25|x64` config. Never copy NW22-built DLLs to
Contents/Nw25/. The csproj correctly separates configs; just use the right one.

**Warning signs:**
- `MissingMethodException` or `TypeLoadException` in the fusion log pointing to a Navisworks
  API type
- Plugin DLL timestamp significantly older than the most recent NW25 build

**Phase to address:** Phase 1 (Diagnosis) — verify DLL timestamps match the most recent
`Debug NW25|x64` build.

---

### Pitfall 10: `[Plugin]` Attribute ID Format — Vendor Code Must Match Exactly

**What goes wrong:**
The plugin is registered as `[Plugin("NVSXBOX.CmdViewpoint", "VIATechnik", ...)]`. The second
parameter (`"VIATechnik"`) is the developer code. Navisworks uses the combination
`<DeveloperCode>.<PluginName>` as a globally unique plugin identifier. If this ID is already
registered by a previous plugin load (e.g., a cached NW22 registration) with conflicting
metadata, NW25 may refuse to re-register it.

**Why it happens:**
Navisworks caches plugin metadata between sessions. If the NW22 plugin loaded previously and
left cached state, and the NW25 plugin attempts to register under the same `NVSXBOX.CmdViewpoint`
ID, the cache collision could prevent registration.

**How to avoid:**
Clear the Navisworks plugin cache by deleting:
`%APPDATA%\Autodesk\Navisworks Manage 2025\Roaming\Plugins\` (if this folder exists)
and any `.cache` files under:
`%LOCALAPPDATA%\Autodesk\Navisworks Manage 2025\`

This is distinct from the NW22 cache because NW versions maintain separate profile directories.
Within NW25 itself, there should be no ID conflict since only one `NVSXBOX.CmdViewpoint` is
loaded.

**Warning signs:**
- Plugin worked once, stopped working after reinstall without a clean cache purge
- Duplicate plugin registration messages in the Navisworks journal

**Phase to address:** Phase 1 (Diagnosis) — rule out cache issues by testing with a clean
Navisworks profile.

---

### Pitfall 11: `EmbedInteropTypes=True` and COM Type Equivalence Across Assembly Versions

**What goes wrong:**
`Autodesk.Navisworks.Interop.ComApi.dll` is referenced with `EmbedInteropTypes=True`. This
embeds the COM type information directly into `CtrlWizNW.dll` at compile time. If the NW2025
version of `Autodesk.Navisworks.ComApi.dll` has different COM interface IIDs (Interface
Identifiers) or GUID-attributed types compared to NW2022, the embedded type equivalence
mechanism may fail at runtime when the CLR tries to match the embedded COM type definitions
against the runtime COM objects provided by NW2025.

**Why it happens:**
COM type embedding works by type equivalence via interface GUIDs. If Autodesk changed any COM
interface GUID between NW22 and NW25 for types used by the plugin
(`InwNvViewPoint2`, `nwECameraMode`, `nwEParadigm`, `InwOpState10`), the embedded type in
the compiled DLL will not match the runtime COM object, causing a `COMException` or
`InvalidCastException`.

**How to avoid:**
Verify that the NW2025 ComApi types the plugin uses (specifically `InwNvViewPoint2` and
`nwEParadigm`) have the same GUIDs as their NW2022 counterparts using ILSpy on both versions'
`Autodesk.Navisworks.ComApi.dll`. The `[Plugin]` loading will succeed but the first COM cast
inside a command handler will throw if GUIDs changed.

**Warning signs:**
- Ribbon appears but clicking "Activate Controller" throws an unhandled exception
- `InvalidCastException` or `COMException` in `%TEMP%\CtrlWizNW.log` referencing
  `InwNvViewPoint2` or `ComApiBridge`

**Phase to address:** Phase 2 (Navigation Functionality) — this does not affect ribbon
appearance, only command execution.

---

## Technical Debt Patterns

| Shortcut | Immediate Benefit | Long-term Cost | When Acceptable |
|----------|-------------------|----------------|-----------------|
| Internal `navisworks.gui.roamer.dll` types in XAML | Ribbon layout reuses NW's own UI components | XAML breaks silently whenever NW updates these internal types; no migration path | Never — should be eliminated once ribbon appears |
| Hardcoded `LcRmFrameworkInterface` command string IDs | One-line access to NW panel toggles | Any NW version update can rename these IDs; failures are silent | Never — should be wrapped in defensive try-catch now |
| `WatiN.Core.dll` + `CredentialManagement.dll` in dependency chain | Leftover from earlier implementation | Unnecessary DLLs to deploy; WatiN requires IE which is absent on Windows 11; increases surface area for load failures | Never — should be removed from csproj references |
| `Debug|x64` config still targeting NW2022 and old bundle path | Existing config left intact | Risk of deploying wrong DLLs to wrong path; confuses diagnostic work | Never — should be retired or clearly documented as legacy |
| No exception logging during plugin load / ribbon initialization | Simpler code | Load failures produce no diagnostic output; `%TEMP%\CtrlWizNW.log` is empty when the plugin never loads | Never — Logger.SetUpLogger should be called from the plugin class constructor |

---

## Integration Gotchas

| Integration | Common Mistake | Correct Approach |
|-------------|----------------|------------------|
| Autodesk Bundle Manifest | Assuming `ComponentEntry` with matching `SeriesMin/Max` is sufficient — ignores potential AppName conflicts | Use a unique `AppName` per version component entry (`CtrlWizNW22`, `CtrlWizNW25`) |
| Navisworks Internal XAML Types | Copying `CustomRibbon.xaml` from NW22 directly to NW25 without verifying type availability | Inspect `navisworks.gui.roamer.dll` exports in NW2025 before assuming types exist |
| CLR Fusion (DLL resolution) | Assuming "DLLs present" means "DLLs loadable" | Enable fusion logging; presence and loadability are separate conditions |
| `EmbedInteropTypes` COM interop | Assuming COM type embedding is version-agnostic | Verify interface GUIDs match between NW22 and NW25 ComApi assemblies |
| XInput P/Invoke | Using `xinput1_3.dll` (old default) | Use `xinput1_4.dll` — always present on Win8+ systems; already fixed in commit cc35155 |
| Navisworks Journal Logging | Not checking the NW journal for plugin load failures | Check `%LOCALAPPDATA%\Autodesk\Navisworks Manage 2025\` for `.log` files on every diagnostic session |

---

## "Looks Done But Isn't" Checklist

- [ ] **PackageContents.xml deployed**: Confirm the file at `CtrlWiz.NW.bundle\PackageContents.xml` is the one from source (not a stale version) — check file timestamp and MD5 against the repo copy
- [ ] **DLLs loadable (not just present)**: Enable fusion logging and launch NW2025 once before declaring DLL deployment complete
- [ ] **XAML types verified in NW2025**: Confirm `NWRibbonButton` exists in the NW2025 version of `navisworks.gui.roamer.dll`
- [ ] **No Zone-of-Origin blocks**: Right-click each DLL in Contents/Nw25/ and check for "Unblock" checkbox
- [ ] **Correct build config used**: Confirm the DLLs in Contents/Nw25/ have timestamps matching the most recent `Debug NW25|x64` build, not a `Debug|x64` (NW2022) build
- [ ] **Logger initialized at startup**: Verify `Logger.SetUpLogger(LoggedApp.NW)` is called during plugin load, not just inside command handlers, so load-time failures produce diagnostic output
- [ ] **Navisworks journal checked**: Open NW2025, let it fully start, close it, then read the journal — any plugin load error appears there

---

## Recovery Strategies

| Pitfall | Recovery Cost | Recovery Steps |
|---------|---------------|----------------|
| Missing dependency DLL | LOW | Copy the missing DLL to Contents/Nw25/ and restart NW2025 |
| XAML type not found in NW2025 | MEDIUM | Update `CustomRibbon.xaml` namespace or replace `local:NWRibbonButton` with pure WPF `Button` type |
| Zone-of-Origin block | LOW | `Unblock-File` on all DLLs (needs elevation); restart NW2025 |
| AppName collision in manifest | LOW | Change `AppName` attribute on Nw25 `ComponentEntry`; restart NW2025 |
| Stale NW22 DLLs in Nw25 folder | LOW | Rebuild with `Debug NW25|x64`; redeploy |
| COM type GUID mismatch | HIGH | Refactor COM usage to use managed API equivalents or rebuild against NW2025 ComApi and test each COM cast |
| Internal command IDs renamed | MEDIUM | Identify new command ID strings for NW2025; wrap all calls in try-catch to prevent crash on unknown IDs |

---

## Pitfall-to-Phase Mapping

| Pitfall | Investigation Phase | Verification |
|---------|---------------------|--------------|
| Missing dependency DLL (Pitfall 1) | Phase 1: Diagnosis | Fusion log shows no bind failures; all DLLs from Nw22 folder present in Nw25 folder |
| XAML internal type missing (Pitfall 2) | Phase 1: Diagnosis | NWRibbonButton confirmed present in NW2025 roamer DLL |
| Internal command ID breakage (Pitfall 3) | Phase 2: Stabilization | All `ExecuteCommand` calls wrapped in try-catch; no exceptions in log |
| AppName collision in manifest (Pitfall 4) | Phase 1: Diagnosis | AppName made unique; ribbon appears after manifest change |
| Bitness mismatch (Pitfall 5) | Phase 1: Diagnosis | All Nw25 DLLs are AnyCPU or x64 |
| Zone-of-Origin block (Pitfall 6) | Phase 1: Diagnosis | All DLLs unblocked; no zone identifier ADS present |
| Bundle folder name (Pitfall 7) | Phase 1: Diagnosis | Confirmed CtrlWiz.NW.bundle is the active bundle path |
| SeriesMin/Max token (Pitfall 8) | Already addressed | Confirmed `Nw25` casing in deployed PackageContents.xml |
| API version mismatch (Pitfall 9) | Phase 1: Diagnosis | DLL timestamps match Debug NW25|x64 build |
| Plugin ID cache collision (Pitfall 10) | Phase 1: Diagnosis | Tested with clean NW2025 profile |
| COM type equivalence (Pitfall 11) | Phase 2: Navigation | Controller activation works without COMException |

---

## Diagnostic Priority Order

Given the exact symptom (ribbon absent, no crash, DLLs present), investigate in this order:

1. **Check Navisworks journal** — `%LOCALAPPDATA%\Autodesk\Navisworks Manage 2025\*.log`
   One line. Takes 30 seconds. Often contains the exact failure reason.

2. **Enable CLR Fusion logging** — captures any DLL bind failure the journal may not surface.
   Fastest way to confirm Pitfall 1.

3. **Inspect `navisworks.gui.roamer.dll` types in NW2025** — confirm `NWRibbonButton` exists.
   Takes 2 minutes with PowerShell + reflection. Rules out Pitfall 2.

4. **Test with AppName made unique** — change Nw25 AppName, restart NW2025.
   Rules out Pitfall 4 with a trivial one-attribute edit.

5. **Check Zone-of-Origin** — right-click DLLs in File Explorer.
   Rules out Pitfall 6 in 60 seconds.

6. **Confirm DLL timestamps** — compare Contents/Nw25/ file dates to most recent build output.
   Rules out Pitfall 9 (wrong DLLs deployed).

---

## Sources

- Direct inspection of `CtrlWizNW\CtrlWizNW.csproj` — build configs, API reference paths,
  Private=False settings, PlatformTarget per config
- Direct inspection of deployed `PackageContents.xml` at
  `C:\ProgramData\Autodesk\ApplicationPlugins\CtrlWiz.NW.bundle\PackageContents.xml`
- Direct inspection of `CtrlWizNW\CustomRibbon\CustomRibbon.xaml` — XAML namespace and type usage
- Direct inspection of `XInputDotNetPure\XInputDotNetPure\Imports.cs` — P/Invoke target DLL
- Direct inspection of `CtrlWizNW\CtrlWiz.NW\CmdViewpoint.cs` — Plugin attributes, command
  structure, P/Invoke usage
- `.planning\codebase\INTEGRATIONS.md` — dependency graph, COM interop, internal command IDs
- `.planning\codebase\ARCHITECTURE.md` — layer boundaries, entry point class
- Autodesk Application Package manifest specification (HIGH confidence on SeriesMin/Max token
  format — derived from existing working Nw22 entry pattern; Nw25 token follows same convention)
- CLR Assembly Binding Log behavior (HIGH confidence — standard .NET Framework diagnostic tool,
  unchanged across NW versions)
- Windows Zone-of-Origin / Mark-of-the-Web behavior (HIGH confidence — standard Windows security
  feature for downloaded files)

---
*Pitfalls research for: Navisworks 2025 plugin — ribbon tab absent diagnostic*
*Researched: 2026-05-03*
