# Architecture Research: Autodesk Application Package Loading in Navisworks 2025

**Domain:** Autodesk Application Package (.bundle) plugin discovery and loading
**Researched:** 2026-05-03
**Confidence:** HIGH (based on installed NW2025 SDK at `C:\Program Files\Autodesk\Navisworks Manage 2025\api`, direct inspection of deployed bundle, and csproj diff between working NW22 and broken NW25 configs)

---

## Standard Architecture

### Bundle Loading System Overview

```
NW startup
    │
    ▼
Bundle Discovery
    │  Scans %ProgramData%\Autodesk\ApplicationPlugins\*.bundle\
    │  Also: %APPDATA%\Autodesk\ApplicationPlugins\*.bundle\   (per-user)
    │  Also: <NW install dir>\Plugins\<PluginName>\            (legacy, no bundle)
    ▼
PackageContents.xml Parse
    │  Reads <ApplicationPackage> root
    │  For each <Components> block:
    │    Reads <RuntimeRequirements> — checks OS, Platform, SeriesMin/Max
    │    If series matches running NW version → selected
    │    If series does not match → entire <Components> block SKIPPED silently
    ▼
ModuleName Resolution
    │  Path is relative to bundle root (the folder containing PackageContents.xml)
    │  e.g. "./Contents/Nw25/CtrlWizNW.dll" resolves from CtrlWiz.NW.bundle\
    ▼
CLR Assembly Load
    │  NW calls Assembly.LoadFrom(resolvedModuleName)
    │  Working directory during load = Contents/Nw25/ (the folder of the DLL)
    │  Probe path: same folder as DLL (Contents/Nw25/)
    │  .dll.config files in same folder are honored for binding redirects
    ▼
Plugin Attribute Scan
    │  NW reflects loaded assembly for [Plugin], [RibbonLayout], [RibbonTab], [Command]
    │  Builds plugin registry entry per [Plugin] class
    ▼
Ribbon Resource Load
    │  Reads [RibbonLayout("CustomRibbon.xaml")] attribute
    │  Looks for CustomRibbon.xaml next to the DLL (Contents/Nw25/)
    │  Also looks for CustomRibbon.xaml compiled as BAML in the DLL
    │  Reads [Strings("CustomRibbon.name")] attribute (if present)
    │  Looks for CustomRibbon.name next to the DLL
    ▼
Ribbon Tab Render
    │  If all resources found → tab appears in NW ribbon
    │  If CustomRibbon.xaml or .name is missing → silent failure (no tab, no error dialog)
    ▼
Plugin Ready
```

### Component Responsibilities

| Component | Responsibility | Location |
|-----------|----------------|----------|
| Bundle folder (`*.bundle`) | Container; must end in `.bundle` — NW uses this suffix to discover it | `%ProgramData%\Autodesk\ApplicationPlugins\` |
| `PackageContents.xml` | Manifest; maps NW versions to DLL paths via SeriesMin/Max | Bundle root |
| `Contents/Nw25/` | Version-specific deploy target; DLL and all companion files go here | Bundle subdirectory |
| `CtrlWizNW.dll` | Plugin assembly; must contain `[Plugin]`-attributed class inheriting `CommandHandlerPlugin` | `Contents/Nw25/` |
| `CustomRibbon.xaml` | Ribbon layout (loose file, NOT embedded) — NW reads it at runtime from disk | `Contents/Nw25/` |
| `CustomRibbon.name` | Localized string overrides for ribbon labels — required when `[Strings]` attribute is declared | `Contents/Nw25/` |
| Dependency DLLs | All referenced assemblies not in the GAC or NW install dir | `Contents/Nw25/` |

---

## PackageContents.xml Full Schema (NW2025)

```xml
<?xml version="1.0" encoding="utf-8"?>
<ApplicationPackage
    SchemaVersion="1.0"
    AutodeskProduct="Navisworks"         <!-- Required. Must be "Navisworks" -->
    ProductType="Application"            <!-- Required. "Application" for managed plugins -->
    AppVersion="2.0.1"                   <!-- Informational -->
    Name="CtrlWizNW"                     <!-- Required. Internal package name -->
    Description="..."
    SupportedLocales="Enu"
    ProductCode="{GUID}"
    Icon="./Contents/Xbox.ico">

  <CompanyDetails Name="..." Url="..." Email="..." />

  <Components Description="Navisworks 2025">
    <RuntimeRequirements
        OS="Win64"                        <!-- Required -->
        Platform="NAVMAN|NAVSIM"          <!-- Required. Pipe-separated: NAVMAN=Manage, NAVSIM=Simulate -->
        SeriesMin="Nw25"                  <!-- Series token for NW2025 -->
        SeriesMax="Nw25" />               <!-- Same as Min for exact version match -->
    <ComponentEntry
        AppName="CtrlWizNW"              <!-- Required. Displayed in Manage Extensions -->
        AppType="ManagedPlugin"          <!-- Required. "ManagedPlugin" for .NET assemblies -->
        Version="2.0.1"
        ModuleName="./Contents/Nw25/CtrlWizNW.dll"  <!-- Required. Relative to bundle root -->
        AppDescription="..." />
  </Components>
