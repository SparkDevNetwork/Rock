<%@ Page Language="C#" MasterPageFile="SiteNavless.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">
                
    <!-- Start Content Area -->
        
    <section id="page-content">            
        <!-- Ajax Error -->
        <div class="alert alert-danger ajax-error" style="display:none">
            <p><strong>Error</strong></p>
            <span class="ajax-error-message"></span>
        </div>

        <div class="row">
            <div class="col-md-3">
                <Rock:Zone Name="Sidebar 1" runat="server" />
            </div>
            <div class="col-md-9">
                <Rock:Zone Name="Main" runat="server" />
            </div>
        </div>
        
    </section>
    <!-- End Content Area -->


</asp:Content>