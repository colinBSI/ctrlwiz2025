using System.Management;

namespace CtrlWiz.NW
{
    /// <summary>
    /// Detects a Google Stadia controller via WMI HID enumeration and shows a one-time
    /// install prompt for the ViGEm Bus Driver if the controller is not yet mapped as XInput.
    /// Call CheckAndNotify() once at plugin activation (CmdViewpoint.ExecuteCommand).
    /// </summary>
    internal static class StadiaDetector
    {
        private const string StadiaVidPid = "VID_18D1&PID_9400";
        private const string ViGEmReleasesUrl = "https://github.com/nefarius/ViGEmBus/releases";

        /// <summary>
        /// Returns true if a device whose DeviceID contains the Stadia USB HID VID/PID
        /// is present in the Win32_PnPEntity WMI class.
        /// WMI query runs in ~10-50 ms — acceptable for activation-time call only.
        /// </summary>
        public static bool IsStadiaPresent()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher(
                    "SELECT DeviceID FROM Win32_PnPEntity WHERE DeviceID LIKE '%" + StadiaVidPid + "%'"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        obj.Dispose();
                        return true;
                    }
                }
            }
            catch { }  // WMI unavailable — silent fail, detection is non-fatal
            return false;
        }

        /// <summary>
        /// Call once at plugin activation. If a Stadia controller is detected as a raw
        /// HID device AND the one-time dialog has not yet been shown, displays a YesNo
        /// MessageBox offering to open the ViGEm download page.
        /// </summary>
        public static void CheckAndNotify()
        {
            if (!IsStadiaPresent())
                return;

            if (CtrlWiz.NW.UI.Properties.Settings.Default.StadiaViGEmDialogShown)
                return;

            var result = System.Windows.Forms.MessageBox.Show(
                "A Google Stadia controller was detected.\n\n" +
                "To use it with CtrlWiz, install the free ViGEm Bus Driver, which re-exposes\n" +
                "the Stadia controller as a standard Xbox controller.\n\n" +
                "Open the ViGEm download page now?",
                "Stadia Controller Detected",
                System.Windows.Forms.MessageBoxButtons.YesNo,
                System.Windows.Forms.MessageBoxIcon.Information);

            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                try { System.Diagnostics.Process.Start(ViGEmReleasesUrl); }
                catch { }  // browser launch failed — non-fatal
            }

            CtrlWiz.NW.UI.Properties.Settings.Default.StadiaViGEmDialogShown = true;
            CtrlWiz.NW.UI.Properties.Settings.Default.Save();
        }
    }
}