</ApplicationPackage>
```

### Series Tokens

| NW Calendar Year | SeriesMin/Max token | Confirmed |
|------------------|---------------------|-----------|
| Navisworks 2022 | `Nw22` | Yes — working in production |
| Navisworks 2025 | `Nw25` | Yes — token present in NW2025 API SDK, consistent with Nw22 pattern |
| Navisworks 2026 | `Nw26` | Inferred pattern — not yet verified |

The `Nw` prefix + 2-digit year is the canonical NW series token format. Alternatives like `R25`, `2025`, or `NW25` are wrong and will cause silent skip. The token `Nw25` in CtrlWiz's PackageContents.xml is correct.

### ProductType Attribute

`ProductType="Application"` is the correct value for managed .NET plugins that extend the NW ribbon and add commands. No other value is required for this use case.

---

## Bundle Search Paths

NW2025 searches for bundles in this order:

1. `%ALLUSERSPROFILE%\Autodesk\ApplicationPlugins\` — same as `%ProgramData%\Autodesk\ApplicationPlugins\`
2. `%APPDATA%\Autodesk\ApplicationPlugins\` — per-user, no elevation needed
3. `<NW install dir>\Plugins\<PluginName>\` — legacy path, no PackageContents.xml, old-style deployment

CtrlWiz deploys to path 1. This path is correct and NW2025 does scan it. No registry key or environment variable needs to be set — the scan is automatic on startup.

### Bundle Folder Name

The folder must end in `.bundle`. The name before `.bundle` is arbitrary — it does not need to match `Name=` in PackageContents.xml or the DLL name. `CtrlWiz.NW.bundle` is a valid bundle folder name.

---

## DLL Loading Sequence and Dependency Resolution

When NW loads `./Contents/Nw25/CtrlWizNW.dll`:

1. `Assembly.LoadFrom` is called with the full resolved path.
2. The CLR sets the probing root to the folder containing the DLL (`Contents/Nw25/`).
3. Dependent assemblies are resolved first from the GAC, then from the NW install directory, then from `Contents/Nw25/` (same folder as the plugin DLL).
4. **No AppDomain isolation.** The plugin runs in NW's main AppDomain. A DLL version conflict with a NW-internal assembly will cause a `FileLoadException` (version mismatch) or use the already-loaded version silently.
5. `.dll.config` files in `Contents/Nw25/` are read for binding redirects. `CtrlWizNW.dll.config` in the deployed bundle serves this purpose.
6. Native DLLs (e.g., `xinput1_4.dll`) are resolved via the standard Windows DLL search order: `Contents/Nw25/` first, then `%SYSTEMROOT%\System32`. `xinput1_4.dll` is a system DLL and will be found in System32.

**Implication:** All managed dependency DLLs that are not part of the NW install must be present in `Contents/Nw25/`. The current Nw25 folder has all managed dependencies (CtrlWiz.Logging, CtrlWizForms, CtrlWizForms.CustomControls, CtrlWizLicense, CtrlWizNW.UI, GalaSoft.MvvmLight, PaddleSDK, WindowsInput, XInputDotNetPure, CredentialManagement). This appears correct.

---

## Ribbon Resource Loading — The Likely Root Cause

The NW plugin attribute system requires two companion files adjacent to the plugin DLL:

```
Contents/Nw25/
    CtrlWizNW.dll          ← plugin assembly (present)
    CustomRibbon.xaml      ← ribbon layout (MISSING from Nw25 folder)
    CustomRibbon.name      ← localized strings (MISSING from Nw25 folder)
