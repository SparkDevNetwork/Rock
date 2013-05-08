 <%@ Control Language="C#" AutoEventWireup="true" CodeFile="Components.ascx.cs" Inherits="RockWeb.Blocks.Administration.Components" %>

<asp:UpdatePanel runat="server">
<ContentTemplate>

    <Rock:ModalAlert ID="mdAlert" runat="server" />

    <asp:Panel ID="pnlList" runat="server" Visible="true" >
        
        <Rock:GridFilter ID="rFilter" runat="server">
            <Rock:LabeledTextBox ID="tbName" runat="server" LabelText="Name" />
            <Rock:LabeledTextBox ID="tbDescription" runat="server" LabelText="Description" />
            <Rock:LabeledRadioButtonList ID="rblActive" runat="server" LabelText="Active" RepeatDirection="Horizontal">
                <asp:ListItem Value="" Text="All" />
                <asp:ListItem Value="Yes" Text="Yes" />
                <asp:ListItem Value="No" Text="No" />
            </Rock:LabeledRadioButtonList>
        </Rock:GridFilter>

        <Rock:Grid ID="rGrid" runat="server" EmptyDataText="No Components Found" OnRowSelected="rGrid_Edit">
            <Columns>
                <Rock:ReorderField />
                <asp:BoundField DataField="Name" HeaderText="Name" />
                <asp:BoundField DataField="Description" HeaderText="Description" />
                <Rock:BoolField DataField="IsActive" HeaderText="Active" />
                <asp:TemplateField>
                    <HeaderStyle CssClass="span1" />
                    <ItemStyle HorizontalAlign="Center"/>
                    <ItemTemplate>
                        <a id="aSecure" runat="server" class="btn btn-mini" height="500px"><i class="icon-lock"></i></a>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </Rock:Grid>

    </asp:Panel>

    <asp:Panel ID="pnlDetails" runat="server" Visible="false" CssClass="admin-details">
        
        <asp:ValidationSummary runat="server" CssClass="alert alert-error" />

        <fieldset>
            <legend><asp:Literal ID="lProperties" runat="server"></asp:Literal></legend>
            <asp:PlaceHolder ID="phProperties" runat="server"></asp:PlaceHolder>
        </fieldset>

        <div class="actions">
            <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" onclick="btnSave_Click" />
            <asp:LinkButton id="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
        </div>

    </asp:Panel>

</ContentTemplate>
</asp:UpdatePanel>
