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
        <div class="hero has-overlay" style="background: url('{{ headerImageId | ImageUrl:'','rootUrl' }}&maxWidth=2500');"></div>
        {%- else -%}
        <div class="hero">
        {%- endif -%}
    </Rock:Lava>
        <div class="container d-flex flex-column text-white" style="min-height: 100vh;">
            <div class="row my-auto">
                <div class="col-lg-10 col-md-8 col-sm-12 text-center mx-auto py-5 my-5">
                    <Rock:Zone Name="Headline" CssClass="zone-headline" runat="server" />
                    <div class="mt-5"><Rock:Zone Name="CTA Buttons" runat="server" /></div>
                    <div class="mt-5"><Rock:Zone Name="Section A" runat="server" /></div>
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

