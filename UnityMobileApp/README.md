Unity Mobile App (MVP)

This is a minimal Unity project skeleton for the warehouse mobile client MVP described in warehouse_mvp_spec.txt.

Quick start

1. Open this folder in Unity (2021 LTS or later recommended).
2. In Project Settings -> Player, configure target platform (Android or iOS) and set the bundle id.
3. Set the Backend base URL in Assets/Scripts/Configuration.cs (or via inspector) to point to your ASP.NET Core backend.
4. Add ZXing.Net or other barcode scanning plugin via the Package Manager (or import the DLL) for camera barcode scanning.
5. Build and run on a phone (Android or iOS). The provided scripts are minimal placeholders to connect to the REST API.

Files added

- Assets/Scripts/API/WarehouseAPI.cs - simple REST client for order and pick endpoints
- Assets/Scripts/Scanning/BarcodeScanner.cs - placeholder scanner that can be wired to ZXing
- Assets/Scripts/UI/OrderScreen.cs - sample UI controller for displaying order data
- Assets/Scripts/UI/PickListScreen.cs - sample UI controller for pick list
- Assets/Scripts/UI/ScanScreen.cs - sample UI controller for scanning workflow

Notes

- This is a lightweight scaffold to get started. You will need to create scenes and UI prefabs in Unity and wire the scripts to UI elements.
- For ZXing integration, add ZXing.Net and call its barcode reader from BarcodeScanner.cs.
