<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctFeature" ContentPlaceHolderID="feature" runat="server">

    <section class="main-feature soft-half lg-soft">
        <div class="container-fluid hard">
            <div class="row">
                <div class="col-md-12">
                    <Rock:Zone Name="Feature" runat="server" />
                </div>
            </div>
        </div>
    </section>

<!--     <div class="soft-sides xs-soft-half-sides">
        <hr class="flush">
    </div> -->

</asp:Content>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">
    
	<main class="soft-half lg-soft">
        
        <!-- Start Content Area -->
        
        <!-- Ajax Error -->
        <div class="alert alert-danger ajax-error" style="display:none">
            <p><strong>Error</strong></p>
            <span class="ajax-error-message"></span>
        </div>

        <div class="row">
            <div class="col-md-12">
                <Rock:Zone Name="Sub Feature" runat="server" />
            </div>
        </div>
    </main>
    
    <section class="background-theme-4 soft-half lg-soft-top lg-soft-sides lg-soft-half-bottom">
        <Rock:Zone Name="Section A" runat="server" />
    </section>

    <section class="soft-half lg-soft">
        <div class="row">
            <div class="col-md-4 col-sm-6">
                <Rock:Zone Name="Section B" runat="server" />
            </div>
            <div class="col-md-4 col-sm-6">
                <Rock:Zone Name="Section C" runat="server" />
            </div>
            <div class="col-md-4 col-sm-6">
                <Rock:Zone Name="Section D" runat="server" />
            </div>
        </div>
    </section>

    <!-- End Content Area -->

	
        
</asp:Content>