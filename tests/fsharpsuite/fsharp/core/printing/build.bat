if "%_echo%"=="" echo off

setlocal
REM Configure the sample, i.e. where to find the F# compiler and C# compiler.
if EXIST build.ok DEL /f /q build.ok

call %~d0%~p0..\..\..\config.bat
@if ERRORLEVEL 1 goto Error

if NOT "%FSC:NOTAVAIL=X%" == "%FSC%" ( 
  ECHO Skipping test for FSI.EXE
  goto Skip
)

rem recall  >fred.txt 2>&1 merges stderr into the stdout redirect
rem however 2>&1  >fred.txt did not seem to do it.

REM Here we use diff.exe without -dew option to trap whitespace changes, like bug 4429.
REM Any whitespace change needs to be investigated, these tests are to check exact output.
REM Base line updates are easy: sd edit and delete the .bsl and rerun the test.
set PRDIFF=%~d0%~p0..\..\..\fsharpqa\testenv\bin\%processor_architecture%\diff.exe
echo Diff tool is %PRDIFF%
if NOT EXIST %PRDIFF% (
    echo ERROR: Diff tool not found at %PRDIFF%
    exit /b 1
)

echo == Plain
"%FSI%" %fsc_flags_errors_ok%  --nologo                                    <test.fsx >z.raw.output.test.default.txt 2>&1
echo == PrintSize 1000
"%FSI%" %fsc_flags_errors_ok%  --nologo --use:preludePrintSize1000.fsx     <test.fsx >z.raw.output.test.1000.txt    2>&1 
echo == PrintSize 200
"%FSI%" %fsc_flags_errors_ok%  --nologo --use:preludePrintSize200.fsx      <test.fsx >z.raw.output.test.200.txt     2>&1 
echo == ShowDeclarationValues off
"%FSI%" %fsc_flags_errors_ok%  --nologo --use:preludeShowDeclarationValuesFalse.fsx <test.fsx >z.raw.output.test.off.txt     2>&1
echo == Quiet
"%FSI%" %fsc_flags_errors_ok% --nologo --quiet                              <test.fsx >z.raw.output.test.quiet.txt   2>&1

REM REVIEW: want to normalise CWD paths, not suppress them. 
findstr /v "%CD%" z.raw.output.test.default.txt | findstr /v -C:"--help' for options" > z.output.test.default.txt
findstr /v "%CD%" z.raw.output.test.1000.txt    | findstr /v -C:"--help' for options" > z.output.test.1000.txt
findstr /v "%CD%" z.raw.output.test.200.txt     | findstr /v -C:"--help' for options" > z.output.test.200.txt
findstr /v "%CD%" z.raw.output.test.off.txt     | findstr /v -C:"--help' for options" > z.output.test.off.txt
findstr /v "%CD%" z.raw.output.test.quiet.txt   | findstr /v -C:"--help' for options" > z.output.test.quiet.txt


if NOT EXIST z.output.test.default.bsl COPY z.output.test.default.txt z.output.test.default.bsl
if NOT EXIST z.output.test.off.bsl     COPY z.output.test.off.txt     z.output.test.off.bsl
if NOT EXIST z.output.test.1000.bsl    COPY z.output.test.1000.txt    z.output.test.1000.bsl
if NOT EXIST z.output.test.200.bsl     COPY z.output.test.200.txt     z.output.test.200.bsl
if NOT EXIST z.output.test.quiet.bsl   COPY z.output.test.quiet.txt   z.output.test.quiet.bsl

%PRDIFF% z.output.test.default.txt z.output.test.default.bsl > z.output.test.default.diff
%PRDIFF% z.output.test.off.txt     z.output.test.off.bsl     > z.output.test.off.diff
%PRDIFF% z.output.test.1000.txt    z.output.test.1000.bsl    > z.output.test.1000.diff
%PRDIFF% z.output.test.200.txt     z.output.test.200.bsl     > z.output.test.200.diff
%PRDIFF% z.output.test.quiet.txt   z.output.test.quiet.bsl   > z.output.test.quiet.diff

echo ======== Differences From ========
TYPE  z.output.test.default.diff
TYPE  z.output.test.off.diff
TYPE  z.output.test.1000.diff
TYPE  z.output.test.200.diff
TYPE  z.output.test.quiet.diff
echo ========= Differences To =========

TYPE  z.output.test.default.diff  > zz.alldiffs
TYPE  z.output.test.off.diff     >> zz.alldiffs
TYPE  z.output.test.1000.diff    >> zz.alldiffs
TYPE  z.output.test.200.diff     >> zz.alldiffs
TYPE  z.output.test.quiet.diff   >> zz.alldiffs

for /f %%c IN (zz.alldiffs) do (
  echo NOTE -------------------------------------
  echo NOTE ---------- THERE ARE DIFFs ----------
  echo NOTE -------------------------------------
  echo .
  echo To update baselines: "sd edit *bsl", "del *bsl", "build.bat" regenerates bsl, "sd diff ...", check what changed.
  goto Error
)

:Ok
echo Built fsharp %~f0 ok.
echo. > build.ok
endlocal
exit /b 0

:Skip
echo Skipped %~f0
endlocal
exit /b 0


:Error
rem Hardwire ERRORLEVEL to be 1, since routes in here from diff check do not have ERRORLEVEL set
rem call %SCRIPT_ROOT%\ChompErr.bat %ERRORLEVEL% %~f0
call %SCRIPT_ROOT%\ChompErr.bat 1 %~f0
endlocal
exit /b %ERRORLEVEL%
