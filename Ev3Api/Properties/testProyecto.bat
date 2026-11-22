@echo off
setlocal enabledelayedexpansion
title TEST PROYECTO Ev3Api
color 0A

echo ============================================================
echo   TEST PROYECTO Ev3Api - 11 comandos de verificacion
echo ============================================================

REM Localiza el .csproj en la carpeta actual o en subcarpetas
set "PROJ_DIR="
for /f "delims=" %%f in ('dir /b /s *.csproj 2^>nul') do (
    set "PROJ_DIR=%%~dpf"
    goto :encontrado
)
echo ERROR: No se encontro ningun .csproj en esta carpeta ni en subcarpetas.
echo Ejecuta este .bat desde la carpeta del proyecto o su carpeta padre.
pause
exit /b 1

:encontrado
cd /d "%PROJ_DIR%"
echo Proyecto detectado en: %PROJ_DIR%
echo.

echo [1/11] Verificando SDK de .NET...
dotnet --version
if errorlevel 1 (
    echo ERROR: .NET SDK no instalado.
    pause
    exit /b 1
)

echo.
echo [2/11] Restaurando paquetes NuGet...
dotnet restore
if errorlevel 1 (
    echo ERROR: Fallo restore.
    pause
    exit /b 1
)

echo.
echo [3/11] Compilando proyecto...
dotnet build -c Release --no-restore
if errorlevel 1 (
    echo ERROR: Fallo compilacion.
    pause
    exit /b 1
)

echo.
echo [4/11] Iniciando API en http://localhost:5000 ...
start "Ev3Api" /B dotnet run --no-build -c Release --urls "http://localhost:5000"
echo Esperando 12 segundos a que arranque...
timeout /t 12 /nobreak >nul

echo.
echo [5/11] Test login exitoso (ADMIN/ADMIN)...
powershell -NoProfile -Command "$b = @{usuario='ADMIN';password='ADMIN'} | ConvertTo-Json; try { $r = Invoke-RestMethod -Uri 'http://localhost:5000/auth/login' -Method POST -Body $b -ContentType 'application/json'; Write-Host '   OK:' $r.mensaje -ForegroundColor Green } catch { Write-Host '   ERROR:' $_.Exception.Message -ForegroundColor Red }"

echo.
echo [6/11] Test login fallido (credenciales malas)...
powershell -NoProfile -Command "$b = @{usuario='ADMIN';password='XXX'} | ConvertTo-Json; try { $r = Invoke-RestMethod -Uri 'http://localhost:5000/auth/login' -Method POST -Body $b -ContentType 'application/json'; Write-Host '   FALLO: deberia haber dado 401' -ForegroundColor Red } catch { Write-Host '   OK (esperado 401):' $_.Exception.Message -ForegroundColor Green }"

echo.
echo [7/11] Test login usuario INACTIVO (A/1)...
powershell -NoProfile -Command "$b = @{usuario='A';password='1'} | ConvertTo-Json; try { $r = Invoke-RestMethod -Uri 'http://localhost:5000/auth/login' -Method POST -Body $b -ContentType 'application/json'; Write-Host '   FALLO: deberia haber dado 403' -ForegroundColor Red } catch { Write-Host '   OK (esperado 403):' $_.Exception.Message -ForegroundColor Green }"

echo.
echo [8/11] Obteniendo token y guardandolo en token.txt...
powershell -NoProfile -Command "$b = @{usuario='ADMIN';password='ADMIN'} | ConvertTo-Json; $r = Invoke-RestMethod -Uri 'http://localhost:5000/auth/login' -Method POST -Body $b -ContentType 'application/json'; $r.data.token | Out-File -FilePath 'token.txt' -NoNewline -Encoding ascii; Write-Host '   Token guardado (' $r.data.token.Length ' caracteres)' -ForegroundColor Green"

echo.
echo [9/11] GET /usuarios (con token)...
powershell -NoProfile -Command "$t = Get-Content 'token.txt' -Raw; try { $r = Invoke-RestMethod -Uri 'http://localhost:5000/usuarios' -Headers @{Authorization='Bearer '+$t}; Write-Host '   OK: ' $r.data.Count ' usuarios encontrados' -ForegroundColor Green } catch { Write-Host '   ERROR:' $_.Exception.Message -ForegroundColor Red }"

echo.
echo [10/11] GET /choferes, /plantas, /camiones, /salidas...
powershell -NoProfile -Command "$t = Get-Content 'token.txt' -Raw; $h = @{Authorization='Bearer '+$t}; foreach ($ep in @('choferes','plantas','camiones','salidas')) { try { $r = Invoke-RestMethod -Uri ('http://localhost:5000/'+$ep) -Headers $h; Write-Host ('   OK '+$ep+': '+$r.data.Count+' registros') -ForegroundColor Green } catch { Write-Host ('   ERROR '+$ep+': '+$_.Exception.Message) -ForegroundColor Red } }"

echo.
echo [11/11] Deteniendo API y limpiando...
taskkill /FI "WINDOWTITLE eq Ev3Api*" /F >nul 2>&1
taskkill /IM dotnet.exe /F >nul 2>&1
del token.txt >nul 2>&1

echo.
echo ============================================================
echo   TESTS COMPLETADOS
echo ============================================================
pause