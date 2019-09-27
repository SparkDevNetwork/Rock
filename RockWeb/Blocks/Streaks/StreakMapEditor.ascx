<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StreakMapEditor.ascx.cs" Inherits="RockWeb.Blocks.Streaks.StreakMapEditor" %>

<asp:UpdatePanel ID="upMapEditor" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-calendar-check"></i>
                    <asp:Literal ID="lTitle" runat="server" />
                </h1>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbMessage" runat="server" NotificationBoxType="Info" />

                <div class="well">
                    <Rock:SlidingDateRangePicker runat="server" ID="sdrpDateRange" EnabledSlidingDateRangeUnits="Day, Week, Month, Year" EnabledSlidingDateRangeTypes="Previous, Last, Current, DateRange" />
                    <asp:LinkButton ID="btnRefresh" runat="server" CssClass="btn btn-xs btn-default" ToolTip="Refresh" OnClick="btnRefresh_Click"><i class="fa fa-refresh"></i> Update</asp:LinkButton>
                </div>

                <Rock:RockCheckBoxList ID="cblCheckboxes" runat="server" RepeatDirection="Vertical" />

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
