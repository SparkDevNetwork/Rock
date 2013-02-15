<%@ Page Title="" ValidateRequest="false" Language="C#" MasterPageFile="~/Themes/RockChMS/Layouts/Site.Master" 
    AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<script runat="server">

    protected override void OnInit( EventArgs e )
    {
        base.OnInit( e );
        AddCSSLink( Page, ResolveUrl( "~/CSS/jquery.tagsinput.css" ) );
        AddScriptLink( Page, ResolveUrl( "~/Scripts/jquery.tagsinput.js" ) );
        AddScriptLink( Page, ResolveUrl( "~/Scripts/tinyscrollbar.min.js" ) );
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


<%-- Changes Start Here --%>
		
		<div class="navbar navbar-static-top pagetitle">
			<div class="navbar-inner">
				<div class="container-fluid">
					<div class="row-fluid">
						<div class="span6">
							<h1>Jon Edmiston</h1>
						</div>
                        <div class="span6 person-labels">
                            <span class="label label-success">Member</span>
                            <span class="label">Westwing</span>
                            <span class="label label-info">Peoria</span>
                            <span class="label label-important">Inactive</span>

                            <ul class="nav pull-right">
                                <li class="dropdown">
                                    <a data-toggle="dropdown" href="#" class="dropdown-toggle" tabindex="0">
                                        <i class="icon-cog"></i>
                                        <span>Actions</span>
                                        <b class="caret"></b>
                                    </a>
                                    <ul class="dropdown-menu">
                                        <li>
                                            <a href="/MyAccount" tabindex="0">Add to Starting Point</a>
                                            <a href="/MyAccount" tabindex="0">Add to Foundations</a>
                                        </li>
                                        <li class="divider"></li>
                                        <li><a href="">Report Data Error</a></li>
                                    </ul>
                                </li>
                            </ul>
                        </div>
                        
					</div> <!-- end row -->
                    <div class="row-fluid">
						<div class="span2">
                            <div class="personimage-wrap">
                                <img src="/Assets/Mockup/jon.jpg" width="140" alt="Jon Edmiston" />
                            </div>
                        </div>

                        <div class="span4">
                            <div class="personsummary">
                                <div class="personsummary-tags">#tags here#</div>
                                <div class="personsummary-demographics">
                                    36 yrs old <small>(2/10)</small> <br />
                                    Male <br />
                                    Married 17 yrs <small>(12/23)</small>
                                </div>
                            </div>
                        </div>

                        <div class="span6">
                            <div class="personcontact">
                                <div class="personcontact-phone">
                                    Unlisted <small>Home</small> <br />
                                    (555) 867-5309 <small>Cell</small> <br />
                                    (555) 847-5329 <small>Work</small> <br />
                                </div>
                                <div class="personcontact-email">
                                    jonedmiston@ccvonline.com
                                </div>
                            </div>
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


<%-- Changes End Here --%>

		
		<footer class="page-footer navbar-fixed-bottom">
			<div class="container-fluid">
				<div class="row-fluid">
					<div class="span12">
						<Rock:Zone ID="Footer" runat="server" />
					</div>
				</div>
			</div>
		</footer>
    
    <style type="text/css">

        small
        {
            color: #7c7c7c;
        }
        
        .pagetitle.navbar .nav > li > a,
        .pagetitle.navbar .nav li.dropdown > .dropdown-toggle .caret
        {
            color: #555;
        }

        .pagetitle.navbar .nav li.dropdown > .dropdown-toggle .caret
        {
            border-bottom-color: #555;
            border-top-color: #555;
        }
        
        .pagetitle.navbar .nav li.dropdown.open > .dropdown-toggle, 
        .pagetitle.navbar .nav li.dropdown.active > .dropdown-toggle, 
        .pagetitle.navbar .nav li.dropdown.open.active > .dropdown-toggle {
            color: inherit;
        }

        .pagetitle.navbar li
        {
            text-align: left;
        }

        .person-labels {
            text-align: right;
            margin-top: 12px;
        }

        .person-labels .label
        {
            margin-top: 11px;
        }

        .personimage-wrap
        {
            padding: 5px 5px 5px 5px;
            background-color: #fff;
            border: 1px solid #bdbdbd;
            display: table;
            width: 100%;
        }

        .personimage-wrap img
        {
            width: 100%;
        }

        .personsummary-tags
        {
            margin-bottom: 44px;
        }

        .personcontact-phone
        {
            margin-bottom: 12px;
        }

    </style>
        
    <script>

       

    </script>
            
</asp:Content>

