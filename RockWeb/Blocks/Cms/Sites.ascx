<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Sites.ascx.cs" Inherits="RockWeb.Blocks.Cms.Sites" %>

<asp:PlaceHolder ID="phList" runat="server" Visible="false">
    <asp:GridView ID="gvList" runat="server" AutoGenerateColumns="false">
        <Columns>
            <asp:HyperLinkField HeaderText="Site" DataNavigateUrlFormatString="~/Site/Edit/{0}" DataNavigateUrlFields="Id" DataTextField="Name" />
            <asp:BoundField HeaderText="Description" DataField="Description" />
            <asp:BoundField HeaderText="Theme" DataField="Theme" />
            <asp:BoundField HeaderText="Default Page" DataField="DefaultPage" />
        </Columns>
    </asp:GridView> 
	<asp:HyperLink NavigateUrl="~/Site/Add" runat="server">Add New Site</asp:HyperLink>
</asp:PlaceHolder>

<asp:PlaceHolder ID="phDetails" runat="server" Visible="false">
    <asp:Label AssociatedControlID="tbName">Site Name</asp:Label>
    <asp:TextBox ID="tbName" runat="server"></asp:TextBox><br />
    <asp:Label AssociatedControlID="tbDescription">Description</asp:Label>
    <asp:TextBox ID="tbDescription" runat="server"></asp:TextBox><br />
    <asp:Label AssociatedControlID="tbTheme">Theme</asp:Label>
	<asp:DropDownList ID="ddlTheme" runat="server"></asp:DropDownList><br />
    <asp:Label AssociatedControlID="tbDefaultPage">Default Page</asp:Label>
    <asp:DropDownList ID="ddlDefaultPage" runat="server" DataTextField="Name" DataValueField="Id"></asp:DropDownList><br />
    <asp:LinkButton ID="lbSave" runat="server" Text="Save" OnClick="lbSave_Click"></asp:LinkButton>&nbsp;
	<asp:HyperLink NavigateUrl="~/Site/List" runat="server">Cancel</asp:HyperLink>
</asp:PlaceHolder>
