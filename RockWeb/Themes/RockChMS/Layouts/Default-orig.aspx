<%@ Page Title="" ValidateRequest="false" Language="C#" MasterPageFile="~/Themes/RockChMS/Layouts/Site.Master" 
    AutoEventWireup="true" Inherits="Rock.Web.UI.Page" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">

    <script type="text/javascript">

        Sys.Application.add_load(function () {
            $('a.logo').attr('href', rock.baseUrl);
        });

    </script>


    <div id="page-frame">
        <div class="navbar navbar-fixed-top">
            <div class="navbar-inner">
                <div class="container">
                    <a class='brand'>Home</a>
                </div>
            </div>
        </div>
    </div>



    <div id="page-frame">
        <header class="topbar topbar-inner">
		    <section class="container">
			    <div class="row">
				    <div class="three columns">
                        <a class='logo'>Home</a>

				    </div>
				    <div class="five columns offset-by-four content">

					    <Rock:Zone ID="zHeader" Name="Header" runat="server" />

					    <div class="filter-search">
						    <input id="search-words">
						    <div class="filter">
                                <dl class="dropdown">
                                    <dt>Name</dt>
                                    <dd>
                                        <ul>
                                            <li>Name</li>
                                            <li>Email Address</li>
                                            <li>Address</li>
                                            <li>Phone</li>
                                            <li>Group</li>
                                        </ul>
                                    </dd>
                                </dl>
                                <input type="hidden" name="hSearchFilter" value="Name" id="hSearchFilter" />
                            </div>
					    </div>
				    </div>
                    <script>
                        // show options when clicked
                        $(".dropdown dt").click(function () {
                            $(".dropdown dd ul").fadeToggle( 'fast' );
                        });

                        // change selection when picked
                        $(".dropdown dd ul li").click(function () {
                            var text = $(this).html();
                            $(".dropdown dt").html(text);
                            $(".dropdown dd ul").hide();
                            $("#hSearchFilter").val( text );
                        }); 
                    </script>
				
				
			    </div>
			    <div class="row">
				    <nav class="twelve columns">
					    <Rock:Zone ID="Menu" runat="server" />

					    <a href="" id="header-lock">Lock</a>
				    </nav>
			    </div>
		    </section>
	    </header>            
    
        <div id="content">        
            <section id="page-title">
		        <div class="row">	
                    <div class="four columns">
			            <h1><Rock:PageTitle runat="server" /></h1>
                    </div>
                    <div class="six columns">
                        <Rock:Zone ID="PageTitleBar" runat="server" />
                    </div>
		        </div>
	        </section>

	        <section id="core-content" class="row container">     
  
                <Rock:Zone ID="ContentLeft" runat="server" />                       
                <Rock:Zone ID="ContentRight" runat="server" />
                <Rock:Zone ID="Content" runat="server" />
                <Rock:Zone ID="UpperBand" runat="server" />
                <Rock:Zone ID="LowerBand" runat="server" />
                <Rock:Zone ID="LowerContentLeft" runat="server" />
                <Rock:Zone ID="LowerContentRight" runat="server" />
                <Rock:Zone ID="LowerContent" runat="server" />
                        
            </section>
	    </div>

	    <footer>
		    <div class="row">
		        <Rock:Zone ID="Footer" runat="server" />
		    </div>
	    </footer>   
    </div>
    <script>
        /* script to manage header lock */
        $(document).ready(function () {
            var headerIsLocked = localStorage.getItem("rock-header-lock");

            if (headerIsLocked == "false") {
                $('#content, #header-lock, header.topbar').toggleClass('unlock');                
            }
        });

        $('#header-lock').click(function () {
            $('#content, #header-lock, header.topbar').toggleClass('unlock');

            if ($('#header-lock').hasClass('unlock')) {
                localStorage.setItem('rock-header-lock', 'false');
            }
            else {
                localStorage.setItem('rock-header-lock', 'true');
            }

            e.preventDefault();
        });
	</script>
            
</asp:Content>

