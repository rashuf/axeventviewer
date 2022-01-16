@ECHO OFF
cd %~dp0
del DaxEvents.exe.config
if %PROCESSOR_ARCHITECTURE%==AMD64 (	
	copy /y DaxEvents.exe.config-x64 DaxEvents.exe.config
) else (	
	copy /y DaxEvents.exe.config-x86 DaxEvents.exe.config
)