using CtrlWiz.NW.UI.Models;
using CtrlWiz.NW.UI.Properties;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CtrlWiz.NW.UI.ViewModels
{
    public class HelpViewModel : INotifyPropertyChanged
    {
        private RelayCommand _restoreDefaultCommand = null;

        private List<ControllerButtonOption> _controllerButtonOptions = null;
        private List<ControllerTriggerOption> _controllerTriggerOptions = null;
        private List<ControllerThumbStickOption> _controllerLThumbStickOptions = null;
        private List<ControllerThumbStickOption> _controllerRThumbStickOptions = null;

        private int _previousSelectedAButtonIndex = -1;
        private int _previousSelectedBButtonIndex = -1;
        private int _previousSelectedXButtonIndex = -1;
        private int _previousSelectedYButtonIndex = -1;
        private int _previousSelectedRBumberIndex = -1;
        private int _previousSelectedStartButtonIndex = -1;
        private int _previousSelectedLTriggerIndex = -1;
        private int _previousSelectedRTriggerIndex = -1;
        private int _previousSelectedLStickIndex = -1;
        private int _previousSelectedRStickIndex = -1;

        public HelpViewModel()
        {
            this.CreateEmptyControllerElementOptions();
            this.ApplyLocalUserSettingsToControllerElementOptions();
        }

        public ObservableCollection<ControllerButtonOption> ControllerButtonOptions
        {
            get => new ObservableCollection<ControllerButtonOption>(_controllerButtonOptions);
            private set { }
        }
        public ObservableCollection<ControllerTriggerOption> ControllerTriggerOptions
        {
            get => new ObservableCollection<ControllerTriggerOption>(_controllerTriggerOptions);
            private set { }
        }
        public ObservableCollection<ControllerThumbStickOption> ControllerLThumbStickOptions
        {
            get => new ObservableCollection<ControllerThumbStickOption>(_controllerLThumbStickOptions);
            private set { }
        }
        public ObservableCollection<ControllerThumbStickOption> ControllerRThumbStickOptions
        {
            get => new ObservableCollection<ControllerThumbStickOption>(_controllerRThumbStickOptions);
            private set { }
        }

        public int SelectedAButtonIndex
        {
            get
            {
                return _previousSelectedAButtonIndex = this.FindSelectedElementIndex(ControllerButton.A);
            }
            set
            {
                string resetElementPropertyName = GetControllerElement.GetPropertyNameByControllerElement(this.ControllerButtonOptions.ElementAt(value).Button);

                this.ChangeElementOption(ControllerButton.A, _previousSelectedAButtonIndex, value);

                this.SaveElementsStateToSettings(value, "NV_AButton", ControllerButton.A, resetElementPropertyName);

                this.NotifyOfSelectedButtonIndexChanged();
            }
        }
        public int SelectedBButtonIndex
        {
            get
            {
                return _previousSelectedBButtonIndex = this.FindSelectedElementIndex(ControllerButton.B);
            }
            set
            {
                string resetElementPropertyName = GetControllerElement.GetPropertyNameByControllerElement(this.ControllerButtonOptions.ElementAt(value).Button);

                this.ChangeElementOption(ControllerButton.B, _previousSelectedBButtonIndex, value);

                this.SaveElementsStateToSettings(value, "NV_BButton", ControllerButton.B, resetElementPropertyName);

                this.NotifyOfSelectedButtonIndexChanged();
            }
        }
        public int SelectedXButtonIndex
        {
            get
            {
                return _previousSelectedXButtonIndex = this.FindSelectedElementIndex(ControllerButton.X);
            }
            set
            {
                string resetElementPropertyName = GetControllerElement.GetPropertyNameByControllerElement(this.ControllerButtonOptions.ElementAt(value).Button);

                this.ChangeElementOption(ControllerButton.X, _previousSelectedXButtonIndex, value);

                this.SaveElementsStateToSettings(value, "NV_XButton", ControllerButton.X, resetElementPropertyName);

                this.NotifyOfSelectedButtonIndexChanged();
            }
        }
        public int SelectedYButtonIndex
        {
            get
            {
                return _previousSelectedYButtonIndex = this.FindSelectedElementIndex(ControllerButton.Y);
            }
            set
            {
                string resetElementPropertyName = GetControllerElement.GetPropertyNameByControllerElement(this.ControllerButtonOptions.ElementAt(value).Button);

                this.ChangeElementOption(ControllerButton.Y, _previousSelectedYButtonIndex, value);

                this.SaveElementsStateToSettings(value, "NV_YButton", ControllerButton.Y, resetElementPropertyName);

                this.NotifyOfSelectedButtonIndexChanged();
            }
        }
        public int SelectedRBumberIndex
        {
            get
            {
                return _previousSelectedRBumberIndex = this.FindSelectedElementIndex(ControllerButton.RBumber);
            }
            set
            {
                string resetElementPropertyName = GetControllerElement.GetPropertyNameByControllerElement(this.ControllerButtonOptions.ElementAt(value).Button);

                this.ChangeElementOption(ControllerButton.RBumber, _previousSelectedRBumberIndex, value);

                this.SaveElementsStateToSettings(value, "NV_RBumber", ControllerButton.RBumber, resetElementPropertyName);

                this.NotifyOfSelectedButtonIndexChanged();
            }
        }
        public int SelectedStartButtonIndex
        {
            get
            {
                return _previousSelectedStartButtonIndex = this.FindSelectedElementIndex(ControllerButton.Start);
            }
            set
            {
                string resetElementPropertyName = GetControllerElement.GetPropertyNameByControllerElement(this.ControllerButtonOptions.ElementAt(value).Button);

                this.ChangeElementOption(ControllerButton.Start, _previousSelectedStartButtonIndex, value);

                this.SaveElementsStateToSettings(value, "NV_StartButton", ControllerButton.Start, resetElementPropertyName);

                this.NotifyOfSelectedButtonIndexChanged();
            }
        }
        public int SelectedLTriggerIndex
        {
            get
            {
                return _previousSelectedLTriggerIndex = this.FindSelectedElementIndex(ControllerTrigger.LT);
            }
            set
            {
                string resetElementPropertyName = GetControllerElement.GetPropertyNameByControllerElement(this.ControllerTriggerOptions.ElementAt(value).Trigger);

                this.ChangeElementOption(ControllerTrigger.LT, _previousSelectedLTriggerIndex, value);

                this.SaveElementsStateToSettings(value, "NV_LTrigger", ControllerTrigger.LT, resetElementPropertyName);

                this.NotifyOfSelectedTriggerIndexChanged();
            }
        }
        public int SelectedRTriggerIndex
        {
            get
            {
                return _previousSelectedRTriggerIndex = this.FindSelectedElementIndex(ControllerTrigger.RT);
            }
            set
            {
                string resetElementPropertyName = GetControllerElement.GetPropertyNameByControllerElement(this.ControllerTriggerOptions.ElementAt(value).Trigger);

                this.ChangeElementOption(ControllerTrigger.RT, _previousSelectedRTriggerIndex, value);

                this.SaveElementsStateToSettings(value, "NV_RTrigger", ControllerTrigger.RT, resetElementPropertyName);

                this.NotifyOfSelectedTriggerIndexChanged();
            }
        }
        public int SelectedLStickIndex
        {
            get
            {
                return _previousSelectedLStickIndex = this.FindSelectedElementIndex(ControllerThumbStick.L);
            }
            set
            {
                string resetElementPropertyName = GetControllerElement.GetPropertyNameByControllerElement(this.ControllerLThumbStickOptions.ElementAt(value).ThumbStick);

                this.ChangeElementOption(ControllerThumbStick.L, _previousSelectedLStickIndex, value);

                this.SaveElementsStateToSettings(value, "NV_LStick", ControllerThumbStick.L, resetElementPropertyName);

                this.UpdateRelatedThumbStickInDroneControlsMode(this.ControllerLThumbStickOptions, value, Function.LeftDroneControls, _previousSelectedLStickIndex,
                    this.ControllerRThumbStickOptions, _previousSelectedRStickIndex, Function.RigthDroneControls, ControllerThumbStick.R, "NV_RStick", Function.Look);

                this.NotifyOfSelectedLThumbStickIndexChanged();
                this.NotifyOfIsTriggerEnabledChanged();
            }
        }
        public int SelectedRStickIndex
        {
            get
            {
                return _previousSelectedRStickIndex = this.FindSelectedElementIndex(ControllerThumbStick.R);
            }
            set
            {
                string resetElementPropertyName = GetControllerElement.GetPropertyNameByControllerElement(this.ControllerRThumbStickOptions.ElementAt(value).ThumbStick);

                this.ChangeElementOption(ControllerThumbStick.R, _previousSelectedRStickIndex, value);

                this.SaveElementsStateToSettings(value, "NV_RStick", ControllerThumbStick.R, resetElementPropertyName);

                this.UpdateRelatedThumbStickInDroneControlsMode(this.ControllerRThumbStickOptions, value, Function.RigthDroneControls, _previousSelectedRStickIndex,
                    this.ControllerLThumbStickOptions, _previousSelectedLStickIndex, Function.LeftDroneControls, ControllerThumbStick.L, "NV_LStick", Function.MoveSprint);

                this.NotifyOfSelectedRThumbStickIndexChanged();
                this.NotifyOfIsTriggerEnabledChanged();
            }
        }

        public bool IsTriggerEnabled => !(
            this.ControllerLThumbStickOptions.Any(o => o.Function == Function.LeftDroneControls && o.IsSelected) ||
            this.ControllerRThumbStickOptions.Any(o => o.Function == Function.RigthDroneControls && o.IsSelected)
            );

        public ICommand RestoreDefault
        {
            get
            {
                if (_restoreDefaultCommand == null)
                {
                    _restoreDefaultCommand = new RelayCommand(this.ExecuteRestoreDefaultCommand, this.CanExecuteRestoreDefaultCommand);
                }

                return _restoreDefaultCommand;
            }
        }

        private void CreateEmptyControllerElementOptions()
        {
            // Local User Settings path.
            //MessageBox.Show(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath);

            // Adding Functions for Buttons.
            _controllerButtonOptions = new List<ControllerButtonOption>();
            _controllerButtonOptions.Add(new ControllerButtonOption(Function.HideUnhide, "Hide/Unhide"));
            _controllerButtonOptions.Add(new ControllerButtonOption(Function.UnhideAll, "Unhide All"));
            _controllerButtonOptions.Add(new ControllerButtonOption(Function.SelectDeselect, "Select - Deselect"));
            _controllerButtonOptions.Add(new ControllerButtonOption(Function.DeselectAll, "Deselect All"));
            _controllerButtonOptions.Add(new ControllerButtonOption(Function.ShortestDistance, "Shortest Distance"));
            _controllerButtonOptions.Add(new ControllerButtonOption(Function.NextViewpoint, "Next Viewpoint"));
            _controllerButtonOptions.Add(new ControllerButtonOption(Function.EnableTextMarkup, "Enable Text Markup"));
            _controllerButtonOptions.Add(new ControllerButtonOption(Function.CreateViewpoint, "Create Viewpoint"));
            _controllerButtonOptions.Add(new ControllerButtonOption(Function.ShowHideProperties, "Show/Hide Properties"));
            _controllerButtonOptions.Add(new ControllerButtonOption(Function.ShowHideSelectionTree, "Show/Hide Selection Tree"));
            _controllerButtonOptions.Add(new ControllerButtonOption(Function.ShowHideViewpoints, "Show/Hide Viewpoints"));
            _controllerButtonOptions.Add(new ControllerButtonOption(Function.MoveLeftInTree, "Move Left In Tree"));
            _controllerButtonOptions.Add(new ControllerButtonOption(Function.PersonView3rd, "3rd Person View"));
            _controllerButtonOptions.Add(new ControllerButtonOption(Function.CollisionGravity, "Collision/Gravity"));
            _controllerButtonOptions.Add(new ControllerButtonOption(ControllerButton.None, Function.None, "None"));

            // Adding Functions for Triggers.
            _controllerTriggerOptions = new List<ControllerTriggerOption>();
            _controllerTriggerOptions.Add(new ControllerTriggerOption(Function.ElevationDown, "Elevation Down"));
            _controllerTriggerOptions.Add(new ControllerTriggerOption(Function.ElevationUp, "Elevation Up"));
            _controllerTriggerOptions.Add(new ControllerTriggerOption(ControllerTrigger.None, Function.None, "None"));

            // Adding Functions for ThumbSticks.
            _controllerLThumbStickOptions = new List<ControllerThumbStickOption>();
            _controllerLThumbStickOptions.Add(new ControllerThumbStickOption(Function.MoveSprint, "Default"));
            _controllerLThumbStickOptions.Add(new ControllerThumbStickOption(Function.LeftDroneControls, "Drone Controls"));
            _controllerLThumbStickOptions.Add(new ControllerThumbStickOption(ControllerThumbStick.None, Function.None, "None"));

            _controllerRThumbStickOptions = new List<ControllerThumbStickOption>();
            _controllerRThumbStickOptions.Add(new ControllerThumbStickOption(Function.Look, "Default"));
            _controllerRThumbStickOptions.Add(new ControllerThumbStickOption(Function.LookInvertedY, "Invert Y"));
            _controllerRThumbStickOptions.Add(new ControllerThumbStickOption(Function.RigthDroneControls, "Drone Controls"));
            _controllerRThumbStickOptions.Add(new ControllerThumbStickOption(ControllerThumbStick.None, Function.None, "None"));
        }
        private void ApplyLocalUserSettingsToControllerElementOptions()
        {
            // Adjust options according to local User Settings.
            // Buttons.
            this.AdjustControllerElementOption(ControllerButton.A, "NV_AButton");
            this.AdjustControllerElementOption(ControllerButton.B, "NV_BButton");
            this.AdjustControllerElementOption(ControllerButton.X, "NV_XButton");
            this.AdjustControllerElementOption(ControllerButton.Y, "NV_YButton");
            this.AdjustControllerElementOption(ControllerButton.RBumber, "NV_RBumber");
            this.AdjustControllerElementOption(ControllerButton.Start, "NV_StartButton");

            // Triggers.
            this.AdjustControllerElementOption(ControllerTrigger.LT, "NV_LTrigger");
            this.AdjustControllerElementOption(ControllerTrigger.RT, "NV_RTrigger");

            // ThumbSticks.
            this.AdjustControllerElementOption(ControllerThumbStick.L, "NV_LStick");
            this.AdjustControllerElementOption(ControllerThumbStick.R, "NV_RStick");
        }
        private void AdjustControllerElementOption(Enum element, string propertyName)
        {
            Function localElementFunction = Settings.Default[propertyName] != null ? (Function)Settings.Default[propertyName] : Function.Unset;

            if (localElementFunction != Function.Unset)
            {
                if (element is ControllerButton)
                {
                    _controllerButtonOptions.ForEach(o =>
                    {
                        if (o.Button == (ControllerButton)element)
                        {
                            o.Button = ControllerButton.Unset;
                            o.IsSelected = false;
                        }

                        if (localElementFunction != Function.None && o.Function == localElementFunction)
                        {
                            o.Button = (ControllerButton)element;
                            o.IsSelected = true;
                        }
                    });
                }
                else if (element is ControllerTrigger)
                {
                    _controllerTriggerOptions.ForEach(o =>
                    {
                        if (o.Trigger == (ControllerTrigger)element)
                        {
                            o.Trigger = ControllerTrigger.Unset;
                            o.IsSelected = false;
                        }

                        if (localElementFunction != Function.None && o.Function == localElementFunction)
                        {
                            o.Trigger = (ControllerTrigger)element;
                            o.IsSelected = true;
                        }
                    });
                }
                else
                {
                    if ((ControllerThumbStick)element == ControllerThumbStick.L)
                    {
                        _controllerLThumbStickOptions.ForEach(o =>
                        {
                            if (o.ThumbStick == (ControllerThumbStick)element)
                            {
                                o.ThumbStick = ControllerThumbStick.Unset;
                                o.IsSelected = false;
                            }

                            if (localElementFunction != Function.None && o.Function == localElementFunction)
                            {
                                o.ThumbStick = (ControllerThumbStick)element;
                                o.IsSelected = true;
                            }
                        });
                    }
                    else
                    {
                        _controllerRThumbStickOptions.ForEach(o =>
                        {
                            if (o.ThumbStick == (ControllerThumbStick)element)
                            {
                                o.ThumbStick = ControllerThumbStick.Unset;
                                o.IsSelected = false;
                            }

                            if (localElementFunction != Function.None && o.Function == localElementFunction)
                            {
                                o.ThumbStick = (ControllerThumbStick)element;
                                o.IsSelected = true;
                            }
                        });
                    }
                }
            }
        }

        private int FindSelectedElementIndex(Enum element)
        {
            int optionIndex = -1;

            if (element is ControllerButton)
                optionIndex = this.ControllerButtonOptions.IndexOf(this.ControllerButtonOptions.FirstOrDefault(o => o.Button == (ControllerButton)element && o.IsSelected));
            else if (element is ControllerTrigger)
                optionIndex = this.ControllerTriggerOptions.IndexOf(this.ControllerTriggerOptions.FirstOrDefault(o => o.Trigger == (ControllerTrigger)element && o.IsSelected));
            else if (element is ControllerThumbStick)
            {
                if ((ControllerThumbStick)element == ControllerThumbStick.L)
                    optionIndex = this.ControllerLThumbStickOptions.IndexOf(this.ControllerLThumbStickOptions.FirstOrDefault(o => o.ThumbStick == (ControllerThumbStick)element && o.IsSelected));
                else
                    optionIndex = this.ControllerRThumbStickOptions.IndexOf(this.ControllerRThumbStickOptions.FirstOrDefault(o => o.ThumbStick == (ControllerThumbStick)element && o.IsSelected));
            }

            if (optionIndex != -1)
                return optionIndex;
            else
            {
                if (element is ControllerButton)
                    return this.ControllerButtonOptions.IndexOf(this.ControllerButtonOptions.FirstOrDefault(o => o.Function == Function.None));
                else if (element is ControllerTrigger)
                    return this.ControllerTriggerOptions.IndexOf(this.ControllerTriggerOptions.FirstOrDefault(o => o.Function == Function.None));
                else
                {
                    if ((ControllerThumbStick)element == ControllerThumbStick.L)
                        return this.ControllerLThumbStickOptions.IndexOf(this.ControllerLThumbStickOptions.FirstOrDefault(o => o.Function == Function.None));
                    else
                        return this.ControllerRThumbStickOptions.IndexOf(this.ControllerRThumbStickOptions.FirstOrDefault(o => o.Function == Function.None));
                }
            }
        }
        private void ChangeElementOption(Enum element, int previousSelectedOptionIndex, int newSelectedOptionIndex)
        {
            ControllerOption previousSelectedOption = null;
            ControllerOption newSelectedOption = null;

            if (element is ControllerButton)
            {
                previousSelectedOption = this.ControllerButtonOptions.ElementAt(previousSelectedOptionIndex);
                newSelectedOption = this.ControllerButtonOptions.ElementAt(newSelectedOptionIndex);
            }
            else if (element is ControllerTrigger)
            {
                previousSelectedOption = this.ControllerTriggerOptions.ElementAt(previousSelectedOptionIndex);
                newSelectedOption = this.ControllerTriggerOptions.ElementAt(newSelectedOptionIndex);
            }
            else
            {
                if ((ControllerThumbStick)element == ControllerThumbStick.L)
                {
                    previousSelectedOption = this.ControllerLThumbStickOptions.ElementAt(previousSelectedOptionIndex);
                    newSelectedOption = this.ControllerLThumbStickOptions.ElementAt(newSelectedOptionIndex);
                }
                else
                {
                    previousSelectedOption = this.ControllerRThumbStickOptions.ElementAt(previousSelectedOptionIndex);
                    newSelectedOption = this.ControllerRThumbStickOptions.ElementAt(newSelectedOptionIndex);
                }
            }

            if (previousSelectedOption.Function != Function.None)
            {
                if (previousSelectedOption is ControllerButtonOption)
                    (previousSelectedOption as ControllerButtonOption).Button = ControllerButton.Unset;
                else if (previousSelectedOption is ControllerTriggerOption)
                    (previousSelectedOption as ControllerTriggerOption).Trigger = ControllerTrigger.Unset;
                else
                    (previousSelectedOption as ControllerThumbStickOption).ThumbStick = ControllerThumbStick.Unset;

                previousSelectedOption.IsSelected = false;
            }

            if (newSelectedOption.Function != Function.None)
            {
                if (previousSelectedOption is ControllerButtonOption)
                    (newSelectedOption as ControllerButtonOption).Button = (ControllerButton)element;
                else if (previousSelectedOption is ControllerTriggerOption)
                    (newSelectedOption as ControllerTriggerOption).Trigger = (ControllerTrigger)element;
                else
                    (newSelectedOption as ControllerThumbStickOption).ThumbStick = (ControllerThumbStick)element;

                newSelectedOption.IsSelected = true;
            }
        }
        private void SaveElementsStateToSettings(int selectedOptionIndex, string propertyName, Enum element, string resetElementPropertyName)
        {
            if (element is ControllerButton)
                Settings.Default[propertyName] = this.ControllerButtonOptions.ElementAt(selectedOptionIndex).Function;
            else if (element is ControllerTrigger)
                Settings.Default[propertyName] = this.ControllerTriggerOptions.ElementAt(selectedOptionIndex).Function;
            else
            {
                if ((ControllerThumbStick)element == ControllerThumbStick.L)
                    Settings.Default[propertyName] = this.ControllerLThumbStickOptions.ElementAt(selectedOptionIndex).Function;
                else
                    Settings.Default[propertyName] = this.ControllerRThumbStickOptions.ElementAt(selectedOptionIndex).Function;
            }

            if (resetElementPropertyName != null)
            {
                Settings.Default[resetElementPropertyName] = Function.None;
            }

            Settings.Default.Save();
        }

        private void UpdateRelatedThumbStickInDroneControlsMode(ObservableCollection<ControllerThumbStickOption> changedOptions,
            int changedOptionIndex, Function changedOptionFunctionToCompare, int changedOptionsPreviousSelectedIndex,
            ObservableCollection<ControllerThumbStickOption> relatedOptions, int relatedOptionsPreviousSelectedIndex,
            Function relatedOptionFunctionToFind, ControllerThumbStick relatedControllerThumbStick, string relatedControllerThumbStickName,
            Function relatedOptionDefaultFunction)
        {
            ControllerThumbStickOption relatedOption = null;
            if (changedOptions.ElementAt(changedOptionIndex).Function == changedOptionFunctionToCompare)
                relatedOption = relatedOptions.FirstOrDefault(o => o.Function == relatedOptionFunctionToFind);
            else if (changedOptions.ElementAt(changedOptionsPreviousSelectedIndex).Function == changedOptionFunctionToCompare)
                relatedOption = relatedOptions.FirstOrDefault(o => o.Function == relatedOptionDefaultFunction);

            if (relatedOption != null)
            {
                int relatedOptionIndex = relatedOptions.IndexOf(relatedOption);

                this.ChangeElementOption(relatedControllerThumbStick, relatedOptionsPreviousSelectedIndex, relatedOptionIndex);

                Settings.Default[relatedControllerThumbStickName] = relatedOption.Function;

                Settings.Default.Save();

                if (relatedControllerThumbStick == ControllerThumbStick.L)
                    this.NotifyOfSelectedLThumbStickIndexChanged();
                else
                    this.NotifyOfSelectedRThumbStickIndexChanged();
            }
        }

        private void NotifyOfSelectedButtonIndexChanged()
        {
            this.RaisePropertyChanged(nameof(SelectedAButtonIndex));
            this.RaisePropertyChanged(nameof(SelectedBButtonIndex));
            this.RaisePropertyChanged(nameof(SelectedXButtonIndex));
            this.RaisePropertyChanged(nameof(SelectedYButtonIndex));
            this.RaisePropertyChanged(nameof(SelectedRBumberIndex));
            this.RaisePropertyChanged(nameof(SelectedStartButtonIndex));
        }
        private void NotifyOfSelectedTriggerIndexChanged()
        {
            this.RaisePropertyChanged(nameof(SelectedLTriggerIndex));
            this.RaisePropertyChanged(nameof(SelectedRTriggerIndex));
        }
        private void NotifyOfSelectedLThumbStickIndexChanged()
        {
            this.RaisePropertyChanged(nameof(SelectedLStickIndex));
        }
        private void NotifyOfSelectedRThumbStickIndexChanged()
        {
            this.RaisePropertyChanged(nameof(SelectedRStickIndex));
        }
        private void NotifyOfIsTriggerEnabledChanged()
        {
            this.RaisePropertyChanged(nameof(IsTriggerEnabled));
        }

        private void ResetControllerComboboxOptions()
        {
            this.ControllerButtonOptions = new ObservableCollection<ControllerButtonOption>(_controllerButtonOptions);
            this.ControllerTriggerOptions = new ObservableCollection<ControllerTriggerOption>(_controllerTriggerOptions);
            this.ControllerLThumbStickOptions = new ObservableCollection<ControllerThumbStickOption>(_controllerLThumbStickOptions);
            this.ControllerRThumbStickOptions = new ObservableCollection<ControllerThumbStickOption>(_controllerRThumbStickOptions);
        }

        private void ExecuteRestoreDefaultCommand()
        {
            if (MessageBox.Show("Do you really want Restore Default settings?", "Question",
                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                Settings.Default.Reset();

                this.CreateEmptyControllerElementOptions();
                this.ApplyLocalUserSettingsToControllerElementOptions();

                this.ResetControllerComboboxOptions();
                this.NotifyOfSelectedButtonIndexChanged();
                this.NotifyOfSelectedTriggerIndexChanged();
                this.NotifyOfSelectedLThumbStickIndexChanged();
                this.NotifyOfSelectedRThumbStickIndexChanged();
                this.NotifyOfIsTriggerEnabledChanged();
            }
        }
        private bool CanExecuteRestoreDefaultCommand()
        {
            return true;
        }

        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}