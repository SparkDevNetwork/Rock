RMDIR "DotLiquid\Bin" /S /Q
RMDIR "DotLiquid\Obj" /S /Q

RMDIR "Rock\Bin" /S /Q
RMDIR "Rock\Obj" /S /Q

RMDIR "Rock.Mailgun\Bin" /S /Q
RMDIR "Rock.Mailgun\Obj" /S /Q

RMDIR "Rock.Mandrill\Bin" /S /Q
RMDIR "Rock.Mandrill\Obj" /S /Q

RMDIR "Rock.Migrations\Bin" /S /Q
RMDIR "Rock.Migrations\Obj" /S /Q

RMDIR "Rock.NMI\Bin" /S /Q
RMDIR "Rock.NMI\Obj" /S /Q

RMDIR "Rock.PayFlowPro\Bin" /S /Q
RMDIR "Rock.PayFlowPro\Obj" /S /Q

RMDIR "Rock.Rest\Bin" /S /Q
RMDIR "Rock.Rest\Obj" /S /Q

RMDIR "Rock.SignNow\Bin" /S /Q
RMDIR "Rock.SignNow\Obj" /S /Q

RMDIR "Rock.Slingshot\Bin" /S /Q
RMDIR "Rock.Slingshot\Obj" /S /Q

RMDIR "Rock.Slingshot.Model\Bin" /S /Q
RMDIR "Rock.Slingshot.Model\Obj" /S /Q

RMDIR "Rock.Version\Bin" /S /Q
RMDIR "Rock.Version\Obj" /S /Q

RMDIR "SignNowSDK\Bin" /S /Q
RMDIR "SignNowSDK\Obj" /S /Q

FOR /D %%f in (packages\*) DO RMDIR %%f /S /Q