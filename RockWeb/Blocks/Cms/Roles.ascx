<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Roles.ascx.cs" Inherits="Rock.Web.Blocks.Cms.Roles" %>

<asp:PlaceHolder ID="phList" runat="server" Visible="false">
    <asp:PlaceHolder ID="phRoles" runat="server"></asp:PlaceHolder>
    <a href="/Role/Add">Add New Role</a>  
</asp:PlaceHolder>

<asp:PlaceHolder ID="phDetails" runat="server" Visible="false">
    <asp:Label AssociatedControlID="tbName">Role Name</asp:Label>
    <asp:TextBox ID="tbName" runat="server"></asp:TextBox><br />
    <asp:LinkButton ID="lbSave" runat="server" Text="Save" OnClick="lbSave_Click"></asp:LinkButton>
    <a href="/Role/List">Cancel</a>  
</asp:PlaceHolder>
