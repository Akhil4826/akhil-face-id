Akhil Face ID - Privacy Policy and Security Notice
Version 1.0, Production Release for HP Victus

Please read this before installing or using Akhil Face ID. You must agree to it to continue.

=====================================================================
PRIVACY POLICY
=====================================================================

1. Everything stays 100% on this PC

Akhil Face ID operates completely offline on your personal computer. It has no
account creation, no cloud service, no external telemetry, no background analytics,
and no advertising. It never uploads, sells or shares your face data, your
passwords, or your activity logs.

2. What Akhil Face ID stores, and where

- Biometric Face Templates: C:\ProgramData\WinFace\profiles.bin
  High-dimensional vector embeddings (512 floating-point values per sample) computed
  from facial landmarks. They are mathematical representations, not photograph files,
  and your actual facial portrait cannot be reconstructed from them. The directory
  is protected with strict NTFS access control lists (Administrators and SYSTEM only).

- Windows Account Credential, Hardware-Encrypted: C:\ProgramData\WinFace\secret.tpm
  Encrypted using hardware keys residing within your laptop's TPM 2.0 (Trusted
  Platform Module) security chip. The encrypted credential can strictly be decrypted
  by the Windows Local System Authority (SYSTEM) at the Windows LogonUI lock screen.
  The sealed file is completely useless on any other computer.
  For the in-app verification test, a protected copy is secured via Windows DPAPI:
  %LOCALAPPDATA%\WinFace\secret.dpapi

- Diagnostic Audit Log: C:\ProgramData\WinFace\log.txt
  Timestamps, operational events, and diagnostic outcomes (e.g., "face recognised").
  It never logs photographs, biometric vectors, or credentials.

- Configuration & Settings: Windows Registry HKLM\SOFTWARE\WinFace (and user preferences
  in HKCU\Software\WinFace).

- Authentication Timestamps & Lockout Counters: HKLM\SOFTWARE\WinFace\State
  Records the timestamp of last successful PIN/password sign-in and consecutive failed
  recognition attempts for Phone-style PIN fallback policies (after restart or 48 hours).
  Updated strictly locally.

- Security Snapshot Photos (Disabled by Default):
  C:\ProgramData\WinFace\intruders. If explicitly enabled in the Security Centre,
  a single low-resolution snapshot is captured upon repeated unauthorized attempts,
  encrypted with local TPM keys. Maximum 20 snapshots retained up to 30 days.

3. What Akhil Face ID never stores

RGB webcam video frames are processed entirely in volatile RAM buffers and immediately
wiped (ZeroMemory). Raw video or photos are never written to disk or transmitted.
The application interface renders solely geometric landmark wireframes.

4. Camera Resource Management

The HP Victus webcam is engaged strictly while an active recognition check executes
(at the Windows lock screen, during facial enrollment, or in the interactive test).
The camera device handle is immediately closed and released upon completion.
Only physical hardware cameras are accepted; virtual video sources are strictly rejected.

5. Offline Integrity

Akhil Face ID is self-contained. Biometric inference, neural model processing,
and authentication validation run locally on your device with no external dependencies.
No telemetry or update calls are made to external repositories.

6. User Control & Data Erasure

- Turn off face unlock anytime: Akhil Face ID > Home > Turn face unlock off.
- One-click full data eradication: Akhil Face ID > Home > Erase all face data.
  This permanently deletes biometric profiles, TPM-sealed secrets, security photos,
  logs, and configuration entries.
- Uninstallation: Remove cleanly via Windows Settings > Installed Apps.

7. Ethical Use

Enroll only your own face or that of an authorized device owner with explicit consent.
Do not use biometric systems to track or monitor unauthorized individuals.

=====================================================================
SECURITY NOTICE & HARDWARE DISCLOSURE
=====================================================================

- Akhil Face ID utilizes your laptop's built-in RGB high-definition webcam.
  While it incorporates multi-layer anti-spoofing (MiniFASNet neural texture analysis,
  3D parallax geometry verification, and screen lighting checks), an RGB camera does
  not possess active infrared structured light hardware found in dedicated Windows Hello
  IR sensors.

- Windows PIN & Password Preservation: Akhil Face ID acts as an added biometric
  convenience provider. Standard Windows PIN and password sign-in tiles remain
  fully active and accessible at all times under "Sign-in options". If face recognition
  times out or is not completed, your standard Windows PIN is immediately presented.

- Low-Light Screen Illumination Assist: During facial enrollment, Akhil Face ID
  provides a screen illumination assist that can illuminate your face with a bright
  white display panel to ensure optimal neural feature extraction in dim environments.

- Experimental Software: Provided under the Apache License 2.0 without warranty.
  Emergency recovery procedures are documented in RECOVERY.md.
