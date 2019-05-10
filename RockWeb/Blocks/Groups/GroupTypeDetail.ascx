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
                                <Rock:RockControlWrapper ID="rcGroupTypes" runat="server" Label="Child Group Types"
                                    Help="The types of child groups that can be added to groups of this type. This is used to define the group hierarchy. To allow an unlimited hierarchy add this type as an allowed child group type.">
                                    <div class="grid">
                                        <Rock:Grid ID="gChildGroupTypes" runat="server" DisplayType="Light" ShowHeader="false" RowItemText="Group Type">
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
                                        <Rock:RockDropDownList ID="ddlGroupCapacityRule" runat="server" Label="Group Capacity Rule" Help="Does this group type support group capacity and if so how is it enforced." />
                                    </div>
                                    <div class="col-xs-6">
                                    </div>
                                </div>

                                <Rock:RockCheckBox ID="cbGroupsRequireCampus" runat="server" Label="Groups Require a Campus" Text="Yes"
                                    Help="This setting will require that all groups of this type have a campus when adding and editing." />
                                <Rock:RockDropDownList ID="ddlGroupStatusDefinedType" runat="server" Label="Group Status Defined Type" Help="Select the defined type to use when setting the group's status. Leave this blank if you don't want groups to prompt for group status." EnhanceForLongLists="true" />
                                <Rock:RockCheckBox ID="cbShowAdministrator" runat="server" Label="Show Administrator" Text="Yes"
                                    Help="This setting determines if groups of this type support assigning an administrator for each group." />
                            </div>
                            <div class="col-md-6">
                                <div class="row">
                                    <div class="col-xs-6">
                                        <Rock:RockCheckBoxList ID="cblLocationSelectionModes" runat="server" Label="Location Selection Modes"
                                            Help="The location selection modes to allow when adding locations to groups of this type." />
                                    </div>
                                    <div class="col-xs-6">
                                        <Rock:RockCheckBox ID="cbAllowMultipleLocations" runat="server" Label="Multiple Locations" Text="Allow"
                                            Help="Check this option if more than one location should be allowed for groups of this type." />
                                        <Rock:RockCheckBox ID="cbEnableLocationSchedules" runat="server" Label="Enable Location Schedules" Text="Yes"
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
                                <Rock:RockCheckBox ID="cbEnableIndexing" runat="server" Label="Enable Indexing"
                                    Help="Determines if groups of this type should be indexed." />
                                <div class="row">
                                    <div class="col-xs-6">
                                        <Rock:RockCheckBox ID="cbAllowSpecificGroupMemberAttributes" runat="server" Label="Allow Specific Group Member Attributes"
                                            Help="Determines if groups of this type are allowed to have their own Group Member Attributes. This will show/hide the Member Attributes section on the Group Details block. If a group of this type already has specific group member attributes they will be kept." />
                                        <Rock:RockCheckBox ID="cbEnableSpecificGroupReq" runat="server" Label="Enable Specific Group Requirements"
                                            Help="Determines if groups of this type are allowed to have Group Requirements. This will show/hide the Group Requirements section on the Group Details block. If a group of this type already has specific group member attributes they will be kept." />
                                        <Rock:NotificationBox ID="nbGroupHistoryWarning" runat="server" NotificationBoxType="Warning" Text="Turning off group history will delete history for all groups and group members of this group type." Visible="false" />
                                        <Rock:RockCheckBox ID="cbEnableGroupHistory" runat="server" Label="Enable Group History"
                                            Help="Determines if groups of this type will keep a history of group and group member changes." AutoPostBack="true" OnCheckedChanged="cbEnableGroupHistory_CheckedChanged" />
                                    </div>
                                    <div class="col-xs-6">
                                        <Rock:RockCheckBox ID="cbAllowGroupSync" runat="server" Label="Allow Group Sync"
                                            Help="Determines if groups of this type are allowed have Group Syncs. This will show/hide the 'Group Sync Settings' section on the Group Details block. If a group of this type already has group syncs the will be kept. Unchecking this box will NOT prevent them from running." />
                                        <Rock:RockCheckBox ID="cbAllowSpecificGrpMemWorkFlows" runat="server" Label="Allow Specific Group Member Workflows"
                                            Help="Determines if groups of this type should be allowed to have Group Member Workflows. This would show/hide the 'Group Member Workflows' section on the Group Details block. If a group of this type already has specific group member workflows they will be kept." />
                                        <Rock:RockCheckBox ID="cbEnableGroupTag" runat="server" Label="Enable Group Tag" 
                                            Help="Determines if groups of this type should be allowed to manage tags." />

                                    </div>
                        </div>
                            </div>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpAttendanceCheckin" runat="server" Title="Attendance / Check-in">
                        <div class="row">
                            <div class="col-md-6">
                                <div class="row">
                                    <div class="col-xs-6">
                                        <Rock:RockCheckBox ID="cbTakesAttendance" runat="server" Label="Takes Attendance" Text="Yes"
                                            Help="Check this option if groups of this type should support taking and tracking attendance." />
                                        <Rock:RockCheckBox ID="cbWeekendService" runat="server" Label="Weekend Service" Text="Yes"
                                            Help="Check this option if attendance in groups of this type should be counted towards attending a weekend service." />
                                        <Rock:RockCheckBox ID="cbSendAttendanceReminder" runat="server" Label="Send Attendance Reminder" Text="Yes"
                                            Help="Check this option if an email should be sent to the group leaders of these group types reminding them to enter attendance information." />
                                    </div>
                                    <div class="col-xs-6">
                                        <Rock:RockCheckBoxList ID="cblScheduleTypes" runat="server" Label="Group Schedule Options" Help="The schedule option types to allow when editing groups of this type." />
                                    </div>
                                </div>

                                <div class="row">
                                    <div class="col-xs-6">
                                        <Rock:RockCheckBox ID="cbGroupAttendanceRequiresLocation" runat="server" Label="Group Attendance Requires Location" Text="Yes"
                                            Help="This option will require that all attendance occurrences have a location." />
                                    </div>
                                    <div class="col-xs-6">
                                        <Rock:RockCheckBox ID="cbGroupAttendanceRequiresSchedule" runat="server" Label="Group Attendance Requires Schedule" Text="Yes"
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
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpScheduling" runat="server" Title="Scheduling">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbSchedulingEnabled" runat="server" Label="Scheduling Enabled" Help="Indicates whether scheduling is enabled for groups of this type."/>
                            </div>
                            <div class="col-md-6">
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockDropDownList ID="ddlScheduleConfirmationSystemEmail" runat="server" Label="Schedule Confirmation Email" Help="The system email to use when a person is scheduled or when the schedule has been updated." />
                                <Rock:RockCheckBox ID="cbRequiresReasonIfDeclineSchedule" runat="server" Label="Requires Reason if Decline Schedule" Help="Indicates whether a person must specify a reason when declining/cancelling." />
                                <Rock:NumberBox ID="nbScheduleConfirmationEmailOffsetDays" runat="server" NumberType="Integer" Label="Schedule Confirmation Email Offset Days" Help="The number of days prior to the schedule to send a confirmation email." />
                            </div>
                            <div class="col-md-6">
                                <Rock:WorkflowTypePicker ID="wtpScheduleCancellationWorkflowType" runat="server" Label="Schedule Cancellation Workflow Type" Help="The workflow type to execute when a person indicates they won't be able to attend at their scheduled time." />

                                <Rock:RockDropDownList ID="ddlScheduleReminderSystemEmail" runat="server" Label="Schedule Reminder Email" Help="The system email to use when sending a schedule reminder." />
                                <Rock:NumberBox ID="nbScheduleReminderEmailOffsetDays" runat="server" NumberType="Integer" Label="Schedule Reminder Email Offset Days" Help="The default number of days prior to the schedule to send a reminder email." />
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
                            Text="Member Attributes apply to all of the group members in every group of this type.  Each member will have their own value for these attributes" />
                        <Rock:RockControlWrapper ID="rcGroupMemberAttributesInherited" runat="server" Label="Inherited Attribute(s)">
                            <div class="grid">
                                <Rock:Grid ID="gGroupMemberAttributesInherited" runat="server" AllowPaging="false" DisplayType="Light" ShowHeader="false" RowItemText="Inherited Member Attribute">
                                    <Columns>
                                        <Rock:RockBoundField DataField="Name" />
                                        <Rock:RockBoundField DataField="Description" />
                                        <Rock:RockTemplateField>
                                            <ItemTemplate>(Inherited from <a href='<%# Eval("Url") %>' target='_blank'><%# Eval("GroupType") %></a>)</ItemTemplate>
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
                            Text="Group Attributes apply to all of the groups of this type.  Each group will have its own value for these attributes" />
                        <Rock:RockControlWrapper ID="rcGroupAttributesInherited" runat="server" Label="Inherited Attribute(s)">
                            <div class="grid">
                                <Rock:Grid ID="gGroupAttributesInherited" runat="server" AllowPaging="false" DisplayType="Light" ShowHeader="false" RowItemText="Inherited Group Attribute">
                                    <Columns>
                                        <Rock:RockBoundField DataField="Name" />
                                        <Rock:RockBoundField DataField="Description" />
                                        <Rock:RockTemplateField>
                                            <ItemTemplate>(Inherited from <a href='<%# Eval("Url") %>' target='_blank'><%# Eval("GroupType") %></a>)</ItemTemplate>
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
                            Text="Group Type Attributes apply to all of the groups of this type.  Each group will have the same value equal to what is set as the default value here." />
                        <Rock:RockControlWrapper ID="rcGroupTypeAttributesInherited" runat="server" Label="Inherited Attribute(s)">
                            <div class="grid">
                                <Rock:Grid ID="gGroupTypeAttributesInherited" runat="server" AllowPaging="false" DisplayType="Light" ShowHeader="false" RowItemText="Inherited Group Type Attribute">
                                    <Columns>
                                        <Rock:RockBoundField DataField="Name" />
                                        <Rock:RockBoundField DataField="Description" />
                                        <Rock:RockTemplateField>
                                            <ItemTemplate>(Inherited from <a href='<%# Eval("Url") %>' target='_blank'><%# Eval("GroupType") %></a>)</ItemTemplate>
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
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpGroupTypeGroupRequirements" runat="server" Title="Group Requirements">
                        <div class="grid">
                            <Rock:Grid ID="gGroupTypeGroupRequirements" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Group Requirement" ShowConfirmDeleteDialog="false">
                                <Columns>
                                    <Rock:RockBoundField DataField="GroupRequirementType.Name" HeaderText="Name" />
                                    <Rock:RockBoundField DataField="GroupRole" HeaderText="Group Role" />
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
                                <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="IconCssClass"
                                    Help="The Font Awesome icon class to use when displaying groups of this group type." />
                                <Rock:ColorPicker ID="cpGroupTypeColor" runat="server" Label="Group Type Color"
                                    Help="The color used to visually distinguish groups on lists." />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbShowInGroupList" runat="server" Label="Show in Group Lists" Text="Yes"
                                    Help="Check this option to include groups of this type in the GroupList block's list of groups." />
                                <Rock:RockCheckBox ID="cbShowInNavigation" runat="server" Label="Show in Navigation" Text="Yes"
                                    Help="Check this option to include groups of this type in the GroupTreeView block's navigation control." />
                                <Rock:RockCheckBox ID="cbShowConnectionStatus" runat="server" Label="Show Connection Status" Text="Yes"
                                    Help="Check this option to show the person's connection status as a column in the group member list." />
                                <Rock:RockCheckBox ID="cbShowMaritalStatus" runat="server" Label="Show Marital Status" Text="Yes"
                                    Help="Check this option to show the person's marital status as a column in the group member list." />
                            </div>
                        </div>

                        <Rock:CodeEditor ID="ceGroupLavaTemplate" Visible="True" runat="server" Label="Group View Lava Template" EditorMode="Lava" EditorHeight="275" Help="This Lava template will be used by the Group Details block when viewing a group. This allows you to customize the layout of a group base on its type." />
                    </Rock:PanelWidget>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
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
                        <Rock:RockCheckBox ID="cbIsLeader" runat="server" Label="Is Leader" Text="Yes" Help="Are people with this role in group considered a 'Leader' of the group?" />
                        <Rock:RockCheckBox ID="cbReceiveRequirementsNotifications" runat="server" Label="Receive Requirements Notifications" Text="Yes" Help="Should this role receive notifications of group members who do not meet their requirements? In order for these notifications to be emailed you will need to setup a 'Process Group Requirements Notification Job'." />
                        <Rock:RockCheckBox ID="cbCanView" runat="server" Label="Can View" Text="Yes" Help="Should users with this role be able to view this group regardless of the security settings on the group?" />
                        <Rock:RockCheckBox ID="cbCanEdit" runat="server" Label="Can Edit" Text="Yes" Help="Should users with this role be able to edit the details and members of this group regardless of the security settings on the group?" />
                        <Rock:RockCheckBox ID="cbCanManageMembers" runat="server" Label="Can Manage Members" Text="Yes" Help="Should users with this role be able to manage the members of this group regardless of the security settings on the group?" />
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

                <Rock:RockDropDownList ID="ddlGroupRequirementType" runat="server" Label="Group Requirement Type" Required="true" ValidationGroup="vg_GroupTypeGroupRequirement" />

                <Rock:GroupRolePicker ID="grpGroupRequirementGroupRole" runat="server" Label="Group Role" Help="Select the group role that this requirement applies to. Leave blank if it applies to all group roles." ValidationGroup="vg_GroupTypeGroupRequirement" />

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
                        <Rock:RockCheckBox ID="cbTriggerIsActive" runat="server" Text="Active" ValidationGroup="Trigger" />
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
                        <Rock:RockCheckBox ID="cbTriggerFirstTime" runat="server" Label="First Time" Text="Yes" ValidationGroup="Trigger"
                            Help="Select this option if workflow should only be started when a person attends a group of this type for the first time. Leave this option unselected if the workflow should be started whenever a person attends a group of this type." />
                        <Rock:RockCheckBox ID="cbTriggerPlacedElsewhereShowNote" runat="server" Label="Show Note" Text="Yes" ValidationGroup="Trigger"
                            Help="Select this option if workflow should show UI for entering a note when the member is placed." />
                        <Rock:RockCheckBox ID="cbTriggerPlacedElsewhereRequireNote" runat="server" Label="Require Note" Text="Yes" ValidationGroup="Trigger"
                            Help="Select this option if workflow should show UI for entering a note and make it required when the member is placed." />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>


    </ContentTemplate>
</asp:UpdatePanel>
