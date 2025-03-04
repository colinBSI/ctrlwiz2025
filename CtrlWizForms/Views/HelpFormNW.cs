using System;
using System.Drawing;
using System.Windows.Forms;

namespace CtrlWiz.Forms.Views
{
    public partial class HelpFormNW : Form, IHelpForm
    {
        public HelpFormNW()
        {
            InitializeComponent();
            ConfigurationButton();
        }

        private void ConfigurationButton()
        {
            #region Button Left
            this.BLeft.Parent = this.pictureBox1;
            this.BLeft.BackColor = Color.Transparent;
            this.BLeft.FlatAppearance.MouseDownBackColor = Color.Transparent;
            this.BLeft.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, Color.Green);
            #endregion

            #region Button Mid
            this.BMid.Parent = this.pictureBox1;
            this.BMid.BackColor = Color.Transparent;
            this.BMid.FlatAppearance.MouseDownBackColor = Color.Transparent;
            this.BMid.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, Color.Green);
            #endregion

            #region Button Right
            this.BRight.Parent = this.pictureBox1;
            this.BRight.BackColor = Color.Transparent;
            this.BRight.FlatAppearance.MouseDownBackColor = Color.Transparent;
            this.BRight.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, Color.Green);
            #endregion
        }

        private void BLeft_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.robinsmorton.com");
        }
        private void BMid_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.buildfore.com/products/ctrlwiz/ctrlwiz-xbox-controller-for-navisworks");
        }
        private void BRight_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.viatechnik.com");
        }
    }
}

#region Working WinForms Version
//using CtrlWiz.Forms.Models;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Drawing;
//using System.Windows;
//using System.Windows.Forms;
//using MessageBox = System.Windows.MessageBox;

//namespace CtrlWiz.Forms.Views
//{
//    public partial class HelpFormNW : Form, IHelpForm
//    {
//        private int _previousAButtonOptionIndex = -1;
//        private int _previousBButtonOptionIndex = -1;
//        private int _previousXButtonOptionIndex = -1;
//        private int _previousYButtonOptionIndex = -1;
//        private int _previousRBumberOptionIndex = -1;
//        private int _previousStartButtonOptionIndex = -1;
//        private int _previousLTriggerOptionIndex = -1;
//        private int _previousRTriggerOptionIndex = -1;
//        private int _previousLStickOptionIndex = -1;
//        private int _previousRStickOptionIndex = -1;

//        private List<ControllerButtonOption> _controllerButtonOptions = null;
//        private List<ControllerTriggerOption> _controllerTriggerOptions = null;
//        private List<ControllerThumbStickOption> _controllerThumbStickOptions = null;

//        private Font _myFont = new Font("Aerial", 10, FontStyle.Regular);

//        public HelpFormNW()
//        {
//            InitializeComponent();
//            ConfigurationButton();
//            BindComboBoxWithControllerFunctions();
//        }

//        private void ConfigurationButton()
//        {
//            #region Button Left
//            this.BLeft.Parent = this.pictureBox1;
//            this.BLeft.BackColor = Color.Transparent;
//            this.BLeft.FlatAppearance.MouseDownBackColor = Color.Transparent;
//            this.BLeft.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, Color.Green);
//            #endregion

//            #region Button Mid
//            this.BMid.Parent = this.pictureBox1;
//            this.BMid.BackColor = Color.Transparent;
//            this.BMid.FlatAppearance.MouseDownBackColor = Color.Transparent;
//            this.BMid.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, Color.Green);
//            #endregion

//            #region Button Right
//            this.BRight.Parent = this.pictureBox1;
//            this.BRight.BackColor = Color.Transparent;
//            this.BRight.FlatAppearance.MouseDownBackColor = Color.Transparent;
//            this.BRight.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, Color.Green);
//            #endregion
//        }

//        private void BLeft_Click(object sender, EventArgs e)
//        {
//            System.Diagnostics.Process.Start("https://buildfore.com/ctrlwiz/xboxnavis");
//        }
//        private void BMid_Click(object sender, EventArgs e)
//        {
//            System.Diagnostics.Process.Start("https://buildfore.com/ctrlwiz?utm_source=Navisworks");
//        }
//        private void BRight_Click(object sender, EventArgs e)
//        {
//            System.Diagnostics.Process.Start("https://www.viatechnik.com/");
//        }

//        private void BindComboBoxWithControllerFunctions()
//        {
//            // User Settings Path.
//            //MessageBox.Show(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath);

//            // Default settings for Buttons.
//            _controllerButtonOptions = new List<ControllerButtonOption>();
//            _controllerButtonOptions.Add(new ControllerButtonOption(Function.HideUnhide, "Hide/Unhide"));
//            _controllerButtonOptions.Add(new ControllerButtonOption(Function.UnhideAll, "Unhide All"));
//            _controllerButtonOptions.Add(new ControllerButtonOption(Function.SelectDeselect, "Select - Deselect"));
//            _controllerButtonOptions.Add(new ControllerButtonOption(Function.DeselectAll, "Deselect All"));
//            _controllerButtonOptions.Add(new ControllerButtonOption(Function.ShortestDistance, "Shortest Distance"));
//            _controllerButtonOptions.Add(new ControllerButtonOption(Function.NextViewpoint, "Next Viewpoint"));
//            _controllerButtonOptions.Add(new ControllerButtonOption(Function.EnableTextMarkup, "Enable Text Markup"));
//            _controllerButtonOptions.Add(new ControllerButtonOption(Function.CreateViewpoint, "Create Viewpoint"));
//            //_controllerButtonOptions.Add(new ControllerButtonOption(Function.ShowHideProperties, "Show/Hide Properties"));
//            //_controllerButtonOptions.Add(new ControllerButtonOption(Function.ShowHideSelectionTree, "Show/Hide Selection Tree"));
//            //_controllerButtonOptions.Add(new ControllerButtonOption(Function.ShowHideViewpoints, "Show/Hide Viewpoints"));
//            //_controllerButtonOptions.Add(new ControllerButtonOption(Function.MoveLeftInTree, "Move Left In Tree"));
//            //_controllerButtonOptions.Add(new ControllerButtonOption(Function.PersonView3rd, "3rd Person View"));
//            //_controllerButtonOptions.Add(new ControllerButtonOption(Function.CollisionGravity, "Collision/Gravity"));
//            _controllerButtonOptions.Add(new ControllerButtonOption(ControllerButton.None, Function.None, "None"));

//            // Default settings for Triggers.
//            _controllerTriggerOptions = new List<ControllerTriggerOption>();
//            _controllerTriggerOptions.Add(new ControllerTriggerOption(Function.ElevationDown, "Elevation Down"));
//            _controllerTriggerOptions.Add(new ControllerTriggerOption(Function.ElevationUp, "Elevation Up"));
//            _controllerTriggerOptions.Add(new ControllerTriggerOption(ControllerTrigger.None, Function.None, "None"));

