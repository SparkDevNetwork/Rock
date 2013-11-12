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
                        <asp:Image ID="imgHome" runat="server" AlternateText="Rock ChMS" ImageUrl="~/Assets/Images/rock-logo.svg" CssClass="brandbar-logo" />
                    </asp:HyperLink>
                    
                    <button class="navbar-toggle" type="button" data-toggle="collapse" data-target=".pageheader-collapse">
                        <span class="icon-bar"></span>
                        <span class="icon-bar"></span>
                        <span class="icon-bar"></span>
                    </button>

			    </div>	
                <div class="navbar-collapse collapse pageheader-collapse">   
                    <div class="navbar-right navbar-text">
                        <Rock:Zone ID="Header" Name="Header" runat="server" />
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
                <Rock:Zone ID="Navigation" runat="server" />
            </div>									
	    </nav>
		
        <!-- Page Title -->
	    <div class="navbar navbar-static-top pagetitle">
		    <div class="container">
                <div class="row">
				    <div class="col-md-6">
					    <Rock:PageIcon ID="PageIcon" runat="server" /> <h1><Rock:PageTitle ID="PageTitle" runat="server" /></h1> <Rock:Zone ID="TitleLeft" runat="server" />  
				    </div>
                    <div class="col-md-6">
                        <Rock:Zone ID="TitleRight" runat="server" />
                    </div>
			    </div>
            </div>
	    </div>

    </header>

    <main>
                
        <!-- Content Area -->

        <Rock:PageBreadCrumbs ID="PageBreadCrumbs" runat="server" />
                    
        <!-- Ajax Error -->
        <div class="alert alert-danger ajax-error" style="display:none">
            <p><strong>Error</strong></p>
            <span class="ajax-error-message" / ></span>
        </div>

        <div class="row">
            <div class="col-md-12">
                <Rock:Zone ID="Feature" runat="server" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-3">
                <Rock:Zone ID="Sidebar1" runat="server" />
            </div>
            <div class="col-md-9">
                <div class="panel panel-default">
                    <div class="panel-body">
                        <Rock:Zone ID="Main" runat="server" />
                    </div>
                </div>
            </div>
        </div>

        <div class="row">
            <div class="col-md-12">
                <Rock:Zone ID="SectionA" runat="server" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-4">
                <Rock:Zone ID="SectionB" runat="server" />
            </div>
            <div class="col-md-4">
                <Rock:Zone ID="SectionC" runat="server" />
            </div>
            <div class="col-md-4">
                <Rock:Zone ID="SectionD" runat="server" />
            </div>
        </div>

        <!-- End Content Area -->

    </main>

    <footer class="page-footer">
        <div class="row">
            <div class="col-md-12">
                <Rock:Zone ID="Footer" runat="server" />
            </div>
        </div>
    </footer>

</asp:Content>
