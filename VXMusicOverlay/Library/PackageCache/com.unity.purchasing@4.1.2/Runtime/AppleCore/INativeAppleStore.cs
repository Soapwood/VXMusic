namespace UnityEngine.Purchasing
{
    internal interface INativeAppleStore : INativeStore
    {
        void SetUnityPurchasingCallback (UnityPurchasingCallback AsyncCallback);
        void RestoreTransactions();
        void RefreshAppReceipt();
        void AddTransactionObserver();
        void SetApplicationUsername(string applicationUsername);
        string appReceipt { get; }
        bool canMakePayments { get; }
        bool simulateAskToBuy { get; set; }
        void FetchStorePromotionOrder();
        void SetStorePromotionOrder(string json);
        void FetchStorePromotionVisibility(string productId);
        void SetStorePromotionVisibility(string productId, string visibility);
        string GetTransactionReceiptForProductId (string productId);
        void InterceptPromotionalPurchases();
        void ContinuePromotionalPurchases();
        void PresentCodeRedemptionSheet();
    }
}
