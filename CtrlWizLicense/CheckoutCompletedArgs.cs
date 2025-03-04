using System;

namespace CtrlWiz.License
{
    public class CheckoutCompletedArgs : EventArgs
    {
        public string CommandId { get; set; }
        public bool Allowed { get; set; }
    }
}