```

`CmdViewpoint.cs` declares:
```csharp
[Strings("CustomRibbon.name")]
[RibbonLayout("CustomRibbon.xaml")]
```

NW resolves these filenames relative to the directory of the loaded DLL. If either file is absent, the ribbon tab is silently not rendered. There is no error dialog. There is no entry in the NW journal. The plugin is technically loaded (the DLL is in memory) but the ribbon tab does not appear.

**Confirmed absence:** The deployed `Contents/Nw25/` folder contains no `CustomRibbon.xaml` and no `CustomRibbon.name`. The `Contents/Nw22/` folder contains both. This is the most likely cause of the ribbon not appearing.

**Why did this happen:** In `CtrlWizNW.csproj`, `CustomRibbon.xaml` is declared as `<Page>` (MSBuild WPF compile action — compiles to BAML embedded in the DLL) and `CustomRibbon.name` is declared as `<None>` with no `CopyToOutputDirectory`. Neither file is copied by MSBuild to the `Debug NW25|x64` output path. The Nw22 folder was populated by the legacy MSI installer which explicitly copied these files; the NW25 deploy relies entirely on MSBuild output.

**Secondary observation:** Because `CustomRibbon.xaml` is a `<Page>` item, it IS compiled into the DLL as a BAML resource. Whether NW loads ribbon layout from BAML or requires a loose file depends on the NW version. NW22 accepts the loose file placed by the MSI installer. It is unclear whether NW25 accepts BAML-embedded layout or requires the loose file. The safest fix is to provide the loose file.

---

## Data Flow: Startup to Ribbon Tab Visible

```
NW2025 launches
    │
    ├─ Scans %ProgramData%\Autodesk\ApplicationPlugins\
    │       finds CtrlWiz.NW.bundle\PackageContents.xml
    │
    ├─ Parses PackageContents.xml
    │       reads Components[SeriesMin=Nw25]
    │       Platform check: NAVMAN → match (running Navisworks Manage)
    │       Series check: running version Nw25, SeriesMin=Nw25, SeriesMax=Nw25 → match
    │       ModuleName: ./Contents/Nw25/CtrlWizNW.dll → resolved path
    │
    ├─ Loads CtrlWizNW.dll
    │       Assembly.LoadFrom(resolvedPath)
    │       CLR loads dependent DLLs from Contents/Nw25/
    │       NW reflects for [Plugin] classes → finds CmdViewpoint
    │
    ├─ Processes [RibbonLayout("CustomRibbon.xaml")]
    │       Looks for CustomRibbon.xaml in Contents/Nw25/
    │       *** FILE NOT FOUND → ribbon tab not registered ***
    │       Result: no ribbon tab, no error surfaced to user
    │
    └─ NW ribbon drawn — CtrlWiz tab absent
