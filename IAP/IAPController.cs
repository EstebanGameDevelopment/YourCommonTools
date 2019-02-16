using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
#if ENABLE_IAP
using UnityEngine.Purchasing;
#endif
using YourCommonTools;

namespace YourCommonTools
{

	/******************************************
	 * 
	 * IAPController
	 * 
	 * It manages all the IAP with the different systems of payment.
	 * 
	 * @author Esteban Gallardo
	 */
	public class IAPController : MonoBehaviour
#if ENABLE_IAP
		, IStoreListener
#endif
	{
        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------
        public const string EVENT_IAP_INITIALIZED = "EVENT_IAP_INITIALIZED";
        public const string EVENT_IAP_CONFIRMATION = "EVENT_IAP_CONFIRMATION";

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------
		private static IAPController _instance;

		public static IAPController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType<IAPController>();
                    if (!_instance)
                    {
                        GameObject container = new GameObject();
                        DontDestroyOnLoad(container);
                        container.name = "IAPController";
                        _instance = container.AddComponent(typeof(IAPController)) as IAPController;
                    }
                }
                return _instance;
			}
		}

		// ----------------------------------------------
		// MEMBERS
		// ----------------------------------------------
#if ENABLE_IAP
		private static IStoreController m_StoreController;          // The Unity Purchasing system.
		private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.
#endif
		private string m_currentCodeTransaction = "";
		private string m_currentEventIAP = "";
		private string m_currentIdProduct = "";

		// ----------------------------------------------
		// GETTERS/SETTERS
		// ----------------------------------------------

		// ----------------------------------------------
		// CONSTRUCTOR
		// ----------------------------------------------	
		// -------------------------------------------
		/* 
		 * Initialitzation
		 */
		public void Init(params string[] _iaps)
		{
#if ENABLE_IAP
			if (m_StoreController == null)
			{
				if (IsInitialized())
				{
					return;
				}

				UIEventController.Instance.UIEvent += new UIEventHandler(OnUIEvent);

				var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());


				for (int i = 0; i < _iaps.Length; i++)
				{
					builder.AddProduct(_iaps[i], ProductType.Consumable);
				}

				UnityPurchasing.Initialize(this, builder);
		}
#endif
		}

#if ENABLE_IAP
		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public void Destroy()
		{
			UIEventController.Instance.UIEvent -= OnUIEvent;
			Destroy(_instance.gameObject);
			_instance = null;
		}

		// -------------------------------------------
		/* 
		 * IsInitialized
		 */
		private bool IsInitialized()
		{
			return m_StoreController != null && m_StoreExtensionProvider != null;
		}

		// -------------------------------------------
		/* 
		 * OnInitialized
		 */
		public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
		{
			m_StoreController = controller;
			m_StoreExtensionProvider = extensions;
            UIEventController.Instance.DispatchUIEvent(EVENT_IAP_INITIALIZED, IsInitialized());
		}

		// -------------------------------------------
		/* 
		 * OnInitializeFailed
		 */
		public void OnInitializeFailed(InitializationFailureReason error)
		{
			Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
            UIEventController.Instance.DispatchUIEvent(EVENT_IAP_INITIALIZED, false);
        }

		// -------------------------------------------
		/* 
		 * BuyProductID
		 */
		public void BuyProductID(string _productId)
		{
			if (IsInitialized())
			{
				Product product = m_StoreController.products.WithID(_productId);

				if (product != null && product.availableToPurchase)
				{
					Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
					m_StoreController.InitiatePurchase(product);
				}
				else
				{
					Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
				}
			}
			else
			{
				Debug.Log("BuyProductID FAIL. Not initialized.");
			}
		}

		// -------------------------------------------
		/* 
		 * RestorePurchases
		 */
		public void RestorePurchases()
		{
			if (!IsInitialized())
			{
				Debug.Log("RestorePurchases FAIL. Not initialized.");
				return;
			}

			// We are not running on an Apple device. No work is necessary to restore purchases.
			Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
		}

		// -------------------------------------------
		/* 
		 * ProcessPurchase
		 */
		public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
		{
			Debug.Log(string.Format("ProcessPurchase: SUCCESS. Product: '{0}'", args.purchasedProduct.definition.id));
			UIEventController.Instance.DispatchUIEvent(EVENT_IAP_CONFIRMATION, true, args.purchasedProduct.definition.id);
			return PurchaseProcessingResult.Complete;
		}


		// -------------------------------------------
		/* 
		 * OnPurchaseFailed
		 */
		public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
		{
			Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
			UIEventController.Instance.DispatchUIEvent(EVENT_IAP_CONFIRMATION, false, product.definition.id);
		}

		// -------------------------------------------
		/* 
		 * OnUIEvent
		 */
		private void OnUIEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == EVENT_IAP_CONFIRMATION)
			{
				bool success = (bool)_list[0];
				string iapID = (string)_list[1];
				Debug.Log("EVENT_IAP_CONFIRMATION::success[" + success + "]::iapID[" + iapID + "]");
			}
		}
#endif
	}

}