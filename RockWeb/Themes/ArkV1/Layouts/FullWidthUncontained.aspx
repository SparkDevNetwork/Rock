<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">

	<main>

        <!-- Start Content Area -->
        <Rock:Lava ID="mainhero" runat="server">
            {% include '/Themes/ArkV1/Assets/Lava/Custom/Components/Hero.lava' %}
        </Rock:Lava>

        <!-- Ajax Error -->
        <div class="alert alert-danger ajax-error no-index" style="display:none">
            <p><strong>Error</strong></p>
            <span class="ajax-error-message"></span>
        </div>

        <Rock:Zone Name="Feature" runat="server" />

        <Rock:Zone Name="Main" runat="server" />

        <Rock:Zone Name="Section A" runat="server" />

        <!-- End Content Area -->

	</main>

</asp:Content>

