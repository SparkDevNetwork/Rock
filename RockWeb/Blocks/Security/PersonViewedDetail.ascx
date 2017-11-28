<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonViewedDetail.ascx.cs" Inherits="RockWeb.Blocks.Security.PersonViewedDetail" %>

<asp:UpdatePanel ID="upPersonViewedDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                        <h1 class="panel-title"><i class="fa fa-eye"></i> <asp:Literal id="gridTitle" runat="server">Profile View Details</asp:Literal></h1>
                    </div>
                    <div class="panel-body">

                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

                        <div id="pnlViewedBy" runat="server">            
                            <div class="grid grid-panel">
                                <Rock:Grid ID="gViewDetails" runat="server" DisplayType="Full" AllowSorting="true" EmptyDataText="No Viewing Details Found" RowItemText="Views">
                                    <Columns>
                                        <Rock:RockBoundField DataField="Source" HeaderText="Source" SortExpression="Source" />
                                        <Rock:RockBoundField DataField="ViewDateTime" HeaderText="Date" SortExpression="ViewDateTime" />
                                        <Rock:RockBoundField DataField="IpAddress" HeaderText="IP Address" SortExpression="IpAddress" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                    </div>
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
