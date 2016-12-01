<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">
    		
<div class="container body-content">		
	
    <div class="row">
        <div class="col-md-12">
            <Rock:Zone Name="Feature" runat="server" />
        </div>
    </div>

    <div class="row">
        <div class="col-md-8">
            <Rock:Zone Name="Main" runat="server" />
        </div>

        <div class="col-md-4">
            <Rock:Zone Name="Sidebar 1" runat="server" />
        </div>
    </div>
    
</div>
		
</asp:Content>

