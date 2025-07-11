﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupDetail.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }

    Sys.Application.add_load(function () {
        $('.js-follow-status').tooltip();
    });
</script>

<asp:UpdatePanel ID="upnlGroupDetail" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbNotFoundOrArchived" runat="server" NotificationBoxType="Warning" Visible="false" Text="That group does not exist or it has been archived." />

        <asp:Panel ID="pnlDetails" CssClass="js-group-panel" runat="server">
            <asp:HiddenField ID="hfGroupId" runat="server" />

            <div class="panel panel-block">

                <div class="panel-heading panel-follow">
                    <h1 class="panel-title pull-left">
                        <asp:Literal ID="lGroupIconHtml" runat="server" />
                        <asp:Literal ID="lReadOnlyTitle" runat="server" />
                    </h1>

                    <div class="panel-labels">
                        <Rock:HighlightLabel ID="hlInactive" runat="server" CssClass="js-inactivegroup-label" LabelType="Danger" Text="Inactive" />
                        <Rock:HighlightLabel ID="hlArchived" runat="server" CssClass="js-archivedgroup-label" LabelType="Danger" Text="Archived" />
                        <Rock:HighlightLabel ID="hlIsPrivate" runat="server" CssClass="js-privategroup-label" LabelType="Default" Text="Private" />
                        <Rock:HighlightLabel ID="hlElevatedSecurityLevel" runat="server" LabelType="Warning" Visible="false" />
                        <Rock:HighlightLabel ID="hlPeerNetwork" runat="server" LabelType="Info" Visible="false" />
                        <Rock:HighlightLabel ID="hlChat" runat="server" LabelType="Info" Visible="false" />
                        <Rock:HighlightLabel ID="hlType" runat="server" LabelType="Type" />
                        <Rock:HighlightLabel ID="hlCampus" runat="server" LabelType="Campus" />
                    </div>

                    <asp:Panel runat="server" ID="pnlFollowing" CssClass="panel-follow-status js-follow-status" data-toggle="tooltip" data-placement="top" title="Click to Follow"></asp:Panel>

                </div>

                <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>

                <div class="panel-badges" id="divBadgeContainer" runat="server">
                    <Rock:BadgeListControl ID="blBadgeList" runat="server" />
                </div>

                <div class="panel-body">
                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    <Rock:NotificationBox ID="nbRoleLimitWarning" runat="server" NotificationBoxType="Warning" Heading="Role Limit Warning" />
                    <Rock:NotificationBox ID="nbNotAllowedToEdit" runat="server" NotificationBoxType="Danger" Visible="false"
                        Text="You are not authorized to save group with the selected group type and/or parent group." />
                    <Rock:NotificationBox ID="nbInvalidParentGroup" runat="server" NotificationBoxType="Danger" Visible="false"
                        Text="The selected parent group does not allow child groups of the selected group type." />
                    <Rock:NotificationBox ID="nbGroupCapacityMessage" runat="server" NotificationBoxType="Warning" Visible="false" />
                    <asp:ValidationSummary ID="vsGroup" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                    <asp:CustomValidator ID="cvGroup" runat="server" Display="None" />

                    <div id="pnlEditDetails" runat="server">
                        <div class="row" style="display: flex; align-items: center;">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.Group, Rock" PropertyName="Name" />
                            </div>
                            <div class="col-md-3">
                                <Rock:RockCheckBox ID="cbIsActive" runat="server" CssClass="js-isactivegroup" Label="Active" />
                            </div>
                            <div class="col-md-3">
                                <Rock:RockCheckBox ID="cbIsPublic" runat="server" CssClass="js-ispublicgroup" Label="Public" />
                            </div>
                        </div>

                        <div class="row js-inactivateoptions">
                            <div class="col-md-6 pull-right">
                                <%-- Inactive Reason ddl this isn't a defined value picker since the values can be filtered by group type --%>
                                <Rock:RockDropDownList ID="ddlInactiveReason" runat="server" Visible="false" Label="Inactive Reason" />
                            </div>
                        </div>

                        <div class="row js-inactivateoptions">
                            <div class="col-md-6 pull-right">
                                <%-- Inactive note multi line --%>
                                <Rock:DataTextBox ID="tbInactiveNote" runat="server" SourceTypeName="Rock.Model.Group, Rock" PropertyName="InactiveReasonNote" TextMode="MultiLine" Rows="4" Visible="false" Label="Inactive Note" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6 pull-right">
                                <%-- Inactivate child groups checkbox --%>
                                <Rock:RockCheckBox ID="cbInactivateChildGroups" runat="server" Text="Inactivate Child Groups" ContainerCssClass="js-inactivatechildgroups" Style="display: none" />
                                <Rock:HiddenFieldWithClass ID="hfHasChildGroups" runat="server" CssClass="js-haschildgroups" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-12">
                                <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.Group, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                            </div>
                        </div>
                        <Rock:PanelWidget ID="wpGeneral" runat="server" Title="General">
                            <div class="row">
                                <div class="col-md-6">
                                    <div class="row">
                                        <div class="col-sm-6">
                                            <Rock:DataDropDownList ID="ddlGroupType" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Name" Label="Group Type" AutoPostBack="true" OnSelectedIndexChanged="ddlGroupType_SelectedIndexChanged" Required="true" />
                                            <Rock:RockLiteral ID="lGroupType" runat="server" Label="Group Type" />
                                        </div>
                                        <div class="col-sm-6">
                                            <Rock:RockCheckBox ID="cbIsSecurityRole" runat="server" Label="Security Role" AutoPostBack="true" OnCheckedChanged="cbIsSecurityRole_CheckedChanged" />
                                        </div>
                                    </div>
                                    <Rock:GroupPicker ID="gpParentGroup" runat="server" Required="false" Label="Parent Group" OnSelectItem="ddlParentGroup_SelectedIndexChanged" />
                                    <Rock:DefinedValuePicker ID="dvpGroupStatus" runat="server" Label="Status" Visible="false" />
                                    <Rock:NumberBox ID="nbGroupCapacity" runat="server" Label="Group Capacity" NumberType="Integer" MinimumValue="0" />
                                    <Rock:PersonPicker ID="ppAdministrator" runat="server" EnableSelfSelection="true" />
                                </div>
                                <div class="col-md-6">
                                    <asp:Panel runat="server" ID="pnlElevatedSecurity">
                                        <Rock:RockRadioButtonList
                                            ID="rblElevatedSecurityLevel"
                                            runat="server"
                                            Label="Elevated Security Level"
                                            Required="true"
                                            Help="Determines the amount of extra security privileges this security role provides. This helps Rock protect accounts of individuals with high-level access."
                                            RepeatDirection="Horizontal"
                                        />
                                    </asp:Panel>
                                    <Rock:CampusPicker ID="cpCampus" runat="server" Label="Campus" />
                                    <Rock:RockDropDownList ID="ddlSignatureDocumentTemplate" runat="server" Label="Require Signed Document"
                                        Help="If members of this group need to have signed a document, select that document type here." />
                                    <Rock:DefinedValuePicker ID="dvpRecordSource" runat="server" Label="Record Source Override" Help="The record source for group members added to this group. If not set, the group type's record source will be used." />
                                </div>
                            </div>

                            <%-- Peer Network --%>
                            <asp:Panel ID="pnlPeerNetworkOverride" runat="server">
                                <Rock:RockCheckBox ID="cbOverrideRelationshipStrength" runat="server" AutoPostBack="true" Label="Override Relationship Strength" Help="A relationship strength has been set for this group type, but you can override that configuration here." OnCheckedChanged="cbOverrideRelationshipStrength_CheckedChanged" />
                                <asp:Panel ID="pnlPeerNetwork" runat="server">
                                    <div class="row">
                                        <div class="col-md-6">
                                            <Rock:RockRadioButtonList ID="rblRelationshipStrength" runat="server" Label="Relationship Strength" Help="Sets the relationship strength for individuals in groups of this type. Advanced settings offer additional customization options based on the individual's role in the group." RepeatDirection="Horizontal" AutoPostBack="true" OnSelectedIndexChanged="rblRelationshipStrength_SelectedIndexChanged" CssClass="js-relationship-strength" />
                                        </div>
                                        <div id="pnlRelationshipGrowth" runat="server" class="col-md-6">
                                            <Rock:RockCheckBox ID="cbEnableRelationshipGrowth" runat="server" Label="Enable Relationship Growth Over Time" Help="Enable this setting to allow relationship strength to grow over time as individuals spend more time together in the group." />
                                        </div>
                                    </div>
                                    <div id="pnlShowPeerNetworkAdvancedSettings" runat="server" class="row">
                                        <div class="col-md-6">
                                            <Rock:Switch ID="swShowPeerNetworkAdvancedSettings" runat="server" AutoPostBack="true" Text="Show Advanced Settings" OnCheckedChanged="swShowPeerNetworkAdvancedSettings_CheckedChanged" />
                                        </div>
                                    </div>
                                    <asp:Panel ID="pnlPeerNetworkAdvanced" runat="server" Visible="false">
                                        <Rock:RockLiteral ID="lRelationshipStrenghAdvanced" runat="server">
                                            You can adjust the relationship score below based on the relationship type of the group members. For instance, if the group members have a strong relationship with the leaders but do not know other non-leaders, you can reduce or eliminate the relationship score percentage between non-leaders.
                                        </Rock:RockLiteral>
                                        <div class="d-grid grid-cols-3 gap-2" style="width: max-content;">
                                            <div></div>
                                            <div class="d-flex align-items-center">
                                                <label class="control-label">
                                                    Leader
                                                </label>
                                            </div>
                                            <div class="d-flex align-items-center">
                                                <label class="control-label">
                                                    Non-Leader
                                                </label>
                                            </div>
                                            <div class="d-flex align-items-center">
                                                <label class="control-label">
                                                    Leader
                                                </label>
                                            </div>
                                            <div class="d-flex align-items-center">
                                                <Rock:RockTextBox ID="tbLeaderToLeaderRelationshipMultiplier" runat="server" CssClass="input-width-sm" />
                                            </div>
                                            <div class="d-flex align-items-center">
                                                <Rock:RockTextBox ID="tbLeaderToNonLeaderRelationshipMultiplier" runat="server" CssClass="input-width-sm" />
                                            </div>
                                            <div class="d-flex align-items-center">
                                                <label class="control-label">
                                                    Non-Leader
                                                </label>
                                            </div>
                                            <div class="d-flex align-items-center">
                                                <Rock:RockTextBox ID="tbNonLeaderToLeaderRelationshipMultiplier" runat="server" CssClass="input-width-sm" />
                                            </div>
                                            <div class="d-flex align-items-center">
                                                <Rock:RockTextBox ID="tbNonLeaderToNonLeaderRelationshipMultiplier" runat="server" CssClass="input-width-sm" />
                                            </div>
                                        </div>
                                    </asp:Panel>
                                </asp:Panel>
                            </asp:Panel>

                        </Rock:PanelWidget>

                        <%-- RSVP Settings --%>
                        <Rock:PanelWidget ID="wpRsvp" runat="server" Title="RSVP">
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockDropDownList ID="ddlRsvpReminderSystemCommunication" runat="server" Label="RSVP Reminder System Communication"
                                        Help="The System Communication that should be sent to remind group members to RSVP for group events." />
                                </div>
                                <div class="col-md-6">
                                    <Rock:RangeSlider ID="rsRsvpReminderOffsetDays" runat="server" Label="RSVP Reminder Offset Days" MinValue="0" MaxValue="30" SelectedValue="1"
                                        Help="The number of days prior to a group event occurrence to send the RSVP reminder." />
                                </div>
                            </div>
                        </Rock:PanelWidget>

                        <Rock:PanelWidget ID="wpMeetingDetails" runat="server" Title="Meeting Details">
                            <div class="grid">
                                <Rock:Grid ID="gGroupLocations" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Location">
                                    <Columns>
                                        <Rock:RockBoundField DataField="Location" HeaderText="Location" />
                                        <Rock:RockBoundField DataField="Type" HeaderText="Type" />
                                        <Rock:RockTemplateField HeaderText="Schedule(s)">
                                            <ItemTemplate>
                                                <asp:Literal ID="litSchedules" runat="server" Text='<%#Eval("Schedules") %>' />
                                            </ItemTemplate>
                                        </Rock:RockTemplateField>
                                        <Rock:EditField OnClick="gGroupLocations_Edit" />
                                        <Rock:DeleteField OnClick="gGroupLocations_Delete" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                            <asp:Panel ID="pnlSchedule" runat="server" Visible="false">

                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:RockRadioButtonList ID="rblScheduleSelect" runat="server" Label="Group Schedule" CssClass="margin-b-sm" OnSelectedIndexChanged="rblScheduleSelect_SelectedIndexChanged" AutoPostBack="true" RepeatDirection="Horizontal" />
                                    </div>
                                    <div class="col-md-6">
                                        <div class="row">
                                            <div class="col-sm-6">
                                                <Rock:DayOfWeekPicker ID="dowWeekly" runat="server" CssClass="input-width-md" Visible="false" Label="Day of the Week" />
                                            </div>
                                            <div class="col-sm-6">
                                                <Rock:TimePicker ID="timeWeekly" runat="server" Visible="false" Label="Time of Day" />
                                            </div>
                                        </div>
                                        <Rock:SchedulePicker ID="spSchedule" runat="server" AllowMultiSelect="false" Visible="false" Label="Named Schedule" />
                                        <asp:HiddenField ID="hfUniqueScheduleId" runat="server" />
                                        <Rock:ScheduleBuilder ID="sbSchedule" runat="server" ShowDuration="false" ShowScheduleFriendlyTextAsToolTip="true" Visible="false" Label="Custom Schedule" />
                                    </div>
                                </div>
                            </asp:Panel>
                        </Rock:PanelWidget>

                        <Rock:PanelWidget ID="wpScheduling" runat="server" Title="Scheduling">
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockCheckBox ID="cbDisableGroupScheduling" runat="server" Label="Disable Group Scheduling" Help="Checking this box will opt the group out from the group scheduling system." />
                                    <Rock:RockCheckBox ID="cbSchedulingMustMeetRequirements" runat="server" Label="Scheduling Must Meet Requirements" Help="Indicates whether group members must meet the group member requirements before they can be scheduled." />
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockCheckBox ID="cbDisableScheduleToolboxAccess" runat="server" Label="Disable Schedule Toolbox Access" Help="Checking this will hide the group from the schedule toolbox." />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockDropDownList ID="ddlAttendanceRecordRequiredForCheckIn" runat="server" Label="Check-in Requirements" Help="Determines if the person must be scheduled prior to checking in." />
                                </div>
                                <div class="col-md-6">
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockDropDownList ID="ddlScheduleConfirmationLogic" runat="server" Label="Schedule Confirmation Logic" Help="Determines if the individual will be asked to Accept or Decline, or if their request will be auto accepted. This setting overrides the group type's setting." />
                                </div>
                                <div class="col-md-6">
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:PersonPicker ID="ppScheduleCoordinatorPerson" runat="server" EnableSelfSelection="true" Label="Schedule Coordinator" Help="The person who receives notifications about changes to scheduled individuals." />
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockCheckBoxList ID="cblScheduleCoordinatorNotificationTypes" runat="server" Label="Schedule Coordinator Notification Options" RepeatDirection="Horizontal" Help='Specifies the types of notifications the coordinator receives about scheduled individuals. Leave blank to use the Group Type settings, or select one or more options to override them.' OnSelectedIndexChanged="cblScheduleCoordinatorNotificationTypes_SelectedIndexChanged" AutoPostBack="true">
                                        <asp:ListItem Value="0" Text="None" />
                                        <asp:ListItem Value="1" Text="Accept" />
                                        <asp:ListItem Value="2" Text="Decline" />
                                        <asp:ListItem Value="4" Text="Self-Schedule" />
                                    </Rock:RockCheckBoxList>
                                </div>
                            </div>
                        </Rock:PanelWidget>

                        <Rock:PanelWidget ID="wpGroupAttributes" runat="server" Title="Group Attribute Values">
                            <Rock:DynamicPlaceholder ID="phGroupAttributes" runat="server"></Rock:DynamicPlaceholder>
                        </Rock:PanelWidget>

                        <Rock:PanelWidget ID="wpGroupMemberAttributes" runat="server" Title="Member Attributes" CssClass="group-type-attribute-panel">
                            <Rock:NotificationBox ID="nbGroupMemberAttributes" runat="server" NotificationBoxType="Info"
                                Text="Member Attributes apply to members in this group. Each member will have their own value for these attributes." />
                            <Rock:RockControlWrapper ID="rcwGroupMemberAttributesInherited" runat="server" Label="Inherited Group Member Attributes">
                                <div class="grid">
                                    <Rock:Grid ID="gGroupMemberAttributesInherited" runat="server" AllowPaging="false" DisplayType="Light" ShowHeader="true" RowItemText="Inherited Member Attribute">
                                        <Columns>
                                            <Rock:RockBoundField DataField="Name" HeaderText="Attribute" />
                                            <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                                            <Rock:RockTemplateField HeaderText="Inherited">
                                                <ItemTemplate>(Inherited from <a href='<%# Eval("Url") %>' target='_blank' rel='noopener noreferrer'><%# Eval("GroupType") %></a>)</ItemTemplate>
                                            </Rock:RockTemplateField>
                                        </Columns>
                                    </Rock:Grid>
                                </div>
                            </Rock:RockControlWrapper>

                            <Rock:RockControlWrapper ID="rcwGroupMemberAttributes" runat="server" Label="Group Member Attribute(s)">
                                <div class="grid">
                                    <Rock:Grid ID="gGroupMemberAttributes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Member Attribute" ShowConfirmDeleteDialog="false">
                                        <Columns>
                                            <Rock:ReorderField />
                                            <Rock:RockBoundField DataField="Name" HeaderText="Attribute" />
                                            <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                                            <Rock:BoolField DataField="IsRequired" HeaderText="Required" />
                                            <Rock:SecurityField TitleField="Name" />
                                            <Rock:EditField OnClick="gGroupMemberAttributes_Edit" />
                                            <Rock:DeleteField OnClick="gGroupMemberAttributes_Delete" />
                                        </Columns>
                                    </Rock:Grid>
                                </div>
                            </Rock:RockControlWrapper>
                        </Rock:PanelWidget>

                        <Rock:PanelWidget ID="wpGroupRequirements" runat="server" Title="Group Requirements">
                            <Rock:RockControlWrapper ID="rcwGroupTypeGroupRequirements" runat="server" Label="Group Requirements for Group Type">
                                <asp:Literal ID="lGroupTypeGroupRequirementsFrom" runat="server" Text="(From ...)" />
                                <div class="grid">
                                    <Rock:Grid ID="gGroupTypeGroupRequirements" runat="server" AllowPaging="false" DisplayType="Light" ShowHeader="true" RowItemText="Group Requirements for Group Type">
                                        <Columns>
                                            <Rock:RockBoundField DataField="GroupRequirementType.Name" HeaderText="Name" />
                                            <Rock:RockBoundField DataField="GroupRole.Name" HeaderText="Role" />
                                        </Columns>
                                    </Rock:Grid>
                                </div>
                            </Rock:RockControlWrapper>

                            <Rock:RockControlWrapper ID="rcwGroupRequirements" runat="server" Label="Specific Group Requirement(s)">
                                <div class="grid">
                                    <Rock:Grid ID="gGroupRequirements" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Group Requirement" ShowConfirmDeleteDialog="false">
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
                        </Rock:PanelWidget>

                        <%-- Chat --%>
                        <Rock:PanelWidget ID="wpChat" runat="server" Title="Chat">
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockDropDownList ID="ddlIsChatEnabled" runat="server" CssClass="input-width-xl" Label="Enable Chat" Help="If enabled, this group will participate in the chat system as a chat channel.">
                                        <asp:ListItem Value="" Text="Inherit from Group Type" />
                                        <asp:ListItem Value="n" Text="No" />
                                        <asp:ListItem Value="y" Text="Yes" />
                                    </Rock:RockDropDownList>
                                    <Rock:RockDropDownList ID="ddlIsLeavingChatChannelAllowed" runat="server" CssClass="input-width-xl" Label="Allow Members to Leave Channel" Help="If enabled, individuals are allowed to leave this chat channel.">
                                        <asp:ListItem Value="" Text="Inherit from Group Type" />
                                        <asp:ListItem Value="n" Text="No" />
                                        <asp:ListItem Value="y" Text="Yes" />
                                    </Rock:RockDropDownList>
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockDropDownList ID="ddlIsChatChannelPublic" runat="server" CssClass="input-width-xl" Label="Make Channel Public" Help="If enabled, this chat channel is visible to everyone when performing a search. This also implies that the channel may be joined by any person via the chat application.">
                                        <asp:ListItem Value="" Text="Inherit from Group Type" />
                                        <asp:ListItem Value="n" Text="No" />
                                        <asp:ListItem Value="y" Text="Yes" />
                                    </Rock:RockDropDownList>
                                    <Rock:RockDropDownList ID="ddlIsChatChannelAlwaysShown" runat="server" CssClass="input-width-xl" Label="Always Show Channel" Help="If enabled, this chat channel is always shown in the channel list even if the person has not joined the channel.">
                                        <asp:ListItem Value="" Text="Inherit from Group Type" />
                                        <asp:ListItem Value="n" Text="No" />
                                        <asp:ListItem Value="y" Text="Yes" />
                                    </Rock:RockDropDownList>
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockDropDownList ID="ddlChatPushNotificationMode" runat="server" CssClass="input-width-xl" Label="Push Notification Mode" Help="Controls how push notifications are sent for this chat channel.">
                                        <asp:ListItem Value="" Text="Inherit from Group Type" />
                                        <asp:ListItem Value="0" Text="All Messages" />
                                        <asp:ListItem Value="1" Text="Mentions" />
                                        <asp:ListItem Value="2" Text="Silent" />
                                    </Rock:RockDropDownList>
                                </div>
                                <div class="col-md-6">
                                    <Rock:ImageUploader ID="imgChatChannelAvatar" runat="server" Label="Channel Avatar" Help="The image to use for this chat channel in the external chat system. Recommended image size is 120x120." />
                                </div>
                            </div>
                        </Rock:PanelWidget>

                        <Rock:PanelWidget ID="wpGroupSync" runat="server" Title="Group Sync Settings">
                            <div class="grid">
                                <Rock:Grid ID="gGroupSyncs" runat="server" AllowPaging="false" DisplayType="Light" ShowHeader="true" RowItemText="Group Sync for Role">
                                    <Columns>
                                        <Rock:RockBoundField DataField="GroupTypeRole.Name" HeaderText="Role Name"></Rock:RockBoundField>
                                        <Rock:RockBoundField DataField="SyncDataView.Name" HeaderText="Data View Name"></Rock:RockBoundField>
                                        <Rock:RockBoundField DataField="ScheduleTimeInterval" HeaderText="Sync Interval"></Rock:RockBoundField>
                                        <Rock:DateTimeField DataField="LastRefreshDateTime" HeaderText="Last Sync"></Rock:DateTimeField>
                                        <Rock:EditField OnClick="gGroupSyncs_Edit" />
                                        <Rock:DeleteField OnClick="gGroupSyncs_Delete" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </Rock:PanelWidget>

                        <Rock:PanelWidget ID="wpMemberWorkflowTriggers" runat="server" Title="Group Member Workflows">
                            <Rock:NotificationBox ID="NotificationBox3" runat="server" NotificationBoxType="Info"
                                Text="The workflow(s) that should be launched when group members are changed in this group." />
                            <div class="grid">
                                <Rock:Grid ID="gMemberWorkflowTriggers" runat="server" EnableResponsiveTable="false" AllowPaging="false" DisplayType="Light" RowItemText="Workflow">
                                    <Columns>
                                        <Rock:ReorderField />
                                        <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                                        <Rock:RockBoundField DataField="WorkflowType.Name" HeaderText="Workflow" />
                                        <Rock:RockTemplateField HeaderText="When">
                                            <ItemTemplate>
                                                <%# FormatTriggerType( Eval("TriggerType"), Eval("TypeQualifier") ) %>
                                            </ItemTemplate>
                                        </Rock:RockTemplateField>
                                        <Rock:BoolField DataField="IsActive" HeaderText="Active" />
                                        <Rock:EditField OnClick="gMemberWorkflowTriggers_Edit" />
                                        <Rock:DeleteField OnClick="gMemberWorkflowTriggers_Delete" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </Rock:PanelWidget>

                        <div class="actions">
                            <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                            <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                        </div>

                    </div>

                    <fieldset id="fieldsetViewDetails" runat="server">
                        <div class="taglist">
                            <Rock:TagList ID="taglGroupTags" runat="server" CssClass="clearfix" />
                        </div>

                        <asp:Literal ID="lContent" runat="server"></asp:Literal>

                        <div class="actions">
                            <asp:LinkButton ID="btnEdit" runat="server" data-shortcut-key="e" AccessKey="m" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                            <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                            <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" CausesValidation="false" />
                            <asp:LinkButton ID="btnArchive" runat="server" Text="Archive" CssClass="btn btn-link js-archive-group" OnClick="btnArchive_Click" CausesValidation="false" />
                            <span class="pull-right">
                                <asp:HyperLink ID="hlGroupPlacement" runat="server" CssClass="btn btn-sm btn-square btn-default" ToolTip="Group Placement"><i class="fa fa-project-diagram"></i></asp:HyperLink>
                                <asp:HyperLink ID="hlGroupRSVP" runat="server" CssClass="btn btn-sm btn-square btn-default" ToolTip="Group RSVP"><i class="fa fa-user-check"></i></asp:HyperLink>
                                <asp:HyperLink ID="hlGroupScheduler" runat="server" CssClass="btn btn-sm btn-square btn-default" ToolTip="Group Scheduler"><i class="fa fa-calendar-alt"></i></asp:HyperLink>
                                <asp:HyperLink ID="hlGroupHistory" runat="server" CssClass="btn btn-sm btn-square btn-default" ToolTip="Group History"><i class="fa fa-history"></i></asp:HyperLink>
                                <asp:HyperLink ID="hlFundraisingProgress" runat="server" CssClass="btn btn-sm btn-square btn-default" ToolTip="Fundraising"><i class="fa fa-line-chart"></i></asp:HyperLink>
                                <asp:HyperLink ID="hlAttendance" runat="server" CssClass="btn btn-sm btn-square btn-default" ToolTip="Attendance"><i class="fa fa-check-square-o"></i></asp:HyperLink>
                                <asp:HyperLink ID="hlMap" runat="server" CssClass="btn btn-sm btn-square btn-default" ToolTip="Interactive Map"><i class="fa fa-map-marker"></i></asp:HyperLink>
                                <asp:LinkButton ID="btnCopy" runat="server" CssClass="btn btn-sm btn-square btn-default " OnClick="btnCopy_Click" ToolTip="Copies the group and all of its associated authorization rules"><i class="fa fa-clone"></i></asp:LinkButton>
                                <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-square btn-security" Title="Secure Group" />
                            </span>
                        </div>

                    </fieldset>
                </div>
            </div>
            
            <Rock:ModalDialog ID="mdCopyGroup" runat="server" ValidationGroup="vgCopyGroup" Title="Copy Group" OnSaveClick="mdCopyGroup_SaveClick" SaveButtonText="Copy" Visible="false">
                <Content>
                    <Rock:NotificationBox ID="mdCopyWarning" runat="server" NotificationBoxType="Warning" Text="
                        This action will copy the selected group and (optionally) it's child groups. Group members will not be copied.<br />
                        The following items will be included for every copied group:<br />
                        <ul>
                            <li>
                                Group member attributes configuration
                            </li>
                            <li>
                                Group attributes configuration and their values
                            </li>
                            <li>
                                Schedules
                            </li>
                            <li>
                                Authorization (both authorizations to the group and access to other entities based on membership to the group)
                            </li>
                            <li>
                                Locations (except when the location is a group member address)
                            </li>
                            <li>
                                Group requirements
                            </li>
                            <li>
                                Group syncs
                            </li>
                        </ul>"
                        />
                    <Rock:RockCheckBox ID="cbCopyGroupIncludeChildGroups" runat="server" Text="Include Child Groups" Checked="true" />
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <!-- Group Member Attribute Modal Dialog -->
        <Rock:ModalDialog ID="dlgGroupMemberAttribute" runat="server" Title="Group Member Attributes" OnSaveClick="dlgGroupMemberAttribute_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="GroupMemberAttribute">
            <Content>
                <Rock:AttributeEditor ID="edtGroupMemberAttributes" runat="server" ShowActions="false" ValidationGroup="GroupMemberAttribute" />
            </Content>
        </Rock:ModalDialog>

        <!-- Group Location Modal Dialog -->
        <Rock:ModalDialog ID="dlgLocations" runat="server" Title="Group Location" SaveButtonText="Ok" OnSaveClick="dlgLocations_OkClick" OnCancelScript="clearActiveDialog();" ValidationGroup="Location">
            <Content>

                <asp:HiddenField ID="hfAddLocationGroupGuid" runat="server" />
                <asp:HiddenField ID="hfAction" runat="server" />
                <Rock:NotificationBox ID="nbGroupLocationEditMessage" runat="server" NotificationBoxType="Info" />

                <asp:ValidationSummary ID="valLocationSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="Location" />

                <ul id="ulNav" runat="server" class="nav nav-pills">
                    <asp:Repeater ID="rptLocationTypes" runat="server">
                        <ItemTemplate>
                            <li class='<%# GetTabClass(Container.DataItem) %>'>
                                <asp:LinkButton ID="lbLocationType" runat="server" Text='<%# Container.DataItem %>' OnClick="lbLocationType_Click" CausesValidation="false">
                                </asp:LinkButton>
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>
                </ul>

                <div class="tabContent">
                    <asp:Panel ID="pnlMemberSelect" runat="server" Visible="true">
                        <Rock:RockDropDownList ID="ddlMember" runat="server" Label="Member" ValidationGroup="Location" />
                    </asp:Panel>
                    <asp:Panel ID="pnlLocationSelect" runat="server" Visible="false">
                        <Rock:LocationPicker ID="locpGroupLocation" runat="server" Label="Location" ValidationGroup="Location" OnSelectLocation="locpGroupLocation_SelectLocation" />
                    </asp:Panel>
                </div>

                <Rock:RockDropDownList ID="ddlLocationType" runat="server" Label="Type" DataValueField="Id" DataTextField="Value" ValidationGroup="Location" />

                <div class="row">
                    <div class="col-md-3">
                        <asp:HiddenField ID="hfGroupLocationGuid" runat="server" />
                        <asp:HiddenField ID="hfInactiveGroupLocationSchedules" runat="server" />
                        <Rock:SchedulePicker ID="spSchedules" runat="server" Label="Schedule(s)" OnSelectItem="spSchedules_SelectItem" ValidationGroup="Location" AllowMultiSelect="true" AllowInactiveSelection="false" />
                    </div>
                    <div class="col-md-9">
                        <%-- Group Location Schedule Capacities (if Group Scheduling Enabled) --%>
                        <Rock:RockControlWrapper ID="rcwGroupLocationScheduleCapacities" runat="server" Label="Capacities" Help="Set the capacities to use when scheduling people to this location.">
                            <asp:Repeater ID="rptGroupLocationScheduleCapacities" runat="server" OnItemDataBound="rptGroupLocationScheduleCapacities_ItemDataBound">
                                <HeaderTemplate>
                                    <div class="row">
                                        <div></div>
                                        <div class="col-xs-3">
                                            <span class="control-label"></span>
                                        </div>
                                        <div class="col-xs-3">
                                            <span class="control-label">Minimum</span>
                                        </div>
                                        <div class="col-xs-3">
                                            <span class="control-label">Desired</span>
                                        </div>
                                        <div class="col-xs-3">
                                            <span class="control-label">Maximum</span>
                                        </div>
                                    </div>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <div class="row margin-t-sm">
                                        <div>
                                            <asp:HiddenField ID="hfScheduleId" runat="server" />
                                        </div>
                                        <div class="col-xs-3">
                                            <asp:Literal ID="lScheduleName" runat="server" />
                                        </div>
                                        <div class="col-xs-3">
                                            <Rock:NumberBox ID="nbMinimumCapacity" CssClass="input-width-sm" runat="server" NumberType="Integer" MinimumValue="0" />
                                        </div>
                                        <div class="col-xs-3">
                                            <Rock:NumberBox ID="nbDesiredCapacity" CssClass="input-width-sm" runat="server" NumberType="Integer" MinimumValue="0" />
                                        </div>
                                        <div class="col-xs-3">
                                            <Rock:NumberBox ID="nbMaximumCapacity" CssClass="input-width-sm" runat="server" NumberType="Integer" MinimumValue="0" />
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </Rock:RockControlWrapper>
                    </div>
                </div>

            </Content>
        </Rock:ModalDialog>

        <!-- Group Requirements Modal Dialog -->
        <Rock:ModalDialog ID="mdGroupRequirement" runat="server" Title="Group Requirement" OnSaveClick="mdGroupRequirement_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="vg_GroupRequirement">
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

                <Rock:RockCheckBox ID="cbAllowLeadersToOverride" runat="server" Text="Allow Leaders to Override" Help="Determines if the leader should be allowed to override meeting the requirement." />

                <Rock:DatePicker ID="dpDueDate" runat="server" Label="Due Date" ValidationGroup="vg_GroupRequirement" />
                <Rock:RockDropDownList ID="ddlDueDateGroupAttribute" runat="server" Label="Due Date Group Attribute" Help="The group attribute that contains the due date for requirements." ValidationGroup="vg_GroupRequirement" />
                <Rock:RockCheckBox ID="cbMembersMustMeetRequirementOnAdd" runat="server" Text="Members must meet this requirement before adding" Help="If this is enabled, a person can only become a group member if this requirement is met. Note: only applies to Data View and SQL type requirements since manual ones can't be checked until after the person is added." />

            </Content>
        </Rock:ModalDialog>

        <%-- Group Sync Settings Modal Dialog --%>
        <Rock:ModalDialog ID="mdGroupSyncSettings" runat="server" Title="Group Sync Settings" OnSaveClick="mdGroupSyncSettings_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="GroupSyncSettings">
            <Content>
                <asp:HiddenField ID="hfGroupSyncGuid" runat="server" />
                <asp:ValidationSummary ID="valGroupSyncSettings" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="GroupSyncSettings" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataViewItemPicker ID="dvipSyncDataView" runat="server" Label="Sync Data View" Help="Select the Data View for the sync" Required="true" ValidationGroup="GroupSyncSettings" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlGroupRoles" runat="server" Label="Group Role to Assign" Help="Select the role to assign the members added by the selected Data View" Required="true" ValidationGroup="GroupSyncSettings" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:IntervalPicker
                            ID="ipScheduleIntervalMinutes"
                            runat="server"
                            Label="Sync Interval"
                            Help="Controls how often the group should sync to the Data View. It will never be less then the Group Sync job execution interval."
                            ValidationGroup="GroupSyncSettings"
                            DefaultValue="12"
                            DefaultInterval="Hour" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlWelcomeCommunication" runat="server" Label="Welcome Communication" ValidationGroup="GroupSyncSettings"></Rock:RockDropDownList>
                    </div>
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlExitCommunication" runat="server" Label="Exit Communication" ValidationGroup="GroupSyncSettings"></Rock:RockDropDownList>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbCreateLoginDuringSync" runat="server" Label="Create Login During Sync" Help="If the individual does not have a login, should one be created during the sync process?" ValidationGroup="GroupSyncSettings" />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

        <%-- Workflow Modal Dialog --%>
        <Rock:ModalDialog ID="dlgMemberWorkflowTriggers" runat="server" OnSaveClick="dlgMemberWorkflowTriggers_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="Trigger">
            <Content>
                <asp:HiddenField ID="hfTriggerGuid" runat="server" />
                <asp:ValidationSummary ID="vsTrigger" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="Trigger" />
                <Rock:NotificationBox ID="nbInvalidWorkflowType" runat="server" NotificationBoxType="Danger" Visible="false"
                    Text="The Workflow Type is missing or invalid. Make sure you selected a valid Workflow Type (and not a category)." />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbTriggerName" runat="server" Label="Name" Required="true" ValidationGroup="Trigger" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbTriggerIsActive" runat="server" Label="Active" ValidationGroup="Trigger" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:WorkflowTypePicker ID="wtpWorkflowType" runat="server" Label="Start Workflow" Required="true" ValidationGroup="Trigger"
                            Help="The workflow type to start." />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlTriggerType" runat="server" Label="When" Required="true" ValidationGroup="Trigger" AutoPostBack="true" OnSelectedIndexChanged="ddlTriggerType_SelectedIndexChanged" />
                        <Rock:RockDropDownList ID="ddlTriggerFromStatus" runat="server" Label="From Status of" ValidationGroup="Trigger" />
                        <Rock:RockDropDownList ID="ddlTriggerToStatus" runat="server" Label="To Status of" ValidationGroup="Trigger" />
                        <Rock:RockDropDownList ID="ddlTriggerFromRole" runat="server" Label="From Role of" ValidationGroup="Trigger" DataTextField="Name" DataValueField="Guid" />
                        <Rock:RockDropDownList ID="ddlTriggerToRole" runat="server" Label="To Role of" ValidationGroup="Trigger" DataTextField="Name" DataValueField="Guid" />
                        <Rock:RockCheckBox ID="cbTriggerFirstTime" runat="server" Label="First Time" ValidationGroup="Trigger"
                            Help="Select this option if workflow should only be started when person attends the group for the first time. Leave this option unselected if the workflow should be started whenever a person attends the group." />
                        <Rock:RockCheckBox ID="cbTriggerPlacedElsewhereShowNote" runat="server" Label="Show Note" ValidationGroup="Trigger"
                            Help="Select this option if workflow should show UI for entering a note when the member is placed." />
                        <Rock:RockCheckBox ID="cbTriggerPlacedElsewhereRequireNote" runat="server" Label="Require Note" ValidationGroup="Trigger"
                            Help="Select this option if workflow should show UI for entering a note and make it required when the member is placed." />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="mdArchive" runat="server" Title="Archive Single Group or All Child Groups" OnSaveClick="mdArchive_AllChildGroupsClick" OnSaveThenAddClick="mdArchive_SingleGroupClick" SaveButtonText="Yes" SaveThenAddButtonText="No" CancelLinkVisible="false">
            <Content>
                <p>Would you like to archive this group's children?</p>
            </Content>
        </Rock:ModalDialog>
        <script>

            Sys.Application.add_load(function () {
                function setIsActiveControls(activeCheckbox) {
                    var $inactiveLabel = $(activeCheckbox).closest(".js-group-panel").find('.js-inactivegroup-label');
                    if ($(activeCheckbox).is(':checked')) {
                        $inactiveLabel.hide();
                    }
                    else {
                        $inactiveLabel.show();
                    }

                    // if isactive was toggled from Active to Inactive and the group has child groups, show the inactivate child groups checkbox
                    var hasChildren = $('.js-haschildgroups').val();
                    var rfvId = "<%= ddlInactiveReason.ClientID %>" + "_rfv";

                    if ($(activeCheckbox).is(':checked')) {
                        $('.js-inactivateoptions').hide();
                        $('.js-inactivatechildgroups').hide();
                        enableRequiredField(rfvId, false);
                    }
                    else {
                        $('.js-inactivateoptions').show();
                        enableRequiredField(rfvId, true);

                        if (hasChildren === "true") {
                            $('.js-inactivatechildgroups').show();
                        }
                    }
                }

                function setPrivateLabel(publicCheckbox) {
                    var $privateLabel = $(publicCheckbox).closest(".js-group-panel").find('.js-privategroup-label');
                    if ($(publicCheckbox).is(':checked')) {
                        $privateLabel.hide();
                    }
                    else {
                        $privateLabel.show();
                    }
                }

                function enableRequiredField(validatorId, enable) {
                    var jqObj = $('#' + validatorId);
                    if (jqObj != null) {
                        var domObj = jqObj.get(0);
                        if (domObj != null) {
                            ValidatorEnable(domObj, enable);
                        }
                    }
                }

                $('.js-isactivegroup').on('click', function () {
                    setIsActiveControls(this);
                });

                $('.js-ispublicgroup').on('click', function () {
                    setPrivateLabel(this);
                });

                $('.js-isactivegroup').each(function (i) {
                    setIsActiveControls(this);
                });

                $('.js-ispublicgroup').each(function (i) {
                    setPrivateLabel(this);
                });

                $('.js-archive-group').on('click', function (e) {
                    e.preventDefault();
                    Rock.dialogs.confirm('Are you sure you want to archive this group?', function (result) {
                        if (result) {
                            window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                        }
                    });
                });

                var $thisBlock = $('#<%= upnlGroupDetail.ClientID %>');

                addRelationshipStrengthTooltips();

                function addRelationshipStrengthTooltips() {
                    var $relStrength = $thisBlock.find('.js-relationship-strength');
                    if (!$relStrength.length) {
                        return;
                    }

                    addTooltip('0', 'No established relationship or interaction.');
                    addTooltip('5', 'Basic interactions with a familiar but limited bond.');
                    addTooltip('10', 'Frequent interactions characterized by a strong and supportive relationship.');
                    addTooltip('20', 'Intense and trusted relationship with a high level of personal engagement and understanding.');

                    function addTooltip(value, tooltip) {
                        var $label = $relStrength
                            .find('input[type="radio"][value="' + value + '"]')
                            .closest('label');

                        $label.attr('title', tooltip);
                        $label.tooltip();

                        // Hide the tooltip when a selection is made.
                        $label.find('input[type="radio"]').on('click', function () {
                            $label.tooltip('hide');
                        });
                    }
                }
            });

        </script>

    </ContentTemplate>
</asp:UpdatePanel>
