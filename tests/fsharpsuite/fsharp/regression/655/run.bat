if "%_echo%"=="" echo off

setlocal
dir build.ok > NUL ) || (
  @echo 'build.ok' not found.
  goto :ERROR
)

call %~d0%~p0..\..\..\config.bat

if not exist test.exe goto SetError

if exist test.ok (del /f /q test.ok)
%CLIX% test.exe
if ERRORLEVEL 1 goto Error
if NOT EXIST test.ok goto SetError


:Ok
echo Ran fsharp %~f0 ok.
endlocal
exit /b 0

:Skip
echo Skipped %~f0
endlocal
exit /b 0


:Error
echo Test Script Failed (perhaps test did not emit test.ok signal file?)
call %SCRIPT_ROOT%\ChompErr.bat %ERRORLEVEL% %~f0
endlocal
exit /b %ERRORLEVEL%

:SetError
set NonexistentErrorLevel 2> nul
goto Error
