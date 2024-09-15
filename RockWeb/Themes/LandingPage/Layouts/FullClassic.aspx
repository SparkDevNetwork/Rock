<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">
    <!-- Ajax Error -->
    <div class="alert alert-danger ajax-error no-index" style="display:none">
        <p><strong>Error</strong></p>
        <span class="ajax-error-message"></span>
    </div>

    <Rock:Lava ID="HeaderImage" runat="server">
        {%- assign headerImageId = CurrentPage | Attribute:'HeaderImage','Id' -%}
        {%- if headerImageId != '' -%}
        <div class="hero has-overlay classic-hero py-sm-5" style="background-image:url('{{ headerImageId | ImageUrl:'','rootUrl' }}&maxWidth=2500');">
        {%- else -%}
        <div class="hero has-overlay classic-hero py-sm-5" style="background-image:url('https://source.unsplash.com/mvxrY7z7gtM/2500x1800');">
        {%- endif -%}
    </Rock:Lava>

        <div class="container d-flex flex-column" style="min-height: 40vh">
            <div class="row my-auto">
                <div class="col-lg-8 col-md-8 col-sm-12 text-center mx-auto">
                    <Rock:Zone Name="Headline" CssClass="zone-headline" runat="server" />
                </div>
            </div>
        </div>
    </div>

    <div class="container d-flex flex-column py-5">
        <div class="row my-auto my-5 pb-2">
            <Rock:Zone Name="Main" CssClass="col-lg-7 col-md-10 col-sm-12 mr-lg-auto" runat="server" />
            <Rock:Zone Name="Workflow" CssClass="zone-workflow no-pad col-lg-4 col-md-10 col-sm-12" runat="server" />
        </div>

        <div class="row my-auto my-5 pb-2">
            <Rock:Zone Name="Extra" CssClass="col-lg-8 col-md-10 col-sm-12 mx-auto" runat="server" />
        </div>
    </div>


    <Rock:Lava ID="SecondaryImage" runat="server">
        {%- assign secondaryImageId = CurrentPage | Attribute:'SecondaryImage','Id' -%}
        {%- if secondaryImageId != '' -%}
        <div class="secondary-hero py-5" style="background: linear-gradient(90deg, var(--secondary-hero-overlay-color, rgba(0,0,0,0)), var(--secondary-hero-overlay-color, rgba(0,0,0,0))),url('{{ secondaryImageId | ImageUrl: '', 'rootUrl' }}&maxWidth=2500') center center; background-size: cover;">
        {%- else -%}
        <div class="secondary-hero py-5" style="background: linear-gradient(90deg, var(--secondary-hero-overlay-color, rgba(0,0,0,0)), var(--secondary-hero-overlay-color, rgba(0,0,0,0))),url('https://images.unsplash.com/photo-1520512533001-af75c194690b?ixlib=rb-0.3.5&ixid=eyJhcHBfaWQiOjEyMDd9&s=d23a0082e9aa3caa886db02d419bdd3d&auto=format&fit=crop&w=2500&q=80&auto=enhance') center center; background-size: cover;">
        {%- endif -%}
    </Rock:Lava>
        <div class="container d-flex flex-column" style="height: 95vh; max-height: 563px;">
            <div class="row my-auto">
                <Rock:Zone Name="Secondary Hero" CssClass="col-lg-4 col-md-8 col-sm-12 py-5 mr-auto text-left" runat="server" />
            </div>
        </div>
    </div>

    <footer class="container">
        <div class="row">
            <div class="col-md-12">
                <Rock:Zone Name="Footer" runat="server" />
            </div>
        </div>
    </footer>

    <div class="d-none">
        <Rock:Zone Name="CTA Buttons" runat="server" />
        <Rock:Zone Name="Section A" runat="server" />
        <Rock:Zone Name="Section B" CssClass="col-md-4" runat="server" />
        <Rock:Zone Name="Section C" CssClass="col-md-4" runat="server" />
        <Rock:Zone Name="Section D" CssClass="col-md-4" runat="server" />
    </div>

</asp:Content>

