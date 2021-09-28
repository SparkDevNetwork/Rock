RMDIR "DotLiquid\Bin" /S /Q
RMDIR "DotLiquid\Obj" /S /Q

RMDIR "Rock\Bin" /S /Q
RMDIR "Rock\Obj" /S /Q

RMDIR "Rock.Checkr\Bin" /S /Q
RMDIR "Rock.Checkr\Obj" /S /Q

RMDIR "Rock.Client\Bin" /S /Q
RMDIR "Rock.Client\Obj" /S /Q

RMDIR "Rock.CodeGeneration\Bin" /S /Q
RMDIR "Rock.CodeGeneration\Obj" /S /Q

RMDIR "Rock.Common\Bin" /S /Q
RMDIR "Rock.Common\Obj" /S /Q

RMDIR "Rock.Common.Web\Bin" /S /Q
RMDIR "Rock.Common.Web\Obj" /S /Q

RMDIR "Rock.DownhillCss\Bin" /S /Q
RMDIR "Rock.DownhillCss\Obj" /S /Q

RMDIR "Rock.Lava\Bin" /S /Q
RMDIR "Rock.Lava\Obj" /S /Q

RMDIR "Rock.Lava.Shared\Bin" /S /Q
RMDIR "Rock.Lava.Shared\Obj" /S /Q

RMDIR "Rock.Mailgun\Bin" /S /Q
RMDIR "Rock.Mailgun\Obj" /S /Q

RMDIR "Rock.Mandrill\Bin" /S /Q
RMDIR "Rock.Mandrill\Obj" /S /Q

RMDIR "Rock.Migrations\Bin" /S /Q
RMDIR "Rock.Migrations\Obj" /S /Q

RMDIR "Rock.MyWell\Bin" /S /Q
RMDIR "Rock.MyWell\Obj" /S /Q

RMDIR "Rock.NMI\Bin" /S /Q
RMDIR "Rock.NMI\Obj" /S /Q

RMDIR "Rock.Oidc\Bin" /S /Q
RMDIR "Rock.Oidc\Obj" /S /Q

RMDIR "Rock.PayFlowPro\Bin" /S /Q
RMDIR "Rock.PayFlowPro\Obj" /S /Q

RMDIR "Rock.Plugin\Bin" /S /Q
RMDIR "Rock.Plugin\Obj" /S /Q

RMDIR "Rock.RestClient\Bin" /S /Q
RMDIR "Rock.RestClient\Obj" /S /Q

RMDIR "Rock.Rest\Bin" /S /Q
RMDIR "Rock.Rest\Obj" /S /Q

RMDIR "Rock.Rest\Bin" /S /Q
RMDIR "Rock.Rest\Obj" /S /Q

RMDIR "Rock.Security.Authentication.Auth0\Bin" /S /Q
RMDIR "Rock.Security.Authentication.Auth0\Obj" /S /Q

RMDIR "Rock.SendGrid\Bin" /S /Q
RMDIR "Rock.SendGrid\Obj" /S /Q

RMDIR "Rock.SignNow\Bin" /S /Q
RMDIR "Rock.SignNow\Obj" /S /Q

RMDIR "Rock.Slingshot.Model\Bin" /S /Q
RMDIR "Rock.Slingshot.Model\Obj" /S /Q

RMDIR "Rock.Slingshot\Bin" /S /Q
RMDIR "Rock.Slingshot\Obj" /S /Q

RMDIR "Rock.Specs\Bin" /S /Q
RMDIR "Rock.Specs\Obj" /S /Q

RMDIR "Rock.StatementGenerator\Bin" /S /Q
RMDIR "Rock.StatementGenerator\Obj" /S /Q

RMDIR "Rock.Tests.Integration\Bin" /S /Q
RMDIR "Rock.Tests.Integration\Obj" /S /Q

RMDIR "Rock.Tests.Shared\Bin" /S /Q
RMDIR "Rock.Tests.Shared\Obj" /S /Q

RMDIR "Rock.Tests.UnitTests\Bin" /S /Q
RMDIR "Rock.Tests.UnitTests\Obj" /S /Q

RMDIR "Rock.Tests\Bin" /S /Q
RMDIR "Rock.Tests\Obj" /S /Q

RMDIR "Rock.Version\Bin" /S /Q
RMDIR "Rock.Version\Obj" /S /Q

RMDIR "Rock.WebStartup\Bin" /S /Q
RMDIR "Rock.WebStartup\Obj" /S /Q

RMDIR "SignNowSDK\Bin" /S /Q
RMDIR "SignNowSDK\Obj" /S /Q

RMDIR "Applications\Wpf\CheckScannerUtility\Bin" /S /Q
RMDIR "Applications\Wpf\CheckScannerUtility\Obj" /S /Q

RMDIR "Applications\Wpf\StatementGenerator\Bin" /S /Q
RMDIR "Applications\Wpf\StatementGenerator\Obj" /S /Q

RMDIR "Applications\Wpf\Rock.Wpf\Bin" /S /Q
RMDIR "Applications\Wpf\Rock.Wpf\Obj" /S /Q



REN RockWeb\Bin\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.dll Microsoft.CodeDom.Providers.DotNetCompilerPlatform.bak
REN RockWeb\Bin\Rock.Common.Mobile.dll Rock.Common.Mobile.bak

DEL "RockWeb\Bin\*.dll"
DEL "RockWeb\Bin\*.pdb"
DEL "RockWeb\Bin\*.xml"

REN RockWeb\Bin\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.bak Microsoft.CodeDom.Providers.DotNetCompilerPlatform.dll
REN RockWeb\Bin\Rock.Common.Mobile.bak Rock.Common.Mobile.dll

RMDIR "Applications\Wpf\packages" /S /Q

FOR /D %%f in (packages\*) DO RMDIR %%f /S /Q