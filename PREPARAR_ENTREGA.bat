@echo off
setlocal EnableExtensions
title AdmiCam - Preparar entrega (API + celular USB)
cd /d "%~dp0"
set "RAIZ=%~dp0"
set "API_DIR=%RAIZ%Ev3Api"
set "ADB_EXE="
set "DISPOSITIVO="
set "ESTADO="
set "PRUEBA_OK=0"
echo.
echo ============================================================================
echo   AdmiCam - PREPARAR ENTREGA
echo   API http://localhost:5000  ^|  Celular conectado por USB
echo ============================================================================
echo.
where dotnet >nul 2>&1
if errorlevel 1 (
    echo [ERROR] No se encontro dotnet en el PATH.
    echo         Instale el SDK de .NET 8 o superior y vuelva a abrir este .bat.
    goto :fin_error
)
echo [1/6] dotnet encontrado.
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
    echo [ERROR] No se encontro adb.exe.
    echo         Instale Android Studio / platform-tools, o agregue la carpeta
    echo         platform-tools al PATH, o defina la variable ANDROID_HOME.
    goto :fin_error
)
echo [2/6] adb encontrado: %ADB_EXE%
for /f "skip=1 tokens=1,2" %%A in ('"%ADB_EXE%" devices 2^>nul') do (
    if "%%B"=="device" if not defined DISPOSITIVO set "DISPOSITIVO=%%A"
    if "%%B"=="unauthorized" set "ESTADO=unauthorized"
    if "%%B"=="offline" set "ESTADO=offline"
)
if not defined DISPOSITIVO (
    if "%ESTADO%"=="unauthorized" (
        echo [ERROR] El celular esta conectado pero la depuracion USB no fue autorizada.
        echo         Acepte en el celular el aviso "Permitir depuracion USB?" y reintente.
    ) else if "%ESTADO%"=="offline" (
        echo [ERROR] El celular aparece como offline. Desconecte y reconecte el cable.
    ) else (
        echo [ERROR] No hay ningun celular conectado por USB.
        echo         Conecte el cable y active Opciones de desarrollador + Depuracion USB.
    )
    goto :fin_error
)
echo [3/6] Celular detectado: %DISPOSITIVO%
powershell -NoProfile -Command "exit [int](-not [bool](Get-NetTCPConnection -LocalPort 5000 -State Listen -ErrorAction SilentlyContinue))" >nul 2>&1
if not errorlevel 1 (
    echo [4/6] La API ya esta escuchando en el puerto 5000.
) else (
    echo [4/6] Iniciando la API en una ventana nueva...
    set "DOTNET_ROLL_FORWARD=Major"
    start "Ev3Api - http://localhost:5000" /D "%API_DIR%" cmd /k dotnet run --launch-profile http
    echo        Esperando a que arranque...
    timeout /t 8 /nobreak >nul
)
echo [5/6] Creando el tunel USB (adb reverse tcp:5000 tcp:5000)...
"%ADB_EXE%" reverse --remove-all >nul 2>&1
"%ADB_EXE%" reverse tcp:5000 tcp:5000
if errorlevel 1 (
    echo [ERROR] No se pudo crear el tunel adb reverse.
    goto :fin_error
)
echo        Reglas activas:
"%ADB_EXE%" reverse --list
echo [6/6] Verificando la conexion desde el celular...
for /f "usebackq delims=" %%L in (`"%ADB_EXE%" shell curl -s -m 8 http://127.0.0.1:5000/api/Camiones/listarPlantas 2^>nul`) do (
    echo(%%L | findstr /c:"resultado" >nul
    if not errorlevel 1 set "PRUEBA_OK=1"
)
if "%PRUEBA_OK%"=="1" (
    echo        OK: la API responde desde el celular.
) else (
    echo        AVISO: no se pudo comprobar con curl desde el celular.
    echo        Prueba abriendo AdmiCam e iniciando sesion con ADMIN / ADMIN.
)
"%ADB_EXE%" shell pm path com.example.admicam >nul 2>&1
if not errorlevel 1 (
    echo        Abriendo AdmiCam en el celular...
    "%ADB_EXE%" shell monkey -p com.example.admicam -c android.intent.category.LAUNCHER 1 >nul 2>&1
) else (
    echo        La app aun no esta instalada: instalarla desde Android Studio.
)
echo.
echo ============================================================================
echo   LISTO
echo   - API:      http://localhost:5000  (Swagger: /swagger)
echo   - Celular:  127.0.0.1:5000 del telefono = PC por USB
echo   - Login:    ADMIN / ADMIN
echo   Si desconectas el cable o reinicias el celular, ejecuta este .bat de nuevo.
echo ============================================================================
goto :fin_ok
:fin_error
echo.
echo ----------------------------------------------------------------------------
echo   No se pudo completar. Revisa los mensajes marcados como [ERROR].
echo ----------------------------------------------------------------------------
echo.
pause
exit /b 1
:fin_ok
echo.
pause
exit /b 0