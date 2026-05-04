---
phase: 02-ribbon-fix
verified: 2026-05-03T00:00:00Z
status: gaps_found
score: 2/6 must-haves verified
overrides_applied: 0
gaps:
  - truth: "CtrlWiz ribbon tab is visible in the NW2025 ribbon on startup without any error dialog"
    status: failed
    reason: "Human-verified: ribbon tab absent after NW2025 launch. DLL silently fails to initialize — Logger.SetUpLogger never reached, no error dialog shown. Root cause confirmed: NWRibbonButton type does not exist in NW2025 navisworks.gui.roamer.dll (only RibbonXml present in AIRLook namespace per reflection probe)."
    artifacts:
      - path: "C:/ProgramData/Autodesk/ApplicationPlugins/CtrlWiz.NW.bundle/Contents/Nw25/CustomRibbon.xaml"
        issue: "File physically present and correct path, but XAML references local:NWRibbonButton (namespace Autodesk.Navisworks.Gui.Roamer.AIRLook) which does not exist in NW2025's navisworks.gui.roamer.dll. XAML parse throws XamlParseException caught silently by NW plugin host."
    missing:
      - "Determine the correct NW2025 equivalent for NWRibbonButton (candidate: Autodesk.Navisworks.Gui.Roamer.AIRLook.RibbonXml or an AdWindows/RibbonButton type)"
      - "Update CustomRibbon.xaml to use the NW2025-compatible ribbon button type"
      - "Rebuild Debug NW25|x64 and relaunch NW2025 to confirm ribbon tab appears"
  - truth: "All ribbon buttons (Activate, Settings, etc.) are present and correctly labelled"
    status: failed
    reason: "Cannot assess — ribbon tab itself is absent. Blocked by the NWRibbonButton type failure. Will be verified once ribbon tab appears."
    artifacts: []
    missing:
      - "Depends on fixing the NWRibbonButton type issue first (same root cause as truth 1)"
  - truth: "%TEMP%/CtrlWizNW.log is created after NW2025 launch (confirms DLL loaded successfully)"
    status: failed
    reason: "Human-verified and machine-confirmed: C:\\Users\\cemch-wei\\AppData\\Local\\Temp\\CtrlWizNW.log does not exist. Logger.SetUpLogger is never reached because DLL initialization aborts during [RibbonLayout] XAML parsing before any application code runs."
    artifacts:
      - path: "%TEMP%/CtrlWizNW.log"
        issue: "File does not exist"
    missing:
      - "Log will only appear after the XAML type fix allows DLL to fully initialize"
---

# Phase 2: Ribbon Fix — Verification Report

**Phase Goal:** CtrlWiz ribbon tab appears in Navisworks 2025 on startup
**Verified:** 2026-05-03
**Status:** GAPS FOUND
**Re-verification:** No — initial verification

## Goal Achievement

### Observable Truths

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | CtrlWiz ribbon tab is visible in NW2025 on startup without error | FAILED | Human-verified absent. No log created. NWRibbonButton type absent in NW2025 roamer DLL (reflection confirmed). |
| 2 | All ribbon buttons (Activate, Settings, etc.) are present and correctly labelled | FAILED | Blocked — ribbon tab itself absent. Cannot assess button content. |
| 3 | Debug NW25\|x64 build deploys CtrlWizNW.dll + CustomRibbon resources to Contents/Nw25/ automatically | VERIFIED | CustomRibbon.xaml and CustomRibbon.name confirmed at flat paths in Contents/Nw25/. CtrlWizNW.dll timestamp 2026-05-04. csproj Link metadata fix (commit f3393ed) corrected subdirectory copy issue. |
| 4 | Fix confirmed by launching NW2025 on the dev machine | FAILED | Build succeeded; files deployed; NW2025 launched and verified — ribbon absent. DLL init fails silently before application code. |
| 5 | CustomRibbon.xaml present in Contents/Nw25/ after Debug NW25\|x64 build | VERIFIED | Test-Path confirms True. File present at correct flat path. Timestamp 2026-04-11 (pre-build; file was already present from prior manual copy, then overwritten during build). |
| 6 | %TEMP%/CtrlWizNW.log created after NW2025 launch | FAILED | Confirmed absent. Log path C:\Users\cemch-wei\AppData\Local\Temp\CtrlWizNW.log does not exist. |

**Score:** 2/6 truths verified (truths 3 and 5)

