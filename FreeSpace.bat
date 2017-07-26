@echo off

del /s /q c:\windows\softwaredistribution\download\* 

del /s /q %temp%\*

dism.exe /online /Cleanup-Image /StartComponentCleanup
dism.exe /online /Cleanup-Image /StartComponentCleanup /ResetBase

echo When you press enter, a defrag will start on the C drive.  Hit Ctrl + C now to cancel.

pause

defrag c: