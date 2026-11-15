// Plays the unlock sound and speaks a welcome greeting. The credential provider starts this as its own
// process because LogonUI exits a moment after a successful sign-in and would cut off sound.
#include <windows.h>
#include <mmsystem.h>
#include <shlwapi.h>
#include <sapi.h>

#pragma comment(lib, "winmm.lib")
#pragma comment(lib, "shlwapi.lib")
#pragma comment(lib, "ole32.lib")

int WINAPI wWinMain(HINSTANCE, HINSTANCE, PWSTR, int) {
    wchar_t p[MAX_PATH];
    DWORD n = GetModuleFileNameW(nullptr, p, MAX_PATH);
    if (n > 0 && n < MAX_PATH && PathRemoveFileSpecW(p)) {
        wchar_t wav[MAX_PATH];
        wcscpy_s(wav, p);
        if (PathAppendW(wav, L"assets\\sfx_unlock.wav")) {
            PlaySoundW(wav, nullptr, SND_FILENAME | SND_ASYNC | SND_NODEFAULT);
        }
    }

    if (SUCCEEDED(CoInitializeEx(nullptr, COINIT_APARTMENTTHREADED))) {
        ISpVoice* voice = nullptr;
        if (SUCCEEDED(CoCreateInstance(CLSID_SpVoice, nullptr, CLSCTX_ALL, IID_ISpVoice, (void**)&voice))) {
            voice->Speak(L"Welcome sir, how are you today?", SPF_DEFAULT, nullptr);
            voice->Release();
        }
        CoUninitialize();
    }
    return 0;
}
