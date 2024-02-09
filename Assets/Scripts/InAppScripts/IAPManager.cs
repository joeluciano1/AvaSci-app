using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPManager : MonoBehaviour, IStoreListener
{
    public static string PRODUCT_ID_MONTHLY_SUB = "avascimonthlysub";
    public static string PRODUCT_ID_YEARLY_SUB = "avasciyearlysub";

    private static IStoreController m_StoreController;
    private static IExtensionProvider m_StoreExtensionProvider;
    public TMP_Text MonthlySubText;
    public TMP_Text YearlySubText;
    [ContextMenu("Test")]
    public void Test()
    {
        DateTime dateTimeNow = DateTime.UtcNow;
        DateTime dateTimeThen = DateTime.UtcNow.AddYears(-1);

        TimeSpan difference = dateTimeNow - dateTimeThen;
        Debug.Log($"You have pata nai kitne Month(s) {difference.Days} Days and {difference.TotalHours} Hours Left Before Trial Ends");
        Debug.Log(dateTimeThen.ToString("yyyy-MM-ddTHH:mm:ss.fffffff"));
    }
    public void Start()
    {
        if (m_StoreController == null)
        {
            InitializePurchasing();
        }
    }
    void InitializePurchasing()
    {
        if (IsInitialized())
        {
            return;
        }

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        // Add the consumable products
        builder.AddProduct(PRODUCT_ID_MONTHLY_SUB, ProductType.Subscription);
        builder.AddProduct(PRODUCT_ID_YEARLY_SUB, ProductType.Subscription);

        UnityPurchasing.Initialize(this, builder);
    }
    public void SetTitleAndService()
    {
        MonthlySubText.text = $"<b>Monthly Subscription:</b>\nYou will get subscribed to monthly full access of the app with autorenewable charge of {GetProductPrice(PRODUCT_ID_MONTHLY_SUB)}/Month";
        YearlySubText.text = $"<b>Yearly Subscription:</b>\nYou will get subscribed to yearly full access of the app with autorenewable charge of {GetProductPrice(PRODUCT_ID_YEARLY_SUB)}/Year";
    }
    private bool IsInitialized()
    {
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }
    public void BuyMonthlySubscription()
    {
        BuyProductID(PRODUCT_ID_MONTHLY_SUB);
    }

    public void BuyYearlySubscription()
    {
        BuyProductID(PRODUCT_ID_YEARLY_SUB);
    }
    void BuyProductID(string productId)
    {
        if (IsInitialized())
        {
            Product product = m_StoreController.products.WithID(productId);

            if (product != null && product.availableToPurchase)
            {
                Debug.Log($"Purchasing product asychronously: '{product.definition.id}'");
                m_StoreController.InitiatePurchase(product);
            }
            else
            {
                Debug.Log("BuyProductID: FAIL. Not purchasing product, either it's not found or not available for purchase");
            }
        }
        else
        {
            Debug.Log("BuyProductID FAIL. Not initialized.");
        }
    }
    public bool CheckSubscription(string productID)
    {
        Product product = m_StoreController.products.WithID(productID);
        return IsSubscriptionValid(product);
    }
    public bool IsSubscriptionValid(Product product)
    {
        if (product == null || !product.hasReceipt)
        {
            return false;
        }

        var receipt = product.receipt;
        var jsonReceipt = JObject.Parse(receipt);
        var payload = jsonReceipt["Payload"]; // This might vary based on the platform

        // For Google Play Store
        // var googleReceipt = JObject.Parse(payload["json"].ToString());
        // var expiryDate = googleReceipt["expiryTimeMillis"].ToString();

        // For Apple App Store
        var appleReceipt = JObject.Parse(payload.ToString());
        var expiryDate = appleReceipt["expires_date_ms"].ToString();

        var currentDate = DateTime.UtcNow;
        var subscriptionExpiryDate = UnixTimeStampToDateTime(double.Parse(expiryDate));
        return subscriptionExpiryDate > currentDate;
    }

    private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
    {
        System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        dtDateTime = dtDateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();
        return dtDateTime;
    }
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        if (string.Equals(args.purchasedProduct.definition.id, PRODUCT_ID_MONTHLY_SUB, System.StringComparison.Ordinal))
        {
            Debug.Log($"ProcessPurchase: Monthly subscription successful: '{args.purchasedProduct.definition.id}'");
            // Handle the monthly subscription purchase
            SubscriptionBody subscriptionBody = new SubscriptionBody()
            {
                UserEmail = ReferenceManager.instance.LoginManager.LoginEmail_InputField.text,
                IsMonthlySub = true
            };
            string json = JsonConvert.SerializeObject(subscriptionBody);
            APIHandler.instance.Post("Auth/Subscribe", json, onSuccess: (response) =>
            {
                ResponseWithNoObject generalResponse = JsonConvert.DeserializeObject<ResponseWithNoObject>(response);
                if (generalResponse.isSuccess)
                {
                    ReferenceManager.instance.IAPPAnel.SetActive(false);
                    ReferenceManager.instance.LoginManager.SignUserIn();
                }
                if (generalResponse.isError)
                {
                    string reasons = "";
                    foreach (var item in generalResponse.serviceErrors)
                    {
                        reasons += $"\n {item.code} {item.description}";
                    }
                    ReferenceManager.instance.PopupManager.Show("Subscribe Failed!", $"Reasons are: {reasons}");

                }
            }, onError: (error) =>
            {
                ReferenceManager.instance.PopupManager.Show("Subscribing Failed!", $"Reasons are: {error}");
            });
        }
        else if (string.Equals(args.purchasedProduct.definition.id, PRODUCT_ID_YEARLY_SUB, System.StringComparison.Ordinal))
        {
            Debug.Log($"ProcessPurchase: Yearly subscription successful: '{args.purchasedProduct.definition.id}'");
            SubscriptionBody subscriptionBody = new SubscriptionBody()
            {
                UserEmail = ReferenceManager.instance.LoginManager.LoginEmail_InputField.text,
                IsMonthlySub = false
            };
            string json = JsonConvert.SerializeObject(subscriptionBody);
            APIHandler.instance.Post("Auth/Subscribe", json, onSuccess: (response) =>
            {
                ResponseWithNoObject generalResponse = JsonConvert.DeserializeObject<ResponseWithNoObject>(response);
                if (generalResponse.isSuccess)
                {
                    ReferenceManager.instance.IAPPAnel.SetActive(false);
                    ReferenceManager.instance.LoginManager.SignUserIn();
                }
                if (generalResponse.isError)
                {
                    string reasons = "";
                    foreach (var item in generalResponse.serviceErrors)
                    {
                        reasons += $"\n {item.code} {item.description}";
                    }
                    ReferenceManager.instance.PopupManager.Show("Subscribe Failed!", $"Reasons are: {reasons}");

                }
            }, onError: (error) =>
            {
                ReferenceManager.instance.PopupManager.Show("Subscribing Failed!", $"Reasons are: {error}");
            });
            // Handle the yearly subscription purchase
        }
        else
        {
            Debug.Log($"ProcessPurchase: FAIL. Unrecognized product: '{args.purchasedProduct.definition.id}'");
        }

        return PurchaseProcessingResult.Complete;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log($"OnInitializeFailed InitializationFailureReason:{error}");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.Log($"OnInitializeFailed InitializationFailureReason:{error} {message}");
    }



    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("OnInitialized: PASS");

        m_StoreController = controller;
        m_StoreExtensionProvider = extensions;
    }

    public void OnPurchaseFailed(UnityEngine.Purchasing.Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log($"OnPurchaseFailed: FAIL. Product: '{product.definition.storeSpecificId}', PurchaseFailureReason: {failureReason}");
    }
    public string GetProductPrice(string productId)
    {
        if (m_StoreController != null && m_StoreController.products != null)
        {
            Product product = m_StoreController.products.WithID(productId);
            if (product != null && product.availableToPurchase)
            {
                return product.metadata.localizedPriceString; // This gives you the localized price string
            }
        }
        return "Not Available";
    }
}