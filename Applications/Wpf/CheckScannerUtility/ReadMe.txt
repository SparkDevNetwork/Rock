/*  2019-02-07 MDP. Last updated 2020-06-20 MDP

  This application is Dependant on an OCX and having the Drivers for the Range Scanner, and MagTek drivers installed.
  So you'll need to
    - install all the stuff in the Rock\Applications\Wpf\CheckScannerUtility\Pre-Requisites folder.

  You'll also need add some C++ Visual Studio tools to run a post build step that is needed to avoid some compatibility issues with the MagTek driver.
  It is very old and throws obscure exceptions if we don't tweak the exe a little with the editbin.exe tool that is included in the C++ tools for Visual Studio
  So, you'll also need to
    - Install the required C++ tools, use the Visual Studio Installer and install 'C++ MFC 14.25.28610 latest tools' (or whatever looks closest to that)

  Now, re-open the solution and correct any compile errors from code Gen objects that are not included in the project.

  If any unexplained compile errors, try turning Visual Studio off and on again

  The above is needed for the postbuild step of the CheckScannerUtility project, which looks like this

call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\Common7\Tools\VsDevCmd.bat"
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\VC\Tools\MSVC\14.25.28610\bin\Hostx86\x86\editbin.exe" /NXCOMPAT:NO "$(TargetPath)""

(If the path of this needs to be changed, feel free to adjust it)

*/
