@echo off
REM ============================================================================
REM CAMBIO (24-11-2026): script para cargar Swagger de la API Ev3Api.
REM Motivo: Swagger solo responde mientras la API está levantada. Este .bat
REM revisa si el puerto 5000 ya está activo; si no, inicia la API con
REM correrApi.bat en otra ventana, espera a que responda y abre el navegador.
REM No modifica ni borra código: solo detecta, espera y abre la URL.
REM ============================================================================
setlocal
cd /d "%~dp0"

echo Revisando si la API ya esta activa en el puerto 5000...
netstat -ano | findstr ":5000" >nul
if not errorlevel 1 (
    echo La API ya esta corriendo. Abriendo Swagger...
    goto :abrir
)

echo La API no esta corriendo. Iniciandola con correrApi.bat en otra ventana...
start "Ev3Api" "%~dp0correrApi.bat"

set /a INTENTOS=0
:esperar
set /a INTENTOS+=1
echo Esperando a que la API responda... intento %INTENTOS%
curl.exe -s -o NUL --max-time 2 http://127.0.0.1:5000/api/Diagnostico/conexion
if not errorlevel 1 goto :abrir
if %INTENTOS% GEQ 25 goto :fallo
timeout /t 2 /nobreak >nul
goto :esperar

:abrir
echo.
echo Abriendo Swagger: http://localhost:5000/swagger
start "" "http://localhost:5000/swagger"
echo.
echo Listo. Para detener la API, ve a la ventana "Ev3Api" y presiona CTRL + C.
pause
exit /b 0

:fallo
echo.
echo ERROR: la API no respondio en el puerto 5000 despues de esperar.
echo Revisa la ventana "Ev3Api" para ver el error de compilacion o arranque.

pause
exit /b 1
