# CtrlWiz Coding Conventions

## Language and Runtime

- All source files are C# targeting .NET Framework 4.8.
- XAML is used exclusively for WPF UI markup (CtrlWizNW.UI, Views/).
- The solution targets Windows only. No cross-platform abstractions are used or expected.
- All projects compile to class libraries (OutputType = Library). There are no standalone executables
  except FullPermissionTest, which is a development harness.

---

## Namespaces

Namespaces mirror the project/assembly name, not the on-disk folder hierarchy:

  Project            Root namespace
  -------            --------------
  CtrlWizNW          CtrlWiz.NW
  CtrlWizNW.UI       CtrlWiz.NW.UI.{Models|ViewModels|Views|Properties}
  CtrlWizRVT         CtrlWiz.RVT
  CtrlWizForms       CtrlWiz.Forms.{Views|Presenters|Models|MessageServices}
  CtrlWizForms.CustomControls   CtrlWizForms.CustomControls (note: no dot before CustomControls)
  CtrlWizLicense     CtrlWiz.License
  CtrlWiz.Logging    CtrlWiz.Logging (and CtrlWiz.Logging.Utility for helpers)
  XInputDotNetPure   XInputDotNetPure

Do not create sub-namespaces beyond those already established. New files in an existing
project go into the project's root namespace unless they belong to an established sub-folder
(Utility, Models, Views, etc.).

---

## Naming Conventions

### Types

- Classes, interfaces, enums, and structs use PascalCase.
- Interface names are prefixed with I: ISpeedSettingForm, IMessageService, ISpeedSettingModel,
  IHelpForm.
- Enums use PascalCase for both the type name and every member value. Sentinel members that
  indicate "nothing assigned yet" are named Unset (not null, Unknown, or Default):
    ControllerButton.Unset = -1
    ControllerTrigger.Unset = -1
    ControllerThumbStick.Unset = -1
    Function.Unset = -1
  Sentinel members that indicate "explicitly mapped to nothing" are named None:
    ControllerButton.None = 6
    Function.None = 21

### Methods

- Public, protected, and internal methods use PascalCase.
- Private methods use PascalCase (same rule applies; there is no camelCase exception for
  private methods in this codebase).
- Async methods do not carry the Async suffix (InitializeController, Update, VibrateOnStart).
  New async methods should follow this existing pattern for consistency.
- Event handler methods are named using the pattern:
    <SourceObject>_<EventName>
  Examples: UIApplication_ViewActivating, InAppCheckout_ActivationChanged,
  SpeedSettingForm_SpeedTrackBarValueChanged, Default_PropertyChanged.

### Fields and Parameters

- Private instance fields use _camelCase with a leading underscore:
    private InputSimulator _inputSimulator;
    private RelayCommand _restoreDefaultCommand;
    private bool _isHovering;
- Private static fields that are effectively constants or singletons use _camelCase:
    private static long lastTime
    private static int fps
  (Note: some older static fields omit the underscore; new code should use the underscore.)
- Local variables and method parameters use camelCase without a leading underscore.
- Boolean state flags use the prefix is or has:
    isRun, isMoving, isRotating, isProductActivated, firsttime (legacy; prefer isFirstTime
    for new code).

### Properties

- Public and internal properties use PascalCase with standard get/set or expression-bodied
  accessors.
- Read-only auto-properties that delegate to a new instance each call use expression-body
  syntax:
    public static MessageService MessageService => new MessageService();
- Properties backed by a private field follow the _field / Property pairing pattern:
    private bool _isHovering;
    // exposed via OnPaint behavior, not a property

### Constants

- Literal constant fields use camelCase (existing code) or PascalCase where they carry
  public/internal visibility:
    private const float constantUnit = 3.28084f;
    private readonly static int LinearSpeedFactor = 10;
  New constants should prefer PascalCase for public/internal and SCREAMING_SNAKE only if
  required by a host SDK (none currently required).

---

## Architectural Patterns

### Revit Commands (IExternalCommand)

Every Revit command class:
1. Applies three Autodesk attribute groups at the top in a named region labeled "Attributes":
     [Autodesk.Revit.Attributes.Transaction(TransactionMode.Manual)]
     [Autodesk.Revit.Attributes.Regeneration(RegenerationOption.Manual)]
     [Autodesk.Revit.Attributes.Journaling(JournalingMode.NoCommandData)]