//            // Default settings for ThumbSticks.
//            _controllerThumbStickOptions = new List<ControllerThumbStickOption>();
//            _controllerThumbStickOptions.Add(new ControllerThumbStickOption(Function.MoveSprint, "Move/Sprint"));
//            _controllerThumbStickOptions.Add(new ControllerThumbStickOption(Function.Look, "Look"));
//            _controllerThumbStickOptions.Add(new ControllerThumbStickOption(ControllerThumbStick.None, Function.None, "None"));

//            // Adjust options according to local User Settings.
//            // Buttons.
//            Function localAButtonFunction = Properties.Settings.Default["NV_AButton"] != null ? (Function)Properties.Settings.Default["NV_AButton"] : Function.Unset;
//            if (localAButtonFunction != Function.Unset)
//            {
//                _controllerButtonOptions.ForEach(o =>
//                {
//                    if (o.Button == ControllerButton.A)
//                    {
//                        o.Button = ControllerButton.Unset;
//                        o.IsSelected = false;
//                    }

//                    if (localAButtonFunction != Function.None && o.Function == localAButtonFunction)
//                    {
//                        o.Button = ControllerButton.A;
//                        o.IsSelected = true;
//                    }
//                });
//            }
//            Function localBButtonFunction = Properties.Settings.Default["NV_BButton"] != null ? (Function)Properties.Settings.Default["NV_BButton"] : Function.Unset;
//            if (localBButtonFunction != Function.Unset)
//            {
//                _controllerButtonOptions.ForEach(o =>
//                {
//                    if (o.Button == ControllerButton.B)
//                    {
//                        o.Button = ControllerButton.Unset;
//                        o.IsSelected = false;
//                    }

//                    if (localBButtonFunction != Function.None && o.Function == localBButtonFunction)
//                    {
//                        o.Button = ControllerButton.B;
//                        o.IsSelected = true;
//                    }
//                });
//            }
//            Function localXButtonFunction = Properties.Settings.Default["NV_XButton"] != null ? (Function)Properties.Settings.Default["NV_XButton"] : Function.Unset;
//            if (localXButtonFunction != Function.Unset)
//            {
//                _controllerButtonOptions.ForEach(o =>
//                {
//                    if (o.Button == ControllerButton.X)
//                    {
//                        o.Button = ControllerButton.Unset;
//                        o.IsSelected = false;
//                    }

//                    if (localXButtonFunction != Function.None && o.Function == localXButtonFunction)
//                    {
//                        o.Button = ControllerButton.X;
//                        o.IsSelected = true;
//                    }
//                });
//            }
//            Function localYButtonFunction = Properties.Settings.Default["NV_YButton"] != null ? (Function)Properties.Settings.Default["NV_YButton"] : Function.Unset;
//            if (localYButtonFunction != Function.Unset)
//            {
//                _controllerButtonOptions.ForEach(o =>
//                {
//                    if (o.Button == ControllerButton.Y)
//                    {
//                        o.Button = ControllerButton.Unset;
//                        o.IsSelected = false;
//                    }

//                    if (localYButtonFunction != Function.None && o.Function == localYButtonFunction)
//                    {
//                        o.Button = ControllerButton.Y;
//                        o.IsSelected = true;
//                    }
//                });
//            }
//            Function localRBumberFunction = Properties.Settings.Default["NV_RBumber"] != null ? (Function)Properties.Settings.Default["NV_RBumber"] : Function.Unset;
//            if (localRBumberFunction != Function.Unset)
//            {
//                _controllerButtonOptions.ForEach(o =>
//                {
//                    if (o.Button == ControllerButton.RBumber)
//                    {
//                        o.Button = ControllerButton.Unset;
//                        o.IsSelected = false;
//                    }

//                    if (localRBumberFunction != Function.None && o.Function == localRBumberFunction)
//                    {
//                        o.Button = ControllerButton.RBumber;
//                        o.IsSelected = true;
//                    }
//                });
//            }
//            Function localStartButtonFunction = Properties.Settings.Default["NV_StartButton"] != null ? (Function)Properties.Settings.Default["NV_StartButton"] : Function.Unset;
//            if (localStartButtonFunction != Function.Unset)
//            {
//                _controllerButtonOptions.ForEach(o =>
//                {
//                    if (o.Button == ControllerButton.Start)
//                    {
//                        o.Button = ControllerButton.Unset;
//                        o.IsSelected = false;
//                    }

//                    if (localStartButtonFunction != Function.None && o.Function == localStartButtonFunction)
//                    {
//                        o.Button = ControllerButton.Start;
//                        o.IsSelected = true;
//                    }
//                });
//            }

//            // Triggers.
//            Function localLTriggerFunction = Properties.Settings.Default["NV_LTrigger"] != null ? (Function)Properties.Settings.Default["NV_LTrigger"] : Function.Unset;
//            if (localLTriggerFunction != Function.Unset)
//            {
//                _controllerTriggerOptions.ForEach(o =>
//                {
//                    if (o.Trigger == ControllerTrigger.LT)
//                    {
//                        o.Trigger = ControllerTrigger.Unset;
//                        o.IsSelected = false;
//                    }

//                    if (localLTriggerFunction != Function.None && o.Function == localLTriggerFunction)
//                    {
//                        o.Trigger = ControllerTrigger.LT;
//                        o.IsSelected = true;
//                    }
//                });
//            }
//            Function localRTriggerFunction = Properties.Settings.Default["NV_RTrigger"] != null ? (Function)Properties.Settings.Default["NV_RTrigger"] : Function.Unset;
//            if (localRTriggerFunction != Function.Unset)
//            {
//                _controllerTriggerOptions.ForEach(o =>
//                {
//                    if (o.Trigger == ControllerTrigger.RT)
//                    {
//                        o.Trigger = ControllerTrigger.Unset;
//                        o.IsSelected = false;
//                    }

//                    if (localRTriggerFunction != Function.None && o.Function == localRTriggerFunction)
//                    {
//                        o.Trigger = ControllerTrigger.RT;
//                        o.IsSelected = true;
//                    }
//                });
//            }

//            // ThumbSticks.
//            Function localLStickFunction = Properties.Settings.Default["NV_LStick"] != null ? (Function)Properties.Settings.Default["NV_LStick"] : Function.Unset;
//            if (localLStickFunction != Function.Unset)
//            {
//                _controllerThumbStickOptions.ForEach(o =>
//                {
//                    if (o.ThumbStick == ControllerThumbStick.L)
//                    {
//                        o.ThumbStick = ControllerThumbStick.Unset;
//                        o.IsSelected = false;
//                    }

