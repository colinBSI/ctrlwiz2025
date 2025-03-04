using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows;

namespace CtrlWizDirectoryPermissionCustomAction
{
    [RunInstaller(true)]

    public class DirectoryPermissionCustomAction : Installer
    {
        private readonly string ctrlWizLicense = "CtrlWizLicense";
        private readonly string system = "System";
        private readonly string roamer = "Roamer";

        protected override void OnBeforeInstall(IDictionary savedState)
        {
            try
            {
                string programDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                ProcessDirectory(Path.Combine(programDataDirectory, ctrlWizLicense));
                ProcessDirectory(Path.Combine(programDataDirectory, system));
                ProcessDirectory(Path.Combine(programDataDirectory, roamer));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private bool ProcessDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                CreateDirectory(path);
            }
            return GrantAccess(path);
        }

        private DirectoryInfo CreateDirectory(string path)
        {
            return Directory.CreateDirectory(path);
        }

        private bool GrantAccess(string path)
        {
            FileSystemRights Rights = FileSystemRights.FullControl;

            // *** Add Access Rule to the actual directory itself
            FileSystemAccessRule AccessRule = new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null), Rights,
                                        InheritanceFlags.None,
                                        PropagationFlags.NoPropagateInherit,
                                        AccessControlType.Allow);

            DirectoryInfo Info = new DirectoryInfo(path);
            DirectorySecurity Security = Info.GetAccessControl(AccessControlSections.Access);

            Security.ModifyAccessRule(AccessControlModification.Set, AccessRule, out bool Result);

            if (!Result)
                return false;
            // *** Always allow objects to inherit on a directory
            InheritanceFlags iFlags = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;

            // *** Add Access rule for the inheritance
            AccessRule = new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null), Rights,
                                        iFlags,
                                        PropagationFlags.InheritOnly,
                                        AccessControlType.Allow);
            Security.ModifyAccessRule(AccessControlModification.Add, AccessRule, out Result);

            if (!Result)
                return false;

            Info.SetAccessControl(Security);

            return true;
        }
    }
}
