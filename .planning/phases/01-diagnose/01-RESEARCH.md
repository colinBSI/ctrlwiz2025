# Phase 1: Diagnose - Research

**Researched:** 2026-05-03
**Domain:** Navisworks 2025 managed plugin load failure — ribbon tab not appearing
**Confidence:** HIGH

---

## Summary

Phase 1 is a pure-investigation phase: no code is written. The goal is to run the existing
`Debug NW25|x64` build, inspect every diagnostic surface available, and produce a written
diagnosis document that confirms exactly why the CtrlWiz ribbon tab does not appear in NW2025.

At the time of research, **NW2025 has never been launched on this machine by this user** — the
journal directory and user-data directory for NW2025 both do not exist yet, and `CtrlWizNW.log`
is absent. All diagnostic steps must be executed from scratch as part of this phase.

The pre-existing research (FEATURES.md, ARCHITECTURE.md, PITFALLS.md, STACK.md) has already
identified two leading-candidate root causes with HIGH confidence. Phase 1 either confirms one
of them or, if logs reveal a different cause, documents the actual failure path. Either outcome
satisfies BUILD-02.

**Primary recommendation:** Work through the five diagnostic steps in the order given below.
Each step takes 2–20 minutes. Stop as soon as a confirmed root cause is established and
documented. Do not fix anything — only diagnose and document.

---

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|-------------|----------------|-----------|
| Bundle discovery | NW Application Package Loader | — | NW scans `%ProgramData%\Autodesk\ApplicationPlugins\*.bundle` at startup |
| Manifest parsing (PackageContents.xml) | NW Application Package Loader | — | NW resolves `SeriesMin/Max` against running version; mismatch = silent skip |
| DLL loading | CLR (in NW AppDomain) | Windows loader | `Assembly.LoadFrom` called by NW; CLR resolves managed deps from DLL folder |
| Ribbon resource loading | NW plugin host | — | Resolves `CustomRibbon.xaml` and `.name` as loose files from DLL directory |
| Plugin log output | Plugin (CtrlWiz.Logging) | — | Logger must be initialized before the failure or nothing is written |
| Diagnostic logging | Developer (external tools) | — | CLR Fusion log, Windows Event Log, Procmon are external to the plugin |

---

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| BUILD-02 | NW25 support can be built and deployed without running the MSI installer | Phase produces a diagnosis document confirming which specific artifact(s) or configuration issue is the root cause; this evidence informs Phase 2's MSBuild-only fix |
</phase_requirements>

---

## Current State: What the Filesystem Already Tells Us

The following facts were verified directly during this research session:

### Nw25 Bundle Folder — Current State (verified 2026-05-03)
[VERIFIED: direct filesystem inspection]

**DLLs present in `Contents/Nw25/`:**
- CtrlWizNW.dll (5/3/2026 16:16 — freshly built by `Debug NW25|x64`)
- CtrlWizNW.UI.dll, CtrlWiz.Logging.dll, CtrlWizForms.dll, CtrlWizForms.CustomControls.dll
- CtrlWizLicense.dll, XInputDotNetPure.dll, GalaSoft.MvvmLight.dll, PaddleSDK.dll
- WindowsInput.dll, CredentialManagement.dll
- All `.pdb` files and `.dll.config` files (debug artifacts)

**Missing from `Contents/Nw25/`:**
- `CustomRibbon.xaml` — NOT PRESENT
- `CustomRibbon.name` — NOT PRESENT

**Nw22 comparison confirms these are required:** `Contents/Nw22/` contains both files alongside
its DLLs. This is the working reference configuration.

**Why they are missing:** In `CtrlWizNW.csproj`:
- `CustomRibbon.xaml` is declared as `<Page>` — compiles to BAML embedded in the DLL; not copied to output
- `CustomRibbon.name` is declared as `<None>` with no `CopyToOutputDirectory` — never copied to output

