# Akhil Face ID - Security Architecture & Policy

Akhil Face ID provides local biometric authentication for Windows 11 on the HP Victus laptop.

## Core Security Foundations

1. **Hardware-Bound TPM 2.0 Encryption**
   All sensitive authentication credentials passed to Windows LogonUI are sealed using TPM 2.0 asymmetric keys (`secret.tpm`). The keys are bound strictly to `NT AUTHORITY\SYSTEM` with access mask `D:P(A;;GA;;;SY)`. They cannot be extracted by user-space applications or offboarded to another device.

2. **Zero-Trust Memory Hygiene**
   All intermediate password and credential buffers in native C++ and C# code utilize `SecureZeroMemory` and explicit zeroization before deallocation.

3. **Multi-Stage Neural Anti-Spoofing**
   - MediaPipe 468 landmark mesh detection with 3D eye and mouth parallax.
   - Dual MiniFASNet neural models (v1se and v2) for texture and presentation attack detection (2D paper prints, screen replay videos).
   - InsightFace ArcFace (`w600k_r50.onnx`) 512-dimensional normalized embeddings for sub-millimeter biometric clustering.

4. **Preservation of Windows Authentication Authority**
   Akhil Face ID never disables, hides, or interferes with the Windows PIN or password authentication providers. If recognition times out, fails, or is cancelled, Windows LogonUI seamlessly surfaces the native PIN tile.

5. **Offline Airgap**
   Akhil Face ID executes 100% locally on the device with zero cloud APIs, zero remote dependencies, and zero outbound telemetry.
