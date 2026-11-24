@echo off
REM ============================================================================
REM CAMBIO (24-11-2026): script para levantar la API sin abrir Visual Studio.
REM Motivo: el proyecto es .NET 8 y se ejecuta solo con el SDK desde terminal.
REM Se usa el perfil "http" de launchSettings.json (http://localhost:5000),
REM que es el puerto que consume la app Kotlin por el tunel USB (adb reverse).
REM Este .bat NO toca ni borra codigo: solo cambia de carpeta y llama a dotnet.
REM ============================================================================
setlocal
cd /d "%~dp0Ev3Api"

echo [1/3] Verificando SDK de .NET...
dotnet --version
if errorlevel 1 (
    echo ERROR: no se encontro dotnet. Instale el SDK de .NET 8 o superior.
    pause
    exit /b 1
)

echo.
echo [2/3] Verificando que el puerto 5000 este libre...
netstat -ano | findstr ":5000" >nul
if not errorlevel 1 (
    echo AVISO: el puerto 5000 ya esta en uso.
    echo Si es otra instancia de la API, cierrala antes o liber el puerto con:
    echo   netstat -ano ^| findstr :5000
    echo   taskkill /PID ^<el_PID_que_aparece^> /F
    echo.
)

echo [3/3] Iniciando API en http://localhost:5000 ...
echo Swagger: http://localhost:5000/swagger
echo Para detenerla presione CTRL + C en esta ventana.
echo.

REM Opcion rapida si ya estan restaurados los paquetes (sin restore):
REM dotnet run --launch-profile http --no-restore
dotnet run --launch-profile http

echo.
echo La API se detuvo.
pause