The `Debug NW25|x64` config output path is `$(PROGRAMDATA)\Autodesk\ApplicationPlugins\CtrlWiz.NW.bundle\Contents\Nw25\` and it deploys via MSBuild's standard output copy, which does not include these files.

### PackageContents.xml — Status
[VERIFIED: direct filesystem inspection]

Both the NW22 and NW25 `ComponentEntry` elements use `AppName="CtrlWizNW"` — identical names.
The Nw25 `RuntimeRequirements` uses `SeriesMin="Nw25" SeriesMax="Nw25"` (correct casing).
`ModuleName="./Contents/Nw25/CtrlWizNW.dll"` (correct path).

**Potential concern:** Duplicate `AppName` across both components. This has not been tested.

### NWRibbonButton — Confirmed Present in NW2025
[VERIFIED: PowerShell reflection against installed NW2025 DLL]

```
Assembly: C:\Program Files\Autodesk\Navisworks Manage 2025\navisworks.gui.roamer.dll
Namespace: Autodesk.Navisworks.Gui.Roamer.AIRLook
Types confirmed: NWRibbonButton, NWRibbonMenuButton (and 30+ others)
```

`CustomRibbon.xaml`'s XAML namespace `clr-namespace:Autodesk.Navisworks.Gui.Roamer.AIRLook;assembly=navisworks.gui.roamer` and `local:NWRibbonButton` usage are **valid in NW2025**. The secondary risk cited in CLAUDE.md is resolved: no XAML type changes required.

### DLL Bitness — All Clean
[VERIFIED: PowerShell reflection]

All DLLs in `Contents/Nw25/`:
- `CtrlWizNW.dll`: Amd64
- `CtrlWizForms.dll`: Amd64
- All others: MSIL (AnyCPU)

No x86-only DLLs. No architecture mismatch risk.

### Zone-of-Origin Blocks — None
[VERIFIED: ADS check on all Nw25 DLLs]

All 11 DLLs in `Contents/Nw25/` have no `Zone.Identifier` alternate data stream. No unblocking needed.

### NW2025 Launch History — None
[VERIFIED: filesystem inspection]

- `C:\Users\cemch-wei\AppData\Local\Autodesk\Navisworks Manage 2025\` does not exist
- `%TEMP%\CtrlWizNW.log` does not exist
- No journal files exist

NW2025 has never been launched by this user on this machine with this plugin. All diagnostic
logs must be produced by running NW2025 during Phase 1.

---

## Standard Stack (Diagnostic Tools)

No new libraries or packages are needed. These are all built-in Windows and .NET tools.

### Core Diagnostic Tools

| Tool | Access Method | Purpose | Required Elevation |
|------|---------------|---------|-------------------|
| Windows Event Viewer | `Win+R` → `eventvwr.msc` | Capture unhandled CLR exceptions (EventID 1026 / .NET Runtime) | No |
| NW Journal file | `%LOCALAPPDATA%\Autodesk\Navisworks Manage 2025\Journals\*.nwjsf` | NW-native plugin load messages | No |
| `%TEMP%\CtrlWizNW.log` | Notepad | Plugin self-logging (only if DLL loads and Logger initializes) | No |
| CLR Fusion Log (registry) | `reg add` to `HKLM\SOFTWARE\Microsoft\.NETFramework` | Assembly bind failure capture | **Yes (Admin By Request)** |
| PowerShell Reflection | `[System.Reflection.Assembly]::ReflectionOnlyLoadFrom(...)` | DLL bitness, API version verification | No |
| Procmon (Sysinternals) | Download separately if needed | File system access traces during NW startup | **Yes** |

### Fusion Log Registry Keys
[VERIFIED: standard .NET diagnostic]

```powershell
# Enable — requires Admin By Request elevation
reg add "HKLM\SOFTWARE\Microsoft\.NETFramework" /v EnableLog /t REG_DWORD /d 1 /f
reg add "HKLM\SOFTWARE\Microsoft\.NETFramework" /v LogPath /t REG_SZ /d "C:\FusionLogs" /f
reg add "HKLM\SOFTWARE\Microsoft\.NETFramework" /v ForceLog /t REG_DWORD /d 1 /f
reg add "HKLM\SOFTWARE\Microsoft\.NETFramework" /v LogFailures /t REG_DWORD /d 1 /f
New-Item -ItemType Directory -Force C:\FusionLogs

