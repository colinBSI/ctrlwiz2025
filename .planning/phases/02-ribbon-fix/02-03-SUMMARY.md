---
phase: 02-ribbon-fix
plan: 03
subsystem: build
tags: [msbuild, navisworks, ribbon, nw25, xaml, nwribbonbutton, plugin-init]

# Dependency graph
requires:
  - phase: 02-ribbon-fix
    plan: 01
    provides: "csproj Content/None items so CustomRibbon.xaml and CustomRibbon.name copy to Nw25 output"
  - phase: 02-ribbon-fix
    plan: 02
    provides: "PackageContents.xml AppName collision fix (manual, elevation-required)"
provides:
  - "Build verification: CustomRibbon.xaml and CustomRibbon.name confirmed deployed to Contents/Nw25/"
  - "Human verification result: ribbon NOT visible in NW2025 — DLL silently fails to initialize"
  - "Root cause hypothesis refined: NWRibbonButton XAML type resolution failure in NW2025"
affects: [03-navigation-validation, gap-closure-plan]

# Tech tracking
tech-stack:
  added: []
  patterns: ["MSBuild Link metadata required to flatten nested source paths into output root"]

key-files:
  created: []
  modified:
    - "CtrlWizNW/CtrlWizNW.csproj"

key-decisions:
  - "Task 1 PASS: Debug NW25|x64 build succeeded; CustomRibbon.xaml and CustomRibbon.name present in Contents/Nw25/"
  - "Task 2 FAIL: Ribbon not visible; DLL silently fails to initialize (no log, no error dialog)"
  - "Root cause hypothesis: NWRibbonButton type may not exist in NW2025's navisworks.gui.roamer.dll — XAML parse fails silently during ribbon layout loading"
  - "Evidence: Plugin Manager shows Enabled (DLL found by NW), %TEMP%\\CtrlWizNW.log never created (Logger.SetUpLogger never reached)"

patterns-established:
  - "MSBuild CopyToOutputDirectory with nested source paths requires <Link> metadata to specify flat destination filename"

requirements-completed: []

# Metrics
duration: "~30 min (human verification included)"
completed: 2026-05-03
---

# Phase 02 Plan 03: Build Verification and NW2025 Launch Summary

**Build succeeded and files deployed, but ribbon is absent — DLL silently fails to initialize, pointing to NWRibbonButton XAML type resolution failure in NW2025**

## Performance

- **Duration:** ~30 min (includes human verification of NW2025 launch)
- **Started:** 2026-05-03
- **Completed:** 2026-05-03
- **Tasks:** 2 (Task 1 PASS, Task 2 FAIL)
- **Files modified:** 1 (CtrlWizNW.csproj — Link metadata added during Task 1)

## Accomplishments

- Debug NW25|x64 build succeeded (MSBuild exit 0)
- CustomRibbon.xaml and CustomRibbon.name confirmed present in `Contents/Nw25/` with fresh timestamps
- CtrlWizNW.dll confirmed updated in `Contents/Nw25/`
- Human verified NW2025 launch: plugin appears as Enabled in Plugin Manager (bundle recognized)
- Human verification confirmed ribbon ABSENT — narrowed root cause to DLL initialization failure

## Task Commits

1. **Task 1: Rebuild Debug NW25|x64 and verify artifact deployment** - `f3393ed` (fix — added Link metadata to flatten CustomRibbon copy paths)
2. **Task 2: Launch NW2025 and confirm CtrlWiz ribbon tab appears** - FAILED (ribbon not visible; no commit)

## Files Created/Modified

