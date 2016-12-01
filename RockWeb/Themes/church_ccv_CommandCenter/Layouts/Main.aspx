<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">
    		
<header>
    <div class="container">
        <div class="row">
            <div class="col-md-4">
                <Rock:Zone Name="HeaderLeft" runat="server" />
            </div>
            <div class="col-md-4">
                <Rock:Zone Name="HeaderMiddle" runat="server" />
            </div>
            <div class="col-md-4">
                <Rock:Zone Name="HeaderRight" runat="server" />
            </div>
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
    
</asp:Content>

