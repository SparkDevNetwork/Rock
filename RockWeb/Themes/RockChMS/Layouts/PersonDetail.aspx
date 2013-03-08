<%@ Page Title="" ValidateRequest="false" Language="C#" MasterPageFile="~/Themes/RockChMS/Layouts/Site.Master" 
    AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<script runat="server">

    protected override void OnInit( EventArgs e )
    {
        base.OnInit( e );
        AddCSSLink( Page, ResolveUrl( "~/CSS/jquery.tagsinput.css" ) );
        AddScriptLink( Page, ResolveUrl( "~/Scripts/jquery.tagsinput.js" ) );
    }
    
</script>
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
									
									<Rock:Zone ID="zHeader" Name="Header" runat="server" />
									
								</div>
							</div> <!-- collapse container -->
							
							<Rock:SearchField ID="searchField" runat="server" />
						</div> <!-- end column -->
					</div> <!-- end row -->

				</div> <!-- end container -->
			</div> <!-- end navbar-inner -->
		</header>
		
		<nav class="navbar navbar-static-top pagenav">
			<div class="navbar-inner">
				<div class="container-fluid">
					
                    <Rock:Zone ID="Menu" runat="server" />										
					
				</div>
			</div>
		</nav>
		
		<div class="navbar navbar-static-top pagetitle">
			<div class="navbar-inner">
				<div class="container-fluid">
					<div class="row-fluid">
						<div class="span6">
							<h1><Rock:PageTitle ID="PageTitle" runat="server" /></h1> <Rock:Zone ID="PageTitleBar" runat="server" />
						</div>
                        <div class="span6">
                            <Rock:Zone ID="Zone1" runat="server" />
                        </div>
					</div>
				</div>
			</div>
		</div>
		
		<div class="container-fluid body-content">
			<div class="row-fluid">
				<div class="span12">

<%-- Content Area --%>
                    <div id="person-profile" class="row-fluid">
                        <div class="span3">
                            <div class="bio-wrap clearfix">
                                <Rock:Zone ID="Bio" runat="server"/>
                                <aside class="bio-details">
                                    <Rock:Zone ID="BioDetails" runat="server"/>
                                </aside>
                            </div>
                        </div>
                        <div class="span9 tags-notes-attributes">
                            <Rock:Zone ID="Tags" runat="server"/>
                            <div class="row-fluid note-attribute-column">
                                <Rock:Zone ID="Notes" runat="server"/>
                                <div class="span4">
                                    <aside class="supplemental-info">
                                        <Rock:Zone ID="SupplementalInfo" runat="server"/>
                                    </aside>
                                </div>
                            </div>
                        </div>
                    </div>
<%-- End Content Area --%>

				</div>
			</div>
		</div>
		
		<footer class="page-footer">
			<div class="container-fluid">
				<div class="row-fluid">
					<div class="span12">
						<Rock:Zone ID="Footer" runat="server" />
					</div>
				</div>
			</div>
		</footer>
    
    <script>

        $('ul.addresses li').live({
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

    </script>
            
</asp:Content>