- `CtrlWizNW/CtrlWizNW.csproj` — Added `<Link>` metadata to Content/None items so the nested `CustomRibbon\CustomRibbon.xaml` and `CustomRibbon\CustomRibbon.name` source paths copy to the output root as flat files (not in a `CustomRibbon\` subdirectory)

## Decisions Made

- Added `<Link>CustomRibbon.xaml</Link>` and `<Link>CustomRibbon.name</Link>` metadata to the four new ItemGroup items — this was a Rule 1 auto-fix discovered during Task 1 verification when the deployed path was found to be `Contents/Nw25/CustomRibbon/CustomRibbon.xaml` instead of `Contents/Nw25/CustomRibbon.xaml`
- Task 2 is marked FAIL: success criteria require ribbon visibility AND log creation; neither condition was met

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Added Link metadata to flatten CustomRibbon file copy paths**

- **Found during:** Task 1 (Rebuild Debug NW25|x64 and verify artifact deployment)
- **Issue:** Without `<Link>` metadata, MSBuild preserves the source directory structure. The files were copied to `Contents/Nw25/CustomRibbon/CustomRibbon.xaml` (subdirectory) rather than `Contents/Nw25/CustomRibbon.xaml` (flat, as required by the NW ribbon loader's `[RibbonLayout]` attribute)
- **Fix:** Added `<Link>CustomRibbon.xaml</Link>` and `<Link>CustomRibbon.name</Link>` to each of the four conditional ItemGroup items in CtrlWizNW.csproj
- **Files modified:** `CtrlWizNW/CtrlWizNW.csproj`
- **Verification:** Post-fix filesystem check confirmed both files at flat paths in `Contents/Nw25/`
- **Committed in:** `f3393ed` (Task 1 commit)

---

**Total deviations:** 1 auto-fixed (Rule 1 — bug in MSBuild copy path)
**Impact on plan:** Essential correctness fix. Files were deployed, but ribbon still absent — the fix resolved the deployment path issue but did not resolve the ribbon initialization failure.

## Issues Encountered

### Task 2 FAIL: Ribbon Not Visible — DLL Silently Fails to Initialize

**Observation (human-verified):**
- NW2025 Plugin Manager: CtrlWizNW shows as **Enabled** — the bundle was found and the DLL was recognized
- Ribbon tab: **NOT visible** after NW2025 fully loaded
- Error dialogs: **None** — NW2025 swallowed any exception silently
- Log file `%TEMP%\CtrlWizNW.log`: **NOT created** — `Logger.SetUpLogger` was never reached

**Interpretation:**
The DLL loads into the NW2025 AppDomain (Plugin Manager confirms this), but plugin initialization fails before the logger initializes. The most likely failure point is ribbon layout parsing: NW2025's `[RibbonLayout]` attribute triggers XAML loading, which attempts to resolve `NWRibbonButton` from `navisworks.gui.roamer.dll`. If this type does not exist in NW2025's version of that DLL, the XAML parse throws a `XamlParseException` that NW2025 catches and discards silently.

**Root cause hypothesis (HIGH confidence):**
`NWRibbonButton` type (used in `CustomRibbon.xaml`) does not exist or has been renamed in NW2025's `navisworks.gui.roamer.dll`.

**Evidence chain:**
1. Plugin Manager shows Enabled → DLL reached NW2025 AppDomain
2. No log → `Logger.SetUpLogger` never called → initialization aborted before any application code ran
3. No error dialog → exception caught and suppressed by NW plugin host
4. CustomRibbon.xaml and CustomRibbon.name are physically present at the correct flat paths → file presence is not the issue
5. RESEARCH.md secondary risk note: "verify NWRibbonButton still exists in NW2025 before fixing" — this risk materialized

**Next steps required (gap closure plan):**
1. Inspect `navisworks.gui.roamer.dll` (NW2025 version) to confirm whether `NWRibbonButton` exists
   ```powershell
   # In elevated PowerShell or dnSpy/ILSpy:
   [System.Reflection.Assembly]::LoadFile('C:\Program Files\Autodesk\Navisworks Manage 2025\navisworks.gui.roamer.dll').GetTypes() | Where-Object { $_.Name -like '*Ribbon*' }
   ```
2. If `NWRibbonButton` is absent: replace with the NW2025 equivalent type (likely `Autodesk.Navisworks.Gui.Roamer.RibbonButton` or similar WPF/Fluent control)
3. If `NWRibbonButton` exists: check NW2025 journal and Windows Event Log (EventID 1026) for additional exception detail
4. Rebuild and re-verify after XAML is fixed

## Next Phase Readiness

- Phase 2 Plan 03 success criteria are NOT met — ribbon is absent and DLL does not initialize
- A gap closure plan is needed before Phase 3 (Navigation Validation) can proceed
- The gap closure plan should focus on: (a) confirm NWRibbonButton type presence/absence in NW2025, (b) fix CustomRibbon.xaml to use the correct NW2025 type, (c) rebuild and re-verify
- Blocker: `NWRibbonButton` type resolution in NW2025's `navisworks.gui.roamer.dll`

## Self-Check: FAILED

**Success criteria not met:**

| Criterion | Status |
|-----------|--------|
| Debug NW25|x64 build succeeded with exit code 0 | PASS |
| CustomRibbon.xaml present in Contents/Nw25/ with fresh timestamp | PASS |
| CustomRibbon.name present in Contents/Nw25/ with fresh timestamp | PASS |
| Human confirms CtrlWiz ribbon tab visible in NW2025 | FAIL |
| %TEMP%\CtrlWizNW.log created (DLL loaded successfully) | FAIL |

Plan 02-03 is incomplete. Two of the four success criteria failed at the human verification checkpoint. Root cause is a secondary failure (XAML type resolution) not addressed by the csproj and PackageContents.xml fixes of plans 02-01 and 02-02.

---
*Phase: 02-ribbon-fix*
*Completed: 2026-05-03*
