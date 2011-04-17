<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Blocks.ascx.cs" Inherits="Rock.Web.Blocks.Cms.Blocks" %>

<asp:PlaceHolder ID="phList" runat="server" Visible="false">

	<asp:GridView ID="gvList" runat="server" AutoGenerateColumns="false">
		<Columns>
			<asp:HyperLinkField meta:resourcekey="gvListBlock" HeaderText="Block" DataNavigateUrlFormatString="~/Bloc/Edit/{0}" DataNavigateUrlFields="Id" DataTextField="Name" />
			<asp:BoundField meta:resourcekey="gvListPath" HeaderText="Path" DataField="Path" />
			<asp:BoundField meta:resourcekey="gvListName" HeaderText="Name" DataField="Name" />
			<asp:BoundField meta:resourcekey="gvListDescription" HeaderText="Description" DataField="Description" />
		</Columns>
	</asp:GridView> 

	<asp:HyperLink ID="hlAdd" runat="server" meta:resourcekey="hlAdd" Text="Add New Block" NavigateUrl="~/Bloc/Add"></asp:HyperLink>

</asp:PlaceHolder>

<asp:UpdatePanel UpdateMode="Always" runat="server">
	<ContentTemplate>

		<asp:PlaceHolder ID="phDetails" runat="server" Visible="false">

			<asp:Label ID="lblPath" runat="server" meta:resourcekey="lblPath" Text="Path" AssociatedControlID="tbPath"/>
			<asp:TextBox ID="tbPath" runat="server"></asp:TextBox><br />

			<asp:Label ID="lblName" runat="server" meta:resourcekey="lblName" Text="Name" AssociatedControlID="tbName" />
			<asp:TextBox ID="tbName" runat="server"></asp:TextBox><br />

			<asp:Label ID="lblDescription" runat="server" meta:resourcekey="lblDescription" Text="Description" AssociatedControlID="tbDescription" />
			<asp:TextBox ID="tbDescription" runat="server"></asp:TextBox><br />

			<asp:Label ID="lblSystem" runat="server" meta:resourcekey="lblSystem" Text="Is this a system block?" AssociatedControlID="cbSystem"/>
			<asp:CheckBox ID="cbSystem" runat="server" /><br />

			<asp:LinkButton ID="lbSave" runat="server" meta:resourcekey="lbSave" Text="Save" OnClick="lbSave_Click"/>&nbsp;
			<asp:HyperLink ID="hlCancel" runat="server" meta:resourcekey="hlCancel" Text="Cancel" NavigateUrl="~/Bloc/List" />

		</asp:PlaceHolder>

    </ContentTemplate>
</asp:UpdatePanel>