//                    if (localLStickFunction != Function.None && o.Function == localLStickFunction)
//                    {
//                        o.ThumbStick = ControllerThumbStick.L;
//                        o.IsSelected = true;
//                    }
//                });
//            }
//            Function localRStickFunction = Properties.Settings.Default["NV_RStick"] != null ? (Function)Properties.Settings.Default["NV_RStick"] : Function.Unset;
//            if (localRStickFunction != Function.Unset)
//            {
//                _controllerThumbStickOptions.ForEach(o =>
//                {
//                    if (o.ThumbStick == ControllerThumbStick.R)
//                    {
//                        o.ThumbStick = ControllerThumbStick.Unset;
//                        o.IsSelected = false;
//                    }

//                    if (localRStickFunction != Function.None && o.Function == localRStickFunction)
//                    {
//                        o.ThumbStick = ControllerThumbStick.R;
//                        o.IsSelected = true;
//                    }
//                });
//            }

//            // Binding collections.
//            // Buttons.
//            // AButtonComboBox
//            this.AButtonComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
//            this.AButtonComboBox.DisplayMember = "FunctionName";
//            this.AButtonComboBox.ValueMember = null;
//            this.AButtonComboBox.DataSource = new BindingSource() { DataSource = _controllerButtonOptions };
//            this.AButtonComboBox.SelectedIndex = localAButtonFunction == Function.None ?
//                _controllerButtonOptions.FindIndex(co => co.Function == Function.None) : _controllerButtonOptions.FindIndex(co =>
//                {
//                    if (co.Button == ControllerButton.A && co.IsSelected)
//                        return true;
//                    else
//                        return false;
//                });
//            this.AButtonComboBox.DrawMode = DrawMode.OwnerDrawFixed;
//            // BButtonComboBox
//            this.BButtonComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
//            this.BButtonComboBox.DisplayMember = "FunctionName";
//            this.BButtonComboBox.ValueMember = null;
//            this.BButtonComboBox.DataSource = new BindingSource() { DataSource = _controllerButtonOptions };
//            this.BButtonComboBox.SelectedIndex = localBButtonFunction == Function.None ?
//                _controllerButtonOptions.FindIndex(co => co.Function == Function.None) : _controllerButtonOptions.FindIndex(co =>
//                {
//                    if (co.Button == ControllerButton.B && co.IsSelected)
//                        return true;
//                    else
//                        return false;
//                });
//            this.BButtonComboBox.DrawMode = DrawMode.OwnerDrawFixed;
//            // XButtonComboBox
//            this.XButtonComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
//            this.XButtonComboBox.DisplayMember = "FunctionName";
//            this.XButtonComboBox.ValueMember = null;
//            this.XButtonComboBox.DataSource = new BindingSource() { DataSource = _controllerButtonOptions };
//            this.XButtonComboBox.SelectedIndex = localXButtonFunction == Function.None ?
//                _controllerButtonOptions.FindIndex(co => co.Function == Function.None) : _controllerButtonOptions.FindIndex(co =>
//                {
//                    if (co.Button == ControllerButton.X && co.IsSelected)
//                        return true;
//                    else
//                        return false;
//                });
//            this.XButtonComboBox.DrawMode = DrawMode.OwnerDrawFixed;
//            // YButtonComboBox
//            this.YButtonComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
//            this.YButtonComboBox.DisplayMember = "FunctionName";
//            this.YButtonComboBox.ValueMember = null;
//            this.YButtonComboBox.DataSource = new BindingSource() { DataSource = _controllerButtonOptions };
//            this.YButtonComboBox.SelectedIndex = localYButtonFunction == Function.None ?
//                _controllerButtonOptions.FindIndex(co => co.Function == Function.None) : _controllerButtonOptions.FindIndex(co =>
//                {
//                    if (co.Button == ControllerButton.Y && co.IsSelected)
//                        return true;
//                    else
//                        return false;
//                });
//            this.YButtonComboBox.DrawMode = DrawMode.OwnerDrawFixed;
//            // RBumberComboBox
//            this.RBumberComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
//            this.RBumberComboBox.DisplayMember = "FunctionName";
//            this.RBumberComboBox.ValueMember = null;
//            this.RBumberComboBox.DataSource = new BindingSource() { DataSource = _controllerButtonOptions };
//            this.RBumberComboBox.SelectedIndex = localRBumberFunction == Function.None ?
//                _controllerButtonOptions.FindIndex(co => co.Function == Function.None) : _controllerButtonOptions.FindIndex(co =>
//                {
//                    if (co.Button == ControllerButton.RBumber && co.IsSelected)
//                        return true;
//                    else
//                        return false;
//                });
//            this.RBumberComboBox.DrawMode = DrawMode.OwnerDrawFixed;
//            // StartButtonComboBox
//            this.StartButtonComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
//            this.StartButtonComboBox.DisplayMember = "FunctionName";
//            this.StartButtonComboBox.ValueMember = null;
//            this.StartButtonComboBox.DataSource = new BindingSource() { DataSource = _controllerButtonOptions };
//            this.StartButtonComboBox.SelectedIndex = localStartButtonFunction == Function.None ?
//                _controllerButtonOptions.FindIndex(co => co.Function == Function.None) : _controllerButtonOptions.FindIndex(co =>
//                {
//                    if (co.Button == ControllerButton.Start && co.IsSelected)
//                        return true;
//                    else
//                        return false;
//                });
//            this.StartButtonComboBox.DrawMode = DrawMode.OwnerDrawFixed;

//            // Triggers.
//            // LTriggerComboBox
//            this.LTriggerComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
//            this.LTriggerComboBox.DisplayMember = "FunctionName";
//            this.LTriggerComboBox.ValueMember = null;
//            this.LTriggerComboBox.DataSource = new BindingSource() { DataSource = _controllerTriggerOptions };
//            this.LTriggerComboBox.SelectedIndex = localLTriggerFunction == Function.None ?
//                _controllerTriggerOptions.FindIndex(co => co.Function == Function.None) : _controllerTriggerOptions.FindIndex(co =>
//                {
//                    if (co.Trigger == ControllerTrigger.LT && co.IsSelected)
//                        return true;
//                    else
//                        return false;
//                });
//            this.LTriggerComboBox.DrawMode = DrawMode.OwnerDrawFixed;
//            // RTriggerComboBox
//            this.RTriggerComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
//            this.RTriggerComboBox.DisplayMember = "FunctionName";
//            this.RTriggerComboBox.ValueMember = null;
//            this.RTriggerComboBox.DataSource = new BindingSource() { DataSource = _controllerTriggerOptions };
//            this.RTriggerComboBox.SelectedIndex = localRTriggerFunction == Function.None ?
//                _controllerTriggerOptions.FindIndex(co => co.Function == Function.None) : _controllerTriggerOptions.FindIndex(co =>
//                {
//                    if (co.Trigger == ControllerTrigger.RT && co.IsSelected)
//                        return true;
//                    else
//                        return false;
//                });
//            this.RTriggerComboBox.DrawMode = DrawMode.OwnerDrawFixed;

