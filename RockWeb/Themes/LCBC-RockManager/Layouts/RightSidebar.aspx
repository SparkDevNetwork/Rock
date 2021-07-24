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

        var rockPage = this.Page as Rock.Web.UI.RockPage;
        if (rockPage != null)
        {
            var pageCache = Rock.Web.Cache.PageCache.Get( rockPage.PageId );
            if (pageCache != null )
            {
                if (pageCache.PageDisplayTitle == false || string.IsNullOrWhiteSpace( rockPage.PageTitle ) )
                {
                    secPageTitle.Visible = false;
                }
            }
        }
    }

</script>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">
        <!-- Start Content Area -->

        <!-- Page Title -->
        <section id="secPageTitle" class="page-title" runat="server">
            <Rock:PageBreadCrumbs ID="PageBreadCrumbs" runat="server" />
            <h1 class="title"><Rock:PageIcon ID="PageIcon" runat="server" /> <Rock:PageTitle ID="PageTitle" runat="server" /></h1>
            <Rock:PageDescription ID="PageDescription" runat="server" />
        </section>

        <section id="page-content">
            <!-- Ajax Error -->
            <div class="alert alert-danger ajax-error no-index" style="display:none"><span class="ajax-error-message"></span></div>

            <div class="row">
                <Rock:Zone Name="Feature" runat="server" CssClass="col-md-12" />
            </div>

            <div class="row">
                <Rock:Zone Name="Main" runat="server" CssClass="col-md-9" />
                <Rock:Zone Name="Sidebar 1" runat="server" CssClass="col-md-3" />
            </div>

            <div class="row">
                <Rock:Zone Name="Section A" runat="server" CssClass="col-md-12" />
            </div>

            <div class="row">
                <Rock:Zone Name="Section B" runat="server" CssClass="col-md-4" />
                <Rock:Zone Name="Section C" runat="server" CssClass="col-md-4" />
                <Rock:Zone Name="Section D" runat="server" CssClass="col-md-4" />
            </div>

            <div class="row">
                <Rock:Zone Name="Section E" runat="server" CssClass="col-md-6" />
                <Rock:Zone Name="Section F" runat="server" CssClass="col-md-6" />
            </div>
        </section>
        <!-- End Content Area -->
</asp:Content>
