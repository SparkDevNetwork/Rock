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

    <div class="persondetails">

        <div class="navbar navbar-static-top persondetails-header">
            <div class="container">
                <Rock:Zone ID="IndividualDetail" runat="server" />
            </div>
        </div>

        <div class="navbar navbar-static-top persondetails-badgebar">
            <div class="container">
                <div class="row">
                    <div class="badge-group col-md-3">
                        <Rock:Zone ID="BadgBarLeft" runat="server" />
                    </div>
                    <div class="badge-group col-md-3">
                        <Rock:Zone ID="BadgBarMiddle" runat="server" />
                    </div>
                    <div class="badge-group col-md-6">
                        <Rock:Zone ID="BadgBarRight" runat="server" />
                    </div>
                </div>
            </div>
        </div>

        <div class="navbar navbar-static-top persondetails-familybar">
			<div class="container">    
                <Rock:Zone ID="FamilyDetail" runat="server" />
            </div>
        </div>

		<div class="container pagetabs">
            <div class="row">
                <div class="col-md-12">
                    <Rock:Zone ID="SubNavigation" runat="server" />
                </div>
            </div> 
		</div>

        <div class="container person-content">
            <div class="row">
                <div class="col-md-8">
                    <Rock:Zone ID="Main" runat="server" />
                </div>
                <div class="col-md-4">
                    <Rock:Zone ID="Sidebar1" runat="server" />
                </div>
            </div>
        </div>

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

    <script>

        $('.persondetails-familybar .address').on({
            mouseenter:
                function () {
                    var actionsDiv = $('div.actions', this);
                    if (actionsDiv.length > 0) {
                        $(actionsDiv).fadeToggle();
                    }
                },
            mouseleave:
                function () {
                    var actionsDiv = $('div.actions', this);
                    if (actionsDiv.length > 0) {
                        $(actionsDiv).fadeToggle();
                    }
                }
        });

        $('.panel .panel-heading').on({
            mouseenter:
                function () {
                    var actionsDiv = $('.actions', this);
                    if (actionsDiv.length > 0) {
                        $(actionsDiv).fadeToggle();
                    }
                },
            mouseleave:
                function () {
                    var actionsDiv = $('.actions', this);
                    if (actionsDiv.length > 0) {
                        $(actionsDiv).fadeToggle();
                    }
                }
        });

        $('.panel-notes .panel-heading').on({
            mouseenter:
                function () {
                    var actionsDiv = $('.actions', this);
                    if (actionsDiv.length > 0) {
                        $(actionsDiv).fadeToggle();
                    }
                },
            mouseleave:
                function () {
                    var actionsDiv = $('.actions', this);
                    if (actionsDiv.length > 0) {
                        $(actionsDiv).fadeToggle();
                    }
                }
        });

        $('.persondetails-familybar').on({
            mouseenter:
                function () {
                    var actionsDiv = $('.container .actions', this);
                    if (actionsDiv.length > 0) {
                        $(actionsDiv).fadeToggle();
                    }
                },
            mouseleave:
                function () {
                    var actionsDiv = $('.container .actions', this);
                    if (actionsDiv.length > 0) {
                        $(actionsDiv).fadeToggle();
                    }
                }
        });

        $('.persondetails-header').on({
            mouseenter:
                function () {
                    var actionsDiv = $('.actions', this);
                    if (actionsDiv.length > 0) {
                        $(actionsDiv).fadeToggle();
                    }
                },
            mouseleave:
                function () {
                    var actionsDiv = $('.actions', this);
                    if (actionsDiv.length > 0) {
                        $(actionsDiv).fadeToggle();
                    }
                }
        });

    </script>

</asp:Content>

