<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Error.aspx.cs" Inherits="RockWeb.Error" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <title>Rock - Error</title>

    <link rel="stylesheet" href="<%= Page.ResolveUrl("~/Themes/Rock/Styles/bootstrap.css") %>" />
    <link rel="stylesheet" href="<%= Page.ResolveUrl("~/Themes/Rock/Styles/theme.css") %>" />

    <!-- Icons -->
    <link rel="shortcut icon" href="<%= Page.ResolveUrl("~/Assets/Icons/favicon.ico") %>">
    <link rel="apple-touch-icon-precomposed" sizes="144x144" href="<%= Page.ResolveUrl("~/Assets/Icons/touch-icon-ipad-retina.png") %>">
    <link rel="apple-touch-icon-precomposed" sizes="114x114" href="<%= Page.ResolveUrl("~/Assets/Icons/touch-icon-iphone-retina.png") %>">
    <link rel="apple-touch-icon-precomposed" sizes="72x72" href="<%= Page.ResolveUrl("~/Assets/Icons/touch-icon-ipad.png") %>">
    <link rel="apple-touch-icon-precomposed" href="<%= Page.ResolveUrl("~/Assets/Icons/touch-icon-iphone.png") %>">

    <script src="<%= ResolveUrl("~/Scripts/jquery-1.10.2.min.js") %>" >
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
            
            <div id="logo">
                <asp:Literal ID="lLogoSvg" runat="server" />
            </div>

            <div id="content-box">
                <div class="row">
                    <div class="col-md-12">
                        
                        <asp:Panel ID="pnlSecurity" runat="server" Visible="false">
                            <div class="error-wrap">
                                <h1>Hey...</h1>
                                <h3>We can't let you view this page...</h3>

                                <p class="error-icon danger">
                                    <i class="fa fa-lock"></i>
                                </p>

                                <p>Unfortunately, you are not authorized to view the page you requested. Please contact
                                    your Rock administrator if you need access to this resource.
                                </p>

                                <p><a onclick="history.go(-1);" class="btn btn-sm btn-primary">Go Back</a></p>
                            </div>
                        </asp:Panel>
                        
                        <asp:Panel ID="pnlException" runat="server" Visible="true">
                            <div class="error-wrap">
                                <h1>That Wasn't Supposed To Happen... </h1>
                            
                                <p class="error-icon warning">
                                    <i class="fa fa-exclamation-triangle"></i>
                                </p>

                                <p>An error has occurred while processing your request.  Your organization's administrators have 
                                been notified of this problem.</p>

                                <p><a onclick="history.go(-1);" class="btn btn-sm btn-primary">Go Back</a></p>
                            </div>

                            <div class="error-details">
                                <asp:Literal ID="lErrorInfo" runat="server"></asp:Literal>
                            </div>    
                        </asp:Panel>

                    </div>
                </div>
            </div>
        </div>

    </form>
</body>
</html>
