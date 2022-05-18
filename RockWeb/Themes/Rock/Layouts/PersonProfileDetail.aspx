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

    <div class="position-relative">
        <Rock:Zone Name="Profile Navigation" CssClass="profile-sticky-nav" runat="server" />
        <div class="person-profile person-profile-ext">
            <!-- Ajax Error -->
            <div class="alert alert-danger ajax-error no-index" style="display:none">
                <p><strong>Error</strong></p>
                <span class="ajax-error-message"></span>
            </div>
            <div class="row d-flex flex-wrap">
                <div class="col-xs-12 col-md-4 mb-4">
                    <Rock:Zone Name="Profile" CssClass="zone-h-100" runat="server" />
                </div>
                <div class="col-xs-12 col-md-8 mb-4">
                    <Rock:Zone Name="Badge Bar" runat="server" />
                </div>
            </div>

            <div class="row">
                <div class="col-md-8">
                    <Rock:Zone Name="Section A1" runat="server" />
                </div>
                <div class="col-md-4">
                    <Rock:Zone Name="Section A2" runat="server" />
                </div>
            </div>

            <div class="row">
                <div class="col-xs-12 col-md-4">
                    <Rock:Zone Name="Section B1" runat="server" />
                </div>
                <div class="col-xs-12 col-md-4">
                    <Rock:Zone Name="Section B2" runat="server" />
                </div>
                <div class="col-xs-12 col-md-4">
                    <Rock:Zone Name="Section B3" runat="server" />
                </div>
            </div>

            <div class="row">
                <div class="col-xs-12">
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

    <script>
        const header = document.querySelector("#zone-profilenavigation");
        const profileImage = document.querySelector("#profile-image");

        if (profileImage) {
            const profileImageOptions = {
            rootMargin: `${header.getBoundingClientRect().bottom * -1}px`,
            threshold: 0
            };

            const profileImageObserver = new IntersectionObserver(function(
            entries,
            profileImageObserver
            ) {
            entries.forEach(entry => {
                if (!entry.isIntersecting) {
                header.classList.add("nav-scrolled");
                } else {
                header.classList.remove("nav-scrolled");
                }
            });
            },
            profileImageOptions);

            profileImageObserver.observe(profileImage);
        } else {
            header.classList.add("nav-scrolled");
        }
    </script>

</asp:Content>
