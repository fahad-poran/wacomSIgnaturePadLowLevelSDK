# 🖋️ Wacom STU-430 Signature Capture & MVC5 Integration

> **Complete documentation** for integrating **Wacom STU-430 signature capture** with smooth rendering, image saving, and ASP.NET MVC5 web upload using FormData.

---

## 📚 Table of Contents
1. [Overview](#overview)
2. [Setup Requirements](#setup-requirements)
3. [Core Workflow](#core-workflow)
4. [Code Implementation](#code-implementation)
   - [1️⃣ Initialize USB Device](#1️⃣-initialize-usb-device)
   - [2️⃣ Capture Signature Form](#2️⃣-capture-signature-form)
   - [3️⃣ Retrieve Pen Data](#3️⃣-retrieve-pen-data)
   - [4️⃣ Draw & Save Smooth Image](#4️⃣-draw--save-smooth-image)
5. [📈 Image Flow](#📈-image-flow)
6. [⚙️ MVC5 Web Integration](#⚙️-mvc5-web-integration)
   - [Frontend (Browser)](#frontend-browser)
   - [Backend (C# Controller)](#backend-c-controller)
7. [💡 Enhancement Ideas](#💡-enhancement-ideas)
8. [📜 License](#📜-license)

---

## 🧠 Overview
This documentation explains how to:
- Capture signatures from **Wacom STU-430** devices.
- Render them smoothly as images (PNG or Base64).
- Save and upload them into your **ASP.NET MVC5 application** using **FormData**.

---

## ⚙️ Setup Requirements
| Type | Details |
|------|----------|
| 💻 **Hardware** | Wacom STU-430 connected via USB |
| 🧩 **Software** | Windows 7/8/10/11 + .NET Framework 4.8 |
| 📦 **SDK** | Wacom STU SDK (`wgssSTU.dll`) |
| 🌐 **Browser Integration** | Optional: SigCaptX Service for web capture |

> ⚡ **Tip:** Ensure `wgssSTU.dll` is correctly referenced in your project.

---

## 🪄 Core Workflow

| Step | Description |
|------|--------------|
| **1** | Initialize and connect to STU-430 via USB |
| **2** | Launch signature form for user input |
| **3** | Retrieve pen stroke data (coordinates, pressure, timestamp) |
| **4** | Render and save as smooth PNG |
| **5** | Upload via FormData to MVC Controller |
| **6** | Store image path or Base64 in database |

---

## 💻 Code Implementation

### 1️⃣ Initialize USB Device
```csharp
wgssSTU.UsbDevices usbDevices = new wgssSTU.UsbDevices();
if (usbDevices.Count == 0)
{
    MessageBox.Show("No STU devices attached");
    return;
}
wgssSTU.IUsbDevice usbDevice = usbDevices[0];
```
> Initializes and connects to the first detected Wacom STU-430 device.

---

### 2️⃣ Capture Signature Form
```csharp
SignatureForm demo = new SignatureForm(usbDevice, false);
demo.ShowDialog();
```
- Displays the signature pad UI.
- `false` disables encryption (set `true` for secure mode).

---

### 3️⃣ Retrieve Pen Data
```csharp
if (penDataType == (int)PenDataOptionMode.PenDataOptionMode_TimeCountSequence)
    penTimeData = demo.getPenTimeData();
else
    penData = demo.getPenData();
```
> Captures **x/y coordinates**, **pressure**, and **timestamps**.

---

### 4️⃣ Draw & Save Smooth Image
```csharp
SaveSignatureAsImage(penData, penTimeData, demo.getCapability());
```

**Rendering Details:**
- Anti-aliasing for smooth curves  
- Rounded line caps  
- High-quality drawing  
- Save as PNG:
```csharp
bitmap.Save(filePath, ImageFormat.Png);
```

---

## 📈 Image Flow

| Flow Step | Description |
|------------|--------------|
| 🖋️ User signs on device | Captures pen coordinates |
| 💾 Application saves image | Converts pen data → bitmap |
| 📸 Image stored locally | As PNG or Base64 |
| 🌐 Uploaded to MVC App | Via AJAX FormData |
| 🗄️ Stored in Database | As path or Base64 string |

---

## ⚙️ MVC5 Web Integration

### 🧩 Frontend (Browser)
HTML + JavaScript example for signature upload:
```html
<form id="signatureForm" enctype="multipart/form-data">
  <input type="file" id="signatureFile" name="signature" accept="image/png" hidden>
</form>

<script>
async function uploadSignature(file) {
  const formData = new FormData();
  formData.append("signature", file);

  const response = await fetch("/Signature/Upload", {
    method: "POST",
    body: formData
  });

  const result = await response.json();
  console.log(result);
}
</script>
```

### ⚙️ Backend (C# Controller)
```csharp
[HttpPost]
public ActionResult Upload(HttpPostedFileBase signature)
{
    if (signature != null && signature.ContentLength > 0)
    {
        string fileName = Path.GetFileName(signature.FileName);
        string path = Path.Combine(Server.MapPath("~/Signatures"), fileName);
        signature.SaveAs(path);

        // Store file path or Base64 in DB
        // Example: db.Signatures.Add(new SignatureModel { Path = path });
        return Json(new { success = true, path });
    }
    return Json(new { success = false, message = "No file uploaded" });
}
```
> 🗂️ Saves the signature PNG to `/Signatures` folder and stores its path in the database.

---

## 💡 Enhancement Ideas
- Use **Bezier interpolation** for smoother curves.
- Add **timestamp replay** for each stroke.
- Integrate with **PDF signing** or **e-form workflows**.
- Capture **pressure, tilt**, and **pen type** for analytics.

---

## 📜 License
MIT © 2025 — Documentation prepared for MVC5 Signature Integration.
