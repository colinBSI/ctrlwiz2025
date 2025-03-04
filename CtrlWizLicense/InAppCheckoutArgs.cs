using PaddleSDK.Product;
using System;


namespace CtrlWiz.License
{
    public class InAppCheckoutArgs : EventArgs
    {
        public bool IsAccessible { get; set; }
        public VerificationState State { get; set; }
    }
}
