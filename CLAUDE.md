# CtrlWiz NW25 — Project Guide

## Project

Xbox controller navigation plugin for Autodesk Navisworks. This milestone: get NW2025 support fully working (ribbon tab + controller navigation).

**Core Value:** An engineer with Navisworks 2025 can activate CtrlWiz from the ribbon and navigate their model with an Xbox controller.

## GSD Workflow

This project uses GSD (Get Shit Done) for structured planning and execution.

**Planning artifacts:**
- `.planning/PROJECT.md` — project context and requirements
- `.planning/REQUIREMENTS.md` — v1 requirements with REQ-IDs
- `.planning/ROADMAP.md` — 4-phase execution plan
- `.planning/research/` — NW2025 plugin research findings

**Current milestone:** NW25 ribbon + navigation fix (4 phases)

**Phase progression:**
```
Phase 1: Diagnose → Phase 2: Ribbon Fix → Phase 3: Navigation Validation → Phase 4: Build Hardening
```

**Commands:**
- `/gsd-plan-phase 1` — plan Phase 1 (Diagnose)
- `/gsd-execute-phase 1` — execute Phase 1
- `/gsd-progress` — check current progress

## Key Technical Context

- **Bundle path:** `C:\ProgramData\Autodesk\ApplicationPlugins\CtrlWiz.NW.bundle\`
- **NW25 DLLs:** `Contents/Nw25/` — deployed by `Debug NW25|x64` build config
- **Root cause (HIGH confidence):** `CustomRibbon.xaml` and `CustomRibbon.name` are missing from `Contents/Nw25/` — the `[RibbonLayout]` attribute expects them as loose files alongside the DLL
- **Secondary risk:** `CustomRibbon.xaml` uses `NWRibbonButton` from `navisworks.gui.roamer.dll` (Autodesk-internal) — verify type still exists in NW2025 before fixing
- **XInput:** P/Invoke targets `xinput1_4.dll` directly (Windows system DLL) — no external dependency needed

## Build Configs

| Config | Target | Output |
|--------|--------|--------|
| `Debug NW25\|x64` | NW2025 API | Auto-deploys to `CtrlWiz.NW.bundle\Contents\Nw25\` |
| `NW25\|x64` | NW2025 API | `CtrlWiz.Release.Assemblies\Navisworks\NW25\` |
| `Debug\|x64` | NW2022 API | **Do not use for NW25** |

## Constraints

- No code signing certificate — unsigned DLLs must load
- Admin elevation required to write to `C:\ProgramData\` (Admin By Request)
- .NET Framework 4.8 — NW2025 still requires it (no .NET 6+ migration)
- Do not regress NW2022 support