//            // ThumbSticks.
//            // LStickComboBox
//            this.LStickComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
//            this.LStickComboBox.DisplayMember = "FunctionName";
//            this.LStickComboBox.ValueMember = null;
//            this.LStickComboBox.DataSource = new BindingSource() { DataSource = _controllerThumbStickOptions };
//            this.LStickComboBox.SelectedIndex = localLStickFunction == Function.None ?
//                _controllerThumbStickOptions.FindIndex(co => co.Function == Function.None) : _controllerThumbStickOptions.FindIndex(co =>
//                {
//                    if (co.ThumbStick == ControllerThumbStick.L && co.IsSelected)
//                        return true;
//                    else
//                        return false;
//                });
//            this.LStickComboBox.DrawMode = DrawMode.OwnerDrawFixed;
//            // RStickComboBox
//            this.RStickComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
//            this.RStickComboBox.DisplayMember = "FunctionName";
//            this.RStickComboBox.ValueMember = null;
//            this.RStickComboBox.DataSource = new BindingSource() { DataSource = _controllerThumbStickOptions };
//            this.RStickComboBox.SelectedIndex = localRStickFunction == Function.None ?
//                _controllerThumbStickOptions.FindIndex(co => co.Function == Function.None) : _controllerThumbStickOptions.FindIndex(co =>
//                {
//                    if (co.ThumbStick == ControllerThumbStick.R && co.IsSelected)
//                        return true;
//                    else
//                        return false;
//                });
//            this.RStickComboBox.DrawMode = DrawMode.OwnerDrawFixed;

//            // Subscribe to option selection events.
//            // Buttons.
//            // AButtonComboBox
//            this.AButtonComboBox.Enter += AButtonComboBox_Enter;
//            this.AButtonComboBox.SelectionChangeCommitted += AButtonComboBox_SelectionChangeCommitted;
//            this.AButtonComboBox.DrawItem += AdjustWidthComboBox_DropDown;
//            this.AButtonComboBox.DrawItem += AButtonComboBox_DrawItem;
//            // BButtonComboBox
//            this.BButtonComboBox.Enter += BButtonComboBox_Enter;
//            this.BButtonComboBox.SelectionChangeCommitted += BButtonComboBox_SelectionChangeCommitted;
//            this.BButtonComboBox.DrawItem += AdjustWidthComboBox_DropDown;
//            this.BButtonComboBox.DrawItem += BButtonComboBox_DrawItem;
//            // XButtonComboBox
//            this.XButtonComboBox.Enter += XButtonComboBox_Enter;
//            this.XButtonComboBox.SelectionChangeCommitted += XButtonComboBox_SelectionChangeCommitted;
//            this.XButtonComboBox.DrawItem += AdjustWidthComboBox_DropDown;
//            this.XButtonComboBox.DrawItem += XButtonComboBox_DrawItem;
//            // YButtonComboBox
//            this.YButtonComboBox.Enter += YButtonComboBox_Enter;
//            this.YButtonComboBox.SelectionChangeCommitted += YButtonComboBox_SelectionChangeCommitted;
//            this.YButtonComboBox.DrawItem += AdjustWidthComboBox_DropDown;
//            this.YButtonComboBox.DrawItem += YButtonComboBox_DrawItem;
//            // RBumberComboBox
//            this.RBumberComboBox.Enter += RBumberComboBox_Enter;
//            this.RBumberComboBox.SelectionChangeCommitted += RBumberComboBox_SelectionChangeCommitted;
//            this.RBumberComboBox.DrawItem += AdjustWidthComboBox_DropDown;
//            this.RBumberComboBox.DrawItem += RBumberComboBox_DrawItem;
//            // StartButtonComboBox
//            this.StartButtonComboBox.Enter += StartButtonComboBox_Enter;
//            this.StartButtonComboBox.SelectionChangeCommitted += StartButtonComboBox_SelectionChangeCommitted;
//            this.StartButtonComboBox.DrawItem += AdjustWidthComboBox_DropDown;
//            this.StartButtonComboBox.DrawItem += StartButtonComboBox_DrawItem;

//            // Triggers.
//            // LTriggerComboBox
//            this.LTriggerComboBox.Enter += LTriggerComboBox_Enter;
//            this.LTriggerComboBox.SelectionChangeCommitted += LTriggerComboBox_SelectionChangeCommitted;
//            this.LTriggerComboBox.DrawItem += LTriggerComboBox_DrawItem;
//            // RTriggerComboBox
//            this.RTriggerComboBox.Enter += RTriggerComboBox_Enter;
//            this.RTriggerComboBox.SelectionChangeCommitted += RTriggerComboBox_SelectionChangeCommitted;
//            this.RTriggerComboBox.DrawItem += RTriggerComboBox_DrawItem;

//            // ThumbSticks.
//            // LStickComboBox
//            this.LStickComboBox.Enter += LStickComboBox_Enter;
//            this.LStickComboBox.SelectionChangeCommitted += LStickComboBox_SelectionChangeCommitted;
//            this.LStickComboBox.DrawItem += LStickComboBox_DrawItem;
//            // RStickComboBox
//            this.RStickComboBox.Enter += RStickComboBox_Enter;
//            this.RStickComboBox.SelectionChangeCommitted += RStickComboBox_SelectionChangeCommitted;
//            this.RStickComboBox.DrawItem += RStickComboBox_DrawItem;
//        }

//        private void ResetPreviousComboBoxOptionIndexes()
//        {
//            _previousAButtonOptionIndex = -1;
//            _previousBButtonOptionIndex = -1;
//            _previousXButtonOptionIndex = -1;
//            _previousYButtonOptionIndex = -1;
//            _previousRBumberOptionIndex = -1;
//            _previousStartButtonOptionIndex = -1;
//            _previousLTriggerOptionIndex = -1;
//            _previousRTriggerOptionIndex = -1;
//            _previousLStickOptionIndex = -1;
//            _previousRStickOptionIndex = -1;
//        }

//        private void RestoreDefaultButton_Click(object sender, EventArgs e)
//        {
//            if (MessageBox.Show("Do you really want Restore Default settings?", "Question",
//                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
//            {
//                ResetPreviousComboBoxOptionIndexes();

//                Properties.Settings.Default.Reset();