2. Implements IExternalCommand with a single public Execute method.
3. Calls ExAppCtrlWizRVT.IsLicenseActive() as the very first statement inside Execute.
   If it returns false, return Result.Cancelled immediately.
4. Wraps the entire body in a try/catch(Exception e) block. On catch, call
   TaskDialog.Show("Error", e.Message) and return Result.Failed.
5. Returns Result.Succeeded at the bottom of the happy path.

### Navisworks Plugin (CommandHandlerPlugin)

- The single CommandHandlerPlugin class (CmdViewpoint) is annotated with [Plugin(...)],
  [Strings(...)], [RibbonLayout(...)], [RibbonTab(...)], and one [Command(...)] attribute
  per ribbon button. All annotations are grouped in a region labeled "Create Ribbon".
- The plugin class implements IDisposable.
- Button IDs follow the pattern ID_Button_N (1-indexed integers).

### Model-View-Presenter (WinForms, CtrlWizForms)

- Views implement interfaces (ISpeedSettingForm, IHelpForm). Never reference a concrete
  form type from a presenter.
- Presenters receive their dependencies by interface through a private constructor that
  accepts (ISpeedSettingForm, ISpeedSettingModel, IMessageService). A public convenience
  constructor with primitive parameters constructs concrete instances internally and
  delegates to the private constructor.
- EventArgs subclasses are lightweight: one or two properties, no logic.
  Example: SettingEventArgs { int SettingValue }.
- The MessageService / IMessageService abstraction wraps MessageBox.Show. Do not call
  MessageBox.Show directly from presenters or models; always inject IMessageService.

### MVVM (WPF, CtrlWizNW.UI)

- ViewModels implement INotifyPropertyChanged directly (no base class). The
  RaisePropertyChanged helper method checks for null before invoking the event:
    if (this.PropertyChanged != null)
        this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
- Commands are typed as ICommand and backed by GalaSoft.MvvmLight RelayCommand.
  They are initialized lazily in the property getter (null-check + assign pattern).
- User settings are accessed and persisted exclusively through Settings.Default[propertyName]
  followed by Settings.Default.Save(). Do not use typed properties on the Settings class
  directly when the property name must be computed at runtime.
- Collections exposed to the view are ObservableCollection<T> returned from a property
  getter that constructs a fresh collection each time from the backing List<T>. This is the
  established pattern in HelpViewModel; do not replace it with a field-backed
  ObservableCollection unless you also wire up list-change propagation.

### Singleton (CtrlWizNW)

- Use the generic Singleton<T> base class (Singleton.cs) for utilities that must have a
  single shared instance. NavisUtils is the only current example:
    public class NavisUtils : Singleton<NavisUtils> { ... }
  Access the instance via NavisUtils.Instance. Do not introduce a second singleton
  implementation.

### Controller Loop

- The controller polling loop is an async void method (InitializeController) that
  immediately awaits a vibration startup method and then awaits a while-loop Task (Update).
- The loop uses await Task.Delay(N) for cadence control. Do not introduce Thread.Sleep.
- The loop reads GamePad.GetState(PlayerIndex.One) once per iteration, stores the result
  in state, and keeps the previous frame in prevState. Edge-detection (button just pressed)
  is done by comparing prevState.Buttons.X == ButtonState.Released &&
  state.Buttons.X == ButtonState.Pressed.
- Dead-zone threshold is 0.002f for thumbstick axes and trigger values. Do not change this
  value without profiling on real hardware.

---

## Error Handling

- Every public entry-point method (IExternalCommand.Execute, IExternalApplication.OnStartup,
  IExternalApplication.OnShutdown) wraps its body in a try/catch(Exception e).
  - Revit entry points call TaskDialog.Show("Error", e.Message) before returning Failed.
  - Non-entry-point catch blocks call ex.LogException() (the extension method from
    CtrlWiz.Logging) and do not re-throw unless the caller needs to know.
- Paddle SDK callbacks are always wrapped in their own try/catch that calls ex.LogException()
  so that a SDK callback exception cannot propagate up to the host application.
- Do not swallow exceptions silently (empty catch blocks). If you cannot handle an exception
  meaningfully, at minimum call ex.LogException().

---

## Logging

