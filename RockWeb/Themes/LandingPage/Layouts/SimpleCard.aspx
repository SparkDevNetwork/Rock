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
                        <Rock:Zone Name="CTA Buttons" CssClass="mt-5" runat="server" />
                        <Rock:Zone Name="Section A" CssClass="mt-5" runat="server" />
                    </div></div>
                </div>
            </div>
        </div>
    </div>

    <!-- Modal -->
    <div class="workflow-modal modal fade" id="workflowModal" tabindex="-1" role="dialog" aria-labelledby="workflowModalLabel">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-body">
                    <Rock:Zone Name="Workflow" CssClass="zone-workflow" runat="server" />
                </div>
            </div>
        </div>
    </div>


    <div class="d-none">
        <Rock:Zone Name="Section B" CssClass="col-md-4" runat="server" />
        <Rock:Zone Name="Section C" CssClass="col-md-4" runat="server" />
        <Rock:Zone Name="Section D" CssClass="col-md-4" runat="server" />
        <Rock:Zone Name="Main" runat="server" />
        <Rock:Zone Name="Extra" runat="server" />
        <Rock:Zone Name="Secondary Hero" CssClass="col-lg-4 col-md-8 col-sm-12 py-5 mr-auto text-left" runat="server" />
        <Rock:Zone Name="Footer" runat="server" />
    </div>
</asp:Content>

