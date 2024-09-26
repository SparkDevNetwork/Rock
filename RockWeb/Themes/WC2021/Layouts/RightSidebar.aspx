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
                else
                {
                    var headerImage = rockPage.GetAttributeValue( "HeaderImage" );
                    if ( string.IsNullOrWhiteSpace( headerImage ) )
                    {
                        secPageTitle.Attributes["class"] += " no-image";
                    }
                    else
                    {
                        secPageTitle.Attributes["class"] += " has-image";
                    }
                }
            }
        }
    }
</script>

<asp:Content ID="ctFeatured" ContentPlaceHolderID="feature" runat="server">
    <section id="secPageTitle" class="page-header" role="banner" runat="server">
        <div class="container">
            <h1 id="page-title" class="page-title"><Rock:PageTitle ID="PageTitle" runat="server" /></h1>
            <Rock:PageDescription ID="PageDescription" runat="server" />
        </div>
    </section>

    <Rock:Zone Name="Feature" runat="server" />
</asp:Content>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">

    <main id="main" class="main container">

        <!-- Ajax Error -->
        <div class="alert alert-danger ajax-error no-index" style="display:none">
            <p><strong>Error</strong></p>
            <span class="ajax-error-message"></span>
        </div>

        <div class="row g">
            <div class="col-xs-12 col-md-9">
                <Rock:Zone Name="Main" CssClass="pad-with-content" runat="server" />
            </div>
            <div class="col-xs-12 col-md-3 sidebar">
                <Rock:Zone Name="Sidebar 1" CssClass="pad-with-content" runat="server" />
            </div>
        </div>
    
    </main>
        <Rock:Zone Name="Section A" runat="server" />

        <div class="container">
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
        </div>

</asp:Content>
