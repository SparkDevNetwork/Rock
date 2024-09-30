<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<script runat="server">

    // keep code below to call base class init method

    /// <summary>
    /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
    protected override void OnPreRender( EventArgs e )
    {
        base.OnPreRender( e );
    }

</script>

<asp:Content ID="ctFeature" ContentPlaceHolderID="feature" runat="server">
    <style>
        body.is-fullscreen #content-wrapper > .row {
        padding: 0 !important;
        }
        body.is-fullscreen #content-wrapper > .row > .col-md-12 {
            padding: 0;
        }
        body.is-fullscreen #content-wrapper, body.is-fullscreen .page-wrapper {
            margin: 0;
            padding: 0;
        }
        .main-content,
        body.is-fullscreen #cms-admin-footer, body.is-fullscreen .navbar-static-side, body.is-fullscreen .rock-top-header {
            display: none;
        }

        .featured-content {
            --block-min-height: 100px;
        }

        .featured-content,
        .featured-content > .col-md-12,
        .main-content {
            padding: 0 !important;
        }
        .block-content-main > .panel.panel-block,
        .panel.panel-block {
            margin-bottom: 0;
            height: calc(100vh - var(--top-header-height, 80px) - 36px); /* Where 152px is the height of the header and footer */
            overflow-y: auto !important;
        }
    </style>
        <!-- Ajax Error -->
    <div class="alert alert-danger ajax-error no-index" style="display:none">
        <p><strong>Error</strong></p>
        <span class="ajax-error-message"></span>
    </div>

    <div class="row">
        <div class="col-md-12">
            <Rock:Zone Name="Feature" runat="server" />
        </div>
    </div>

    <div class="featured-content row py-3">
        <div class="col-md-12">
            <Rock:Zone Name="Main" runat="server" CssClass="page-fullscreen-capable" />
        </div>
    </div>
</asp:Content>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">

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

    <div class="row">
        <div class="col-md-6">
            <Rock:Zone Name="Section E" runat="server" />
        </div>
        <div class="col-md-6">
            <Rock:Zone Name="Section F" runat="server" />
        </div>
    </div>
    <!-- End Content Area -->

</asp:Content>