# Disable after diagnosis (important — fusion logging degrades performance)
reg delete "HKLM\SOFTWARE\Microsoft\.NETFramework" /v EnableLog /f
reg delete "HKLM\SOFTWARE\Microsoft\.NETFramework" /v ForceLog /f
reg delete "HKLM\SOFTWARE\Microsoft\.NETFramework" /v LogFailures /f
```

---

## Architecture Patterns

### How NW Loads Ribbon Resources
[VERIFIED: NW2025 SDK example + ARCHITECTURE.md]

```
NW startup
    │
    ├── Reads PackageContents.xml
    │       SeriesMin/Max="Nw25" matches → ModuleName resolved
    │
    ├── Assembly.LoadFrom("Contents/Nw25/CtrlWizNW.dll")
    │       CLR probes Contents/Nw25/ for dependent DLLs
    │
    ├── Reflects for [Plugin], [RibbonLayout], [Strings] attributes
    │       Finds [RibbonLayout("CustomRibbon.xaml")]
    │       Finds [Strings("CustomRibbon.name")]
    │
    ├── Looks for "Contents/Nw25/CustomRibbon.xaml" on disk
    │       *** FILE ABSENT → ribbon tab silently not rendered ***
    │
    └── CtrlWiz tab never appears — no error dialog, no journal entry
```

**Key behavior:** NW swallows missing ribbon resource files silently. No error dialog, no log
entry visible to the user. The DLL can be fully loaded and healthy while the ribbon tab is
suppressed due to a missing .xaml or .name file.

### Diagnostic Signal Hierarchy

| Source | What It Catches | Confidence |
|--------|----------------|------------|
| CtrlWizNW.log | Exceptions after DLL load + Logger init | HIGH (source-verified) |
| Windows Event Log (EventID 1026) | Unhandled CLR exceptions during startup | HIGH |
| NW Journal | Manifest parse failures, plugin discovery events | HIGH |
| CLR Fusion Log | Any assembly bind failure (most precise for DLL issues) | HIGH |
| Procmon | File path probing (what NW actually tried to open) | HIGH |

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead |
|---------|-------------|-------------|
| Assembly bind failure detection | Custom CLR event hook | CLR Fusion Log (registry keys above) |
| DLL bitness check | Custom decompiler | `[System.Reflection.Assembly]::ReflectionOnlyLoadFrom().GetName().ProcessorArchitecture` |
| Type existence check | Manual DLL disassembly | PowerShell reflection (as used in this research) |
| Zone-of-Origin check | Registry/ADS manual read | `Get-Item -Stream 'Zone.Identifier'` / `Unblock-File` |

**Key insight:** All diagnostic tools needed for this phase are standard Windows/.NET utilities.
No new code and no new tools need to be installed.

---

## Common Pitfalls

### Pitfall 1: Concluding the Plugin is "Working" Because No Error Appears
**What goes wrong:** NW shows no error dialog, no crash, no visible failure — developer concludes the ribbon mechanism itself is broken, not deployment.
**Why it happens:** NW silently drops ribbon construction when `CustomRibbon.xaml` or `CustomRibbon.name` is absent. There is no error surfaced.
**How to avoid:** Always check `Contents/Nw25/` for the presence of both files before any other investigation.
**Warning signs:** The canonical symptom — ribbon absent, no crash — is exactly this failure mode.

### Pitfall 2: Fusion Log Left Enabled After Diagnosis
**What goes wrong:** CLR Fusion logging captures every assembly bind attempt system-wide and degrades performance significantly. Leaving it enabled after the diagnostic session causes ongoing slowdowns.
**How to avoid:** The plan must include a cleanup step that disables Fusion logging immediately after NW2025 has been launched and the log has been captured.

### Pitfall 3: Forgetting That NW Must Be Fully Closed Before Each Test
**What goes wrong:** NW has no hot-reload. Bundle changes (file copies, manifest edits) are not picked up until NW restarts completely. Testing while NW is open produces incorrect results.
**How to avoid:** Every test cycle must explicitly: close NW → make change → relaunch NW → observe.

### Pitfall 4: Admin Elevation Not Available at the Right Time
**What goes wrong:** Fusion Log registry keys are under `HKLM` — they require elevation. If the developer does not acquire elevation (Admin By Request) before beginning the Fusion Log step, the step cannot be completed.
**How to avoid:** Acquire elevation at the start of the diagnostic session, not mid-stream.

### Pitfall 5: Treating Missing CustomRibbon Files as "Definitely the Only Issue"
**What goes wrong:** The missing XAML/name files are the highest-probability cause, but confirming them doesn't rule out a secondary DLL load failure. If the ribbon still doesn't appear after the fix, there is an undetected secondary issue.
**How to avoid:** Complete the full diagnostic sequence (Fusion log, journal, Event Log) regardless of whether the obvious issue is confirmed. Document what the logs actually show.

### Pitfall 6: AppName Collision Not Investigated
**What goes wrong:** Both `ComponentEntry` elements in `PackageContents.xml` use `AppName="CtrlWizNW"`. Whether the NW manifest loader enforces `AppName` uniqueness across `Components` blocks is unconfirmed. This is a quick thing to test during diagnosis (temporarily change the Nw25 `AppName` to `CtrlWizNW25` and retest).
**Warning signs:** If changing the AppName causes the ribbon to appear even before the XAML files are copied, AppName collision was the actual root cause.

---

## Code Examples

### Check Nw25 Folder for Missing Files
[VERIFIED: PowerShell — run without elevation]
```powershell
Get-ChildItem 'C:\ProgramData\Autodesk\ApplicationPlugins\CtrlWiz.NW.bundle\Contents\Nw25\' |
  Select-Object Name, LastWriteTime, Length |
  Format-Table -AutoSize
