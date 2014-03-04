<%@ Page ValidateRequest="true" Language="C#" MasterPageFile="Site.Master"
    AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">

    <main class="container">

        <!-- Start Content Area -->

        <Rock:PageBreadCrumbs ID="PageBreadCrumbs" runat="server" />
                    
        <!-- Ajax Error -->
        <div class="alert alert-danger ajax-error" style="display:none">
            <p><strong>Error</strong></p>
            <span class="ajax-error-message" / ></span>
        </div>

        <div class="row">
            <div class="col-md-12">
                <Rock:Zone Name="Feature" runat="server" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-9">
                <div class="panel panel-default">
                    <div class="panel-body">
                        <Rock:Zone Name="Main" runat="server" />
                    </div>
                </div>
            </div>
            <div class="col-md-3">
                <Rock:Zone Name="Sidebar1" runat="server" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-12">
                <Rock:Zone Name="SectionA" runat="server" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-4">
                <Rock:Zone Name="SectionB" runat="server" />
            </div>
            <div class="col-md-4">
                <Rock:Zone Name="SectionC" runat="server" />
            </div>
            <div class="col-md-4">
                <Rock:Zone Name="SectionD" runat="server" />
            </div>
        </div>

        <!-- End Content Area -->
    </main>

</asp:Content>
