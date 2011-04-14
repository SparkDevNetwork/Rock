<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Blocks.ascx.cs" Inherits="Rock.Web.Blocks.Cms.Blocks" %>

<asp:PlaceHolder ID="phList" runat="server" Visible="false">
	<asp:GridView ID="gvList" runat="server" AutoGenerateColumns="false">
		<Columns>
			<asp:HyperLinkField HeaderText="Block" DataNavigateUrlFormatString="~/Bloc/Edit/{0}" DataNavigateUrlFields="Id" DataTextField="Name" />
			<asp:BoundField HeaderText="Path" DataField="Path" />
			<asp:BoundField HeaderText="Name" DataField="Name" />
			<asp:BoundField HeaderText="Description" DataField="Description" />

		</Columns>
	</asp:GridView> 
	<asp:HyperLink ID="hlAdd" NavigateUrl="~/Bloc/Add" runat="server">Add New Block</asp:HyperLink>
</asp:PlaceHolder>

<asp:UpdatePanel UpdateMode="Always" runat="server">
	<ContentTemplate>
		<asp:PlaceHolder ID="phDetails" runat="server" Visible="false">
			<asp:Label AssociatedControlID="tbPath">Path</asp:Label>
			<asp:TextBox ID="tbPath" runat="server"></asp:TextBox><br />

			<asp:Label AssociatedControlID="tbName">Name</asp:Label>
			<asp:TextBox ID="tbName" runat="server"></asp:TextBox><br />

			<asp:Label AssociatedControlID="tbDescription">Description</asp:Label>
			<asp:TextBox ID="tbDescription" runat="server"></asp:TextBox><br />

			<asp:Label AssociatedControlID="cbSystem">Is this a "system" block?</asp:Label>
			<asp:CheckBox ID="cbSystem" runat="server" /><br />

			<asp:LinkButton ID="lbSave" runat="server" Text="Save" OnClick="lbSave_Click"></asp:LinkButton>&nbsp;
			<asp:HyperLink ID="hlCancel" NavigateUrl="~/Bloc/List" runat="server">Cancel</asp:HyperLink>
		</asp:PlaceHolder>
</ContentTemplate>
</asp:UpdatePanel>
