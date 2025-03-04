namespace CtrlWiz.NW.UI.Models
{
    public class ControllerTriggerOption : ControllerOption
    {
        public ControllerTriggerOption(Function function, string functionName)
        {
            Function = function;
            FunctionName = functionName;
        }
        public ControllerTriggerOption(ControllerTrigger trigger, Function function, string functionName)
        {
            Trigger = trigger;
            Function = function;
            FunctionName = functionName;
            IsSelected = true;
        }

        public ControllerTrigger Trigger { get; set; } = ControllerTrigger.Unset;
    }
}