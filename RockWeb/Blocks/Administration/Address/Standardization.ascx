<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Standardization.ascx.cs" Inherits="RockWeb.Blocks.Administration.Address.Standardization" %>

<asp:UpdatePanel ID="upPages" runat="server">
<ContentTemplate>

    <h2>Services</h2>
    <Rock:Grid ID="rGrid" runat="server" PageSize="3" >
        <Columns>
            <Rock:ReorderField />
            <asp:BoundField DataField="Name" HeaderText="Name" />
            <asp:BoundField DataField="Description" HeaderText="Description" />
            <Rock:BoolField DataField="Active" HeaderText="Active" />
            <Rock:EditField OnClick="rGrid_Edit" />
        </Columns>
    </Rock:Grid>

    <asp:Panel ID="pnlDetails" runat="server" Visible="false" CssClass="admin-details">
        
        <asp:HiddenField ID="hfServiceId" runat="server" />
        <asp:ValidationSummary ID="vsPages" runat="server" CssClass="failureNotification" ValidationGroup="StandardizationValidationGroup"/>
        <fieldset>
            <legend>Service Properties</legend>
            <ol id="olProperties" runat="server">
            </ol>
        </fieldset>
        <br />
        <asp:Button id="btnCancel" runat="server" Text="Cancel" CausesValidation="false" OnClick="btnCancel_Click" />
        <asp:Button ID="btnSave" runat="server" Text="Save" ValidationGroup="StandardizationValidationGroup" CssClass="button" onclick="btnSave_Click" />

    </asp:Panel>

    <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Error" Visible="false" />

</ContentTemplate>
</asp:UpdatePanel>
