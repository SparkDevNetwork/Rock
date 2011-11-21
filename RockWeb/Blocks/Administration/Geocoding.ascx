<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Geocoding.ascx.cs" Inherits="RockWeb.Blocks.Administration.Geocoding" %>

<asp:UpdatePanel ID="upPages" runat="server">
<ContentTemplate>

    <Rock:Grid ID="rGrid" runat="server" PageSize="3" >
        <Columns>
            <Rock:ReorderField />
            <asp:BoundField DataField="Name" HeaderText="Name" />
        </Columns>
    </Rock:Grid>

    <asp:Panel ID="pnlDetails" runat="server" Visible="false" CssClass="admin-details">

        <asp:ValidationSummary ID="vsPages" runat="server" CssClass="failureNotification" ValidationGroup="GeocodingValidationGroup"/>
        <fieldset>
            <legend>Properties</legend>
            <ol>
            </ol>
        </fieldset>
        <br />

    </asp:Panel>

    <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Error" Visible="false" />

</ContentTemplate>
</asp:UpdatePanel>
