<%@ Page Language="C#" AutoEventWireup="true" CodeFile="error.aspx.cs" Inherits="error" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <title>Rock ChMS - Error</title>

    <link rel="stylesheet" href="<%= Page.ResolveUrl("~/Themes/RockChMS/Styles/bootstrap.css") %>" />
    <link rel="stylesheet" href="<%= Page.ResolveUrl("~/Themes/RockChMS/Styles/theme.css") %>" />

    <!-- Icons -->
    <link rel="shortcut icon" href="<%= Page.ResolveUrl("~/Assets/Icons/favicon.ico") %>">
    <link rel="apple-touch-icon-precomposed" sizes="144x144" href="<%= Page.ResolveUrl("~/Assets/Icons/touch-icon-ipad-retina.png") %>">
    <link rel="apple-touch-icon-precomposed" sizes="114x114" href="<%= Page.ResolveUrl("~/Assets/Icons/touch-icon-iphone-retina.png") %>">
    <link rel="apple-touch-icon-precomposed" sizes="72x72" href="<%= Page.ResolveUrl("~/Assets/Icons/touch-icon-ipad.png") %>">
    <link rel="apple-touch-icon-precomposed" href="<%= Page.ResolveUrl("~/Assets/Icons/touch-icon-iphone.png") %>">

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
            <img alt="Rock ChMS" id="logoImg" runat="server" class="pageheader-logo" src="#" />

            <div id="content-box">
                <div class="row">
                    <div class="col-md-12">
                        
                        <asp:Panel ID="pnlSecurity" runat="server" CssClass="error-wrap" Visible="false">
                            <h1>Hey...</h1>
                            <h3>We can't let you view this page...</h3>

                            <p>Unfortunately, you are not authorized to view the page you requested. Please contact
                                your Rock administrator if you need access to this resource.
                            </p>

                            <p style="text-align: center;">
                                <img src="<%= Page.ResolveUrl("~/Assets/Images/chip-angry.png") %>" />
                            </p>

                            <p><a onclick="history.go(-1);" class="btn btn-sm btn-primary">Go Back</a></p>

                        </asp:Panel>
                        
                        <asp:Panel ID="pnlException" runat="server" CssClass="error-wrap" Visible="false">
                            <h1>Ah Man... </h1>
                            <p>An error has occurred while processing your request.  The Rock ChMS administrators have 
                            been notified of this problem.</p>

                            <p style="text-align: center;">
                                <img src="<%= Page.ResolveUrl("~/Assets/Images/chip-shocked.png") %>" />
                            </p>

                            <p><a onclick="history.go(-1);" class="btn btn-sm btn-primary">Go Back</a></p>

                            <asp:Literal ID="lErrorInfo" runat="server"></asp:Literal>
                        </asp:Panel>

                    </div>
                </div>
            </div>
        </div>

    </form>
</body>
</html>
