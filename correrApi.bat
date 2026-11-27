@echo off
setlocal
cd /d "%~dp0Ev3Api"
echo [1/4] Verificando SDK de .NET...
dotnet --version
if errorlevel 1 (
    echo ERROR: no se encontro dotnet. Instale el SDK de .NET 8 o superior.
    pause
    exit /b 1
)
set "ADB_EXE="
where adb >nul 2>&1
if not errorlevel 1 set "ADB_EXE=adb"
if not defined ADB_EXE (
    for %%P in ("%ANDROID_HOME%\platform-tools\adb.exe" "%ANDROID_SDK_ROOT%\platform-tools\adb.exe" "%LOCALAPPDATA%\Android\Sdk\platform-tools\adb.exe" "%USERPROFILE%\AppData\Local\Android\Sdk\platform-tools\adb.exe" "C:\Android\Sdk\platform-tools\adb.exe") do (
        if exist "%%~P" if not defined ADB_EXE set "ADB_EXE=%%~P"
    )
)
echo [2/4] Verificando que el puerto 5000 este libre...
netstat -ano | findstr ":5000" >nul
if not errorlevel 1 (
    echo AVISO: el puerto 5000 ya esta en uso.
    echo Si es otra instancia de la API, cierrala antes o liber el puerto con:
    echo   netstat -ano ^| findstr :5000
    echo   taskkill /PID ^<el_PID_que_aparece^> /F
)
if defined ADB_EXE (
    echo [3/4] Configurando tunel USB para el celular...
    "%ADB_EXE%" reverse tcp:5000 tcp:5000
    if errorlevel 1 (
        echo AVISO: no se pudo crear el tunel. Conecte el celular por USB con depuracion activada.
    ) else (
        "%ADB_EXE%" reverse --list
    )
) else (
    echo [3/4] adb no encontrado: el celular no podra conectar. Use PREPARAR_ENTREGA.bat.
)
echo [4/4] Iniciando API en http://localhost:5000 ...
echo Swagger: http://localhost:5000/swagger
echo Para detenerla presione CTRL + C en esta ventana.
dotnet run --launch-profile http
echo La API se detuvo.
pause
