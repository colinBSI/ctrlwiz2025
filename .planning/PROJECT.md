# CtrlWiz NW25

## What This Is

CtrlWiz is an Xbox controller navigation plugin for Autodesk Navisworks and Revit. It maps gamepad input (thumbsticks, triggers, buttons) to 3D viewport navigation and model interaction commands. This milestone focuses on getting Navisworks 2025 support fully working — the plugin deploys but the ribbon tab does not appear, preventing users from activating controller navigation.

## Core Value

An engineer with Navisworks 2025 can activate CtrlWiz from the ribbon and navigate their model with an Xbox controller.

## Requirements

### Validated

- ✓ Xbox controller navigation works in Navisworks 2022 — existing
- ✓ Xbox controller navigation works in Revit 2019–2022 — existing
- ✓ Customizable button/trigger/thumbstick mappings via WPF settings UI — existing
- ✓ In-app licensing via Paddle SDK — existing
- ✓ File-based exception logging to %TEMP% — existing

### Active

- [ ] Navisworks 2025 ribbon tab appears on startup
- [ ] Xbox controller navigation functions in Navisworks 2025 (walk, orbit, pan, zoom)
- [ ] Button mappings and settings UI accessible in NW25

### Out of Scope

- MSI installer packaging for NW25 — separate deployment concern, not needed to validate the fix
- New controller features or mappings — focus is NW25 parity with NW22, not new capabilities
- Revit version updates — not related to this milestone

## Context

- **Bundle structure**: `C:\ProgramData\Autodesk\ApplicationPlugins\CtrlWiz.NW.bundle\` with `Contents/Nw22/` (MSI-installed) and `Contents/Nw25/` (deployed by Debug NW25|x64 build config)
- **PackageContents.xml**: Nw25 component entry added (commit 5e1fbf8) — `SeriesMin="Nw25" SeriesMax="Nw25"`, `ModuleName="./Contents/Nw25/CtrlWizNW.dll"`
- **XInput fix applied**: P/Invoke rewritten to target `xinput1_4.dll` (Windows system DLL) directly, no external DLL dependency (commit cc35155)
- **What's been checked**: DLLs confirmed present in Contents/Nw25/; plugin log and Navisworks journal not yet investigated
- **Machine**: Windows 11 Pro, not local admin by default (Admin By Request for elevation), Navisworks Manage 2025 v22.5.1433.58
- **Build config**: `Debug NW25|x64` auto-deploys to bundle on build; compile constant `NW2025` gates version-specific code

## Constraints

- **Compatibility**: Must target Navisworks 2025 API — version-gated via `NW2025` compile constant
- **Deployment**: No code-signing certificate; unsigned DLLs must load in NW2025
- **Elevation**: Writing to `C:\ProgramData\` requires Admin By Request elevation
- **No installer**: NW25 deployed manually for now — MSI packaging is out of scope

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| P/Invoke xinput1_4.dll directly | XInputInterface.dll missing; Windows system DLL always present | ✓ Good |
| Separate Nw25/ DLL folder in bundle | Matches Autodesk bundle convention for multi-version plugins | — Pending |
| SeriesMin/Max="Nw25" in PackageContents.xml | Required for NW2025 to recognize and load the plugin | — Pending |

---
*Last updated: 2026-05-03 after initialization*

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `/gsd-transition`):
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

**After each milestone** (via `/gsd-complete-milestone`):
1. Full review of all sections
2. Core Value check — still the right priority?
3. Audit Out of Scope — reasons still valid?
4. Update Context with current state
