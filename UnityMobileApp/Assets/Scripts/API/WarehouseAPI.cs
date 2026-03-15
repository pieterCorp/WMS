using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace MobileApp.API
{
    [Serializable]
    public class OrderItemDto
    {
        public string product;
        public string sku;
        public string location;
        public int quantity;
    }

    [Serializable]
    public class OrderDto
    {
        public string orderId;
        public OrderItemDto[] items;
    }

    [Serializable]
    public class PickRequest
    {
        public string orderId;
        public string productBarcode;
        public string rack;
        public string slot;
    }

    public class WarehouseAPI : MonoBehaviour
    {
        public string backendBaseUrl = "http://10.0.2.2:5000"; // default for emulator; set to your backend

        public void GetOrder(string orderId, Action<OrderDto> onSuccess, Action<string> onError)
        {
            StartCoroutine(GetOrderCoroutine(orderId, onSuccess, onError));
        }

        IEnumerator GetOrderCoroutine(string orderId, Action<OrderDto> onSuccess, Action<string> onError)
        {
            var url = $"{backendBaseUrl}/orders/{UnityWebRequest.EscapeURL(orderId)}";
            using (var req = UnityWebRequest.Get(url))
            {
                req.timeout = 10;
                yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
#else
                if (req.isNetworkError || req.isHttpError)
#endif
                {
                    onError?.Invoke(req.error);
                }
                else
                {
                    try
                    {
                        var json = req.downloadHandler.text;
                        var order = JsonUtility.FromJson<OrderDto>(json);
                        onSuccess?.Invoke(order);
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke(ex.Message);
                    }
                }
            }
        }

        public void ConfirmPick(PickRequest pick, Action onSuccess, Action<string> onError)
        {
            StartCoroutine(ConfirmPickCoroutine(pick, onSuccess, onError));
        }

        IEnumerator ConfirmPickCoroutine(PickRequest pick, Action onSuccess, Action<string> onError)
        {
            var url = $"{backendBaseUrl}/pick";
            var body = JsonUtility.ToJson(pick);
            var bodyRaw = System.Text.Encoding.UTF8.GetBytes(body);
            using (var req = new UnityWebRequest(url, "POST"))
            {
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.timeout = 10;

                yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
#else
                if (req.isNetworkError || req.isHttpError)
#endif
                {
                    onError?.Invoke(req.error + ": " + req.downloadHandler.text);
                }
                else
                {
                    onSuccess?.Invoke();
                }
            }
        }
    }
}
