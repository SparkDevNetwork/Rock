<%@ Page ValidateRequest="false" Language="C#" MasterPageFile="Site.Master" Trace="false"
    AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">

    <!-- Page Header -->
    <header class="pagerheader">
        <div class="container">
            
                <!-- Brand Bar -->
			    <div class="navbar-header">
                    <asp:HyperLink ID="hlHome" runat="server" CssClass="navbar-brand" NavigateUrl="~" ToolTip="Rock ChMS">
                        <asp:Image ID="imgLogo" runat="server" AlternateText="Rock ChMS" ImageUrl="~/Assets/Images/rock-logo.svg" CssClass="brandbar-logo" />
                    </asp:HyperLink>
                    
                    <button class="navbar-toggle" type="button" data-toggle="collapse" data-target=".pagenav">
                        <i class="fa fa-bars fa-2x"></i>
                    </button>
			    </div>	

                <div class="pull-right header-zone">
                    <Rock:Zone Name="Header" runat="server" />
                </div>
			
				<Rock:SearchField ID="searchField" CssClass="pull-right" runat="server" />

        </div>


        <!-- Main Navigation -->
	    <nav class="pagenav navbar-collapse collapse">
            <div class="container">
                <div class="">
                    <Rock:Zone Name="Navigation" runat="server" />
                </div>
            </div>									
	    </nav>

    </header>

    <div class="persondetails">

        <div class="navbar navbar-static-top persondetails-header">
            <div class="container">
                <Rock:Zone Name="Individual Detail" runat="server" />
            </div>
        </div>

        <div class="navbar navbar-static-top persondetails-badgebar">
            <div class="container">
                <div class="row">
                    <div class="badge-group col-md-3">
                        <Rock:Zone Name="Badg Bar Left" runat="server" />
                    </div>
                    <div class="badge-group col-md-3">
                        <Rock:Zone Name="Badg Bar Middle" runat="server" />
                    </div>
                    <div class="badge-group col-md-6">
                        <Rock:Zone Name="Badg Bar Right" runat="server" />
                    </div>
                </div>
            </div>
        </div>

        <div class="navbar navbar-static-top persondetails-familybar">
			<div class="container">    
                <Rock:Zone Name="Family Detail" runat="server" />
            </div>
        </div>

		<div class="container pagetabs">
            <div class="row">
                <div class="col-md-12">
                    <Rock:Zone Name="Sub Navigation" runat="server" />
                </div>
            </div> 
		</div>

        <div class="container person-content">
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

    <footer class="page-footer">
        <div class="container">
            <div class="row">
                <div class="col-md-12">
                    <Rock:Zone Name="Footer" runat="server" />
                </div>
            </div>
        </div>
    </footer>

</asp:Content>

