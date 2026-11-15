"""
Packages the portable release (AkhilFaceID-Portable-x64.zip),
calculates SHA-256 hashes, and writes release documentation.
"""
import hashlib
import os
import shutil
import zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DIST_DIR = ROOT / "dist"
DIST_DIR.mkdir(parents=True, exist_ok=True)

BUILD_DIR = ROOT / "build"
if not (BUILD_DIR / "fgsetup.exe").exists() and (ROOT / "build" / "Release" / "fgsetup.exe").exists():
    BUILD_DIR = ROOT / "build" / "Release"
APP_BIN = ROOT / "app" / "WinFace" / "bin" / "publish"
MODELS_DIR = ROOT / "models" / "runtime"
ASSETS_DIR = ROOT / "assets"

PORTABLE_NAME = "AkhilFaceID-Portable-x64"
PORTABLE_STAGING = DIST_DIR / PORTABLE_NAME
PORTABLE_ZIP = DIST_DIR / f"{PORTABLE_NAME}.zip"

def create_portable_package():
    print(f"Creating portable package in {PORTABLE_STAGING}...")
    if PORTABLE_STAGING.exists():
        shutil.rmtree(PORTABLE_STAGING)
    PORTABLE_STAGING.mkdir(parents=True)

    # 1. Main Executables and Managed Assemblies
    for f in ["AkhilFaceID.exe", "AkhilFaceID.dll", "AkhilFaceID.deps.json", "AkhilFaceID.runtimeconfig.json",
              "WinFace.exe", "WinFace.dll", "WinFace.deps.json", "WinFace.runtimeconfig.json"]:
        src = APP_BIN / f
        if src.exists():
            shutil.copy2(src, PORTABLE_STAGING / f)

    # 2. Native C++ Binaries
    for f in ["WinFaceCP.dll", "fgsetup.exe", "fgcredtest.exe", "fgsound.exe", "onnxruntime.dll"]:
        src = BUILD_DIR / f
        if src.exists():
            shutil.copy2(src, PORTABLE_STAGING / f)

    # 3. Offline Models
    dest_models = PORTABLE_STAGING / "models"
    dest_models.mkdir(parents=True, exist_ok=True)
    for m in ["face_detector.onnx", "face_landmarks_detector.onnx", "fas_v1se_s4.0.onnx",
              "fas_v2_s2.7.onnx", "canonical_face.bin", "w600k_r50.onnx"]:
        src = MODELS_DIR / m
        if src.exists():
            shutil.copy2(src, dest_models / m)

    # 4. Assets
    dest_assets = PORTABLE_STAGING / "assets"
    dest_assets.mkdir(parents=True, exist_ok=True)
    for a in ["sfx_unlock.wav", "tile.bmp", "akhilfaceid.png", "akhilfaceid.ico"]:
        src = ASSETS_DIR / a
        if src.exists():
            shutil.copy2(src, dest_assets / a)

    # 5. Documents
    for doc in ["README.md", "PRIVACY.md", "SECURITY.md", "LICENSE"]:
        src = ROOT / doc
        if src.exists():
            shutil.copy2(src, PORTABLE_STAGING / doc)
    rec = ROOT / "install" / "RECOVERY.md"
    if rec.exists():
        shutil.copy2(rec, PORTABLE_STAGING / "RECOVERY.md")

    # 6. Portable Readme
    portable_readme = """# AKHIL FACE ID - PORTABLE RELEASE (x64)

Welcome to the portable release of **Akhil Face ID**.

## Quick Start
1. Double-click `AkhilFaceID.exe` (or `WinFace.exe`).
2. The Dashboard will initialize and automatically detect your webcam (e.g. HP Wide Vision HD Camera).
3. Click "Start Face Recognition" or "Test" in the navigation bar to test local biometric recognition.
4. Click "Enroll Face" to enroll your biometric profile using guided 5-pose 3D mesh scanning.

## Features Included
- 100% Offline, on-device AI facial recognition (ArcFace 512D + MediaPipe 478 3D landmarks).
- Real-time anti-spoofing (Dual MiniFASNet + 3D Parallax DLT Homography).
- System tray minimization and Windows startup support.
- Requires no external installation, no Visual Studio, and no cloud dependencies.
"""
    (PORTABLE_STAGING / "README-PORTABLE.md").write_text(portable_readme, encoding="utf-8")

    # Zip the portable directory
    print(f"Compressing {PORTABLE_ZIP}...")
    if PORTABLE_ZIP.exists():
        PORTABLE_ZIP.unlink()
    with zipfile.ZipFile(PORTABLE_ZIP, "w", zipfile.ZIP_DEFLATED, compresslevel=6) as zf:
        for root_dir, _, files in os.walk(PORTABLE_STAGING):
            for file in files:
                abs_file = Path(root_dir) / file
                rel_file = abs_file.relative_to(DIST_DIR)
                zf.write(abs_file, rel_file)
    print(f"Portable release created: {PORTABLE_ZIP.stat().st_size / (1024*1024):.1f} MB")

def generate_checksums():
    print("Calculating SHA-256 checksums...")
    sums = []
    for f in sorted(DIST_DIR.iterdir()):
        if f.is_file() and f.suffix in [".exe", ".zip", ".msix"]:
            h = hashlib.sha256(f.read_bytes()).hexdigest()
            sums.append(f"{h}  {f.name}")
            # Also create individual .sha256 file
            (DIST_DIR / f"{f.name}.sha256").write_text(f"{h}  {f.name}\n", encoding="ascii")
    
    sha_file = DIST_DIR / "SHA256SUMS.txt"
    sha_file.write_text("\n".join(sums) + "\n", encoding="ascii")
    print(f"Wrote {sha_file}")

if __name__ == "__main__":
    create_portable_package()
    generate_checksums()

