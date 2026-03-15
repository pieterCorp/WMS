using System;
using UnityEngine;
using UnityEngine.UI;
using MobileApp.API;

namespace MobileApp.UI
{
    public class OrderScreen : MonoBehaviour
    {
        public WarehouseAPI api;
        public Text orderIdText;
        public Text itemsText;

        public void LoadOrder(string orderId)
        {
            orderIdText.text = "Loading...";
            api.GetOrder(orderId, OnOrder, OnError);
        }

        void OnOrder(OrderDto order)
        {
            if (order == null)
            {
                orderIdText.text = "Order not found";
                itemsText.text = "";
                return;
            }

            orderIdText.text = order.orderId;
            itemsText.text = "";
            if (order.items != null)
            {
                foreach (var it in order.items)
                {
                    itemsText.text += $"{it.location} - {it.product} x{it.quantity}\n";
                }
            }
        }

        void OnError(string err)
        {
            orderIdText.text = "Error";
            itemsText.text = err;
        }
    }
}
