<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">

    <Rock:Lava ID="PageColor" runat="server">
            {% assign pageColor = CurrentPage | Attribute:'PageColor' %}
            <div class="position-fixed top-zero right-zero bottom-zero left-zero brand-bg" style="background-color: {{ pageColor }}; z-index: -1;"></div>
    </Rock:Lava>

    <div class="soft-top soft-sides xs-soft-half-top xs-soft-half-sides">

        <!-- Breadcrumbs -->
        <Rock:PageBreadCrumbs ID="PageBreadCrumbs" runat="server" />

        <!-- Ajax Error -->
        <div class="alert alert-danger ajax-error no-index" style="display:none">
            <p><strong>Error</strong></p>
            <span class="ajax-error-message"></span>
        </div>

        <Rock:Zone Name="Feature" runat="server" />

        <Rock:Lava ID="PageConstrained" runat="server">
            {% assign isPageConstrained = CurrentPage | Attribute:'PageConstrained' %}
            {% if isPageConstrained == 'Yes' %}
                <div class="page-constrained mx-auto">
            {% endif %}
        </Rock:Lava>

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

        <Rock:Lava ID="PageConstrainedClose" runat="server">
            {% assign isPageConstrained = CurrentPage | Attribute:'PageConstrained' %}
            {% if isPageConstrained == 'Yes' %}
                </div>
            {% endif %}
        </Rock:Lava>

    </div>

</asp:Content>
