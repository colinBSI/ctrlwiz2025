# Coding Conventions
_Last updated: 2026-05-01_

## Summary

CtrlWiz is a C# .NET Framework 4.8 solution following standard Microsoft/Visual Studio conventions. The codebase applies MVP (Model-View-Presenter) in the WinForms layer (`CtrlWizForms/`) and MVVM in the WPF layer (`CtrlWizNW.UI/`). Naming follows PascalCase for types and public members, camelCase with underscore prefix for private fields. There is no automated formatter configuration (.editorconfig, Resharper, StyleCop) present in the repository.

---

## Naming Conventions

### Classes and Types
- **PascalCase** for all class, interface, enum, and struct names.
- Interface names prefixed with `I`: `ISpeedSettingForm`, `ISpeedSettingModel`, `IMessageService`, `IHelpForm`.
- EventArgs subclasses suffixed `Args` or `EventArgs`: `SettingEventArgs`, `ActivationChangedArgs`, `CheckoutCompletedArgs`.
- Command classes prefixed `Cmd`: `CmdViewpoint`, `CmdActivateController`, `CmdHelp`, `CmdSettings`.
- External application entry point prefixed `ExApp`: `ExAppCtrlWizRVT`.
- Presenter classes suffixed `Presenter`: `SpeedSettingPresenter`, `HelpPresenterNW`, `HelpPresenterRVT`.
- ViewModel classes suffixed `ViewModel`: `HelpViewModel`.
- Model option classes suffixed `Option`: `ControllerButtonOption`, `ControllerTriggerOption`, `ControllerThumbStickOption`.

### Methods
- **PascalCase** for all public and private methods.
- Private event handler methods follow the pattern `[SenderName]_[EventName]`: `LinearTrackBar_ValueChanged`, `UIApplication_ViewActivating`, `GetControllerElement_SettingsPropertyChanged`.
- `async` methods do NOT use an `Async` suffix: `InitializeController`, `Update`, `VibrateOnStart`.

### Fields and Variables
- Private instance fields: `_camelCaseWithLeadingUnderscore`
  - Examples: `_inputSimulator`, `_selectDeselectButton`, `_controllerButtonOptions`
- Private static fields: same `_camelCase` prefix: `_logFilePath`, `_restoreDefaultCommand`
- Local variables and method parameters: plain `camelCase`: `localAButtonFunction`, `commandData`
- Inconsistency: some older static flags drop the underscore: `isProductActivated`, `isRun` in `CtrlWizRVT/CtrlWiz.RVT/CmdActivateController.cs`

### Properties
- **PascalCase** for all properties.
- Auto-properties with expression-body getters preferred for simple factory instances:
  ```csharp
  public static MessageService MessageService => new MessageService();
  ```
- Read-only properties: `{ get; private set; }`

### Enums
- **PascalCase** for type and member names.
- All enums include a sentinel `Unset = -1` and a `None` member (for "no selection assigned"):
  ```csharp
  // CtrlWizNW.UI/Models/ControllerButton.cs
  public enum ControllerButton { X = 0, Y = 1, A = 2, B = 3, RBumber = 4, Start = 5, None = 6, Unset = -1 }
  ```
- The `[Serializable]` attribute is applied to enums persisted in user settings (`Function` enum in `CtrlWizNW.UI/Models/Function.cs`).

### Constants
- `const` / `static readonly` fields at class level use camelCase when private:
  ```csharp
  private const float constantUnit = 3.28084f;          // camelCase (inconsistent)
  private readonly static int LinearSpeedFactor = 10;   // PascalCase (inconsistent)
  ```
  New code should use PascalCase for all `const` and `static readonly` fields.

---

## Namespace Conventions

Namespaces reflect project assembly names and internal folder structure:

