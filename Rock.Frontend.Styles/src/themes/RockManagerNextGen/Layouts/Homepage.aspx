<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctFeature" ContentPlaceHolderID="feature" runat="server">
    <section class="main-feature">
        <div class="container">
            <div class="row">
                <Rock:Zone Name="Feature" runat="server" CssClass="col-md-12" />
            </div>
        </div>
    </section>
</asp:Content>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">
	<main class="container">
        <!-- Start Content Area -->

        <!-- Ajax Error -->
        <div class="alert alert-danger ajax-error no-index" style="display:none"><span class="ajax-error-message"></span></div>

        <div class="row">
            <Rock:Zone Name="Sub Feature" runat="server" CssClass="col-md-12" />
        </div>

        <div class="row">
            <Rock:Zone Name="Section A" runat="server"  CssClass="col-md-12"  />
        </div>

        <div class="row">
            <Rock:Zone Name="Section B" runat="server" CssClass="col-md-4"  />
            <Rock:Zone Name="Section C" runat="server" CssClass="col-md-4" />
            <Rock:Zone Name="Section D" runat="server" CssClass="col-md-4" />
        </div>

        <!-- End Content Area -->
	</main>
</asp:Content>

