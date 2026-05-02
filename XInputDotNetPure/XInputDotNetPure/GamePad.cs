namespace XInputDotNetPure
{
    public class GamePad
    {
        public static GamePadState GetState(PlayerIndex playerIndex)
        {
            return GetState(playerIndex, GamePadDeadZone.IndependentAxes);
        }

        public static GamePadState GetState(PlayerIndex playerIndex, GamePadDeadZone deadZone)
        {
            uint result = Imports.XInputGetState((uint)playerIndex, out GamePadState.RawState state);
            return new GamePadState(result == Utils.Success, state, deadZone);
        }

        public static void SetVibration(PlayerIndex playerIndex, float leftMotor, float rightMotor)
        {
            var vibration = new Imports.Vibration
            {
                wLeftMotorSpeed = (ushort)(leftMotor * 65535f),
                wRightMotorSpeed = (ushort)(rightMotor * 65535f)
            };
            Imports.XInputSetState((uint)playerIndex, ref vibration);
        }
    }
}