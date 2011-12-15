<%@ Page Title="" ValidateRequest="false" Language="C#" MasterPageFile="~/Themes/RockCMS/Layouts/Dialog.Master"
    AutoEventWireup="true" CodeFile="Dialog.aspx.cs" Inherits="RockWeb.Themes.RockCMS.Layouts.Dialog" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">
    <div id="dialog">
        <div id="content" class="group">
            <asp:PlaceHolder ID="Content" runat="server">
            </asp:PlaceHolder>
        </div>
    </div>
</asp:Content>

