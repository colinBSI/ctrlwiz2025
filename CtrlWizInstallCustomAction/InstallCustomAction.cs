using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration.Install;
using System.Windows.Forms;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;

using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace CtrlWizInstallCustomAction
{
    [RunInstaller(true)]

    public class InstallCustomAction : Installer
    {
        IDictionary mySavedState = new Hashtable();

        protected override void OnBeforeInstall(IDictionary savedState)
        {
            if (!LaunchOnBeforeInstall())
            {
                //base.Rollback(mySavedState);
                throw new InstallException("Installation Cancelled!");
            }
        }

        private bool LaunchOnBeforeInstall()
        {
            string message = "Installation requires that ADMINISTRATIVE privileges are granted to the USER installing the software.  Licensing features on this software will not work if the USER does not have ADMINISTRATIVE privileges during the entire installation process.";

            if (MessageBox.Show(message, "Message", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.Cancel)
            {
                return false;
            }
            else
            {
                return true;
            }

        }



        //protected override void OnAfterInstall(IDictionary savedState)
        //{
        //    base.OnAfterInstall(savedState);
        //    //Test("OnAfterInstall");
        //}

        //protected override void OnCommitted(IDictionary savedState)
        //{
        //    base.OnCommitted(savedState);
        //    //Test("OnCommitted");
        //}

        //protected override void OnCommitting(IDictionary savedState)
        //{
        //    base.OnCommitting(savedState);
        //    //Test("OnCommitting");
        //}

        //private void Test(string state)
        //{
        //    StringBuilder sb = new StringBuilder();

        //    Process process = Process.GetCurrentProcess();

        //    //sb.AppendLine($"ProcessName - {process.ProcessName}");
        //    //sb.AppendLine($"IsAdministrator - {UACHelper.UACHelper.IsAdministrator}");
        //    //sb.AppendLine($"IsDesktopOwner - {UACHelper.UACHelper.IsDesktopOwner}");
        //    //sb.AppendLine($"IsElevated - {UACHelper.UACHelper.IsElevated}");
        //    //sb.AppendLine($"IsUACEnable - {UACHelper.UACHelper.IsUACEnable}");
        //    //sb.AppendLine($"IsUACSupported - {UACHelper.UACHelper.IsUACSupported}");
        //    //sb.AppendLine($"IsVirtualized - {UACHelper.UACHelper.IsVirtualized}");
        //    //sb.AppendLine($"IsProcessElevated - {UACHelper.UACHelper.IsProcessElevated(process)}");

        //    using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
        //    {
        //        sb.AppendLine($"Owner - {identity.Owner.Value}");
        //        sb.AppendLine($"User - {identity.User.Value}");
        //    }

        //    MessageBox.Show(sb.ToString(), state);
        //}

        //private bool IsAdministrator()
        //{
        //    using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
        //    {
        //        WindowsPrincipal principal = new WindowsPrincipal(identity);
        //        return principal.IsInRole(WindowsBuiltInRole.Administrator);
        //    }
        //}

        //private bool IsPowerUser()
        //{
        //    using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
        //    {
        //        WindowsPrincipal principal = new WindowsPrincipal(identity);
        //        return principal.IsInRole(WindowsBuiltInRole.PowerUser);
        //    }
        //}

        //private bool IsUser()
        //{
        //    using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
        //    {
        //        WindowsPrincipal principal = new WindowsPrincipal(identity);
        //        return principal.IsInRole(WindowsBuiltInRole.User);
        //    }
        //}

        //private bool IsGuest()
        //{
        //    using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
        //    {
        //        WindowsPrincipal principal = new WindowsPrincipal(identity);
        //        return principal.IsInRole(WindowsBuiltInRole.Guest);
        //    }
        //}

        //private bool IsReplicator()
        //{
        //    using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
        //    {
        //        WindowsPrincipal principal = new WindowsPrincipal(identity);
        //        return principal.IsInRole(WindowsBuiltInRole.Replicator);
        //    }
        //}

        //private bool IsAccountOperator()
        //{
        //    using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
        //    {
        //        WindowsPrincipal principal = new WindowsPrincipal(identity);
        //        return principal.IsInRole(WindowsBuiltInRole.AccountOperator);
        //    }
        //}

        ////private bool IsAdmin()
        ////{
        ////    SecurityIdentifier sidAdmin = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
        ////    using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
        ////    {
        ////        WindowsPrincipal principal = new WindowsPrincipal(identity);
        ////        return principal.IsInRole(sidAdmin);
        ////    }
        ////}

        //private StringBuilder Roles()
        //{

        //    StringBuilder sb = new StringBuilder();
        //    using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
        //    {
        //        WindowsPrincipal principal = new WindowsPrincipal(identity);
        //        SecurityIdentifier domainSid = identity.User.AccountDomainSid;
        //        //SecurityIdentifier BuiltinDomainSid = new SecurityIdentifier(WellKnownSidType.BuiltinDomainSid, identity.Owner);
        //        SecurityIdentifier BuiltinAdministratorsSid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, domainSid);
        //        //SecurityIdentifier BuiltinUsersSid = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, identity.Owner);
        //        //SecurityIdentifier BuiltinGuestsSid = new SecurityIdentifier(WellKnownSidType.BuiltinGuestsSid, identity.Owner);
        //        //SecurityIdentifier BuiltinPowerUsersSid = new SecurityIdentifier(WellKnownSidType.BuiltinPowerUsersSid, identity.Owner);
        //        //SecurityIdentifier BuiltinAccountOperatorsSid = new SecurityIdentifier(WellKnownSidType.BuiltinAccountOperatorsSid, identity.Owner);
        //        //SecurityIdentifier BuiltinSystemOperatorsSid = new SecurityIdentifier(WellKnownSidType.BuiltinSystemOperatorsSid, identity.Owner);
        //        //SecurityIdentifier BuiltinPrintOperatorsSid = new SecurityIdentifier(WellKnownSidType.BuiltinPrintOperatorsSid, identity.Owner);
        //        //SecurityIdentifier BuiltinBackupOperatorsSid = new SecurityIdentifier(WellKnownSidType.BuiltinBackupOperatorsSid, identity.Owner);
        //        //SecurityIdentifier BuiltinReplicatorSid = new SecurityIdentifier(WellKnownSidType.BuiltinReplicatorSid, identity.Owner);
        //        SecurityIdentifier AccountAdministratorSid = new SecurityIdentifier(WellKnownSidType.AccountAdministratorSid, domainSid);
        //        //SecurityIdentifier AccountGuestSid = new SecurityIdentifier(WellKnownSidType.AccountGuestSid, identity.Owner);
        //        //SecurityIdentifier AccountKrbtgtSid = new SecurityIdentifier(WellKnownSidType.AccountKrbtgtSid, identity.Owner);
        //        //SecurityIdentifier AccountDomainAdminsSid = new SecurityIdentifier(WellKnownSidType.AccountDomainAdminsSid, identity.Owner);
        //        //SecurityIdentifier AccountDomainUsersSid = new SecurityIdentifier(WellKnownSidType.AccountDomainUsersSid, identity.Owner);
        //        //SecurityIdentifier AccountDomainGuestsSid = new SecurityIdentifier(WellKnownSidType.AccountDomainGuestsSid, identity.Owner);
        //        //SecurityIdentifier AccountComputersSid = new SecurityIdentifier(WellKnownSidType.AccountComputersSid, identity.Owner);
        //        //SecurityIdentifier AccountControllersSid = new SecurityIdentifier(WellKnownSidType.AccountControllersSid, identity.Owner);
        //        //SecurityIdentifier AccountCertAdminsSid = new SecurityIdentifier(WellKnownSidType.AccountCertAdminsSid, identity.Owner);
        //        //SecurityIdentifier AccountSchemaAdminsSid = new SecurityIdentifier(WellKnownSidType.AccountSchemaAdminsSid, identity.Owner);
        //        //SecurityIdentifier AccountPolicyAdminsSid = new SecurityIdentifier(WellKnownSidType.AccountPolicyAdminsSid, domainSid);
        //        //SecurityIdentifier AccountRasAndIasServersSid = new SecurityIdentifier(WellKnownSidType.AccountRasAndIasServersSid, identity.Owner);

        //        //sb.AppendLine($"BuiltinDomainSid - {principal.IsInRole(BuiltinDomainSid)}");
        //        //sb.AppendLine($"BuiltinAdministratorsSid - {principal.IsInRole(BuiltinAdministratorsSid)}");
        //        //sb.AppendLine($"BuiltinUsersSid - {principal.IsInRole(BuiltinUsersSid)}");
        //        //sb.AppendLine($"BuiltinGuestsSid - {principal.IsInRole(BuiltinGuestsSid)}");
        //        //sb.AppendLine($"BuiltinPowerUsersSid - {principal.IsInRole(BuiltinPowerUsersSid)}");
        //        //sb.AppendLine($"BuiltinAccountOperatorsSid - {principal.IsInRole(BuiltinAccountOperatorsSid)}");
        //        //sb.AppendLine($"BuiltinBackupOperatorsSid - {principal.IsInRole(BuiltinBackupOperatorsSid)}");
        //        //sb.AppendLine($"BuiltinReplicatorSid - {principal.IsInRole(BuiltinReplicatorSid)}");
        //        //sb.AppendLine($"AccountAdministratorSid - {principal.IsInRole(AccountAdministratorSid)}");
        //        //sb.AppendLine($"AccountGuestSid - {principal.IsInRole(AccountGuestSid)}");
        //        //sb.AppendLine($"AccountKrbtgtSid - {principal.IsInRole(AccountKrbtgtSid)}");
        //        //sb.AppendLine($"AccountDomainAdminsSid - {principal.IsInRole(AccountDomainAdminsSid)}");
        //        //sb.AppendLine($"AccountDomainUsersSid - {principal.IsInRole(AccountDomainUsersSid)}");
        //        //sb.AppendLine($"AccountDomainGuestsSid - {principal.IsInRole(AccountDomainGuestsSid)}");
        //        //sb.AppendLine($"AccountComputersSid - {principal.IsInRole(AccountComputersSid)}");
        //        //sb.AppendLine($"AccountControllersSid - {principal.IsInRole(AccountControllersSid)}");
        //        //sb.AppendLine($"AccountCertAdminsSid - {principal.IsInRole(AccountCertAdminsSid)}");
        //        //sb.AppendLine($"AccountSchemaAdminsSid - {principal.IsInRole(AccountSchemaAdminsSid)}");
        //        //sb.AppendLine($"AccountPolicyAdminsSid - {principal.IsInRole(AccountPolicyAdminsSid)}");
        //        //sb.AppendLine($"AccountRasAndIasServersSid - {principal.IsInRole(AccountRasAndIasServersSid)}");
        //        sb.AppendLine($"IsWellKnown - {AccountAdministratorSid.IsWellKnown(WellKnownSidType.AccountAdministratorSid)}");
        //        sb.AppendLine($"IsAccountSid - {AccountAdministratorSid.IsAccountSid()}");

        //    }
        //    return sb;
        //}


        ////

        //private const string uacRegistryKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System";
        //private const string uacRegistryValue = "EnableLUA";

        //private static uint STANDARD_RIGHTS_READ = 0x00020000;
        //private static uint TOKEN_QUERY = 0x0008;
        //private static uint TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);

        //[DllImport("advapi32.dll", SetLastError = true)]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //static extern bool OpenProcessToken(IntPtr ProcessHandle, UInt32 DesiredAccess, out IntPtr TokenHandle);

        //[DllImport("advapi32.dll", SetLastError = true)]
        //public static extern bool GetTokenInformation(IntPtr TokenHandle, TOKEN_INFORMATION_CLASS TokenInformationClass, IntPtr TokenInformation, uint TokenInformationLength, out uint ReturnLength);

        //public enum TOKEN_INFORMATION_CLASS
        //{
        //    TokenUser = 1,
        //    TokenGroups,
        //    TokenPrivileges,
        //    TokenOwner,
        //    TokenPrimaryGroup,
        //    TokenDefaultDacl,
        //    TokenSource,
        //    TokenType,
        //    TokenImpersonationLevel,
        //    TokenStatistics,
        //    TokenRestrictedSids,
        //    TokenSessionId,
        //    TokenGroupsAndPrivileges,
        //    TokenSessionReference,
        //    TokenSandBoxInert,
        //    TokenAuditPolicy,
        //    TokenOrigin,
        //    TokenElevationType,
        //    TokenLinkedToken,
        //    TokenElevation,
        //    TokenHasRestrictions,
        //    TokenAccessInformation,
        //    TokenVirtualizationAllowed,
        //    TokenVirtualizationEnabled,
        //    TokenIntegrityLevel,
        //    TokenUIAccess,
        //    TokenMandatoryPolicy,
        //    TokenLogonSid,
        //    MaxTokenInfoClass
        //}

        //public enum TOKEN_ELEVATION_TYPE
        //{
        //    TokenElevationTypeDefault = 1,
        //    TokenElevationTypeFull,
        //    TokenElevationTypeLimited
        //}

        //public static bool IsUacEnabled
        //{
        //    get
        //    {
        //        RegistryKey uacKey = Registry.LocalMachine.OpenSubKey(uacRegistryKey, false);
        //        bool result = uacKey.GetValue(uacRegistryValue).Equals(1);
        //        return result;
        //    }
        //}

        //public static bool IsProcessElevated
        //{
        //    get
        //    {
        //        if (IsUacEnabled)
        //        {
        //            IntPtr tokenHandle;
        //            if (!OpenProcessToken(Process.GetCurrentProcess().Handle, TOKEN_READ, out tokenHandle))
        //            {
        //                throw new ApplicationException("Could not get process token.  Win32 Error Code: " + Marshal.GetLastWin32Error());
        //            }

        //            TOKEN_ELEVATION_TYPE elevationResult = TOKEN_ELEVATION_TYPE.TokenElevationTypeDefault;

        //            int elevationResultSize = Marshal.SizeOf((int)elevationResult);
        //            uint returnedSize = 0;
        //            IntPtr elevationTypePtr = Marshal.AllocHGlobal(elevationResultSize);

        //            bool success = GetTokenInformation(tokenHandle, TOKEN_INFORMATION_CLASS.TokenElevationType, elevationTypePtr, (uint)elevationResultSize, out returnedSize);
        //            if (success)
        //            {
        //                elevationResult = (TOKEN_ELEVATION_TYPE)Marshal.ReadInt32(elevationTypePtr);
        //                bool isProcessAdmin = elevationResult == TOKEN_ELEVATION_TYPE.TokenElevationTypeFull;
        //                return isProcessAdmin;
        //            }
        //            else
        //            {
        //                throw new ApplicationException("Unable to determine the current elevation.");
        //            }
        //        }
        //        else
        //        {
        //            WindowsIdentity identity = WindowsIdentity.GetCurrent();
        //            WindowsPrincipal principal = new WindowsPrincipal(identity);
        //            bool result = principal.IsInRole(WindowsBuiltInRole.Administrator);
        //            return result;
        //        }
        //    }
        //}

        //static bool IsElevated
        //{
        //    get
        //    {
        //        var id = WindowsIdentity.GetCurrent();
        //        return id.Owner != id.User;
        //    }
        //}

        //private bool IsAdmin
        //{
        //    get
        //    {
        //        WindowsIdentity identity = WindowsIdentity.GetCurrent();
        //        if (identity != null)
        //        {
        //            WindowsPrincipal principal = new WindowsPrincipal(identity);
        //            List<Claim> list = new List<Claim>(principal.UserClaims);
        //            Claim c = list.Find(p => p.Value.Contains("S-1-5-32-544"));
        //            if (c != null)
        //                return true;
        //        }
        //        return false;
        //    }
        //}
    }
}