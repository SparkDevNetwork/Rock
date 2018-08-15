<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctFeature" ContentPlaceHolderID="feature" runat="server">

  <Rock:Zone Name="Feature" runat="server" />

</asp:Content>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">
    
	<main>
        
        <!-- Start Content Area -->
        
        <!-- Ajax Error -->
        <div class="alert alert-danger ajax-error" style="display:none">
            <p><strong>Error</strong></p>
            <span class="ajax-error-message"></span>
        </div>

        <Rock:Zone Name="Sub Feature" runat="server" />

        <Rock:Zone Name="Section A" runat="server" />

        <Rock:Zone Name="Section B" runat="server" />

        <Rock:Zone Name="Section C" runat="server" />

        <Rock:Zone Name="Section D" runat="server" />

        <!-- End Content Area -->

	</main>
        
</asp:Content>

