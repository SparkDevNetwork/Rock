1. The application is Dependant on an OCX and having the Drivers for the Range Scanner, and MagTek drivers installed. So you'll need to install all the stuff in the Rock\Applications\Wpf\CheckScannerUtility\Pre-Requisites folder.
2. You'll also need add some C++ Visual Studio tools to run a post build step that is needed to avoid some compatibility issues with the MagTek driver. It is very old and throws obscure exceptions if we don't tweak the exe a little with the editbin.exe tool that is included in the C++ tools for Visual Studio
  To install the required C++ tools, use the Visual Studio Installer and install 'VC++ 2017 version 15.9 v14.16 latest tools' (or whatever looks closest to that)
3. Correct any compile errors from code Gen objects that are not included in the project.
4. Restart Visual Studio if there are unexplained compile errors.

The above is needed for the postbuild step of the CheckScannerUtility project, which looks like this

call "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\Tools\VsDevCmd.bat"
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\VC\Tools\MSVC\14.16.27023\bin\Hostx86\x86\editbin.exe" /NXCOMPAT:NO "$(TargetPath)"
