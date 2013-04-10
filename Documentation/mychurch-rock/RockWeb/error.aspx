<%@ Page Language="C#" AutoEventWireup="true" CodeFile="error.aspx.cs" Inherits="error" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <title>Oops...</title>

    <link rel="stylesheet" href="<%= Page.ResolveUrl("~/Themes/RockChMS/Css/rock-boot.less") %>" />
    <link rel="stylesheet" href="<%= Page.ResolveUrl("~/Themes/RockChMS/Css/rock-boot-responsive.less") %>" />
    <link rel="stylesheet" href="<%= Page.ResolveUrl("~/Themes/RockChMS/Css/rock-theme.less") %>" />

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
<body id="splash">
    <form id="form1" runat="server">
    
        
        <div id="content">
            <img alt="Rock ChMS" id="logoImg" runat="server" class="pageheader-logo" src="#" />

            <div id="content-box">
                <div class="row-fluid">
                    <div class="span12">
                        <div class="error-wrap">
                            <h1>Ah Man... An Error Occurred...</h1>
                            <p>An error has occurred while processing your request.  The Rock ChMS administrators have 
                            been notified of this problem.</p>

                            <p><a onclick="history.go(-1);" class="btn small">Go Back</a></p>

                            <asp:Literal ID="lErrorInfo" runat="server"></asp:Literal>
                        </div>
                    </div>
                </div>
            </div>
        </div>

    </form>
</body>
</html>
