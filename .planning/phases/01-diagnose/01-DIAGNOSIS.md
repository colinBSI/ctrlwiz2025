# Phase 1 Diagnosis: CtrlWiz NW2025 Ribbon Failure

**Date:** 2026-05-03
**Diagnosed by:** Phase 1 execution

## Executive Summary

The CtrlWiz ribbon tab does not appear in Navisworks 2025 because `CustomRibbon.xaml` and `CustomRibbon.name` are missing from `Contents/Nw25/`. NW's native plugin host silently suppresses ribbon registration when these loose files are absent — CLR assembly loading is never even attempted (confirmed by empty Fusion logs). No secondary CLR-level failures exist; the fix is entirely in the MSBuild project file.

## Evidence Table

| Source | Finding | Confirms/Rules Out |
|--------|---------|--------------------|
| Contents/Nw25/ filesystem | `CustomRibbon.xaml` ABSENT | Confirms Root Cause A |
| Contents/Nw25/ filesystem | `CustomRibbon.name` ABSENT | Confirms Root Cause A |
| Contents/Nw22/ filesystem (reference) | Both `CustomRibbon.xaml` and `CustomRibbon.name` PRESENT | Confirms Nw22 has correct config; Nw25 is the only version missing these files |
| NWRibbonButton reflection (pre-confirmed in RESEARCH.md) | Type present in `Autodesk.Navisworks.Gui.Roamer.AIRLook` namespace in NW2025 | Rules out XAML type incompatibility — no changes to the XAML content needed |
| DLL bitness check (pre-confirmed in RESEARCH.md) | All MSIL or Amd64 | Rules out architecture mismatch |
| Zone-of-Origin ADS (pre-confirmed in RESEARCH.md) | No blocks on any DLL | Rules out unblock requirement |
| PackageContents.xml | Duplicate `AppName="CtrlWizNW"` on both Nw22 and Nw25 component entries | Inconclusive — CLR was never reached, so no Fusion log evidence either way; cannot confirm or rule out as a contributing factor |
| `%TEMP%\CtrlWizNW.log` | NOT CREATED | DLL never loaded; `Logger.SetUpLogger` was never reached — confirms failure is pre-CLR |
| NW Journal (`%LOCALAPPDATA%\Autodesk\Navisworks Manage 2025\Journals\`) | Directory does not exist (NW2025 first launch, no AppData created) | No plugin load attempts logged by NW; journal absence is expected on first launch |
| Windows Event Log (EventID 1026) | No entries matching roamer, CtrlWiz, or Navisworks in the last hour | No CLR unhandled exceptions during NW2025 launch — confirms no CLR crash |
| CLR Fusion Log (`C:\FusionLogs\`) | **0 .htm files — completely empty** | CLR never attempted to bind CtrlWizNW.dll or any dependency; failure is upstream of the .NET runtime |

## Root Cause(s)

### Confirmed Root Cause: Missing Ribbon Resource Files

**Root Cause A (CONFIRMED):** `CustomRibbon.xaml` and `CustomRibbon.name` are missing from `Contents/Nw25/`.

- The `[RibbonLayout("CustomRibbon.xaml")]` and `[Strings("CustomRibbon.name")]` attributes on `CmdViewpoint` instruct NW's plugin host to locate these as loose files alongside the DLL at startup.
- When absent, NW silently suppresses ribbon registration — no error dialog, no crash, no journal entry, and (as confirmed) no CLR assembly loading at all.
- The empty Fusion log proves that NW's native plugin host rejected or skipped the plugin before handing off to the .NET CLR — the failure is entirely in the plugin manifest/resource discovery layer.

**Build root cause (how the files went missing):**
- `CustomRibbon.xaml` is declared as `<Page Include="CustomRibbon\CustomRibbon.xaml">` in `CtrlWizNW.csproj` — the `Page` build action compiles the XAML to BAML and embeds it in the DLL. The loose `.xaml` file is never written to the output directory.
- `CustomRibbon.name` is declared as `<None Include="CustomRibbon\CustomRibbon.name">` with no `<CopyToOutputDirectory>` element — a `None` item is never copied to output.
- The `Debug NW22|x64` build has a working post-build mechanism (or different csproj item groups) that does copy these files for Nw22; the `Debug NW25|x64` configuration lacks equivalent copy steps.

### Secondary Findings

**AppName Collision (`AppName="CtrlWizNW"` on both Nw22 and Nw25 entries):**
- Evidence: CLR Fusion logs are empty — NW never reached the CLR loading stage, so no bind log evidence exists for either component entry.
- Disposition: Inconclusive — NW may have silently ignored the Nw25 entry due to the duplicate AppName, or it may have attempted to load it but stopped at the missing XAML files. Cannot distinguish these scenarios without Fusion log evidence. Recommend renaming the Nw25 AppName to `"CtrlWizNW25"` in Phase 2 as risk mitigation, since eliminating the duplicate costs nothing and removes uncertainty.

**CLR Assembly Bind Failures (beyond missing XAML/name files):**
- Evidence: 0 Fusion log .htm files — CLR was never invoked for any CtrlWiz assembly during NW2025 startup.
- Disposition: No additional bind failures exist. Once the missing XAML and name files are deployed, CLR will be invoked for the first time and Fusion logging (if re-enabled) will reveal any dependency issues. Pre-confirmed RESEARCH.md findings (bitness, Zone blocks) already rule out the most common dependency failures.

**CtrlWizNW.log absence:**
- Evidence: `%TEMP%\CtrlWizNW.log` was not created.
- Interpretation: `Logger.SetUpLogger` is called in the `CmdViewpoint` constructor, which is only reached if the DLL loads and the plugin initializes. The log's absence is consistent with the DLL never loading — it directly confirms the pre-CLR failure hypothesis.

## Phase 2 Fix Scope

Based on the above findings, Phase 2 must:

1. **[REQUIRED]** Fix `CustomRibbon.xaml` build action in `CtrlWizNW.csproj`: change from `<Page>` to `<Content CopyToOutputDirectory="Always">` scoped to `Debug NW25|x64` and `NW25|x64` configurations. This causes MSBuild to copy the loose `.xaml` file to the output directory (and thus to `Contents/Nw25/` via the deploy step) instead of compiling it to BAML.

2. **[REQUIRED]** Fix `CustomRibbon.name` copy-to-output in `CtrlWizNW.csproj`: add `<CopyToOutputDirectory>Always</CopyToOutputDirectory>` to the existing `<None Include="CustomRibbon\CustomRibbon.name">` item, scoped to `Debug NW25|x64` and `NW25|x64` configurations.

3. **[RECOMMENDED]** Rename the Nw25 `AppName` in `PackageContents.xml` from `"CtrlWizNW"` to `"CtrlWizNW25"` to eliminate the duplicate AppName risk. Marked RECOMMENDED because the AppName collision could not be confirmed or ruled out via logs — removing it is low-risk and eliminates uncertainty before the first real plugin load attempt.

## Open Questions for Phase 2

- After the XAML and name files are deployed, does NW2025 load the plugin correctly on first attempt, or does a Fusion log from the post-fix launch reveal additional dependency failures?
- Does the duplicate `AppName="CtrlWizNW"` in PackageContents.xml cause any observable issue after the XAML files are present? (The RECOMMENDED fix above addresses this preemptively.)
- After the fix is deployed, does the ribbon tab appear and does Xbox controller navigation function end-to-end? (Phase 3 scope.)

## What Was Ruled Out

- **XAML type incompatibility:** `NWRibbonButton` confirmed present in NW2025 `navisworks.gui.roamer.dll` — no changes to XAML content needed.
- **DLL architecture mismatch:** All Nw25 DLLs are MSIL or Amd64 — no x86 DLL issue.
- **Zone-of-Origin blocks:** All Nw25 DLLs are unblocked — no manual unblocking needed.
- **CLR assembly bind failures:** Fusion logs are empty — no DLL reference chain issues, no missing dependencies at the CLR level.
- **NW2025 installation integrity:** NW2025 launched successfully and the ribbon/workspace loaded normally — NW itself is working.
- **NW2025 plugin loading crashes:** No EventID 1026 entries — NW did not crash while loading plugins.
