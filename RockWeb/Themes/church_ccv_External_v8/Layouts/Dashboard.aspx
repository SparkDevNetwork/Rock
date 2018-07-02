<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctFeature" ContentPlaceHolderID="feature" runat="server">

    <section class="main-feature">
        <div class="container-fluid">
            <div class="row">
                <div class="col-md-12">
                    <Rock:Zone Name="Feature" runat="server" />
                </div>
            </div>
        </div>
    </section>

</asp:Content>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">

	<main>
        <div class="container">
            <!-- Start Content Area -->

            <!-- Ajax Error -->
            <div class="alert alert-danger ajax-error" style="display:none">
                <p><strong>Error</strong></p>
                <span class="ajax-error-message"></span>
            </div>

        </div>

        <div class="container-fluid">
			<div class="row">
                <div id="subnavbar-bg" class="col-md-12">
						<Rock:Zone Name="Sub Navbar" runat="server" />
                </div>
            </div>

            <div class="row">
                <div id="ternavbar-bg" class="col-md-12">
						<Rock:Zone Name="Tertiary Navbar" runat="server" />
                </div>
            </div>

            <div class="row">
                <div id="single-section-bg" class="col-md-12">
                    <Rock:Zone Name="Single Section" runat="server" />
                </div>
            </div>
			
            <!-- End Content Area -->

        </div>
	</main>

</asp:Content>

