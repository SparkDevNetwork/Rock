<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Http404Error.aspx.cs" Inherits="Http404Error" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <title>Rock - Page Not Found</title>

    <link rel="stylesheet" href="<%= Page.ResolveUrl("~/Themes/Rock/Styles/bootstrap.css") %>" />
    <link rel="stylesheet" href="<%= Page.ResolveUrl("~/Themes/Rock/Styles/theme.css") %>" />

    <!-- Icons -->
    <link rel="shortcut icon" href="<%= Page.ResolveUrl("~/Assets/Icons/favicon.ico") %>"/>
    <link rel="apple-touch-icon-precomposed" sizes="144x144" href="<%= Page.ResolveUrl("~/Assets/Icons/touch-icon-ipad-retina.png") %>"/>
    <link rel="apple-touch-icon-precomposed" sizes="114x114" href="<%= Page.ResolveUrl("~/Assets/Icons/touch-icon-iphone-retina.png") %>"/>
    <link rel="apple-touch-icon-precomposed" sizes="72x72" href="<%= Page.ResolveUrl("~/Assets/Icons/touch-icon-ipad.png") %>"/>
    <link rel="apple-touch-icon-precomposed" href="<%= Page.ResolveUrl("~/Assets/Icons/touch-icon-iphone.png") %>"/>

    <script src="<%= ResolveUrl("~/Scripts/jquery-1.12.4.min.js") %>" ></script>

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

                        <div class="error-wrap">
                            <h1>We Can't Find That Page</h1>

                            <p class="error-icon info">
                                <i class="fa fa-question-circle"></i>
                            </p>

                            <p>
                                Sorry, but the page you are looking for can not be found. Check the address of the page
                                and see your administrator if you still need assistance.
                            </p>
                        </div>
                    </div>
                </div>
            </div>

        </div>

    </form>
</body>
</html>
