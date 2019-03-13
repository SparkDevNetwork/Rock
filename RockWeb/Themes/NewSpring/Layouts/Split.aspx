<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">

    <Rock:Lava ID="PageColor" runat="server">
        {% assign pageColor = CurrentPage | Attribute:'PageColor' %}
        <div class="position-fixed top-zero right-zero bottom-zero left-zero" style="background-color: {{ pageColor }}; z-index: -1;"></div>
    </Rock:Lava>

    <div class="row">
        <div class="col-md-5">
            <div class="soft">
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
            </div>
        </div><div class="col-md-7">
            <Rock:Zone Name="Main" runat="server" />
        </div>
    </div>

    <Rock:Zone Name="Section A" runat="server" />
    <Rock:Zone Name="Section B" runat="server" />
    <Rock:Zone Name="Section C" runat="server" />
    <Rock:Zone Name="Section D" runat="server" />

</asp:Content>