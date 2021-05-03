<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RoomList.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Manager.RoomList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lPanelTitle" runat="server" Text="Room List" />
                </h1>
                <div class="panel-labels">
                    <asp:LinkButton ID="btnShowFilter" runat="server" CssClass="btn btn-default btn-xs" OnClick="btnShowFilter_Click">
                        <i class="fa fa-filter"></i>
                        Filters
                    </asp:LinkButton>
                </div>
            </div>
            <asp:Panel ID="pnlFilterCriteria" runat="server" CssClass="panel-heading" Visible="false">
                <div class="w-100">
                    <Rock:RockListBox ID="lbSchedules" runat="server" Label="Schedules" ValidationGroup="vgFilterCriteria" />
                </div>
                <div class="actions mt-2">
                    <asp:LinkButton ID="btnApplyFilter" runat="server" CssClass="filter btn btn-action btn-xs" Text="Apply Filter" OnClick="btnApplyFilter_Click" ValidationGroup="vgFilterCriteria" CausesValidation="true" />
                    <asp:LinkButton ID="btnClearFilter" runat="server" CssClass="filter-clear btn btn-default btn-xs" Text="Clear Filter" OnClick="btnClearFilter_Click" CausesValidation="false" />
                </div>
            </asp:Panel>
            <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" />
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:Grid ID="gRoomList" runat="server" RowItemText="Room" DisplayType="Light" UseFullStylesForLightGrid="true" ShowActionRow="false" OnRowDataBound="gRoomList_RowDataBound" OnRowSelected="gRoomList_RowSelected">
                        <Columns>
                            <Rock:RockLiteralField ID="lRoomName" HeaderText="Room" />
                            <Rock:RockLiteralField ID="lGroupNameAndPath" HeaderText="Group" />
                            <Rock:RockLiteralField ID="lCheckedInCount" HeaderText="Checked-in" HeaderStyle-CssClass="text-nowrap" />
                            <Rock:RockLiteralField ID="lPresentCount" HeaderText="Present" />
                            <Rock:RockLiteralField ID="lCheckedOutCount" HeaderText="Checked-Out" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
