using System.Windows.Forms;
using CtrlWiz.Forms.Views;

namespace CtrlWiz.Forms.Presenters
{
    public class HelpPresenterRVT
    {
        IHelpForm helpForm;

        public HelpPresenterRVT()
        {
            this.helpForm = new HelpFormRVT();
        }

        public DialogResult ShowDialog()
        {
            return helpForm.ShowDialog();
        }
    }
}