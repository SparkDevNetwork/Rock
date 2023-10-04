<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<script runat="server">
    protected void Page_Load( object sender, EventArgs e )
    {
        var rockPage = this.Page as Rock.Web.UI.RockPage;
        var _backgroundImage = "";

        if ( rockPage != null )
        {
            var pageCache = Rock.Web.Cache.PageCache.Get( rockPage.PageId );
            if ( pageCache != null )
            {
                h1PageTitle.Visible = pageCache.PageDisplayTitle;
                divPageDesc.Visible = pageCache.PageDisplayDescription;
                Rock.Web.Cache.BlockCache HeaderBlocks = pageCache.Blocks.Find( b => b.Zone == "PageHeader" );
                if ( HeaderBlocks == null && !pageCache.PageDisplayTitle && !pageCache.PageDisplayDescription )
                {
                    divParallax.Attributes.Add( "class", "hide hide-block" );
                }
                _backgroundImage = pageCache.GetAttributeValue( "HeaderImage" );
            }
        }

        if ( !string.IsNullOrWhiteSpace( _backgroundImage ) )
        {
            if ( !_backgroundImage.Contains( "http" ) )
            {
                _backgroundImage = "/GetImage.ashx?guid=" + _backgroundImage;
            }
            divHeaderBg.Style.Add( "background-image", "url(\"" + _backgroundImage + "\")" );
        }
        else
        {
            divHeaderBg.Attributes["class"] += " defaultBg";
        }
    }
</script>

<asp:Content ID="ctFeature" ContentPlaceHolderID="feature" runat="server">
    <div class="container-fluid section-parallax" id="divParallax" runat="server">
        <div class="row">
            <div id="divHeaderBg" class="section-page-title et_parallax_bg" runat="server"></div>
            <div class="section-parallax-content padding-h-lg text-center">
                <Rock:PageIcon ID="PageIcon" runat="server" />
                <h1 id="h1PageTitle" runat="server">
                    <Rock:PageTitle ID="PageTitle" runat="server" />
                </h1>
                <div class="parallax_desc" id="divPageDesc" runat="server">
                    <Rock:PageDescription ID="PageDescription" runat="server"></Rock:PageDescription>
                </div>
                <Rock:Zone Name="Page Header" runat="server" />
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">

    <main class="container">

        <!-- Start Content Area -->

        <!-- Page Title -->
        <div class="row">
            <div class="col-md-8">
            </div>
            <div id="loginStatusZone" class="col-md-12">
                <Rock:Zone Name="LoginLogout" runat="server" />
            </div>
        </div>
        <Rock:PageBreadCrumbs ID="PageBreadCrumbs" runat="server" />

        <!-- Ajax Error -->
        <div class="alert alert-danger ajax-error" style="display: none">
            <p><strong>Error</strong></p>
            <span class="ajax-error-message"></span>
        </div>

        <div class="row">
            <div class="col-md-12">
                <Rock:Zone Name="Feature" runat="server" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-8">
                <Rock:Zone Name="Main" runat="server" />
            </div>
            <div class="col-md-4">
                <Rock:Zone Name="Sidebar 1" runat="server" />
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

    </main>

</asp:Content>
