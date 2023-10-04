Lakepointe Rock Theme, www.lakepointe.org

### Top Zone

* HTML Content Block, Header Left

  * Context Name: LPCHeaderLeft

  * Advanced settings:

    * CSS Class: hidden-xs
    * Pre-HTML `<div class="pull-left headerbar-left">`
    * Post-HTML `</div>`

  * HTML Content:

    ```html
    <a class="hidden-xs hidden-sm" href="/"><span id="et-info-home">Home</span></a>
    <a href="/contact"><span id="et-info-email">Contact</span></a>
    <a href="https://my.lakepointe.org"><span id="et-info-account">My Account</span></a>
    <a href="https://www.xn--lpespaol-i3a.org"><span id="et-info-directions">Español</span></a>
    ```

    ​

* HTML Content Block, Header Right

  - Context Name: LPCHeaderRight

  - Advanced settings:

    - Pre-HTML `<div class="pull-right headerbar-right">`
    - Post-HTML `</div>`

  - HTML Content:

    ```html
    <div class="lp-countdown">
    	<a href="https://www.lakepointe.org/live">
    		<div id="2446-countdown" class="tminus_countdown" style="width:auto; height:auto;"><div class="lakepointe-countdown omitweeks"><div id="2446-tophtml" class="lakepointe-tophtml">LIVE SERVICE IN: </div><div id="2446-dashboard" class="lakepointe-dashboard"><div class="lakepointe-timer_icon"></div><div class="lakepointe-dash lakepointe-days_dash" data-days_cid="2446" data-value=""><div class="lakepointe-dash_title">Days</div><div class="lakepointe-digit" data-digit_cid="2446">0</div><div class="lakepointe-digit" data-digit_cid="2446">3</div></div><div class="lakepointe-dash lakepointe-hours_dash" data-hours_cid="2446" data-value=""><div class="lakepointe-dash_title">Hours</div><div class="lakepointe-digit" data-digit_cid="2446">0</div><div class="lakepointe-digit" data-digit_cid="2446">2</div></div><div class="lakepointe-dash lakepointe-minutes_dash" data-minutes_cid="2446" data-value=""><div class="lakepointe-dash_title">Minutes</div><div class="lakepointe-digit" data-digit_cid="2446">4</div><div class="lakepointe-digit" data-digit_cid="2446">7</div></div><div class="lakepointe-dash lakepointe-seconds_dash" data-seconds_cid="2446" data-value=""><div class="lakepointe-dash_title">Seconds</div><div class="lakepointe-digit" data-digit_cid="2446">2</div><div class="lakepointe-digit" data-digit_cid="2446">5</div></div><div class="lakepointe-trailing_text"></div></div><div id="2446-bothtml" class="lakepointe-bothtml"> </div></div></div><div id="2446-countdown-swap" style="display:none"></div>				</a>
    </div> 
    <script language="javascript" type="text/javascript">
    				jQuery(document).ready(function($) {
    					$('#2446-dashboard').tminusCountDown({
    				targetDate: {
    					'day': 	26,
    					'month': 08,
    					'year': 2017,
    					'hour': 18,
    					'min': 	00,
    					'sec': 	00,
    					'launchtime': '8/26/2017 18:00:00',
    					'tminus_type':	'recurring',
    					'use_cookie': '',
    					'expiry_days': '',
    					'localtime': '{{ 'Now' | Date:'M/d/yyyy H:mm:ss' }}'
    				},
    				id: '2446',
    				style: 'lakepointe',
    				omitSecs: '',
    				omitMins: '',
    				omitHours: '',
    				omitDays: '',
    				omitWeeks: 'true'
    				, launchTarget: '2446-countdown-swap', onComplete: function() {
    								$('#2446-countdown-swap').css({'width' : 'auto', 'height' : 'auto'});
    								$('#2446-countdown-swap').html('<div class=\"countdown-is-live\">Live service is now in progress!<br /> Watch Now!</div>');
    							}, pid: 2446, cid: 2297, hangtime:  3300});

    		});
    	</script>
    ```

    ​

