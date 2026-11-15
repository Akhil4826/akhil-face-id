# Akhil Face ID - Emergency Recovery Guide

Your Windows PIN and password tiles are **never** hidden or removed by Akhil Face ID.
If facial recognition ever misbehaves, simply click **Sign-in options** on the Windows lock screen and select your PIN or password.

---

## Level 1 - Turn Akhil Face ID Off (When Already Signed In)

Open PowerShell as Administrator:
```powershell
& "C:\Program Files\Akhil Face ID\fgsetup.exe" mode off
```
Or use the desktop application: **Akhil Face ID > Home > Turn face unlock off**.
To uninstall completely: run the uninstaller from Windows Settings > Installed Apps.

---

## Level 2 - Recovery Console (If Lock Screen Fails to Load)

If a system update or corrupted driver ever prevents the lock screen from loading:

1. On the Windows sign-in screen, hold the **Shift** key and click **Power > Restart**.
2. Navigate to: **Troubleshoot > Advanced options > Command Prompt**.
3. If BitLocker is enabled on your HP Victus laptop, enter your **BitLocker recovery key** when prompted.
   (If needed: `manage-bde -unlock C: -RecoveryPassword YOUR-48-DIGIT-KEY`).
4. Identify your primary Windows drive (often **C:** or **D:** in recovery mode):
   ```cmd
   dir C:\Windows
   ```
5. Deregister the Akhil Face ID Credential Provider from the offline registry:
   ```cmd
   reg load HKLM\OFFSOFT C:\Windows\System32\config\SOFTWARE
   reg delete "HKLM\OFFSOFT\Microsoft\Windows\CurrentVersion\Authentication\Credential Providers\{C188DC15-E41E-4CCF-9DA9-8238E1D0BBDF}" /f
   reg unload HKLM\OFFSOFT
   ```
6. Type `exit` and click **Continue** to boot into Windows normally.
   Standard Windows PIN and password tiles will be active.
