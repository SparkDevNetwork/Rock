<%@ Page ValidateRequest="false" Language="C#" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>
<%@ Import Namespace="System.Web.Optimization" %>
<!DOCTYPE html>

<script runat="server">

    /// <summary>
    /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
    protected override void OnInit( EventArgs e )
    {
        base.OnInit( e );
        
        //
    }    
    
</script>

<html class="no-js">
<head runat="server">

    <meta http-equiv="X-UA-Compatible" content="IE=10" />
    <meta charset="utf-8">
    <title></title>
    
    <script src="<%# ResolveUrl("~/Scripts/modernizr.js") %>" ></script>
    <script src="<%# ResolveUrl("~/Scripts/jquery-1.10.2.min.js") %>"></script>

    <!-- Set the viewport width to device width for mobile -->
	<meta name="viewport" content="width=device-width, initial-scale=1.0">

	<!-- Included CSS Files -->
	<link rel="stylesheet" href="<%# ResolveRockUrl("~~/Styles/bootstrap.css") %>"/>
    <link rel="stylesheet" href="<%# ResolveRockUrl("~~/Styles/theme.css") %>"/>
	<link rel="stylesheet" href="<%# ResolveRockUrl("~/Styles/developer.css") %>"/>

    <!-- Icons -->
    <link rel="shortcut icon" href="<%# ResolveRockUrl("~/Assets/Icons/favicon.ico") %>">
    <link rel="apple-touch-icon-precomposed" sizes="144x144" href="<%# ResolveRockUrl("~/Assets/Icons/touch-icon-ipad-retina.png") %>">
    <link rel="apple-touch-icon-precomposed" sizes="114x114" href="<%# ResolveRockUrl("~/Assets/Icons/touch-icon-iphone-retina.png") %>">
    <link rel="apple-touch-icon-precomposed" sizes="72x72" href="<%# ResolveRockUrl("~/Assets/Icons/touch-icon-ipad.png") %>">
    <link rel="apple-touch-icon-precomposed" href="<%# ResolveRockUrl("~/Assets/Icons/touch-icon-iphone.png") %>">
    
</head>
<body>

    <form id="form1" runat="server">
    
        <!-- Page Header -->
        <header class="pagerheader">
            <div class="container">
            
                    <!-- Brand Bar -->
			        <div class="navbar-header">
                        <asp:HyperLink ID="hlHome" runat="server" CssClass="navbar-brand" NavigateUrl="~" ToolTip="Rock">
                            <asp:Image ID="imgLogo" runat="server" AlternateText="Rock" ImageUrl="~/Assets/Images/rock-logo.svg" CssClass="brandbar-logo" />
                        </asp:HyperLink>
                    
                        <button class="navbar-toggle" type="button" data-toggle="collapse" data-target=".pagenav">
                            <i class="fa fa-bars fa-2x"></i>
                        </button>
			        </div>	

                    <div class="pull-right header-zone">
                        <Rock:Zone Name="Header" runat="server" />
                    </div>
			
				    <Rock:SearchField ID="searchField" runat="server" />

            </div>


            <!-- Main Navigation -->
	        <nav class="pagenav navbar-collapse collapse">
                <div class="container">
                    <div class="">
                        <Rock:Zone Name="Navigation" runat="server" />
                    </div>
                </div>									
	        </nav>

        </header>

        <div class="personprofile">

            <div class="navbar navbar-static-top personprofilebar-bio">
                <div class="container">
                    <Rock:Zone Name="Individual Detail" runat="server" />
                </div>
            </div>

            <div class="navbar navbar-static-top personprofilebar-badge">
                <div class="container">
                    <div class="row">
                        <div class="badge-group col-sm-4">
                            <Rock:Zone Name="Badg Bar Left" runat="server" />
                        </div>
                        <div class="badge-group col-sm-4">
                            <Rock:Zone Name="Badg Bar Middle" runat="server" />
                        </div>
                        <div class="badge-group col-sm-4">
                            <Rock:Zone Name="Badg Bar Right" runat="server" />
                        </div>
                    </div>
                </div>
            </div>

            <div class="navbar navbar-static-top personprofilebar-family">
			    <div class="container">    
                    <Rock:Zone Name="Family Detail" runat="server" />
                </div>
            </div>

		    <div class="container pagetabs">
                <div class="row">
                    <div class="col-md-12">
                        <Rock:Zone Name="Sub Navigation" runat="server" />
                    </div>
                </div> 
		    </div>

            <div class="container person-content">
                <div class="row">
                    <div class="col-md-8">
                        <Rock:Zone Name="Section A1" runat="server" />
                    </div>
                    <div class="col-md-4">
                        <Rock:Zone Name="Section A2" runat="server" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-4">
                        <Rock:Zone Name="Section B1" runat="server" />
                    </div>
                    <div class="col-md-4">
                        <Rock:Zone Name="Section B2" runat="server" />
                    </div>
                    <div class="col-md-4">
                        <Rock:Zone Name="Section B3" runat="server" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-12">
                        <Rock:Zone Name="Section C1" runat="server" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-4">
                        <Rock:Zone Name="Section D1" runat="server" />
                    </div>
                    <div class="col-md-8">
                        <Rock:Zone Name="Section D2" runat="server" />
                    </div>
                </div>
            </div>

	    </div>

        <footer class="page-footer">
            <div class="container">
                <div class="row">
                    <div class="col-md-12">
                        <Rock:Zone Name="Footer" runat="server" />
                    </div>
                </div>
            </div>
        </footer>

        <ajaxToolkit:ToolkitScriptManager ID="sManager" runat="server"/>

        <asp:UpdateProgress id="updateProgress" runat="server">
		        <ProgressTemplate>
		            <div class="updateprogress-status">
                        <div class="spinner">
                          <div class="rect1"></div>
                          <div class="rect2"></div>
                          <div class="rect3"></div>
                          <div class="rect4"></div>
                          <div class="rect5"></div>
                        </div>
                    </div>
                    <div class="updateprogress-bg modal-backdrop"> 
                         
		            </div>
		        </ProgressTemplate>
        </asp:UpdateProgress>

    </form>

</body>

</html>
