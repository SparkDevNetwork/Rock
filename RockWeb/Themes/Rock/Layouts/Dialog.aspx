<%@ Page Title="" ValidateRequest="false" Language="C#" MasterPageFile="~/Themes/Rock/Layouts/Site.Master"
    AutoEventWireup="true" CodeFile="Dialog.aspx.cs" Inherits="Rock.Themes.Rock.Layouts.Dialog" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">
    <div id="dialog">
        <div id="content" class="group">
            <asp:PlaceHolder ID="Content" runat="server">
            </asp:PlaceHolder>
        </div>
    </div>
</asp:Content>

