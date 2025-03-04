using PaddleSDK.Product;
using System;


namespace CtrlWiz.License
{
    public class ActivationChangedArgs : EventArgs
    {
        public bool IsActivated { get; set; }
        public VerificationState State { get; set; }
    }
}
