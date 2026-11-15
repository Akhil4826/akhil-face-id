# ARCHITECTURE SPECIFICATION: AKHIL FACE ID

**Product:** Akhil Face ID  
**Architecture Classification:** Hardware-Assisted On-Device Biometric Desktop System  
**Target Platform:** Windows 11 (x64)  
**Primary Device Target:** HP Victus Laptop (Built-in HP Wide Vision HD Webcam `VID_0408&PID_5482`)

---

## 1. System Decomposition

Akhil Face ID operates across four isolated domains, maintaining zero network exposure and strict separation of privilege:

```mermaid
flowchart TD
    subgraph UI ["User Desktop Domain (Standard / Admin User)"]
        A["Akhil Face ID WPF Application\n(AkhilFaceID.exe - .NET 8/10 Desktop)"]
        B["System Tray & Background Daemon"]
        C["Local Configuration & Preferences\n(%AppData%\\AkhilFaceID)"]
        A <--> B
        A --> C
    end

    subgraph NativeTool ["Native Execution Layer (C++20 CLI Engine)"]
        D["fgsetup.exe (Management & Engine CLI)"]
        E["fgsound.exe (Independent Audio Player)"]
        A -- "Anonymous Pipe JSON IPC" --> D
        D -. "Spawn" .-> E
    end

    subgraph CVEngine ["In-Process Biometric Engine (fgengine)"]
        F["Windows Media Foundation\n(720p/1080p MJPG -> BGR32)"]
        G["MediaPipe BlazeFace SSD\n(face_detector.onnx)"]
        H["MediaPipe 3D Landmarker\n(face_landmarks_detector.onnx - 478 pts)"]
        I["ArcFace INT8 / FP32 Embedding\n(w600k_r50.onnx - 512D Vector)"]
        J["MiniFASNet Dual Anti-Spoofing\n(fas_v1se & fas_v2)"]
        K["3D Parallax DLT Homography Engine"]
        L["State Machine Decision Engine"]
        F --> G --> H
        H --> I & J & K
        I & J & K --> L
    end

    subgraph Winlogon ["Windows Authentication Domain (SYSTEM - Winlogon / LogonUI)"]
        M["LogonUI.exe (Lock Screen / Sign-in)"]
        N["AkhilFaceIDCP.dll (Credential Provider V2)"]
        O["GDI+ Layered Desktop HUD Overlay"]
        P["Single-Use Proof Token (10s expiry)"]
        Q["TPM 2.0 RSA-2048 Vault\n(MS_PLATFORM_CRYPTO_PROVIDER)"]
        R["LSASS Authentication Token Generation"]
        M --> N
        N --> O
        N --> CVEngine
        L -- "Unlock Signal" --> P
        P --> Q --> R
    end
```

---

## 2. Biometric Verification & Liveness Pipeline

```mermaid
sequenceDiagram
    autonumber
    actor User as User Face
    participant Cam as HP Wide Vision Webcam
    participant Detect as BlazeFace & 3D Landmarker
    participant AntiSpoof as Dual MiniFASNet & Geometry
    participant Recog as ArcFace 512D Embedder
    participant State as Decision Engine
    participant TPM as TPM 2.0 Security Vault
    participant LSASS as Windows LSASS / LogonUI

    User ->> Cam: Real-time RGB Frames (30 FPS)
    Cam ->> Detect: Capture Frame (1280x720 BGR)
    Detect ->> Detect: Locate Bounding Box & 478 3D Landmarks
    Detect ->> AntiSpoof: Crop Normalized ROI
    AntiSpoof ->> AntiSpoof: Classify Texture (Screen/Paper vs Live Skin >= 0.60)
    Detect ->> Recog: 5-Point Least-Squares Similarity Alignment
    Recog ->> Recog: Extract 512D ArcFace Vector & Compute Cosine Similarity
    Recog ->> State: Frame Vector Match >= Threshold (0.42 default)
    State ->> User: Prompt Random CSPRNG Head Turn (Left / Right)
    User ->> Detect: Head Turn Motion
    Detect ->> AntiSpoof: Track Landmark Trajectories
    AntiSpoof ->> AntiSpoof: Calculate DLT Homography Residual (>= 0.025 depth check)
    AntiSpoof ->> State: Parallax Depth Valid (Non-planar)
    State ->> TPM: Issue Single-Use Proof Token (10s window)
    TPM ->> TPM: Decrypt RSA-2048 Sealed Password in Memory
    TPM ->> LSASS: Submit KERB_INTERACTIVE_UNLOCK_LOGON
    LSASS -->> User: Workstation Unlocked
```

---

## 3. Storage and Filesystem Architecture

```
C:\Program Files\AkhilFaceID\            [Protected: Read-only for standard users]
├── AkhilFaceID.exe                     [Main WPF User Application]
├── AkhilFaceID.dll                     [Compiled Application Assembly]
├── AkhilFaceIDCP.dll                   [Windows Credential Provider V2]
├── fgsetup.exe                         [Native Engine CLI]
├── fgsound.exe                         [Standalone Audio Process]
├── onnxruntime.dll                     [ONNX Runtime Dynamic Library]
├── models\
│   ├── face_detector.onnx              [BlazeFace Short-Range Detector]
│   ├── face_landmarks_detector.onnx    [478 3D Landmark Regressor]
│   ├── fas_v1se_s4.0.onnx              [MiniFASNet Texture Anti-Spoof V1]
│   ├── fas_v2_s2.7.onnx                [MiniFASNet Texture Anti-Spoof V2]
│   ├── canonical_face.bin              [Procrustes Canonical Reference Model]
│   └── w600k_r50.onnx                  [InsightFace ArcFace 512D Recognition Model]
├── assets\
│   ├── sfx_unlock.wav                  [PCM WAV Sound Asset]
│   └── tile.bmp                        [LogonUI Sign-in Tile Icon]
└── RECOVERY.md                         [Emergency Offline Recovery Instructions]

C:\ProgramData\AkhilFaceID\             [Protected ACL: SYSTEM & Admins Full Control]
├── templates.bin                       [Biometric Enrolled Vectors - DPAPI Encrypted]
├── secret.tpm                          [TPM-Sealed RSA-2048 Password Container]
├── settings.json                       [System-wide Recognition Configuration]
├── log.txt                             [Application Event Log]
└── intruders\                          [AES-256-GCM Encrypted Intruder Snapshots]
```

---

## 4. Hardware Camera Interfacing

The HP Victus built-in camera is validated via Windows Media Foundation:
- **Symbolic Link:** `\\?\usb#vid_0408&pid_5482&mi_00#6&8323d14&0&0000#{e5323777-f976-4f5b-9b55-b94699c46e44}\global`
- **Bus Interface:** USB 2.0 Internal Hub (`VID_0408` Quanta Computer Inc, `PID_5482` HP Wide Vision HD Camera)
- **Format:** MJPG / YUY2 negotiated to 1280x720 at 30 FPS.
- **Hardware Filtering:** The engine validates the hardware prefix against `\\?\usb#`, strictly excluding virtual cameras (`root#`, `swd#`, OBS Virtual Camera, ManyCam) and HDMI capture devices.

