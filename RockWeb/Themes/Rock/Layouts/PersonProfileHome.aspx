<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<%@ MasterType TypeName="Rock.Web.UI.RockMasterPage" %>
<script runat="server">

    protected override void OnLoad( EventArgs e )
    {
        base.OnLoad( e );
        Master.ShowPageTitle = false;
    }

</script>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">

    <div class="profile-content position-relative">
        <div id="profilenavigation" class="profile-sticky-nav">
            <div class="profile-sticky-nav-placeholder"></div>
            <div class="profile-nav">
                <Rock:Zone Name="Profile Navigation Left" CssClass="flex-1 z-10" runat="server" />
                <Rock:Zone Name="Profile Navigation" CssClass="zone-nav" runat="server" />
                <Rock:Zone Name="Profile Navigation Right" CssClass="d-flex flex-1 justify-content-end overflow-hidden" runat="server" />
            </div>
        </div>

        <!-- Ajax Error -->
        <div class="alert alert-danger ajax-error no-index" style="display:none">
            <p><strong>Error</strong></p>
            <span class="ajax-error-message"></span>
        </div>
        <div class="person-profile person-profile-row">


            <Rock:Zone Name="Profile" CssClass="profile-main profile-sidebar" runat="server" />

            <div class="profile-data">
                <Rock:Zone Name="Badge Bar" CssClass="zone-badgebar" runat="server" />

                <div class="person-profile-row">
                    <Rock:Zone Name="Section A1" CssClass="profile-notes" runat="server" />
                    <Rock:Zone Name="Section A2" CssClass="profile-sidebar" runat="server" />
                </div>

                <Rock:Zone Name="Section A3" runat="server" />
            </div>
        </div>

        <div class="person-profile-ext">

            <div class="row">
                <div class="col-md-12">
                    <Rock:Zone Name="Section C1" runat="server" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-4">
                    <Rock:Zone Name="Section D1" runat="server" />
                </div>
                <div class="col-md-8">
                    <Rock:Zone Name="Section D2" runat="server" />
                </div>
            </div>
        </div>
	</div>
</asp:Content>
