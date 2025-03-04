using System.Windows.Forms;
using CtrlWiz.Forms.Views;

namespace CtrlWiz.Forms.Presenters
{
    public class HelpPresenterNW
    {
        IHelpForm helpForm;

        public HelpPresenterNW()
        {
            this.helpForm = new HelpFormNW();
        }

        public DialogResult ShowDialog()
        {
            return helpForm.ShowDialog();
        }
    }
}