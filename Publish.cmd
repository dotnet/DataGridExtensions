SET VERSION=1.0.33.0

PUSHD %~dp0\Deploy

CALL :PUSH DataGridExtensions

PAUSE
GOTO :EOF

:PUSH
..\.nuget\nuget push %1.%VERSION%.nupkg
GOTO :EOF