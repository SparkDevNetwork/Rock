<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">
    		
<div class="container body-content">
	<Rock:Zone Name="Main" runat="server" />
    
    <div class="row row--fullscreen">
        <div class="col-sm-12 align--middle text-center">
            <h2 class="push--bottom">Sorry About That, It Looks Like Iggy Misplaced That Link.</h2>
            <a onclick="history.go(-1);" class="btn btn-lg btn-primary"><span class="fa fa-arrow-left"></span> Go Back One Page</a>
        </div>

    </div>
    
</div>
		
</asp:Content>