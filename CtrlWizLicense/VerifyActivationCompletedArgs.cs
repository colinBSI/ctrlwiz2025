using System;
using PaddleSDK.Product;

namespace CtrlWiz.License
{
    internal class VerifyActivationCompletedArgs : EventArgs
    {
        internal VerificationState State { get; set; }
    }
}
