<%@ Page Title="" ValidateRequest="false" Language="C#" MasterPageFile="~/Themes/Rock/Layouts/Site.Master"
    AutoEventWireup="true" CodeFile="Splash.aspx.cs" Inherits="Rock.Themes.Rock.Layouts.Splash" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">
            <div id="splash">
                <h1>Rock ChMS</h1>
                
                <div id="content" class="group">
                    <asp:PlaceHolder ID="Content" runat="server">
                        
                    </asp:PlaceHolder>
                </div>
            </div>
</asp:Content>

