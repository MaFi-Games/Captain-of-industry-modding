@echo off

if "%COI_ROOT%" == "" (
	echo Environmental variable COI_ROOT is not defined
	goto :eof
)

for %%s in ("Mafi.dll" "Mafi.Core.dll" "Mafi.Base.dll" "Mafi.Unity.dll", "Mafi.ModsAuthoringSupport.dll") do (
	IF EXIST "%~dp0\%%s" del "%~dp0\%%s"
	mklink "%~dp0\%%s" "%COI_ROOT%\Captain of Industry_Data\Managed\%%s"
)