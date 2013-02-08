<%@ Page Title="" ValidateRequest="false" Language="C#" MasterPageFile="~/Themes/CheckinAdventureKids/Layouts/Site.Master" 
    AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">
    		
		<div class="container-fluid body-content">
			<div class="row-fluid">
				<div id="checkin-content" class="span12  ">
					<Rock:Zone ID="Content" runat="server" />
				</div>
			</div>
		</div>
		
</asp:Content>

