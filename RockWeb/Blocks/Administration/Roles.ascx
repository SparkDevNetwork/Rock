<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Roles.ascx.cs" Inherits="RockWeb.Blocks.Administration.Roles" %>

<asp:UpdatePanel ID="upRoles" runat="server">
<ContentTemplate>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert-message block-message error"/>

    <asp:Panel ID="pnlList" runat="server">

        <Rock:GridFilter ID="rFilter" runat="server">
            <Rock:LabeledTextBox ID="tbNameFilter" runat="server" LabelText="Role Name"></Rock:LabeledTextBox>
        </Rock:GridFilter>
        <Rock:Grid ID="rGrid" runat="server" AllowSorting="true" RowItemText="role" >
            <Columns>
                <asp:BoundField DataField="Id" HeaderText="Id" SortExpression="Id" />
                <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                <asp:BoundField DataField="Description" HeaderText="Description" />
                <Rock:EditField OnClick="rGrid_Edit" />
                <Rock:DeleteField OnClick="rGrid_Delete" />
            </Columns>
        </Rock:Grid>

    </asp:Panel>

    <asp:Panel ID="pnlDetails" runat="server" Visible="false">
        <asp:HiddenField ID="hfId" runat="server" />

        <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert-message block-message error"/>

        <fieldset>
            <legend><asp:Literal ID="lAction" runat="server"></asp:Literal> Role</legend>
            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Crm.Group, Rock" PropertyName="Name" />
            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Crm.Group, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
        </fieldset>

        <div class="actions">
            <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" onclick="btnSave_Click" />
            <asp:LinkButton id="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
        </div>

    </asp:Panel>

</ContentTemplate>
</asp:UpdatePanel>
