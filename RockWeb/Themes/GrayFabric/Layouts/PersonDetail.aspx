<%@ Page ValidateRequest="false" Language="C#" MasterPageFile="Site.Master" Trace="false"
    AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">

    <!-- Page Header -->
    <header class="navbar navbar-static-top pageheader">
        <div class="navbar-inner">
            <div class="container-fluid">
                <div class="row-fluid">
                    <div class="span2 clearfix">

                        <asp:HyperLink ID="HyperLink1" runat="server" CssClass="brand" NavigateUrl="~" ToolTip="Rock ChMS">
                            <asp:Image ID="Image1" runat="server" AlternateText="Rock ChMS" ImageUrl="~/Assets/Images/rock-logo.svg" CssClass="pageheader-logo" />
                        </asp:HyperLink>

                    </div>

                    <div class="span10 clearfix">

                        <div class="pageheader-collapse pull-right">
                            <a class="btn btn-navbar" data-target=".nav-collapse" data-toggle="collapse">
                                <span class="icon-bar"></span>
                                <span class="icon-bar"></span>
                                <span class="icon-bar"></span>
                            </a>

                            <div class="nav-collapse collapse">

                                <Rock:Zone Name="Heading" runat="server" />

                            </div>
                        </div>
                        <!-- collapse container -->

                        <Rock:SearchField ID="searchField" runat="server" />
                    </div>
                    <!-- end column -->
                </div>
                <!-- end row -->

            </div>
            <!-- end container -->
        </div>
        <!-- end navbar-inner -->
    </header>

    <nav class="navbar navbar-static-top pagenav">
        <div class="navbar-inner">
            <div class="container-fluid">

                <Rock:Zone Name="Menu" runat="server" />

            </div>
        </div>
    </nav>

    <div class="persondetails">

        <div class="navbar navbar-static-top persondetails-header">
            <div class="navbar-inner">
                <div class="container-fluid">
                    <Rock:Zone Name="HeaderZone" runat="server" />
                </div>
            </div>
        </div>

        <div class="navbar navbar-static-top persondetails-badgebar">
            <div class="navbar-inner">
                <div class="container-fluid">
                    <div class="row-fluid">
                        <div class="badge-group span3">
                            <Rock:Zone Name="Badg Bar Zone 1" runat="server" />
                        </div>
                        <div class="badge-group span3">
                            <Rock:Zone Name="Badg Bar Zone 2" runat="server" />
                        </div>
                        <div class="badge-group span6">
                            <Rock:Zone Name="Badg Bar Zone 3" runat="server" />
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div class="navbar navbar-static-top persondetails-familybar">
			<div class="navbar-inner">
				<div class="container-fluid">    
                    <Rock:Zone Name="Family Zone" runat="server" />
                </div>
            </div>
        </div>

		<div class="container-fluid pagetabs">
            <div class="row-fluid">
                <div class="span12">
                    <Rock:Zone Name="Tabs Zone" runat="server" />
                </div>
            </div> 
		</div>

        <div class="container-fluid">
            <div class="row-fluid">
                <div class="span8">
                    <Rock:Zone Name="Content Zone Left" runat="server" />
                </div>
                <div class="span4">
                    <Rock:Zone Name="Content Zone Right" runat="server" />
                </div>
            </div>
        </div>

	</div>

    <footer class="page-footer">
        <div class="container-fluid">
            <div class="row-fluid">
                <div class="span12">
                    <Rock:Zone Name="Footer" runat="server" />
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

        $('.widget header').on({
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
                    var actionsDiv = $('.container-fluid > .actions', this);
                    if (actionsDiv.length > 0) {
                        $(actionsDiv).fadeToggle();
                    }
                },
            mouseleave:
                function () {
                    var actionsDiv = $('.container-fluid > .actions', this);
                    if (actionsDiv.length > 0) {
                        $(actionsDiv).fadeToggle();
                    }
                }
        });

        $('.persontimeline article').on({
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