//                BindComboBoxWithControllerFunctions();
//            }
//        }

//        private void AButtonComboBox_Enter(object sender, EventArgs e)
//        {
//            _previousAButtonOptionIndex = this.AButtonComboBox.SelectedIndex;
//        }
//        private void AButtonComboBox_DrawItem(object sender, DrawItemEventArgs e)
//        {
//            if (e.Index >= 0)
//            {
//                ControllerButtonOption selectedOption = this.AButtonComboBox.Items[e.Index] as ControllerButtonOption;

//                // Disabling selected item.
//                if (selectedOption.IsSelected && selectedOption.Button != ControllerButton.A &&
//                    selectedOption.Button != ControllerButton.None)
//                {
//                    e.Graphics.DrawString(selectedOption.FunctionName, _myFont, Brushes.LightGray, e.Bounds);
//                }
//                else
//                {
//                    e.DrawBackground();
//                    e.Graphics.DrawString(selectedOption.FunctionName, _myFont, Brushes.Black, e.Bounds);
//                    e.DrawFocusRectangle();
//                }
//            }
//        }
//        private void AButtonComboBox_SelectionChangeCommitted(object sender, EventArgs e)
//        {
//            if (this.AButtonComboBox.SelectedIndex == _previousAButtonOptionIndex)
//                return;

//            ControllerButtonOption selectedOption = this.AButtonComboBox.SelectedValue as ControllerButtonOption;

//            if (selectedOption.Button != ControllerButton.Unset && selectedOption.Button != ControllerButton.None)
//            {
//                this.AButtonComboBox.SelectedIndex = _previousAButtonOptionIndex;
//            }
//            else
//            {
//                Properties.Settings.Default["NV_AButton"] = (Function)selectedOption.Function;

//                if (selectedOption.Button != ControllerButton.None)
//                {
//                    selectedOption.IsSelected = true;
//                    selectedOption.Button = ControllerButton.A;
//                }

//                ControllerButtonOption previousSelectedOption = this.AButtonComboBox.Items[_previousAButtonOptionIndex] as ControllerButtonOption;

//                if (previousSelectedOption.Button != ControllerButton.None)
//                {
//                    (this.AButtonComboBox.Items[_previousAButtonOptionIndex] as ControllerButtonOption).Button = ControllerButton.Unset;
//                    (this.AButtonComboBox.Items[_previousAButtonOptionIndex] as ControllerButtonOption).IsSelected = false;
//                }

//                _previousAButtonOptionIndex = this.AButtonComboBox.SelectedIndex;

//                Properties.Settings.Default.Save();
//            }
//        }

//        private void BButtonComboBox_Enter(object sender, EventArgs e)
//        {
//            _previousBButtonOptionIndex = this.BButtonComboBox.SelectedIndex;
//        }
//        private void BButtonComboBox_DrawItem(object sender, DrawItemEventArgs e)
//        {
//            if (e.Index >= 0)
//            {
//                ControllerButtonOption selectedOption = this.BButtonComboBox.Items[e.Index] as ControllerButtonOption;

//                // Disabling selected item.
//                if (selectedOption.IsSelected && selectedOption.Button != ControllerButton.B &&
//                    selectedOption.Button != ControllerButton.None)
//                {
//                    e.Graphics.DrawString(selectedOption.FunctionName, _myFont, Brushes.LightGray, e.Bounds);
//                }
//                else
//                {
//                    e.DrawBackground();
//                    e.Graphics.DrawString(selectedOption.FunctionName, _myFont, Brushes.Black, e.Bounds);
//                    e.DrawFocusRectangle();
//                }
//            }
//        }
//        private void BButtonComboBox_SelectionChangeCommitted(object sender, EventArgs e)
//        {
//            if (this.BButtonComboBox.SelectedIndex == _previousBButtonOptionIndex)
//                return;

//            ControllerButtonOption selectedOption = this.BButtonComboBox.SelectedValue as ControllerButtonOption;

//            if (selectedOption.Button != ControllerButton.Unset && selectedOption.Button != ControllerButton.None)
//            {
//                this.BButtonComboBox.SelectedIndex = _previousBButtonOptionIndex;
//            }
//            else
//            {
//                Properties.Settings.Default["NV_BButton"] = (Function)selectedOption.Function;

//                if (selectedOption.Button != ControllerButton.None)
//                {
//                    selectedOption.IsSelected = true;
//                    selectedOption.Button = ControllerButton.B;
//                }

//                ControllerButtonOption previousSelectedOption = this.BButtonComboBox.Items[_previousBButtonOptionIndex] as ControllerButtonOption;

//                if (previousSelectedOption.Button != ControllerButton.None)
//                {
//                    (this.BButtonComboBox.Items[_previousBButtonOptionIndex] as ControllerButtonOption).Button = ControllerButton.Unset;
//                    (this.BButtonComboBox.Items[_previousBButtonOptionIndex] as ControllerButtonOption).IsSelected = false;
//                }

//                _previousBButtonOptionIndex = this.BButtonComboBox.SelectedIndex;

//                Properties.Settings.Default.Save();
//            }
//        }

//        private void XButtonComboBox_Enter(object sender, EventArgs e)
//        {
//            _previousXButtonOptionIndex = this.XButtonComboBox.SelectedIndex;
//        }
//        private void XButtonComboBox_DrawItem(object sender, DrawItemEventArgs e)
//        {
//            if (e.Index >= 0)
//            {
//                ControllerButtonOption selectedOption = this.XButtonComboBox.Items[e.Index] as ControllerButtonOption;

//                // Disabling selected item.
//                if (selectedOption.IsSelected && selectedOption.Button != ControllerButton.X &&
//                    selectedOption.Button != ControllerButton.None)
//                {
//                    e.Graphics.DrawString(selectedOption.FunctionName, _myFont, Brushes.LightGray, e.Bounds);
//                }
//                else
//                {
//                    e.DrawBackground();
//                    e.Graphics.DrawString(selectedOption.FunctionName, _myFont, Brushes.Black, e.Bounds);
//                    e.DrawFocusRectangle();
//                }
//            }
//        }
//        private void XButtonComboBox_SelectionChangeCommitted(object sender, EventArgs e)
//        {
//            if (this.XButtonComboBox.SelectedIndex == _previousXButtonOptionIndex)
//                return;

//            ControllerButtonOption selectedOption = this.XButtonComboBox.SelectedValue as ControllerButtonOption;

//            if (selectedOption.Button != ControllerButton.Unset && selectedOption.Button != ControllerButton.None)
//            {
//                this.XButtonComboBox.SelectedIndex = _previousXButtonOptionIndex;
//            }
//            else
//            {
//                Properties.Settings.Default["NV_XButton"] = (Function)selectedOption.Function;

