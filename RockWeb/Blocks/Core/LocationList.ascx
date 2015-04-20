<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LocationList.ascx.cs" Inherits="RockWeb.Blocks.Core.LocationList" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-building-o"></i> Location List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilter" runat="server" >
                        <Rock:RockTextBox ID="txtStreetAddress1" runat="server" Label="Street Address Starts With" />
                        <Rock:RockTextBox ID="txtCity" runat="server" Label="City Starts With" />
                        <Rock:RockCheckBox ID="cbNotGeocoded" runat="server" Label="Not Geocoded" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="rGrid" runat="server" RowItemText="Location" OnRowSelected="rGrid_Edit" TooltipField="Description">
                        <Columns>
                            <Rock:RockBoundField DataField="Id" HeaderText="Id" Visible="false" />
                            <Rock:RockBoundField DataField="Street1" HeaderText="Street Address" />
                            <Rock:RockBoundField DataField="City" HeaderText="City" />
                            <Rock:RockBoundField DataField="State" HeaderText="State" />
                            <Rock:RockBoundField DataField="PostalCode" HeaderText="Postal Code" />
                            <Rock:RockBoundField DataField="Country" HeaderText="Country" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
