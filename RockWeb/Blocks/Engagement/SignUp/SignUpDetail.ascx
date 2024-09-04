<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SignUpDetail.ascx.cs" Inherits="RockWeb.Blocks.Engagement.SignUp.SignUpDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%= hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upnlSignUpDetail" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbNotFoundOrArchived" runat="server" NotificationBoxType="Warning" Visible="false" Text="The selected group does not exist or it has been archived." />
        <Rock:NotificationBox ID="nbInvalidGroupType" runat="server" NotificationBoxType="Warning" Visible="false" Text="The selected group is not of a type that can be edited as a sign-up group." />
        <Rock:NotificationBox ID="nbParentNotFound" runat="server" NotificationBoxType="Warning" Visible="false" Text="The selected parent group does not exist or it has been archived." />
        <Rock:NotificationBox ID="nbNotAuthorizedToView" runat="server" NotificationBoxType="Warning" Visible="false" />
        <Rock:NotificationBox ID="nbGroupTypeNotAllowed" runat="server" NotificationBoxType="Warning" Visible="false" />

        <Rock:HiddenFieldWithClass ID="hfTotalOpportunitiesCount" runat="server" CssClass="js-total-opportunities-count" />
        <Rock:HiddenFieldWithClass ID="hfTotalParticipantsCount" runat="server" CssClass="js-total-participants-count" />

        <asp:Panel ID="pnlDetails" CssClass="js-sign-up-detail-panel" runat="server">
            <div class="panel panel-block">

                <div class="panel-heading panel-follow">
                    <h1 class="panel-title pull-left">
                        <asp:Literal ID="lTitle" runat="server" />
                    </h1>

                    <div id="pnlLabelsAndFollow" runat="server">
                        <div class="panel-labels">
                            <Rock:HighlightLabel ID="hlProjectType" runat="server" LabelType="Default" />
                            <Rock:HighlightLabel ID="hlCampus" runat="server" LabelType="Campus" />
                            <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                        </div>

                        <asp:Panel ID="pnlFollowing" runat="server" CssClass="panel-follow-status" data-toggle="tooltip" data-placement="top" title="Click to Follow"></asp:Panel>
                    </div>
                </div>

                <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>

                <div class="panel-body">

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" Visible="false" />
                    <asp:ValidationSummary ID="vsSignUpGroup" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                    <asp:CustomValidator ID="cvGroup" runat="server" Display="Dynamic" ValidationGroup="vg_Group_Custom" />

                    <div id="pnlEditDetails" runat="server">

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="tbName" runat="server" Label="Project Name" Required="true" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                            </div>
                        </div>

                        <Rock:RockTextBox ID="tbDescription" runat="server" Label="Description" TextMode="MultiLine" Rows="4" />

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockDropDownList ID="ddlGroupType" runat="server" DataTextField="Name" DataValueField="Id" Label="Group Type" Required="true" Help="The type of sign-up group." AutoPostBack="true" OnSelectedIndexChanged="ddlGroupType_SelectedIndexChanged" />
                                <Rock:RockLiteral ID="lGroupType" runat="server" Label="Group Type" />
                            </div>
                            <div class="col-md-6">
                                <Rock:CampusPicker ID="cpCampus" runat="server" Label="Campus" />
                            </div>
                        </div>

                        <div id="pnlEditAttributes" runat="server" class="row">
                            <div class="col-md-12">
                                <Rock:AttributeValuesContainer ID="avcEditAttributes" runat="server" ShowCategoryLabel="false" />
                            </div>
                        </div>

                        <div id="pnlProjectType" runat="server" class="row js-project-type">
                            <div class="col-md-12">
                                <Rock:RockRadioButtonList ID="rblProjectType" runat="server" RepeatDirection="Horizontal" Label="Project Type" OnSelectedIndexChanged="rblProjectType_SelectedIndexChanged" AutoPostBack="true" />
                            </div>
                        </div>

                        <Rock:PanelWidget ID="wpCommunication" runat="server" Title="Communications">
                            <div id="pnlReminderCommunication" runat="server" class="row js-reminder-controls" style="display: none;">
                                <div class="col-md-6">
                                    <Rock:RockDropDownList ID="ddlReminderCommunication" runat="server" Label="Reminder Communication Template" Help="The communication template to use when sending reminders." />
                                </div>
                                <div class="col-md-6">
                                    <Rock:NumberBox ID="nbReminderOffsetDays" runat="server" NumberType="Integer" Label="Number of Days Ahead to Send Reminders" Help="The number of days before the event to send reminders." />
                                </div>
                            </div>

                            <div id="pnlReminderAddlDetails" runat="server" class="row js-reminder-controls" style="display: none;">
                                <div class="col-md-12">
                                    <Rock:HtmlEditor ID="htmlReminderAddlDetails" runat="server" Label="Reminder Details" Height="90" Help="Optional additional project details that will be appended to the reminder communication." />
                                </div>
                            </div>

                            <Rock:HtmlEditor ID="htmlConfirmationDetails" runat="server" Label="Confirmation Details" Height="90" Help="Optional additional project details that will be appended to the communication that is sent when registering." />
                        </Rock:PanelWidget>

                        <Rock:PanelWidget ID="wpMemberAttributes" runat="server" Title="Member Attributes">
                            <Rock:NotificationBox ID="nbMemberAttributes" runat="server" NotificationBoxType="Info">
                                Member Attributes apply to all of the members of this project. Each member will have their own value for these attributes.
                                <br />
                                <br />
                                The <strong>Sign-Up Register</strong> block will only show these attributes when the block is operating in "Anonymous" mode.
                            </Rock:NotificationBox>
                            <Rock:RockControlWrapper ID="rcwMemberAttributesInherited" runat="server" Label="Inherited Member Attributes">
                                <div class="grid">
                                    <Rock:Grid ID="gMemberAttributesInherited" runat="server" AllowPaging="false" DisplayType="Light" ShowHeader="false" RowItemText="Inherited Member Attribute">
                                        <Columns>
                                            <Rock:RockBoundField DataField="Name" />
                                            <Rock:RockBoundField DataField="Description" />
                                            <Rock:RockTemplateField>
                                                <ItemTemplate>(Inherited from <a href='<%# Eval("Url") %>' target='_blank' rel='noopener noreferrer'><%# Eval("GroupType") %></a>)</ItemTemplate>
                                            </Rock:RockTemplateField>
                                        </Columns>
                                    </Rock:Grid>
                                </div>
                            </Rock:RockControlWrapper>

                            <Rock:RockControlWrapper ID="rcwMemberAttributes" runat="server" Label="Member Attributes">
                                <div class="grid">
                                    <Rock:Grid ID="gMemberAttributes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Member Attribute" ShowConfirmDeleteDialog="false">
                                        <Columns>
                                            <Rock:ReorderField />
                                            <Rock:RockBoundField DataField="Name" HeaderText="Attribute" />
                                            <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                                            <Rock:BoolField DataField="IsRequired" HeaderText="Required" />
                                            <Rock:SecurityField TitleField="Name" />
                                            <Rock:EditField OnClick="gMemberAttributes_Edit" />
                                            <Rock:DeleteField OnClick="gMemberAttributes_Delete" />
                                        </Columns>
                                    </Rock:Grid>
                                </div>
                            </Rock:RockControlWrapper>
                        </Rock:PanelWidget>

                        <Rock:PanelWidget ID="wpMemberOpportunityAttributes" runat="server" Title="Member Opportunity Attributes">
                            <Rock:NotificationBox ID="nbMemberOpportunityAttributes" runat="server" NotificationBoxType="Info">
                                Member Opportunity Attributes apply to all of the members in every opportunity of this project. Each member will have their own value for these attributes, for each opportunity.
                                <br />
                                <br />
                                The <strong>Sign-Up Register</strong> block will only show these attributes when the block is operating in "Anonymous" mode.
                            </Rock:NotificationBox>
                            <div class="grid">
                                <Rock:Grid ID="gMemberOpportunityAttributes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Member Opportunity Attribute" ShowConfirmDeleteDialog="false">
                                    <Columns>
                                        <Rock:ReorderField />
                                        <Rock:RockBoundField DataField="Name" HeaderText="Attribute" />
                                        <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                                        <Rock:BoolField DataField="IsRequired" HeaderText="Required" />
                                        <Rock:SecurityField TitleField="Name" />
                                        <Rock:EditField OnClick="gMemberOpportunityAttributes_Edit" />
                                        <Rock:DeleteField OnClick="gMemberOpportunityAttributes_Delete" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </Rock:PanelWidget>

                        <Rock:PanelWidget ID="wpGroupRequirements" runat="server" Title="Group Requirements">
                            <div runat="server" class="row">
                                <div class="col-md-12">
                                    <Rock:RockControlWrapper ID="rcwGroupTypeGroupRequirements" runat="server" Label="Group Requirements for Group Type">
                                        <asp:Literal ID="lGroupTypeGroupRequirements" runat="server" Text="(From ...)" />
                                        <div class="grid">
                                            <Rock:Grid ID="gGroupTypeGroupRequirements" runat="server" AllowPaging="false" DisplayType="Light" ShowHeader="true" RowItemText="Group Requirements for Group Type">
                                                <Columns>
                                                    <Rock:RockBoundField DataField="GroupRequirementType.Name" HeaderText="Name" />
                                                </Columns>
                                            </Rock:Grid>
                                        </div>
                                    </Rock:RockControlWrapper>

                                    <Rock:RockControlWrapper ID="rcwGroupRequirements" runat="server" Label="Specific Group Requirements">
                                        <div calss="grid">
                                            <Rock:Grid ID="gGroupRequirements" runat="server" DataKeyNames="Guid" AllowPaging="false" DisplayType="Light" RowItemText="Group Requirement" ShowConfirmDeleteDialog="false" OnGridRebind="gGroupRequirements_GridRebind">
                                                <Columns>
                                                    <Rock:RockBoundField DataField="GroupRequirementType.Name" HeaderText="Name" />
                                                    <Rock:RockBoundField DataField="GroupRole" HeaderText="Group Role" />
                                                    <Rock:RockBoundField DataField="AppliesToAgeClassification" HeaderText="Age Classification" />
                                                    <Rock:RockLiteralField ID="lAppliesToDataViewId" runat="server" ItemStyle-HorizontalAlign="Center" HeaderText="Data View" OnDataBound="lAppliesToDataViewId_OnDataBound" />
                                                    <Rock:BoolField DataField="MustMeetRequirementToAddMember" HeaderText="Required For New Members" />
                                                    <Rock:BoolField DataField="GroupRequirementType.CanExpire" HeaderText="Can Expire" />
                                                    <Rock:EnumField DataField="GroupRequirementType.RequirementCheckType" HeaderText="Type" />
                                                    <Rock:EditField OnClick="gGroupRequirements_Edit" />
                                                    <Rock:DeleteField OnClick="gGroupRequirements_Delete" />
                                                </Columns>
                                            </Rock:Grid>
                                        </div>
                                    </Rock:RockControlWrapper>
                                </div>
                            </div>
                        </Rock:PanelWidget>

                        <div class="actions">
                            <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                            <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                        </div>

                    </div>

                    <div id="pnlViewDetails" runat="server">

                        <div id="pnlDescription" runat="server" class="row">
                            <div class="col-md-12">
                                <Rock:RockControlWrapper ID="rcwViewDescription" runat="server" Label="Description">
                                    <asp:Literal ID="lDescription" runat="server" />
                                </Rock:RockControlWrapper>
                            </div>
                        </div>

                        <div id="pnlDisplayAttributes" runat="server" class="row">
                            <div class="col-md-12">
                                <Rock:AttributeValuesContainer ID="avcDisplayAttributes" runat="server" ShowCategoryLabel="false" />
                            </div>
                        </div>

                        <div id="pnlViewGroupRequirements" runat="server" class="row">
                            <div class="col-md-12">
                                <Rock:RockControlWrapper ID="rcwRequirementsList" runat="server" Label="Group Requirements">
                                    <ul>
                                        <asp:Repeater ID="rptRequirementsList" runat="server" OnItemDataBound="rptRequirementsList_ItemDataBound">
                                            <ItemTemplate>
                                                <li id="liRequirement" runat="server">
                                                    <asp:Literal ID="lRequirement" runat="server" />
                                                    <Rock:Badge ID="bRequiresLogin" runat="server" BadgeType="warning">Requires Login to Register</Rock:Badge>
                                                </li>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </ul>
                                </Rock:RockControlWrapper>
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-12">
                                <h3>Opportunities</h3>
                                <div class="d-flex flex-wrap justify-content-between align-items-center border-bottom border-panel">
                                    <p>
                                        <asp:Literal ID="lOpportunitiesDescription" runat="server">Below are opportunities for the sign-up based on location and schedule.</asp:Literal>
                                    </p>
                                    <div class="form-group">
                                        <Rock:ButtonGroup ID="bgOpportunitiesTimeframe" runat="server" FormGroupCssClass="toggle-container" SelectedItemClass="btn btn-xs btn-primary active" UnselectedItemClass="btn btn-xs btn-default" AutoPostBack="true" OnSelectedIndexChanged="bgOpportunitiesTimeframe_SelectedIndexChanged">
                                            <asp:ListItem Text="Upcoming" Value="1" Selected="True" />
                                            <asp:ListItem Text="Past" Value="2" />
                                        </Rock:ButtonGroup>
                                    </div>
                                </div>

                                <Rock:NotificationBox ID="nbNoAllowedScheduleTypes" runat="server" Visible="false" NotificationBoxType="Warning" />
                                <Rock:NotificationBox ID="nbNoAllowedLocationPickerModes" runat="server" Visible="false" NotificationBoxType="Warning" />

                                <div class="grid">
                                    <Rock:Grid ID="gOpportunities" runat="server" DataKeyNames="GroupId,GroupLocationId,LocationId,ScheduleId" AllowPaging="false" DisplayType="Light" CssClass="js-grid-opportunities" RowItemText="Opportunity" OnDataBinding="gOpportunities_DataBinding" OnRowDataBound="gOpportunities_RowDataBound" OnGridRebind="gOpportunities_GridRebind">
                                        <Columns>
                                            <Rock:RockBoundField DataField="Name" HeaderText="Opportunity Name" />
                                            <Rock:RockBoundField DataField="FriendlyDateTime" HeaderText="Date/Time" />
                                            <Rock:RockBoundField DataField="FriendlyLocation" HeaderText="Location" />
                                            <Rock:RockBoundField DataField="ProgressBar" HeaderText="Sign-Ups" ItemStyle-CssClass="progress-sign-ups-cell align-middle" HtmlEncode="false" />
                                            <Rock:LinkButtonField ID="lbOpportunityDetail" Text="<i class='fa fa-users'></i>" ToolTip="Attendee List" CssClass="btn btn-default btn-sm btn-square" OnClick="lbOpportunityDetail_Click" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                                            <Rock:EditField ID="efOpportunities" OnClick="gOpportunities_Edit" />
                                            <Rock:DeleteField ID="dfOpportunities" OnClick="gOpportunities_Delete" />
                                        </Columns>
                                    </Rock:Grid>
                                </div>
                            </div>
                        </div>

                        <div id="pnlActions" runat="server" class="actions">
                            <asp:LinkButton ID="btnEdit" runat="server" data-shortcut-key="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                            <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                            <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" CausesValidation="false" />
                            <span class="pull-right">
                                <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-square btn-security" Title="Secure Group" />
                            </span>
                        </div>

                    </div>
                </div>
            </div>
        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <!-- Member Attribute Modal Dialog -->
        <Rock:ModalDialog ID="mdMemberAttribute" runat="server" Title="Member Attribute" OnSaveClick="mdMemberAttribute_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="vg_MemberAttribute">
            <Content>
                <Rock:AttributeEditor ID="edtMemberAttribute" runat="server" ShowActions="false" ValidationGroup="vg_MemberAttribute" />
            </Content>
        </Rock:ModalDialog>

        <!-- Member Opportunity Attribute Modal Dialog -->
        <Rock:ModalDialog ID="mdMemberOpportunityAttribute" runat="server" Title="Member Opportunity Attribute" OnSaveClick="mdMemberOpportunityAttribute_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="vg_MemberOpportunityAttribute">
            <Content>
                <Rock:AttributeEditor ID="edtMemberOpportunityAttribute" runat="server" ShowActions="false" ValidationGroup="vg_MemberOpportunityAttribute" />
            </Content>
        </Rock:ModalDialog>

        <!-- Group Requirements Modal Dialog -->
        <Rock:ModalDialog ID="mdGroupRequirement" runat="server" Title="Group Requirement" SaveButtonText="Ok" OnSaveClick="mdGroupRequirement_OkClick" OnCancelScript="clearActiveDialog();" ValidationGroup="vg_GroupRequirement">
            <Content>
                <asp:HiddenField ID="hfGroupRequirementGuid" runat="server" />

                <Rock:NotificationBox ID="nbDuplicateGroupRequirement" runat="server" NotificationBoxType="Warning" />

                <asp:ValidationSummary ID="vsGroupRequirement" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="vg_GroupRequirement" />

                <Rock:RockDropDownList ID="ddlGroupRequirementType" runat="server" Label="Group Requirement Type" Required="true" AutoPostBack="true" OnSelectedIndexChanged="ddlGroupRequirementType_SelectedIndexChanged" ValidationGroup="vg_GroupRequirement" />

                <Rock:GroupRolePicker ID="grpGroupRequirementGroupRole" runat="server" Label="Applies to Group Role" Help="Select the group role that this requirement applies to. Leave blank if it applies to all group roles." ValidationGroup="vg_GroupRequirement" />

                <Rock:RockRadioButtonList ID="rblAppliesToAgeClassification" runat="server" Label="Applies to Age Classification" RepeatDirection="Horizontal" Help="Determines which age classifications this requirement applies to."></Rock:RockRadioButtonList>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataViewItemPicker ID="dvpAppliesToDataView" runat="server" Label="Applies to Data View" EntityTypeId="15" Help="An optional data view to determine who the requirement applies to." />
                    </div>
                </div>

                <Rock:RockDropDownList ID="ddlDueDateGroupAttribute" runat="server" Label="Due Date Group Attribute" Help="The group attribute that contains the due date for requirements." ValidationGroup="vg_GroupRequirement" />
                <Rock:DatePicker ID="dpDueDate" runat="server" Label="Due Date" ValidationGroup="vg_GroupRequirement" />

                <Rock:RockCheckBox ID="cbAllowLeadersToOverride" runat="server" Text="Allow Leaders to Override" Help="Determines if the leader should be allowed to override meeting the requirement." />

                <Rock:RockCheckBox ID="cbMembersMustMeetRequirementOnAdd" runat="server" Text="Members must meet this requirement before adding" Help="If this is enabled, a person can only become a group member if this requirement is met. Note: only applies to Data View and SQL type requirements since manual ones can't be checked until after the person is added." />
            </Content>
        </Rock:ModalDialog>

        <!-- Add Opportunity Modal Dialog -->
        <Rock:ModalDialog ID="mdAddOpportunity" runat="server" Title="Add Opportunity" OnSaveClick="mdAddOpportunity_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="vg_AddOpportunity">
            <Content>
                <asp:ValidationSummary ID="vsOpportunity" runat="server" ValidationGroup="vg_AddOpportunity" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <asp:CustomValidator ID="cvOpportunity" runat="server" Display="Dynamic" ValidationGroup="vg_AddOpportunity_Custom" />

                <Rock:NotificationBox ID="nbOpportunityAlreadyExists" runat="server" NotificationBoxType="Info" />
                <Rock:NotificationBox ID="nbScheduleTypeNotAllowed" runat="server" NotificationBoxType="Warning" />
                <Rock:NotificationBox ID="nbLocationModeNotAllowed" runat="server" NotificationBoxType="Warning" />

                <Rock:RockTextBox ID="tbOpportunityName" runat="server" Label="Opportunity Name" Help="Optional name for the opportunity. This is helpful to provide if your project will have several opportunities configured." />

                <div class="row">
                    <div class="col-md-8">
                        <div id="pnlScheduleTypeButtonGroup" runat="server" class="form-group">
                            <Rock:ButtonGroup ID="bgScheduleType" runat="server" FormGroupCssClass="toggle-container" SelectedItemClass="btn btn-xs btn-primary active" UnselectedItemClass="btn btn-xs btn-default" AutoPostBack="true" OnSelectedIndexChanged="bgScheduleType_SelectedIndexChanged">
                                <asp:ListItem Text="Custom Schedule" Value="2" Selected="True" />
                                <asp:ListItem Text="Named Schedule" Value="4" />
                            </Rock:ButtonGroup>
                        </div>
                        <Rock:RockControlWrapper ID="rcwScheduleBuilder" runat="server" Label="Schedule" Required="true" ValidationGroup="vg_AddOpportunity_Custom">
                            <Rock:ScheduleBuilder ID="sbSchedule" runat="server" OnSaveSchedule="sbSchedule_SaveSchedule" />
                            <asp:Literal ID="lScheduleText" runat="server" />
                        </Rock:RockControlWrapper>
                        <Rock:SchedulePicker ID="spSchedule" runat="server" AllowMultiSelect="false" Label="Schedule" Required="true" OnSelectItem="spSchedule_SelectItem" ValidationGroup="vg_AddOpportunity_Custom" />

                        <Rock:LocationPicker ID="lpLocation" runat="server" Label="Location" Required="true" OnSelectLocation="lpLocation_SelectLocation" ValidationGroup="vg_AddOpportunity_Custom" />

                    </div>
                    <div class="col-md-4">
                        <div class="well">
                            <Rock:RockTextBox ID="tbMinimumAttendance" runat="server" Label="Minimum Attendance" />
                            <Rock:RockTextBox ID="tbDesiredAttendance" runat="server" Label="Desired Attendance" />
                            <Rock:RockTextBox ID="tbMaximumAttendance" runat="server" Label="Maximum Attendance" />
                        </div>
                    </div>
                </div>

                <Rock:HtmlEditor ID="htmlOpportunityReminderAddlDetails" runat="server" Label="Reminder Details" Height="90" Help="Optional additional project details that will be appended to the reminder communication." />
                <Rock:HtmlEditor ID="htmlOpportunityConfirmationDetails" runat="server" Label="Confirmation Details" Height="90" Help="Optional additional project details that will be appended to the communication that is sent when registering." />
            </Content>
        </Rock:ModalDialog>

        <script>

            Sys.Application.add_load(function () {
                var $thisBlock = $('#<%= upnlSignUpDetail.ClientID %>');

                function toggleReminderControlsVisibility(projectTypeGuid) {
                    var projectTypeGuid = projectTypeGuid || $thisBlock.find('.js-project-type input[checked]').first().val();

                    if (!projectTypeGuid) {
                        return;
                    }

                    var isInPersonProjectType = !!(projectTypeGuid && projectTypeGuid.toUpperCase() === 'FF3F0C5C-9775-4A09-9CCF-94902DB99BF6');
                    var $reminderControls = $thisBlock.find('.js-reminder-controls');
                    if (isInPersonProjectType) {
                        $reminderControls.show();
                    } else {
                        $reminderControls.hide();
                    }
                }

                $thisBlock.find('.js-progress-sign-ups').each(function () {
                    var $progress = $(this);
                    $progress.tooltip({
                        html: true
                    });
                });

                $thisBlock.find('.js-project-type .rockradiobuttonlist > label').on('click', function () {
                    var projectTypeGuid = $(this).find('input').first().val();
                    if (!projectTypeGuid) {
                        projectTypeGuid = 'none';
                    }
                    toggleReminderControlsVisibility(projectTypeGuid);
                });

                toggleReminderControlsVisibility();

                var participantLabel;

                // project delete prompt
                $('#<%= btnDelete.ClientID %>').on('click', function (e) {
                    var projectOpportunityConfirmMessage = 'Are you sure you want to delete this Project?';

                    var totalOpportunitiesCount = parseInt($thisBlock.find('.js-total-opportunities-count').val());
                    if (totalOpportunitiesCount > 0) {
                        var opportunityLabel = totalOpportunitiesCount > 1
                            ? 'opportunities'
                            : 'opportunity';

                        projectOpportunityConfirmMessage = 'This Project has ' + totalOpportunitiesCount.toLocaleString('en-US') + ' ' + opportunityLabel;
                        var projectOpportunityMessageSuffix = ' and remove all opportunities?';

                        var totalParticipantsCount = parseInt($thisBlock.find('.js-total-participants-count').val());
                        if (totalParticipantsCount > 0) {
                            participantLabel = totalParticipantsCount > 1
                                ? 'participants'
                                : 'participant';

                            projectOpportunityConfirmMessage += ' and ' + totalParticipantsCount.toLocaleString('en-US') + ' ' + participantLabel + '.';
                            projectOpportunityMessageSuffix = ', removing all opportunities and participants?'
                        }
                        else {
                            projectOpportunityConfirmMessage += '.';
                        }

                        projectOpportunityConfirmMessage += ' Are you sure you want to delete this Project' + projectOpportunityMessageSuffix;
                    }

                    e.preventDefault();
                    Rock.dialogs.confirm(projectOpportunityConfirmMessage, function (result) {
                        if (result) {
                            window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                        }
                    });
                });

                // opportunity delete prompt
                $thisBlock.find('table.js-grid-opportunities a.grid-delete-button').on('click', function (e) {
                    var $btn = $(this);
                    var $row = $btn.closest('tr');

                    var opportunityConfirmMessage = 'Are you sure you want to delete this Opportunity?';

                    if ($row.hasClass('js-has-participants')) {
                        var $progressBar = $row.find('.js-progress-sign-ups');
                        var participantCount = parseInt($progressBar.attr('data-slots-filled'));
                        participantLabel = participantCount > 1
                            ? 'participants'
                            : 'participant';

                        opportunityConfirmMessage = 'This Opportunity has ' + participantCount.toLocaleString('en-US') + ' ' + participantLabel + '. Are you sure you want to delete this Opportunity and remove all participants? ';
                    }

                    e.preventDefault();
                    Rock.dialogs.confirm(opportunityConfirmMessage, function (result) {
                        if (result) {
                            window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                        }
                    });
                });
            });

        </script>

    </ContentTemplate>
</asp:UpdatePanel>
