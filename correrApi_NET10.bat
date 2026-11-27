@echo off
setlocal
 title Ev3Api - inicio compatible con .NET 10
echo =============================================
echo   Ev3Api - inicio con runtime .NET 10
echo =============================================
echo.
set "API_DIR=%~dp0Ev3Api"
if not exist "%API_DIR%\Ev3Api.csproj" (
  if exist "%~dp0admiCam-main\Ev3Api\Ev3Api.csproj" (
    set "API_DIR=%~dp0admiCam-main\Ev3Api"
  )
)
if not exist "%API_DIR%\Ev3Api.csproj" (
  echo ERROR: No se encontro Ev3Api\Ev3Api.csproj.
  echo Pon este .bat en la misma carpeta que correrApi.bat, al lado de Ev3Api.
  goto :fallo
)
where dotnet >nul 2>&1
if errorlevel 1 (
  echo ERROR: No se encontro el comando dotnet.
  echo Comprueba que el SDK de .NET este instalado.
  goto :fallo
)
echo Runtimes instalados:
dotnet --list-runtimes
echo.
echo Proyecto: "%API_DIR%\Ev3Api.csproj"
echo Direccion prevista: http://localhost:5000
echo.
set "DOTNET_ROLL_FORWARD=Major"
pushd "%API_DIR%"
if errorlevel 1 goto :fallo
dotnet run --launch-profile http
set "CODIGO_SALIDA=%errorlevel%"
popd
echo.
if not "%CODIGO_SALIDA%"=="0" (
  echo La API no se pudo iniciar. Revisa el error mostrado arriba.
  echo Si falta Microsoft.AspNetCore.App, comprueba dotnet --list-runtimes.
) else (
  echo La API se detuvo.
)
echo Presiona cualquier tecla para cerrar.
pause >nul
exit /b %CODIGO_SALIDA%
:fallo
echo.
pause
exit /b 1