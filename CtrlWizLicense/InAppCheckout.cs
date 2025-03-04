using CtrlWiz.License.Configs;
using CtrlWiz.License.Properties;
using CtrlWiz.Logging;
using PaddleSDK;
using PaddleSDK.Checkout;
using PaddleSDK.Product;
using System;
using System.IO;

namespace CtrlWiz.License
{
    public class InAppCheckout
    {
        private readonly string tempFileName = null;

        private readonly string commandId = null;

        public bool IsProductActivated { get; private set; } = false;

        public string ApiKey => Resources.ApiKey;

        public string VendorId => Resources.VendorId;

        public string VendorName => Resources.VendorName;

        public string ProductId { get; protected set; }

        public string ProductName { get; protected set; }

        public PaddleProduct Product { get; private set; }

        public PaddleProductConfig ProductInfo { get; private set; }

        public ProductType Type { get; private set; }

        //public bool IsAccessible { get; private set; }

        //private static InAppCheckoutStatus status = InAppCheckoutStatus.Default;

        //private static bool isRunning = false;

        //public PaddleProduct GetProduct { get { return product; } private set { } }

        public InAppCheckout(ProductVersion productVersion, string commandId = null)
        {
            this.commandId = commandId;

            switch (productVersion)
            {
                case ProductVersion.NW:
                    ProductId = Resources.ProductIdNW;
                    ProductName = Resources.ProductNameNW;
                    Type = ProductType.SDKProduct;
                    tempFileName = Path.Combine(Path.GetTempPath(), Resources.TempNW);
                    break;
                case ProductVersion.RVT:
                    ProductId = Resources.ProductIdRVT;
                    ProductName = Resources.ProductNameRVT;
                    Type = ProductType.SDKProduct;
                    tempFileName = Path.Combine(Path.Combine(Path.GetTempPath(), ".."), Resources.TempRVT);
                    break;
            }
            ProductInfo = new PaddleProductConfig { ProductName = this.ProductName, VendorName = this.VendorName };

            // Initialize the SDK singleton with the config
            Paddle.Configure(Resources.ApiKey, Resources.VendorId, this.ProductId, this.ProductInfo);

            // Set up events for Checkout
            Paddle.Instance.TransactionCompleteEvent += PaddleCheckoutForm_TransactionCompleteEvent;
            Paddle.Instance.TransactionErrorEvent += PaddleCheckoutForm_TransactionErrorEvent;
            Paddle.Instance.TransactionBeginEvent += PaddleCheckoutForm_TransactionBeginEvent;

            Product = PaddleProduct.CreateProduct(this.ProductId, this.Type, this.ProductInfo);
        }

