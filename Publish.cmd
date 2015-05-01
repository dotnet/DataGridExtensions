SET VERSION=1.0.23.0

PUSHD %~dp0\Deploy

CALL :PUSH DataGridExtensions

PAUSE
GOTO :EOF

:PUSH
nuget push %1.%VERSION%.nupkg
GOTO :EOF