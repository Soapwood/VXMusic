using System;
using System.Collections.ObjectModel;
using Uniject;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Interfaces;

namespace UnityEngine.Purchasing
{
    class GooglePlayStore: AbstractStore
    {
        IGooglePlayStoreRetrieveProductsService m_RetrieveProductsService;
        IGooglePlayStorePurchaseService m_StorePurchaseService;
        IGoogleFetchPurchases m_FetchPurchases;
        IGooglePlayStoreFinishTransactionService m_FinishTransactionService;
        IGooglePurchaseCallback m_GooglePurchaseCallback;
        IGooglePlayStoreExtensionsInternal m_GooglePlayStoreExtensions;
        IGooglePlayConfigurationInternal m_GooglePlayConfigurationInternal;
        IUtil m_Util;
        bool m_HasInitiallyRetrievedProducts;

        public GooglePlayStore(IGooglePlayStoreRetrieveProductsService retrieveProductsService,
            IGooglePlayStorePurchaseService storePurchaseService,
            IGoogleFetchPurchases fetchPurchases,
            IGooglePlayStoreFinishTransactionService transactionService,
            IGooglePurchaseCallback googlePurchaseCallback,
            IGooglePlayConfigurationInternal googlePlayConfigurationInternal,
            IGooglePlayStoreExtensionsInternal googlePlayStoreExtensions,
            IUtil util)
        {
            m_Util = util;
            m_RetrieveProductsService = retrieveProductsService;
            m_StorePurchaseService = storePurchaseService;
            m_FetchPurchases = fetchPurchases;
            m_FinishTransactionService = transactionService;
            m_GooglePurchaseCallback = googlePurchaseCallback;
            m_GooglePlayConfigurationInternal = googlePlayConfigurationInternal;
            m_GooglePlayStoreExtensions = googlePlayStoreExtensions;
        }

        /// <summary>
        /// Init GooglePlayStore
        /// </summary>
        /// <param name="callback">The `IStoreCallback` will be call when receiving events from the google store</param>
        public override void Initialize(IStoreCallback callback)
        {
            var scriptingStoreCallback = new ScriptingStoreCallback(callback, m_Util);
            m_RetrieveProductsService.SetStoreCallback(scriptingStoreCallback);
            m_FetchPurchases.SetStoreCallback(scriptingStoreCallback);
            m_FinishTransactionService.SetStoreCallback(scriptingStoreCallback);
            m_GooglePurchaseCallback.SetStoreCallback(scriptingStoreCallback);
            m_GooglePlayStoreExtensions.SetStoreCallback(scriptingStoreCallback);

            m_HasInitiallyRetrievedProducts = false;
        }

        /// <summary>
        /// Call the Google Play Store to retrieve the store products. The `IStoreCallback` will be call with the retrieved products.
        /// </summary>
        /// <param name="products">The catalog of products to retrieve the store information from</param>
        public override void RetrieveProducts(ReadOnlyCollection<ProductDefinition> products)
        {
            var shouldFetchPurchases = ShouldFetchPurchasesNext();

            m_RetrieveProductsService.RetrieveProducts(products, shouldFetchPurchases);
        }

        bool ShouldFetchPurchasesNext()
        {
            var shouldFetchPurchases = true;
            
            if (!m_HasInitiallyRetrievedProducts)
            {
                m_HasInitiallyRetrievedProducts = true;

                shouldFetchPurchases = !m_GooglePlayConfigurationInternal.IsFetchPurchasesAtInitializeSkipped();
            }

            return shouldFetchPurchases;
        }

        /// <summary>
        /// Call the Google Play Store to purchase a product. The `IStoreCallback` will be call when the purchase is successful.
        /// </summary>
        /// <param name="product">The product to buy</param>
        /// <param name="dummy">No longer used / required, since fraud prevention is handled by the Google SDK now</param>
        public override void Purchase(ProductDefinition product, string dummy)
        {
            m_StorePurchaseService.Purchase(product);
        }

        /// <summary>
        /// Call the Google Play Store to consume a product.
        /// </summary>
        /// <param name="product">Product to consume</param>
        /// <param name="transactionId">Transaction / order id</param>
        public override void FinishTransaction(ProductDefinition product, string transactionId)
        {
            m_FinishTransactionService.FinishTransaction(product, transactionId);
        }

        public void OnPause(bool isPaused)
        {
            if (!isPaused)
            {
                m_RetrieveProductsService.ResumeConnection();
                m_FetchPurchases.FetchPurchases();
            }
        }
    }
}
