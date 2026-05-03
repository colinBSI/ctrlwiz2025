# Stack Research — NW2025 Plugin Diagnostic Tooling

**Domain:** Autodesk Navisworks 2025 managed plugin — load failure / ribbon not appearing
**Researched:** 2026-05-03
**Confidence:** HIGH (primary sources: NW2025 API XML, deployed bundle inspection, source code, NW install directory)

---

## The Diagnostic Stack, Ordered by Signal Quality

Work through these in order. Each layer catches a different class of failure.

---

## 1. CtrlWiz Plugin Log (Highest Signal — Check First)

**What it is:** The plugin's own exception log, written by `CtrlWiz.Logging\Logger.cs`.

**Log path:**
```
%TEMP%\CtrlWizNW.log
```
Expands to: `C:\Users\cemch-wei\AppData\Local\Temp\CtrlWizNW.log`

**Critical caveat:** `Logger.SetUpLogger(LoggedApp.NW)` must be called before any exception is logged. If the plugin fails during static initialization or before `SetUpLogger` runs (e.g., a `TypeLoadException` during class loading), **nothing will be written here**. An empty or absent log means the failure happened before the logger was ever initialized — which is actually informative.

**How to check:**
1. Close Navisworks, delete or rename the existing log to start clean.
2. Launch Navisworks 2025. Wait for full startup.
3. Open `%TEMP%\CtrlWizNW.log` in Notepad.

**What to look for:**
- Any content at all — even a single line confirms the plugin DLL loaded and `CmdViewpoint`'s static context initialized.
- `FileNotFoundException`, `DllNotFoundException`, `TypeLoadException` — these indicate a missing dependency.
- Paddle/licensing exceptions during startup — `InAppCheckout` is wired early in the command flow.
- If the file does not exist or is empty: the plugin DLL was never loaded by NW, or crashed before `SetUpLogger` ran.

**Confidence:** HIGH — this is source-verified behavior.

---

## 2. Navisworks Journal File (Best for Manifest/Discovery Failures)

**What it is:** Navisworks writes a journal (macro recording file) for every session. It records application events including plugin discovery messages.

