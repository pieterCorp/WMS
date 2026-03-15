using System;
using UnityEngine;
using UnityEngine.UI;
using MobileApp.API;

namespace MobileApp.UI
{
    public class PickListScreen : MonoBehaviour
    {
        public OrderDto currentOrder;
        public Text titleText;
        public Text listText;

        public void ShowOrder(OrderDto order)
        {
            currentOrder = order;
            if (order == null)
            {
                titleText.text = "No order";
                listText.text = "";
                return;
            }

            titleText.text = $"Order {order.orderId}";
            listText.text = "";
            foreach (var it in order.items)
            {
                listText.text += $"{it.location} - {it.product} x{it.quantity}\n";
            }
        }
    }
}
