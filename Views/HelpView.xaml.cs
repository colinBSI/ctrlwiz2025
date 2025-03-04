using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

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

            //this.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), @"../Images/ControllerMap.png")));
        }

        private void LeftLink_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://buildfore.com/ctrlwiz/xboxnavis");
        }
        private void MiddleLink_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://buildfore.com/ctrlwiz?utm_source=Navisworks");
        }
        private void RightLink_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.viatechnik.com/");
        }
    }
}