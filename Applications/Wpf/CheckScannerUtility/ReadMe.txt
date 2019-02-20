1. The application is Dependant on an OCX and having the the Drivers for the Range Scanner installed
The post build of the project

2. call "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\Tools\VsDevCmd.bat"
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\VC\Tools\MSVC\14.16.27023\bin\Hostx86\x86\editbin.exe" /NXCOMPAT:NO "$(TargetPath)"
is Dependant on VsDevCmd.Bat (Visual Studio Dev Command)
This may need to be installed if it does not exists within the defined path
