<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PledgeAnalytics.ascx.cs" Inherits="RockWeb.Blocks.Finance.PledgeAnalytics" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-star"></i> Blank List Block</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gList" runat="server" AllowSorting="true">
                        <Columns>
                            <Rock:RockBoundField DataField="GivingId" HeaderText="GivingId" SortExpression="GivingId" />
                            <Rock:RockBoundField DataField="PledgeTotal" HeaderText="Pledge Total" SortExpression="PledgeTotal" />
                            <Rock:RockBoundField DataField="AccountName" HeaderText="Account" SortExpression="AccountName" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
