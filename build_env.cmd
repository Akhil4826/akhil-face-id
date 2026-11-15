@echo off
call "C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\VC\Auxiliary\Build\vcvars64.bat"
set "SDK_ROOT=C:\Projects\winface-main\third_party\winsdk\c"
set "PATH=%SDK_ROOT%\bin\10.0.22621.0\x64;C:\Users\akhil\AppData\Roaming\Python\Python312\Scripts;%PATH%"
set "INCLUDE=%INCLUDE%;%SDK_ROOT%\Include\10.0.22621.0\ucrt;%SDK_ROOT%\Include\10.0.22621.0\um;%SDK_ROOT%\Include\10.0.22621.0\shared;%SDK_ROOT%\Include\10.0.22621.0\winrt"
set "LIB=%LIB%;%SDK_ROOT%\ucrt\x64;%SDK_ROOT%\um\x64"
set "WindowsSdkDir=%SDK_ROOT%\"
set "WindowsSDKVersion=10.0.22621.0\"
%*