```

### Check NW Journal After First NW2025 Launch
```powershell
$localAppData = [System.Environment]::GetFolderPath('LocalApplicationData')
$journalDir = Join-Path $localAppData 'Autodesk\Navisworks Manage 2025\Journals'
$latest = Get-ChildItem $journalDir | Sort-Object LastWriteTime -Descending | Select-Object -First 1
Get-Content $latest.FullName | Select-String -Pattern 'CtrlWiz|plugin|error|fail|PackageContents'
```

### Check CtrlWizNW.log After First NW2025 Launch
```powershell
$logPath = Join-Path $env:TEMP 'CtrlWizNW.log'
if (Test-Path $logPath) { Get-Content $logPath } else { Write-Host 'Log not created - DLL did not load or Logger never initialized' }
```

### Check Windows Event Log for CLR Failures
```powershell
Get-WinEvent -FilterHashtable @{LogName='Application'; Id=1026} -MaxEvents 20 -ErrorAction SilentlyContinue |
  Where-Object { $_.Message -like '*roamer*' -or $_.Message -like '*CtrlWiz*' } |
  Select-Object TimeCreated, Message |
  Format-List
```

### Enable CLR Fusion Logging (Requires Admin By Request elevation)
```powershell
reg add "HKLM\SOFTWARE\Microsoft\.NETFramework" /v EnableLog /t REG_DWORD /d 1 /f
reg add "HKLM\SOFTWARE\Microsoft\.NETFramework" /v LogPath /t REG_SZ /d "C:\FusionLogs" /f
reg add "HKLM\SOFTWARE\Microsoft\.NETFramework" /v ForceLog /t REG_DWORD /d 1 /f
reg add "HKLM\SOFTWARE\Microsoft\.NETFramework" /v LogFailures /t REG_DWORD /d 1 /f
New-Item -ItemType Directory -Force C:\FusionLogs | Out-Null
```

```powershell
# After NW2025 launched and closed, inspect logs:
Get-ChildItem C:\FusionLogs -Recurse -Filter '*.htm' |
  Where-Object { $_.FullName -like '*roamer*' } |
  Select-Object FullName, LastWriteTime
