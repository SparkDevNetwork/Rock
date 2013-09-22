<%@ Page ValidateRequest="false" Language="C#" MasterPageFile="Site.Master" 
    AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">
    		
<div class="container body-content">		
	<Rock:Zone ID="Content" runat="server" />
</div>
		
</asp:Content>