| Project folder | Namespace |
|---|---|
| `CtrlWizNW/CtrlWiz.NW/` | `CtrlWiz.NW` |
| `CtrlWizNW.UI/ViewModels/` | `CtrlWiz.NW.UI.ViewModels` |
| `CtrlWizNW.UI/Views/` | `CtrlWiz.NW.UI.Views` |
| `CtrlWizNW.UI/Models/` | `CtrlWiz.NW.UI.Models` |
| `CtrlWizNW.UI/Properties/` | `CtrlWiz.NW.UI.Properties` |
| `CtrlWizForms/Views/` | `CtrlWiz.Forms.Views` |
| `CtrlWizForms/Presenters/` | `CtrlWiz.Forms.Presenters` |
| `CtrlWizForms/Models/` | `CtrlWiz.Forms.Models` |
| `CtrlWizForms/MessageServices/` | `CtrlWiz.Forms.MessageServices` |
| `CtrlWizForms.CustomControls/` | `CtrlWizForms.CustomControls` |
| `CtrlWizRVT/CtrlWiz.RVT/` | `CtrlWiz.RVT` |
| `CtrlWizRVT/Utility/` | `CtrlWiz.RVT.Utility` |
| `CtrlWizRVT/States/` | `CtrlWiz.RVT.States` |
| `CtrlWizRVT/Dialogs/` | `CtrlWiz.RVT.Dialogs` |
| `CtrlWizLicense/` | `CtrlWiz.License` |
| `CtrlWizLicense/Configs/` | `CtrlWiz.License.Configs` |
| `CtrlWiz.Logging/` | `CtrlWiz.Logging` |
| `CtrlWiz.Logging/Utility/` | `CtrlWiz.Logging.Utility` |

Note: `CtrlWizForms.CustomControls` uses `CtrlWizForms.CustomControls` (not `CtrlWiz.Forms.CustomControls`), diverging from the `CtrlWiz.*` pattern.

---

## Import Organization

No tooling enforces import order. The observed pattern in files like `CtrlWizNW/CtrlWiz.NW/CmdViewpoint.cs` is:

1. Autodesk / third-party SDK namespaces
2. Internal `CtrlWiz.*` namespaces
3. `System.*` namespaces last

This is **inverted** from standard .NET convention (System namespaces first). No path aliases or `using static` directives are used.

---

## Code Style

### Braces
- **Allman style** throughout: opening brace on its own line for all blocks (`if`, `else`, `for`, `while`, `try`, `catch`, `using`).
- Exception: expression-body members (`=>`) for simple one-liners:
  ```csharp
  public Vector3 Point3DtoV3(Point3D p) => new Vector3((float)p.X, (float)p.Y, (float)p.Z);
  internal static float ConvertIntoRadians(this int angle) => (float)(angle * Math.PI / 180);
  ```

### `this.` Qualification
Used consistently in constructors and when subscribing to events to distinguish injected fields from local variables:
```csharp
// CtrlWizForms/Presenters/SpeedSettingPresenter.cs
this.speedSettingForm = new SpeedSettingForm(linearSetting, angularSetting, linearSpeedFactor);
this.speedSettingForm.LinearTrackBarValueChanged += SpeedSettingForm_SpeedTrackBarValueChanged;
```
Not used in static methods or for simple property access outside constructors.

### `var` Usage
Explicit types are strongly preferred. `var` is not used in any file reviewed.

---

## Architectural Patterns

### MVP (WinForms layer)
Location: `CtrlWizForms/`

- **View interfaces** in `CtrlWizForms/Views/`: `ISpeedSettingForm`, `IHelpForm`
- **View implementations** in same folder: `SpeedSettingForm.cs`, `HelpFormNW.cs`, `HelpFormRVT.cs`
- **Presenters** in `CtrlWizForms/Presenters/`: `SpeedSettingPresenter.cs`, `HelpPresenterNW.cs`, `HelpPresenterRVT.cs`
- **Models** in `CtrlWizForms/Models/`: `ISpeedSettingModel` / `SpeedSettingModel` (currently empty stubs)

Presenters have two constructors: a public one that creates concrete implementations (production use), and a private one accepting interfaces (for future testability):
```csharp
// CtrlWizForms/Presenters/SpeedSettingPresenter.cs
public SpeedSettingPresenter(int linearSetting, int angularSetting, int linearSpeedFactor) { ... }
private SpeedSettingPresenter(ISpeedSettingForm form, ISpeedSettingModel model, IMessageService svc) { ... }
```

### MVVM (WPF layer)
Location: `CtrlWizNW.UI/`

- **View**: `CtrlWizNW.UI/Views/HelpView.xaml` / `HelpView.xaml.cs` — minimal code-behind
- **ViewModel**: `CtrlWizNW.UI/ViewModels/HelpViewModel.cs` — implements `INotifyPropertyChanged`
- **Commands**: `GalaSoft.MvvmLight.Command.RelayCommand` for button bindings
- Property change notification:
  ```csharp
  private void RaisePropertyChanged(string propertyName)
  {
      if (this.PropertyChanged != null)
          this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
  }
  ```
- `nameof()` used for all `RaisePropertyChanged` calls — no magic strings.
- `ObservableCollection<T>` used for all bindable list properties.