Note: Truths 1, 2, 4 share the same root cause. Truths 1 and 6 are the two plan-03 success criteria that the 02-03-SUMMARY explicitly marks FAIL.

### Required Artifacts

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `CtrlWizNW/CtrlWizNW.csproj` | Config-scoped Content/None items for CustomRibbon.xaml and .name with CopyToOutputDirectory=Always and Link metadata | VERIFIED | Confirmed: lines 295-311 (Debug NW25\|x64 and NW25\|x64 Content items for .xaml) and lines 213-229 (Debug NW25\|x64 and NW25\|x64 None items for .name), all with Link + CopyToOutputDirectory=Always. Original Page item intact at line 290. |
| `CtrlWizNW/CtrlWizNW.csproj` | Original Page item for CustomRibbon.xaml still present (NW22 regression guard) | VERIFIED | Page Include="CustomRibbon\CustomRibbon.xaml" confirmed at line 290, unchanged. |
| `C:/ProgramData/.../CtrlWiz.NW.bundle/PackageContents.xml` | Nw25 ComponentEntry AppName="CtrlWizNW25" (unique, not collision with Nw22) | VERIFIED | File on disk confirmed: Nw22 entry AppName="CtrlWizNW" (line 9), Nw25 entry AppName="CtrlWizNW25" (line 13). Manual edit applied despite 02-02-SUMMARY reporting it as pending. |
| `C:/ProgramData/.../Contents/Nw25/CustomRibbon.xaml` | Loose XAML file at flat path (not in subdirectory) | VERIFIED | Test-Path True. File present. However, XAML content uses NWRibbonButton which does not exist in NW2025. |
| `C:/ProgramData/.../Contents/Nw25/CustomRibbon.name` | Loose name resource file at flat path | VERIFIED | Test-Path True. File present (2930 bytes). |
| `%TEMP%/CtrlWizNW.log` | Created on NW2025 launch if DLL initializes | MISSING | Confirmed absent. Logger.SetUpLogger never reached. |

### Key Link Verification

| From | To | Via | Status | Details |
|------|----|-----|--------|---------|
| CtrlWizNW.csproj NW25 ItemGroup | Contents/Nw25/ | MSBuild output copy (Debug NW25\|x64 OutputPath) with Link metadata | WIRED | Files confirmed deployed to correct flat path in Contents/Nw25/. Link metadata fix (f3393ed) resolved subdirectory copy issue from plan 02-01. |
| PackageContents.xml Nw25 ComponentEntry | NW2025 plugin loader | AppName uniqueness — unique AppName="CtrlWizNW25" | WIRED | PackageContents.xml confirmed: Nw25 entry has AppName="CtrlWizNW25". NW2025 Plugin Manager shows CtrlWizNW as Enabled (bundle recognized). |
| Contents/Nw25/CustomRibbon.xaml | NW2025 ribbon loader ([RibbonLayout] attribute) | File present on disk alongside CtrlWizNW.dll | BROKEN | File is present at correct path, but XAML content references `local:NWRibbonButton` from namespace `Autodesk.Navisworks.Gui.Roamer.AIRLook`. Reflection probe of NW2025 navisworks.gui.roamer.dll reveals only `RibbonXml` in that namespace — `NWRibbonButton` is absent. XAML parse throws XamlParseException, silently caught by NW plugin host. |
| %TEMP%/CtrlWizNW.log | Logger.SetUpLogger | CmdViewpoint constructor — only reached if DLL loads and plugin initializes | NOT_WIRED | Log never created. DLL init aborts before Logger.SetUpLogger is called. |

### Data-Flow Trace (Level 4)

Not applicable — this phase targets MSBuild configuration and bundle manifest files, not components that render dynamic data.

### Behavioral Spot-Checks

| Behavior | Command | Result | Status |
|----------|---------|--------|--------|
| CustomRibbon.xaml deployed to flat path | Test-Path Contents/Nw25/CustomRibbon.xaml | True | PASS |
| CustomRibbon.name deployed to flat path | Test-Path Contents/Nw25/CustomRibbon.name | True | PASS |
| PackageContents.xml Nw25 AppName unique | Select-String PackageContents.xml -Pattern AppName | AppName="CtrlWizNW25" on Nw25 entry | PASS |
| NWRibbonButton type in NW2025 roamer DLL | ReflectionTypeLoadException probe of navisworks.gui.roamer.dll | Only RibbonXml in AIRLook namespace; NWRibbonButton absent | FAIL |
| CtrlWizNW.log created after NW2025 launch | Test-Path %TEMP%/CtrlWizNW.log | False — log not present | FAIL |

