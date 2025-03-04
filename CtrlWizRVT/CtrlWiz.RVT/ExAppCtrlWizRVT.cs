using Autodesk.Revit.UI;
using CtrlWiz.Forms.MessageServices;
using CtrlWiz.License;
using CtrlWiz.License.Configs;
using CtrlWiz.Logging;
using CtrlWiz.Logging.Utility;
using CtrlWiz.RVT.Properties;
using System;
using System.Reflection;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace CtrlWiz.RVT
{
    public class ExAppCtrlWizRVT : IExternalApplication
    {
        private static bool isProductActivated = false;

        public static InAppCheckout InAppCheckout { get; private set; }

        public static MessageService MessageService => new MessageService();

        public static bool IsLicenseActive()
        {
            try
            {
                if (!isProductActivated)
                {
                    InAppCheckout.StartPaddle(out string msg);
                    if (msg != null) MessageService.ShowError(msg);

                    isProductActivated = InAppCheckout.IsProductActivated;
                }
                return isProductActivated;
            }
            catch (Exception e)
            {
                TaskDialog.Show("Error", e.Message);
                return false;
            }
            //return true;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            //Thread thread = new Thread(() =>
            //{
            //Create license Product.
            //InAppCheckout = new InAppCheckout(ProductVersion.RVT);
            //InAppCheckout.ActivationChanged += InAppCheckout_ActivationChanged;
            //Dispatcher.Run();
            //});
            //thread.SetApartmentState(ApartmentState.STA);
            //thread.IsBackground = true;
            //thread.Start();

            Logger.SetUpLogger(LoggedApp.RVT);

            try
            {
                InAppCheckout = new InAppCheckout(ProductVersion.RVT);
                InAppCheckout.ActivationChanged += InAppCheckout_ActivationChanged;

                // Create tab and panel.
                application.CreateRibbonTab(Resources.TabName);
                RibbonPanel ribbonPanel = application.CreateRibbonPanel(Resources.TabName, Resources.RibbonPanelName);

                string assemblyPath = Assembly.GetExecutingAssembly().Location;

                // Add Activate Controller button.
                PushButtonData activateControllerPushButtonData = new PushButtonData(Resources.ActivateControllerButtonName,
                    Resources.ActivateControllerButtonText.Replace(" ", "\r\n"), assemblyPath, typeof(CmdActivateController).FullName);
                PushButton activateControllerPushButton = ribbonPanel.AddItem(activateControllerPushButtonData) as PushButton;
                activateControllerPushButton.LargeImage = GetImage(Resources.ActivateController.ToBitmap().GetHbitmap());
                activateControllerPushButton.ToolTip = Resources.ActivateControllerButtonToolTip;

                // Add Help button.
                PushButtonData helpPushButtonData = new PushButtonData(Resources.HelpButtonName, 
                    Resources.HelpButtonText.Replace(" ", "\r\n"), assemblyPath, typeof(CmdHelp).FullName);
                PushButton helpPushButton = ribbonPanel.AddItem(helpPushButtonData) as PushButton;
                helpPushButton.LargeImage = GetImage(Resources.Help28.GetHbitmap());
                helpPushButton.ToolTip = Resources.HelpButtonToolTip;

                // Add Feature Request button.
                PushButtonData featureRequestPushButtonData = new PushButtonData(Resources.FeatureRequestButtonName, 
                    Resources.FeatureRequestButtonText.Replace(" ", "\r\n"), assemblyPath, typeof(CmdFeatureRequest).FullName);
                PushButton featureRequestPushButton = ribbonPanel.AddItem(featureRequestPushButtonData) as PushButton;
                featureRequestPushButton.LargeImage = GetImage(Resources.Idea.GetHbitmap());
                featureRequestPushButton.ToolTip = Resources.FeatureRequestButtonToolTip;

                // Add Speed Settings button.
                PushButtonData speedSettingsPushButtonData = new PushButtonData(Resources.SpeedSettingsButtonName, 
                    Resources.SpeedSettingsButtonText.Replace(" ", "\r\n"), assemblyPath, typeof(CmdSettings).FullName);
                PushButton speedSettingsPushButton = ribbonPanel.AddItem(speedSettingsPushButtonData) as PushButton;
                speedSettingsPushButton.LargeImage = GetImage(Resources.Settings.ToBitmap().GetHbitmap());
                speedSettingsPushButton.ToolTip = Resources.SpeedSettingsButtonToolTip;

                // Add License button.
                PushButtonData licensePushButtonData = new PushButtonData(Resources.LicenseButtonName, 
                    Resources.LicenseButtonText.Replace(" ", "\r\n"), assemblyPath, typeof(CmdLicense).FullName);
                PushButton licensePushButton = ribbonPanel.AddItem(licensePushButtonData) as PushButton;
                licensePushButton.LargeImage = GetImage(Resources.License.ToBitmap().GetHbitmap());
                licensePushButton.ToolTip = Resources.LicenseButtonToolTip;


                ContextualHelp contextHelp = new ContextualHelp(ContextualHelpType.Url, "https://help.buildfore.com/ctrlwiz/ctrlwiz-for-revit/ctrlwiz-xbox-controller-for-revit");

                activateControllerPushButton.SetContextualHelp(contextHelp);
                helpPushButton.SetContextualHelp(contextHelp);
                featureRequestPushButton.SetContextualHelp(contextHelp);
                speedSettingsPushButton.SetContextualHelp(contextHelp);
                licensePushButton.SetContextualHelp(contextHelp);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                TaskDialog.Show("Error", e.Message);

                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            try
            {
                InAppCheckout.ActivationChanged -= InAppCheckout_ActivationChanged;
            }
            catch (Exception e)
            {
                TaskDialog.Show("Error", e.Message);
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        private void InAppCheckout_ActivationChanged(object sender, ActivationChangedArgs e)
        {
            isProductActivated = e.IsActivated;
        }

        private BitmapSource GetImage(IntPtr intPtr)
        {
            return Imaging.CreateBitmapSourceFromHBitmap(intPtr, IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }
    }
}