- Call Logger.SetUpLogger(LoggedApp.NW) or Logger.SetUpLogger(LoggedApp.RVT) exactly once,
  during application startup (OnStartup for Revit; plugin initialization for Navisworks).
- Log exceptions by calling the extension method on the exception object:
    catch (Exception ex) { ex.LogException(); }
  This is defined in CtrlWiz.Logging.Utility.ExtensionMethods and is available wherever
  CtrlWiz.Logging is referenced.
- Do not call Logger methods directly (Logger.LogException). Use the extension method.
- Log file locations (read-only, set by Logger.SetUpLogger):
    NW:  %TEMP%\CtrlWizNW.log
    RVT: %TEMP%\..\CtrlWizRVT.log   (one level above the per-user temp folder)

---

## License Gate

- Any command that requires an active license must call ExAppCtrlWizRVT.IsLicenseActive()
  (Revit) before performing any work. The Navisworks equivalent checks isProductActivated
  inside the command handler.
- InAppCheckout is constructed once during OnStartup and stored on the application class as
  a static property. Do not construct a new InAppCheckout per command invocation.
- Subscribe to InAppCheckout.ActivationChanged in OnStartup and unsubscribe in OnShutdown.

---

## Build Configurations

- Debug: Targets one specific host version (RVT 2019 or NW 2022). Used for local
  development only. Output goes to bin\Debug\ or directly into the ApplicationPlugins bundle.
- Release configurations are named after the host version: RVT19, RVT20, RVT21, RVT22,
  NW17, NW18, NW19, NW20, NW21, NW22.
- Each release configuration defines a single conditional compilation symbol:
    RVT: RVT19 / RVT20 / RVT21 / RVT22
    NW:  NW2017 / NW2018 / NW2019 / NW2020 / NW2021 / NW2022
  TRACE is also defined in all release builds.
- Release builds output to ..\CtrlWiz.Release.Assemblies\{Revit|Navisworks}\$(Configuration)\.
- Post-build signing: every NW and RVT project signs its output DLL with signtool.exe using
  the VIATechnik_CS_cert.pfx certificate and the Comodoca SHA-256 timestamp authority. The
  certificate path is relative to the project; keep it in the project root.
- Do not add new PropertyGroup blocks for host SDK paths using absolute paths. SDK paths
  must be resolved through $(ProgramW6432)\Autodesk\<Product> <Year>\ using MSBuild
  properties defined per configuration.

---

## Comments and Dead Code

- Comments explaining intent ("// Ask the Product to get its latest state") are acceptable.
- Do not leave large commented-out code blocks in production files without a clear note
  explaining why the code is retained. Prefer deleting dead code and using git history for
  recovery.
- Commented-out code that represents an in-progress alternative implementation must be
  accompanied by a TODO or FIXME comment with a brief explanation.
- The #region / #endregion pattern is used in places (e.g., "Create Ribbon", "Attributes",
  "declare FPS"). New regions may be added only to group tightly related declarations (plugin
  attributes, ribbon commands). Do not use regions to hide implementation logic.

---

## Resource Strings

- All user-visible strings for ribbon labels, tooltip text, and dialog captions are stored
  in project-level Resources.resx files and accessed through the generated Resources class:
    Resources.TabName, Resources.ActivateControllerButtonText, etc.
- Do not hard-code user-visible strings inline in code. URLs that are not user-visible
  (e.g., Process.Start("https://...")) are acceptable inline.
- Sensitive values (Paddle API key, vendor ID, product IDs) are embedded in Resources.resx
  and accessed through properties on InAppCheckout. Do not add new sensitive values as
  compile-time literals.

---

## File and Folder Layout

- Host-specific source files go inside a folder named after the root namespace sub-part:
    CtrlWizRVT/CtrlWiz.RVT/
    CtrlWizNW/CtrlWiz.NW/
- Shared utilities go into a Utility/ subfolder within the project that owns them.
- Models, Views, Presenters, and ViewModels each get their own top-level folder within their
  project. Do not mix concerns across folders.
- Designer-generated files (*.Designer.cs, *.Designer.xaml) are kept alongside their
  hand-authored counterparts and edited only by tooling.
- The Views/ folder at the repository root contains loose XAML files that duplicate content
  in CtrlWizNW.UI/Views/. Do not add further files there; consolidate to CtrlWizNW.UI.
