# UPSTREAM CODE AUDIT: WinFace v1.1.0

**Audit Date:** November 15, 2026  
**Auditor:** Principal Windows Desktop Architect & Cybersecurity Specialist  
**Upstream Target:** WinFace (https://github.com/nyxri0f8/winface)  
**Workspace:** `c:\Projects\winface-main`  
**Target Laptop:** HP Victus Laptop (Windows 11 x64, Integrated HP Wide Vision HD Camera `VID_0408&PID_5482`)

---

## 1. Executive Summary

WinFace is a native Windows facial recognition logon system designed to bring biometric unlocking to standard RGB webcams without dedicated infrared (IR) depth sensors. It is composed of three primary tiers:
1. **Windows Credential Provider V2 (`WinFaceCP.dll`)**: A native C++20 COM DLL registered into `LogonUI.exe` executing within the `NT AUTHORITY\SYSTEM` context on the secure Winlogon desktop (`WinSta0\Winlogon`).
2. **Recognition Engine (`fgengine`)**: A native C++ computer vision pipeline relying on ONNX Runtime (v1.24.4), MediaPipe FaceMesh (BlazeFace detector + 478 3D landmark regressor), ArcFace embedding extraction (512D vectors), and dual MiniFASNet anti-spoofing classifiers.
3. **Desktop Application (`WinFace.exe`) & Helper Tools (`fgsetup.exe`, `fgsound.exe`, `fgcredtest.exe`)**: A C# WPF management console (.NET 8.0/10.0-windows) driving administrative operations via streaming JSON IPC over standard pipes to `fgsetup.exe`.

The upstream codebase is exceptionally well-engineered with high performance and strong architectural discipline. However, several critical constraints, missing upstream files, and security considerations must be addressed to transform it into the robust, polished product **Akhil Face ID**.

---

## 2. Directory and File Layout Audit

| Directory / File | Lines / Size | Language / Tech | Quality Assessment & Audit Notes |
| :--- | :--- | :--- | :--- |
| `CMakeLists.txt` | 51 lines | CMake 3.20+ | Uses static CRT (`MultiThreaded`), CFG (`/guard:cf`), SDL (`/sdl`), DEP (`/NXCOMPAT`), ASLR (`/DYNAMICBASE`, `/HIGHENTROPYVA`). Delays loading `onnxruntime.dll` on `WinFaceCP.dll`. |
| `engine/camera.cpp, .h` | ~330 lines | C++20, Media Foundation | Enumerates video capture sources via `MFEnumDeviceSources`. Enforces hardware bus checks (`usb#`, `pci#`, `acpi#`, `display#`). Filters IR cameras and HDMI grabbers. Outputs mirrored 720p BGR buffers. |
| `engine/facemesh.cpp, .h` | ~215 lines | C++20, ONNX Runtime | Runs BlazeFace short-range detector (128x128) and landmark regressor (256x256) generating 478 3D points. Computes head pose (yaw, pitch, roll) via Procrustes alignment to `canonical_face.bin`. |
| `engine/geom.cpp, .h` | ~160 lines | C++20 | Implements 5-point similarity transformation, affine warp, and DLT homography estimation with median residual computation for 3D parallax liveness. |
| `engine/recog.cpp, .h` | ~100 lines | C++20, ONNX Runtime | Computes 512D ArcFace embeddings using `arcface_int8.onnx` or `w600k_r50.onnx`. Implements dual MiniFASNet texture checks (`fas_v1se` and `fas_v2`). |
| `engine/decide.cpp, .h` | ~380 lines | C++20 | Core state machine: `Search` -> `Hold` -> `Turn` -> `Action` -> `Flash` -> `Unlock` / `Fail`. Manages CSPRNG random challenges and timeouts. |
| `engine/profiles.cpp, .h`| ~50 lines | C++20 | Binary template serialization (`templates.bin`). Stores enrolled 512D float vectors per profile name. |
| `cp/dll.cpp` | ~75 lines | C++20, COM | Exports standard COM entrypoints (`DllGetClassObject`, `DllCanUnloadNow`). Class factory for `CLSID_FaceProvider`. |
| `cp/provider.cpp` | ~150 lines | C++20, Winlogon | Implements `ICredentialProvider` and `ICredentialProviderSetUserArray`. Filters user tiles, enforces single credential tile per matching user. |
| `cp/credential.cpp` | ~275 lines | C++20, Winlogon | Implements `ICredentialProviderCredential2`. Manages scanner thread lifecycle, display state events, and single-use proof token verification. |
| `cp/helpers.cpp, .h` | ~130 lines | C++20, LSA | Serializes Windows credentials into `KERB_INTERACTIVE_UNLOCK_LOGON` or CredPack buffers for LSASS submission. |
| `cp/secret.cpp, .h` | ~210 lines | C++20, CNG/TPM | Manages RSA-2048 keys sealed in TPM 2.0 (`MS_PLATFORM_CRYPTO_PROVIDER`) with SDDL DACL `D:P(A;;GA;;;SY)`. Also implements DPAPI fallback for test mode. |
| `cp/overlay.cpp, .h` | ~580 lines | C++20, GDI+ | Direct layered window (`WS_EX_LAYERED`) rendering HUD animations on `Winlogon` desktop. Zero camera preview pixels exposed. |
| `cp/scanner.cpp, .h` | ~180 lines | C++20 | Background worker thread driving camera acquisition and `DecisionEngine` evaluation. |
| `cp/intruder.cpp, .h` | ~360 lines | C++20, Crypto | AES-256-GCM encrypted JPEG storage for failed authentication attempts. Keys sealed by TPM. |
| `tools/fgsetup.cpp` | ~1,050 lines | C++20 | Administrative tool driving enrollment, password vaulting, settings changes, status queries, and camera inspection with JSON output. |
| `tools/fgsound.cpp` | ~30 lines | C++20 | Standalone process playing `sfx_unlock.wav` via `PlaySoundW` so audio is not truncated when `LogonUI.exe` terminates. |
| `tools/fgcredtest.cpp` | ~100 lines | C++20, CredUI | Test harness launching `CredUIPromptForWindowsCredentialsW` to verify credentials without locking the workstation. |
| `tools/fgoverlaytest.cpp`| ~600 lines | C++20 | Development tool rendering HUD frames to disk or testing HUD directly on active desktop. |
| `app/WinFace/` | ~3,500 lines | C#, WPF (.NET 8/10) | Rich WPF UI (Pages: Home, Faces, Password, Security, Settings, Logs, About, Setup). Drives `fgsetup.exe --json`. |
| `app/WinFaceUpdater/` | ~300 lines | C#, WPF | Background update checker querying GitHub Releases and verifying SHA256 hashes. |
| `installer/winface.iss` | ~430 lines | Inno Setup 6 Pascal | Inno Setup script packaging native and managed binaries, checking Smart App Control (SAC), and configuring registry. |

---

## 3. Discovered Discrepancies in the Downloaded Repository

When inspecting the extracted GitHub repository ZIP (`c:\Projects\winface-main`), two key build-time items were absent because they were excluded from the upstream source repository:
1. **Pre-trained ONNX Models (`models/runtime/`)**:
   - `face_detector.onnx` (MediaPipe BlazeFace SSD)
   - `face_landmarks_detector.onnx` (MediaPipe 478 3D landmarks)
   - `fas_v1se_s4.0.onnx` (MiniFASNet anti-spoof)
   - `fas_v2_s2.7.onnx` (MiniFASNet anti-spoof)
   - `canonical_face.bin` (MediaPipe canonical 3D face mesh)
   - `w600k_r50.onnx` (InsightFace Buffalo_L ArcFace recognition model)
   *Resolution*: Extracted from the verified release archive and official InsightFace repository and staged cleanly in `models/runtime/`.
2. **ONNX Runtime C++ Development SDK (`third_party/onnxruntime-win-x64-1.24.4/`)**:
   - Headers and import library `onnxruntime.lib` were not included in the repository zip.
   *Resolution*: Acquired and placed the official Microsoft release package into `third_party/onnxruntime-win-x64-1.24.4/`.
3. **Windows SDK Build Tooling**:
   - The host environment had MSVC 19.51 installed, but the Windows SDK headers and `rc.exe` were missing.
   *Resolution*: Extracted the official `Microsoft.Windows.SDK.CPP` and `Microsoft.Windows.SDK.CPP.x64` NuGet packages into `third_party/winsdk` and configured `build_env.cmd` for completely self-contained compilation.

---

## 4. Upstream Audit Findings

### Strengths
- **Pure In-Process Architecture**: Does not rely on long-running insecure network sockets or open named pipes.
- **Hardware Security**: Uses TPM 2.0 with SDDL restriction strictly to `SYSTEM`, making credential retrieval impossible for unprivileged users.
- **Single-Use Proof Token**: LSASS credential serialization cannot be triggered arbitrarily; it requires a time-bounded atomic proof token issued by the liveness and recognition engine.
- **Comprehensive Liveness Defense**: Combines dual-model texture classification, CSPRNG head turn verification, and 3D parallax DLT homography residual checks.

### Areas Requiring Improvement for Akhil Face ID
1. **Branding & Visual Identity**: Fully rename and rebrand all UI, icons, metadata, and strings from "WinFace" / "FaceGate" to "Akhil Face ID".
2. **Webcam Diagnostics & Built-in Camera Support**: Ensure full compatibility with the HP Victus integrated camera (`VID_0408&PID_5482`), display clear diagnostics in the UI, and provide manual and automatic device selection.
3. **Template Encryption**: Enforce authenticated encryption (DPAPI / AES-256-GCM) on biometric templates stored on disk.
4. **Desktop Integration & Startup Experience**: Implement system tray integration, minimize to tray, auto-start on user logon, and desktop shortcuts.
5. **Release Packaging**: Produce complete, standalone deliverables: Inno Setup executable installer (`AkhilFaceID-Setup-x64.exe`) and portable standalone release package (`AkhilFaceID-Portable-x64.zip`).

