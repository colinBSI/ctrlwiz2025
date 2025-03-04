namespace CtrlWiz.RVT.States
{
    internal static class SpeedSettings
    {
        internal static int LinearSpeedFactor => 10;
        internal static int LinearSpeed { get; set; } = 2 * LinearSpeedFactor;
        internal static int AngularSpeed { get; set; } = 45;
    }
}