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
		    <div class="navbar navbar-static-top persondetails-header">
			    <div class="navbar-inner">
				    <div class="container-fluid">
                        <div class="actions" style="display: none;">
                            <a href="#" class="edit btn btn-mini"><i class="icon-pencil"></i> Edit Individual</a>
                        </div>
					    <div class="row-fluid">
						    <div class="span6">
							    <h1>Jon Edmiston</h1>
						    </div>
                            <div class="span6 labels">
                                <span class="label label-success">Member</span>
                                <span class="label">Westwing</span>
                                <span class="label label-info">Peoria</span>
                                <span class="label label-important">Inactive</span>

                                <ul class="nav pull-right">
                                    <li class="dropdown">
                                        <a class="persondetails-actions" data-toggle="dropdown" href="#" class="dropdown-toggle" tabindex="0">
                                            <i class="icon-cog"></i>
                                            <span>Actions</span>
                                            <b class="caret"></b>
                                        </a>
                                        <ul class="dropdown-menu">
                                            <li>
                                                <a href="/MyAccount" tabindex="0">Add to Starting Point</a>
                                                <a href="/MyAccount" tabindex="0">Add to Foundations</a>
                                                <a href="/MyAccount" tabindex="0">Email Individual</a>
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
                                <div class="photo">
                                    <img src="/Assets/Mockup/jon.jpg" width="140" alt="Jon Edmiston" />
                                </div>
                            </div>

                            <div class="span4">
                                <div class="summary">
                                    <div class="tags">#tags here#</div>
                                    <div class="demographics">
                                        36 yrs old <small>(2/10)</small> <br />
                                        Male <br />
                                        Married 17 yrs <small>(12/23)</small>
                                    </div>
                                </div>
                            </div>

                            <div class="span6">
                                <div class="personcontact">

                                    <ul class="unstyled phonenumbers">
                                        <li data-value="(555)555-5555">Unlisted <small>Home</small></li>
                                        <li data-value="(555)555-5555">(555) 867-5309 <small>Cell</small></li>
                                        <li data-value="(555)555-5555">(555) 847-5329 <small>Work</small></li>
                                    </ul>

                                    <div class="email">
                                        jonedmiston@ccvonline.com
                                    </div>
                                </div>
                            </div>
                        </div>
				    </div>
			    </div>
		    </div>
		
            <div class="navbar navbar-static-top persondetails-badgebar">
			    <div class="navbar-inner">
				    <div class="container-fluid">
					    <div class="row-fluid">
                                <div class="badge-group span3">
                                    <div class="badge" data-toggle="tooltip" title="eRA as of 2/12/2011">
                                        <img src="../../../Assets/Mockup/era.jpg" />
                                    </div>
                                    <div class="badge" data-toggle="tooltip" title="Family has attended 14 times in the last 16 weeks">
                                        <img src="../../../Assets/Mockup/attendence-16.jpg" />
                                    </div>
                                    <div class="badge" data-toggle="tooltip" title="Currently serves in <br/>Neighborhood Group Leaders <br/> Children's Volunteers">
                                        <img src="../../../Assets/Mockup/volunteer.jpg" />
                                    </div>
                                    <div class="badge" data-toggle="tooltip" title="DISC Summary: S/C">
                                        <img src="../../../Assets/Mockup/disc.jpg" />
                                    </div>
                                </div>
                                <div class="badge-group span3">
                                    <div class="badge" data-toggle="tooltip" title="Family Attendance Summary for the last 12 months">
                                        <img src="../../../Assets/Mockup/attendence-bars.jpg" />
                                    </div>
                                </div>
                                <div class="badge-group span6">
                                    <div class="badge" data-toggle="tooltip" title="all of the next steps smashed together for mockup">
                                        <img src="../../../Assets/Mockup/next-steps.jpg" />
                                    </div>
                                </div>
                        </div>
                    </div>
                </div>
            </div>

            <div class="navbar navbar-static-top persondetails-familybar">
			    <div class="navbar-inner">
				    <div class="container-fluid">
                        <div class="actions" style="display: none;">
                            <a href="#" class="edit btn btn-mini"><i class="icon-pencil"></i> Edit Family</a>
                        </div>
					    <div class="row-fluid">
                            
						    <div class="span8 members clearfix">
                                <header>Edmiston Family </header>

                                <ul class="clearfix">
                                    <li>
                                        <a href="">
                                            <img src="../../../Assets/Mockup/heidi.jpg" />
                                            <div class="member">
                                                <h4>Heidi</h4> <small class="age">(39)</small>
                                                <small>Wife</small>
                                            </div>
                                        </a>
                                    </li>
                                    <li>
                                        <a href="">
                                            <img src="../../../Assets/Mockup/alex.jpg" />
                                            <div class="member">
                                                <h4>Alex</h4> <small class="age">(10)</small>
                                                <small>Son</small>
                                            </div>
                                        </a>
                                    </li>
                                    <li>
                                        <a href="">
                                            <img src="../../../Assets/Mockup/adam.jpg" />
                                            <div class="member">
                                                <h4>Adam</h4> <small class="age">(9)</small>
                                                <small>Son</small>
                                            </div>
                                        </a>
                                    </li>
                   
                                    <li>
                                        <a href="">
                                            <img src="../../../Assets/Mockup/rachael-sue.jpg" />
                                            <div class="member">
                                                <h4>Rachael-Sue</h4> <small class="age">(2)</small>
                                                <small>Pet</small>
                                            </div>
                                        </a>
                                    </li>
                                    <li>
                                        <a href="">
                                            <img src="../../../Assets/Mockup/monica.jpg" />
                                            <div class="member">
                                                <h4>Monica</h4> <small class="age">(1)</small>
                                                <small>Pet</small>
                                            </div>
                                        </a>
                                    </li>
                    
                                </ul>
                            </div>
                            <div class="span4 addresses clearfix">
                               <ul>
                                    <li class="address clearfix">
                                        <h4>Home Address</h4>
                                        <a href="" class="map" title="Map This Address"><i class="icon-map-marker"></i></a>
                                        <div class="address">
                                           9039 W Molly Ln<br />
                                           Peoria, AZ 85383
                                        </div>
                                        <div class="actions">
                                            <a href="" title="Geocoded | GPS: 33.7281 -112.2546"><i class="icon-globe"></i></a>
                                            <a href="" title="Address Standardized"><i class="icon-magic"></i></a>
                                        </div>
                                    </li>

                                   <li class="address clearfix">
                                        <h4>Previous Address</h4>
                                        <a href="" class="map" title="Map This Address"><i class="icon-map-marker"></i></a>
                                        <div class="address">
                                           1730 E Rose Garden Ln<br />
                                           Phoenix, AZ 85024
                                        </div>
                                        <div class="actions">
                                            <a href="" title="Geocoded | GPS: 33.7281 -112.2546"><i class="icon-globe"></i></a>
                                            <a href="" title="Address Standardized"><i class="icon-magic"></i></a>
                                        </div>
                                    </li>
                                </ul>

                            </div>
                        </div>
                    </div>
                </div>
            </div>



			<div class="container-fluid pagetabs">
                <div class="row-fluid">
                    <div class="span12">

							<ul class="nav nav-pills">
                                <li class="active">
                                    <a href="#">Person Details</a>
                                </li>
                                <li>
                                    <a href="#">Extended Attributes</a>
                                </li>
                       
                                <li>
                                    <a href="#">Groups</a>
                                </li>
                                <li>
                                    <a href="#">Staff Details</a>
                                </li>
                                <li>
                                    <a href="#">Contributions</a>
                                </li>
                                <li>
                                    <a href="#">History</a>
                                </li>
                            </ul>
									
                    </div>
                </div> 
			</div>

            <div class="container-fluid">
                <div class="row-fluid">
                    <div class="span8">
                        <section class="widget persontimeline">
                            <header class="clearfix"><h4><i class="icon-calendar"></i> Timeline</h4> <a class="btn btn-small add-note"><i class="icon-plus"></i></a></header>

                            <script>
                                $('.add-note').click(function(){
                                    $('.note-entry').slideToggle();
                                });
                            </script>

                            <div class="widget-content">
                                
                                <div class="note-entry clearfix" style="display: none;">
                                    <div class="note">
                                        <label>Note</label>
                                        <textarea></textarea>
                                    </div>
                                    
                                    <div class="settings clearfix">
                                        <div class="options">
                                            <label class="checkbox">
                                                <input type="checkbox" value="">
                                                Alert
                                            </label>
                                        
                                            <label class="checkbox">
                                                <input type="checkbox" value="">
                                                Private
                                            </label>
                                        </div>

                                        <button class="btn btn-mini security" type="button"><i class="icon-lock"></i> Security</button>
                                    </div>
                                    <div class="actions">
                                        <a class="btn btn-primary btn-small">Add Note</a>
                                        <a class="btn btn-small">Cancel</a>
                                    </div>
                                </div>
                                
                                <article class="clearfix highlight">
                                    <i class="icon-comment"></i>
                                    <div class="details">
                                        <h5>Bob Johnson <span class="date">4/1/2013</span></h5>
                                        Talk to security before allowing to serve in any ministry area. Appears to drive a white cargo van.
                                    </div>
                                </article>
                                <article class="clearfix">
                                    <i class="icon-calendar"></i>
                                    <div class="details">
                                        <h5>Event Registration  <span class="date">1/1/2013</span></h5>
                                        Register for Feed My Staving Puppies (Jon, Heidi, Alex and Adam)
                                    </div>
                                </article>
                                <article class="clearfix personal">
                                    <i class="icon-comment"></i>
                                    <div class="details">
                                        <h5>You - Personal Note <span class="date">10/11/2012</span></h5>
                                        Had Lunch with Jon today to talk about using new neighborhood map.
                                    </div>
                                    <div class="actions" style="display: none;">
                                        <a href=""><i class="icon-pencil"></i></a>
                                        <a href=""><i class="icon-remove"></i></a>
                                    </div>
                                </article>
                                <article class="clearfix">
                                    <i class="icon-envelope"></i>
                                    <div class="details">
                                        <h5>Email from: Dustin Tappan <span class="date">10/1/2012</span></h5>
                                        An email was sent to Jon from Dustin Tappan on 10/31 @9:35am.
                                    </div>
                                </article>
                                <article class="clearfix">
                                    <i class="icon-phone"></i>
                                    <div class="details">
                                        <h5>Call To Scott Merlin <span class="date">7/11/2012</span></h5>
                                        Jon called Scott Merlin's phone, 2999, on Monday October 12th at 10:51am and talked for 10mins.
                                    </div>
                                </article>
                                <article class="clearfix">
                                    <i class="icon-comment"></i>
                                    <div class="details">
                                        <h5>Bob Johnson <span class="date">3/11/2012</span></h5>
                                        Talked to Jon about joining the Security Team and gave him the forms needed to apply.
                                    </div>
                                </article>
                                <article class="clearfix">
                                    <i class="icon-calendar"></i>
                                    <div class="details">
                                        <h5>Event Registration  <span class="date">2/1/2012</span></h5>
                                        Register for Feed My Staving Puppies (Jon, Heidi, Alex and Adam)
                                    </div>
                                </article>
                                <article class="clearfix">
                                    <i class="icon-comment"></i>
                                    <div class="details">
                                        <h5>Mike McClain <span class="date">2/1/2012</span></h5>
                                        Had Lunch with Jon today to talk about using new neighborhood map.
                                    </div>
                                </article>
                                <article class="clearfix">
                                    <i class="icon-envelope"></i>
                                    <div class="details">
                                        <h5>Email from: Dustin Tappan  <span class="date">1/11/2012</span></h5>
                                        An email was sent to Jon from Dustin Tappan on 10/31 @9:35am.
                                    </div>
                                </article>
                                <article class="clearfix">
                                    <i class="icon-phone"></i>
                                    <div class="details">
                                        <h5>Call To Scott Merlin  <span class="date">1/1/2012</span></h5>
                                        Jon called Scott Merlin's phone, 2999, on Monday October 12th at 10:51am and talked for 10mins.
                                    </div>
                                </article>
                                <article class="clearfix">
                                    <i class="icon-comment"></i>
                                    <div class="details">
                                        <h5>7/1/2012 - Bob Johnson</h5>
                                        Talked to Jon about joining the Security Team and gave him the forms needed to apply.
                                    </div>
                                </article>

                                <a class="btn btn-mini load-more"><i class="icon-angle-down"></i> <span>Load More</span> <i class="icon-angle-down"></i></a>
                            </div>
                            
                        </section>
                    </div>
                    <div class="span4">
                        <section class="widget bookmarkattributes attributeholder">
                            <header class="clearfix">
                                <h4 class="pull-left"><i class="icon-bookmark"></i> Bookmarked Attributes</h4> 
                                <div class="actions pull-right">
                                    <a class="edit" href=""><i class="icon-pencil"></i></a>
                                    <a class="edit" href=""><i class="icon-cog"></i></a>
                                </div></header>
                            <div class="widget-content">
                                <ul>
                                    <li><strong>Baptism Date</strong> 12/16/2010</li>
                                    <li><strong>How Joined</strong> Baptism</li>
                                    <li><strong>Last Gave</strong> 3/1/2013</li>
                                    <li><strong>T-shirt Size</strong> Size L</li>
                                    <li><strong>Favorite Movie</strong> We Were Soliders</li>
                                    <li><strong>Inspiring Quote</strong> We Were Soliders</li>
                                </ul>
                            </div>
                            
                        </section>

                        <section class="widget relationships">
                            <header class="clearfix">
                                <h4 class="pull-left"><i class="icon-exchange"></i> Relationships</h4> 
                                <div class="actions pull-right">
                                    <a class="edit" href=""><i class="icon-pencil"></i></a>
                                    <a class="edit" href=""><i class="icon-plus"></i></a>
                                </div></header>
                            <div class="widget-content">
                                <ul class="personlist">
                                    <li><i class="icon-circle"></i><strong>Susan Edmiston</strong> (Mother)</li>
                                    <li><strong>Ken Edmiston</strong> (Father)</li>
                                    <li><strong>Bret Filipek</strong> (Invitee)</li>
                                    <li><i class="icon-circle"></i><strong>Jen Edmonson</strong> (Inviter)</li>
                                    <li><i class="icon-circle"></i><strong>Diane Bowen</strong> (Invitee)</li>
                                </ul>
                            </div>
                            
                        </section>

                        <section class="widget relationships">
                            <header class="clearfix">
                                <h4 class="pull-left"><i class="icon-pushpin"></i> Peer Network</h4> 
                                <div class="actions pull-right">

                                </div></header>
                            <div class="widget-content">
                                <ul class="personlist">
                                    <li>
                                        <a 
                                            href="http://www.google.com" 
                                            rel="popover" 
                                            class="popover-person" 
                                            data-container="body" 
                                            data-original-title="<img src='../../../Assets/Mockup/alex.jpg' /> <div>Bill Long<small>Member</small></div>" 
                                            data-content="<strong>Spouse</strong> Jasmin
                                                            <br /><strong>Age</strong> 44
                                                            <br /><strong>Area</strong> Westwing
                                                            <br /><strong>Email</strong> bill.long@phoenixpd.gov
                                                            <p /><strong>Home Phone</strong> (623) 555-2426
                                                            <br /><strong>Cell Phone</strong> (623) 532-2252">
                                                <i class="icon-circle"></i>
                                                <strong>Bill Long</strong>
                                        </a>
                                    </li>
                                    <li><a href="http://www.google.com" rel="popover" class="popover-person" data-original-title="Jasmin Long" data-content="<p>9039 W Molly Ln<br>Peoria, AZ 85383</p><p>(623) 555-2426</p>"><strong>Jasmin Long</strong></a></li>
                                    <li><a href="http://www.google.com" rel="popover" class="popover-person" data-original-title="<img src='../../../Assets/Mockup/alex.jpg' /> Quan Fan" data-content="<p>9039 W Molly Ln<br>Peoria, AZ 85383</p><p>(623) 555-2426</p>"><i class="icon-circle"></i><strong>Quan Fan</strong></a></li>
                                    <li><a href="http://www.google.com" rel="popover" class="popover-person" data-original-title="<img src='../../../Assets/Mockup/alex.jpg' /> Shelly Fan" data-content="<p>9039 W Molly Ln<br>Peoria, AZ 85383</p><p>(623) 555-2426</p>"><i class="icon-circle"></i><strong>Shelly Fan</strong></a></li>
                                    <li><a href="http://www.google.com" rel="popover" class="popover-person" data-original-title="Stacey McClure" data-content="<p>9039 W Molly Ln<br>Peoria, AZ 85383</p><p>(623) 555-2426</p>"><strong>Stacey McClure</strong></a></li>
                                    <li><a href="http://www.google.com" rel="popover" class="popover-person" data-original-title="<img src='../../../Assets/Mockup/alex.jpg' /> Coleen McClure" data-content="<p>9039 W Molly Ln<br>Peoria, AZ 85383</p><p>(623) 555-2426</p>"><i class="icon-circle"></i><strong>Coleen McClure</strong></a></li>
                                    <li><a href="http://www.google.com" rel="popover" class="popover-person" data-original-title="<img src='../../../Assets/Mockup/alex.jpg' /> Tonya McDonald" data-content="<p>9039 W Molly Ln<br>Peoria, AZ 85383</p><p>(623) 555-2426</p>"><i class="icon-circle"></i><strong>Tonya McDonald</strong></a></li>
                                    <li><a href="http://www.google.com" rel="popover" class="popover-person" data-original-title="Joe McDonald" data-content="<p>9039 W Molly Ln<br>Peoria, AZ 85383</p><p>(623) 555-2426</p>"><strong>Joe McDonald</strong></a></li>
                                </ul>
                            </div>
                            
                        </section>
                    </div>
                </div>
            </div>
		    
        </div>



        <script>
            $(function () {
                $(".popover-person").popover({ trigger: 'hover', html: 'true', delay: 200 });
                $(".badge").tooltip({html: 'true', delay: 200 });
                
            });
        </script> 

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

        /* persondetails-header */
        .persondetails-header .navbar-inner
        {
            padding-bottom: 24px;
        }

        .persondetails-header .actions
        {
            position: absolute; 
            right: 12px; 
            z-order: 99;
            margin-top: 50px;
        }

        .persondetails-header .labels {
            text-align: right;
            margin-top: 12px;
        }

        .persondetails-header .label
        {
            margin-top: 11px;
        }

        .persondetails .navbar .nav > li > a:hover,
        .persondetails .navbar .nav > li > a:focus
        {
            color: inherit;
        }

        .persondetails .navbar .nav > li > a .caret
        {
            border-top: 4px solid #bbb;
        }

        .persondetails .navbar .nav > li > a:hover .caret,
        .persondetails .navbar .nav > li > a:focus .caret
        {
            border-top: 4px solid #515151;
        }

        .persondetails-header .photo
        {
            padding: 5px 5px 5px 5px;
            background-color: #fff;
            border: 1px solid #bdbdbd;
            display: table;
            width: 100%;
            max-width: 300px;
        }

        .persondetails-header .photo img
        {
            width: 100%;
        }

        .persondetails-header .tags
        {
            margin-bottom: 44px;
        }

        .persondetails-header .phone
        {
            margin-bottom: 12px;
        }

        /* persondetails-badgebar */

        .persondetails-badgebar .navbar-inner
        {
            background-color: #e6e6e6;
            background-image: none;
            box-shadow: none;
            border: 0;
            min-height: 50px;
            border-bottom: 1px solid #c8c8c8;
        }

        .persondetails-badgebar .badge-group
        {
            margin: 10px 0;
        }

        .persondetails-badgebar .badge-group:last-child
        {
            text-align: right;
        }

        @media (max-width: 767px) {
	       .persondetails-badgebar .badge-group:last-child
            {
                text-align: left;
            }
        }

        .persondetails-badgebar .badge
        {
            background-color: inherit;
        }

        /* persondetails-familybar */

        .persondetails-familybar .navbar-inner
        {
            background: #dfdfdf;
            padding: 12px 0;
            border-bottom: 1px solid #c8c8c8;
        }

        .persondetails-familybar .members header
        {
            font-family: 'OpenSansSemibold', 'Helvetica Neue',Helvetica,Arial,sans-serif;
            font-size: 16px;
            margin-bottom: 12px;
        }

        .persondetails-familybar .members ul
        {
            list-style: none;
            margin: 6px 0;
        }

        .persondetails-familybar .members li
        {
            float: left;
            margin-right: 12px;
            margin-bottom: 12px;
            min-width: 150px;
        }

        .persondetails-familybar .members li a
        {
            color: #515151;
        }

        .persondetails-familybar .members li img
        {
            float: left;
            border: 1px solid #fff;
            width: 37px;
            height: 37px;
        }

        .persondetails-familybar .member
        {
            float: left;
            margin-left: 4px;
        }

        .persondetails-familybar .member h4
        {
            margin: 0;
            font-size: 14px;
            font-weight: normal;
            font-family: 'OpenSans', 'Helvetica Neue',Helvetica,Arial,sans-serif;
            float: left;
            max-width: 88px;
            text-overflow: ellipsis;
            overflow: hidden;
        }

        .persondetails-familybar .member small
        {
            display: block;
            line-height: 16px;
            margin: 0;
        }

        .persondetails-familybar .member small.age
        {
            display: inline;
            margin-left: 4px;
        }

        .persondetails-familybar .addresses ul
        {
            list-style: none;
            margin: 0;
        }

        .persondetails-familybar .addresses li
        {
            display: block;
            clear: both;
            margin-bottom: 12px;
        }

        .persondetails-familybar .addresses h4
        {
            margin: 0;
            font-size: 14px;
            font-family: 'OpenSansSemibold', 'Helvetica Neue',Helvetica,Arial,sans-serif;
            font-weight: normal;
        }

        .persondetails-familybar .addresses a.map,
        .persondetails-familybar .addresses div.actions,
        .persondetails-familybar .addresses div.address
        {
            float: left;
            margin-right: 4px;
        }

        .persondetails-familybar .address .actions
        {
            display: none;
        }

        .persondetails-familybar .address .actions a
        {
            padding: 1px;
        }

        .persondetails-familybar .container-fluid > .actions
        {
            position: absolute; 
            right: 12px; 
            z-order: 99;
        }
        

        .bookmarkattributes ul
        {
            list-style-type: none;
            margin-left: 0;
        }


        .pagetabs
        {
            margin-top: 12px;
        }

        .pagetabs .nav
        {
            margin: 10px 0;
        }

        .persontimeline header h4
        {
            float: left;
        }

        .persontimeline header .btn
        {
            float: right;
            margin-top: 6px;
        }

        .persontimeline article
        {
            background-color: red;
            border-radius: 4px; /*@baseBorderRadius*/
            border: 1px solid #d3d3d3;
            padding: 12px;
            margin-bottom: 12px;
            background-color: #f3f3f3; /* variable? */
        }

        .persontimeline article .date
        {
            font-size: 13px;
            font-family: 'OpenSansLight', 'Helvetica Neue',Helvetica,Arial,sans-serif;
            margin-left: 8px;
        }

        .persontimeline article i
        {
            float: left;
            color: #a8a8a8;
            width: 6%;
            font-size: 20px;
        }

        .persontimeline article h5
        {
            margin-top: 0;
            margin-bottom: 4px;
        }

        .persontimeline article .details
        {
            float: left;
            width: 84%;
        }

        .persontimeline article .actions
        {
            float: right;
            width: 10%;
        }

        .persontimeline article .actions i
        {
            font-size: 14px;
            
        }

        .persontimeline article .actions a
        {
            float: right;
            margin-left: 6px;
        }

        .note-container article:last-child
        {
            margin-bottom: 0;
        }

        .persontimeline article.highlight
        {
            background-color: #f2dede;
            border: 1px solid #eed3d7;
            color: #b94a48;

            /*
                @errorText:               #b94a48;
                @errorBackground:         #f2dede;
                @errorBorder:             darken(spin(@errorBackground, -10), 3%);
                */
        }

        .persontimeline article.highlight i
        {
            color: #b94a48;
        }

        .persontimeline article.personal
        {
            background-color: #D9EDF7;
            border: 1px solid #bce8f1;
            color: #3a87ad;

            /*
                @errorText:               #b94a48;
                @errorBackground:         #f2dede;
                @errorBorder:             darken(spin(@errorBackground, -10), 3%);
                */
        }

        .persontimeline article.personal i
        {
            color: #3a87ad;
        }

        .persontimeline .load-more
        {
            margin: 0 40%;
            min-width: 120px;
            width: 20%;
        }

        .persontimeline .load-more span
        {
            margin: 0 12px;
        }

        .note-entry
        {
            background-color: #F3F3F3;
            border: 1px solid #D3D3D3;
            border-radius: 4px 4px 4px 4px;
            margin-bottom: 12px;
            padding: 6px 6px 0 6px;
        }

        .note-entry textarea
        {
            -webkit-box-sizing: border-box;
            -moz-box-sizing: border-box;
            box-sizing: border-box;
            width: 100%;
        }

        .note-entry .settings .options
        {
            float: left;
        }

        .note-entry .settings .options label
        {
            float: left;
            margin-right: 12px;
            margin-left: 4px;
        }

        .note-entry .settings button.security
        {
            float: right;
        }

        .note-entry .actions
        {
            border-top: 1px solid #D3D3D3;
            background-color: #ededed;
            margin: 6px -6px 0 -6px;
            padding: 6px;
            text-align: right;
            padding: 8px 6px;
            border-radius: 0 0 4px 4px;
        }






        /* generic */
        small
        {
            font-family: 'OpenSansLight', 'Helvetica Neue',Helvetica,Arial,sans-serif;
            font-size: 13px;
            margin-top: 2px;
        }

        .popover-title img,
        .popover-title div
        {
            float: left;
        }

        .popover-title div
        {
            margin: 4px 0 0 8px;
        }

        .popover-title small
        {
            display: block;
            font-weight: normal;
        }

        .popover-title:after
        {
            content: ".";
            display: block;
            height: 0;
            clear: both;
            visibility: hidden;
        }

        .widget header .actions 
        {
            margin-top: 8px;
            display: none;
        }

        .widget header .actions a
        {
            padding: 4px;
        }

        .personlist
        {
            list-style: none;
            margin-left: 0;
        }

        .personlist li
        {
            margin-left: 16px;
        }

        .personlist i[class=icon-circle]
        {
            font-size: 9px;
            color: #d0cfcf;
            margin-left: -16px;
            margin-right: 3px;
            vertical-align: middle;
        }

        @media (max-width: 767px) {
	        /* center the person image */
            .persondetails-header .photo
            {
                margin: 0 auto;
            }

            /* make widgets extend to the edges */
            .persondetails .widget
            {
                margin-right: -20px;
                margin-left: -20px;
                border-radius: 0;
            }

            /* make person notes full width */
            .persontimeline .widget-content
            {
                padding-left: 0;
                padding-right: 0;
            }

            /* remove border radius on widget header */
            .persondetails .widget header
            {
                border-radius: 0;
            }

            /* remove border radius on person notes */
            .persontimeline article
            {
                border-radius: 0;
            }


        }

    </style>
        
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

        $('.widget header').live({
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

        $('.persontimeline article').live({
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

