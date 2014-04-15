
::
:: Script to build Rock documentation via Sandcastle 
::
set DXROOT=c:\Program Files (x86)\EWSoftware\Sandcastle Help File Builder
set SHFBROOT=%DXROOT%
set LANGUAGE=%SHFBROOT%\SandcastleBuilderGUI.exe

%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe Build\Rock.shfbproj