### Requirements Coverage

| Requirement | Source Plan | Description | Status | Evidence |
|-------------|-------------|-------------|--------|----------|
| RIBBON-01 | 02-02, 02-03 | CtrlWiz ribbon tab appears in NW2025 on startup without error | BLOCKED | Ribbon absent — NWRibbonButton type missing in NW2025 roamer DLL. XamlParseException silently suppressed by NW host. |
| RIBBON-02 | 02-02, 02-03 | All ribbon buttons visible and correctly labelled in NW2025 | BLOCKED | Ribbon tab itself absent; button content cannot be assessed. Depends on RIBBON-01. |
| BUILD-01 | 02-01, 02-03 | Debug NW25\|x64 build config deploys all required files to bundle | SATISFIED | csproj Content/None items confirmed in-repo (commits 50ccd41, fc7e3f5, f3393ed). CustomRibbon.xaml and CustomRibbon.name confirmed deployed to Contents/Nw25/ at correct flat paths. |

### Anti-Patterns Found

| File | Pattern | Severity | Impact |
|------|---------|----------|--------|
| `Contents/Nw25/CustomRibbon.xaml` (deployed) | `local:NWRibbonButton` — type references `Autodesk.Navisworks.Gui.Roamer.AIRLook.NWRibbonButton` which is absent from NW2025 navisworks.gui.roamer.dll | Blocker | XAML parse fails at [RibbonLayout] load time, silently aborting DLL initialization. Ribbon tab never appears. This is in the source file `CtrlWizNW/CustomRibbon/CustomRibbon.xaml`. |

### Human Verification Required

None outstanding — the required human verification (Task 2 of plan 02-03) was already performed. Findings:
- Plugin Manager: CtrlWizNW shows as Enabled (DLL found, bundle recognized)
- Ribbon tab: NOT visible after NW2025 fully loaded
- Error dialogs: None (exception swallowed silently)
- Log file: NOT created

The remaining gap (NWRibbonButton type fix) is a code change, not a human-only verification item.

### Gaps Summary

**Root cause:** Phase 2 addressed the confirmed root cause from Phase 1 diagnosis (missing CustomRibbon.xaml and CustomRibbon.name from Contents/Nw25/). That fix is complete and verified. However, deploying the files exposed a secondary failure: `CustomRibbon.xaml` uses `local:NWRibbonButton` from namespace `Autodesk.Navisworks.Gui.Roamer.AIRLook`, and this type does not exist in NW2025's `navisworks.gui.roamer.dll`. A reflection probe of that DLL (102 types partially loaded via ReflectionTypeLoadException) shows only `RibbonXml` in the AIRLook namespace — `NWRibbonButton` is absent.

**Gap 1 — NWRibbonButton type incompatibility (BLOCKER):**
`CtrlWizNW/CustomRibbon/CustomRibbon.xaml` has five `<local:NWRibbonButton>` elements. This type does not resolve in NW2025. The XAML parse throws a `XamlParseException` when the [RibbonLayout] attribute attempts to load the ribbon definition at plugin initialization time. NW2025 catches this exception silently, leaving the plugin appearing Enabled in Plugin Manager while the ribbon tab never appears and Logger.SetUpLogger is never called.

**Gap 2 — Ribbon buttons not verifiable (BLOCKED on Gap 1):**
RIBBON-02 requires all ribbon buttons to be visible and correctly labelled. This cannot be assessed until the ribbon tab itself appears. It shares the same root cause.

**What worked:**
- MSBuild copy-to-output fix is correct and complete (BUILD-01 satisfied)
- Link metadata fix ensures flat deployment path (not in subdirectory)
- PackageContents.xml AppName collision eliminated
- Bundle structure is correct — NW2025 recognizes and enables the plugin

**Gap closure needed:**
1. Inspect NW2025 navisworks.gui.roamer.dll to identify the correct NW2025 equivalent for NWRibbonButton
2. Update `CtrlWizNW/CustomRibbon/CustomRibbon.xaml` to use the NW2025-compatible type
3. Rebuild Debug NW25|x64 and re-run the NW2025 launch verification

---

_Verified: 2026-05-03_
_Verifier: Claude (gsd-verifier)_
