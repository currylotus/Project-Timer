@echo off
setlocal EnableExtensions

set "HERE=%~dp0"
set "PROJ=%HERE%ProjectTimer.csproj"
set "CFG=Release"
set "DIST=%HERE%dist"

set "ACTION=%~1"
if not defined ACTION set "ACTION=build"

if /i "%ACTION%"=="help"    goto help
if /i "%ACTION%"=="clean"   goto clean
if /i "%ACTION%"=="build"   goto build
if /i "%ACTION%"=="run"     goto run
if /i "%ACTION%"=="publish" goto publish
goto usage

:exe
set "EXE="
set "EXEDIR="
for /d %%d in ("%HERE%bin\%CFG%\*") do set "EXEDIR=%%~fd"
if defined EXEDIR if exist "%EXEDIR%\ProjectTimer.exe" set "EXE=%EXEDIR%\ProjectTimer.exe"
goto :eof

:unlock
taskkill /f /im ProjectTimer.exe >nul 2>&1
ping -n 2 127.0.0.1 >nul 2>&1
goto :eof

:build
if not exist "%PROJ%" (
    echo.
    echo [X] ProjectTimer.csproj nicht gefunden: %PROJ%
    exit /b 1
)
call :unlock
echo.
echo Building Project Timer [%CFG%] ...
dotnet build "%PROJ%" -c %CFG% --nologo -v minimal -consoleloggerparameters:"Summary;ErrorsOnly"
if errorlevel 1 (
    echo.
    echo [X] Build fehlgeschlagen.
    exit /b 1
)
call :exe
if not defined EXE (
    echo.
    echo [X] ProjectTimer.exe nicht gefunden unter bin\%CFG%.
    exit /b 1
)
echo.
echo [OK] %EXE%
exit /b 0

:run
call "%~f0" build
if errorlevel 1 exit /b 1
call :exe
if not defined EXE exit /b 1
echo.
start "" "%EXE%"
exit /b 0

:clean
call :unlock
echo.
echo Cleaning ...
dotnet clean "%PROJ%" -c %CFG% --nologo -v minimal -consoleloggerparameters:"Summary;ErrorsOnly"
echo [OK] cleaned
exit /b 0

:publish
call "%~f0" build
if errorlevel 1 exit /b 1
echo.
echo Publishing single file nach dist ...
dotnet publish "%PROJ%" -c %CFG% -r win-x64 --self-contained false -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:BaseOutputPath="%HERE%obj/publish" -o "%DIST%" --nologo -v minimal -consoleloggerparameters:"Summary;ErrorsOnly"
if errorlevel 1 (
    echo.
    echo [X] Publish fehlgeschlagen.
    exit /b 1
)
echo.
echo [OK] %DIST%\ProjectTimer.exe
exit /b 0

:help
echo.
echo   build.bat [build^|run^|clean^|publish^|help]
echo.
echo     build    Release-Build ^(Standard^)
echo     run      Build, dann starten
echo     clean    Artefakte loeschen
echo     publish  Single-File-EXE nach dist\
echo.
exit /b 0

:usage
echo.
echo   build.bat [build^|run^|clean^|publish^|help]
echo.
echo   Unbekannte Aktion: %ACTION%
exit /b 2