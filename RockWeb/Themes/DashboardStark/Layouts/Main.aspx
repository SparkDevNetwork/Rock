<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">
    		
<header>
    <div class="container">
        <div class="pull-left">
            <Rock:Zone Name="HeaderLeft" runat="server" />
        </div>	
	    <div class="pull-right">
            <Rock:Zone Name="HeaderRight" runat="server" />
	    </div>
    </div>
</header>
   
<main>
    <div class="container">
        <div class="row">
            <Rock:Zone Name="Section A" runat="server" />
        </div>

        <div class="row">
            <Rock:Zone Name="Section B" runat="server" />
        </div>

        <div class="row">
            <Rock:Zone Name="Section C" runat="server" />
        </div>

        <div class="row">
            <Rock:Zone Name="Section D" runat="server" />
        </div>

        <div class="row">
            <Rock:Zone Name="Section E" runat="server" />
        </div>

        <div class="row">
            <Rock:Zone Name="Section F" runat="server" />
        </div>
    </div>
</main>
    
    
<footer>
    <div class="container">		
	    <div class="pull-left">
            <Rock:Zone Name="FooterLeft" runat="server" />
        </div>
        <div class="pull-right">
            <Rock:Zone Name="FooterRight" runat="server" />
        </div>
    </div>
</footer>		
</asp:Content>

