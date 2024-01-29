<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SignUpOverview.ascx.cs" Inherits="RockWeb.Blocks.Engagement.SignUp.SignUpOverview" %>

<asp:UpdatePanel ID="upnlSignUpOverview" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdSignUpOverview" runat="server" />

        <Rock:NotificationBox ID="nbNotAuthorizedToDelete" runat="server" NotificationBoxType="Warning" Visible="false" Text="You are not authorized to delete this Sign-Up Opportunity." />

        <asp:Panel ID="pnlDetails" runat="server">
            <asp:HiddenField ID="hfAction" runat="server" />
            <div class="panel panel-block">

                <div class="panel-heading">
                    <h1 class="panel-title pull-left">Sign-Up Overview</h1>
                </div>

                <div class="panel-body">
                    <div class="grid grid-panel">
                        <Rock:GridFilter ID="gfOpportunities" runat="server" OnDisplayFilterValue="gfOpportunities_DisplayFilterValue" OnApplyFilterClick="gfOpportunities_ApplyFilterClick" OnClearFilterClick="gfOpportunities_ClearFilterClick" FieldLayout="Custom">
                            <div class="d-flex justify-content-between flex-wrap">
                                <Rock:SlidingDateRangePicker ID="sdrpDateRange" runat="server" Label="Schedule Date Range" />
                                <Rock:GroupPicker ID="gpParentGroup" runat="server" Label="Parent Group" />
                            </div>
                            <Rock:RockControlWrapper ID="rcwSlotsAvailableFilter" runat="server" Label="Slots Available">
                                <div class="form-control-group key-value-rows">
                                    <Rock:RockDropDownList ID="ddlSlotsAvailableComparisonType" runat="server" CssClass="input-width-lg controls-row" OnTextChanged="ddlSlotsAvailableComparisonType_TextChanged" AutoPostBack="true" />
                                    <Rock:NumberBox ID="nbSlotsAvailableFilterCompareValue" runat="server" CssClass="input-width-sm controls-row" />
                                </div>
                            </Rock:RockControlWrapper>
                        </Rock:GridFilter>
                        <Rock:Grid ID="gOpportunities" runat="server" DataKeyNames="Guid,Id,GroupId,LocationId,ScheduleId" DisplayType="Full" AllowSorting="true" CssClass="js-grid-opportunities" RowItemText="Opportunity" OnDataBinding="gOpportunities_DataBinding" OnRowDataBound="gOpportunities_RowDataBound" OnGridRebind="gOpportunities_GridRebind" OnRowSelected="gOpportunities_RowSelected" ExportSource="ColumnOutput" ShowConfirmDeleteDialog="true">
                            <Columns>
                                <Rock:SelectField />
                                <Rock:RockBoundField DataField="ProjectName" HeaderText="Project Name" SortExpression="ProjectName" ExcelExportBehavior="AlwaysInclude" />
                                <Rock:RockBoundField DataField="FriendlySchedule" HeaderText="Schedule" SortExpression="NextOrLastStartDateTime" ExcelExportBehavior="AlwaysInclude" />
                                <Rock:RockBoundField DataField="LeaderCount" HeaderText="Leader Count" SortExpression="LeaderCount" ExcelExportBehavior="AlwaysInclude" />
                                <Rock:RockLiteralField ID="lParticipantCountBadgeHtml" HeaderText="Participant Count" SortExpression="ParticipantCount" ExcelExportBehavior="NeverInclude" />
                                <Rock:LinkButtonField ID="lbOpportunityDetail" Text="<i class='fa fa-users'></i>" ToolTip="Attendee List" CssClass="btn btn-default btn-sm btn-square" OnClick="lbOpportunityDetail_Click" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                                <Rock:DeleteField ID="dfOpportunities" OnClick="dfOpportunities_Click" />

                                <%-- Fields that are only shown when exporting --%>
                                <Rock:RockBoundField DataField="ParticipantCount" HeaderText="Participant Count" Visible="False" ExcelExportBehavior="AlwaysInclude" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>

            </div>
        </asp:Panel>

        <script>

            Sys.Application.add_load(function () {
                var $thisBlock = $('#<%= upnlSignUpOverview.ClientID %>');

                // Get all delete buttons.
                var $deleteButtons = $thisBlock.find('table.js-grid-opportunities a.grid-delete-button');

                // Disable any buttons whose rows indicate deleting is not allowed.
                $deleteButtons.each(function () {
                    var $btn = $(this);
                    var $row = $btn.closest('tr');

                    if ($row.hasClass('js-cannot-delete')) {
                        $btn.addClass('disabled');
                    }
                });

                // Custom delete prompt.
                $deleteButtons.on('click', function (e) {
                    var $btn = $(this);
                    var $row = $btn.closest('tr');

                    var confirmMessage = 'Are you sure you want to delete this Opportunity?';

                    if ($row.hasClass('js-has-participants')) {
                        var participantCount = parseInt($row.find('.participant-count-badge').html());
                        var participantLabel = participantCount > 1
                            ? 'participants'
                            : 'participant';

                        confirmMessage = 'This Opportunity has ' + participantCount.toLocaleString('en-US') + ' ' + participantLabel + '. Are you sure you want to delete this Opportunity and remove all participants? ';
                    }

                    e.preventDefault();
                    Rock.dialogs.confirm(confirmMessage, function (result) {
                        if (result) {
                            window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                        }
                    });
                });

            });

        </script>

    </ContentTemplate>
</asp:UpdatePanel>
