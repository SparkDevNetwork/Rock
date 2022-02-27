<%@ Page Language="C#" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<!DOCTYPE html>

<%--<script runat="server">

    // keep code below to call base class init method

    /// <summary>
    /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
    protected override void OnInit( EventArgs e )
    {
        base.OnInit( e );

        lLogoSvg.Text = System.IO.File.ReadAllText( HttpContext.Current.Request.MapPath("~/Assets/Images/PCC_logo.svg") );
    }

</script>--%>

<html>
<head runat="server">
    <meta charset="utf-8">
    <title></title>

    <!-- Set the viewport width to device width for mobile disabling zooming -->
    <meta name="viewport" content="width=device-width, initial-scale=1.0, user-scalable=no">

    <script src="<%# ResolveRockUrl("~/Scripts/modernizr.js", true) %>"></script>
    <script src="<%# System.Web.Optimization.Scripts.Url("~/Scripts/Bundles/RockJQueryLatest") %>"></script>

    <!-- Included CSS Files -->
    <link rel="stylesheet" href="<%# ResolveRockUrl("~/Themes/Rock/Styles/bootstrap.css", true) %>" />
    <link rel="stylesheet" href="<%# ResolveRockUrl("~/Themes/Rock/Styles/theme.css", true) %>" />
    <link rel="stylesheet" href="<%# ResolveRockUrl("~/Themes/CONNECT/Styles/bootstrap.css", true) %>" />
    <link rel="stylesheet" href="<%# ResolveRockUrl("~/Themes/CONNECT/Styles/theme.css", true) %>" />

    <script src="<%# ResolveRockUrl("~/Scripts/bootstrap.min.js", true) %>"></script>

    <style type="text/css">
        #splash #logo {
            width: 250px;
            height: 200px;
        }

        body {
            background-color: unset;
        }

        .btn-link {
            color: #666;
        }

        .text-primary {
            color: #00B8E4;
        }
        /*.container-fluid {
            padding-left: unset;
            padding-right: unset;
        }*/

        .login-background {
            background: url(/Content/ExternalSite/Graphics/connect_portal_bg_1.jpg) no-repeat;
            -webkit-background-size: cover;
            -moz-background-size: cover;
            -o-background-size: cover;
            background-size: cover;
            background-position: 50% 50%;
            min-height: 100vh;
        }

        @media (max-width: 991px) and (min-width: 768px) {
            .login-background {
                min-height: 30vh;
            }
        }
    </style>

</head>
<body id="splash">

    <form id="form1" runat="server">

        <div class="container-fluid">

            <div class="row">

                <div style="padding-left: unset;" class="col-xs-12 col-sm-12 col-md-6">

                    <div class="hidden-xs login-background"></div>

                    <div id="content-box" class="clearfix">
                        <Rock:Zone Name="Main" runat="server" />
                    </div>

                </div>

                <div class="col-xs-12 col-sm-12 col-md-6">

                    <div class="row align-middle">

                        <div class="col-xs-12">

                            <div id="logo">
                                <img class="img img-responsive" src="https://passiontechrockdiag.blob.core.windows.net/rock-assets/design/PCC_logo.png" alt="site logo" />
                            </div>

                        </div>

                        <div class="col-xs-12">

                            <div id="login-box" class="clearfix">
                                <Rock:Zone Name="Login" runat="server" />
                            </div>

                        </div>

                    </div>

                </div>

            </div>

        </div>

    </form>
</body>
</html>
