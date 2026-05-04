---
phase: 01-diagnose
plan: 01-02
status: complete
completed: 2026-05-03
---

# Summary: 01-02 Log Analysis and Diagnosis

## Objective Achieved

All captured log sources from Plan 01-01 have been analysed and `01-DIAGNOSIS.md` has been written with a confirmed root cause, filled evidence table, and Phase 2 fix scope.

## Key Output

- **Document:** `.planning/phases/01-diagnose/01-DIAGNOSIS.md`

## Confirmed Root Cause (one sentence)

`CustomRibbon.xaml` and `CustomRibbon.name` are missing from `Contents/Nw25/` because the `CtrlWizNW.csproj` build items use `<Page>` (compiles XAML to embedded BAML) and `<None>` (never copied) instead of `<Content CopyToOutputDirectory="Always">` for the NW25 configurations.

## Secondary Issues Found

**Yes — one inconclusive secondary issue:**
- Duplicate `AppName="CtrlWizNW"` in PackageContents.xml for both Nw22 and Nw25 entries. Cannot be confirmed or ruled out as a contributing factor because CLR Fusion logs are empty (NW never reached CLR loading). Marked RECOMMENDED to rename to `"CtrlWizNW25"` in Phase 2 as risk mitigation.

**No additional CLR bind failures:** Fusion log was empty — no dependency chain issues at the CLR level.

## Phase 2 Fix Scope

3 action items:

1. **[REQUIRED]** Change `CustomRibbon.xaml` build action from `<Page>` to `<Content CopyToOutputDirectory="Always">` in `CtrlWizNW.csproj` for NW25 configs.
2. **[REQUIRED]** Add `<CopyToOutputDirectory>Always</CopyToOutputDirectory>` to `CustomRibbon.name` `<None>` item in `CtrlWizNW.csproj` for NW25 configs.
3. **[RECOMMENDED]** Rename `AppName="CtrlWizNW"` to `"CtrlWizNW25"` in the Nw25 component entry of `PackageContents.xml`.

## Open Questions for Phase 2

- After the XAML and name files are deployed, does Fusion logging reveal any further dependency failures on the first real plugin load?
- Does the duplicate AppName cause any observable issue once the XAML files are present?

## Self-Check: PASSED

- [x] Fusion log .htm files checked — 0 files (CLR never invoked; pre-CLR failure confirmed)
- [x] 01-DIAGNOSIS.md written with all sections filled from actual evidence
- [x] Root Cause A (missing CustomRibbon files) formally confirmed
- [x] AppName collision dispositioned as Inconclusive with RECOMMENDED fix
- [x] No CLR bind failures — explicitly stated "no additional failures found"
- [x] Phase 2 fix scope defined with REQUIRED vs RECOMMENDED labels
- [x] No unfilled bracket placeholders remain in the document
- [x] BUILD-02 satisfied: diagnosis confirms which artifacts are missing and why, enabling a pure MSBuild fix in Phase 2
