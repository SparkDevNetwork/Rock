<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SignUpOpportunityAttendeeList.ascx.cs" Inherits="RockWeb.Blocks.Engagement.SignUp.SignUpOpportunityAttendeeList" %>

<asp:UpdatePanel ID="upnlSignUpOpportunityAttendeeList" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbMissingIds" runat="server" NotificationBoxType="Warning" />
        <Rock:NotificationBox ID="nbNotFoundOrArchived" runat="server" NotificationBoxType="Warning" Visible="false" Text="The selected group does not exist or it has been archived." />
        <Rock:NotificationBox ID="nbNotAuthorizedToView" runat="server" NotificationBoxType="Warning" />
        <Rock:NotificationBox ID="nbInvalidGroupType" runat="server" NotificationBoxType="Warning" Visible="false" Text="The selected group is not of a type that can be edited as a sign-up group." />
        <Rock:NotificationBox ID="nbOpportunityNotFound" runat="server" NotificationBoxType="Warning" Text="The selected sign-up opportunity does not exist." />
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <asp:Panel ID="pnlDetails" runat="server">
            <div class="panel panel-block">

                <div class="panel-heading">
                    <h1 class="panel-title pull-left">
                        <asp:Literal ID="lTitle" runat="server" />
                    </h1>

                    <div class="panel-labels">
                        <Rock:HighlightLabel ID="hlGroupType" runat="server" LabelType="Default" />
                        <Rock:HighlightLabel ID="hlCampus" runat="server" LabelType="Campus" />
                        <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                    </div>
                </div>

                <div class="panel-body">

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockControlWrapper ID="rcwLocation" runat="server" Label="Location">
                                <asp:Literal ID="lLocation" runat="server" />
                            </Rock:RockControlWrapper>
                        </div>
                        <div class="col-md-6">
                            <Rock:RockControlWrapper ID="rcwSchedule" runat="server" Label="Schedule">
                                <asp:Literal ID="lSchedule" runat="server" />
                            </Rock:RockControlWrapper>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockControlWrapper ID="rcwConfiguredSlots" runat="server" Label="Configured Slots">
                                <asp:Literal ID="lConfiguredSlots" runat="server" />
                            </Rock:RockControlWrapper>
                        </div>
                        <div class="col-md-6">
                            <Rock:RockControlWrapper ID="rcwSlotsFilled" runat="server" Label="Slots Filled">
                                <Rock:Badge ID="bSlotsFilled" runat="server" BadgeType="W" />
                            </Rock:RockControlWrapper>
                        </div>
                    </div>

                </div>

            </div>

            <div class="panel panel-block">

                <div class="panel-heading">
                    <h1 class="panel-title pull-left">Attendees</h1>
                </div>

                <div class="panel-body">
                    <div class="grid grid-panel">
                        <Rock:GridFilter ID="gfAttendees" runat="server" OnDisplayFilterValue="gfAttendees_DisplayFilterValue" OnApplyFilterClick="gfAttendees_ApplyFilterClick" OnClearFilterClick="gfAttendees_ClearFilterClick">
                            <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" />
                            <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" />
                            <Rock:RockCheckBoxList ID="cblRole" runat="server" Label="Role" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal" />
                            <Rock:RockCheckBoxList ID="cblGroupMemberStatus" runat="server" Label="Group Member Status" RepeatDirection="Horizontal" />
                            <Rock:CampusPicker ID="cpCampusFilter" runat="server" Label="Family Campus" />
                            <Rock:RockCheckBoxList ID="cblGenderFilter" runat="server" RepeatDirection="Horizontal" Label="Gender">
                                <asp:ListItem Text="Male" Value="Male" />
                                <asp:ListItem Text="Female" Value="Female" />
                                <asp:ListItem Text="Unknown" Value="Unknown" />
                            </Rock:RockCheckBoxList>
                            <asp:PlaceHolder ID="phAttributeFilters" runat="server" />
                        </Rock:GridFilter>
                        <Rock:Grid ID="gAttendees" runat="server" DataKeyNames="Id" DisplayType="Full" AllowSorting="true" CssClass="js-grid-members" RowItemText="Attendee" OnRowDataBound="gAttendees_RowDataBound" OnGridRebind="gAttendees_GridRebind" OnRowSelected="gAttendees_RowSelected" ExportSource="ColumnOutput">
                            <Columns>
                                <Rock:SelectField></Rock:SelectField>
                                <Rock:RockLiteralField ID="lExportFullName" HeaderText="Name" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                <Rock:RockLiteralField ID="lNameWithHtml" HeaderText="Name" SortExpression="GroupMember.Person.LastName,GroupMember.Person.NickName" ExcelExportBehavior="NeverInclude" />
                                <Rock:RockBoundField DataField="GroupMember.GroupRole.Name" HeaderText="Role" SortExpression="GroupMember.GroupRole.Name" />
                                <Rock:RockBoundField DataField="GroupMember.GroupMemberStatus" HeaderText="Member Status" SortExpression="GroupMember.GroupMemberStatus" />

                                <%-- Fields that are only shown when exporting --%>
                                <Rock:RockBoundField DataField="GroupMember.Note" HeaderText="Note" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                <Rock:RockBoundField DataField="GroupMember.Person.NickName" HeaderText="Nick Name" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                <Rock:RockBoundField DataField="GroupMember.Person.LastName" HeaderText="Last Name" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                <Rock:RockBoundField DataField="GroupMember.Person.BirthDate" HeaderText="Birth Date" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                <Rock:RockBoundField DataField="GroupMember.Person.Age" HeaderText="Age" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                <Rock:RockBoundField DataField="GroupMember.Person.Email" HeaderText="Email" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                <Rock:RockBoundField DataField="GroupMember.Person.RecordStatusValueId" HeaderText="RecordStatusValueId" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                <Rock:DefinedValueField DataField="GroupMember.Person.RecordStatusValueId" HeaderText="Record Status" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                <Rock:RockBoundField DataField="GroupMember.Person.Gender" HeaderText="Gender" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                <Rock:RockBoundField DataField="GroupMember.Person.IsDeceased" HeaderText="Is Deceased" Visible="false" ExcelExportBehavior="AlwaysInclude" />

                                <Rock:RockLiteralField ID="lExportHomePhone" HeaderText="Home Phone" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                <Rock:RockLiteralField ID="lExportCellPhone" HeaderText="Cell Phone" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                <Rock:RockLiteralField ID="lExportHomeAddress" HeaderText="Home Address" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                <Rock:RockLiteralField ID="lExportLatitude" HeaderText="Latitude" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                <Rock:RockLiteralField ID="lExportLongitude" HeaderText="Longitude" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>

            </div>

        </asp:Panel>

        <script>

            Sys.Application.add_load(function () {
                var $thisBlock = $('#<%= upnlSignUpOpportunityAttendeeList.ClientID %>');

                $thisBlock.find('div.photo-icon').lazyload({
                    effect: 'fadeIn'
                });

                // person-link-popover
                $thisBlock.find('.js-person-popover').popover({
                    placement: 'right',
                    trigger: 'manual',
                    delay: 500,
                    html: true,
                    content: function () {
                        var dataUrl = Rock.settings.get('baseUrl') + 'api/People/PopupHtml/' + $(this).attr('personid') + '/false';

                        var result = $.ajax({
                            type: 'GET',
                            url: dataUrl,
                            dataType: 'json',
                            contentType: 'application/json; charset=utf-8',
                            async: false
                        }).responseText;

                        var resultObject = JSON.parse(result);

                        return resultObject.PickerItemDetailsHtml;

                    }
                }).on('mouseenter', function () {
                    var _this = this;
                    $(this).popover('show');
                    $(this).siblings('.popover').on('mouseleave', function () {
                        $(_this).popover('hide');
                    });
                }).on('mouseleave', function () {
                    var _this = this;
                    setTimeout(function () {
                        if (!$thisBlock.find('.popover:hover').length) {
                            $(_this).popover('hide')
                        }
                    }, 100);
                });

                // delete/archive prompt
                $thisBlock.find('table.js-grid-members a.grid-delete-button').on('click', function (e) {
                    var $btn = $(this);
                    var $row = $btn.closest('tr');
                    var actionName = 'delete';

                    if ($row.hasClass('js-has-grouphistory')) {
                        var actionName = 'archive';
                    }

                    var confirmMessage = 'Are you sure you want to ' + actionName + ' this group member?';

                    e.preventDefault();
                    Rock.dialogs.confirm(confirmMessage, function (result) {
                        if (result) {
                            window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                        }
                    });
                });

                // initialize any unmet requirements tooltips
                $thisBlock.find('.unmet-group-requirements').each(function () {
                    var $requirements = $(this);
                    var tooltip = $('#' + $requirements.data('tip')).html();
                    $requirements.tooltip({
                        html: true,
                        title: tooltip
                    });
                });
            });

        </script>

    </ContentTemplate>
</asp:UpdatePanel>
