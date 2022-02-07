<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Roster.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Manager.Roster" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $('.js-cancel-checkin').on('click', function (event) {
            event.stopImmediatePropagation();
            var personName = $(this).parent().siblings(".js-name").find(".js-checkin-person-name").first().text();
            return Rock.dialogs.confirmDelete(event, 'Check-in for ' + personName);
        });

        var showMarkPresentConfirmation = $('.js-mark-present-show-confirmation').val() == 'true';

        if (showMarkPresentConfirmation) {
            $('.js-mark-present').on('click', function (e) {

                // Make sure the element that triggered this event isn't disabled.
                if (e.currentTarget && e.currentTarget.disabled) {
                    return false;
                }

                e.preventDefault();

                Rock.dialogs.confirm('Are you sure you want to mark this person back to Present?', function (result) {
                    if (result) {
                        window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                    }
                })
            });
        }
    });
</script>
<Rock:RockUpdatePanel ID="upnlContent" class="overflow-visible" runat="server">
    <ContentTemplate>


        <asp:Panel ID="pnlContent" runat="server" CssClass="checkin-roster">

            <div class="page-title-inject d-flex flex-wrap justify-content-between align-items-center">
                <div class="my-2">
                    <Rock:LocationPicker ID="lpLocation" runat="server" LabelName="Select a Location" AllowedPickerModes="Named" CssClass="picker-lg" OnSelectLocation="lpLocation_SelectLocation" IncludeInactiveNamedLocations="true" />
                </div>
                <asp:Panel ID="pnlSubPageNav" runat="server" CssClass="d-print-none my-2">
                    <Rock:PageNavButtons ID="pbSubPages" runat="server" IncludeCurrentQueryString="true" />
                </asp:Panel>
            </div>

            <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" />
            <Rock:HiddenFieldWithClass ID="hfMarkPresentShowConfirmation" CssClass="js-mark-present-show-confirmation" runat="server" />

            <!-- Check Out All button -->
            <div class="row">
                <div class="col-md-12">
                    <asp:LinkButton ID="btnCheckoutAll" runat="server" CssClass="btn btn-primary btn-sm pull-right mb-2" Visible="false" Text="Check Out All" OnClick="btnCheckoutAll_Click" />
                </div>
            </div>

            <!-- Roster (main view) -->
            <asp:Panel ID="pnlRoster" runat="server" CssClass="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title">Room Roster</h1>
                    <div class="pull-right">
                        <Rock:ButtonGroup ID="bgStatus" runat="server" FormGroupCssClass="toggle-container" SelectedItemClass="btn btn-info btn-xs" UnselectedItemClass="btn btn-default btn-xs" AutoPostBack="true" OnSelectedIndexChanged="bgStatus_SelectedIndexChanged">
                            <asp:ListItem Text="All" Value="1" />
                            <asp:ListItem Text="Checked-in" Value="2" />
                            <asp:ListItem Text="Present" Value="3" />
                            <asp:ListItem Text="Checked-out" Value="4" />
                        </Rock:ButtonGroup>
                    </div>
                </div>
                <div class="panel-body">
                    <div class="grid grid-panel">
                        <Rock:Grid ID="gAttendees" runat="server" DisplayType="Light" UseFullStylesForLightGrid="true" OnRowDataBound="gAttendees_RowDataBound" OnRowSelected="gAttendees_RowSelected" DataKeyNames="PersonGuid,AttendanceIds" AllowSorting="true" ShowActionRow="false">
                            <Columns>
                                <Rock:RockLiteralField ID="lPhoto" ItemStyle-CssClass="avatar-column" ColumnPriority="TabletSmall" />
                                <Rock:RockLiteralField ID="lMobileIcon" HeaderStyle-CssClass="d-table-cell d-sm-none d-print-none" ItemStyle-CssClass="mobile-icon d-table-cell d-sm-none d-print-none py-0 align-middle" />
                                <Rock:RockLiteralField ID="lName" HeaderText="Name" HeaderStyle-CssClass="print-first-col" ItemStyle-CssClass="name js-name align-middle print-first-col" SortExpression="NickName,LastName,PersonGuid" />
                                <Rock:RockLiteralField ID="lBadges" HeaderStyle-CssClass="d-none d-sm-table-cell d-print-none" ItemStyle-CssClass="badges d-none d-sm-table-cell d-print-none align-middle" />
                                <Rock:RockLiteralField ID="lGroupNameAndPath" HeaderText="Group" Visible="false" SortExpression="GroupName,NickName,LastName,PersonGuid" />
                                <Rock:RockBoundField DataField="Tag" HeaderText="Tag" HeaderStyle-CssClass="d-none d-sm-table-cell" ItemStyle-CssClass="tag d-none d-sm-table-cell align-middle" SortExpression="Tag" />
                                <Rock:RockBoundField DataField="ServiceTimes" HeaderText="Service Times" HeaderStyle-CssClass="d-none d-sm-table-cell" ItemStyle-CssClass="service-times d-none d-sm-table-cell align-middle" SortExpression="ServiceTimesScheduleOrder" />
                                <Rock:RockLiteralField ID="lMobileTagAndSchedules" HeaderText="Tag" HeaderStyle-CssClass="d-sm-none" ItemStyle-CssClass="tags-and-schedules d-table-cell d-sm-none" />
                                <Rock:RockLiteralField ID="lElapsedCheckInTime" HeaderText="Check-in Time" HeaderStyle-HorizontalAlign="Right" ItemStyle-CssClass="check-in-time align-middle" ItemStyle-HorizontalAlign="Right" ColumnPriority="TabletSmall" SortExpression="CheckInTime" />
                                <Rock:RockLiteralField ID="lElapsedPresentTime" HeaderText="Present Time" HeaderStyle-HorizontalAlign="Right" ItemStyle-CssClass="check-in-time align-middle" ItemStyle-HorizontalAlign="Right" ColumnPriority="TabletSmall" SortExpression="PresentDateTime" />
                                <Rock:RockLiteralField ID="lElapsedCheckedOutTime" HeaderText="Checked-out Time" HeaderStyle-HorizontalAlign="Right" ItemStyle-CssClass="check-in-time align-middle" ItemStyle-HorizontalAlign="Right" ColumnPriority="TabletSmall" SortExpression="CheckOutTime" />
                                <Rock:RockLiteralField ID="lStatusTag" HeaderStyle-CssClass="d-none d-sm-table-cell print-last-col" ItemStyle-CssClass="status-tag d-none d-sm-table-cell align-middle print-last-col" ItemStyle-HorizontalAlign="Right" ColumnPriority="TabletSmall" />

                                <Rock:LinkButtonField ID="btnCancel" ItemStyle-CssClass="grid-columnaction d-print-none" CssClass="btn btn-danger btn-square js-cancel-checkin" Text="<i class='fa fa-times'></i>" ToolTip="Cancel" OnClick="btnCancel_Click" OnDataBound="btnCancel_DataBound" />
                                <Rock:LinkButtonField ID="btnPresent" ItemStyle-CssClass="grid-columnaction d-print-none" CssClass="btn btn-success btn-square js-mark-present" Text="<i class='fa fa-user-check'></i>" ToolTip="Mark Present" OnClick="btnPresent_Click" OnDataBound="btnPresent_DataBound" />
                                
                                <Rock:LinkButtonField ID="btnStaying" ItemStyle-CssClass="grid-columnaction" CssClass="btn btn-default btn-square" Text="<i class='fa fa-user-clock'></i>" ToolTip="Staying" OnClick="btnStaying_Click" />
                                <Rock:LinkButtonField ID="btnNotPresent" ItemStyle-CssClass="grid-columnaction" CssClass="btn btn-default btn-square" Text="<i class='fa fa-user-slash'></i>" ToolTip="Not Present" OnClick="btnNotPresent_Click" OnDataBound="btnNotPresent_DataBound" />
                                <Rock:LinkButtonField ID="btnCheckOut" ItemStyle-CssClass="grid-columnaction" CssClass="btn btn-primary btn-square" Text="<i class='fa fa-user-minus'></i>" ToolTip="Check out" OnClick="btnCheckOut_Click" OnDataBound="btnCheckOut_DataBound" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </asp:Panel>

            <!-- Confirm Staying modal -->
            <Rock:ModalDialog ID="mdConfirmStaying" runat="server" Title="Confirm" SaveButtonText="Check In" OnSaveClick="mdConfirmStaying_SaveClick">
                <Content>
                    <asp:HiddenField ID="hfConfirmStayingAttendanceId" runat="server" />
                    <Rock:NotificationBox ID="nbConfirmStayingWarning" runat="server" NotificationBoxType="Warning" />
                    <asp:Literal ID="lConfirmStayingPromptText" runat="server" Text="Which schedule would you like this person to stay for:" />
                    <Rock:RockRadioButtonList ID="rblScheduleStayingFor" runat="server" Label="Schedule" RepeatDirection="Horizontal" />
                </Content>
            </Rock:ModalDialog>

            <!-- Confirm Checkout All modal -->
            <Rock:ModalDialog ID="mdConfirmCheckoutAll" runat="server" Title="Confirm" ValidationGroup="vgConfirmCheckoutAll" SaveButtonText="Check Out" OnSaveClick="mdConfirmCheckoutAll_SaveClick">
                <Content>
                    <asp:Literal ID="lConfirmCheckoutAll" runat="server" Text="Which schedules would you like to check out for:" />
                    <Rock:RockCheckBoxList ID="cblSchedulesCheckoutAll" runat="server" Label="Schedules" RepeatDirection="Horizontal" Required="true" ValidationGroup="vgConfirmCheckoutAll" />
                </Content>
            </Rock:ModalDialog>

            <!-- Confirm Checkout Attendee modal -->
            <Rock:ModalDialog ID="mdConfirmCheckoutAttendee" runat="server" Title="Confirm" ValidationGroup="vgConfirmCheckoutAttendee" SaveButtonText="Check Out" OnSaveClick="mdConfirmCheckoutAttendee_SaveClick" >
                <Content>
                    <asp:HiddenField ID="hfConfirmCheckoutAttendeeAttendanceIds" runat="server" />
                    <asp:Literal ID="lConfirmCheckoutAttendee" runat="server" Text="Which schedules would you like to check out for:" />
                    <Rock:RockCheckBoxList ID="cblSchedulesCheckoutAttendee" runat="server" Label="Schedules" RepeatDirection="Horizontal" Required="true" ValidationGroup="vgConfirmCheckoutAttendee" />
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>
    </ContentTemplate>
</Rock:RockUpdatePanel>
