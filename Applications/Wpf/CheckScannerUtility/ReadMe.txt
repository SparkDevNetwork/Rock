/*  2019-02-07 MDP. 2020-06-20 MDP. Last Updated 2024-06-24 JDR.

  This application is Dependant on an OCX and having the Drivers for the Range Scanner, and MagTek drivers installed.
  You'll need to do the following:

    	- Navigate to the ~\Rock\Applications\Wpf\CheckScannerUtility\Pre-Requisites folder.
	- Open the 'Install Canon RangerOCX' folder. 
	- Double-click the install file (as of this update it is called '50_80DRR_V2851')
	- Once it is complete, go back to the Pre-Requisite folder and open 'Install Image safe Drivers' > 'ExcellaSTXImageSafeInstall' and double-click the 'setup' file to install.
	- Once that is complete, go back to the Pre-Requisite folder and open 'Install MagTek MTMICRImageOCX' and double-click the 'setup' file to install.

  You'll also need add some C++ Visual Studio tools to run a post build step that is needed to avoid some compatibility issues with the MagTek driver.
  It is very old and throws obscure exceptions if we don't tweak the exe a little with the editbin.exe tool that is included in the C++ tools for Visual Studio.
  You'll need to do the following:

    	- Install the required C++ tools > Open your Visual Studio Installer
	- Click 'Modify' on the version you're using (tested on VS 2022, but should work with VS 2019 as well)
	- Select the 'Individual Components' tab
	- Search for 'MSVC v'
	- You should see an option that looks like this - 'MSVC v143 - VS {your version} C++ x64/x86 build tools'
	- NOTE: MSVC v143 is the latest stable version as of 2024-06-24, when you install it might be a different version.

  Now, re-open the solution and correct any compile errors from Code Gen objects that are not included in the project.

  If any unexplained compile errors, try exiting Visual Studio and open it again.

  The above is needed for the postbuild step of the CheckScannerUtility project, which looks like this (old example):


	call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\Common7\Tools\VsDevCmd.bat"
	"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\VC\Tools\MSVC\14.25.28610\bin\Hostx86\x86\editbin.exe" /NXCOMPAT:NO "$(TargetPath)""

The build will fail if the path of this needs to be changed, so feel free to adjust it. You can do so by following these steps: 

	- Right-click on CheckScannerUtility > Properties
	- In the left nav, click 'Build Events' 
	- In the bottom text area you'll see the Post-build event command line, which looks like this:

	call "$(DevEnvDir)..\Tools\VsDevCmd.bat"
	"C:\Program Files\Microsoft Visual Studio\2022\Professional\VC\Tools\MSVC\14.40.33807\bin\Hostx86\x86\editbin.exe" /NXCOMPAT:NO "$(TargetPath)"

	The path here needs to match the actual location of 'editbin.exe'. Yours may look different than ours, as seen in the two examples provided above, so keep that in mind.

*/
