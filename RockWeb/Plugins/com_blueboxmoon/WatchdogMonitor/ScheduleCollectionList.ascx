<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduleCollectionList.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.WatchdogMonitor.ScheduleCollectionList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlSchedules" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-clock"></i> Schedules</h1>
            </div>

            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:Grid ID="gSchedules" runat="server" OnRowSelected="gSchedules_RowSelected" OnGridRebind="gSchedules_GridRebind" OnRowDataBound="gSchedules_RowDataBound">
                        <Columns>
                            <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <asp:BoundField DataField="Schedule" HeaderText="Schedule" />
                            <Rock:DeleteField OnClick="gSchedulesDelete_Click" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>