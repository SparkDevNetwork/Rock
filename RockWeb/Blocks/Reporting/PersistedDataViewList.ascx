<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersistedDataViewList.ascx.cs" Inherits="RockWeb.Blocks.Reporting.PersistedDataViewList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-database"></i>
                    Persisted Data View List
                </h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gList" runat="server" AllowSorting="true" OnRowSelected="gList_RowSelected">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="PersistedScheduleIntervalMinutes" HeaderText="Persisted Interval" SortExpression="PersistedScheduleIntervalMinutes" />
                            <Rock:RockBoundField DataField="PersistedLastRunDurationMilliseconds" HeaderText="Time to Build (ms)" NullDisplayText="-" DataFormatString="{0:F0}" SortExpression="PersistedLastRunDurationMilliseconds" />
                            <Rock:RockBoundField DataField="RunCount" HeaderText="Run Count" NullDisplayText="-" SortExpression="RunCount" />
                            <Rock:DateTimeField DataField="LastRunDateTime" HeaderText="Last Run Date Time" NullDisplayText="-" SortExpression="LastRunDateTime" />
                            <Rock:DateTimeField DataField="PersistedLastRefreshDateTime" HeaderText="Last Refresh" NullDisplayText="-" SortExpression="LastRefreshDateTime" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
