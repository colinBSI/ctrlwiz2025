namespace CtrlWiz.NW.UI.Models
{
    public class ControllerOption
    {
        public Function Function { get; protected set; } = Function.Unset;
        public string FunctionName { get; protected set; }
        public bool IsSelected { get; set; } = false;
    }
}