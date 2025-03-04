using System;
using System.Drawing;
using System.Media;
using System.Windows.Forms;


namespace CtrlWiz.RVT.Dialogs
{
    public partial class PerspectiveViewNotActiveDialogBox : Form
    {
        public PerspectiveViewNotActiveDialogBox()
        {
            InitializeComponent();
        }

        public new static void ShowDialog()
        {
            Form dialog = new PerspectiveViewNotActiveDialogBox();
            dialog.ShowDialog();
        }

        private void helpLinkLbl1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            e.Link.Visited = true;
            System.Diagnostics.Process.Start("https://help.ctrlwiz.com/ctrlwiz-for-revit/perspective-view");
        }

        private void DialogBoxWithLink_Load(object sender, EventArgs e)
        {
            this.exclamationPicBox1.Image = SystemIcons.Exclamation.ToBitmap();
            SystemSounds.Exclamation.Play();
        }
    }
}