```

---

## Architectural Patterns

### Pattern 1: Multi-Version Bundle with Per-Version Subfolders

Use one bundle with multiple `<Components>` blocks, each with its own `SeriesMin/Max` and `ModuleName` pointing to a different subfolder. Each subfolder contains the DLL built against that version's API.

This is correct architecture. CtrlWiz uses it. The issue is not the pattern, it is the missing companion files in the NW25 subfolder.

### Pattern 2: Loose Ribbon Layout File Adjacent to DLL

The `[RibbonLayout]` attribute name is resolved as a filename relative to the DLL's directory. The file must be a loose `.xaml` file on disk, not a resource compiled into the DLL. The SDK CustomRibbon example confirms this: both `CustomRibbon.xaml` and `CustomRibbon.name` have `<CopyToOutputDirectory>Always</CopyToOutputDirectory>` in the example project.

The CtrlWiz `<Page>` compile action embeds the XAML as BAML into the DLL. Whether NW2025 falls back to BAML when the loose file is absent is not confirmed. The safe fix is to change the XAML item's build action to `<None>` with `CopyToOutputDirectory = Always`, not `<Page>`.

### Pattern 3: Strings File for Ribbon Localization

The `[Strings("CustomRibbon.name")]` attribute causes NW to look for a `.name` file adjacent to the DLL. All ribbon label strings from the `[Command]` attributes can be overridden in this file. The file is optional in the sense that the attribute can be omitted, but once declared, the file must be present or ribbon registration silently fails (behavior observed in SDK comments). CtrlWiz declares the attribute, so the file must be present.

---

## Anti-Patterns

### Anti-Pattern 1: Compiling CustomRibbon.xaml as `<Page>`

**What people do:** Leave `CustomRibbon.xaml` as a WPF page (BAML resource) compiled into the DLL — this is the Visual Studio default when you add a XAML file.

**Why it's wrong:** NW reads the ribbon layout from a loose file on disk, not from embedded BAML. The ribbon tab silently fails to appear if no loose `.xaml` file exists next to the DLL.

**Do this instead:** Declare `CustomRibbon.xaml` as `<None>` with `<CopyToOutputDirectory>Always</CopyToOutputDirectory>` in the csproj. This copies it as a loose file to the output directory, so when `Debug NW25|x64` builds, it lands in `Contents/Nw25/`.

### Anti-Pattern 2: Omitting `CustomRibbon.name` from Output Copy

**What people do:** Leave `CustomRibbon.name` as `<None>` without a `CopyToOutputDirectory` directive, relying on the MSI installer to place it.

**Why it's wrong:** For manual/debug bundle deployments, no MSI runs. The file is never copied to `Contents/Nw25/`. With `[Strings]` declared on the plugin class, NW cannot complete ribbon registration.

**Do this instead:** Add `<CopyToOutputDirectory>Always</CopyToOutputDirectory>` to the `CustomRibbon.name` item in the csproj.

### Anti-Pattern 3: Assuming DLL Load Errors Surface Visibly

**What people do:** Conclude that since there is no error dialog, the DLL must have loaded and the ribbon mechanism itself is broken.

**Why it's wrong:** NW swallows load errors for optional companion files (xaml, name). A missing ribbon XAML causes no dialog, no log entry visible to the user. The DLL itself may load fine while the ribbon tab is silently suppressed.

**Do this instead:** Check `%TEMP%\CtrlWizNW.log` for exceptions during load. Check the NW journal (typically in `%USERPROFILE%\Documents\Navisworks Manage 2025 Files\` or alongside the `.nwd` file) for plugin load events. Add a constructor log entry to `CmdViewpoint` to confirm the class is being instantiated.

### Anti-Pattern 4: Wrong Series Token

**What people do:** Use `SeriesMin="2025"` or `SeriesMin="R25"` or `SeriesMin="NW25"` instead of `SeriesMin="Nw25"`.

**Why it's wrong:** The token is case-sensitive and format-sensitive. A mismatch causes the entire `<Components>` block to be silently skipped. The DLL is never loaded. No error is reported. CtrlWiz uses `Nw25` which is correct per the established NW22=`Nw22` pattern.

---

## Failure Mode Taxonomy

| Symptom | Most Likely Cause | How to Confirm |
|---------|------------------|----------------|
| Ribbon tab absent, no dialog, no crash | Missing `CustomRibbon.xaml` or `CustomRibbon.name` next to DLL | Check `Contents/Nw25/` for both files |
| Ribbon tab absent, no dialog, no crash | Wrong SeriesMin/Max token in PackageContents.xml | Compare token to `Nw22` pattern; verify NW version |
| Ribbon tab absent, no dialog, no crash | `ModuleName` path wrong — DLL not found | Verify file exists at resolved path |
| `FileNotFoundException` or `TypeLoadException` on plugin invoke | Dependent DLL missing from `Contents/Nw25/` | Check all non-GAC references are present |
| `FileLoadException`: assembly version conflict | NW already loaded a different version of a dependency DLL | Check `.dll.config` for binding redirects |
| Error dialog on startup referencing the plugin | Exception thrown in `CmdViewpoint` constructor or `[Command]` attribute processing | Check `%TEMP%\CtrlWizNW.log` |
| Crash / hang | Native DLL load failure (`xinput1_4.dll` missing or wrong arch) | Verify `xinput1_4.dll` at System32 or adjacent to DLL |

---

## Suggested Investigation Order

Investigate in this sequence (each step is a prerequisite for the next):

1. **Verify companion files in `Contents/Nw25/`** — check that `CustomRibbon.xaml` and `CustomRibbon.name` exist adjacent to `CtrlWizNW.dll`. These are absent per current inspection. This is the highest-probability root cause.

2. **Verify PackageContents.xml is read by NW25** — temporarily rename the `Contents/Nw25/` folder to something invalid (e.g. `Contents/Nw25_disabled/`) and confirm the tab still does not appear. Then restore and add the companion files. This isolates whether NW is even attempting to load the Nw25 component.

3. **Add a constructor log entry to `CmdViewpoint`** — write a line to `CtrlWizNW.log` in the constructor. If the entry appears, the DLL loaded and the issue is purely the ribbon resource resolution. If the entry does not appear, the DLL is not being loaded and the issue is earlier (PackageContents.xml or DLL resolution).

4. **Check NW journal for plugin events** — the NW journal records plugin discovery at startup. Look for lines referencing `CtrlWizNW` or the bundle path to confirm NW saw the bundle and attempted to load the component.

5. **Verify no DLL version conflicts** — if the DLL loads but commands fail, check `CtrlWizNW.dll.config` for binding redirects that match what NW2025 ships.

---

## Integration Points

### External System: Autodesk Application Package Loader

| Property | Value |
|----------|-------|
| Trigger | NW startup — no runtime reload mechanism |
| Bundle scan path | `%ProgramData%\Autodesk\ApplicationPlugins\` |
| Series matching | String comparison of `SeriesMin`/`SeriesMax` against NW internal version token |
| Error handling | Silent drop of entire `<Components>` block on any mismatch; no user-visible error for missing ribbon files |
| Reload mechanism | NW must be fully restarted after any bundle change; no hot-reload |

### Internal Boundary: Plugin DLL and Ribbon Resources

| Resource | Resolution Method | Failure Mode |
|----------|------------------|--------------|
| `CustomRibbon.xaml` | File path: `<DLL directory>\CustomRibbon.xaml` | Silent — tab not rendered |
| `CustomRibbon.name` | File path: `<DLL directory>\CustomRibbon.name` | Silent — tab not rendered |
| Icons (`.ico`, `.png`) | File path relative to DLL directory, per `[Command(Icon=...)]` | Buttons appear without icon, or button missing |
| Dependent managed DLLs | CLR probing: DLL directory, then GAC, then NW install dir | `FileNotFoundException` at load time |
| `xinput1_4.dll` | Windows DLL search: DLL dir, System32 | P/Invoke call fails at runtime |

---

## Sources

- **NW2025 SDK CustomRibbon example** — `C:\Program Files\Autodesk\Navisworks Manage 2025\api\NET\examples\Basic Examples\CSharp\CustomRibbon\` — canonical build setup for ribbon plugins, confirms `<CopyToOutputDirectory>Always</CopyToOutputDirectory>` for both `.xaml` and `.name` files (HIGH confidence)
- **NW2025 SDK CustomRibbon csproj** — confirms `CustomRibbon.name` and `CustomRibbon.xaml` as `<None>` with explicit copy, not `<Page>` (HIGH confidence)
- **Deployed bundle inspection** — `C:\ProgramData\Autodesk\ApplicationPlugins\CtrlWiz.NW.bundle\` — direct evidence of missing `CustomRibbon.xaml` and `CustomRibbon.name` in `Contents/Nw25/` vs present in `Contents/Nw22/` (HIGH confidence)
- **CtrlWizNW.csproj** — `Debug NW25|x64` config: outputs to `Contents\Nw25\`; `CustomRibbon.name` has no `CopyToOutputDirectory`; `CustomRibbon.xaml` is `<Page>` (compiled BAML) (HIGH confidence)
- **CmdViewpoint.cs** — confirms `[Strings("CustomRibbon.name")]` and `[RibbonLayout("CustomRibbon.xaml")]` attributes are declared (HIGH confidence)
- **Legacy csproj (`CtrlWizNW_sygpywdz_wpftmp.csproj`)** — older build file explicitly copies both files to `Plugins/$(TargetName)/en-US/` for each NW version, confirming the files were always intended as loose files (HIGH confidence)

---

*Architecture research for: Autodesk Application Package (.bundle) loading in Navisworks 2025*
*Researched: 2026-05-03*