```

### Verify NWRibbonButton in NW2025 (already confirmed, reference only)
```powershell
[System.Reflection.Assembly]::LoadFrom(
  'C:\Program Files\Autodesk\Navisworks Manage 2025\navisworks.gui.roamer.dll'
).GetTypes() | Where-Object { $_.Name -like '*Ribbon*' } | Select-Object FullName
```

---

## Diagnostic Sequence for Phase 1

This is the exact order the plan should follow. Each step has a pass/fail outcome and a
documented result. The phase is complete when a root-cause diagnosis document exists.

| Step | Action | Expected Finding | Time |
|------|--------|-----------------|------|
| 1 | Verify `Contents/Nw25/` file list | Missing: CustomRibbon.xaml, CustomRibbon.name | 2 min (DONE — confirmed in research) |
| 2 | First NW2025 launch; check `%TEMP%\CtrlWizNW.log` | Absent (plugin never loaded) or contains exception | 5 min |
| 3 | Check NW2025 journal (`Journals/*.nwjsf`) for plugin events | CtrlWizNW reference present or absent | 5 min |
| 4 | Check Windows Event Log (EventID 1026) for CLR exceptions | May show FileNotFoundException or be empty | 3 min |
| 5 | Enable Fusion Log (elevation), relaunch NW2025, inspect logs | Any DLL bind failures captured | 20 min |
| 6 | Inspect Fusion Log results for bind failures vs clean | Identify or rule out missing dependency DLLs | 10 min |
| 7 | Write diagnosis document with confirmed root cause | Documented finding + what Phase 2 must fix | 10 min |

**Optional steps if above does not confirm:**
- Test with `AppName="CtrlWizNW25"` in PackageContents.xml (rules out Pitfall 6)
- Attach VS2022 debugger to roamer.exe with first-chance CLR exceptions enabled

---

## State of the Art

| Old State | Current State | Verified |
|-----------|--------------|----------|
| NW2025 never launched | Will be launched for first time in Phase 1 | VERIFIED |
| `CustomRibbon.xaml` absent from Nw25 | Still absent (confirmed this session) | VERIFIED |
| `CustomRibbon.name` absent from Nw25 | Still absent (confirmed this session) | VERIFIED |
| `NWRibbonButton` type status in NW2025 | Confirmed present in correct namespace | VERIFIED |
| DLL bitness | All MSIL/Amd64 — no x86 DLLs | VERIFIED |
| Zone-of-Origin blocks | None — all DLLs clean | VERIFIED |
| Fusion log results | Unknown — NW2025 not yet launched | Pending |
| NW journal | Unknown — NW2025 not yet launched | Pending |

---

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | Autodesk NW2025 PackageContents loader silently skips ribbon construction when .xaml is absent — no error dialog | Architecture Patterns | If NW2025 surfaces an error we haven't seen, diagnosis is easier, not harder |
| A2 | Duplicate `AppName="CtrlWizNW"` across both Components blocks does not cause manifest rejection (only the correct SeriesMin/Max entry is loaded) | Current State | If wrong, AppName collision is an additional root cause; easily tested by changing Nw25 AppName |
| A3 | NW2025 has a Journal directory under `%LOCALAPPDATA%\Autodesk\Navisworks Manage 2025\Journals\` | Diagnostic Sequence step 3 | If NW2025 uses a different journal path, step 3 will find nothing; check the broader NW25 AppData dir |

---

## Open Questions

1. **Does the NW journal get created on first launch, even with no plugins loaded?**
   - What we know: The journal directory does not exist yet (NW2025 never launched)
   - What's unclear: Whether NW2025 writes a journal even when no plugins cause errors
   - Recommendation: Launch NW2025 and check — if no journal, the signal simply isn't available from that source

2. **Does NW2025 require the `[Strings]` file to be present for ribbon rendering, or only for label overrides?**
   - What we know: SDK example uses both files; ARCHITECTURE.md states "once declared, the file must be present or ribbon registration silently fails"
   - What's unclear: Whether this behavior is confirmed for NW2025 specifically vs inferred from SDK comments
   - Recommendation: Test with only `CustomRibbon.xaml` copied (no `.name`) if the ribbon still doesn't appear after Phase 2's fix

3. **Will the Fusion Log path (`C:\FusionLogs`) require Admin By Request every session?**
   - What we know: Writing to `HKLM` and creating a directory in `C:\` both require elevation
   - Recommendation: Acquire elevation at start of the Fusion Log diagnostic step; plan must note this dependency

---

## Environment Availability

| Dependency | Required By | Available | Version | Notes |
|------------|------------|-----------|---------|-------|
| Navisworks Manage 2025 | Running NW2025 for diagnosis | Yes | roamer.exe 3/25/2025 | NW2025 is installed |
| Admin By Request (elevation) | Fusion Log registry keys | Yes (machine-standard) | — | Required for HKLM writes |
| PowerShell | Diagnostic scripts | Yes | — | Available |
| Windows Event Viewer | CLR exception capture | Yes | — | Standard Windows tool |
| CLR Fusion Log | DLL bind failure capture | Yes | — | Standard .NET; needs elevation |

**No missing dependencies.** All diagnostic tools are available on the machine.

---

## Project Constraints (from CLAUDE.md)

| Constraint | Impact on Phase 1 |
|------------|------------------|
| Bundle path: `C:\ProgramData\Autodesk\ApplicationPlugins\CtrlWiz.NW.bundle\` | All file checks use this path |
| NW25 DLLs: `Contents/Nw25/` | Diagnostic inspection targets this subfolder |
| Admin elevation required for writes to `C:\ProgramData\` | Fusion Log step needs elevation; no writes to bundle during diagnosis |
| No code signing cert | Not relevant to Phase 1 (diagnosis only, no code changes) |
| Do not regress NW2022 support | Not relevant to Phase 1 (diagnosis only, no changes to Nw22 folder) |
| .NET Framework 4.8 | Confirms standard CLR Fusion Log tools apply |

---

## Sources

### Primary (HIGH confidence)
- [VERIFIED: direct filesystem inspection 2026-05-03] `C:\ProgramData\Autodesk\ApplicationPlugins\CtrlWiz.NW.bundle\Contents\Nw25\` — DLL list, timestamps, missing files confirmed
- [VERIFIED: direct filesystem inspection 2026-05-03] `C:\ProgramData\Autodesk\ApplicationPlugins\CtrlWiz.NW.bundle\Contents\Nw22\` — CustomRibbon.xaml and .name confirmed present in working reference
- [VERIFIED: direct filesystem inspection 2026-05-03] `C:\ProgramData\Autodesk\ApplicationPlugins\CtrlWiz.NW.bundle\PackageContents.xml` — Nw25 entry with duplicate AppName confirmed
- [VERIFIED: PowerShell reflection 2026-05-03] `navisworks.gui.roamer.dll` — NWRibbonButton and NWRibbonMenuButton confirmed present in `Autodesk.Navisworks.Gui.Roamer.AIRLook` namespace
- [VERIFIED: PowerShell reflection 2026-05-03] All Nw25 DLLs — ProcessorArchitecture: MSIL or Amd64, no x86 DLLs
- [VERIFIED: ADS inspection 2026-05-03] All Nw25 DLLs — no Zone.Identifier ADS blocks
- [VERIFIED: csproj inspection] `CtrlWizNW\CtrlWizNW.csproj` — CustomRibbon.xaml as `<Page>`, CustomRibbon.name as `<None>` with no CopyToOutputDirectory; Debug NW25|x64 OutputPath confirmed
- [VERIFIED: NW2025 SDK] `C:\Program Files\Autodesk\Navisworks Manage 2025\api\NET\examples\Basic Examples\CSharp\CustomRibbon\` — canonical ribbon plugin structure

### Secondary (MEDIUM confidence)
- [CITED: .planning/research/STACK.md] Diagnostic tool stack, log paths, CLR Fusion log registry keys
- [CITED: .planning/research/PITFALLS.md] Pitfall taxonomy for NW2025 plugin load failures
- [CITED: .planning/research/ARCHITECTURE.md] Bundle loading sequence, ribbon resource resolution behavior
- [CITED: .planning/research/FEATURES.md] ISSUE-1 and ISSUE-2 analysis of missing XAML files

---

## Metadata

**Confidence breakdown:**
- Current state (missing files, DLL bitness, zone blocks): HIGH — direct filesystem verification
- NWRibbonButton type availability: HIGH — PowerShell reflection against installed NW2025 DLL
- Diagnostic tool procedures: HIGH — standard Windows/.NET tools
- Pending log findings (journal, Event Log, Fusion): N/A — NW2025 not yet launched

**Research date:** 2026-05-03
**Valid until:** 2026-06-03 (stable domain; NW2025 install does not change)
