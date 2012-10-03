<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Tags.ascx.cs" Inherits="RockWeb.Blocks.Administration.Tags" %>

<asp:UpdatePanel ID="upPanel" runat="server">
<ContentTemplate>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert-message block-message error"/>

    <asp:Panel ID="pnlList" runat="server">

        <Rock:Grid ID="rGrid" runat="server" >
            <Columns>
                <Rock:ReorderField />
                <asp:BoundField DataField="Id" HeaderText="Id" Visible="false" />
                <asp:BoundField DataField="Name" HeaderText="Name" />
                <Rock:EditField OnClick="rGrid_Edit" />
                <Rock:DeleteField OnClick="rGrid_Delete" />
            </Columns>
        </Rock:Grid>

    </asp:Panel>

    <asp:Panel ID="pnlDetails" runat="server" Visible="false">

        <asp:HiddenField ID="hfId" runat="server" />

        <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert-message block-message error"/>

        <fieldset>
            <legend><asp:Literal ID="lAction" runat="server"></asp:Literal> Tag</legend>
            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Core.Attribute, Rock" PropertyName="Name" />
        </fieldset>

        <div class="actions">
            <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn primary" onclick="btnSave_Click" />
            <asp:LinkButton id="btnCancel" runat="server" Text="Cancel" CssClass="btn secondary" CausesValidation="false" OnClick="btnCancel_Click" />
        </div>

    </asp:Panel>

</ContentTemplate>
</asp:UpdatePanel>
