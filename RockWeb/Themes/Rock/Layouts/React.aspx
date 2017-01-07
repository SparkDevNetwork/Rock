<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">
   
        <!-- Start Content Area -->
        
        <!-- Page Title -->
        <section id="page-title">
            <h1 class="title"><Rock:PageIcon ID="PageIcon" runat="server" /> <Rock:PageTitle ID="PageTitle" runat="server" /></h1>
            <Rock:PageBreadCrumbs ID="PageBreadCrumbs" runat="server" />
            <Rock:PageDescription ID="PageDescription" runat="server" />
        </section>
        
        <section id="page-content">            
            <div id="react-root" />
        </section>
        <!-- End Content Area -->
        <script src="http://localhost:8080/RockWeb/Themes/Rock/bundle.js"></script>


</asp:Content>
