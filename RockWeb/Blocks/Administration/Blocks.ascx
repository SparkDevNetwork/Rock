<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Blocks.ascx.cs" Inherits="RockWeb.Blocks.Administration.Blocks" %>

<asp:PlaceHolder ID="phList" runat="server" Visible="false">
    <Rock:Grid ID="rGrid" runat="server" >
        <Columns>
            <asp:HyperLinkField HeaderText="Block" DataTextField="Name" DataNavigateUrlFormatString="~/Bloc/Edit/{0}" DataNavigateUrlFields="id" />
            <asp:BoundField HeaderText="Description" DataField="Description" />
            <Rock:BoolField DataField="System" HeaderText="System" SortExpression="System"></Rock:BoolField>
            <Rock:DeleteField/>
        </Columns>
    </Rock:Grid>
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
