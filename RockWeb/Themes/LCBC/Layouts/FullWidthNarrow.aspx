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
<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">
    <section id="secPageTitle" class="page-header" role="banner" runat="server">
        <Rock:Lava ID="rlHeroImage" runat="server">
            {% assign heroImage = CurrentPage | Attribute:'HeaderImage','Object' %}
            {% assign heroVideo = CurrentPage | Attribute:'HeaderVideo','RawValue' %}
            
            {% if heroVideo != '' %}
                <video id="hero-video" autoplay loop muted playsinline class="hero-video" preload="auto">
                    <source src="{{ heroVideo }}" type="video/mp4">
                </video>
                <div class="hero-overlay"></div>
            {% else %}
                {% if heroImage != empty %}
                    {% assign photoUrl = heroImage.Guid | Prepend:'https://origin.lcbcchurch.com/GetImage.ashx?Guid=' %}
                    {% assign width = heroImage.Width %}
                    <img
                        src="{{ photoUrl | TriumphImgCdn:'w=1100&h=550&fit=crop&auto=compress' }}"
                        srcset="{%- if width >= 550 -%}{{ photoUrl | TriumphImgCdn:'w=350&h=175&fit=crop&auto=format,compress' }} 350w{%- endif -%}
                                {%- if width >= 1100 -%},{{ photoUrl | TriumphImgCdn:'w=750&h=375&fit=crop&auto=format,compress' }} 750w{%- endif -%}
                                {%- if width >= 1445 -%},{{ photoUrl | TriumphImgCdn:'w=1100&h=550&fit=crop&auto=format,compress' }} 1100w{%- endif -%}
                                {%- if width >= 1680 -%},{{ photoUrl | TriumphImgCdn:'w=1500&h=750&fit=crop&auto=format,compress' }} 1500w{%- endif -%}
                                {%- if width >= 2048 -%},{{ photoUrl | TriumphImgCdn:'w=2200&h=1100&fit=crop&auto=format,compress' }} 2200w{%- endif -%}
                                {%- if width >= 3000 -%},{{ photoUrl | TriumphImgCdn:'w=3000&h=1500&fit=crop&auto=format,compress' }} 3000w{%- endif -%}"
                        sizes="100vw"
                        class="hero-image"
                        alt=""
                        width="{{ heroImage.Width }}"
                        height="{{ heroImage.Height }}"
                        itemprop="image">
                    <div class="hero-overlay"></div>
                {% endif %}
            {% endif %}
        </Rock:Lava>
        <div class="container">
            <div class="row">
                <div class="col-md-12">
                    <h1 id="page-title" class="page-title"><Rock:PageTitle ID="PageTitle" runat="server" /></h1>
                    <Rock:PageDescription ID="PageDescription" runat="server" />
                </div>
            </div>
        </div>
    </section>

    <Rock:Zone Name="Feature" runat="server" />
	<main class="container">

        <!-- Ajax Error -->
        <div class="alert alert-danger ajax-error no-index" style="display:none">
            <p><strong>Error</strong></p>
            <span class="ajax-error-message"></span>
        </div>

        <div class="row d-flex justify-content-center">
            <div class="col-xs-12 col-md-6 col-lg-6">
                <Rock:Zone Name="Main" CssClass="pad-with-content" runat="server" />
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

    <Rock:Zone Name="Footer" CssClass="zone-footer no-index" runat="server" />
</asp:Content>

