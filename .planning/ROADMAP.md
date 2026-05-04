# Roadmap: CtrlWiz NW25

## Overview

Four phases take the NW25 plugin from an unknown failure state to a clean, hardened release: first confirm exactly what is broken (Diagnose), then fix the ribbon so the tab appears (Ribbon Fix), then verify all navigation and settings work end-to-end (Navigation Validation), and finally lock down the build pipeline so future NW version ports are straightforward (Build Hardening).

## Phases

**Phase Numbering:**
- Integer phases (1, 2, 3): Planned milestone work
- Decimal phases (2.1, 2.2): Urgent insertions (marked with INSERTED)

Decimal phases appear between their surrounding integers in numeric order.

- [x] **Phase 1: Diagnose** - Confirm the exact failure path before any code changes (completed 2026-05-03)
- [ ] **Phase 2: Ribbon Fix** - Make the CtrlWiz ribbon tab appear in Navisworks 2025
- [ ] **Phase 3: Navigation Validation** - Verify Xbox controller navigation and settings UI work end-to-end in NW2025
- [ ] **Phase 4: Build Hardening** - Ensure the NW25 build is clean, reliable, and easy to port to future NW versions

## Phase Details

### Phase 1: Diagnose
**Goal**: Confirm the exact failure path before writing any code
**Depends on**: Nothing (first phase)
**Requirements**: BUILD-02
**Success Criteria** (what must be TRUE):
  1. NW2025 journal and CLR Fusion log have been inspected and the bind failure (or absence thereof) is documented
  2. It is known whether NWRibbonButton type resolves in navisworks.gui.roamer.dll under NW2025
  3. It is confirmed which DLL and XAML artifacts are actually present in Contents/Nw25/ at build time (no stale Debug|x64 artifacts)
  4. A written diagnosis exists stating the specific root cause of the ribbon not appearing
**Plans**: 2 plans

Plans:
- [x] 01-01-PLAN.md — Snapshot Nw25 artifacts, enable Fusion logging, launch NW2025, capture all log sources
- [x] 01-02-PLAN.md — Analyze captured logs and write DIAGNOSIS.md with confirmed root cause

### Phase 2: Ribbon Fix
**Goal**: CtrlWiz ribbon tab appears in Navisworks 2025 on startup
**Depends on**: Phase 1
**Requirements**: RIBBON-01, RIBBON-02, BUILD-01
**Success Criteria** (what must be TRUE):
  1. CtrlWiz ribbon tab is visible in the NW2025 ribbon on startup without any error dialog
  2. All ribbon buttons (Activate, Settings, etc.) are present and correctly labelled
  3. Debug NW25|x64 build deploys both CtrlWizNW.dll and CustomRibbon resources to Contents/Nw25/ automatically on build
  4. The fix is confirmed by launching NW2025 on the dev machine
**Plans**: TBD
**UI hint**: yes

### Phase 3: Navigation Validation
**Goal**: Xbox controller navigation and settings UI work end-to-end in NW2025
**Depends on**: Phase 2
**Requirements**: NAV-01, NAV-02, NAV-03, NAV-04, SET-01, SET-02
**Success Criteria** (what must be TRUE):
  1. User can activate CtrlWiz from the NW2025 ribbon button without error
  2. Thumbstick inputs produce the expected viewport navigation (walk, orbit, pan, zoom) in NW2025
  3. Trigger and face button inputs execute their mapped Navisworks commands in NW2025
  4. User can open the button-mapping settings UI from the ribbon in NW2025
  5. Button mappings saved in one NW2025 session are present when NW2025 is relaunched
**Plans**: TBD

### Phase 4: Build Hardening
**Goal**: NW25 build pipeline is clean, reliable, and self-documenting for future NW version ports
**Depends on**: Phase 3
**Requirements**: (none — hardens BUILD-01 and BUILD-02 already validated)
**Success Criteria** (what must be TRUE):
  1. Logger.SetUpLogger is called early enough that plugin load-time failures appear in the log
  2. A code comment in the project file or XAML documents the CustomRibbon copy pattern for the next NW version port
  3. Dead assembly references (WatiN.Core.dll, CredentialManagement.dll) are either removed or confirmed necessary with a comment
**Plans**: TBD

## Progress

**Execution Order:**
Phases execute in numeric order: 1 → 2 → 3 → 4

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Diagnose | 2/2 | Complete | 2026-05-03 |
| 2. Ribbon Fix | 0/? | Not started | - |
| 3. Navigation Validation | 0/? | Not started | - |
| 4. Build Hardening | 0/? | Not started | - |
