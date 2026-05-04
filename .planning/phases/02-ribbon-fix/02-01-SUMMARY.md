---
phase: 02-ribbon-fix
plan: 01
subsystem: build
tags: [msbuild, csproj, ribbon, nw25, copy-to-output]
dependency_graph:
  requires: []
  provides: [CustomRibbon.xaml copied to Nw25 output, CustomRibbon.name copied to Nw25 output]
  affects: [CtrlWizNW build output, Contents/Nw25/ bundle directory]
tech_stack:
  added: []
  patterns: [MSBuild config-scoped ItemGroup with Condition, Content/None with CopyToOutputDirectory=Always]
key_files:
  created: []
  modified:
    - CtrlWizNW/CtrlWizNW.csproj
decisions:
  - "Use additive condition-scoped ItemGroups (not replace existing items) so NW22 Page/None behavior is completely unaffected"
  - "Both Debug NW25|x64 and NW25|x64 configurations receive identical copy directives for both files"
metrics:
  duration: "~10 minutes"
  completed: "2026-05-03"
  tasks_completed: 2
  tasks_total: 2
---

# Phase 2 Plan 1: csproj Ribbon File Copy Fix Summary

**One-liner:** Added MSBuild config-scoped Content and None items so CustomRibbon.xaml and CustomRibbon.name are copied as loose files to the NW25 build output directory, fixing the confirmed root cause of the missing ribbon tab.

## What Was Done

The `CtrlWizNW/CtrlWizNW.csproj` was missing build item declarations that would copy `CustomRibbon.xaml` and `CustomRibbon.name` to the NW25 output directory. The existing items — a `<Page>` for the .xaml (compiles to BAML, no loose file) and a bare `<None>` for the .name (no copy action) — served NW22 but left the NW25 output missing both files.

Two sets of conditional ItemGroups were added, one pair conditioned on `'$(Configuration)|$(Platform)' == 'Debug NW25|x64'` and one on `'NW25|x64'`. Each ItemGroup contains the respective file with `<CopyToOutputDirectory>Always</CopyToOutputDirectory>`.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Add config-scoped Content items for CustomRibbon.xaml | 50ccd41 | CtrlWizNW/CtrlWizNW.csproj |
| 2 | Add config-scoped None items for CustomRibbon.name | fc7e3f5 | CtrlWizNW/CtrlWizNW.csproj |

## Verification Results

All five plan-specified checks passed:

1. `Content Include="CustomRibbon\CustomRibbon.xaml"` — present (lines 282, 288)
2. Condition block for `Debug NW25|x64` on a Content ItemGroup — present (line 292)
3. Condition block for `NW25|x64` on a Content ItemGroup — present (line 300)
4. Original `Page Include="CustomRibbon\CustomRibbon.xaml"` — still present and untouched (line 273)
5. `CopyToOutputDirectory>Always` — present on all four new items (lines 218, 224, 297, 303)

XML validity confirmed via PowerShell `[xml]` cast — no exception.

## Decisions Made

- **Additive pattern over replacement:** The existing `<Page>` and bare `<None>` items were left exactly as-is. New ItemGroups are purely additive, condition-gated. MSBuild resolves duplicate Include paths correctly: the conditional item's CopyToOutputDirectory takes effect only when the condition is active.
- **Both NW25 configs covered:** `Debug NW25|x64` (deploys to `C:\ProgramData\...`) and `NW25|x64` (release output to `CtrlWiz.Release.Assemblies\`) both receive the copy items so neither configuration is left incomplete.

## Deviations from Plan

None — plan executed exactly as written.

## Known Stubs

None.

## Threat Flags

None — no new network endpoints, auth paths, file access patterns, or schema changes introduced. The csproj modification affects only build-time file copy behavior to a developer-local bundle path.

## Self-Check: PASSED

- `CtrlWizNW/CtrlWizNW.csproj` — FOUND and modified
- Commit `50ccd41` — FOUND (Task 1)
- Commit `fc7e3f5` — FOUND (Task 2)
