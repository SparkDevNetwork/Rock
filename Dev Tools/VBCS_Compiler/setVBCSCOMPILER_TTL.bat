REM Sets vbcscompiler.exe time to live to 10 seconds. Dev environment defaults to 600 secs which can cause issues when switching branches
REM In a production environment, it will be 10 seconds, even without the VBCSCOMPILER_TTL environment variable
setx VBCSCOMPILER_TTL 10