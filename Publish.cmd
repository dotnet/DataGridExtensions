IF /I NOT "%1" == "true" GOTO :EOF

PUSHD %~dp0\Deploy

CALL :PUSH DataGridExtensions

GOTO :EOF

:PUSH
"%~dp0.nuget\nuget" push %1.1.0.??.0.nupkg
GOTO :EOF