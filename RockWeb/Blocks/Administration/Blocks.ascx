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
            <fieldset>
                <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.CMS.Block, Rock" TextBoxCssClass="xlarge" PropertyName="Name" />

			    <Rock:DataTextBox ID="tbPath" runat="server" SourceTypeName="Rock.CMS.Block, Rock" TextBoxCssClass="xlarge" PropertyName="Path" />

                <Rock:DataTextBox ID="tbDescription" runat="server" TextBoxTextMode="MultiLine" TextBoxRows="6" SourceTypeName="Rock.CMS.Block, Rock" TextBoxCssClass="xxlarge" PropertyName="Description" />

                <Rock:LabeledCheckBox ID="cbSystem" runat="server" LabelText="Is this a system block?"></Rock:LabeledCheckBox>

                <div class="actions">
			        <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn primary" OnClick="lbSave_Click"/>&nbsp;
			        <asp:HyperLink ID="hlCancel" runat="server" Text="Cancel" CssClass="btn" NavigateUrl="~/Bloc/List" />
                </div>
            </fieldset>
		</asp:PlaceHolder>

    </ContentTemplate>
</asp:UpdatePanel>
