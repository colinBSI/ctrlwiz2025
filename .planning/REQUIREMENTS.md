# Requirements: CtrlWiz NW25

**Defined:** 2026-05-03
**Core Value:** An engineer with Navisworks 2025 can activate CtrlWiz from the ribbon and navigate their model with an Xbox controller.

## v1 Requirements

### Ribbon

- [ ] **RIBBON-01**: CtrlWiz ribbon tab appears in Navisworks 2025 on startup without error
- [ ] **RIBBON-02**: All ribbon buttons are visible and correctly labelled in NW2025

### Navigation

- [ ] **NAV-01**: User can activate the CtrlWiz controller from the NW2025 ribbon button
- [ ] **NAV-02**: Xbox controller thumbsticks navigate the NW2025 viewport per user mapping (walk, orbit, pan, zoom)
- [ ] **NAV-03**: Xbox controller triggers execute mapped Navisworks commands in NW2025
- [ ] **NAV-04**: Xbox controller face buttons execute mapped Navisworks commands in NW2025

### Settings

- [ ] **SET-01**: User can open the button-mapping settings UI from the NW2025 ribbon
- [ ] **SET-02**: User's button mappings persist between NW2025 sessions

### Build

- [ ] **BUILD-01**: Debug NW25|x64 build config deploys all required files (DLLs + XAML + .name resource) to the bundle
- [ ] **BUILD-02**: NW25 support can be built and deployed without running the MSI installer

## v2 Requirements

### Distribution

- **DIST-01**: MSI installer packages and deploys NW25 DLLs and resource files to end-user machines
- **DIST-02**: CtrlWizNWSetup .vdproj updated with NW2025 component entries

### Diagnostics

- **DIAG-01**: Logger.SetUpLogger called early enough to capture plugin load-time failures
- **DIAG-02**: Startup log entry confirms NW version and bundle path on each launch

## Out of Scope

| Feature | Reason |
|---------|--------|
| MSI installer for NW25 | Separate deployment concern; focus is validating the fix first |
| New controller features or mappings | This milestone is NW25 parity with NW22, not new capabilities |
| Revit version updates | Not related to NW25 ribbon issue |
| NW2022 changes | Already working; do not regress |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| RIBBON-01 | — | Pending |
| RIBBON-02 | — | Pending |
| NAV-01 | — | Pending |
| NAV-02 | — | Pending |
| NAV-03 | — | Pending |
| NAV-04 | — | Pending |
| SET-01 | — | Pending |
| SET-02 | — | Pending |
| BUILD-01 | — | Pending |
| BUILD-02 | — | Pending |

**Coverage:**
- v1 requirements: 10 total
- Mapped to phases: 0 (roadmap pending)
- Unmapped: 10 ⚠️

---
*Requirements defined: 2026-05-03*
*Last updated: 2026-05-03 after initial definition*
