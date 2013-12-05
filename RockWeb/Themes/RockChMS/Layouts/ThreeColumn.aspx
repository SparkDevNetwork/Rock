<%@ Page ValidateRequest="false" Language="C#" MasterPageFile="Site.Master"
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
		
        <!-- Page Title -->
	    <div class="navbar navbar-static-top pagetitle">
		    <div class="container">
                <div class="row">
				    <div class="col-md-6">
					    <Rock:PageIcon ID="PageIcon" runat="server" /> <h1><Rock:PageTitle ID="PageTitle" runat="server" /></h1> <Rock:Zone Name="Title Left" runat="server" />  
				    </div>
                    <div class="col-md-6">
                        <Rock:Zone Name="Title Right" runat="server" />
                    </div>
			    </div>
            </div>
	    </div>

    </header>

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
            <div class="col-md-3">
                <Rock:Zone Name="Sidebar 1" runat="server" />
            </div>
            <div class="col-md-6">
                <Rock:Zone Name="Main" runat="server" />
            </div>
            <div class="col-md-3">
                <Rock:Zone Name="Sidebar 2" runat="server" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-12">
                <Rock:Zone Name="Section A" runat="server" />
            </div>
        </div>

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

        <!-- End Content Area -->

    </main>

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



