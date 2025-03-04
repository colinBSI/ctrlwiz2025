using CtrlWiz.Forms.Views;
using System;
using System.Windows.Forms;

namespace CtrlWiz.Forms
{
    public partial class SpeedSettingForm : Form, ISpeedSettingForm
    {
        private static int linearSpeedFactor;

        public event EventHandler<SettingEventArgs> LinearTrackBarValueChanged;

        public event EventHandler<SettingEventArgs> AngularTrackBarValueChanged;

        public SpeedSettingForm(int linearSetting, int angularSetting, int linearSpeedFactor)
        { 
            InitializeComponent();
            SpeedSettingForm.linearSpeedFactor = linearSpeedFactor;
            this.LinearTrackBar.Value = linearSetting;
            this.AngularTrackBar.Value = angularSetting;
            this.LinearTrackBar.ValueChanged += LinearTrackBar_ValueChanged;
            this.AngularTrackBar.ValueChanged += AngularTrackBar_ValueChanged;
            this.LinearLabel.Text = $"{((float)linearSetting / SpeedSettingForm.linearSpeedFactor).ToString()} m/s";
            this.AngularLabel.Text = $"{angularSetting.ToString()} deg/s";
        }

        private void LinearTrackBar_ValueChanged(object sender, EventArgs e)
        {
            this.LinearLabel.Text = $"{((float)LinearTrackBar.Value / SpeedSettingForm.linearSpeedFactor).ToString()} m/s";
            LinearTrackBarValueChanged?.Invoke(this, new SettingEventArgs() { SettingValue = LinearTrackBar.Value });
        }

        private void AngularTrackBar_ValueChanged(object sender, EventArgs e)
        {
            this.AngularLabel.Text = $"{AngularTrackBar.Value.ToString()} deg/s";
            AngularTrackBarValueChanged?.Invoke(this, new SettingEventArgs() { SettingValue = AngularTrackBar.Value });
        }
    }
}