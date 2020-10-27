<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Search.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Manager.Search" %>

<Rock:RockUpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" Dismissable="true" />

        <asp:Panel ID="pnlContent" runat="server" CssClass="checkin-manager">
            <div class="input-group mb-4 w-100">
                <Rock:RockTextBox ID="tbSearch" runat="server" CssClass="js-search" PrependText="<i class='fa fa-search'></i>" spellcheck="false" onkeydown="javascript:return handleSearchBoxKeyPress(this, event.keyCode);"/>
            </div>

            <asp:Panel ID="pnlSearchResults" runat="server" CssClass="panel panel-block" Visible="false">
                <div class="panel-body">
                    <div class="grid grid-panel">
                        <Rock:Grid ID="gAttendees" runat="server" DisplayType="Light" UseFullStylesForLightGrid="true" OnRowDataBound="gAttendees_RowDataBound" OnRowSelected="gAttendees_RowSelected" DataKeyNames="PersonGuid,AttendanceIds">
                            <Columns>
                                <Rock:RockLiteralField ID="lPhoto" ItemStyle-CssClass="avatar-column" ColumnPriority="TabletSmall" />
                                <Rock:RockLiteralField ID="lMobileIcon" HeaderStyle-CssClass="d-sm-none" ItemStyle-CssClass="mobile-icon d-table-cell d-sm-none" />
                                <Rock:RockLiteralField ID="lName" HeaderText="Name" ItemStyle-CssClass="name js-name" />
                                <Rock:RockLiteralField ID="lBadges" HeaderStyle-CssClass="d-none d-sm-table-cell" ItemStyle-CssClass="badges d-none d-sm-table-cell align-middle" />
                                <Rock:RockBoundField DataField="Tag" HeaderText="Tag" HeaderStyle-CssClass="d-none d-sm-table-cell" ItemStyle-CssClass="tag d-none d-sm-table-cell align-middle" />
                                <Rock:RockBoundField DataField="ServiceTimes" HeaderText="Service Times" HeaderStyle-CssClass="d-none d-sm-table-cell" ItemStyle-CssClass="service-times d-none d-sm-table-cell align-middle" />
                                <Rock:RockLiteralField ID="lMobileTagAndSchedules" HeaderText="Tag & Schedules" HeaderStyle-CssClass="d-sm-none" ItemStyle-CssClass="tags-and-schedules d-table-cell d-sm-none" />
                                <Rock:RockLiteralField ID="lStatusTag" ItemStyle-CssClass="status-tag d-none d-sm-table-cell align-middle" ItemStyle-HorizontalAlign="Right" ColumnPriority="TabletSmall" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </asp:Panel>
        </asp:Panel>

        <script>
            // handle onkeypress for the search box
            function handleSearchBoxKeyPress(element, keyCode) {
                if (keyCode == 13) {
                    window.location = "javascript:__doPostBack('<%=upnlContent.ClientID %>', 'search')";

                    // prevent double-postback
                    $(element).prop('disabled', true)
                        .attr('disabled', 'disabled')
                        .addClass('disabled');

                    return true;
                }
            }
        </script>

    </ContentTemplate>
</Rock:RockUpdatePanel>

