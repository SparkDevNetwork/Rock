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
        <div id="profilenavigation" class="profile-sticky-nav">
            <div class="profile-sticky-nav-placeholder"></div>
            <div class="profile-nav">
                <Rock:Zone Name="Profile Navigation Left" CssClass="flex-1 z-10" runat="server" />
                <div class="overflow-nav-container" id="overflow-nav">
                    <Rock:Zone Name="Profile Navigation" CssClass="profile-sticky-nav" runat="server" />
                </div>
                <Rock:Zone Name="Profile Navigation Right" CssClass="d-flex flex-1 justify-content-end overflow-hidden" runat="server" />
            </div>
        </div>

            <!-- Ajax Error -->
            <div class="alert alert-danger ajax-error no-index" style="display:none">
                <p><strong>Error</strong></p>
                <span class="ajax-error-message"></span>
            </div>
        <div class="person-profile person-profile-row">


            <div class="profile-main profile-sidebar">
                <Rock:Zone Name="Profile" runat="server" />
            </div>
            <div class="profile-data">
                <Rock:Zone Name="Badge Bar" runat="server" />

                <div class="person-profile-row">
                    <div class="profile-notes">
                        <Rock:Zone Name="Section A1" runat="server" />
                    </div>

                    <div class="profile-sidebar">
                        <Rock:Zone Name="Section A2" runat="server" />
                    </div>
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

<script>
    const header = document.querySelector("#profilenavigation");
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
