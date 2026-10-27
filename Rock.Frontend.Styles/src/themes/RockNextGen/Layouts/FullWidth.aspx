<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<script runat="server">

    /// <summary>
    /// Removes empty title strip styling while keeping the Context zone available for editing.
    /// </summary>
    /// <param name="e">The event data.</param>
    protected override void OnPreRenderComplete( EventArgs e )
    {
        base.OnPreRenderComplete( e );

        var pageCache = Rock.Web.Cache.PageCache.Get( PageId );
        if ( pageCache == null )
        {
            return;
        }

        var hasPageTitle = pageCache.PageDisplayTitle && !string.IsNullOrWhiteSpace( base.PageTitle );
        var hasBreadCrumbs = pageCache.PageDisplayBreadCrumb && BreadCrumbs != null && BreadCrumbs.Count > 0;
        var hasPageDescription = pageCache.PageDisplayDescription && !string.IsNullOrWhiteSpace( pageCache.Description );
        var hasContextBlocks = Zones.ContainsKey( "Context" ) && Zones["Context"].Value.HasControls();

        secPageTitle.Attributes["class"] = secPageTitle.Attributes["class"].Replace( " page-header-empty", string.Empty );

        if ( !hasPageTitle && !hasBreadCrumbs && !hasPageDescription && !hasContextBlocks )
        {
            // Keep the zone in the control tree so administrators can add its first block.
            secPageTitle.Attributes["class"] += " page-header-empty";
        }
    }

</script>
<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">
        <!-- Page Title -->
        <section id="secPageTitle" class="page-header fullwidth" runat="server">
            <div class="page-title">
                <h1 class="title"><Rock:PageIcon ID="PageIcon" runat="server" /> <Rock:PageTitle ID="PageTitle" runat="server" /></h1>
                <Rock:PageBreadCrumbs ID="PageBreadCrumbs" runat="server" />
                <Rock:PageDescription ID="PageDescription" runat="server" />
            </div>
            <Rock:Zone Name="Context" CssClass="zone-context" runat="server" />
        </section>

        <!-- Start Content Area -->

        <section id="page-content">

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

            <div class="row">
                <div class="col-md-6">
                    <Rock:Zone Name="Section E" runat="server" />
                </div>
                <div class="col-md-6">
                    <Rock:Zone Name="Section F" runat="server" />
                </div>
            </div>
        </section>

        <!-- End Content Area -->



</asp:Content>