**Log path:**
```
%LOCALAPPDATA%\Autodesk\Navisworks Manage 2025\Journals\
```
Expands to: `C:\Users\cemch-wei\AppData\Local\Autodesk\Navisworks Manage 2025\Journals\`

Files are named by session timestamp, e.g. `journal.{timestamp}.nwjsf`.

**How to check:**
1. Launch Navisworks 2025, then close it.
2. Navigate to the Journals directory.
3. Open the most recent `.nwjsf` file in a text editor (it is plain text).
4. Search for `CtrlWiz`, `PackageContents`, `plugin`, or `error`.

**What to look for:**
- Lines referencing `CtrlWiz.NW.bundle` — confirms the manifest was read.
- `Failed to load` or `Error loading plugin` lines — NW logs manifest parse failures here.
- Absence of any CtrlWiz reference — the manifest was not found or not parsed at all (check bundle location and PackageContents.xml schema).
- `SeriesMin`/`SeriesMax` mismatch errors — if NW25 does not recognize `Nw25` as a valid series token.

**Confidence:** HIGH — journal files are a standard NW developer diagnostic tool.

---

## 3. Windows Application Event Log (Assembly Bind Failures)

**What it is:** The .NET CLR logs assembly binding failures and managed exceptions that escape into the host process under `Windows Logs > Application`.

**How to check:**
1. Open Event Viewer: `Win+R` → `eventvwr.msc`.
2. Navigate to `Windows Logs > Application`.
3. After launching NW2025, filter by Source = `.NET Runtime` or `Application Error`.
4. Look for entries timestamped during the NW startup window.

**What to look for:**
- Source `.NET Runtime`, EventID `1026`: unhandled managed exception in `roamer.exe`. The message will contain the exception type and stack trace. This is the clearest possible signal — you will see exactly which DLL failed to load and why.
- Source `Application Error`, EventID `1000`: native crash. Less likely for a managed plugin failure, but possible if a P/Invoke goes wrong at load time (e.g., `DllNotFoundException` for a native dependency).
- Message pattern: `System.IO.FileNotFoundException: Could not load file or assembly 'XYZ' or one of its dependencies`.

**Confidence:** HIGH — this is how .NET assembly load failures surface when the plugin host (roamer.exe) does not catch them.

---

## 4. Fusion / Autodesk Desktop App Logs

**What it is:** Autodesk's application framework (Fusion/ADF) manages the `ApplicationPlugins` folder scanning. It may log plugin discovery independently from Navisworks.

**Log path candidates:**
```
%LOCALAPPDATA%\Autodesk\Web Services\Logs\
%APPDATA%\Autodesk\ApplicationPlugins\
%LOCALAPPDATA%\Autodesk\ODIS\
```

**How to check:**
1. Look in `%LOCALAPPDATA%\Autodesk\Web Services\Logs\` for recent log files (`.log` extension).
2. Search for `CtrlWiz` or `PackageContents` in those files.

**Realistic signal:** The Fusion layer primarily handles cloud/web-connected plugin distribution (Autodesk App Store). For locally deployed `ApplicationPlugins` bundles, NW reads `PackageContents.xml` directly without going through Fusion. **Low probability of useful signal here** for a manually deployed bundle — check this last.

**Confidence:** MEDIUM — Fusion log paths are based on known Autodesk infrastructure patterns, but their relevance to local bundle failures is LOW.

---

## 5. .NET Assembly Bind Log (fuslogvw / FUSION_LOG)

**What it is:** The .NET framework's assembly binding log records every successful and failed assembly resolution attempt. This catches `DllNotFoundException`, missing GAC entries, version mismatches, and processor architecture conflicts.

**How to enable (requires elevation):**

Option A — Registry (most reliable for in-process logging):
```powershell
# Requires Admin By Request elevation
reg add "HKLM\SOFTWARE\Microsoft\.NETFramework" /v EnableLog /t REG_DWORD /d 1 /f
reg add "HKLM\SOFTWARE\Microsoft\.NETFramework" /v LogPath /t REG_SZ /d "C:\FusionLogs" /f
reg add "HKLM\SOFTWARE\Microsoft\.NETFramework" /v ForceLog /t REG_DWORD /d 1 /f
reg add "HKLM\SOFTWARE\Microsoft\.NETFramework" /v LogFailures /t REG_DWORD /d 1 /f
mkdir C:\FusionLogs
```

Option B — fuslogvw.exe (GUI, ships with Windows SDK):
- Launch `fuslogvw.exe` from Visual Studio Developer Command Prompt.
- Settings > Log bind failures to disk.
- Set a custom log path.

**After enabling:**
1. Launch Navisworks 2025 and wait for full startup.
2. Close NW.
3. Disable logging (reverse the registry keys above — leave these enabled only during diagnosis).
4. Browse `C:\FusionLogs\` — there will be subdirectories per process, with HTML files for each assembly bind attempt.

**What to look for:**
- Failed binds to `XInputDotNetPure`, `CtrlWizNW`, `CtrlWizNW.UI`, `GalaSoft.MvvmLight`, `PaddleSDK`, `WindowsInput`, `CredentialManagement`.
- `HRESULT 0x80070002` (file not found) — DLL is not in the probing path.
- `HRESULT 0x80131040` (version mismatch) — DLL found but wrong version or public key token.
- Processor architecture mismatch: `AnyCPU` DLL refusing to load in an x64 host, or vice versa. The NW25 build config uses `PlatformTarget=x64`; confirm all dependency DLLs are x64-compatible.

**This is the highest-precision diagnostic tool** for dependency resolution failures. If the ribbon is missing and the CtrlWizNW.log is empty, enable this before anything else.

**Confidence:** HIGH — standard .NET diagnostic, fully applicable to NW as a CLR host.

---

## 6. Process Monitor (Procmon) — File System Access Patterns

**What it is:** Sysinternals Process Monitor captures every file system, registry, and network operation. Useful when you need to see exactly which paths NW tries to read during bundle discovery.

**Tool:** Download from https://learn.microsoft.com/en-us/sysinternals/downloads/procmon (requires elevation to run).

**Filter configuration:**
```
Process Name is roamer.exe
Path contains Autodesk\ApplicationPlugins
Operation is ReadFile OR CreateFile OR QueryBasicInformationFile
```

**How to use:**
1. Start Procmon, configure filter above, clear the capture buffer.
2. Launch Navisworks 2025.
3. Once NW is fully loaded, stop capture.
4. Filter results to `roamer.exe` + path containing `CtrlWiz`.

**What to look for:**
- `NAME NOT FOUND` on `PackageContents.xml` — NW looked for the manifest but didn't find it (wrong path or filename).
- `NAME NOT FOUND` on `Contents\Nw25\CtrlWizNW.dll` — NW found the manifest, parsed the Nw25 component entry, but the DLL path is wrong relative to the bundle root.
- `ACCESS DENIED` on any file in `C:\ProgramData\Autodesk\ApplicationPlugins\` — a permissions problem (unlikely given the bundle exists, but possible).
- `NAME NOT FOUND` on dependency DLLs (`XInputDotNetPure.dll`, `GalaSoft.MvvmLight.dll`, etc.) being probed in system directories — indicates the CLR is not finding them in the bundle folder.
- Successful `ReadFile` on `PackageContents.xml` followed by no attempt to open `CtrlWizNW.dll` — suggests a manifest parse/validation error (SeriesMin/SeriesMax mismatch, or `AppType` string not recognized).

**Confidence:** HIGH — Procmon is the definitive tool for "what did NW actually try to open."

---

## 7. NW API: Enumerate Loaded Plugins Programmatically

**What it is:** The Navisworks managed API exposes `Application.Plugins.PluginRecords` — a collection of all `PluginRecord` objects that NW discovered at startup. Each record has `IsLoaded` and `IsEnabled` properties, and `LoadPlugin()` / `TryLoadPlugin()`. If the plugin's `PluginRecord` appears in the collection but `IsLoaded = false`, the failure is in loading (DLL/dependency), not in manifest discovery.

**Diagnostic plugin approach:**
Write a minimal second NW plugin (or a Navisworks Automation script) that prints the contents of `Application.Plugins.PluginRecords` to a file:

```csharp
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Plugins;
using System.IO;

