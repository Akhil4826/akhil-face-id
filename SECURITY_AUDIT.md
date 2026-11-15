# SECURITY AUDIT & THREAT MODEL: AKHIL FACE ID

**Product:** Akhil Face ID  
**Audit Standard:** Zero-Trust Endpoint Architecture & Threat Matrix  
**Date:** November 15, 2026  

---

## 1. Threat Matrix & Security Analysis

| Threat / Attack Vector | Severity | Existing Mitigation in Upstream | Akhil Face ID Hardening | Status |
| :--- | :--- | :--- | :--- | :--- |
| **Password Theft via Memory Dumping** | Critical | Memory wiped using `SecureZeroMemory` in RAII struct `SecurePassword`. | Verified RAII scoping; zero password string copies preserved in managed .NET heap. | **Protected** |
| **Biometric Template Tampering / Theft** | High | Stored in `C:\ProgramData\WinFace\templates.bin` restricted by NTFS ACL to Administrators & SYSTEM. | Added **authenticated DPAPI encryption** on disk (`templates.bin`) preventing offline cold-disk extraction. | **Enhanced** |
| **Plaintext Password Extraction from Disk** | Critical | Password sealed with RSA-2048 key inside hardware TPM 2.0 via `MS_PLATFORM_CRYPTO_PROVIDER`. Key DACL restricted to `SYSTEM` (`D:P(A;;GA;;;SY)`). | Validated TPM sealed container. Unprivileged accounts or copied disk images cannot decrypt the secret. | **Protected** |
| **Named Pipe / RPC Privilege Escalation** | Critical | **Zero-IPC** inside `LogonUI.exe`. The Credential Provider runs purely in-process; no network ports or named pipes exist. | Maintained pure in-process execution for authentication. Desktop app communicates only with elevated CLI via anonymous pipes. | **Protected** |
| **Replay / Spoofing via 2D Photo or Screen** | High | Dual MiniFASNet texture classification (`fas_v1se`, `fas_v2`), CSPRNG head-turn challenge, and 3D parallax DLT homography residual check. | Thresholds enforced (cosine >= 0.42, DLT residual >= 0.025). Documented webcam limitations vs active IR hardware. | **Protected** |
| **Arbitrary Credential Serialization** | High | Single-use proof token (`take_verified()`) expires after 10s and is cleared atomically on first access. | Retained atomic clearing (`verified_at_.exchange(0)`). Reject serialization without valid token. | **Protected** |
| **Virtual Camera Injection (OBS, Deepfake)** | Medium | Hardware bus prefix enforcement (`\\?\usb#`, `\\?\pci#`, etc.); virtual links (`swd#`, `root#`) rejected. | Verified camera filter against HP Wide Vision HD camera. Pinned camera ID enforced. | **Protected** |
| **Accidental Lockout / System Inaccessibility** | Critical | Standard Windows PIN and Password tiles are **never** hidden, altered, or disabled. | Dual fallback always active. Comprehensive offline recovery procedure documented in `RECOVERY.md`. | **Protected** |

---

## 2. Cryptographic Architecture

### A. TPM 2.0 Key Isolation
- **Provider:** Microsoft Platform Crypto Provider (`MS_PLATFORM_CRYPTO_PROVIDER`).
- **Algorithm:** RSA 2048-bit with OAEP padding (`BCRYPT_OAEP_PADDING_INFO` with `BCRYPT_SHA256_ALGORITHM`).
- **Key Flags:** `NCRYPT_MACHINE_KEY_FLAG | NCRYPT_SILENT_FLAG`.
- **DACL:** Restricted strictly to Local System (`NT AUTHORITY\SYSTEM`) via SDDL string:
  ```
  D:P(A;;GA;;;SY)
  ```
- **Consequence:** Even local administrators cannot decrypt the stored password blob directly without impersonating `SYSTEM` or running within `LogonUI`. The blob is bound to the physical motherboard's TPM silicon and cannot be decrypted on another machine.

### B. Biometric Template Protection
- Biometric embeddings represent mathematically reversible facial characteristics.
- Storing them in raw binary float arrays poses privacy risks if the disk is mounted offline.
- **Akhil Face ID Implementation**:
  - `templates.bin` is protected at the file system level with NTFS ACLs (`Administrators:F`, `SYSTEM:F`, `Users:R`).
  - Biometric vector buffers are encrypted using Windows DPAPI (`CryptProtectData`) with machine-scope and optional user-entropy flags before writing to disk, ensuring data at rest is encrypted.

### C. Password Hygiene
- Passwords entered in the UI are converted to unmanaged `SecureString` / `SecurePassword` structures.
- Upon completion of password verification or encryption, `SecureZeroMemory` wipes the memory buffers.

---

## 3. RGB Webcam Limitations vs Windows Hello IR

Akhil Face ID utilizes the laptop's standard RGB camera. Users must be aware of the inherent technical trade-offs compared to specialized Windows Hello hardware:

1. **Illumination Dependence**: RGB recognition requires ambient light. In complete darkness, facial landmarks cannot be regressed unless screen brightness provides illumination.
2. **Infrared Spectral Filtering**: True Windows Hello cameras use 850nm/940nm active IR illumination to detect subsurface skin scattering and reject high-resolution synthetic masks. While Akhil Face ID's dual MiniFASNet models detect print/screen moire patterns and 3D parallax catches planar rotations, it cannot match physical time-of-flight depth sensors.
3. **Safety Fallback**: The software **never** disables PIN or password sign-in. If ambient lighting is insufficient, the user simply enters their Windows PIN or password.

