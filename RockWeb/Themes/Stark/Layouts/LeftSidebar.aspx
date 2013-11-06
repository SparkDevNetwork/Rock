<%@ Page ValidateRequest="false" Language="C#" MasterPageFile="Site.Master"
    AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">

        <!-- Page Header -->
    <header>
        
        <!-- Brand Bar -->
        <nav class="navbar navbar-inverse navbar-static-top brandbar" role="navigation">
            <div class="container">
			    <div class="navbar-header">
                   <button class="navbar-toggle" type="button" data-toggle="collapse" data-target=".navbar-collapse">
                        <span class="icon-bar"></span>
                        <span class="icon-bar"></span>
                        <span class="icon-bar"></span>
                    </button>
                    <a class="navbar-brand" href="#">Stark Theme</a>
                    <Rock:Zone Name="Header" runat="server" />
			    </div>	
                <div class="navbar-collapse collapse">   
                    <!-- Main Navigation -->
                    <Rock:Zone Name="Navigation" runat="server" />
			    </div>	
            </div>
        </nav>

    </header>

    <main class="container">
                
        <!-- Start Content Area -->

        <!-- Page Title -->
        <Rock:PageIcon ID="PageIcon" runat="server" /> <h1><Rock:PageTitle ID="PageTitle" runat="server" /></h1>

        <Rock:PageBreadCrumbs ID="PageBreadCrumbs" runat="server" />
                    
        <!-- Ajax Error -->
        <div class="alert alert-danger ajax-error" style="display:none">
            <p><strong>Error</strong></p>
            <span class="ajax-error-message"></span>
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
            <div class="col-md-9">
                <Rock:Zone Name="Main" runat="server" />
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

    <footer>
        <div class="container">
            
            <hr />
            
            <div class="row">
                <div class="col-md-12">
                    <Rock:Zone Name="Footer" runat="server" />
                </div>
            </div>

            <div class="row">
                <div class="col-md-3 col-md-offset-8">
                    <Rock:Zone ID="Login" runat="server" />
                </div>
            </div>

        </div>
    </footer>

</asp:Content>