//                if (selectedOption.Button != ControllerButton.None)
//                {
//                    selectedOption.IsSelected = true;
//                    selectedOption.Button = ControllerButton.X;
//                }

//                ControllerButtonOption previousSelectedOption = this.XButtonComboBox.Items[_previousXButtonOptionIndex] as ControllerButtonOption;

//                if (previousSelectedOption.Button != ControllerButton.None)
//                {
//                    (this.XButtonComboBox.Items[_previousXButtonOptionIndex] as ControllerButtonOption).Button = ControllerButton.Unset;
//                    (this.XButtonComboBox.Items[_previousXButtonOptionIndex] as ControllerButtonOption).IsSelected = false;
//                }

//                _previousXButtonOptionIndex = this.XButtonComboBox.SelectedIndex;

//                Properties.Settings.Default.Save();
//            }
//        }

//        private void YButtonComboBox_Enter(object sender, EventArgs e)
//        {
//            _previousYButtonOptionIndex = this.YButtonComboBox.SelectedIndex;
//        }
//        private void YButtonComboBox_DrawItem(object sender, DrawItemEventArgs e)
//        {
//            if (e.Index >= 0)
//            {
//                ControllerButtonOption selectedOption = this.YButtonComboBox.Items[e.Index] as ControllerButtonOption;

//                // Disabling selected item.
//                if (selectedOption.IsSelected && selectedOption.Button != ControllerButton.Y &&
//                    selectedOption.Button != ControllerButton.None)
//                {
//                    e.Graphics.DrawString(selectedOption.FunctionName, _myFont, Brushes.LightGray, e.Bounds);
//                }
//                else
//                {
//                    e.DrawBackground();
//                    e.Graphics.DrawString(selectedOption.FunctionName, _myFont, Brushes.Black, e.Bounds);
//                    e.DrawFocusRectangle();
//                }
//            }
//        }
//        private void YButtonComboBox_SelectionChangeCommitted(object sender, EventArgs e)
//        {
//            if (this.YButtonComboBox.SelectedIndex == _previousYButtonOptionIndex)
//                return;

//            ControllerButtonOption selectedOption = this.YButtonComboBox.SelectedValue as ControllerButtonOption;

//            if (selectedOption.Button != ControllerButton.Unset && selectedOption.Button != ControllerButton.None)
//            {
//                this.YButtonComboBox.SelectedIndex = _previousYButtonOptionIndex;
//            }
//            else
//            {
//                Properties.Settings.Default["NV_YButton"] = (Function)selectedOption.Function;

//                if (selectedOption.Button != ControllerButton.None)
//                {
//                    selectedOption.IsSelected = true;
//                    selectedOption.Button = ControllerButton.Y;
//                }

//                ControllerButtonOption previousSelectedOption = this.YButtonComboBox.Items[_previousYButtonOptionIndex] as ControllerButtonOption;

//                if (previousSelectedOption.Button != ControllerButton.None)
//                {
//                    (this.YButtonComboBox.Items[_previousYButtonOptionIndex] as ControllerButtonOption).Button = ControllerButton.Unset;
//                    (this.YButtonComboBox.Items[_previousYButtonOptionIndex] as ControllerButtonOption).IsSelected = false;
//                }

//                _previousYButtonOptionIndex = this.YButtonComboBox.SelectedIndex;

//                Properties.Settings.Default.Save();
//            }
//        }

//        private void RBumberComboBox_Enter(object sender, EventArgs e)
//        {
//            _previousRBumberOptionIndex = this.RBumberComboBox.SelectedIndex;
//        }
//        private void RBumberComboBox_DrawItem(object sender, DrawItemEventArgs e)
//        {
//            if (e.Index >= 0)
//            {
//                ControllerButtonOption selectedOption = this.RBumberComboBox.Items[e.Index] as ControllerButtonOption;

//                // Disabling selected item.
//                if (selectedOption.IsSelected && selectedOption.Button != ControllerButton.RBumber &&
//                    selectedOption.Button != ControllerButton.None)
//                {
//                    e.Graphics.DrawString(selectedOption.FunctionName, _myFont, Brushes.LightGray, e.Bounds);
//                }
//                else
//                {
//                    e.DrawBackground();
//                    e.Graphics.DrawString(selectedOption.FunctionName, _myFont, Brushes.Black, e.Bounds);
//                    e.DrawFocusRectangle();
//                }
//            }
//        }
//        private void RBumberComboBox_SelectionChangeCommitted(object sender, EventArgs e)
//        {
//            if (this.RBumberComboBox.SelectedIndex == _previousRBumberOptionIndex)
//                return;

//            ControllerButtonOption selectedOption = this.RBumberComboBox.SelectedValue as ControllerButtonOption;

//            if (selectedOption.Button != ControllerButton.Unset && selectedOption.Button != ControllerButton.None)
//            {
//                this.RBumberComboBox.SelectedIndex = _previousRBumberOptionIndex;
//            }
//            else
//            {
//                Properties.Settings.Default["NV_RBumber"] = (Function)selectedOption.Function;

//                if (selectedOption.Button != ControllerButton.None)
//                {
//                    selectedOption.IsSelected = true;
//                    selectedOption.Button = ControllerButton.RBumber;
//                }

//                ControllerButtonOption previousSelectedOption = this.RBumberComboBox.Items[_previousRBumberOptionIndex] as ControllerButtonOption;

//                if (previousSelectedOption.Button != ControllerButton.None)
//                {
//                    (this.RBumberComboBox.Items[_previousRBumberOptionIndex] as ControllerButtonOption).Button = ControllerButton.Unset;
//                    (this.RBumberComboBox.Items[_previousRBumberOptionIndex] as ControllerButtonOption).IsSelected = false;
//                }

//                _previousRBumberOptionIndex = this.RBumberComboBox.SelectedIndex;

//                Properties.Settings.Default.Save();
//            }
//        }

//        private void StartButtonComboBox_Enter(object sender, EventArgs e)
//        {
//            _previousStartButtonOptionIndex = this.StartButtonComboBox.SelectedIndex;
//        }
//        private void StartButtonComboBox_DrawItem(object sender, DrawItemEventArgs e)
//        {
//            if (e.Index >= 0)
//            {
//                ControllerButtonOption selectedOption = this.StartButtonComboBox.Items[e.Index] as ControllerButtonOption;

