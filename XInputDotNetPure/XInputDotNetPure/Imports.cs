using System.Runtime.InteropServices;

namespace XInputDotNetPure
{
    internal static class Imports
    {
        private const string DLLName = "xinput1_4.dll";

        [DllImport(DLLName)]
        public static extern uint XInputGetState(uint dwUserIndex, out GamePadState.RawState pState);

        [DllImport(DLLName)]
        public static extern uint XInputSetState(uint dwUserIndex, ref Vibration pVibration);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Vibration
        {
            public ushort wLeftMotorSpeed;
            public ushort wRightMotorSpeed;
        }
    }
}