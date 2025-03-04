using System;
using System.Windows;

namespace CtrlWiz.NW.UI.Views
{
    /// <summary>
    /// Interaction logic for HelpView.xaml
    /// </summary>
    public partial class HelpView : Window
    {
        public HelpView()
        {
            InitializeComponent();
        }

        private void LeftLink_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.robinsmorton.com");
        }
        private void MiddleLink_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.buildfore.com/products/ctrlwiz/ctrlwiz-xbox-controller-for-navisworks");
        }
        private void RightLink_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.viatechnik.com");
        }
    }
}