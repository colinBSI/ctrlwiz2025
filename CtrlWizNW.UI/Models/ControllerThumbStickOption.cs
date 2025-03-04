namespace CtrlWiz.NW.UI.Models
{
    public class ControllerThumbStickOption : ControllerOption
    {
        public ControllerThumbStickOption(Function function, string functionName)
        {
            Function = function;
            FunctionName = functionName;
        }
        public ControllerThumbStickOption(ControllerThumbStick thumbStick, Function function, string functionName)
        {
            ThumbStick = thumbStick;
            Function = function;
            FunctionName = functionName;
            IsSelected = true;
        }

        public ControllerThumbStick ThumbStick { get; set; } = ControllerThumbStick.Unset;
    }
}