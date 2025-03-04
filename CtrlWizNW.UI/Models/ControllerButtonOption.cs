namespace CtrlWiz.NW.UI.Models
{
    public class ControllerButtonOption : ControllerOption
    {
        public ControllerButtonOption(Function function, string functionName)
        {
            Function = function;
            FunctionName = functionName;
        }
        public ControllerButtonOption(ControllerButton button, Function function, string functionName)
        {
            Button = button;
            Function = function;
            FunctionName = functionName;
            IsSelected = true;
        }

        public ControllerButton Button { get; set; } = ControllerButton.Unset;
    }
}