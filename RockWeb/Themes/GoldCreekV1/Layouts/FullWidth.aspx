<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">

    <!-- Page Title -->
    <Rock:Lava ID="PageHero" runat="server">

    {% assign heroImg = CurrentPage | Attribute:'HeroImage','RawValue' %}
    {% assign heroClasses = 'hero hero-no-img' %}

    {% if heroImg and heroImg != empty  %}
        {%- capture bgImg -%}
            style="background-image:url(/GetImage.ashx?Guid={{ heroImg }});background-size: cover;background-position: center;background-repeat: no-repeat;"
        {%- endcapture -%}
        {% assign heroClasses = 'hero hero-has-img' %}

    {% endif %}

    <section id="secPageTitle" class="{{ heroClasses }}" {{ bgImg }}>

        <div class="container container-lg">
            <div class="row">
                <div class="col-md-10">

    </Rock:Lava>

                    <Rock:PageBreadCrumbs ID="PageBreadCrumbs" runat="server" />
                    <h1 class="pagetitle h3 display text-white m-0"><Rock:PageTitle ID="PageTitle" runat="server" /></h1>
                </div>
            </div>
        </div>

    </section>


    <main class="main">
        <!-- Ajax Error -->
        <div class="alert alert-danger ajax-error no-index" style="display:none">
            <p><strong>Error</strong></p>
            <span class="ajax-error-message"></span>
        </div>

        <div class="row">
            <Rock:Zone Name="Feature" CssClass="main-content col-xs-12 col-sm-12" runat="server" />
        </div>

        <div class="container">
            <div class="row">
                <div class="col-xs-12 col-sm-12">
                    <Rock:Zone Name="Main" CssClass="main-content" runat="server" />
                </div>
            </div>
        </div>

        <div class="row">
            <div class="col-xs-12 col-sm-12">
                <Rock:Zone Name="Section A" CssClass="main-content" runat="server" />
            </div>
        </div>

        <div class="row">
            <div class="col-xs-12 col-sm-12 col-md-4">
                <Rock:Zone Name="Section B" CssClass="" runat="server" />
            </div>
            <div class="col-xs-12 col-sm-12 col-md-4">
                <Rock:Zone Name="Section C" CssClass="" runat="server" />
            </div>
            <div class="col-xs-12 col-sm-12 col-md-4">
                <Rock:Zone Name="Section D" CssClass="" runat="server" />
            </div>
        </div>

        <!-- End Content Area -->

	</main>

</asp:Content>

