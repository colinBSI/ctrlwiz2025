using System.Windows.Forms;

namespace CtrlWiz.Forms.MessageServices
{
    public class MessageService : IMessageService
    {
        public void ShowMessage(string message)
        {
            MessageBox.Show(message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void ShowExclamation(string message)
        {
            MessageBox.Show(message, "Exclamation", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}