using CtrlWiz.Forms.Views;
using CtrlWiz.Forms.Models;
using CtrlWiz.Forms.MessageServices;
using System.Windows.Forms;

namespace CtrlWiz.Forms.Presenters
{
    public class SpeedSettingPresenter
    {
        private readonly ISpeedSettingForm speedSettingForm;

        private readonly ISpeedSettingModel speedSettingModel;

        private readonly IMessageService messageService;

        public int LinearSetting { get; private set; }

        public int AngularSetting { get; private set; }

        public SpeedSettingPresenter(int linearSetting, int angularSetting, int linearSpeedFactor)
        {
            LinearSetting = linearSetting;
            AngularSetting = angularSetting;
            this.speedSettingForm = new SpeedSettingForm(linearSetting, angularSetting, linearSpeedFactor);
            this.speedSettingModel = new SpeedSettingModel();
            this.messageService = new MessageService();
            this.speedSettingForm.LinearTrackBarValueChanged += SpeedSettingForm_SpeedTrackBarValueChanged;
            this.speedSettingForm.AngularTrackBarValueChanged += SpeedSettingForm_AngularTrackBarValueChanged;
        }

        private SpeedSettingPresenter(ISpeedSettingForm speedSettingForm, ISpeedSettingModel speedSettingModel, IMessageService messageService)
        {
            this.speedSettingForm = speedSettingForm;
            this.speedSettingModel = speedSettingModel;
            this.messageService = messageService;
            this.speedSettingForm.LinearTrackBarValueChanged += SpeedSettingForm_SpeedTrackBarValueChanged;
            this.speedSettingForm.AngularTrackBarValueChanged += SpeedSettingForm_AngularTrackBarValueChanged;
        }

        public DialogResult ShowDialog()
        {
            return speedSettingForm.ShowDialog();
        }

        private void SpeedSettingForm_AngularTrackBarValueChanged(object sender, SettingEventArgs e)
        {
            AngularSetting = e.SettingValue;
        }

        private void SpeedSettingForm_SpeedTrackBarValueChanged(object sender, SettingEventArgs e)
        {
            LinearSetting = e.SettingValue;
        }
    }
}