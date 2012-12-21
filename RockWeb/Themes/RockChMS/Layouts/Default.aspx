<%@ Page Title="" ValidateRequest="false" Language="C#" MasterPageFile="~/Themes/RockChMS/Layouts/Site.Master" 
    AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">

    <script type="text/javascript">

        Sys.Application.add_load(function () {
            $('a.brand').attr('href', rock.baseUrl);
        });

    </script>
    
    <!-- Page Header -->
		<header class="navbar navbar-static-top pageheader">
			<div class="navbar-inner">
				<div class="container-fluid">
					<div class="row-fluid">
						<div class="span2 clearfix">
	
							<a class="brand"><img alt="Rock ChMS" src="/RockWeb/Assets/Images/rock-logo.svg" class="pageheader-logo" /></a>
					
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
		
		<div class="container-fluid">
			<div class="row-fluid">
				<div class="span12">
					<Rock:Zone ID="Content" runat="server" />
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
        
</asp:Content>

