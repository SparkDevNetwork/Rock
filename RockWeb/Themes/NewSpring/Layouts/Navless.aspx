<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">

    <div class="position-relative soft-top soft-sides xs-soft-half-top xs-soft-half-sides">

        <!-- Breadcrumbs -->
        <Rock:PageBreadCrumbs ID="PageBreadCrumbs" runat="server" />

        <!-- Ajax Error -->
        <div class="alert alert-danger ajax-error no-index" style="display:none">
            <p><strong>Error</strong></p>
            <span class="ajax-error-message"></span>
        </div>

        <Rock:Lava ID="PageColor" runat="server">
            {% assign pageColor = CurrentPage | Attribute:'PageColor' %}
            <div class="position-absolute full-screen" style="background-color: {{ pageColor }};"></div>
        </Rock:Lava>

        <Rock:Zone Name="Feature" runat="server" />

        <div id="content" class="clearfix">
            <Rock:Zone Name="Main" runat="server" />

            <div class="page-constrained mx-auto">
                <Rock:Zone Name="Section A" runat="server" />
            </div>

            <Rock:Zone Name="Section B" runat="server" />

            <div class="page-constrained mx-auto">
                <Rock:Zone Name="Section C" runat="server" />
            </div>
            
            <Rock:Zone Name="Section D" runat="server" />
        </div>
        
    </div>

</asp:Content>