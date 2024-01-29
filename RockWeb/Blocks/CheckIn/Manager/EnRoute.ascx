<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EnRoute.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Manager.EnRoute" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">People En Route
                </h1>
            </div>
            <div class="panel-body">
                <Rock:GroupPicker ID="gpGroups" runat="server" Label="Groups" AllowMultiSelect="true" ValidationGroup="vgFilterCriteria" EnableFullWidth="true" />
                <small>
                    <asp:Literal ID="lblIncludeChildGroups" runat="server" Text="Child Groups Included" Visible="false" /></small>
                <Rock:RockCheckBox ID="cbIncludeChildGroups" runat="server" Text="Include Child Groups" />
                <Rock:RockListBox ID="lbSchedules" runat="server" Label="Schedules" ValidationGroup="vgFilterCriteria" />

                <Rock:RockTextBox ID="tbSearch" runat="server" CssClass="js-search" Label="Search by Name" PrependText="<i class='fa fa-search'></i>" spellcheck="false" onkeydown="javascript:return handleSearchBoxKeyPress(this, event.keyCode);" />

                <div class="actions margin-t-md">
                    <asp:LinkButton ID="btnApplyFilter" runat="server" CssClass="filter btn btn-action btn-xs" Text="Apply Filter" OnClick="btnApplyFilter_Click" ValidationGroup="vgFilterCriteria" CausesValidation="true" />
                    <asp:LinkButton ID="btnClearFilter" runat="server" CssClass="filter-clear btn btn-default btn-xs" Text="Clear Filter" OnClick="btnClearFilter_Click" CausesValidation="false" />
                </div>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:Grid ID="gAttendees" runat="server" DisplayType="Light" UseFullStylesForLightGrid="true" OnRowDataBound="gAttendees_RowDataBound" DataKeyNames="PersonGuid,AttendanceIds" ShowActionRow="false">
                        <Columns>
                            <Rock:RockLiteralField ID="lPhoto" ItemStyle-CssClass="avatar-column" ColumnPriority="TabletSmall" />
                            <Rock:RockLiteralField ID="lName" HeaderText="Name" HeaderStyle-CssClass="print-first-col" ItemStyle-CssClass="name js-name align-middle print-first-col" />

                            <Rock:RockLiteralField ID="lGroupNameAndPath" HeaderText="Group" Visible="true" />
                            <Rock:RockBoundField DataField="ServiceTimes" HeaderText="Service Times" HeaderStyle-CssClass="d-none d-sm-table-cell" ItemStyle-CssClass="service-times d-none d-sm-table-cell align-middle" />
                            <Rock:RockLiteralField ID="lLocation" HeaderText="Room" Visible="true" HeaderStyle-CssClass="print-last-col" ItemStyle-CssClass="align-middle print-last-col" />

                            <Rock:LinkButtonField ID="btnMovePerson" HeaderStyle-CssClass="d-print-none" ItemStyle-CssClass="grid-columnaction d-print-none" CssClass="btn btn-default btn-square" Text="<i class='fa fa-external-link-alt'></i>" ToolTip="Move Person" OnClick="btnMovePerson_Click" />
                        </Columns>
                    </Rock:Grid>
                </div>
        </asp:Panel>

        <Rock:ModalDialog ID="mdMovePerson" runat="server" Title="Move Person" SaveButtonText="Move" OnSaveClick="mdMovePerson_SaveClick">
            <Content>
                <asp:Panel ID="pnlMovePersonMultipleAttendance" runat="server">
                    <Rock:NotificationBox ID="nbMovePersonInstructions" runat="server" NotificationBoxType="Info" />
                    <Rock:RockDropDownList ID="ddlMovePersonSelectAttendance" runat="server" Label="Attendance" OnSelectedIndexChanged="ddlMovePersonSelectAttendance_SelectedIndexChanged" AutoPostBack="true" />
                </asp:Panel>

                <div class="row">
                    <div class="col-md-4">
                        <Rock:RockDropDownList ID="ddlMovePersonSchedule" runat="server" Label="Schedule" AutoPostBack="true" Required="true" OnSelectedIndexChanged="ddlMovePersonSchedule_SelectedIndexChanged" />
                    </div>
                    <div class="col-md-4">
                        <Rock:RockDropDownList ID="ddlMovePersonLocation" runat="server" Label="Location" AutoPostBack="true" Required="true" OnSelectedIndexChanged="ddlMovePersonLocation_SelectedIndexChanged" />
                    </div>
                    <div class="col-md-4">
                        <Rock:RockDropDownList ID="ddlMovePersonGroup" runat="server" Label="Group" AutoPostBack="true" Required="true" OnSelectedIndexChanged="ddlMovePersonGroup_SelectedIndexChanged" />
                    </div>
                </div>

                <Rock:NotificationBox ID="nbMovePersonLocationFull" runat="server" NotificationBoxType="Warning" />

            </Content>
        </Rock:ModalDialog>

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
</asp:UpdatePanel>
