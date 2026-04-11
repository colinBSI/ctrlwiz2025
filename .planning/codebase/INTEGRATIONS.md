# External Integrations

**Analysis Date:** 2026-04-11

## APIs & External Services

**Payment & Licensing:**
- Paddle - License verification, in-app checkout, and payment processing
  - SDK/Client: `Paddle.PaddleSDK` (2.0.5)
  - Implementation: `CtrlWizLicense/InAppCheckout.cs`
  - Auth: API key and Vendor ID stored in embedded resources (`Resources.ApiKey`, `Resources.VendorId`)
  - Purpose: License activation, product verification, checkout dialogs for both Revit and Navisworks versions

**Code Signing:**
- Comodoca/DigiCert Timestamp Authority - Authenticates signed assemblies with trusted timestamp
  - Endpoint: `http://timestamp.comodoca.com/?td=sha256`
  - Purpose: Ensures code signature validity across time

## Data Storage

**Databases:**
- None detected - No direct database integration

**File Storage:**
- Local filesystem only
  - License state files stored in: Windows Temp directory and platform-specific paths
  - Log files: `CtrlWizNW.log`, `CtrlWizRVT.log`, `CtrlWiz.log` in temp directory
  - Implementation: `CtrlWiz.Logging/Logger.cs`

**Caching:**
- Windows Credential Manager integration
  - Storage: `CredentialManagement` (1.0.2) wrapper around Windows native credential storage
  - Purpose: Secure token and license credential caching
  - Implementation: `CtrlWizLicense` project

## Authentication & Identity

**Auth Provider:**
- Paddle SDK-based licensing - Custom implementation
  - Implementation files:
    - `CtrlWizLicense/InAppCheckout.cs` - License checkout and activation workflow
    - `CtrlWizLicense/CtrlWizPaddle.cs` - Paddle product interface (stub implementation)
    - `CtrlWizLicense/ActivationChangedArgs.cs` - License activation event args
    - `CtrlWizLicense/VerifyActivationCompletedArgs.cs` - Verification completion event args
  - Approach: In-app checkout with Paddle SDK for license verification
  - Product configuration:
    - Navisworks: `ProductIdNW`, `ProductNameNW` (stored in resources)
    - Revit: `ProductIdRVT`, `ProductNameRVT` (stored in resources)

**Credentials:**
- Windows Credential Manager - Stores license tokens securely
- Credential lookup/storage handled by `CredentialManagement` library
- No hardcoded credentials in source

## Monitoring & Observability

**Error Tracking:**
- None detected - No external error reporting service integrated

**Logs:**
- Local file logging approach
  - Implementation: `CtrlWiz.Logging/Logger.cs`
  - Location: Windows Temp directory (`Path.GetTempPath()`)
  - Files:
    - Navisworks: `CtrlWizNW.log`
    - Revit: `CtrlWizRVT.log` (stored in parent temp directory)
  - Format: Timestamp (UTC+0), exception message, inner exception info
  - Encoding: UTF-8
  - Append-only file logging with error extension utilities

## CI/CD & Deployment

**Hosting:**
- Local installer distribution - No cloud hosting detected
- Windows Installer (MSI) packages created via Visual Studio setup projects

**CI Pipeline:**
- None detected - Build is manual through Visual Studio/MSBuild

**Build Artifact Output:**
- Release assemblies organized in: `..\CtrlWiz.Release.Assemblies\` directory structure
- Configuration-specific outputs: `Navisworks\$(Configuration)\`, `Revit\$(Configuration)\`

**Code Signing:**
- All assemblies signed post-build with signtool.exe
- Certificate: `VIATechnik_CS_cert.pfx`
- Timestamp: Comodoca authority ensures signature validity

## Environment Configuration

**Required env vars:**
- No environment variables required at runtime
- Build-time SDK paths are configuration-driven:
  - Navisworks SDK path: `$(ProgramW6432)\Autodesk\Navisworks Manage <YEAR>\*.dll`
  - Revit SDK path: `$(ProgramW6432)\Autodesk\Revit <YEAR>\*.dll`
- These are resolved by MSBuild property groups per build configuration

**Secrets location:**
- Paddle API Key and Vendor ID: Embedded in compiled resources
  - File: `CtrlWizLicense/Properties/Resources.resx` (not directly readable in source, compiled into assembly)
- License tokens: Windows Credential Manager (secure OS-level storage)
- Code signing certificate: `VIATechnik_CS_cert.pfx` (in repository but should be rotated before public use)

## Webhooks & Callbacks

**Incoming:**
- None detected

**Outgoing:**
- Paddle license verification callbacks
  - Triggered by: In-app checkout completion, license activation status changes
  - Implementation: `InAppCheckoutStatus` enum and event args classes
  - Events: `ActivationChangedArgs`, `CheckoutCompletedArgs`, `VerifyActivationCompletedArgs`

## Browser Automation Integration

**WatiN Browser Automation:**
- Library: `WatiN` (2.1.0) - Web automation framework for license verification workflows
- Interop assemblies: `Interop.SHDocVw`, `Microsoft.mshtml`
- Purpose: Automate browser-based license verification steps within checkout flow
- Implementation: Used by Paddle integration for browser automation during licensing

## Input Hardware Integration

**Xbox Controller Input:**
- Input simulation: `InputSimulator` (1.0.4.0)
- Raw input handling: `XInputDotNetPure` (custom library)
- Purpose: Read Xbox controller state and simulate keyboard/mouse input for Revit/Navisworks navigation
- Implementation files:
  - `XInputDotNetPure/XInputDotNetPure.csproj`
  - `CtrlWizNW/packages.config` - References InputSimulator
- Target: Windows XInput-compatible devices

## Plugin Architecture Integration

**Navisworks Plugin:**
- Autodesk Navisworks API (2017-2022 versions)
- Integration point: Custom ribbon UI via `CtrlWizNW/CustomRibbon/`
- Manifest: `CustomRibbon/PackageContents.xml`
- Plugin bundle: `.bundle\Contents\v18\` deployment structure

**Revit Plugin:**
- Revit API (2019-2022 versions)
- Integration point: Revit AddIn manifest and API hooks
- Custom actions: Directory permissions setup via `CtrlWizDirectoryPermissionCustomAction`

---

*Integration audit: 2026-04-11*
