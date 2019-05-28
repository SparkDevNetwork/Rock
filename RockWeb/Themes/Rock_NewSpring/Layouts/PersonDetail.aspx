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

        <div class="personprofilebar-bio">
                <Rock:Zone Name="Individual Detail" runat="server" />
        </div>

        <div class="personprofilebar-badge">
            <div class="row">
                <div class="badge-group col-sm-4">
                    <Rock:Zone Name="Badge Bar Left" runat="server" />
                </div>
                <div class="badge-group col-sm-4">
                    <Rock:Zone Name="Badge Bar Middle" runat="server" />
                </div>
                <div class="badge-group col-sm-4">
                    <Rock:Zone Name="Badge Bar Right" runat="server" />
                </div>
            </div>
        </div>

        <div class="personprofilebar-family">
            <Rock:Zone Name="Family Detail" runat="server" />
        </div>

		<div class="pagetabs">
            <div class="row">
                <div class="col-md-12">
                    <Rock:Zone Name="Sub Navigation" runat="server" />
                </div>
            </div> 
		</div>

        <div class="person-content">
            <div class="row">
                <div class="col-md-8">
                    <Rock:Zone Name="Section A1" runat="server" />
                </div>
                <div class="col-md-4">
                    <Rock:Zone Name="Section A2" runat="server" />
                </div>
            </div>

            <div class="row">
                <div class="col-md-4">
                    <Rock:Zone Name="Section B1" runat="server" />
                </div>
                <div class="col-md-4">
                    <Rock:Zone Name="Section B2" runat="server" />
                </div>
                <div class="col-md-4">
                    <Rock:Zone Name="Section B3" runat="server" />
                </div>
            </div>

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