        public void StartPaddle(out string message)
        {
            message = null;

            try
            {
                //// Initialize the SDK singleton with the config.
                //Paddle.Configure(apiKey, vendorId, productId, productInfo);

                //// Set up events for Checkout.
                //Paddle.Instance.TransactionCompleteEvent += PaddleCheckoutForm_TransactionCompleteEvent;
                //Paddle.Instance.TransactionErrorEvent += PaddleCheckoutForm_TransactionErrorEvent;
                //Paddle.Instance.TransactionBeginEvent += PaddleCheckoutForm_TransactionBeginEvent;

                // Ask the Product to get it's latest state and info from the Paddle Platform.

                Product.Refresh((success) =>
                {
                    try
                    {
                        // Product data was successfully refreshed.
                        if (success)
                        {
                            if (!Product.Activated)
                            {
                                // Product is not activated, so let's show the Product Access dialog to gatekeep app.
                                //Paddle.Instance.ShowProductAccessWindowForProduct(Product);
                                //Paddle.Instance.ShowLicenseRecoveryWindowForProduct(Product);
                                Paddle.Instance.ShowCheckoutWindowForProduct(Product);

                            }
                            else
                            {
                                Product.VerifyActivation((state, errorMessage) =>
                                {
                                    try
                                    {
                                        if (state != VerificationState.Verified)
                                        {
                                            Product.Deactivate((isActive, msg) =>
                                            {
                                                try
                                                {
                                                }
                                                catch (Exception ex)
                                                {
                                                    ex.LogException();
                                                }
                                            });
                                            ActivationChanged?.Invoke(this, new ActivationChangedArgs() { IsActivated = false });
                                        };
                                    }
                                    catch (Exception ex)
                                    {
                                        ex.LogException();
                                    }
                                });
                            }
                        }
                        else
                        {
                            // The SDK was unable to get the last info from the Paddle Platform.
                            // We can show the Product Access dialog with the data provided in the PaddleProductConfig object.
                            //Paddle.Instance.ShowProductAccessWindowForProduct(Product);
                            Paddle.Instance.ShowCheckoutWindowForProduct(Product);
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.LogException();
                    }
                });

                IsProductActivated = Product.Activated;
                ActivationChanged?.Invoke(this, new ActivationChangedArgs() { IsActivated = IsProductActivated });

                // Write a file with license details in case of connection problems on app restart.
                using (FileStream fileStream = new FileStream(tempFileName, FileMode.OpenOrCreate))
                using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                {
                    binaryWriter.Write(Product.Activated);
                }

            }
            catch (Exception ex)
            {
                // Unable to reach Paddle servers. Read the license file for offline check.
                if (File.Exists(tempFileName))
                {
                    using (FileStream fileStream = new FileStream(tempFileName, FileMode.Open))
                    using (BinaryReader binaryReader = new BinaryReader(fileStream))
                    {
                        IsProductActivated = binaryReader.ReadBoolean();
                    }
                }
                if (!IsProductActivated)
                    message = "Unable to verify license.\nPlease check your internet connection.";

                ex.LogException();
            }
        }

        //public async Task<bool> StartPaddle1()
        //{
        //    //this.VerificationCompletEvent += InAppCheckout_VerificationCompletEvent;
        //    //LicenseVerification();
        //    isRunning = true;
        //    while (isRunning)
        //    {
        //        if (status == InAppCheckoutStatus.Default)
        //        {
        //            status = InAppCheckoutStatus.Pending;
        //            LicenseActivation();
        //        }
        //        await Task.Delay(100);
        //    }
        //    status = InAppCheckoutStatus.Default;
        //    return false;
        //}

        //private void InAppCheckout_VerificationCompletEvent(object sender, InAppCheckoutArgs e)
        //{
        //    if (e.State == VerificationState.Unverified)
        //    {
        //        product.Deactivate((isActive, msg) => { });
        //        IsAccessible = false;
        //        //InAppCheckoutCompleteEvent?.Invoke(this, new InAppCheckoutArgs() { IsAccessible = false, State = e.State });
        //    }
        //    IsAccessible = true;
        //    isRunning = false;

        //}

        public void ShowLicenseDetailsWindow(out string message)
        {
            message = null;

            try
            {
                // Deactivate for extra check when connection is resumed
                // or again read from the offline license file.
                if (Product == null && IsProductActivated == true)
                {
                    ActivationChanged?.Invoke(this, new ActivationChangedArgs() { IsActivated = false });
                }

                Product.Refresh((success) =>
                {
                    try
                    {
                        // Product data was successfully refreshed.
                        if (success)
                        {
                            Paddle.Instance.ShowLicenseActivationWindowForProduct(Product);
                            ActivationChanged?.Invoke(this, new ActivationChangedArgs() { IsActivated = false });
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.LogException();
                    }
                });
            }
            catch (Exception ex)
            {
                // The SDK was unable to get the last info from the Paddle Platform.
                message = "Unable to get license details.\nPlease check your internet connection.";

                ex.LogException();
            }
        }

        private void PaddleCheckoutForm_TransactionBeginEvent(object sender, TransactionBeginEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void PaddleCheckoutForm_TransactionErrorEvent(object sender, TransactionErrorEventArgs e)
        {
            // throw new NotImplementedException();
        }

        private void PaddleCheckoutForm_TransactionCompleteEvent(object sender, TransactionCompleteEventArgs e)
        {
            //LicenseVerification();
            //throw new NotImplementedException();
        }

        public void Start()
        {
            // Ask the Product to get it's latest state and info from the Paddle Platform
            Product.Refresh((success) =>
            {
                try
                {
                    // product data was successfully refreshed
                    if (success)
                    {
                        if (Product.Activated)
                        {
                            this.VerifyActivationCompleted += InAppCheckout_VerifyActivationCompleted;
                            VerifyActivation();
                        }
                        else
                        {
                            // Product is not activated, so let's show the Product Access dialog to gatekeep your app
                            //Paddle.Instance.ShowProductAccessWindowForProduct(Product);
                            Paddle.Instance.ShowCheckoutWindowForProduct(Product);
                            this.CheckoutCompleted?.Invoke(this, new CheckoutCompletedArgs() { Allowed = false, CommandId = commandId });
                        }
                    }
                    else
                    {
                        // The SDK was unable to get the last info from the Paddle Platform.
                        // We can show the Product Access dialog with the data provided in the PaddleProductConfig object.
                        //Paddle.Instance.ShowProductAccessWindowForProduct(Product);
                        Paddle.Instance.ShowCheckoutWindowForProduct(Product);
                        this.CheckoutCompleted?.Invoke(this, new CheckoutCompletedArgs() { Allowed = false, CommandId = commandId });
                    }
                }
                catch (Exception ex)
                {
                    ex.LogException();
                }
            });
        }

        public void ShowLicenseDetailsWindow()
        {
            Product.Refresh((success) =>
            {
                try
                {
                    // Product data was successfully refreshed.
                    if (success)
                    {
                        Paddle.Instance.ShowLicenseActivationWindowForProduct(Product);
                        //ActivationChanged?.Invoke(this, new ActivationChangedArgs() { IsActivated = false });
                    }
                }
                catch (Exception ex)
                {
                    // The SDK was unable to get the last info from the Paddle Platform.
                    //message = "Unable to get license details.\nPlease check your internet connection.";

                    ex.LogException();
                }
            });
        }

        private void InAppCheckout_VerifyActivationCompleted(object sender, VerifyActivationCompletedArgs e)
        {
            try
            {
                switch (e.State)
                {
                    case VerificationState.Unverified:
                        IsProductActivated = false;
                        this.CheckoutCompleted?.Invoke(this, new CheckoutCompletedArgs() { Allowed = false, CommandId = commandId });
                        break;
                    case VerificationState.Verified:
                        IsProductActivated = true;
                        this.CheckoutCompleted?.Invoke(this, new CheckoutCompletedArgs() { Allowed = true, CommandId = commandId });
                        break;
                    case VerificationState.UnableToVerify:
                        IsProductActivated = true;
                        this.CheckoutCompleted?.Invoke(this, new CheckoutCompletedArgs() { Allowed = true, CommandId = commandId });
                        break;
                    default:
                        IsProductActivated = false;
                        this.CheckoutCompleted?.Invoke(this, new CheckoutCompletedArgs() { Allowed = false, CommandId = commandId });
                        break;
                }
            }
            catch (Exception ex)
            {
                ex.LogException();
            }
        }

        private void VerifyActivation()
        {
            Product.VerifyActivation((verificationState, errorMessage) =>
            {
                try
                {
                    this.VerifyActivationCompleted?.Invoke(this, new VerifyActivationCompletedArgs() { State = verificationState });
                }
                catch (Exception ex)
                {
                    ex.LogException();
                }
            });
        }

        private event EventHandler<VerifyActivationCompletedArgs> VerifyActivationCompleted;

        public event EventHandler<CheckoutCompletedArgs> CheckoutCompleted;

        public event EventHandler<ActivationChangedArgs> ActivationChanged;

        //public event EventHandler<InAppCheckoutArgs> VerificationCompleteEvent;

        //public event EventHandler<InAppCheckoutArgs> InAppCheckoutCompleteEvent;
    }
}