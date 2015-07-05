<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupDetail.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upnlGroupDetail" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server">
            <asp:HiddenField ID="hfGroupId" runat="server" />

            <div class="panel panel-block">

                <div class="panel-heading clearfix">
                    <h1 class="panel-title pull-left">
                        <asp:Literal ID="lGroupIconHtml" runat="server" />
                        <asp:Literal ID="lReadOnlyTitle" runat="server" />
                    </h1>

                    <div class="panel-labels">
                        <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                        <Rock:HighlightLabel ID="hlIsPrivate" runat="server" LabelType="Default" Text="Private" />
                        <Rock:HighlightLabel ID="hlType" runat="server" LabelType="Type" />
                        <Rock:HighlightLabel ID="hlCampus" runat="server" LabelType="Campus" />
                    </div>
                </div>
                
                <div class="panel-body">
                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    <Rock:NotificationBox ID="nbRoleLimitWarning" runat="server" NotificationBoxType="Warning" Heading="Role Limit Warning" />
                    <Rock:NotificationBox ID="nbNotAllowedToEdit" runat="server" NotificationBoxType="Danger" Visible="false"
                        Text="You are not authorized to save group with the selected group type and/or parent group." />
                    <Rock:NotificationBox ID="nbInvalidParentGroup" runat="server" NotificationBoxType="Danger" Visible="false"
                        Text="The selected parent group does not allow child groups of the selected group type." />
                    <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <div id="pnlEditDetails" runat="server">

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.Group, Rock" PropertyName="Name" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbIsActive" runat="server" Text="Active" />
                                <Rock:RockCheckBox ID="cbIsPublic" runat="server" Text="Public" />
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
                                    <Rock:DataDropDownList ID="ddlGroupType" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Name" Label="Group Type" AutoPostBack="true" OnSelectedIndexChanged="ddlGroupType_SelectedIndexChanged" />
                                    <Rock:RockLiteral ID="lGroupType" runat="server" Label="Group Type" />
                                    <Rock:GroupPicker ID="gpParentGroup" runat="server" Required="false" Label="Parent Group" OnSelectItem="ddlParentGroup_SelectedIndexChanged" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:DataDropDownList ID="ddlCampus" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.Campus, Rock" PropertyName="Name" Label="Campus" />
                                    <Rock:RockCheckBox ID="cbIsSecurityRole" runat="server" Label="Security Role" Text="Yes" />
                                </div>
                            </div>
                        </Rock:PanelWidget>

                        <Rock:PanelWidget ID="wpMeetingDetails" runat="server" Title="Meeting Details">
                            <div class="grid">
                                <Rock:Grid ID="gLocations" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Location">
                                    <Columns>
                                        <Rock:RockBoundField DataField="Location" HeaderText="Location" />
                                        <Rock:RockBoundField DataField="Type" HeaderText="Type" />
                                        <Rock:RockBoundField DataField="Schedules" HeaderText="Schedule(s)" />
                                        <Rock:EditField OnClick="gLocations_Edit" />
                                        <Rock:DeleteField OnClick="gLocations_Delete" />
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

                        <Rock:PanelWidget ID="wpGroupAttributes" runat="server" Title="Group Attribute Values">
                            <asp:PlaceHolder ID="phGroupAttributes" runat="server" EnableViewState="false"></asp:PlaceHolder>
                        </Rock:PanelWidget>

                        <Rock:PanelWidget ID="wpGroupMemberAttributes" runat="server" Title="Member Attributes" CssClass="group-type-attribute-panel">
                            <Rock:NotificationBox ID="nbGroupMemberAttributes" runat="server" NotificationBoxType="Info"
                                Text="Member Attributes apply to members in this group.  Each member will have their own value for these attributes" />
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
                                <Rock:Grid ID="gGroupMemberAttributes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Member Attribute" ShowConfirmDeleteDialog="false">
                                    <Columns>
                                        <Rock:ReorderField />
                                        <Rock:RockBoundField DataField="Name" HeaderText="Attribute" />
                                        <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                                        <Rock:BoolField DataField="IsRequired" HeaderText="Required" />
                                        <Rock:EditField OnClick="gGroupMemberAttributes_Edit" />
                                        <Rock:DeleteField OnClick="gGroupMemberAttributes_Delete" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </Rock:PanelWidget>

                        <Rock:PanelWidget ID="wpGroupRequirements" runat="server" Title="Group Requirements">
                            <Rock:RockCheckBox ID="cbMembersMustMeetRequirementsOnAdd" runat="server" Text="Members must meet all requirements before adding" Help="If this is enabled, a person can only become a group member if all the requirements are met. If this is left as disabled, requirements won't be checked when adding. Note: only Data View and SQL type requirements need to be met since manual ones can't be checked until after the person is added." />
                            <div class="grid">
                                <Rock:Grid ID="gGroupRequirements" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Group Requirement" ShowConfirmDeleteDialog="false">
                                    <Columns>
                                        <Rock:RockBoundField DataField="GroupRequirementType.Name" HeaderText="Name" />
                                        <Rock:RockBoundField DataField="GroupRole" HeaderText="Group Role" />
                                        <Rock:BoolField DataField="GroupRequirementType.CanExpire" HeaderText="Can Expire" />
                                        <Rock:EnumField DataField="GroupRequirementType.RequirementCheckType" HeaderText="Type" />
                                        <Rock:EditField OnClick="gGroupRequirements_Edit" />
                                        <Rock:DeleteField OnClick="gGroupRequirements_Delete" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </Rock:PanelWidget>

                        <Rock:PanelWidget ID="wpGroupSync" runat="server" Title="Group Sync Settings">
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:DataViewPicker ID="dvpSyncDataview" Label="Sync Data View" runat="server"></Rock:DataViewPicker>
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockCheckBox ID="rbCreateLoginDuringSync" runat="server" Label="Create Login During Sync" Help="If the individual does not have a login should one be created during the sync process?" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockDropDownList ID="ddlWelcomeEmail" runat="server" Label="Welcome Email"></Rock:RockDropDownList>
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockDropDownList ID="ddlExitEmail" runat="server" Label="Exit Email"></Rock:RockDropDownList>
                                </div>
                            </div>
                        </Rock:PanelWidget>

                        <Rock:PanelWidget ID="wpMemberWorkflowTriggers" runat="server" Title="Group Member Workflows" >
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
                            <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                            <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                        </div>

                    </div>

                    <fieldset id="fieldsetViewDetails" runat="server">

                        <p class="description">
                            <asp:Literal ID="lGroupDescription" runat="server"></asp:Literal>
                        </p>

                        <div class="row">
                            <div class="col-md-6">
                                <div class="row">
                                    <div class="col-sm-6">
                                        <asp:Literal ID="lblMainDetails" runat="server" />
                                    </div>
                                    <div class="col-sm-6">
                                        <asp:PlaceHolder ID="phAttributes" runat="server"></asp:PlaceHolder>
                                    </div>
                                </div>
                                <Rock:RockControlWrapper id="rcwLinkedRegistrations" runat="server" Label="Linked Registrations">
                                    <ul class="list-unstyled">
                                        <asp:Repeater ID="rptLinkedRegistrations" runat="server">
                                            <ItemTemplate>
                                                <li><a href='<%# RegistrationInstanceUrl( (int)Eval("RegistrationInstanceId" ) ) %>'><%# Eval("Title") %></a></li>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </ul>
                                </Rock:RockControlWrapper>
                            </div>
                            <div class="col-md-6 location-maps">
                                <asp:PlaceHolder ID="phMaps" runat="server" />
                            </div>
                        </div>


                        <div class="actions">
                            <asp:LinkButton ID="btnEdit" runat="server" AccessKey="m" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                            <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                            <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" CausesValidation="false" />
                            <span class="pull-right">
                                <asp:HyperLink ID="hlAttendance" runat="server" CssClass="btn btn-sm btn-default" ToolTip="Attendance"><i class="fa fa-check-square-o"></i></asp:HyperLink>
                                <asp:HyperLink ID="hlMap" runat="server" CssClass="btn btn-sm btn-default" ToolTip="Interactive Map"><i class="fa fa-map-marker"></i></asp:HyperLink>
                                <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-security" Title="Secure Group" />
                            </span>
                        </div>

                    </fieldset>
                </div>
            </div>

        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <!-- Group Member Attribute Modal Dialog -->
        <Rock:ModalDialog ID="dlgGroupMemberAttribute" runat="server" Title="Group Member Attributes" OnSaveClick="dlgGroupMemberAttribute_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="GroupMemberAttribute">
            <Content>
                <Rock:AttributeEditor ID="edtGroupMemberAttributes" runat="server" ShowActions="false" ValidationGroup="GroupMemberAttribute" />
            </Content>
        </Rock:ModalDialog>

        <!-- Locations Modal Dialog -->
        <Rock:ModalDialog ID="dlgLocations" runat="server" Title="Group Location" OnSaveClick="dlgLocations_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="Location">
            <Content>

                <asp:HiddenField ID="hfAddLocationGroupGuid" runat="server" />

                <asp:ValidationSummary ID="valLocationSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="Location" />

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
                        <Rock:LocationPicker ID="locpGroupLocation" runat="server" Label="Location" ValidationGroup="Location" />
                    </asp:Panel>
                </div>

                <Rock:RockDropDownList ID="ddlLocationType" runat="server" Label="Type" DataValueField="Id" DataTextField="Value" ValidationGroup="Location" />

                <Rock:SchedulePicker ID="spSchedules" runat="server" Label="Schedule(s)" ValidationGroup="Location" AllowMultiSelect="true" />

            </Content>
        </Rock:ModalDialog>

        <!-- Group Requirements Modal Dialog -->
        <Rock:ModalDialog ID="mdGroupRequirement" runat="server" Title="Group Requirement" OnSaveClick="mdGroupRequirement_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="vg_GroupRequirement">
            <Content>
                <asp:HiddenField ID="hfGroupRequirementGuid" runat="server" />

                <Rock:NotificationBox id="nbDuplicateGroupRequirement" runat="server" NotificationBoxType="Warning" />

                <asp:ValidationSummary ID="vsGroupRequirement" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="vg_GroupRequirement" />

                <Rock:RockDropDownList ID="ddlGroupRequirementType" runat="server" Label="Group Requirement Type" Required="true" ValidationGroup="vg_GroupRequirement"/>

                <Rock:GroupRolePicker ID="grpGroupRequirementGroupRole" runat="server" Label="Group Role" Help="Select the group role that this requirement applies to. Leave blank if it applies to all group roles." ValidationGroup="vg_GroupRequirement" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgMemberWorkflowTriggers" runat="server" OnSaveClick="dlgMemberWorkflowTriggers_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="Trigger">
            <Content>
                <asp:HiddenField ID="hfTriggerGuid" runat="server" />
                <asp:ValidationSummary ID="vsTrigger" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="Trigger" />
                <Rock:NotificationBox ID="nbInvalidWorkflowType" runat="server" NotificationBoxType="Danger" Visible="false"
                    Text="The Workflow Type is missing or invalid. Make sure you selected a valid Workflow Type (and not a category)." />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbTriggerName" runat="server" Label="Name" Required="true" ValidationGroup="Trigger" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbTriggerIsActive" runat="server" Text="Active" ValidationGroup="Trigger"  />
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
                            Help="Select this option if workflow should only be started when person attends the group for the first time. Leave this option unselected if the workflow should be started whenever a person attends the group."/>
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
