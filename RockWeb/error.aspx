<%@ Page Language="C#" AutoEventWireup="true" CodeFile="error.aspx.cs" Inherits="error" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Oops...</title>
    <link rel="stylesheet" href="<%= Page.ResolveUrl("~/CSS/bootstrap.min.css") %>" />
    <link rel="stylesheet" href="<%= Page.ResolveUrl("~/CSS/bootstrap-responsive.min.css") %>" />
    <link rel="stylesheet" href="<%= Page.ResolveUrl("~/CSS/RockCore.css") %>" />
    <link rel="stylesheet" href="<%= Page.ResolveUrl("~/Themes/RockChMS/CSS/RockTheme.css") %>" />

    <script src="<%= ResolveUrl("~/Scripts/jquery-1.8.3.min.js") %>" >
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
            <h1>Rock ChMS</h1>
            <div id="content-box">
                <h1>Ah Man... An Error Occurred...</h1>
                <p>An error has occurred while processing your request.  The Rock ChMS administrators have 
                been notified of this problem.</p>

                <p><a onclick="history.go(-1);" class="btn small">Go Back</a></p>

                <asp:Literal ID="lErrorInfo" runat="server"></asp:Literal>
            </div>
        </div>

    </form>
</body>
</html>
