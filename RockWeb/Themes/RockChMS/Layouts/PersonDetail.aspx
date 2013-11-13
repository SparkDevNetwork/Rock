<%@ Page ValidateRequest="false" Language="C#" MasterPageFile="Site.Master" Trace="false"
    AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">

    <!-- Page Header -->
    <header>
        
        <!-- Brand Bar -->
        <div class="navbar navbar-default navbar-static-top brandbar" role="navigation">
            <div class="container">
			    <div class="navbar-header">
                    
                    <asp:HyperLink ID="hlHome" runat="server" CssClass="navbar-brand" NavigateUrl="~" ToolTip="Rock ChMS">
                        <asp:Image ID="imgLogo" runat="server" AlternateText="Rock ChMS" ImageUrl="~/Assets/Images/rock-logo.svg" CssClass="brandbar-logo" />
                    </asp:HyperLink>
                    
                    <button class="navbar-toggle" type="button" data-toggle="collapse" data-target=".pageheader-collapse">
                        <span class="icon-bar"></span>
                        <span class="icon-bar"></span>
                        <span class="icon-bar"></span>
                    </button>

			    </div>	
                <div class="navbar-collapse collapse pageheader-collapse">   
                    <div class="navbar-right">
                        <Rock:Zone Name="Header" runat="server" />
                    </div>
                    <div class="navbar-right navbar-text">				
					    <Rock:SearchField ID="searchField" runat="server" />
                    </div>
			    </div>	
            </div>
        </div>

        <!-- Main Navigation -->
	    <nav class="navbar navbar-static-top pagenav">
            <div class="container">
                <Rock:Zone Name="Navigation" runat="server" />
            </div>									
	    </nav>

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
                <div class="col-md-8">
                    <Rock:Zone Name="Section D1" runat="server" />
                </div>
                <div class="col-md-4">
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

