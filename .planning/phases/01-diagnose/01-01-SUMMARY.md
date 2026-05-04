---
phase: 01-diagnose
plan: 01-01
status: complete
completed: 2026-05-03
---

# Summary: 01-01 First-Launch Diagnostic Capture

## Objective Achieved

All diagnostic artifacts from the first NW2025 launch have been captured. No code was changed. Every log source has been checked and its state (present or absent) documented.

## What Was Found

### Contents/Nw25/ Artifact Snapshot

25 files present. All DLLs have 2026-05-03 timestamps (fresh build deployed).

**CONFIRMED MISSING:**
- `CustomRibbon.xaml` — ABSENT from Contents/Nw25/
- `CustomRibbon.name` — ABSENT from Contents/Nw25/

**Reference (Nw22 — working):** Both `CustomRibbon.name` and `CustomRibbon.xaml` present in Contents/Nw22/ (timestamps 2026-05-01).

### PackageContents.xml

Duplicate `AppName="CtrlWizNW"` confirmed on both component entries:
- Nw22 entry: `AppName="CtrlWizNW"` → `./Contents/Nw22/CtrlWizNW.dll`
- Nw25 entry: `AppName="CtrlWizNW"` → `./Contents/Nw25/CtrlWizNW.dll`

Both entries use `AppType="ManagedPlugin"` and `Version="2.0.1"`.

### NW2025 Launch Observation

- NW2025 launched successfully — ribbon and workspace loaded normally
- **CtrlWiz tab: DID NOT APPEAR** in the ribbon (as expected given missing XAML files)
- NW2025 was fully closed after observation

### Log Source Results

| Source | Result | Interpretation |
|--------|--------|----------------|
| `%TEMP%\CtrlWizNW.log` | NOT CREATED | DLL never loaded; `Logger.SetUpLogger` never called |
| NW Journal (`%LOCALAPPDATA%\Autodesk\Navisworks Manage 2025\Journals\`) | Directory DOES NOT EXIST | NW2025 AppData dir absent — NW launched but created no AppData on this machine |
| Windows Event Log (EventID 1026) | No entries | No CLR unhandled exceptions from roamer/CtrlWiz process |
| CLR Fusion Log (`C:\FusionLogs\`) | **EMPTY — 0 .htm files** | CLR never attempted to bind any assemblies for the plugin |

### Critical Finding: Empty Fusion Logs

The FusionLogs directory is completely empty despite Fusion logging being active during the NW2025 launch. This means **CLR never attempted to load CtrlWizNW.dll or any of its dependencies**. The plugin loading failure is happening at the NW plugin host layer (manifest/ribbon resource stage), before the .NET runtime is involved.

This rules out CLR-level failures (bad references, architecture mismatches, missing GAC entries) as contributing factors. The failure is entirely in NW's native plugin discovery pipeline.

### Fusion Logging State

- Registry keys enabled before NW launch (user-confirmed via elevated PowerShell — Step B succeeded)
- Registry keys disabled after capture: `EnableLog` key absent — **PASS**
- `C:\FusionLogs\` preserved (empty)

## Key Files

### Modified
- None (diagnosis phase — no code changes)

### Created
- `C:\FusionLogs\` (empty directory, preserved for reference)

## Decisions / Observations

- NW2025 has never been run on this machine before (no AppData directory exists) — this first launch was fully clean
- The AppName duplicate in PackageContents.xml has no observable Fusion log evidence either way; cannot be confirmed or ruled out via CLR logs since CLR was never reached
- All 11 DLLs in Nw25 are present with correct 2026-05-03 timestamps — the deployment pipeline (Debug NW25|x64 build config) is working

## Self-Check: PASSED

- [x] Contents/Nw25/ file list captured — CustomRibbon.xaml and CustomRibbon.name confirmed absent
- [x] PackageContents.xml duplicate AppName confirmed
- [x] NW2025 launched and ribbon observation recorded (no CtrlWiz tab)
- [x] CtrlWizNW.log checked — NOT CREATED
- [x] NW Journal checked — directory not created on first launch
- [x] Windows Event Log (EventID 1026) queried — no entries
- [x] Fusion log checked — 0 .htm files (CLR never invoked for plugin)
- [x] Fusion logging disabled and verified off
