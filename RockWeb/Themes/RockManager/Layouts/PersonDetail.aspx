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
    <div class="personprofile">

        <!-- Ajax Error -->
        <div class="alert alert-danger ajax-error no-index" style="display:none"><span class="ajax-error-message"></span></div>

        <Rock:Zone Name="Individual Detail" runat="server" CssClass="personprofilebar-bio" />

        <div class="personprofilebar-badge">
            <div class="row">
                <Rock:Zone Name="Badge Bar Left" runat="server" CssClass="badge-group col-sm-4" />
                <Rock:Zone Name="Badge Bar Middle" runat="server" CssClass="badge-group col-sm-4" />
                <Rock:Zone Name="Badge Bar Right" runat="server" CssClass="badge-group col-sm-4" />
            </div>
        </div>

        <Rock:Zone Name="Family Detail" runat="server" CssClass="personprofilebar-family" />

        <Rock:Zone Name="Sub Navigation" runat="server" CssClass="pagetabs" />

        <div class="person-content">
            <div class="row">
                <Rock:Zone Name="Section A1" runat="server" CssClass="col-md-8" />
                <Rock:Zone Name="Section A2" runat="server" CssClass="col-md-4" />
            </div>

            <div class="row">
                <Rock:Zone Name="Section B1" runat="server" CssClass="col-md-4" />
                <Rock:Zone Name="Section B2" runat="server" CssClass="col-md-4" />
                <Rock:Zone Name="Section B3" runat="server" CssClass="col-md-4" />
            </div>

            <div class="row">
                <Rock:Zone Name="Section C1" runat="server" CssClass="col-md-12" />
            </div>

            <div class="row">
                <Rock:Zone Name="Section D1" runat="server" CssClass="col-md-4" />
                <Rock:Zone Name="Section D2" runat="server" CssClass="col-md-8" />
            </div>
        </div>

	</div>
</asp:Content>