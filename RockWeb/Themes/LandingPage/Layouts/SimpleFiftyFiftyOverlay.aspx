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
        <div class="hero has-overlay" style="background-image: url('{{ headerImageId | ImageUrl:'','rootUrl' }}&maxWidth=2500');">
        {%- else -%}
        <div class="hero">
        {%- endif -%}
    </Rock:Lava>
        <div class="container d-flex flex-column" style="min-height: 100vh">
            <div class="row my-auto">
                <Rock:Zone Name="Headline" CssClass="zone-headline col-lg-5 col-md-10 col-sm-12 py-5 text-white my-lg-auto my-5 text-center text-lg-left mx-auto ml-lg-0 mr-lg-auto" runat="server" />
                <Rock:Zone Name="Workflow" CssClass="zone-workflow pad-workflow col-lg-5 col-md-8 col-sm-12 py-5 px-sm-4 px-0 mx-auto ml-lg-auto mr-lg-0" runat="server" />
            </div>
        </div>
    </div>


    <div class="d-none">
        <Rock:Zone Name="CTA Buttons" runat="server" />
        <Rock:Zone Name="Section A" runat="server" />
        <Rock:Zone Name="Section B" runat="server" />
        <Rock:Zone Name="Section C" runat="server" />
        <Rock:Zone Name="Section D" runat="server" />
        <Rock:Zone Name="Main" runat="server" />
        <Rock:Zone Name="Extra" runat="server" />
        <Rock:Zone Name="Secondary Hero" runat="server" />
        <Rock:Zone Name="Footer" runat="server" />
    </div>
</asp:Content>

