<%@ Page ValidateRequest="false" Language="C#" MasterPageFile="~/Themes/RockChMS/Layouts/Site.Master" Trace="false"
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

    <div class="persondetails">

        <div class="navbar navbar-static-top persondetails-header">
            <div class="container">
                <Rock:Zone ID="HeaderZone" runat="server" />
            </div>
        </div>

        <div class="navbar navbar-static-top persondetails-badgebar">
            <div class="container">
                <div class="row">
                    <div class="badge-group col-md-3">
                        <Rock:Zone ID="BadgBarZone1" runat="server" />
                    </div>
                    <div class="badge-group col-md-3">
                        <Rock:Zone ID="BadgBarZone2" runat="server" />
                    </div>
                    <div class="badge-group col-md-6">
                        <Rock:Zone ID="BadgBarZone3" runat="server" />
                    </div>
                </div>
            </div>
        </div>

        <div class="navbar navbar-static-top persondetails-familybar">
			<div class="container">    
                <Rock:Zone ID="FamilyZone" runat="server" />
            </div>
        </div>

		<div class="container pagetabs">
            <div class="row">
                <div class="col-md-12">
                    <Rock:Zone ID="TabsZone" runat="server" />
                </div>
            </div> 
		</div>

        <div class="container">
            <div class="row">
                <div class="col-md-8">
                    <Rock:Zone ID="ContentZoneLeft" runat="server" />
                </div>
                <div class="col-md-4">
                    <Rock:Zone ID="ContentZoneRight" runat="server" />
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

        $('.persondetails-familybar .address').live({
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

        $('.panel .panel-heading').live({
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

        $('.panel-notes .panel-heading').live({
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

        $('.persondetails-familybar').live({
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

        $('.persondetails-header').live({
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

