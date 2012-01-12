<%@ Page Title="" ValidateRequest="false" Language="C#" MasterPageFile="~/Themes/RockCMS/Layouts/Site.Master"
    AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="RockWeb.Themes.RockCMS.Layouts.Default" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">
    <div id="page-frame">
        <header class="topbar topbar-inner">
		    <section class="container">
			    <div class="row">
				    <div class="three columns">
                        <asp:Literal ID="lLogo" runat="server"></asp:Literal>

				    </div>
				    <div class="five columns offset-by-four content">

					    <asp:PlaceHolder ID="Header" runat="server"></asp:PlaceHolder>

					    <div class="filter-search">
						    <input id="search-words">
						    <div class="filter">People</div>
					    </div>

				    </div>
				
				
			    </div>
			    <div class="row">
				    <nav class="twelve columns">
					    <asp:PlaceHolder ID="Menu" runat="server"></asp:PlaceHolder>

					    <a href="" id="header-lock">Lock</a>
				    </nav>
			    </div>
		    </section>
	    </header>            
    
        <div id="content">        
            <section id="page-title">
		        <div class="row">	
                    <div class="four columns">
			            <h1><asp:Literal ID="lPageTitle" runat="server"></asp:Literal></h1>
                    </div>
                    <div class="six columns">
                        <asp:PlaceHolder ID="PageTitleBar" runat="server"></asp:PlaceHolder>
                    </div>
		        </div>
	        </section>

	        <section class="row container">     
  
                <asp:PlaceHolder ID="ContentLeft" runat="server"></asp:PlaceHolder>                        
                <asp:PlaceHolder ID="ContentRight" runat="server"></asp:PlaceHolder>                        
                <asp:PlaceHolder ID="Content" runat="server"></asp:PlaceHolder>
                <asp:PlaceHolder ID="UpperBand" runat="server"></asp:PlaceHolder>
                <asp:PlaceHolder ID="LowerBand" runat="server"></asp:PlaceHolder>
                <asp:PlaceHolder ID="LowerContentLeft" runat="server"></asp:PlaceHolder>
                <asp:PlaceHolder ID="LowerContentRight" runat="server"></asp:PlaceHolder>
                <asp:PlaceHolder ID="LowerContent" runat="server"></asp:PlaceHolder>           
            </section>
	    </div>

	    <footer>
		    <div class="row">
		        <asp:PlaceHolder ID="Footer" runat="server"></asp:PlaceHolder>
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