### Header Zone

* HTML Content Block, Header

  * Context Name: LPC Header

  * HTML Content

    ```html
    <span class="logo_helper"></span><a href="//my.lakepointe.org"><img src="~~/Assets/images/logoLTBLue.png" class="hdr-logo img-responsive" /></a>
    ```

### Top Navigation Zone

* HTML Content Block, LPC Main Nav

  * Context Name: LPCMainNav

  * HTML Content:

    ```html
    <ul id="top-menu" class="nav navbar-nav">
        <li id="menu-item-4208" class="dropdown mega-dropdown"><a href="#" data-toggle="dropdown" class="dropdown-toggle">LOCATIONS</a>
            <ul class="dropdown-menu mega-dropdown-menu">
                <li id="menu-item-4291" class="dropdown-submenu col-sm-3"><a href="#" data-toggle="dropdown" class="dropdown-toggle">CAMPUS INFO</a>
                    <div class="dropdown-menu">
                        <ul>
                            <li id="menu-item-4209"><a href="https://www.lakepointe.org/rockwall/">Rockwall Campus</a></li>
                            <li id="menu-item-4210"><a href="https://www.lakepointe.org/towneast/">Town East Campus</a></li>
                            <li id="menu-item-4211"><a href="https://www.lakepointe.org/firewheel/">Firewheel Campus</a></li>
                            <li id="menu-item-4212"><a href="https://www.lakepointe.org/forney/">Forney Campus</a></li>
                            <li id="menu-item-4213"><a href="https://www.lakepointe.org/richland/">Richland Campus</a></li>
                            <li id="menu-item-25613"><a href="https://www.lakepointe.org/whiterock/">White Rock Campus</a></li>
                            <li id="menu-item-4214"><a href="https://www.lakepointe.org/lpe/">LP Español</a></li>
                        </ul>
                    </div>
                </li>
                <li id="menu-item-4301" class="dropdown-submenu col-sm-3"><a href="#" data-toggle="dropdown" class="dropdown-toggle">I&#8217;M NEW</a>
                    <div class="dropdown-menu">
                        <ul>
                            <li id="menu-item-4413"><a href="https://www.lakepointe.org/what-to-expect/">What to Expect</a></li>
                            <li id="menu-item-4536"><a href="https://www.lakepointe.org/what-we-believe/">What We Believe</a></li>
                            <li id="menu-item-4219"><a href="https://www.lakepointe.org/contact/meet-our-leaders/">Meet Our Leaders</a></li>
                            <li id="menu-item-4539"><a href="https://www.lakepointe.org/what-we-value/">What We Value</a></li>
                            <li id="menu-item-4540"><a href="https://www.lakepointe.org/nextsteps/">Take Your Next Step</a></li>
                        </ul>
                    </div>
                </li>
                <li id="menu-item-4293" class="dropdown-submenu col-sm-3"><a href="#" data-toggle="dropdown" class="dropdown-toggle">CHURCH ONLINE</a>
                    <div class="dropdown-menu">
                        <ul>
                            <li id="menu-item-26216"><a href="https://www.lakepointelive.tv">Internet Campus</a></li>
                            <li id="menu-item-4298"><a target="_blank" href="http://fournineteen.com/shorelive">Shore Live : Grades 6-8</a></li>
                            <li id="menu-item-4299"><a target="_blank" href="http://fournineteen.com/uprisinglive">Uprising Live : Grades 9-12</a></li>
                        </ul>
                    </div>
                </li>
                <li id="menu-item-4295" class="dropdown-submenu col-sm-3"><a href="#" data-toggle="dropdown" class="dropdown-toggle">HAPPENING NOW</a>
                    <div class="dropdown-menu">
                        <ul>
                            <li id="menu-item-6836"><a href="https://www.lakepointe.org/events/">Upcoming Events</a></li>
                            <li id="menu-item-4296"><a target="_blank" href="http://lpstories.org">Stories of Life Change</a></li>
                            <li id="menu-item-16943"><a href="https://www.lakepointe.org/stories/">Lake Pointe Blog</a></li>
                        </ul>
                    </div>
                </li>
            </ul>
        </li>
        <li id="menu-item-4277" class="dropdown mega-dropdown"><a href="#" data-toggle="dropdown" class="dropdown-toggle">CONNECT</a>
            <ul class="dropdown-menu mega-dropdown-menu">
                <li id="menu-item-5138" class="dropdown-submenu col-sm-3"><a href="#" data-toggle="dropdown" class="dropdown-toggle">LIFE GROUPS</a>
                    <div class="dropdown-menu">
                        <ul>
                            <li id="menu-item-5137"><a href="https://www.lakepointe.org/groups/">About Life Groups</a></li>
                            <li id="menu-item-4226"><a href="http://my.lakepointe.org/default.aspx?page=5672">Find a Life Group</a></li>
                            <li id="menu-item-5190"><a href="https://www.lakepointe.org/groups/currentstudy/">Current Study</a></li>
                            <li id="menu-item-4419"><a href="https://www.lakepointe.org/groups/teacher-training/">Leader Resources</a></li>
                        </ul>
                    </div>
                </li>
                <li id="menu-item-4231" class="dropdown-submenu col-sm-3"><a href="#" data-toggle="dropdown" class="dropdown-toggle">MARRIED &amp; SINGLE</a>
                    <div class="dropdown-menu">
                        <ul>
                            <li id="menu-item-4438"><a href="https://www.lakepointe.org/homepointe/">Marriage &amp; Family</a></li>
                            <li id="menu-item-4279"><a target="_blank" href="https://www.lakepointe.org/mens/">Men</a></li>
                            <li id="menu-item-4280"><a target="_blank" href="http://lpwomen.org">Women</a></li>
                            <li id="menu-item-28747"><a href="https://www.lakepointe.org/singles/">Singles</a></li>
                            <li id="menu-item-28758"><a href="https://www.lakepointe.org/ourgeneration/">Our Generation</a></li>
                        </ul>
                    </div>
                </li>
                <li id="menu-item-4502" class="dropdown-submenu col-sm-3"><a href="#" data-toggle="dropdown" class="dropdown-toggle">STUDENTS &amp; SPORTS</a>
                    <div class="dropdown-menu">
                        <ul>
                            <li id="menu-item-4247"><a target="_blank" href="http://lpkids.com">Children</a></li>
                            <li id="menu-item-4248"><a target="_blank" href="http://fournineteen.com">Students</a></li>
                            <li id="menu-item-28741"><a href="https://www.lakepointe.org/college/">College</a></li>
                            <li id="menu-item-4252"><a target="_blank" href="http://lpsports.com">Sports</a></li>
                        </ul>
                    </div>
                </li>
                <li id="menu-item-4801" class="dropdown-submenu col-sm-3"><a href="#" data-toggle="dropdown" class="dropdown-toggle">MY.LAKEPOINTE.ORG</a>
                    <div class="dropdown-menu">
                        <ul>
                            <li id="menu-item-4811"><a href="https://my.lakepointe.org/default.aspx?page=5259">Account Log In</a></li>
                            <li id="menu-item-4802"><a href="https://my.lakepointe.org/default.aspx?page=5086">Edit My Profile</a></li>
                            <li id="menu-item-4803"><a href="https://my.lakepointe.org/default.aspx?page=5627">View Giving Contributions</a></li>
                            <li id="menu-item-4804"><a href="https://my.lakepointe.org/default.aspx?page=5264">Manage Giving Reminders</a></li>
                        </ul>
                    </div>
                </li>
            </ul>
        </li>
        <li id="menu-item-4673" class="dropdown"><a href="#" data-toggle="dropdown" class="dropdown-toggle">GROW</a>
            <ul class="dropdown-menu">
                <li id="menu-item-4674"><a href="https://www.lakepointe.org/nextsteps/">Take Your Next Step</a></li>
                <li id="menu-item-6451"><a href="https://www.lakepointe.org/homepointe/">HomePointe</a></li>
                <li id="menu-item-4259"><a href="https://www.lakepointe.org/equip/">Equipping Center</a></li>
                <li id="menu-item-4258"><a href="https://www.lakepointe.org/lpu/">Lake Pointe University</a></li>
                <li id="menu-item-4676"><a href="https://www.lakepointe.org/bookstore/">Bookstore</a></li>
            </ul>
        </li>
        <li id="menu-item-29427" class="dropdown"><a href="#" data-toggle="dropdown" class="dropdown-toggle">SERVE</a>
            <ul class="dropdown-menu">
                <li id="menu-item-5110"><a href="https://www.lakepointe.org/volunteer/">Volunteer On Campus</a></li>
                <li id="menu-item-4342"><a href="https://www.lakepointe.org/missions/">Local Missions</a></li>
                <li id="menu-item-29431"><a href="https://www.lakepointe.org/missions/">North American Missions</a></li>
                <li id="menu-item-29437"><a href="https://www.lakepointe.org/missions/">International Missions</a></li>
            </ul>
        </li>
        <li id="menu-item-18765" class="dropdown mega-dropdown"><a href="#" data-toggle="dropdown" class="dropdown-toggle">CARE</a>
            <ul class="dropdown-menu mega-dropdown-menu">
                <li id="menu-item-18766" class="dropdown-submenu col-sm-3"><a href="#" data-toggle="dropdown" class="dropdown-toggle">COUNSELING</a>
                    <div class="dropdown-menu">
                        <ul>
                            <li id="menu-item-24413"><a href="https://www.lakepointe.org/counseling/">Professional Counseling</a></li>
                            <li id="menu-item-31218"><a href="https://www.lakepointe.org/counseling/marriagecounseling/">Marriage Counseling</a></li>
                            <li id="menu-item-4266"><a href="https://www.lakepointe.org/stephen-ministry/">Stephen Ministry</a></li>
                        </ul>
                    </div>
                </li>
                <li id="menu-item-18774" class="dropdown-submenu col-sm-3"><a href="#" data-toggle="dropdown" class="dropdown-toggle">GROUPS</a>
                    <div class="dropdown-menu">
                        <ul>
                            <li id="menu-item-4263"><a href="https://www.lakepointe.org/counseling/groups/">Groups</a></li>
                            <li id="menu-item-5136"><a href="https://www.lakepointe.org/reengageform/">RE | ENGAGE</a></li>
                        </ul>
                    </div>
                </li>
                <li id="menu-item-18767" class="dropdown-submenu col-sm-3"><a href="#" data-toggle="dropdown" class="dropdown-toggle">RESOURCES</a>
                    <div class="dropdown-menu">
                        <ul>
                            <li id="menu-item-22601"><a href="https://www.lakepointe.org/counseling/topicresources/">Topic Articles</a></li>
                            <li id="menu-item-22600"><a href="https://www.lakepointe.org/counseling/bookresources/">Recommended Books</a></li>
                        </ul>
                    </div>
                </li>
                <li id="menu-item-18775" class="dropdown-submenu col-sm-3"><a href="#" data-toggle="dropdown" class="dropdown-toggle">COMPASSION</a>
                    <div class="dropdown-menu">
                        <ul>
                            <li id="menu-item-4553"><a href="https://www.lakepointe.org/prayer/">Prayer Ministry</a></li>
                            <li id="menu-item-18768"><a href="https://www.lakepointe.org/foodpantry/">Food Pantry</a></li>
                            <li id="menu-item-4275"><a href="https://www.lakepointe.org/benevolence/">Benevolence</a></li>
                            <li id="menu-item-6387"><a href="https://www.lakepointe.org/crisiscare/">Crisis Care</a></li>
                            <li id="menu-item-4556"><a href="https://www.lakepointe.org/job-search/">Job Search</a></li>
                        </ul>
                    </div>
                </li>
            </ul>
        </li>
        <li id="menu-item-4503" class="dropdown"><a href="#" data-toggle="dropdown" class="dropdown-toggle">MEDIA</a>
            <ul class="dropdown-menu">
                <li id="menu-item-4341"><a href="https://www.lakepointe.org/message/">Messages</a></li>
                <li id="menu-item-26217"><a href="https://www.lakepointelive.tv">Internet Campus</a></li>
                <li id="menu-item-4504"><a href="https://www.lakepointe.org/equip/">Equipping Center</a></li>
                <li id="menu-item-4505"><a href="https://www.lakepointe.org/lpu/">Lake Pointe University</a></li>
                <li id="menu-item-4507"><a href="https://www.lakepointe.org/social-media/">Social Media</a></li>
                <li id="menu-item-4506"><a href="https://www.lakepointe.org/bookstore/">Bookstore</a></li>
                <li id="menu-item-7079"><a href="https://www.lakepointe.org/library/">Library</a></li>
                <li id="menu-item-21323"><a href="https://www.lakepointe.org/stories/">Stories</a></li>
            </ul>
        </li>
        <li id="menu-item-4276"><a href="https://my.lakepointe.org/default.aspx?page=5601">GIVE</a></li>
        <li class="hidden-xs"><a href="/search/" onclick="$('header nav .navbar-nav').slideUp(function () { $('header nav .navbar-form').slideDown(function () { $('#input-hdr-search').focus();}); }); return false;"><i class="icon_search"></i></a></li>
    </ul>
    <div class="navbar-form" style="display: none;">
        <a href="/search/" onclick="inputHdrSearch(); return false;" class="pull-right visible-xs"><i class="icon_search"></i></a>
        <input type="text" name="query" placeholder="Search..." class="form-control" id="input-hdr-search">
        <a href="#" onclick="$('header nav .navbar-form').slideUp(function () { $('header nav .navbar-nav').slideDown(); }); return false;" class="pull-right close hidden-xs"><i class="icon_close"></i></a>
    </div>
    ```

