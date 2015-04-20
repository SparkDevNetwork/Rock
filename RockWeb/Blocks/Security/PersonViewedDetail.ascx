<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonViewedDetail.ascx.cs" Inherits="RockWeb.Blocks.Security.PersonViewedDetail" %>

<asp:UpdatePanel ID="upPersonViewedDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">

            <Rock:ModalAlert ID="mdGridWarning" runat="server" />

            <div id="pnlViewedBy" runat="server">
                <h4 id="gridTitle" runat="server">Profile View Details</h4>
                
                <div class="grid">
                    <Rock:Grid ID="gViewDetails" runat="server" DisplayType="Full" AllowSorting="true" EmptyDataText="No Viewing Details Found" RowItemText="Views">
                        <Columns>
                            <Rock:RockBoundField DataField="Source" HeaderText="Source" SortExpression="Source" />
                            <Rock:RockBoundField DataField="ViewDateTime" HeaderText="Date" SortExpression="ViewDateTime" />
                            <Rock:RockBoundField DataField="IpAddress" HeaderText="IP Address" SortExpression="IpAddress" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
