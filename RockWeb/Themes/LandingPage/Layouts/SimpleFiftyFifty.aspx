<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">
    <!-- Ajax Error -->
    <div class="alert alert-danger ajax-error no-index" style="display:none">
        <p><strong>Error</strong></p>
        <span class="ajax-error-message"></span>
    </div>


    <div class="hero-split container-fluid">
        <div class="row">
            <div class="col-lg-4 col-md-8 col-sm-12 py-5 m-auto">
                <Rock:Zone Name="Headline" CssClass="zone-headline" runat="server" />
                <Rock:Zone Name="CTA Buttons" CssClass="mt-5" runat="server" />
                <Rock:Zone Name="Section A" runat="server" />
            </div>
            <Rock:Lava ID="HeaderImage" runat="server">
                {%- assign headerImageId = CurrentPage | Attribute:'HeaderImage','Id' -%}
                {%- if headerImageId != '' -%}
                <div class="col-lg-6 col-md-12" style="background: url('{{ headerImageId | ImageUrl:'','rootUrl' }}&maxWidth=2500') center center no-repeat; background-size: cover; min-height: 100vh;"></div>
                {%- else -%}
                <div class="col-lg-6 col-md-12"></div>
                {%- endif -%}
            </Rock:Lava>

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
        <Rock:Zone Name="Header" runat="server" />
        <Rock:Zone Name="Section B" CssClass="col-md-4" runat="server" />
        <Rock:Zone Name="Section C" CssClass="col-md-4" runat="server" />
        <Rock:Zone Name="Section D" CssClass="col-md-4" runat="server" />
        <Rock:Zone Name="Main" runat="server" />
        <Rock:Zone Name="Extra" runat="server" />
        <Rock:Zone Name="Secondary Hero" CssClass="col-lg-4 col-md-8 col-sm-12 py-5 mr-auto text-left" runat="server" />
        <Rock:Zone Name="Footer" runat="server" />
    </div>
</asp:Content>

