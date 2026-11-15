# MODEL & DEPENDENCY LICENSE AUDIT: AKHIL FACE ID

**Product:** Akhil Face ID  
**Date:** November 15, 2026  
**Compliance Standard:** Open-Source Software Redistribution & Intellectual Property Review  

---

## 1. Machine Learning Models Audit

| Model Component | Filename | Parameters / Size | Upstream Origin | Redistribution License | Packaging / Distribution Strategy |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Face Detector** | `face_detector.onnx` | BlazeFace SSD (128x128), ~418 KB | Google MediaPipe | **Apache License 2.0** | Bundled directly in setup and portable releases. |
| **3D Landmarks** | `face_landmarks_detector.onnx` | 478 3D Landmarks (256x256), ~4.9 MB | Google MediaPipe | **Apache License 2.0** | Bundled directly in setup and portable releases. |
| **Texture Anti-Spoof V1** | `fas_v1se_s4.0.onnx` | MiniFASNetV1SE (80x80), ~1.7 MB | Silent-Face-Anti-Spoofing | **Apache License 2.0** | Bundled directly in setup and portable releases. |
| **Texture Anti-Spoof V2** | `fas_v2_s2.7.onnx` | MiniFASNetV2 (80x80), ~1.7 MB | Silent-Face-Anti-Spoofing | **Apache License 2.0** | Bundled directly in setup and portable releases. |
| **Reference Geometry** | `canonical_face.bin` | 468 vertices x float32 + weights, ~7.5 KB | Google MediaPipe | **Apache License 2.0** | Bundled directly in setup and portable releases. |
| **Face Recognition** | `w600k_r50.onnx` | ArcFace ResNet-50 (112x112), ~174 MB | DeepInsight (InsightFace) | **Non-Commercial Research License** | Downloaded / extracted with explicit user license acceptance agreement. |

---

## 2. Software & Library Dependencies Audit

| Library / Tool | Version | Origin | License | Terms Compliance |
| :--- | :--- | :--- | :--- | :--- |
| **Microsoft ONNX Runtime** | 1.24.4 | Microsoft | **MIT License** | Fully redistributable C++ runtime (`onnxruntime.dll`). |
| **.NET Runtime** | 8.0 / 10.0 Desktop | Microsoft | **MIT License** | Self-contained / framework-dependent WPF application deployment. |
| **Inno Setup** | 6.7.0 | Jordan Russell | **Inno Setup License (Modified BSD-style)** | Commercial and non-commercial installer creation permitted. |
| **WinFace Base Code** | 1.1.0 | Upstream Contributors | **Apache License 2.0** | Permissive open-source license. Original copyright headers preserved. |

---

## 3. Commercial & Redistribution Compliance Summary

1. **Apache 2.0 & MIT Compliance**:
   All foundational components (MediaPipe detector, landmark regressor, anti-spoofing models, ONNX Runtime, and original WinFace source code) are licensed under Apache 2.0 or MIT, granting full rights to reproduce, modify, and distribute.
2. **InsightFace Non-Commercial Clause**:
   The `w600k_r50.onnx` model provided by InsightFace is restricted to non-commercial evaluation and personal use.
   Akhil Face ID adheres to this requirement by:
   - Requiring the user to acknowledge and accept the InsightFace non-commercial license in the setup wizard.
   - Restricting Akhil Face ID to personal laptop use without monetization.

