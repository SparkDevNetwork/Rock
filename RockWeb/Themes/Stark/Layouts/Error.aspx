<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Error.aspx.cs" Inherits="error" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <title>Rock ChMS - Error</title>

    <link rel="stylesheet" href="<%= Page.ResolveUrl("~/Themes/Stark/Styles/bootstrap.css") %>" />
    <link rel="stylesheet" href="<%= Page.ResolveUrl("~/Themes/Stark/Styles/theme.css") %>" />

    <script src="<%= ResolveUrl("~/Scripts/jquery.js") %>" >
    <script>
        $(document).ready(function () {
            $(".stack-trace").hide();
            
            //toggle the componenet with class msg_body
            $(".exception-type").click(function () {
                $(this).next(".stack-trace").slideToggle(500);
            });
        });
    </script>
</head>
<body id="splash" class="error">
    <form id="form1" runat="server">
    
        
        <div id="content">
            
            <div id="content-box">
                <div class="row">
                    <div class="col-md-12">
                        <div class="error-wrap">
                            <h1>Ah Man...</h1>
                            <p><strong>An Error Occurred...</strong>  The website administrators have 
                            been notified of this problem.</p>

                            <p><a onclick="history.go(-1);" class="btn btn-sm btn-primary">Go Back</a></p>

                            <asp:Literal ID="lErrorInfo" runat="server"></asp:Literal>
                        </div>
                    </div>
                </div>
            </div>
        </div>

    </form>
</body>
</html>
