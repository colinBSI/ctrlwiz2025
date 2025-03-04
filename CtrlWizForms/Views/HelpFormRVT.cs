using System;
using System.Drawing;
using System.Windows.Forms;

namespace CtrlWiz.Forms.Views
{
    public partial class HelpFormRVT : Form, IHelpForm
    {
        public HelpFormRVT()
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
            this.BRight.Parent = this.pictureBox1;
            this.BRight.BackColor = Color.Transparent;
            this.BRight.FlatAppearance.MouseDownBackColor = Color.Transparent;
            this.BRight.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, Color.Green);
            #endregion

            #region Button Right
            this.BMid.Parent = this.pictureBox1;
            this.BMid.BackColor = Color.Transparent;
            this.BMid.FlatAppearance.MouseDownBackColor = Color.Transparent;
            this.BMid.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, Color.Green);
            #endregion
        }

        private void BLeft_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.robinsmorton.com");
        }

        private void BMid_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.buildfore.com/products/ctrlwiz/ctrlwiz-xbox-controller-for-revit");
        }

        private void BRight_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.viatechnik.com");
        }
    }
}