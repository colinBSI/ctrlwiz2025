using System;
using System.Windows.Forms;

namespace CtrlWiz.Forms.Views
{
    public interface ISpeedSettingForm
    {
        event EventHandler<SettingEventArgs> LinearTrackBarValueChanged;

        event EventHandler<SettingEventArgs> AngularTrackBarValueChanged;

        DialogResult ShowDialog();
    }
}