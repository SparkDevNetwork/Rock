<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Http429Error.aspx.cs" Inherits="Http429Error" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Rock - Page Not Found</title>

    <link rel="stylesheet" href="<%= Page.ResolveUrl("~/Themes/Rock/Styles/bootstrap.css") %>" />
    <link rel="stylesheet" href="<%= Page.ResolveUrl("~/Themes/Rock/Styles/theme.css") %>" />

    <!-- Icons -->
    <link rel="shortcut icon" href="<%= Page.ResolveUrl("~/Assets/Icons/favicon.ico") %>"/>
    <link rel="apple-touch-icon-precomposed" sizes="144x144" href="<%= Page.ResolveUrl("~/Assets/Icons/touch-icon-ipad-retina.png") %>"/>
    <link rel="apple-touch-icon-precomposed" sizes="114x114" href="<%= Page.ResolveUrl("~/Assets/Icons/touch-icon-iphone-retina.png") %>"/>
    <link rel="apple-touch-icon-precomposed" sizes="72x72" href="<%= Page.ResolveUrl("~/Assets/Icons/touch-icon-ipad.png") %>"/>
    <link rel="apple-touch-icon-precomposed" href="<%= Page.ResolveUrl("~/Assets/Icons/touch-icon-iphone.png") %>"/>

    <script src="<%= System.Web.Optimization.Scripts.Url("~/Scripts/Bundles/RockJQueryLatest" ) %>" ></script>

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
                            <h1>Too many requests to page.</h1>

                            <p class="error-icon info">
                                <i class="fa fa-stopwatch"></i>
                            </p>

                            <p>
                                Sorry, but this page has received too many requests. 
                                See your administrator if you still need assistance.
                            </p>
                        </div>
                    </div>
                </div>
            </div>

        </div>

    </form>
</body>
</html>
