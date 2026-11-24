@echo off
REM ============================================================================
REM CAMBIO (24-11-2026): script de un clic para recrear el tunel USB de la API.
REM Motivo: la app Kotlin usa http://127.0.0.1:5000/ (api.url en gradle.properties).
REM En un celular fisico esa direccion solo funciona mientras exista la regla
REM `adb reverse tcp:5000 tcp:5000`, que se pierde al desconectar el cable o al
REM reiniciar el telefono. Este .bat la restaura sin tocar la API ni el codigo.
REM ============================================================================
cd /d "%~dp0"

echo Configurando tunel USB para la API (puerto 5000)...
adb reverse tcp:5000 tcp:5000
if errorlevel 1 (
    echo ERROR: no se pudo configurar adb reverse.
    echo Verifica que el celular este conectado por USB y con depuracion activada.
    pause
    exit /b 1
)

adb reverse --list
echo.
echo Listo. Comprueba que la API este encendida en http://localhost:5000
echo Prueba rapida en el PC: http://localhost:5000/api/Diagnostico/conexion
echo Luego abre AdmiCam en el celular y entra con ADMIN / ADMIN.
pause
