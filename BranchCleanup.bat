RMDIR "DotLiquid\Bin" /S /Q
RMDIR "DotLiquid\Obj" /S /Q

RMDIR "Rock.Checkr\Bin" /S /Q
RMDIR "Rock.Checkr\Obj" /S /Q

RMDIR "Rock.Client\Bin" /S /Q
RMDIR "Rock.Client\Obj" /S /Q

RMDIR "Rock.CodeGeneration\Bin" /S /Q
RMDIR "Rock.CodeGeneration\Obj" /S /Q

RMDIR "Rock.DownhillCss\Bin" /S /Q
RMDIR "Rock.DownhillCss\Obj" /S /Q

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

DEL "RockWeb\Bin\*.dll"
DEL "RockWeb\Bin\*.pdb"
DEL "RockWeb\Bin\*.xml"

FOR /D %%f in (packages\*) DO RMDIR %%f /S /Q