### Navigation

* Page Menu Block, Navigation
  * Root Page, external home page
  * Number of Levels: 3

### Footer Zone

* HTML Content Block, Footer

  * Context Name: LPCFooter

  * HTML Content:

    ```html
    <div id="footer-widgets" class="clearfix">
    	<div class="footer-widget"><div id="text-5" class="fwidget et_pb_widget widget_text"><h4 class="title">Locations</h4>			<div class="textwidget"><a href="https://www.lakepointe.org/rockwall/">Rockwall Campus</a><br>
    <a href="https://www.lakepointe.org/towneast/">Town East Campus</a><br>
    <a href="https://www.lakepointe.org/firewheel/">Firewheel Campus</a><br>
    <a href="https://www.lakepointe.org/forney/">Forney Campus</a><br>
    <a href="https://www.lakepointe.org/richland/">Richland Campus</a><br>
    <a href="https://www.lakepointe.org/whiterock/">White Rock Campus</a><br>

    <a href="https://www.lakepointe.org/lpe/">LP en Español</a><br>
    <a href="https://www.lakepointe.org/icampus/">Internet Campus</a><br>
    <a href="https://www.lakepointe.org/events/">Upcoming Events</a></div>
    		</div> <!-- end .fwidget --></div> <!-- end .footer-widget --><div class="footer-widget"><div id="text-6" class="fwidget et_pb_widget widget_text"><h4 class="title">Ministries</h4>			<div class="textwidget"><a href="https://www.lakepointe.org/groups/">Life Groups</a><br>
    <a href="https://www.lakepointe.org/homepointe/">HomePointe</a><br>
    <a href="https://www.lakepointe.org/missions/">Missions &amp; Outreach</a><br>
    <a href="https://www.lakepointe.org/reengage/">ReEngage</a><br>
    <a href="https://www.lpkids.com" target="_blank">LP Kids | Birth - Grade 5</a><br>
    <a href="https://www.fournineteen.com" target="_blank">4:19 | Middle &amp; High School</a><br>
    <a href="http://themansite.org" target="_blank">The Man Site | Men's Ministry</a><br>
    <a href="http://lpwomen.org" target="_blank">Women's Ministry</a><br>
    <a href="http://lpsingles.org" target="_blank">Singles Ministry</a><br>
    <a href="http://lpsports.com" target="_blank">Sports Ministry</a><br>
    <a href="https://www.lpkids.com/rockwall/soar/" target="_blank">SOAR Special Needs</a></div>
    		</div> <!-- end .fwidget --></div> <!-- end .footer-widget --><div class="footer-widget"><div id="text-4" class="fwidget et_pb_widget widget_text"><h4 class="title">Resources</h4>			<div class="textwidget"><a href="https://www.lakepointe.org/message/">Messages</a><br>
    <a href="https://www.lakepointe.org/lpu/">Lake Pointe University</a><br>
    <a href="https://www.lakepointe.org/counseling/">Counseling &amp; Support</a><br>
    <a href="https://www.lakepointe.org/homepointe/">Marriage &amp; Families</a><br>
    <a href="https://www.lakepointe.org/weddings/">Weddings at Lake Pointe</a><br>
    <a href="https://www.lakepointe.org/adoption/">Adoption Resources</a><br>
    <a href="https://www.lakepointe.org/equip/">Equipping Center</a><br>
    <a href="https://www.lakepointe.org/hr/">Careers at LP</a><br>
    <a href="https://www.lakepointe.org/store/">Lake Pointe Store</a><br>
    <a href="https://www.lakepointe.org/job-search/">Job Connection</a></div>
    		</div> <!-- end .fwidget --></div> <!-- end .footer-widget -->
    		<div class="footer-widget last"><div id="text-8" class="fwidget et_pb_widget widget_text">
    		    <div class="textwidget padding-b-xl">
    		        <a style="margin-top: 10px;" class="btn btn-primary" href="https://www.lakepointe.org/nextsteps/">Next Steps</a><br><br><a href="https://www.lakepointe.org/nextsteps/">Take your next step on your journey of faith.</a></div>
    		</div> <!-- end .fwidget --><div id="lsi_widget-2" class="fwidget et_pb_widget widget_lsi_widget"><h4 class="title">Connect With Us</h4><ul class="lsi-social-icons icon-set-lsi_widget-2" style="text-align: left"><li class="lsi-social-facebook"><a class="" rel="nofollow" aria-label="Facebook" href="https://www.facebook.com/lpconnect" target="_blank"><i class="lsicon lsicon-facebook"></i></a></li><li class="lsi-social-twitter"><a class="" rel="nofollow" aria-label="Twitter" href="https://twitter.com/LPConnect" target="_blank"><i class="lsicon lsicon-twitter"></i></a></li><li class="lsi-social-instagram"><a class="" rel="nofollow" aria-label="Instagram" href="http://instagram.com/lpconnect" target="_blank"><i class="lsicon lsicon-instagram"></i></a></li><li class="lsi-social-email"><a class="" rel="nofollow" aria-label="Contact" href="http://www.lakepointe.org/contact"><i class="lsicon lsicon-email"></i></a></li></ul></div> <!-- end .fwidget --></div> <!-- end .footer-widget -->	</div>
    ```

    ​

### Footer Bar Zone

* HTML Content Block, Footer Bar

  * Context Name: LPCFooterBar

  * HTML Content

    ```html
    <span>Copyright &copy; 2017 Lake Pointe Church. <span class="hidden-xs hidden-sm hidden-md">All rights reserved.</span> <br class="hidden-lg hidden-md" /> <a href="/privacy-notice" target="_blank"> Privacy Notice</a> | <a href="/conditions-of-use">Terms &amp; Conditions</a><br class="hidden-lg" />
	    <span class="footer-pull-right"><span id="et-info-home">701 E. I-30 | Rockwall, TX 75087</span><br class="hidden-lg hidden-md"><span id="et-info-phone"><a href="tel:+14696982200">469-698-2200</a></span></span>
    </span>
    ```

    ​