//                // Disabling selected item.
//                if (selectedOption.IsSelected && selectedOption.Button != ControllerButton.Start &&
//                    selectedOption.Button != ControllerButton.None)
//                {
//                    e.Graphics.DrawString(selectedOption.FunctionName, _myFont, Brushes.LightGray, e.Bounds);
//                }
//                else
//                {
//                    e.DrawBackground();
//                    e.Graphics.DrawString(selectedOption.FunctionName, _myFont, Brushes.Black, e.Bounds);
//                    e.DrawFocusRectangle();
//                }
//            }
//        }
//        private void StartButtonComboBox_SelectionChangeCommitted(object sender, EventArgs e)
//        {
//            if (this.StartButtonComboBox.SelectedIndex == _previousStartButtonOptionIndex)
//                return;

//            ControllerButtonOption selectedOption = this.StartButtonComboBox.SelectedValue as ControllerButtonOption;

//            if (selectedOption.Button != ControllerButton.Unset && selectedOption.Button != ControllerButton.None)
//            {
//                this.StartButtonComboBox.SelectedIndex = _previousStartButtonOptionIndex;
//            }
//            else
//            {
//                Properties.Settings.Default["NV_StartButton"] = (Function)selectedOption.Function;

//                if (selectedOption.Button != ControllerButton.None)
//                {
//                    selectedOption.IsSelected = true;
//                    selectedOption.Button = ControllerButton.Start;
//                }

//                ControllerButtonOption previousSelectedOption = this.StartButtonComboBox.Items[_previousStartButtonOptionIndex] as ControllerButtonOption;

//                if (previousSelectedOption.Button != ControllerButton.None)
//                {
//                    (this.StartButtonComboBox.Items[_previousStartButtonOptionIndex] as ControllerButtonOption).Button = ControllerButton.Unset;
//                    (this.StartButtonComboBox.Items[_previousStartButtonOptionIndex] as ControllerButtonOption).IsSelected = false;
//                }

//                _previousStartButtonOptionIndex = this.StartButtonComboBox.SelectedIndex;

//                Properties.Settings.Default.Save();
//            }
//        }

//        private void LTriggerComboBox_Enter(object sender, EventArgs e)
//        {
//            _previousLTriggerOptionIndex = this.LTriggerComboBox.SelectedIndex;
//        }
//        private void LTriggerComboBox_DrawItem(object sender, DrawItemEventArgs e)
//        {
//            if (e.Index >= 0)
//            {
//                ControllerTriggerOption selectedOption = LTriggerComboBox.Items[e.Index] as ControllerTriggerOption;

//                // Disabling selected item.
//                if (selectedOption.IsSelected && selectedOption.Trigger != ControllerTrigger.LT &&
//                    selectedOption.Trigger != ControllerTrigger.None)
//                {
//                    e.Graphics.DrawString(selectedOption.FunctionName, _myFont, Brushes.LightGray, e.Bounds);
//                }
//                else
//                {
//                    e.DrawBackground();
//                    e.Graphics.DrawString(selectedOption.FunctionName, _myFont, Brushes.Black, e.Bounds);
//                    e.DrawFocusRectangle();
//                }
//            }
//        }
//        private void LTriggerComboBox_SelectionChangeCommitted(object sender, EventArgs e)
//        {
//            if (this.LTriggerComboBox.SelectedIndex == _previousLTriggerOptionIndex)
//                return;

//            ControllerTriggerOption selectedOption = this.LTriggerComboBox.SelectedValue as ControllerTriggerOption;

//            if (selectedOption.Trigger != ControllerTrigger.Unset && selectedOption.Trigger != ControllerTrigger.None)
//            {
//                this.LTriggerComboBox.SelectedIndex = _previousLTriggerOptionIndex;
//            }
//            else
//            {
//                Properties.Settings.Default["NV_LTrigger"] = (Function)selectedOption.Function;

//                if (selectedOption.Trigger != ControllerTrigger.None)
//                {
//                    selectedOption.IsSelected = true;
//                    selectedOption.Trigger = ControllerTrigger.LT;
//                }

//                ControllerTriggerOption previousSelectedOption = this.LTriggerComboBox.Items[_previousLTriggerOptionIndex] as ControllerTriggerOption;

//                if (previousSelectedOption.Trigger != ControllerTrigger.None)
//                {
//                    (this.LTriggerComboBox.Items[_previousLTriggerOptionIndex] as ControllerTriggerOption).Trigger = ControllerTrigger.Unset;
//                    (this.LTriggerComboBox.Items[_previousLTriggerOptionIndex] as ControllerTriggerOption).IsSelected = false;
//                }

//                _previousLTriggerOptionIndex = this.LTriggerComboBox.SelectedIndex;

//                Properties.Settings.Default.Save();
//            }
//        }

//        private void RTriggerComboBox_Enter(object sender, EventArgs e)
//        {
//            _previousRTriggerOptionIndex = this.RTriggerComboBox.SelectedIndex;
//        }
//        private void RTriggerComboBox_DrawItem(object sender, DrawItemEventArgs e)
//        {
//            if (e.Index >= 0)
//            {
//                ControllerTriggerOption selectedOption = this.RTriggerComboBox.Items[e.Index] as ControllerTriggerOption;

//                // Disabling selected item.
//                if (selectedOption.IsSelected && selectedOption.Trigger != ControllerTrigger.RT &&
//                    selectedOption.Trigger != ControllerTrigger.None)
//                {
//                    e.Graphics.DrawString(selectedOption.FunctionName, _myFont, Brushes.LightGray, e.Bounds);
//                }
//                else
//                {
//                    e.DrawBackground();
//                    e.Graphics.DrawString(selectedOption.FunctionName, _myFont, Brushes.Black, e.Bounds);
//                    e.DrawFocusRectangle();
//                }
//            }
//        }
//        private void RTriggerComboBox_SelectionChangeCommitted(object sender, EventArgs e)
//        {
//            if (this.RTriggerComboBox.SelectedIndex == _previousRTriggerOptionIndex)
//                return;

//            ControllerTriggerOption selectedOption = this.RTriggerComboBox.SelectedValue as ControllerTriggerOption;

//            if (selectedOption.Trigger != ControllerTrigger.Unset && selectedOption.Trigger != ControllerTrigger.None)
//            {
//                this.RTriggerComboBox.SelectedIndex = _previousRTriggerOptionIndex;
//            }
//            else
//            {
//                Properties.Settings.Default["NV_RTrigger"] = (Function)selectedOption.Function;

//                if (selectedOption.Trigger != ControllerTrigger.None)
//                {
//                    selectedOption.IsSelected = true;
//                    selectedOption.Trigger = ControllerTrigger.RT;
//                }

//                ControllerTriggerOption previousSelectedOption = this.RTriggerComboBox.Items[_previousRTriggerOptionIndex] as ControllerTriggerOption;

