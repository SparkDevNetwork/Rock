<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Blocks.ascx.cs" Inherits="RockWeb.Blocks.Cms.Blocks" %>

<asp:PlaceHolder ID="phList" runat="server" Visible="false">

	<asp:GridView ID="gvList" runat="server" AutoGenerateColumns="false">
		<Columns>
			<asp:HyperLinkField HeaderText="Block" DataNavigateUrlFormatString="~/Bloc/Edit/{0}" DataNavigateUrlFields="Id" DataTextField="Name" />
			<asp:BoundField HeaderText="Path" DataField="Path" />
			<asp:BoundField HeaderText="Name" DataField="Name" />
			<asp:BoundField HeaderText="Description" DataField="Description" />
		</Columns>
	</asp:GridView> 

	<asp:HyperLink ID="hlAdd" runat="server" Text="Add New Block" NavigateUrl="~/Bloc/Add"></asp:HyperLink>

</asp:PlaceHolder>

<asp:UpdatePanel UpdateMode="Always" runat="server">
	<ContentTemplate>

		<asp:PlaceHolder ID="phDetails" runat="server" Visible="false">

			<asp:Label ID="lblPath" runat="server" Text="Path" AssociatedControlID="tbPath"/>
			<asp:TextBox ID="tbPath" runat="server"></asp:TextBox><br />

			<asp:Label ID="lblName" runat="server" Text="Name" AssociatedControlID="tbName" />
			<asp:TextBox ID="tbName" runat="server"></asp:TextBox><br />

			<asp:Label ID="lblDescription" runat="server" Text="Description" AssociatedControlID="tbDescription" />
			<asp:TextBox ID="tbDescription" runat="server"></asp:TextBox><br />

			<asp:Label ID="lblSystem" runat="server" Text="Is this a system block?" AssociatedControlID="cbSystem"/>
			<asp:CheckBox ID="cbSystem" runat="server" /><br />

			<asp:LinkButton ID="lbSave" runat="server" Text="Save" OnClick="lbSave_Click"/>&nbsp;
			<asp:HyperLink ID="hlCancel" runat="server" Text="Cancel" NavigateUrl="~/Bloc/List" />

		</asp:PlaceHolder>

    </ContentTemplate>
</asp:UpdatePanel>
