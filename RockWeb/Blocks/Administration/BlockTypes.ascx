<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BlockTypes.ascx.cs" Inherits="RockWeb.Blocks.Administration.BlockTypes" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlList" runat="server">
            <Rock:Grid ID="gBlockTypes" runat="server" EmptyDataText="No Block Types Found" RowItemText="block type" AllowSorting="true">
                <Columns>
                    <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                    <asp:BoundField HeaderText="Path" DataField="Path" SortExpression="Path" />
                    <asp:BoundField HeaderText="Description" DataField="Description" SortExpression="Description" />
                    <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                    <Rock:EditField OnClick="gBlockTypes_Edit" />
                    <Rock:DeleteField OnClick="gBlockTypes_Delete" />
                </Columns>
            </Rock:Grid>
        </asp:Panel>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false">

            <asp:HiddenField ID="hfBlockTypeId" runat="server" />

            <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert-message block-message error alert" />

            <fieldset>
                <legend>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </legend>
                <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Cms.BlockType, Rock" PropertyName="Name" />
                <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Cms.BlockType, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                <Rock:DataTextBox ID="tbPath" runat="server" SourceTypeName="Rock.Cms.BlockType, Rock" PropertyName="Path" CssClass="input-xlarge" />
            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn secondary" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

        </asp:Panel>

        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Error" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>

