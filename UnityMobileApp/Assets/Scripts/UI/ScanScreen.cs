using System;
using UnityEngine;
using UnityEngine.UI;
using MobileApp.API;
using MobileApp.Scanning;

namespace MobileApp.UI
{
    public class ScanScreen : MonoBehaviour
    {
        public WarehouseAPI api;
        public BarcodeScanner scanner;
        public Text statusText;

        private string currentOrderId;
        private OrderDto currentOrder;

        void Start()
        {
            if (scanner != null)
            {
                scanner.OnBarcodeScanned += OnBarcodeScanned;
            }
        }

        public void BeginOrderFlow(string orderId)
        {
            statusText.text = "Getting order...";
            currentOrderId = orderId;
            api.GetOrder(orderId, (order) =>
            {
                currentOrder = order;
                statusText.text = "Order loaded. Scan rack.";
                // Start camera for scanning
                scanner?.StartCamera();
            }, (err) =>
            {
                statusText.text = "Error: " + err;
            });
        }

        void OnBarcodeScanned(string code)
        {
            // Very simple flow: if scanned code looks like rack (starts with RACK or A), assume rack; otherwise product.
            if (code.StartsWith("RACK-") || code.StartsWith("A"))
            {
                statusText.text = "Rack scanned: " + code + ". Now scan product.";
            }
            else
            {
                statusText.text = "Product scanned: " + code + ". Confirming pick...";
                var pick = new PickRequest()
                {
                    orderId = currentOrderId,
                    productBarcode = code,
                    rack = "unknown",
                    slot = "unknown"
                };

                api.ConfirmPick(pick, () =>
                {
                    statusText.text = "Pick confirmed.";
                }, (err) =>
                {
                    statusText.text = "Pick error: " + err;
                });
            }
        }

        void OnDestroy()
        {
            if (scanner != null)
            {
                scanner.OnBarcodeScanned -= OnBarcodeScanned;
                scanner.StopCamera();
            }
        }
    }
}
