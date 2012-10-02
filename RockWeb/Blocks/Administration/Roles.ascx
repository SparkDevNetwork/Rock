<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Roles.ascx.cs" Inherits="RockWeb.Blocks.Administration.Roles" %>

<asp:UpdatePanel ID="upRoles" runat="server">
<ContentTemplate>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert-message block-message error"/>

    <asp:Panel ID="pnlList" runat="server">

        <div class="grid-filter">
        <fieldset>
            <legend>Filter Options</legend>
            <Rock:LabeledTextBox ID="tbNameFilter" runat="server" LabelText="Role Name" AutoPostBack="true" OnTextChanged="tbNameFilter_TextChanged"></Rock:LabeledTextBox>
        </fieldset>
        </div>

        <Rock:Grid ID="rGrid" runat="server" AllowSorting="true" >
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
            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Groups.Group, Rock" PropertyName="Name" />
            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Groups.Group, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
        </fieldset>

        <div class="actions">
            <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn primary" onclick="btnSave_Click" />
            <asp:LinkButton id="btnCancel" runat="server" Text="Cancel" CssClass="btn secondary" CausesValidation="false" OnClick="btnCancel_Click" />
        </div>

    </asp:Panel>

</ContentTemplate>
</asp:UpdatePanel>
