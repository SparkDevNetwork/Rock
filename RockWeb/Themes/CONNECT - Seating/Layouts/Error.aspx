<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Error.aspx.cs" Inherits="RockWeb.Themes.Stark.Layouts.Error" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Rock - Error</title>

    <link rel="stylesheet" href="<%= Page.ResolveUrl("~/Themes/PCC CONNECT/Styles/bootstrap.css") %>" />
    <link rel="stylesheet" href="<%= Page.ResolveUrl("~/Themes/PCC CONNECT/Styles/theme.css") %>" />
    <script src="<%# System.Web.Optimization.Scripts.Url("~/Scripts/Bundles/RockJQueryLatest" ) %>">
        $(document).ready(function () {
            $(".stack-trace").hide();

            //toggle the componenet with class msg_body
            $(".exception-type").click(function () {
                $(this).next(".stack-trace").slideToggle(500);
            });
        });
    </script>

       <style type="text/css">

    .error-wrap #logo {

        width: 250px;
    height: 250px;
    }

    </style>

 
</head>
<body id="splash" class="error">
    <form id="form1" runat="server">


        <div id="content">

            <div id="content-box">
                <div class="row">
                    <div class="col-md-12">
                        <div class="error-wrap" style="min-height: 100vh;">
                            
                            <div id="logo text-center">
                <img class="img img-responsive" src="https://passiontechrockdiag.blob.core.windows.net/rock-assets/design/PCC_logo.png" alt="site logo" />
            </div>

                            <p class="alert alert-info"><strong>An Error Occurred...</strong>  The team has
                            been notified of this problem. If you have further questions, please email <a href="mailto:connect.passioncitychurch.com">connect@passioncitychurch.com</a></p>
                            
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
