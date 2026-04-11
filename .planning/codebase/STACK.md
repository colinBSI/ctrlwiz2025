# Technology Stack

**Analysis Date:** 2026-04-11

## Languages

**Primary:**
- C# - Desktop application development for Revit and Navisworks plugins
- XAML - UI markup for WPF interfaces

**Secondary:**
- VB.NET - Support within MSBuild and setup projects (legacy/installer components)

## Runtime

**Environment:**
- .NET Framework 4.8 - Core runtime target across all projects
- Windows-only deployment (plugin architecture for Autodesk applications)

**Application Types:**
- Class libraries (.dll) - Primary output type for plugin assemblies
- Windows Presentation Foundation (WPF) - UI components
- Windows Forms - Legacy UI components for some dialogs

## Frameworks

**Core:**
- Autodesk Navisworks API (2017-2022) - Plugin integration for Navisworks Manage/Simulate
- Revit API (2019-2022) - Plugin integration for Revit
- WPF (Windows Presentation Foundation) - Desktop UI framework for modern interfaces
- Windows Forms - Legacy UI framework for compatibility

**UI & Presentation:**
- MVVM Light (5.4.1.1) - Model-View-ViewModel pattern implementation
- Custom XAML Ribbon controls - Navigation UI for Autodesk applications

**Installation & Setup:**
- Visual Studio Installer Projects (.vdproj) - WiX-based installers for CtrlWizNWSetup and CtrlWizRVTSetup
- Custom MSI actions for directory permissions and installation hooks

**Input Handling:**
- InputSimulator (1.0.4.0) - Xbox controller input simulation to Windows
- XInputDotNetPure (custom) - Low-level Xbox controller input reading

## Key Dependencies

**Critical:**
- Autodesk.Navisworks.Api - Core Navisworks plugin integration
- RevitAPI - Core Revit plugin integration
- Paddle.PaddleSDK (2.0.5) - License verification and payment processing
- Newtonsoft.Json (11.0.2, binding redirect to 12.0.0.0) - JSON serialization for license and config data

**Infrastructure:**
- CredentialManagement (1.0.2) - Windows Credential Manager integration for secure token storage
- StructureMap (2.6.3) - Dependency injection container
- WatiN (2.1.0) - Browser automation for license verification workflows
- WindowsInput - System-level input APIs for controller simulation

**System Interop:**
- System.ComponentModel.Composition - MEF plugin architecture
- Interop.SHDocVw - COM interop for Internet Explorer automation (license verification)
- Microsoft.mshtml - COM interop for HTML rendering

## Configuration

**Environment:**
- Assembly binding redirects configured in app.config files for assembly version compatibility
- Multi-configuration build: Debug, Release, NW17-NW22 (Navisworks versions), RVT19-RVT22 (Revit versions)
- Conditional compilation symbols (NW2017, NW2018, etc., RVT19, RVT20, etc.) for version-specific code

**Build:**
- MSBuild project format (.csproj) - All C# projects
- Custom property groups per configuration for Autodesk SDK path injection:
  - `$(AdWindowsPath)`, `$(AutodeskNavisworksApiPath)`, `$(AutodeskNavisworksControlsPath)` (Navisworks)
  - `$(RevitAPIPath)`, `$(RevitAPIUIPath)` (Revit)
- Code signing enabled - Post-build event uses signtool.exe with timestamp authority (comodoca)

**Signing & Security:**
- Certificate file: `VIATechnik_CS_cert.pfx` - Code signing certificate for all assemblies
- Timestamp authority: http://timestamp.comodoca.com/?td=sha256
- All output assemblies signed with SHA256

## Platform Requirements

**Development:**
- Visual Studio 2019 or later (ToolsVersion 15.0)
- Windows SDK (10.App Certification Kit) - For signtool.exe code signing
- Autodesk Navisworks 2017-2022 SDKs installed (referenced by environment variable paths in build config)
- Revit 2019-2022 SDKs installed (referenced by environment variable paths in build config)
- .NET Framework 4.8 Developer Pack

**Production:**
- Windows (7/8/10/11) - Plugin architecture targets Windows Autodesk applications
- .NET Framework 4.8 runtime
- Autodesk Navisworks Manage or Simulate (2017-2022) OR Revit (2019-2022)
- Xbox controller hardware (XInput-compatible device)
- Administrator permissions for installation and directory permission configuration

**Bundle/Deployment:**
- CtrlWiz.bundle folder structure for Navisworks (ApplicationPlugins directory)
- Revit AddIn manifest and DLL placement in Revit AddIns directory
- Installer creates application plugin bundles in Autodesk installation directories

---

*Stack analysis: 2026-04-11*