//                if (previousSelectedOption.Trigger != ControllerTrigger.None)
//                {
//                    (this.RTriggerComboBox.Items[_previousRTriggerOptionIndex] as ControllerTriggerOption).Trigger = ControllerTrigger.Unset;
//                    (this.RTriggerComboBox.Items[_previousRTriggerOptionIndex] as ControllerTriggerOption).IsSelected = false;
//                }

//                _previousRTriggerOptionIndex = this.RTriggerComboBox.SelectedIndex;

//                Properties.Settings.Default.Save();
//            }
//        }

//        private void LStickComboBox_Enter(object sender, EventArgs e)
//        {
//            _previousLStickOptionIndex = this.LStickComboBox.SelectedIndex;
//        }
//        private void LStickComboBox_DrawItem(object sender, DrawItemEventArgs e)
//        {
//            if (e.Index >= 0)
//            {
//                ControllerThumbStickOption selectedOption = this.LStickComboBox.Items[e.Index] as ControllerThumbStickOption;

//                // Disabling selected item.
//                if (selectedOption.IsSelected && selectedOption.ThumbStick != ControllerThumbStick.L &&
//                    selectedOption.ThumbStick != ControllerThumbStick.None)
//                {
//                    e.Graphics.DrawString(selectedOption.FunctionName, _myFont, Brushes.LightGray, e.Bounds);
//                }
//                else
//                {
//                    e.DrawBackground();
//                    e.Graphics.DrawString(selectedOption.FunctionName, _myFont, Brushes.Black, e.Bounds);
//                    e.DrawFocusRectangle();
//                }
//            }
//        }
//        private void LStickComboBox_SelectionChangeCommitted(object sender, EventArgs e)
//        {
//            if (this.LStickComboBox.SelectedIndex == _previousLStickOptionIndex)
//                return;

//            ControllerThumbStickOption selectedOption = this.LStickComboBox.SelectedValue as ControllerThumbStickOption;

//            if (selectedOption.ThumbStick != ControllerThumbStick.Unset && selectedOption.ThumbStick != ControllerThumbStick.None)
//            {
//                this.LStickComboBox.SelectedIndex = _previousLStickOptionIndex;
//            }
//            else
//            {
//                Properties.Settings.Default["NV_LStick"] = (Function)selectedOption.Function;

//                if (selectedOption.ThumbStick != ControllerThumbStick.None)
//                {
//                    selectedOption.IsSelected = true;
//                    selectedOption.ThumbStick = ControllerThumbStick.L;
//                }

//                ControllerThumbStickOption previousSelectedOption = this.LStickComboBox.Items[_previousLStickOptionIndex] as ControllerThumbStickOption;

//                if (previousSelectedOption.ThumbStick != ControllerThumbStick.None)
//                {
//                    (this.LStickComboBox.Items[_previousLStickOptionIndex] as ControllerThumbStickOption).ThumbStick = ControllerThumbStick.Unset;
//                    (this.LStickComboBox.Items[_previousLStickOptionIndex] as ControllerThumbStickOption).IsSelected = false;
//                }

//                _previousLStickOptionIndex = this.LStickComboBox.SelectedIndex;

//                Properties.Settings.Default.Save();
//            }
//        }

//        private void RStickComboBox_Enter(object sender, EventArgs e)
//        {
//            _previousRStickOptionIndex = this.RStickComboBox.SelectedIndex;
//        }
//        private void RStickComboBox_DrawItem(object sender, DrawItemEventArgs e)
//        {
//            if (e.Index >= 0)
//            {
//                ControllerThumbStickOption selectedOption = this.RStickComboBox.Items[e.Index] as ControllerThumbStickOption;

//                // Disabling selected item.
//                if (selectedOption.IsSelected && selectedOption.ThumbStick != ControllerThumbStick.R &&
//                selectedOption.ThumbStick != ControllerThumbStick.None)
//                {
//                    e.Graphics.DrawString(selectedOption.FunctionName, _myFont, Brushes.LightGray, e.Bounds);
//                }
//                else
//                {
//                    e.DrawBackground();
//                    e.Graphics.DrawString(selectedOption.FunctionName, _myFont, Brushes.Black, e.Bounds);
//                    e.DrawFocusRectangle();
//                }
//            }
//        }
//        private void RStickComboBox_SelectionChangeCommitted(object sender, EventArgs e)
//        {
//            if (this.RStickComboBox.SelectedIndex == _previousRStickOptionIndex)
//                return;

//            ControllerThumbStickOption selectedOption = this.RStickComboBox.SelectedValue as ControllerThumbStickOption;

//            if (selectedOption.ThumbStick != ControllerThumbStick.Unset && selectedOption.ThumbStick != ControllerThumbStick.None)
//            {
//                this.RStickComboBox.SelectedIndex = _previousRStickOptionIndex;
//            }
//            else
//            {
//                Properties.Settings.Default["NV_RStick"] = (Function)selectedOption.Function;

//                if (selectedOption.ThumbStick != ControllerThumbStick.None)
//                {
//                    selectedOption.IsSelected = true;
//                    selectedOption.ThumbStick = ControllerThumbStick.R;
//                }

//                ControllerThumbStickOption previousSelectedOption = this.RStickComboBox.Items[_previousRStickOptionIndex] as ControllerThumbStickOption;

//                if (previousSelectedOption.ThumbStick != ControllerThumbStick.None)
//                {
//                    (this.RStickComboBox.Items[_previousRStickOptionIndex] as ControllerThumbStickOption).ThumbStick = ControllerThumbStick.Unset;
//                    (this.RStickComboBox.Items[_previousRStickOptionIndex] as ControllerThumbStickOption).IsSelected = false;
//                }

//                _previousRStickOptionIndex = this.RStickComboBox.SelectedIndex;

//                Properties.Settings.Default.Save();
//            }
//        }

//        private void AdjustWidthComboBox_DropDown(object sender, DrawItemEventArgs e)
//        {
//            ComboBox senderComboBox = (ComboBox)sender;
//            int width = senderComboBox.DropDownWidth;
//            Graphics g = senderComboBox.CreateGraphics();
//            Font font = senderComboBox.Font;
//            int verticalScrollBarWidth =
//                (senderComboBox.Items.Count > senderComboBox.MaxDropDownItems)
//                ? SystemInformation.VerticalScrollBarWidth : 0;

//            int newWidth = 0;
//            foreach (ControllerOption o in ((ComboBox)sender).Items)
//            {
//                newWidth = (int)g.MeasureString(o.FunctionName, font).Width
//                    + verticalScrollBarWidth;
//                if (width < newWidth)
//                {
//                    width = newWidth;
//                }
//            }
//            senderComboBox.DropDownWidth = width;
//        }

//        protected override void OnClosing(CancelEventArgs e)
//        {
//            Properties.Settings.Default.Save();
//        }
//    }
//}
#endregion Working Version