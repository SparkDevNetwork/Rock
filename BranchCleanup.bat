DEL Rock\Bin\*.* /Q
DEL Rock\Obj\*.* /Q

DEL Rock.Mailgun\Bin\*.* /Q
DEL Rock.Mailgun\Obj\*.* /Q

DEL Rock.Mandrill\Bin\*.* /Q
DEL Rock.Mandrill\Obj\*.* /Q

DEL Rock.Migrations\Bin\*.* /Q
DEL Rock.Migrations\Obj\*.* /Q

DEL Rock.NMI\Bin\*.* /Q
DEL Rock.NMI\Obj\*.* /Q

DEL Rock.PayFlowPro\Bin\*.* /Q
DEL Rock.PayFlowPro\Obj\*.* /Q

DEL Rock.Rest\Bin\*.* /Q
DEL Rock.Rest\Obj\*.* /Q

DEL Rock.SignNow\Bin\*.* /Q
DEL Rock.SignNow\Obj\*.* /Q

FOR /D %%f in (packages\*) DO RMDIR %%f /S /Q