// In a CommandHandlerPlugin.Execute():
var sb = new System.Text.StringBuilder();
foreach (PluginRecord pr in Application.Plugins.PluginRecords)
{
    sb.AppendLine($"{pr.Id} | IsLoaded={pr.IsLoaded} | IsEnabled={pr.IsEnabled}");
}
File.WriteAllText(@"C:\Temp\nw_plugins.txt", sb.ToString());
```

**What to look for:**
- `NVSXBOX.CmdViewpoint.VIATechnik` present in the list with `IsLoaded=false` — plugin discovered, DLL not yet loaded (lazy load). Try calling `TryLoadPlugin()` and catching `PluginLoadException`.
- `NVSXBOX.CmdViewpoint.VIATechnik` absent entirely — the manifest was not parsed, or the `SeriesMin/SeriesMax` filter excluded the component entry.
- `PluginLoadException` from `TryLoadPlugin()` — will carry an `InnerException` with the actual CLR failure (usually a `FileNotFoundException` or `TypeLoadException`).

**Note:** This approach requires a working plugin to host the diagnostic code. Use NW Automation API (`Autodesk.Navisworks.Automation`) as an alternative if you can't load another plugin.

**Confidence:** HIGH — this is documented NW API behavior (confirmed in `Autodesk.Navisworks.Api.xml`).

---

## 8. Visual Studio Debugger — Attach to roamer.exe

**What it is:** Attach VS2022 to the running `roamer.exe` process with "mixed mode" (managed + native) debugging enabled.

**How to use:**
1. Build the solution with `Debug NW25|x64` configuration (already done — this produces debug symbols and the PDB files are in `Contents\Nw25\`).
2. Launch Navisworks 2025.
3. In Visual Studio: `Debug > Attach to Process > roamer.exe`. Check "Managed (.NET Framework 4.x)" code type.
4. Set a breakpoint in `CmdViewpoint.cs` — either the static constructor or the class declaration.
5. In NW, attempt to trigger the plugin (e.g., click where the CtrlWiz tab would be, or use NW's Options > Load Plugins if available).

**Alternative — first-chance exceptions:**
In VS with roamer.exe attached: `Debug > Windows > Exception Settings`. Enable "Common Language Runtime Exceptions > Thrown". This will break on the exact line that throws when the CLR tries to load any dependency.

**What to look for:**
- `FileNotFoundException` on a dependency DLL — tells you exactly which file is missing.
- `TypeLoadException` — a type in one of the DLLs references a type that no longer exists in the NW2025 API version of the assembly (API breaking change between NW22 and NW25).
- `BadImageFormatException` — architecture mismatch (x86 DLL loaded in x64 process, or vice versa).

**The `Debug NW25|x64` config produces full debug symbols** (confirmed: `DebugType=full`, `DebugSymbols=true`, `Optimize=false`), so you will get source-level breakpoints in `CmdViewpoint.cs`.

**Confidence:** HIGH — PDB files are present in the bundle, debugger attach is standard for managed plugin development.

---

## 9. ILSpy / dotPeek — Verify Compiled Assembly Targets

**What it is:** Decompilers that show the compiled .NET target framework, processor architecture, and referenced assembly versions of a DLL.

**Use for:**
- Confirm `CtrlWizNW.dll` in `Contents\Nw25\` is compiled against the NW2025 API version (not the NW2022 API).
- Verify the assembly's `TargetFramework` attribute is `.NETFramework,Version=v4.8`.
- Check that the `Autodesk.Navisworks.Api` reference in `CtrlWizNW.dll`'s manifest points to the NW2025 version number, not NW2022.

**How to use:**
1. Install ILSpy (free, https://github.com/icsharpcode/ILSpy) or JetBrains dotPeek.
2. Open `C:\ProgramData\Autodesk\ApplicationPlugins\CtrlWiz.NW.bundle\Contents\Nw25\CtrlWizNW.dll`.
3. Check References — what version of `Autodesk.Navisworks.Api` is referenced? It should match the NW2025 install (not NW2022's version).
4. Check the `Assembly` node → `[assembly: TargetFramework]` attribute.

**If the DLL references the NW2022 version of the API:** The `Debug NW25|x64` build config resolves against `Navisworks Manage 2025\Autodesk.Navisworks.Api.dll` (confirmed in `CtrlWizNW.csproj` lines 178-180), so a freshly built `Debug NW25|x64` DLL should reference the correct version. But if an older build artifact is sitting in the bundle from a previous `Debug|x64` build (which references NW2022), this would explain the failure.

**Confidence:** HIGH — source-verified from the csproj.

---

## Recommended Diagnostic Sequence

Given the current state (DLLs present, PackageContents.xml correct, ribbon not appearing):

| Step | Action | Time | Catches |
|------|--------|------|---------|
| 1 | Check `%TEMP%\CtrlWizNW.log` | 2 min | Exceptions that happened after DLL load |
| 2 | Check Windows Event Log (Application, .NET Runtime, EventID 1026) | 3 min | Unhandled CLR exceptions during startup |
| 3 | Check NW journal in `%LOCALAPPDATA%\Autodesk\Navisworks Manage 2025\Journals\` | 5 min | Manifest parse failures, plugin discovery errors |
| 4 | Enable .NET Assembly Bind Log, relaunch NW, inspect `C:\FusionLogs\` | 15 min | Missing/wrong-version dependencies |
| 5 | Run Procmon with roamer.exe filter | 20 min | File not found at the path NW actually probes |
| 6 | Attach VS debugger to roamer.exe with first-chance CLR exceptions | 20 min | Exact line/DLL causing load failure |
| 7 | Open CtrlWizNW.dll in ILSpy — verify API version references | 5 min | Wrong build artifact in bundle |

---

## NW2025 vs NW2022: Known Differences Relevant to Plugin Loading

| Concern | NW2022 | NW2025 | Impact |
|---------|--------|--------|--------|
| .NET target | .NET Framework 4.8 | .NET Framework 4.8 | No change required |
| DLL signing | Not required | Not required | `SignAssembly=false` in csproj is fine |
| PackageContents SeriesMin/SeriesMax | `Nw22` | `Nw25` | Already correct in deployed XML |
| AppType | `ManagedPlugin` | `ManagedPlugin` | No change |
| Platform token | `NAVMAN\|NAVSIM` | `NAVMAN\|NAVSIM` | Already correct |
| xinput dependency | XInputInterface.dll (custom) | xinput1_4.dll (system) | Fixed in commit cc35155 |
| API breaking changes | — | Unknown — NW2025 may have changed internal API surface | TypeLoadException risk |
| `navisworks.gui.roamer.dll` internal commands | Undocumented IDs | May differ or be removed | HIGH risk if these fail silently |
| Navisworks version number | v18 series | v22.x series | PackageContents uses `Nw22`/`Nw25` strings, not version numbers |

**Highest-risk items for NW2025 breakage:**

1. **API version mismatch in the compiled DLL.** If the `Debug NW25|x64` build was not the last build deployed, an older artifact compiled against NW2022 APIs may be sitting in `Contents\Nw25\`. Verify with ILSpy.

2. **`navisworks.gui.roamer.dll` internal command IDs.** `LcRmFrameworkInterface.ExecuteCommand("RoamerGUI_OM_ATTRIB_BAR", ...)` and similar calls use undocumented string IDs that may not exist in NW2025. These calls are in `CmdViewpoint.cs` and could throw at runtime, but they run only after the ribbon appears — so this is not the cause of the missing ribbon, but may cause crashes once the ribbon is visible.

3. **`EmbedInteropTypes=True` on `Autodesk.Navisworks.Interop.ComApi.dll`.** The COM type identity for `InwNvViewPoint2` is embedded at compile time. If the NW2025 COM API changed the TypeLib GUID or interface GUID, you will get a `COMException` or `InvalidCastException` at runtime. Again, this runs after load, not during load.

---

## What NOT to Use

| Avoid | Why | Use Instead |
|-------|-----|-------------|
| NW's "Manage" > "Plugins" dialog (if it exists) | Not a standard NW feature — NW does not have a plugin manager UI for `PackageContents` bundles | Use `Application.Plugins.PluginRecords` via API |
| Relying on the CtrlWizNW.log alone | Logger not initialized until after static class loading — misses the most common failure mode | Check Event Log first |
| Checking only the deployed bundle | The source-controlled `PackageContents.xml` and the deployed one are now in sync (confirmed), but the DLL in the bundle may be stale | Rebuild `Debug NW25|x64` explicitly and verify file timestamps |

---

## Version Compatibility

| Component | Version in Bundle | NW2025 Requirement | Compatible? |
|-----------|------------------|-------------------|-------------|
| CtrlWizNW.dll | Built against NW2025 API (if `Debug NW25\|x64` was last build) | NW2025 API DLLs | Conditional — verify with ILSpy |
| XInputDotNetPure.dll | P/Invokes `xinput1_4.dll` | `xinput1_4.dll` is a Windows system DLL | Yes — always present on Win8+ |
| GalaSoft.MvvmLight.dll v5.4.1 | .NET 4.5+ | .NET 4.8 host | Yes |
| PaddleSDK.dll v2.0.5 | net35 build | Runs on .NET 4.8 | Yes |
| WindowsInput.dll v1.0.4 | net20 build | Runs on .NET 4.8 | Yes |
| CtrlWizNW.dll.config | Redirects Newtonsoft.Json 0-12→12, StructureMap 0-2.6.4→2.6.4 | These redirects are NW2022-era assumptions | Check if NW2025 ships different versions of these |

---

## Sources

- `C:\Program Files\Autodesk\Navisworks Manage 2025\Autodesk.Navisworks.Api.xml` — PluginRecord, PluginLoadException, ApplicationPlugins API (HIGH confidence, official NW2025 API documentation)
- `C:\ProgramData\Autodesk\ApplicationPlugins\CtrlWiz.NW.bundle\PackageContents.xml` — deployed manifest confirmed correct (HIGH confidence, direct inspection)
- `C:\ProgramData\Autodesk\ApplicationPlugins\CtrlWiz.NW.bundle\Contents\Nw25\` — DLL inventory confirmed complete (HIGH confidence, direct inspection)
- `CtrlWizNW\CtrlWizNW.csproj` — build config, API reference paths, signing config (HIGH confidence, source code)
- `CtrlWiz.Logging\Logger.cs` — log path derivation (HIGH confidence, source code)
- `XInputDotNetPure\XInputDotNetPure\Imports.cs` — P/Invoke target `xinput1_4.dll` confirmed (HIGH confidence, source code)
- `CtrlWizNW\CtrlWiz.NW\CmdViewpoint.cs` — plugin attributes, `[Plugin]`, `[RibbonLayout]`, `[RibbonTab]` (HIGH confidence, source code)
- Microsoft .NET Assembly Binding Log documentation — standard .NET diagnostic (HIGH confidence)
- Sysinternals Process Monitor — standard Windows diagnostic tool (HIGH confidence)

---

*Stack research for: Navisworks 2025 plugin load failure diagnostics*
*Researched: 2026-05-03*
