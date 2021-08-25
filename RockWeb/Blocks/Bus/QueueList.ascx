<%@ Control Language="C#" AutoEventWireup="true" CodeFile="QueueList.ascx.cs" Inherits="RockWeb.Blocks.Bus.QueueList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-code-branch"></i>
                    Queues
                </h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gList" runat="server" AllowSorting="true" OnRowSelected="gList_RowSelected">
                        <Columns>
                            <Rock:RockBoundField DataField="QueueName" HeaderText="Queue" SortExpression="QueueName" />
                            <Rock:RockBoundField DataField="QueueType" HeaderText="Type" SortExpression="QueueType" />
                            <Rock:RockBoundField DataField="TimeToLiveSeconds" HeaderText="TTL (Seconds)" SortExpression="TimeToLiveSeconds" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                            <Rock:RockBoundField DataField="RatePerMinute" HeaderText="Consumed Last Minute" SortExpression="RatePerMinute" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                            <Rock:RockBoundField DataField="RatePerHour" HeaderText="Consumed Last Hour" SortExpression="RatePerHour" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                            <Rock:RockBoundField DataField="RatePerDay" HeaderText="Consumed Last Day" SortExpression="RatePerDay" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>