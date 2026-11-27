@echo off
setlocal EnableExtensions
cd /d "%~dp0"
set "ADB_EXE="
where adb >nul 2>&1
if not errorlevel 1 set "ADB_EXE=adb"
if not defined ADB_EXE (
    for %%P in (
        "%ANDROID_HOME%\platform-tools\adb.exe"
        "%ANDROID_SDK_ROOT%\platform-tools\adb.exe"
        "%LOCALAPPDATA%\Android\Sdk\platform-tools\adb.exe"
        "%USERPROFILE%\AppData\Local\Android\Sdk\platform-tools\adb.exe"
        "C:\Android\Sdk\platform-tools\adb.exe"
        "C:\Android\android-sdk\platform-tools\adb.exe"
        "%ProgramFiles%\Android\Android Studio\platform-tools\adb.exe"
        "%ProgramFiles(x86)%\Android\android-sdk\platform-tools\adb.exe"
    ) do (
        if exist "%%~P" if not defined ADB_EXE set "ADB_EXE=%%~P"
    )
)
if not defined ADB_EXE (
    echo ERROR: no se encontro adb.exe.
    echo Instale Android Studio / platform-tools, o agregue platform-tools al PATH,
    echo o defina la variable ANDROID_HOME.
    pause
    exit /b 1
)
echo Usando adb: %ADB_EXE%
echo Configurando tunel USB para la API (puerto 5000)...
"%ADB_EXE%" reverse tcp:5000 tcp:5000
if errorlevel 1 (
    echo ERROR: no se pudo configurar adb reverse.
    echo Verifica que el celular este conectado por USB y con depuracion activada.
    pause
    exit /b 1
)
"%ADB_EXE%" reverse --list
echo.
echo Listo. Comprueba que la API este encendida en http://localhost:5000
echo Prueba rapida en el PC: http://localhost:5000/api/Diagnostico/conexion
echo Luego abre AdmiCam en el celular y entra con ADMIN / ADMIN.
pause