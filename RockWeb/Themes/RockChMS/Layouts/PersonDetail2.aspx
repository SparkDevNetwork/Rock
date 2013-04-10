<%@ Page ValidateRequest="false" Language="C#" MasterPageFile="~/Themes/RockChMS/Layouts/Site.Master" 
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
									
									<Rock:Zone ID="Heading" Name="Header" runat="server" />
									
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
		<div class="persondetails">
		    <div class="navbar navbar-static-top personheader">
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
		
            <div class="navbar navbar-static-top personbadgebar">
			    <div class="navbar-inner">
				    <div class="container-fluid">
					    <div class="row-fluid">
                                <div class="badge-group span3">
                                    <img src="../../../Assets/Mockup/era.jpg" class="rock-badge" />
                                    <img src="../../../Assets/Mockup/attendence-16.jpg" class="rock-badge" />
                                    <img src="../../../Assets/Mockup/volunteer.jpg" class="rock-badge" />
                                    <img src="../../../Assets/Mockup/disc.jpg" class="rock-badge" />
                                </div>
                                <div class="badge-group span3">
                                    <img src="../../../Assets/Mockup/attendence-bars.jpg" class="rock-badge" />
                                </div>
                                <div class="badge-group span6">
                                    <img src="../../../Assets/Mockup/next-steps.jpg" class="rock-badge" />
                                </div>
                        </div>
                    </div>
                </div>
            </div>

            <div class="navbar navbar-static-top personfamilybar">
			    <div class="navbar-inner">
				    <div class="container-fluid">
					    <div class="row-fluid">
						    <div class="span8 groupmembers clearfix">
                                <header>Edmiston Family <a href="#" class="edit"><i class="icon-edit"></i></a></header>

                                <ul>
                                    <li>
                                        <a href="">
                                            <img src="../../../Assets/Mockup/heidi.jpg")" />
                                            <div class="groupmembers-details">
                                                <h4>Heidi</h4> <small class="age">(39)</small>
                                                <small>Wife</small>
                                            </div>
                                        </a>
                                    </li>
                                    <li>
                                        <a href="">
                                            <img src="../../../Assets/Mockup/alex.jpg" />
                                            <div class="groupmembers-details">
                                                <h4>Alex</h4> <small class="age">(10)</small>
                                                <small>Son</small>
                                            </div>
                                        </a>
                                    </li>
                                    <li>
                                        <a href="">
                                            <img src="../../../Assets/Mockup/adam.jpg" />
                                            <div class="groupmembers-details">
                                                <h4>Adam</h4> <small class="age">(9)</small>
                                                <small>Son</small>
                                            </div>
                                        </a>
                                    </li>
                   
                                    <li>
                                        <a href="">
                                            <img src="../../../Assets/Mockup/rachael-sue.jpg" />
                                            <div class="groupmembers-details">
                                                <h4>Rachael-Sue</h4> <small class="age">(2)</small>
                                                <small>Pet</small>
                                            </div>
                                        </a>
                                    </li>
                                    <li>
                                        <a href="">
                                            <img src="../../../Assets/Mockup/monica.jpg" />
                                            <div class="groupmembers-details">
                                                <h4>Monica</h4> <small class="age">(1)</small>
                                                <small>Pet</small>
                                            </div>
                                        </a>
                                    </li>
                    
                                </ul>
                            </div>
                            <div class="span4 group-addresses clearfix">
                               <ul>
                                    <li class="clearfix">
                                        <h4>Home Address</h4>
                                        <a href="" class="map"><i class="icon-map-marker"></i></a>
                                        <div class="address">
                                           9039 W Molly Ln<br />
                                           Peoria, AZ 85383
                                        </div>
                                        <div class="actions">
                                            <a href="" title="GPS: 33.7281 -112.2546"><i class="icon-globe"></i></a>
                                            <a href="" title="Address Standardized"><i class="icon-magic"></i></a>
                                        </div>
                                    </li>

                                   <li class="clearfix">
                                        <h4>Previous Address</h4>
                                        <a href="" class="map"><i class="icon-map-marker"></i></a>
                                        <div class="address">
                                           1730 E Rose Garden Ln<br />
                                           Phoenix, AZ 85024
                                        </div>
                                        <div class="actions">
                                            <a href="" title="GPS: 33.7281 -112.2546"><i class="icon-globe"></i></a>
                                            <a href="" title="Address Standardized"><i class="icon-magic"></i></a>
                                        </div>
                                    </li>
                                </ul>
                            </div>
                        </div>
                    </div>
                </div>
            </div>


		    <div class="container-fluid body-content">
			    <div class="row-fluid badgebar">
				    <div class="span12">



				    </div>
			    </div>
		    </div>
        </div>