### Singleton
Generic thread-safe Singleton base class at `CtrlWizNW/Singleton.cs`:
```csharp
public abstract class Singleton<T> where T : class, new()
{
    private static readonly Lazy<T> instance = new Lazy<T>(() => new T());
    public static T Instance { get { lock (lockObject) return instance.Value; } }
}
```
Used by `NavisUtils : Singleton<NavisUtils>` in `CtrlWizNW/CtrlWiz.NW/NavisUtils.cs`.

### Extension Methods
Each project places extension methods in a `Utility/ExtensionMethods.cs` file:
- `CtrlWiz.Logging/Utility/ExtensionMethods.cs` — `GetExceptionInfo(this Exception ex)`
- `CtrlWizRVT/Utility/ExtensionMethods.cs` — geometry helpers (`RotateByAxis`, `ConvertToVector3`, `ConvertToXYZ`) — marked `internal static`
- `CtrlWizForms.CustomControls/Utility/ExtensionMethods.cs` — `FillRoundedRectangle` for GDI+

### `#region` Usage
`#region` / `#endregion` used for:
- Grouping Revit attributes: `#region Attributes` in `CmdActivateController.cs`
- Labeling disabled/old code: `#region Working WinForms Version`, `#region Unused Code`
- Partitioning NW plugin classes: `#region CmdTargetEnable`, `#region CmdTargetDisable`

---

## Event Handling

Standard .NET pattern with typed `EventArgs` subclasses. Null-conditional invocation used throughout:
```csharp
// CtrlWizForms/Views/SpeedSettingForm.cs
public event EventHandler<SettingEventArgs> LinearTrackBarValueChanged;

private void LinearTrackBar_ValueChanged(object sender, EventArgs e)
{
    LinearTrackBarValueChanged?.Invoke(this, new SettingEventArgs() { SettingValue = LinearTrackBar.Value });
}
```

Settings change propagation chains through a static event on `GetControllerElement`:
```csharp
// CtrlWizNW.UI/Properties/GetControllerElement.cs
Settings.Default.PropertyChanged += Default_PropertyChanged;
public static event PropertyChangedEventHandler SettingsPropertyChanged;
```

---

## Error Handling

### NW Plugin (CmdViewpoint.cs)
```csharp
catch (Exception ex)
{
    ex.LogException();   // write to %TEMP%\CtrlWizNW.log, swallow exception
}
```

### Revit Plugin (ExAppCtrlWizRVT.cs, CmdActivateController.cs)
```csharp
catch (Exception e)
{
    TaskDialog.Show("Error", e.Message);
    return Result.Failed;
}
```

The inner game loop in `CmdActivateController.cs` shows error AND re-throws:
```csharp
catch (Exception ex)
{
    ExAppCtrlWizRVT.MessageService.ShowError($"Error occurred:\n{ex.Message}");
    throw;
}
```

**What is not done:** No structured log levels, no `AggregateException` handling for async tasks, and some catch blocks in the NW main loop silently swallow all exceptions.

---

## Logging Pattern

`CtrlWiz.Logging/Logger.cs` is a `static` class writing to a UTF-8 flat file.

**Initialization (called once at plugin startup):**
```csharp
Logger.SetUpLogger(LoggedApp.NW);   // in CmdViewpoint constructor
Logger.SetUpLogger(LoggedApp.RVT);  // in ExAppCtrlWizRVT.OnStartup
```

**Log file locations:**
- NW: `%TEMP%\CtrlWizNW.log`
- RVT: `%TEMP%\..\CtrlWizRVT.log`

**Usage:**
```csharp
ex.LogException();   // extension method, appends timestamp + message + stack trace
```

Only exceptions are logged. No informational, debug, or warning logging exists.

---

## Settings / User Preferences

Controller button mappings are persisted via .NET `Properties.Settings.Default` in `CtrlWizNW.UI`. Keys use the `NV_` prefix and store `Function` enum values:
- Buttons: `NV_AButton`, `NV_BButton`, `NV_XButton`, `NV_YButton`, `NV_RBumber`, `NV_StartButton`
- Triggers: `NV_LTrigger`, `NV_RTrigger`
- ThumbSticks: `NV_LStick`, `NV_RStick`

Persistence: `Settings.Default.Save()` called after each user change.
Reset: `Settings.Default.Reset()` called by Restore Default command.

---

## Comments and Documentation

- No XML doc comments (`///`) exist anywhere in the codebase.
- Inline `//` comments explain non-obvious API calls and math operations.
- Large blocks of superseded code are commented out rather than deleted, usually wrapped in a named `#region`. Examples: `CtrlWizForms/Views/HelpFormNW.cs`, `CtrlWizNW.UI/Properties/GetControllerElement.cs`.
- Designer-generated files (`*.Designer.cs`) are not manually commented.
