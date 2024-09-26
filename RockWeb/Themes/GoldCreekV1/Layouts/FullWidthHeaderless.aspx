<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">

    <main class="main">
        <!-- Ajax Error -->
        <div class="alert alert-danger ajax-error no-index" style="display:none">
            <p><strong>Error</strong></p>
            <span class="ajax-error-message"></span>
        </div>

        <div class="row">
            <Rock:Zone Name="Feature" CssClass="main-content col-xs-12 col-sm-12" runat="server" />
        </div>

        <div class="container">
            <div class="row">
                <div class="col-xs-12 col-sm-12">
                    <Rock:Zone Name="Main" CssClass="main-content" runat="server" />
                </div>
            </div>
        </div>

        <div class="row">
            <div class="col-xs-12 col-sm-12">
                <Rock:Zone Name="Section A" CssClass="main-content" runat="server" />
            </div>
        </div>

        <div class="row">
            <div class="col-xs-12 col-sm-12 col-md-4">
                <Rock:Zone Name="Section B" CssClass="" runat="server" />
            </div>
            <div class="col-xs-12 col-sm-12 col-md-4">
                <Rock:Zone Name="Section C" CssClass="" runat="server" />
            </div>
            <div class="col-xs-12 col-sm-12 col-md-4">
                <Rock:Zone Name="Section D" CssClass="" runat="server" />
            </div>
        </div>

        <!-- End Content Area -->

	</main>

</asp:Content>

