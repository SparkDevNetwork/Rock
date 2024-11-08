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
        <div class="hero hero-card has-overlay" style="background-image: url('{{ headerImageId | ImageUrl:'','rootUrl' }}&maxWidth=2500');">
        {%- else -%}
        <div class="hero hero-card">
        {%- endif -%}
    </Rock:Lava>
        <div class="container d-flex flex-column" style="min-height: 100vh;">
            <div class="row my-auto">
                <div class="col-lg-5 col-md-8 col-sm-12 mx-auto">
                    <div class="panel px-3"><div class="panel-body text-center py-5">
                        <Rock:Zone Name="Headline" CssClass="zone-headline" runat="server" />
                        <Rock:Zone Name="Section A" CssClass="mt-5" runat="server" />
                        <Rock:Zone Name="Workflow" CssClass="zone-workflow text-left" runat="server" />
                    </div></div>
                </div>
            </div>
        </div>
    </div>


    <div class="d-none">
        <Rock:Zone Name="CTA Buttons" runat="server" />
        <Rock:Zone Name="Section B" runat="server" />
        <Rock:Zone Name="Section C" runat="server" />
        <Rock:Zone Name="Section D" runat="server" />
        <Rock:Zone Name="Main" runat="server" />
        <Rock:Zone Name="Extra" runat="server" />
        <Rock:Zone Name="Secondary Hero" runat="server" />
        <Rock:Zone Name="Footer" runat="server" />
    </div>

</asp:Content>

