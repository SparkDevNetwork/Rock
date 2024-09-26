<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">

    <main>

        <!-- Start Content Area -->

        <!-- Ajax Error -->
        <div class="alert alert-danger ajax-error no-index" style="display:none">
            <p><strong>Error</strong></p>
            <span class="ajax-error-message"></span>
        </div>

        <Rock:Zone Name="Feature" runat="server" />

        <div class="container">
            <div class="row">
                <div class="col-md-3">
                    <Rock:Zone Name="Sidebar 1" runat="server" />
                </div>
                <div class="col-md-9">
                    <Rock:Zone Name="Main" runat="server" />
                </div>
            </div>
        </div>

        <Rock:Zone Name="Section A" runat="server" />

        <div class="container">
            <div class="row">
                <div class="col-md-4">
                    <Rock:Zone Name="Section B" runat="server" />
                </div>
                <div class="col-md-4">
                    <Rock:Zone Name="Section C" runat="server" />
                </div>
                <div class="col-md-4">
                    <Rock:Zone Name="Section D" runat="server" />
                </div>
            </div>
        </div>

        <!-- End Content Area -->

    </main>

</asp:Content>
