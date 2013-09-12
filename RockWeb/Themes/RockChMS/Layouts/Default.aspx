<%@ Page ValidateRequest="false" Language="C#" MasterPageFile="~/Themes/RockChMS/Layouts/Site.Master" 
    AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">
    
    <!-- Page Header -->
    <header>
        
        <!-- Brand Bar -->
        <div class="navbar navbar-default navbar-static-top brandbar" role="navigation">
            <div class="container">
			    <div class="navbar-header">
                    
                    <asp:HyperLink ID="HyperLink1" runat="server" CssClass="navbar-brand" NavigateUrl="~" ToolTip="Rock ChMS">
                        <asp:Image ID="Image1" runat="server" AlternateText="Rock ChMS" ImageUrl="~/Assets/Images/rock-logo.svg" CssClass="brandbar-logo" />
                    </asp:HyperLink>
                    
                    <button class="navbar-toggle" type="button" data-toggle="collapse" data-target=".pageheader-collapse">
                        <span class="icon-bar"></span>
                        <span class="icon-bar"></span>
                        <span class="icon-bar"></span>
                    </button>

			    </div>	
                <div class="navbar-collapse collapse pageheader-collapse">   
                    <div class="navbar-right navbar-text">
                        <Rock:Zone ID="Heading" Name="Header" runat="server" />
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
                <Rock:Zone ID="Menu" runat="server" />
            </div>									
	    </nav>
		
        <!-- Page Title -->
	    <div class="navbar navbar-static-top pagetitle">
		    <div class="container">
                <div class="row">
				    <div class="col-md-6">
					    <Rock:PageIcon ID="PageIcon" runat="server" /> <h1><Rock:PageTitle ID="PageTitle" runat="server" /></h1> <Rock:Zone ID="PageTitleBar" runat="server" />  
				    </div>
                    <div class="col-md-6">
                        <Rock:Zone ID="Zone1" runat="server" />
                    </div>
			    </div>
            </div>
	    </div>

    </header>
		
	<div class="body-content container">
            
                <Rock:PageBreadCrumbs ID="PageBreadCrumbs" runat="server" />

                <!-- display any ajax error messages here (use with ajax-client-error-handler.js) -->
                <div class="alert alert-error ajax-error" style="display:none">
                    <strong>Ooops!</strong>
                    <span class="ajax-error-message"></span>
                </div>

                <Rock:Zone ID="Content" runat="server" />
	</div>
		
	<footer class="page-footer">
		<div class="container">
			<div class="row">
				<div class="col-md-12">
					<Rock:Zone ID="Footer" runat="server" />
				</div>
			</div>
		</div>
	</footer>
        
</asp:Content>

