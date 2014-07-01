<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LocationList.ascx.cs" Inherits="RockWeb.Blocks.Core.LocationList" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <div class="grid">
            <Rock:GridFilter ID="rFilter" runat="server" >
                <Rock:RockTextBox ID="txtStreetAddress1" runat="server" Label="Street Address Starts With" />
                <Rock:RockTextBox ID="txtCity" runat="server" Label="City Starts With" />
                <Rock:RockCheckBox ID="cbNotGeocoded" runat="server" Label="Not Geocoded" />
            </Rock:GridFilter>
            <Rock:Grid ID="rGrid" runat="server" RowItemText="Location" OnRowSelected="rGrid_Edit" TooltipField="Description">
                <Columns>
                    <asp:BoundField DataField="Id" HeaderText="Id" Visible="false" />
                    <asp:BoundField DataField="Street1" HeaderText="Street Address" />
                    <asp:BoundField DataField="City" HeaderText="City" />
                    <asp:BoundField DataField="State" HeaderText="State" />
                    <asp:BoundField DataField="Zip" HeaderText="Zip" />
                    <asp:BoundField DataField="Country" HeaderText="Country" />
                </Columns>
            </Rock:Grid>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
