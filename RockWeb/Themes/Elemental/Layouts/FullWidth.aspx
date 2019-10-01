<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">


	<main class="main">
<div id="main-content" class="container-fluid main-content">
        <!-- Start Content Area -->

        <!-- Page Title -->
        <h1 class="page-title"><Rock:PageTitle ID="PageTitle" runat="server" /></h1>

        <!-- Ajax Error -->
        <div class="alert alert-danger ajax-error no-index" style="display:none">
            <p><strong>Error</strong></p>
            <span class="ajax-error-message"></span>
        </div>

<Rock:Lava ID="GroupResults" runat="server">
<script src="/Scripts/jquery.lazyload.min.js"></script>
<script>
    Sys.Application.add_load( function () {
        $(function() {
            $("img.js-lazy").lazyload({
                effect : "fadeIn",
                threshold: 1500,
                container: $("#main-content")
            });
        });
    });
</script>

</Rock:Lava>




        <div class="row">
            <div class="col-md-12">
                <Rock:Zone Name="Feature" runat="server" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-12">
                <Rock:Zone Name="Main" runat="server" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-12">
                <Rock:Zone Name="Section A" runat="server" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-4">
                <Rock:Zone Name="Section B" runat="server" />
            </div>
            <div class="col-md-4">
                <Rock:Zone Name="Section C" runat="server" />
            </div>
            <div class="col-md-4">
                <Rock:Zone Name="Section D" runat="server" />
            </div>
        </div>

        <!-- End Content Area -->
    </div>
	</main>
</asp:Content>

<asp:Content ID="ctFeature" ContentPlaceHolderID="feature" runat="server">

<aside class="aside">

    <div class="" style="background-image: url(/themes/Elemental/Assets/Images/groups.jpg); background-position: 50% 50%; background-repeat: no-repeat; background-size: cover; width: 100%;">
        <img alt="" src="/themes/Elemental/Assets/Images/groups.jpg">
    </div>

</aside>

</asp:Content>