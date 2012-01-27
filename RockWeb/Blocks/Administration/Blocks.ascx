<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Blocks.ascx.cs" Inherits="RockWeb.Blocks.Administration.Blocks" %>

<asp:UpdatePanel ID="upBlocks" runat="server">
<ContentTemplate>

    <asp:Panel ID="pnlList" runat="server">
        
        <Rock:Grid ID="gBlocks" runat="server" EmptyDataText="No Blocks Found" >
            <Columns>
                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                <asp:BoundField HeaderText="Path" DataField="Path" SortExpression="Path" />
                <asp:BoundField HeaderText="Description" DataField="Description" />
                <Rock:BoolField DataField="System" HeaderText="System" />
                <Rock:EditField OnClick="gBlocks_Edit" />
                <Rock:DeleteField OnClick="gBlocks_Delete" />
            </Columns>
        </Rock:Grid>

    </asp:Panel>

    <asp:Panel ID="pnlDetails" runat="server" Visible="false">
    
        <asp:HiddenField ID="hfBlockId" runat="server" />

        <asp:ValidationSummary runat="server" CssClass="failureNotification"/>

            <fieldset>
                <legend><asp:Literal ID="lAction" runat="server"></asp:Literal> Block</legend>
                <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.CMS.Block, Rock" PropertyName="Name" />
                <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.CMS.Block, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                <Rock:DataTextBox ID="tbPath" runat="server" SourceTypeName="Rock.CMS.Block, Rock" PropertyName="Path" />
            </fieldset>

        </div>

        <div class="actions">
            <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn primary" onclick="btnSave_Click" />
            <asp:LinkButton id="btnCancel" runat="server" Text="Cancel" CssClass="btn secondary" CausesValidation="false" OnClick="btnCancel_Click" />
        </div>

    </asp:Panel>

    <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Error" Visible="false" />

</ContentTemplate>
</asp:UpdatePanel>

