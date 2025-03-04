using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace FullPermissionTest
{
    class Program
    {
        private static string ctrlWizLicense = "CtrlWizLicense";
        private static string system = "System";
        private static string roamer = "Roamer";
        private static string programDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        private static string testDirectory = @"C:\Test";

        static void Main(string[] args)
        {
            var isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
            //CreateDirectory
            GrantAccess(Path.Combine(programDataDirectory, ctrlWizLicense));
            GrantAccess(Path.Combine(programDataDirectory, system));

            //ProcessDirectory(ctrlWizLicense);
            //ProcessDirectory(system);
        }

        private static void ProcessDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                CreateDirectory(path);
            }
            GrantAccess(path);
        }

        //private static void GrantAccess(string path)
        //{
        //    DirectoryInfo dInfo = new DirectoryInfo(path);
        //    DirectorySecurity dSecurity = dInfo.GetAccessControl(AccessControlSections.Owner);
        //    dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
        //    SecurityIdentifier sid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
        //    dSecurity.SetOwner(sid);
        //    dInfo.SetAccessControl(dSecurity);
        //}

        private static DirectoryInfo CreateDirectory(string path)
        {
            return Directory.CreateDirectory(path);
        }

        private static bool GrantAccess(string path)
        {
            FileSystemRights Rights = (FileSystemRights)0;
            Rights = FileSystemRights.FullControl;

            // *** Add Access Rule to the actual directory itself
            FileSystemAccessRule AccessRule = new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null), Rights,
                                        InheritanceFlags.None,
                                        PropagationFlags.NoPropagateInherit,
                                        AccessControlType.Allow);

            DirectoryInfo Info = new DirectoryInfo(path);
            DirectorySecurity Security = Info.GetAccessControl(AccessControlSections.Access);

            bool Result = false;
            Security.ModifyAccessRule(AccessControlModification.Set, AccessRule, out Result);

            if (!Result)
                return false;

            // *** Always allow objects to inherit on a directory
            InheritanceFlags iFlags = InheritanceFlags.ObjectInherit;
            iFlags = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;

            // *** Add Access rule for the inheritance
            AccessRule = new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null), Rights,
                                        iFlags,
                                        PropagationFlags.InheritOnly,
                                        AccessControlType.Allow);
            Result = false;
            Security.ModifyAccessRule(AccessControlModification.Add, AccessRule, out Result);

            if (!Result)
                return false;

            Info.SetAccessControl(Security);

            return true;
        }

        private static void GrantAccess1(string path)
        {
            var info = new DirectoryInfo(path);
            var control = info.GetAccessControl();

            control.SetAccessRule(new FileSystemAccessRule(
                    new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null),
                    FileSystemRights.FullControl,
                    InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow));

            info.SetAccessControl(control);
        }
    }
}