using System;
using System.Collections;
using UnityEngine;

namespace MobileApp.Scanning
{
    // Minimal barcode scanner placeholder. For production, integrate ZXing.Net or a native plugin.
    public class BarcodeScanner : MonoBehaviour
    {
        public Action<string> OnBarcodeScanned;

        private WebCamTexture webCamTexture;

        public void StartCamera(int requestedWidth = 640, int requestedHeight = 480)
        {
            if (webCamTexture != null && webCamTexture.isPlaying) return;
            if (WebCamTexture.devices.Length == 0)
            {
                Debug.LogWarning("No camera devices found");
                return;
            }
            webCamTexture = new WebCamTexture(WebCamTexture.devices[0].name, requestedWidth, requestedHeight);
            webCamTexture.Play();
            StartCoroutine(ScanLoop());
        }

        public void StopCamera()
        {
            if (webCamTexture == null) return;
            webCamTexture.Stop();
            StopAllCoroutines();
        }

        IEnumerator ScanLoop()
        {
            // Placeholder: this loop just waits. Replace with ZXing decoding to detect barcodes from camera frames.
            while (true)
            {
                yield return new WaitForSeconds(0.5f);
                // TODO: decode frame to barcode using ZXing.
            }
        }

        // Call this from a UI button to simulate a scan (useful in editor/testing).
        public void SimulateScan(string barcode)
        {
            OnBarcodeScanned?.Invoke(barcode);
        }
    }
}
