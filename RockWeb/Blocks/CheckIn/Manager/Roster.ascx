<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Roster.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Manager.Roster" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $('.js-cancel-checkin').on('click', function (event) {
            event.stopImmediatePropagation();
            var personName = $(this).parent().siblings(".js-name").find(".js-checkin-person-name").first().text();
            return Rock.dialogs.confirmDelete(event, 'Check-in for ' + personName);
        });
    });
</script>
<Rock:RockUpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>


        <asp:Panel ID="pnlContent" runat="server" CssClass="checkin-roster">

            <div class="page-title-inject d-flex flex-wrap justify-content-between align-items-center">
                <div class="my-2">
                    <Rock:LocationPicker ID="lpLocation" runat="server" LabelName="Select a Location" AllowedPickerModes="Named" CssClass="picker-lg" OnSelectLocation="lpLocation_SelectLocation" IncludeInactiveNamedLocations="true" />
                </div>
                <asp:Panel ID="pnlSubPageNav" runat="server" CssClass="my-2">
                    <Rock:PageNavButtons ID="pbSubPages" runat="server" IncludeCurrentQueryString="true" />
                </asp:Panel>
            </div>

            <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" />

            <asp:Panel ID="pnlRoster" runat="server" CssClass="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title">Room Roster</h1>
                    <div class="pull-right">
                        <Rock:ButtonGroup ID="bgStatus" runat="server" FormGroupCssClass="toggle-container" SelectedItemClass="btn btn-info btn-xs" UnselectedItemClass="btn btn-default btn-xs" AutoPostBack="true" OnSelectedIndexChanged="bgStatus_SelectedIndexChanged">
                            <asp:ListItem Text="All" Value="1" />
                            <asp:ListItem Text="Checked-in" Value="2" />
                            <asp:ListItem Text="Present" Value="3" />
                        </Rock:ButtonGroup>
                    </div>
                </div>
                <div class="panel-body">
                    <div class="grid grid-panel">
                        <Rock:Grid ID="gAttendees" runat="server" DisplayType="Light" UseFullStylesForLightGrid="true" OnRowDataBound="gAttendees_RowDataBound" OnRowSelected="gAttendees_RowSelected" DataKeyNames="PersonGuid,AttendanceIds">
                            <Columns>
                                <Rock:RockLiteralField ID="lPhoto" ItemStyle-CssClass="avatar-column" ColumnPriority="TabletSmall" />
                                <Rock:RockLiteralField ID="lMobileIcon" HeaderStyle-CssClass="d-table-cell d-sm-none" ItemStyle-CssClass="mobile-icon d-table-cell d-sm-none py-0 align-middle" />
                                <Rock:RockLiteralField ID="lName" HeaderText="Name" ItemStyle-CssClass="name js-name" />
                                <Rock:RockLiteralField ID="lBadges" HeaderStyle-CssClass="d-none d-sm-table-cell" ItemStyle-CssClass="badges d-none d-sm-table-cell align-middle" />
                                <Rock:RockBoundField DataField="Tag" HeaderText="Tag" HeaderStyle-CssClass="d-none d-sm-table-cell" ItemStyle-CssClass="tag d-none d-sm-table-cell align-middle" />
                                <Rock:RockBoundField DataField="ServiceTimes" HeaderText="Service Times" HeaderStyle-CssClass="d-none d-sm-table-cell" ItemStyle-CssClass="service-times d-none d-sm-table-cell align-middle" />
                                <Rock:RockLiteralField ID="lMobileTagAndSchedules" HeaderText="Tag & Schedules" HeaderStyle-CssClass="d-sm-none" ItemStyle-CssClass="tags-and-schedules d-table-cell d-sm-none" />
                                <Rock:RockLiteralField ID="lElapsedCheckInTime" HeaderText="Check-in Time" HeaderStyle-HorizontalAlign="Right" ItemStyle-CssClass="check-in-time align-middle" ItemStyle-HorizontalAlign="Right" ColumnPriority="TabletSmall" />
                                <Rock:RockLiteralField ID="lStatusTag"  HeaderStyle-CssClass="d-none d-sm-table-cell" ItemStyle-CssClass="status-tag d-none d-sm-table-cell align-middle" ItemStyle-HorizontalAlign="Right" ColumnPriority="TabletSmall" />

                                <Rock:LinkButtonField ID="btnCancel"  ItemStyle-CssClass="grid-columnaction"  CssClass="btn btn-danger btn-square js-cancel-checkin" Text="<span class='d-none d-sm-inline'>Cancel</span> <i class='fa fa-times'></i>" OnClick="btnCancel_Click" OnDataBound="btnCancel_DataBound" />
                                <Rock:LinkButtonField ID="btnPresent"  ItemStyle-CssClass="grid-columnaction" CssClass="btn btn-success btn-square" Text="<span class='d-none d-sm-inline'>Present</span> <i class='fa fa-user-check'></i>" OnClick="btnPresent_Click" OnDataBound="btnPresent_DataBound" />
                                <Rock:LinkButtonField ID="btnCheckOut" ItemStyle-CssClass="grid-columnaction" CssClass="btn btn-primary btn-square" Text="<span class='d-none d-sm-inline'>Check-out</span> <i class='fa fa-user-minus'></i>" OnClick="btnCheckOut_Click" OnDataBound="btnCheckOut_DataBound" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </asp:Panel>

        </asp:Panel>

    </ContentTemplate>
</Rock:RockUpdatePanel>