<%-- Changes End Here --%>

		
		<footer class="page-footer">
			<div class="container-fluid">
				<div class="row-fluid">
					<div class="span12">
						<Rock:Zone ID="Footer" runat="server" />
					</div>
				</div>
			</div>
		</footer>
    
    <style type="text/css">

        .persondetails small
        {
            color: #7c7c7c;
        }

        .persondetails a i[class^="icon-"]
        {
            color: #9e9e9e;
        }

        .persondetails a:hover i[class^="icon-"]
        {
            text-decoration: none;
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

        .personbadgebar .navbar-inner
        {
            background-color: #e6e6e6;
            background-image: none;
            box-shadow: none;
            border: 0;
            min-height: 50px;
            border-bottom: 1px solid #c8c8c8;
        }

        .personbadgebar .badge-group
        {
            margin: 10px 0;
        }

        .personbadgebar .badge-group:last-child
        {
            text-align: right;
        }

        @media (max-width: 767px) {
	       .personbadgebar .badge-group:last-child
            {
                text-align: left;
            }
        }

        .personbadgebar .rock-badge
        {
            margin-right: 12px;
        }

        .personfamilybar .navbar-inner
        {
            background: #dfdfdf;
            padding: 12px 0;
            border-bottom: 1px solid #c8c8c8;
        }

        .personfamilybar .groupmembers header
        {
            font-family: 'OpenSansSemibold', 'Helvetica Neue',Helvetica,Arial,sans-serif;
        }

        .personfamilybar .groupmembers ul
        {
            list-style: none;
            margin: 6px 0;
        }

        .personfamilybar .groupmembers li
        {
            float: left;
            margin-right: 12px;
            margin-bottom: 12px;
            min-width: 130px;
        }

        .personfamilybar .groupmembers li a
        {
            color: #515151;
        }

        .personfamilybar .groupmembers li img
        {
            float: left;
            border: 1px solid #fff;
        }

        .personfamilybar .groupmembers-details
        {
            float: left;
            margin-left: 4px;
        }

        .personfamilybar .groupmembers-details h4
        {
            margin: 0;
            font-size: 14px;
            font-weight: normal;
            font-family: 'OpenSans', 'Helvetica Neue',Helvetica,Arial,sans-serif;
            float: left;
        }

        .personfamilybar .groupmembers-details small
        {
            display: block;
        }

        .personfamilybar .groupmembers-details small.age
        {
            float: left;
            margin-left: 4px;
        }

        .personfamilybar .group-addresses ul
        {
            list-style: none;
            margin: 0;
        }

        .personfamilybar .group-addresses li
        {
            display: block;
            clear: both;
            margin-bottom: 12px;
        }

        .personfamilybar .group-addresses h4
        {
            margin: 0;
            font-size: 14px;
            font-family: 'OpenSansSemibold', 'Helvetica Neue',Helvetica,Arial,sans-serif;
            font-weight: normal;
        }

        .personfamilybar .group-addresses a.map,
        .personfamilybar .group-addresses div.actions,
        .personfamilybar .group-addresses div.address
        {
            float: left;
            margin-right: 4px;
        }



    </style>
        
    <script>

       

    </script>
            
</asp:Content>

