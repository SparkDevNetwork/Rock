<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupTypeDetail.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupTypeDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upGroupType" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-users"></i>
                    <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlType" runat="server" LabelType="Type" />
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <asp:HiddenField ID="hfGroupTypeId" runat="server" />

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div id="pnlViewDetails" runat="server">
                    <p class="description">
                        <asp:Literal ID="lGroupTypeDescription" runat="server"></asp:Literal>
                    </p>

                    <div class="row">
                        <div class="cold-md-12">
                            <asp:Literal ID="lblMainDetails" runat="server" />
                        </div>
                    </div>
                    <div>
                        <div class="col-md-12">
                            <Rock:AttributeValuesContainer ID="avcDisplayAttributes" runat="server" />
                        </div>
                    </div>
                </div>

                <div id="pnlEditDetails" runat="server">

                    <asp:ValidationSummary ID="valGroupTypeDetail" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                    <asp:CustomValidator ID="cvGroupType" runat="server" Display="None" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Name" />
                        </div>
                        <div class="col-md-6">
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                        </div>
                    </div>

                    <Rock:PanelWidget ID="wpGeneral" runat="server" Title="General">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DefinedValuePicker ID="dvpGroupTypePurpose" runat="server" Label="Purpose"
                                    Help="An optional field used to qualify what the over-all purpose of this group type is for.  Additional values can be added by editing the 'Group Type Purpose' Defined Type." />

                                <Rock:RockCheckBox ID="cbAllowAnyChildGroupType" runat="server" Label="Allow Any Child Group Type" Help="Determines if all types of child groups can be added to groups of this type" AutoPostBack="true" OnCheckedChanged="cbAllowAnyChildGroupType_CheckedChanged" />
                                <Rock:RockControlWrapper ID="rcwAllowedChildGroupTypes" runat="server" Label="Allowed Child Group Types" 
                                    Help="The types of child groups that can be added to groups of this type. This is used to define the group hierarchy. To allow an unlimited hierarchy add this type as an allowed child group type.">

                                    <div class="grid">
                                        <Rock:Grid ID="gChildGroupTypes" runat="server" DisplayType="Light" ShowHeader="false" RowItemText="Group Type" HideDeleteButtonForIsSystem="false">
                                            <Columns>
                                                <Rock:RockBoundField DataField="Name" />
                                                <Rock:DeleteField OnClick="gChildGroupTypes_Delete" />
                                            </Columns>
                                        </Rock:Grid>
                                    </div>
                                </Rock:RockControlWrapper>

                                <Rock:GroupTypePicker ID="gtpInheritedGroupType" runat="server" Label="Inherited Group Type"
                                    Help="Group Type to inherit attributes from" AutoPostBack="true" OnSelectedIndexChanged="gtpInheritedGroupType_SelectedIndexChanged" />

                                <div class="row">
                                    <div class="col-xs-6">
                                        <Rock:RockDropDownList ID="ddlGroupCapacityRule" runat="server" Label="Group Capacity Rule" Help="Does this group type support group capacity and if so how is it enforced." OnSelectedIndexChanged="ddlGroupCapacityRule_SelectedIndexChanged" AutoPostBack="true"/>
                                    </div>
                                    <div class="col-xs-6">
                                        <Rock:RockCheckBox ID="cbRequireCapacityRule" runat="server" Label="Required" Help="When checked a value for Group Capacity will be required on all groups of this type." Visible="false" />
                                    </div>
                                </div>

                                <Rock:RockCheckBox ID="cbGroupsRequireCampus" runat="server" Label="Groups Require a Campus"
                                    Help="This setting will require that all groups of this type have a campus when adding and editing." />
                                <Rock:RockDropDownList ID="ddlGroupStatusDefinedType" runat="server" Label="Group Status Defined Type" Help="Select the defined type to use when setting the group's status. Leave this blank if you don't want groups to prompt for group status." EnhanceForLongLists="true" />
                                <Rock:RockCheckBox ID="cbShowAdministrator" runat="server" Label="Show Administrator"
                                    Help="This setting determines if groups of this type support assigning an administrator for each group." />
                            </div>
                            <div class="col-md-6">
                                <div class="row">
                                    <div class="col-xs-6">
                                        <Rock:RockCheckBoxList ID="cblLocationSelectionModes" runat="server" Label="Location Selection Modes"
                                            Help="The location selection modes to allow when adding locations to groups of this type." />
                                    </div>
                                    <div class="col-xs-6">
                                        <Rock:RockCheckBox ID="cbAllowMultipleLocations" runat="server" Label="Multiple Locations"
                                            Help="Check this option if more than one location should be allowed for groups of this type." />
                                        <Rock:RockCheckBox ID="cbEnableLocationSchedules" runat="server" Label="Enable Location Schedules"
                                            Help="Check this option if group locations should be associated with one or more pre-defined schedules." />
                                    </div>
                                </div>
                                <Rock:RockControlWrapper ID="rcLocationTypes" runat="server" Label="Location Types"
                                    Help="Groups can have one or more location types attached to them.  For instance you may want to have a meeting location and an assignment target location.">
                                    <div class="grid">
                                        <Rock:Grid ID="gLocationTypes" runat="server" DisplayType="Light" ShowHeader="false" RowItemText="Location Type">
                                            <Columns>
                                                <Rock:RockBoundField DataField="Value" />
                                                <Rock:DeleteField OnClick="gLocationTypes_Delete" />
                                            </Columns>
                                        </Rock:Grid>
                                    </div>
                                </Rock:RockControlWrapper>
                                <Rock:RockCheckBox ID="cbDontInactivateMembers" runat="server" Label="Don't Inactivate Members"
                                    Help="By default, whenever a person record is inactivated, all of that person's group memberships are also inactivated. Check this option if members in groups of this type should not be inactivated when their person record is inactivated." />
                                <Rock:RockCheckBox ID="cbEnableIndexing" runat="server" Label="Enable Universal Search Indexing"
                                    Help="Determines if groups of this type should be indexed for Universal Search." />
                                <div class="row">
                                    <div class="col-xs-6">
                                        <Rock:RockCheckBox ID="cbAllowSpecificGroupMemberAttributes" runat="server" Label="Allow Specific Group Member Attributes"
                                            Help="Determines if groups of this type are allowed to have their own Group Member Attributes. This will show/hide the Member Attributes section on the Group Details block. If a group of this type already has specific group member attributes they will be kept." />
                                        <Rock:RockCheckBox ID="cbEnableSpecificGroupReq" runat="server" Label="Enable Specific Group Requirements"
                                            Help="Determines if groups of this type are allowed to have Group Requirements. If enabled, this will show the Group Requirements section on the Group Details block. If disabled, the Group Requirements section will still be shown if the group already has specific requirements." />
                                        <Rock:NotificationBox ID="nbGroupHistoryWarning" runat="server" NotificationBoxType="Warning" Text="Turning off group history will delete history for all groups and group members of this group type." Visible="false" />
                                        <Rock:RockCheckBox ID="cbEnableGroupHistory" runat="server" Label="Enable Group History"
                                            Help="Determines if groups of this type will keep a history of group and group member changes." AutoPostBack="true" OnCheckedChanged="cbEnableGroupHistory_CheckedChanged" />
                                    </div>
                                    <div class="col-xs-6">
                                        <Rock:RockCheckBox ID="cbAllowGroupSync" runat="server" Label="Allow Group Sync"
                                            Help="Determines if groups of this type are allowed have Group Syncs. This will show/hide the 'Group Sync Settings' section on the Group Details block. If a group of this type already has group syncs they will be kept. Unchecking this box will NOT prevent them from running." />
                                        <Rock:RockCheckBox ID="cbAllowSpecificGrpMemWorkFlows" runat="server" Label="Allow Specific Group Member Workflows"
                                            Help="Determines if groups of this type should be allowed to have Group Member Workflows. This would show/hide the 'Group Member Workflows' section on the Group Details block. If a group of this type already has specific group member workflows they will be kept." />
                                        <Rock:RockCheckBox ID="cbEnableGroupTag" runat="server" Label="Enable Group Tag"
                                            Help="Determines if groups of this type should be allowed to manage tags." />

                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:RockCheckBox ID="cbEnableInactiveReason" runat="server" Label="Enable Inactive Reason" Help="Allows a reason for inactivation to be selected. The reasons are setup in the Defined Type 'Inactive Group Reasons'" AutoPostBack="true" OnCheckedChanged="cbEnableInactiveReason_CheckedChanged" />
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:RockCheckBox ID="cbRequireInactiveReason" runat="server" Label="Require Inactive Reason" Help="Requires an Inactive Reason to be selected for the group when inactivating it. The reasons are setup in the Defined Type 'Inactive Group Reasons'" Enabled="false" />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </Rock:PanelWidget>

                    <%-- RSVP Settings --%>
                    <Rock:PanelWidget ID="wpRsvp" runat="server" Title="RSVP">
                        <asp:UpdatePanel runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:RockCheckBox ID="cbGroupRSVPEnabled" runat="server" Label="Group RSVP Enabled" AutoPostBack="true" OnCheckedChanged="cbRsvp_CheckedChanged"
                                                           Help="This option will allow group RSVP." />
                                    </div>
                                </div>
                                <asp:Panel runat="server" ID="pnlRsvpSettings">
                                    <div class="row">
                                        <div class="col-md-6">
                                            <Rock:RockDropDownList ID="ddlRsvpReminderSystemCommunication" runat="server" Label="RSVP Reminder System Communication"
                                                Help="The System Communication that should be sent to remind group members to RSVP for group events." />
                                        </div>
                                        <div class="col-md-6">
                                            <Rock:RangeSlider ID="rsRsvpReminderOffsetDays" runat="server" Label="RSVP Reminder Offset Days" MinValue="0" MaxValue="30" SelectedValue="1"
                                                Help="The number of days prior to a group event occurrence to send the RSVP reminder.  Set this value to 0 if you prefer to set this on the individual group." />
                                        </div>
                                    </div>
                                </asp:Panel>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpAttendanceCheckin" runat="server" Title="Attendance / Check-in">
                        <div class="row">
                            <div class="col-md-6">
                                <div class="row">
                                    <div class="col-xs-6">
                                        <Rock:RockCheckBox ID="cbTakesAttendance" runat="server" Label="Takes Attendance"
                                            Help="Check this option if groups of this type should support taking and tracking attendance." />
                                        <Rock:RockCheckBox ID="cbWeekendService" runat="server" Label="Weekend Service"
                                            Help="Check this option if attendance in groups of this type should be counted towards attending a weekend service." />
                                    </div>
                                    <div class="col-xs-6">
                                        <Rock:RockCheckBoxList ID="cblScheduleTypes" runat="server" Label="Group Schedule Options" Help="The schedule option types to allow when editing groups of this type." />
                                    </div>
                                </div>

                                <div class="row">
                                    <div class="col-xs-6">
                                        <Rock:RockCheckBox ID="cbGroupAttendanceRequiresLocation" runat="server" Label="Group Attendance Requires Location"
                                            Help="This option will require that all attendance occurrences have a location." />
                                    </div>
                                    <div class="col-xs-6">
                                        <Rock:RockCheckBox ID="cbGroupAttendanceRequiresSchedule" runat="server" Label="Group Attendance Requires Schedule"
                                            Help="This option will require that all attendance occurrences have a schedule." />
                                    </div>
                                </div>


                            </div>
                            <div class="col-md-6">
                                <Rock:RockControlWrapper ID="rcScheduleExclusions" runat="server" Label="Schedule Exclusions"
                                    Help="The date ranges that groups of this type do not meet (regardless of their individual schedules).">
                                    <div class="grid">
                                        <Rock:Grid ID="gScheduleExclusions" runat="server" DisplayType="Light" ShowHeader="false" RowItemText="Exclusion">
                                            <Columns>
                                                <asp:TemplateField>
                                                    <ItemTemplate>
                                                        <%# ((DateTime)Eval("Value.Start")).ToShortDateString() %> -
                                                        <%# ((DateTime)Eval("Value.End")).ToShortDateString() %>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <Rock:EditField OnClick="gScheduleExclusions_Edit" />
                                                <Rock:DeleteField OnClick="gScheduleExclusions_Delete" />
                                            </Columns>
                                        </Rock:Grid>
                                    </div>
                                </Rock:RockControlWrapper>
                                <Rock:RockDropDownList ID="ddlAttendanceRule" runat="server" Label="Check-in Rule"
                                    Help="The rule that check in should use when a person attempts to check in to a group of this type.  If 'None' is selected, user will not be added to group and is not required to belong to group.  If 'Add On Check In' is selected, user will be added to group if they don't already belong.  If 'Already Belongs' is selected, user must already be a member of the group or they will not be allowed to check in." />
                                <Rock:RockDropDownList ID="ddlPrintTo" runat="server" Label="Print Using"
                                    Help="When printing check-in labels, should the device's printer or the location's printer be used?  Note: the device has a similar setting which takes precedence over this setting.">
                                    <asp:ListItem Text="Device Printer" Value="1" />
                                    <asp:ListItem Text="Location Printer" Value="2" />
                                </Rock:RockDropDownList>
                            </div>

                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbSendAttendanceReminder" runat="server" Label="Send Attendance Reminder"
                                    AutoPostBack="true" OnCheckedChanged="cbSendAttendanceReminder_CheckedChanged"
                                    Help="Will enable the sending of automatic attendance reminders for all groups of this type." />

                                <Rock:RockDropDownList ID="ddlAttendanceReminderCommunication" runat="server" Label="Attendance Reminder Communication Template"
                                    Help="The communication template to use for sending attendance reminders." />

                                <Rock:NumberBox ID="nbAttendanceReminderOffsetMinutes" runat="server" Label="Attendance Reminder Start Offset Minutes"
                                    AppendText="minutes"
                                    Help="The number of minutes before the group starts that the reminder should be sent if &quot;Send Attendance Reminder&quot; is checked.  By default, the &quot;Send Group Attendance Reminders&quot; job runs every 15 minutes and will only send reminders after this time has past for a given group." />
                            </div>

                            <div class="col-md-6">
                                <Rock:RockCheckBoxList ID="cblAttendanceReminderFollowupDays" runat="server" Label="Attendance Reminder Follow-up Days" RepeatDirection="Horizontal" RepeatColumns="4"
                                    Help="A list of days after the occurrence that a reminder should be sent if attendance has not been entered. The reminder will be sent at the same time as the original reminder.">
                                    <asp:ListItem Text="1" Value="1" />
                                    <asp:ListItem Text="2" Value="2" />
                                    <asp:ListItem Text="3" Value="3" />
                                    <asp:ListItem Text="4" Value="4" />
                                    <asp:ListItem Text="5" Value="5" />
                                    <asp:ListItem Text="6" Value="6" />
                                    <asp:ListItem Text="7" Value="7" />
                                </Rock:RockCheckBoxList>
                            </div>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpScheduling" runat="server" Title="Scheduling">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbSchedulingEnabled" runat="server" Label="Scheduling Enabled" AutoPostBack="true" OnCheckedChanged="cbSchedulingEnabled_CheckedChanged" Help="Indicates whether scheduling is enabled for groups of this type."/>
                            </div>
                            <div class="col-md-6">
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockDropDownList ID="ddlScheduleConfirmationSystemCommunication" runat="server" Label="Schedule Confirmation Communication" Help="The system communication to use when a person is scheduled or when the schedule has been updated." />
                                <Rock:RockCheckBox ID="cbRequiresReasonIfDeclineSchedule" runat="server" Label="Requires Reason If Schedule Declined" Help="Indicates whether a person must specify a reason when declining/cancelling." />
                                <Rock:NumberBox ID="nbScheduleConfirmationOffsetDays" runat="server" NumberType="Integer" Label="Schedule Confirmation Offset Days" Help="The number of days prior to the schedule to send a confirmation notification." />
                                <Rock:RockDropDownList ID="ddlScheduleConfirmationLogic" runat="server" Label="Schedule Confirmation Logic" Help="Determines if the individual will be asked to Accept or Decline, or if their request will be auto accepted. This setting will be the default for all groups of this type." />
                            </div>
                            <div class="col-md-6">
                                <Rock:WorkflowTypePicker ID="wtpScheduleCancellationWorkflowType" runat="server" Label="Schedule Cancellation Workflow" Help="The workflow type to execute when a person indicates they won't be able to attend at their scheduled time." />

                                <Rock:RockDropDownList ID="ddlScheduleReminderSystemCommunication" runat="server" Label="Schedule Reminder Communication" Help="The system communication to use when sending a schedule reminder." />
                                <Rock:NumberBox ID="nbScheduleReminderOffsetDays" runat="server" NumberType="Integer" Label="Schedule Reminder Offset Days" Help="The default number of days prior to the schedule to send a reminder notification." />

                                <Rock:RockCheckBoxList ID="cblScheduleCoordinatorNotificationTypes" runat="server" Label="Schedule Coordinator Notification Options" RepeatDirection="Horizontal" Help='Specifies the types of notifications the coordinator receives regarding scheduled individuals. For example, selecting "Self-Schedule" notifies the coordinator when someone signs up for additional times via the Group Schedule Toolbox.'>
                                    <asp:ListItem Value="1" Text="Accept" />
                                    <asp:ListItem Value="2" Text="Decline" />
                                    <asp:ListItem Value="4" Text="Self-Schedule" />
                                </Rock:RockCheckBoxList>
                            </div>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpRoles" runat="server" Title="Roles">
                        <Rock:ModalAlert ID="mdGroupTypeRolesDeleteWarning" runat="server" />
                        <div class="grid">
                            <Rock:Grid ID="gGroupTypeRoles" runat="server" EnableResponsiveTable="false" AllowPaging="false" DisplayType="Light" RowItemText="Role" TooltipField="Description">
                                <Columns>
                                    <Rock:ReorderField />
                                    <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                                    <Rock:BoolField DataField="IsLeader" HeaderText="Is Leader" />
                                    <Rock:BoolField DataField="ReceiveRequirementsNotifications" HeaderText="Receives Requirements Notifications" />
                                    <Rock:BoolField DataField="CanView" HeaderText="Can View" />
                                    <Rock:BoolField DataField="CanEdit" HeaderText="Can Edit" />
                                    <Rock:BoolField DataField="CanManageMembers" HeaderText="Can Manage Members" />
                                    <Rock:RockBoundField DataField="MinCount" HeaderText="Minimum Required" DataFormatString="{0:N0}" />
                                    <Rock:RockBoundField DataField="MaxCount" HeaderText="Maximum Allowed" DataFormatString="{0:N0}" />
                                    <Rock:RockTemplateField HeaderText="Default">
                                        <ItemTemplate>
                                            <input type="radio" value='<%# Eval( "Guid" ) %>' name="GroupTypeDefaultRole" <%# ((Guid)Eval("Guid")).Equals(DefaultRoleGuid) ? "checked" : "" %> />
                                        </ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:EditField OnClick="gGroupTypeRoles_Edit" />
                                    <Rock:DeleteField OnClick="gGroupTypeRoles_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpGroupMemberAttributes" runat="server" Title="Member Attributes" CssClass="group-type-attribute-panel">
                        <Rock:NotificationBox ID="nbGroupMemberAttributes" runat="server" NotificationBoxType="Info"
                            Text="Member Attributes apply to all of the group members in every group of this type. Each member will have their own value for these attributes." />
                        <Rock:RockControlWrapper ID="rcGroupMemberAttributesInherited" runat="server" Label="Inherited Attribute(s)">
                            <div class="grid">
                                <Rock:Grid ID="gGroupMemberAttributesInherited" runat="server" AllowPaging="false" DisplayType="Light" ShowHeader="false" RowItemText="Inherited Member Attribute">
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
                        <div class="grid">
                            <Rock:Grid ID="gGroupMemberAttributes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Member Attribute">
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
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpGroupAttributes" runat="server" Title="Group Attributes" CssClass="group-type-attribute-panel">
                        <Rock:NotificationBox ID="NotificationBox1" runat="server" NotificationBoxType="Info"
                            Text="Group Attributes apply to all of the groups of this type. Each group will have its own value for these attributes." />
                        <Rock:RockControlWrapper ID="rcGroupAttributesInherited" runat="server" Label="Inherited Attribute(s)">
                            <div class="grid">
                                <Rock:Grid ID="gGroupAttributesInherited" runat="server" AllowPaging="false" DisplayType="Light" ShowHeader="false" RowItemText="Inherited Group Attribute">
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
                        <div class="grid">
                            <Rock:Grid ID="gGroupAttributes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Group Attribute">
                                <Columns>
                                    <Rock:ReorderField />
                                    <Rock:RockBoundField DataField="Name" HeaderText="Attribute" />
                                    <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                                    <Rock:BoolField DataField="IsRequired" HeaderText="Required" />
                                    <Rock:SecurityField TitleField="Name" />
                                    <Rock:EditField OnClick="gGroupAttributes_Edit" />
                                    <Rock:DeleteField OnClick="gGroupAttributes_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpGroupTypeAttributes" runat="server" Title="Group Type Attributes" CssClass="group-type-attribute-panel">
                        <Rock:NotificationBox ID="NotificationBox2" runat="server" NotificationBoxType="Info"
                            Text="Group Type Attributes apply to all of the groups of this type. Each group will have the same value equal to what is set as the default value here." />
                        <Rock:RockControlWrapper ID="rcGroupTypeAttributesInherited" runat="server" Label="Inherited Attribute(s)">
                            <div class="grid">
                                <Rock:Grid ID="gGroupTypeAttributesInherited" runat="server" AllowPaging="false" DisplayType="Light" ShowHeader="false" RowItemText="Inherited Group Type Attribute">
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
                        <div class="grid">
                            <Rock:Grid ID="gGroupTypeAttributes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Group Type Attribute">
                                <Columns>
                                    <Rock:ReorderField />
                                    <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                                    <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                                    <Rock:BoolField DataField="IsRequired" HeaderText="Required" />
                                    <Rock:SecurityField TitleField="Name" />
                                    <Rock:EditField OnClick="gGroupTypeAttributes_Edit" />
                                    <Rock:DeleteField OnClick="gGroupTypeAttributes_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                        <Rock:AttributeValuesContainer ID="avcEditAttributes" runat="server" />
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpGroupTypeGroupRequirements" runat="server" Title="Group Requirements">
                        <div class="grid">
                            <Rock:Grid ID="gGroupTypeGroupRequirements" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Group Requirement" ShowConfirmDeleteDialog="false">
                                <Columns>
                                    <Rock:RockBoundField DataField="GroupRequirementType.Name" HeaderText="Name" />
                                    <Rock:RockBoundField DataField="GroupRole" HeaderText="Group Role" />
                                    <Rock:RockBoundField DataField="AppliesToAgeClassification" HeaderText="Age Classification" />
                                    <Rock:RockLiteralField ID="lAppliesToDataViewId" runat="server" ItemStyle-HorizontalAlign="Center" HeaderText="Data View" OnDataBound="lAppliesToDataViewId_OnDataBound" />
                                    <Rock:BoolField DataField="MustMeetRequirementToAddMember" HeaderText="Required For New Members" />
                                    <Rock:BoolField DataField="GroupRequirementType.CanExpire" HeaderText="Can Expire" />
                                    <Rock:EnumField DataField="GroupRequirementType.RequirementCheckType" HeaderText="Type" />
                                    <Rock:EditField OnClick="gGroupTypeGroupRequirements_Edit" />
                                    <Rock:DeleteField OnClick="gGroupTypeGroupRequirements_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpMemberWorkflowTriggers" runat="server" Title="Group Member Workflows">
                        <Rock:NotificationBox ID="NotificationBox3" runat="server" NotificationBoxType="Info"
                            Text="The workflow(s) that should be launched when group members are changed in groups of this type." />
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

                    <Rock:PanelWidget ID="wpDisplay" runat="server" Title="Display Options">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbGroupTerm" runat="server" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="GroupTerm" Required="true"
                                    Help="The term to use for groups of this group type." />
                                <Rock:DataTextBox ID="tbGroupMemberTerm" runat="server" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="GroupMemberTerm" Required="true"
                                    Help="The term to use for members in groups of this group type." />
                                <Rock:DataTextBox ID="tbAdministratorTerm" runat="server" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="AdministratorTerm" Required="true"
                                    Help="This setting allows you to customize the term used for the administrator of the group." />
                                <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="IconCssClass" Label="Icon CSS Class"
                                    Help="The Font Awesome icon class to use when displaying groups of this group type." />
                                <Rock:ColorPicker ID="cpGroupTypeColor" runat="server" Label="Group Type Color"
                                    Help="The color used to visually distinguish groups on lists." />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbShowInGroupList" runat="server" Label="Show in Group Lists"
                                    Help="Check this option to include groups of this type in the GroupList block's list of groups." />
                                <Rock:RockCheckBox ID="cbShowInNavigation" runat="server" Label="Show in Navigation"
                                    Help="Check this option to include groups of this type in the GroupTreeView block's navigation control." />
                                <Rock:RockCheckBox ID="cbShowConnectionStatus" runat="server" Label="Show Connection Status"
                                    Help="Check this option to show the person's connection status as a column in the group member list." />
                                <Rock:RockCheckBox ID="cbShowMaritalStatus" runat="server" Label="Show Marital Status"
                                    Help="Check this option to show the person's marital status as a column in the group member list." />
                            </div>
                        </div>

                        <Rock:CodeEditor ID="ceGroupLavaTemplate" Visible="True" runat="server" Label="Group View Lava Template" EditorMode="Lava" EditorHeight="275" Help="This Lava template will be used by the Group Details block when viewing a group. This allows you to customize the layout of a group base on its type." />
                    </Rock:PanelWidget>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </div>
            </div>

        </asp:Panel>


        <Rock:ModalAlert ID="modalAlert" runat="server" />

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgGroupTypeRoles" runat="server" OnSaveClick="gGroupTypeRoles_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="Roles">
            <Content>
                <asp:HiddenField ID="hfRoleGuid" runat="server" />
                <asp:ValidationSummary ID="vsRoles" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="Roles" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbRoleName" runat="server" SourceTypeName="Rock.Model.GroupTypeRole, Rock" PropertyName="Name" ValidationGroup="Roles" />
                    </div>
                    <div class="col-md-6">
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-12">
                        <Rock:DataTextBox ID="tbRoleDescription" runat="server" SourceTypeName="Rock.Model.GroupTypeRole, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" ValidationGroup="Roles" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbIsLeader" runat="server" Label="Is Leader" Help="Are people with this role in group considered a 'Leader' of the group?" />
                        <Rock:RockCheckBox ID="cbReceiveRequirementsNotifications" runat="server" Label="Receive Requirements Notifications" Help="Should this role receive notifications of group members who do not meet their requirements? In order for these notifications to be sent you will need to setup a 'Process Group Requirements Notification Job'." />
                        <Rock:RockCheckBox ID="cbCanView" runat="server" Label="Can View" Help="Should users with this role be able to view this group regardless of the security settings on the group?" />
                        <Rock:RockCheckBox ID="cbCanEdit" runat="server" Label="Can Edit" Help="Should users with this role be able to edit the details and members of this group regardless of the security settings on the group?" />
                        <Rock:RockCheckBox ID="cbCanManageMembers" runat="server" Label="Can Manage Members" Help="Should users with this role be able to manage the members of this group regardless of the security settings on the group?" />
                        <Rock:RockCheckBox ID="cbIsCheckInAllowed" runat="server" Label="Is Check-in Allowed" Help="Used with the &quot;already belongs&quot; check-in filter to only allow certain roles to check into a group." />
                    </div>
                    <div class="col-md-6">
                        <Rock:NumberBox ID="nbMinimumRequired" runat="server" NumberType="Integer" Label="Minimum Required" Help="The minimum number of people with this role that group should allow." />
                        <Rock:NumberBox ID="nbMaximumAllowed" runat="server" NumberType="Integer" Label="Maximum Allowed" Help="The maximum number of people with this role that group should allow." />
                        <asp:CustomValidator ID="cvAllowed" runat="server" Display="None" OnServerValidate="cvAllowed_ServerValidate"
                            ValidationGroup="Roles" ErrorMessage="The Minimum Required should be less than Maximum Allowed." />
                        <asp:PlaceHolder ID="phGroupTypeRoleAttributes" runat="server" />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgChildGroupType" runat="server" OnSaveClick="dlgChildGroupType_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="ChildGroupTypes">
            <Content>
                <Rock:RockDropDownList ID="ddlChildGroupType" runat="server" DataTextField="Name" DataValueField="Id" Label="Child Group Type" ValidationGroup="ChildGroupTypes" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgScheduleExclusion" runat="server" OnSaveClick="dlgScheduleExclusion_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="ScheduleExclusion">
            <Content>
                <asp:HiddenField ID="hfScheduleExclusion" runat="server" />
                <Rock:DateRangePicker ID="drpScheduleExclusion" runat="server" Label="Schedule Exclusion" ValidationGroup="ScheduleExclusion" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgLocationType" runat="server" OnSaveClick="dlgLocationType_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="LocationType">
            <Content>
                <Rock:RockDropDownList ID="ddlLocationType" runat="server" DataTextField="Value" DataValueField="Id" Label="Location Type" ValidationGroup="LocationType" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgGroupTypeAttribute" runat="server" Title="Group Type Attributes" OnSaveClick="dlgGroupTypeAttribute_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="GroupTypeAttributes">
            <Content>
                <Rock:AttributeEditor ID="edtGroupTypeAttributes" runat="server" ShowActions="false" ValidationGroup="GroupTypeAttributes" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgGroupAttribute" runat="server" Title="Group Attributes" OnSaveClick="dlgGroupAttribute_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="GroupAttributes">
            <Content>
                <Rock:AttributeEditor ID="edtGroupAttributes" runat="server" ShowActions="false" ValidationGroup="GroupAttributes" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgGroupMemberAttribute" runat="server" Title="Group Member Attributes" OnSaveClick="dlgGroupMemberAttribute_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="GroupMemberttributes">
            <Content>
                <Rock:AttributeEditor ID="edtGroupMemberAttributes" runat="server" ShowActions="false" ValidationGroup="GroupMemberttributes" />
            </Content>
        </Rock:ModalDialog>

        <!-- GroupType Group Requirements Modal Dialog -->
        <Rock:ModalDialog ID="mdGroupTypeGroupRequirement" runat="server" Title="Group Requirement" OnSaveClick="mdGroupTypeGroupRequirement_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="vg_GroupTypeGroupRequirement">
            <Content>
                <asp:HiddenField ID="hfGroupTypeGroupRequirementGuid" runat="server" />

                <Rock:NotificationBox ID="nbDuplicateGroupRequirement" runat="server" NotificationBoxType="Warning" />

                <asp:ValidationSummary ID="vsGroupTypeGroupRequirement" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="vg_GroupTypeGroupRequirement" />

                <Rock:RockDropDownList ID="ddlGroupRequirementType" runat="server" Label="Group Requirement Type" Required="true" AutoPostBack="true" OnSelectedIndexChanged="ddlGroupRequirementType_SelectedIndexChanged" ValidationGroup="vg_GroupTypeGroupRequirement" />

                <Rock:GroupRolePicker ID="grpGroupRequirementGroupRole" runat="server" Label="Applies to Group Role" Help="Select the group role that this requirement applies to. Leave blank if it applies to all group roles." ValidationGroup="vg_GroupTypeGroupRequirement" />

                <Rock:RockRadioButtonList ID="rblAppliesToAgeClassification" runat="server" Label="Applies to Age Classification" RepeatDirection="Horizontal" Help="Determines which age classifications this requirement applies to."></Rock:RockRadioButtonList>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataViewItemPicker ID="dvpAppliesToDataView" runat="server" Label="Applies to Data View" EntityTypeId="15" Help="An optional data view to determine who the requirement applies to." />
                    </div>
                </div>

                <Rock:RockCheckBox ID="cbAllowLeadersToOverride" runat="server" Text="Allow Leaders to Override" Help="Determines if the leader should be allowed to override meeting the requirement." />

                <Rock:DatePicker ID="dpDueDate" runat="server" Label="Due Date" ValidationGroup="vg_GroupTypeGroupRequirement" />
                <Rock:RockDropDownList ID="ddlDueDateGroupAttribute" runat="server" Label="Due Date Group Attribute" Help="The group attribute that contains the due date for requirements." ValidationGroup="vg_GroupTypeGroupRequirement" />

                <Rock:RockCheckBox ID="cbMembersMustMeetRequirementOnAdd" runat="server" Text="Members must meet this requirement before adding" Help="If this is enabled, a person can only become a group member if this requirement is met. Note: only applies to Data View and SQL type requirements since manual ones can't be checked until after the person is added." />
            </Content>
        </Rock:ModalDialog>

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
                            Help="Select this option if workflow should only be started when a person attends a group of this type for the first time. Leave this option unselected if the workflow should be started whenever a person attends a group of this type." />
                        <Rock:RockCheckBox ID="cbTriggerPlacedElsewhereShowNote" runat="server" Label="Show Note" ValidationGroup="Trigger"
                            Help="Select this option if workflow should show UI for entering a note when the member is placed." />
                        <Rock:RockCheckBox ID="cbTriggerPlacedElsewhereRequireNote" runat="server" Label="Require Note" ValidationGroup="Trigger"
                            Help="Select this option if workflow should show UI for entering a note and make it required when the member is placed." />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>


    </ContentTemplate>
</asp:UpdatePanel>
