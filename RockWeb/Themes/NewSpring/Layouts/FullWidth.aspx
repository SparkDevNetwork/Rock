<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">

    <Rock:Lava ID="PageHasVideoAssign" runat="server">
        {% assign pageHasVideo = 0 %}
    </Rock:Lava>

    <Rock:Lava ID="PageColor" runat="server">
            {% assign pageColor = CurrentPage | Attribute:'PageColor' %}
            <div class="position-fixed top-zero right-zero bottom-zero left-zero" style="background-color: {{ pageColor }}; z-index: -1;"></div>
    </Rock:Lava>

    <div class="soft xs-soft-half hard-bottom xs-hard-bottom clearfix">

        <Rock:Lava ID="PageTitle" runat="server">
        {% if CurrentPage.PageDisplayTitle == true %}
            {[pageHeader]}
        {% endif %}
        </Rock:Lava>

        <!-- Breadcrumbs -->
        <Rock:PageBreadCrumbs ID="PageBreadCrumbs" runat="server" />

        <!-- Ajax Error -->
        <div class="alert alert-danger ajax-error no-index" style="display:none">
            <p><strong>Error</strong></p>
            <span class="ajax-error-message"></span>
        </div>

        <Rock:Zone Name="Feature" runat="server" />
        <Rock:Zone Name="Main" runat="server" />
        <Rock:Zone Name="Section A" runat="server" />
        <Rock:Zone Name="Section B" runat="server" />
        <Rock:Zone Name="Section C" runat="server" />
        <Rock:Zone Name="Section D" runat="server" />

    </div>

    <Rock:Lava ID="PageHasVideoCheck" runat="server">
        {% if pageHasVideo > 0 %}
            <script src="https://fast.wistia.com/assets/external/E-v1.js" async=""></script>
        {% endif %}
    </Rock:Lava>

</asp:Content>