<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DataAutomationSettings.ascx.cs" Inherits="RockWeb.Blocks.Administration.DataAutomationSettings" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-tachometer"></i>
                    Data Automation Settings
                </h1>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />

                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <Rock:PanelWidget ID="pwGeneralSettings" runat="server" Title="General Settings">
                    <Rock:NumberBox ID="nbGenderAutoFill" runat="server" AppendText="%" CssClass="input-width-md" Label="Gender AutoFill Confidence" MinimumValue="0" MaximumValue="100" NumberType="Double" Help="The minimum confidence level required to automatically set blank genders in the Data Automation service job. If set to 0 then gender will not be automatically determined." />
                </Rock:PanelWidget>

                <fieldset>
                    <legend>
                        Data Automation
                    </legend>

                    <Rock:PanelWidget ID="pwReactivatePeople" runat="server" title="Reactivate People">

                        <Rock:RockCheckBox ID="cbReactivatePeople" runat="server"
                            Label="Enable" Text="Enable the automatic activating of individuals who are currently inactive and who meet any of the following selected criteria. Note: This will not include people who were inactivated for a reason that was configured to not allow automatic reactivation."
                            AutoPostBack="true" OnCheckedChanged="cbDataAutomationEnabled_CheckedChanged" />

                        <hr />

                        <asp:Panel ID="pnlReactivatePeople" runat="server" Enabled="false" CssClas="data-integrity-options">

                            <div class="clearfix margin-b-lg">
                                <div class="pull-left" style="width: 40px">
                                    <Rock:RockCheckBox ID="cbLastContribution" runat="server"/>
                                </div>
                                <div class="pull-left">
                                    <Rock:NumberBox ID="nbLastContribution" runat="server" Label="Any family member has made a contribution in the last" AppendText="days" CssClass="input-width-md" Text="90" />
                                </div>
                            </div>

                            <div class="clearfix margin-b-lg">
                                <div class="pull-left" style="width: 40px">
                                    <Rock:RockCheckBox ID="cbAttendanceInServiceGroup" runat="server" />
                                </div>
                                <div class="pull-left">
                                    <Rock:NumberBox ID="nbAttendanceInServiceGroup" runat="server" Label="Any family member has attended a group that is considered a service in the last" AppendText="days" CssClass="input-width-md" Text="90" />
                                </div>
                            </div>

                             <div class="clearfix margin-b-lg">
                                <div class="pull-left" style="width: 40px">
                                    <Rock:RockCheckBox ID="cbRegisteredInAnyEvent" runat="server" />
                                </div>
                                <div class="pull-left">
                                    <Rock:NumberBox ID="nbRegisteredInAnyEvent" runat="server" Label="Any family member has registered for any event in the last" AppendText="days" CssClass="input-width-md" Text="90" />
                                </div>
                            </div>

                            <div class="clearfix margin-b-lg">
                                <div class="pull-left" style="width: 40px">
                                    <Rock:RockCheckBox ID="cbAttendanceInGroupType" runat="server" />
                                </div>
                                <div class="pull-left">
                                    <Rock:RockControlWrapper ID="rcwAttendanceInGroupType" runat="server" Label="Any family member has attended a group of this type in the last">
                                    <div class="row">
                                        <div class="col-xs-12 col-sm-6 col-md-4">
                                            <Rock:RockListBox ID="rlbAttendanceInGroupType" runat="server" DataTextField="text" DataValueField="value" />
                                        </div>
                                        <div class="col-xs-12 col-sm-6 col-md-4">
                                            <Rock:NumberBox ID="nbAttendanceInGroupType" runat="server" AppendText="days" CssClass="input-width-md" Text="90" />
                                        </div>
                                    </div>
                                </Rock:RockControlWrapper>
                                </div>
                            </div>

                            <div class="clearfix margin-b-lg">
                                <div class="pull-left" style="width: 40px">
                                    <Rock:RockCheckBox ID="cbSiteLogin" runat="server" />
                                </div>
                                <div class="pull-left">
                                    <Rock:NumberBox ID="nbSiteLogin" runat="server" Label="Any family member has logged into Rock in the last" AppendText="days" CssClass="input-width-md" Text="90" />
                                </div>
                            </div>

                            <div class="clearfix margin-b-lg">
                                <div class="pull-left" style="width: 40px">
                                    <Rock:RockCheckBox ID="cbPrayerRequest" runat="server" />
                                </div>
                                <div class="pull-left">
                                    <Rock:NumberBox ID="nbPrayerRequest" runat="server" Label="Any family member has submitted a prayer request in the last" AppendText="days" CssClass="input-width-md" Text="90" />
                                </div>
                            </div>

                            <div class="clearfix margin-b-lg">
                                <div class="pull-left" style="width: 40px">
                                    <Rock:RockCheckBox ID="cbPersonAttributes" runat="server" />
                                </div>
                                <div class="pull-left">
                                    <Rock:RockControlWrapper ID="rcwPersonAttributes" runat="server" Label="Any family member has a new value for any of the following person attributes in the last">
                                    <div class="row">
                                        <div class="col-xs-12 col-sm-6 col-md-4">
                                            <Rock:RockListBox ID="rlbPersonAttributes" runat="server" DataTextField="text" DataValueField="value">
                                            </Rock:RockListBox>
                                        </div>
                                        <div class="col-xs-12 col-sm-6 col-md-4">
                                            <Rock:NumberBox ID="nbPersonAttributes" runat="server" AppendText="days" CssClass="input-width-md" Text="90" />
                                        </div>
                                    </div>
                                </Rock:RockControlWrapper>
                                </div>
                            </div>

                            <div class="clearfix margin-b-lg">
                                <div class="pull-left" style="width: 40px">
                                    <Rock:RockCheckBox ID="cbInteractions" runat="server" />
                                </div>
                                <div class="pull-left">
                                    <Rock:RockControlWrapper ID="rcwInteractions" runat="server" Label="Any family member has an interaction of the following type in the last">
                                    <asp:Repeater ID="rInteractions" runat="server">
                                        <ItemTemplate>
                                            <div class="row margin-b-sm">
                                                <asp:HiddenField ID="hfInteractionTypeId" runat="server" Value='<%# Eval("Guid") %>' />
                                                <div class="col-md-5 col-sm-6 col-xs-8 padding-t-sm">
                                                    <Rock:RockCheckBox ID="cbInterationType" runat="server" SelectedIconCssClass="fa fa-check-square-o" UnSelectedIconCssClass="fa fa-square-o" Text='<%# Eval("Name") %>' Checked='<%# (bool)Eval("IsInteractionTypeEnabled") %>' />
                                                </div>
                                                <div class="col-md-7 col-sm-6 col-xs-4">
                                                    <Rock:NumberBox ID="nbInteractionDays" runat="server" AppendText="days" CssClass="input-width-md" Text='<%#Eval("LastInteractionDays") %>' />
                                                </div>
                                            </div>
                                            <hr />
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </Rock:RockControlWrapper>
                                </div>
                            </div>

                            <div class="clearfix margin-b-lg">
                                <div class="pull-left" style="width: 40px">
                                    <Rock:RockCheckBox ID="cbIncludeDataView" runat="server" />
                                </div>
                                <div class="pull-left">
                                    <Rock:DataViewItemPicker ID="dvIncludeDataView" runat="server" Label="The person is in the following data view" CssClass="input-width-xl" />
                                </div>
                            </div>

                            <div class="clearfix margin-b-lg">
                                <div class="pull-left" style="width: 40px">
                                    <Rock:RockCheckBox ID="cbExcludeDataView" runat="server" />
                                </div>
                                <div class="pull-left">
                                    <Rock:DataViewItemPicker ID="dvExcludeDataView" runat="server" Label="Exclude any person in the following data view" CssClass="input-width-xl" />
                                </div>
                            </div>

                        </asp:Panel>

                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="pwInactivatePeople" runat="server" Title="Inactivate People">

                        <Rock:NotificationBox ID="nbInactiveWarning" runat="server" NotificationBoxType="Warning" Title="Important" Text="<p>
                            Enabling this option could result in a large number of people being inactivated in your database. Everyone in your database that does not meet all of the selected criteria below will be inactivated. Unselecting any of the options
                            below, will result in more people being inactivated. Each person that is inactivated will also be inactivated in most of the groups that they belong to as well. This includes the security roles they belong to.
                            Once these people have been inactivated in their groups, there is no process to revert that change."/>

                        <Rock:RockCheckBox ID="cbInactivatePeople" runat="server"
                            Label="Enable" Text="Enable the automatic inactivating of individuals who are currently active and who meet all of the following selected criteria."
                            AutoPostBack="true" OnCheckedChanged="cbDataAutomationEnabled_CheckedChanged" />

                        <hr />

                        <asp:Panel ID="pnlInactivatePeople" runat="server" Enabled="false" CssClas="data-integrity-options">

                            <div class="clearfix margin-b-lg">
                                <div class="pull-left">
                                    <Rock:NumberBox ID="nbRecordsOlderThan" runat="server" Label="The number of days that the records must be older to get considered for Inactivate process." AppendText="days" CssClass="input-width-md" Text="180" />
                                </div>
                            </div>

                            <div class="clearfix margin-b-lg">
                                <div class="pull-left" style="width: 40px">
                                    <Rock:RockCheckBox ID="cbNoLastContribution" runat="server" />
                                </div>
                                <div class="pull-left">
                                    <Rock:NumberBox ID="nbNoLastContribution" runat="server" Label="No family member has made a contribution in the last" AppendText="days" CssClass="input-width-md" Text="500" />
                                </div>
                            </div>

                            <div class="clearfix margin-b-lg">
                                <div class="pull-left" style="width: 40px">
                                    <Rock:RockCheckBox ID="cbNoAttendanceInGroupType" runat="server" />
                                </div>
                                <div class="pull-left">
                                    <div class="row">
                                        <div class="col-xs-12 col-sm-6">
                                            <div style="max-width: 300px;">
                                                <Rock:NumberBox ID="nbNoAttendanceInGroupType" runat="server" AppendText="days" CssClass="input-width-md" Text="500" Label="No family member has attended any group type that takes attendance in the last"/>
                                            </div>
                                        </div>
                                        <div class="col-xs-12 col-sm-6">
                                            <div style="max-width: 300px;">
                                                <Rock:RockListBox ID="rlbNoAttendanceInGroupType" runat="server" DataTextField="text" DataValueField="value" Label="Ignore any attendance in the following group types" />
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <div class="clearfix margin-b-lg">
                                <div class="pull-left" style="width: 40px">
                                    <Rock:RockCheckBox ID="cbNoRegistrationInAnyEvent" runat="server" />
                                </div>
                                <div class="pull-left">
                                    <Rock:NumberBox ID="nbNoRegistrationInAnyEvent" runat="server" Label="No family member has registered for any event  in the last" AppendText="days" CssClass="input-width-md" Text="500" />
                                </div>
                            </div>

                            <div class="clearfix margin-b-lg">
                                <div class="pull-left" style="width: 40px">
                                    <Rock:RockCheckBox ID="cbNoSiteLogin" runat="server" />
                                </div>
                                <div class="pull-left">
                                    <Rock:NumberBox ID="nbNoSiteLogin" runat="server" Label="No family member has logged into Rock in the last" AppendText="days" CssClass="input-width-md" Text="500" />
                                </div>
                            </div>

                            <div class="clearfix margin-b-lg">
                                <div class="pull-left" style="width: 40px">
                                    <Rock:RockCheckBox ID="cbNoPrayerRequest" runat="server" />
                                </div>
                                <div class="pull-left">
                                    <Rock:NumberBox ID="nbNoPrayerRequest" runat="server" Label="No family member has submitted a prayer request in the last" AppendText="days" CssClass="input-width-md" Text="500" />
                                </div>
                            </div>

                            <div class="clearfix margin-b-lg">
                                <div class="pull-left" style="width: 40px">
                                    <Rock:RockCheckBox ID="cbNoPersonAttributes" runat="server" />
                                </div>
                                <div class="pull-left">
                                    <div class="row">
                                        <div class="col-xs-12 col-sm-6 col-md-4">
                                            <div style="max-width: 300px;">
                                                <Rock:NumberBox ID="nbNoPersonAttributes" runat="server" AppendText="days" CssClass="input-width-md" Label="No family member has a person attribute value updated in the last"/>
                                            </div>
                                        </div>
                                        <div class="col-xs-12 col-sm-6 col-md-4">
                                            <div style="max-width: 300px;">
                                                <Rock:RockListBox ID="rlbNoPersonAttributes" runat="server" DataTextField="text" DataValueField="value" Label="Ignore any updates to the following attributes">
                                                </Rock:RockListBox>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <div class="clearfix margin-b-lg">
                                <div class="pull-left" style="width: 40px">
                                    <Rock:RockCheckBox ID="cbNoInteractions" runat="server" />
                                </div>
                                <div class="pull-left">
                                    <Rock:RockControlWrapper ID="rcwNoInteractions" runat="server" Label="No family member has an interaction of the following type in the last">
                                        <asp:Repeater ID="rNoInteractions" runat="server">
                                            <ItemTemplate>
                                                <div class="row margin-b-sm">
                                                    <asp:HiddenField ID="hfInteractionTypeId" runat="server" Value='<%# Eval("Guid") %>' />
                                                    <div class="col-md-5 col-sm-6 col-xs-8 padding-t-sm">
                                                        <Rock:RockCheckBox ID="cbInterationType" runat="server" SelectedIconCssClass="fa fa-check-square-o" UnSelectedIconCssClass="fa fa-square-o" Text='<%# Eval("Name") %>' Checked='<%# (bool)Eval("IsInteractionTypeEnabled") %>' />
                                                    </div>
                                                    <div class="col-md-7 col-sm-6 col-xs-4">
                                                        <Rock:NumberBox ID="nbNoInteractionDays" runat="server" AppendText="days" CssClass="input-width-md" Text='<%#Eval("LastInteractionDays") %>' />
                                                    </div>
                                                </div>
                                                <hr />
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </Rock:RockControlWrapper>
                                </div>
                            </div>

                            <div class="clearfix margin-b-lg">
                                <div class="pull-left" style="width: 40px">
                                    <Rock:RockCheckBox ID="cbNotInDataView" runat="server" />
                                </div>
                                <div class="pull-left">
                                    <Rock:DataViewItemPicker ID="dvNotInDataView" runat="server" Label="The person is not in the following data view" CssClass="input-width-xl" />
                                </div>
                            </div>
                        </asp:Panel>

                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="pwUpdateCampus" runat="server" Title="Update Family Campus">

                        <Rock:RockCheckBox ID="cbCampusUpdate" runat="server"
                            Label="Enable" Text="Enable the automatic updating of campus for families who currently have a different campus than what is determined by the following selected criteria."
                            AutoPostBack="true" OnCheckedChanged="cbDataAutomationEnabled_CheckedChanged" />

                        <hr />

                        <asp:Panel ID="pnlCampusUpdate" runat="server" Enabled="false" CssClas="data-integrity-options">

                            <div class="clearfix margin-b-lg">
                                <div class="pull-left" style="width: 40px">
                                    <Rock:RockCheckBox ID="cbMostFamilyAttendance" runat="server" />
                                </div>
                                <div class="pull-left">
                                    <Rock:NumberBox ID="nbMostFamilyAttendance" runat="server" Label="Calculate campus based on the most family attendance to a campus-specific location in the last" AppendText="days" CssClass="input-width-md" Text="90" />
                                    <Rock:NumberBox ID="nbTimesToTriggerCampusChange" runat="server" Label="Minimum number of times to attend a campus before triggering a campus change" MinimumValue="1" AppendText="times" CssClass="input-width-md" Text="3" />
                                </div>
                                <div class="pull-right">
                                    <Rock:SchedulePicker ID="spExcludeSchedules" runat="server" AllowMultiSelect="true" Label="Exclude Schedules" />
                                </div>
                            </div>
    
                            <div class="clearfix margin-b-lg">
                                <div class="pull-left" style="width: 40px">
                                    <Rock:RockCheckBox ID="cbMostFamilyGiving" runat="server" />
                                </div>
                                <div class="pull-left">
                                    <Rock:NumberBox ID="nbMostFamilyGiving" runat="server" Label="Calculate campus based on the most family giving to a campus-specific account in the last" AppendText="days" CssClass="input-width-md" Text="90" />
                                    <Rock:RockDropDownList ID="ddlAttendanceOrGiving" CssClass="input-width-lg" runat="server" Label="If the calculated campus for most attendance and most giving are different" />
                                </div>
                            </div>
                            <div class="clearfix margin-b-lg">
                                <div class="pull-left" style="width: 40px">
                                    <Rock:RockCheckBox ID="cbIgnoreIfManualUpdate" runat="server" />
                                </div>
                                <div class="pull-left">
                                    <Rock:NumberBox ID="nbIgnoreIfManualUpdate" runat="server" Label="Ignore any family that has had a manual campus update in the last" AppendText="days" CssClass="input-width-md" Text="90" />
                                </div>
                            </div>

                            <div class="clearfix margin-b-lg">
                                <div class="pull-left" style="width: 40px">
                                    <Rock:RockCheckBox ID="cbIgnoreCampusChanges" runat="server" />
                                </div>
                                <div class="pull-left">
                                    <Rock:RockControlWrapper ID="rcwIgnoreCampusChanges" runat="server" Label="Ignore any update that would change the campus">
                                    <asp:Repeater ID="rptIgnoreCampusChanges" runat="server" OnItemDataBound="rptIgnoreCampusChanges_ItemDataBound" OnItemCommand="rptIgnoreCampusChanges_ItemCommand">
                                        <ItemTemplate>
                                            <asp:HiddenField ID="hfRowId" runat="server" Value='<%# Eval("Id") %>' />
                                            <div class="row margin-l-sm margin-b-sm form-inline">
                                                <Rock:CampusPicker ID="cpFromCampus" runat="server" Label="From" CssClass="margin-r-sm" />
                                                <Rock:CampusPicker ID="cpToCampus" runat="server" Label="To" CssClass="margin-r-sm" />
                                                <Rock:RockDropDownList ID="ddlAttendanceOrGiving" runat="server" Label="Based On" CssClass="margin-r-sm" >
                                                    <asp:ListItem Text="Either" Value="" Selected="True" />
                                                    <asp:ListItem Text="Giving" Value="1" />
                                                    <asp:ListItem Text="Attendance" Value="2" />
                                                </Rock:RockDropDownList>
                                                <asp:LinkButton ID="lbDelete" runat="server" CssClass="btn btn-xs btn-square btn-danger form-action-remove" CommandName="delete" CommandArgument='<%# Eval("Id") %>'><i class="fa fa-times"></i></asp:LinkButton>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                    <asp:LinkButton ID="lbAdd" CssClass="btn btn-xs btn-action margin-l-sm" runat="server" OnClick="lbAdd_Click">
                                        <i class="fa fa-plus-circle"></i>
                                    </asp:LinkButton>
                                </Rock:RockControlWrapper>
                                </div>
                            </div>
                        </asp:Panel>

                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="pwAdultChildren" runat="server" Title="Move Adult Children">

                        <Rock:RockCheckBox ID="cbAdultChildren" runat="server"
                            Label="Enable" Text="Enable the automatic moving of children in a family to their own new family when they become adults."
                            AutoPostBack="true" OnCheckedChanged="cbDataAutomationEnabled_CheckedChanged" />

                        <hr />

                        <asp:Panel ID="pnlAdultChildren" runat="server" Enabled="false" CssClass="data-integrity-options">
                            <Rock:RockCheckBox ID="cbisMoveGraduated" runat="server" Label="Should children only be moved if they have graduated?" Text="Yes" />
                            <Rock:NumberBox ID="nbAdultAge" runat="server" Label="The age a child should be considered an adult" AppendText="years" CssClass="input-width-md" />
                            <Rock:GroupRolePicker ID="rpParentRelationship" runat="server" Label="An optional known relationship that should be added between the parent(s) and the new adult"
                                Help="Usually this would be set to 'Parent', but if you have a different known relationship you want to use, you can set it here" />
                            <Rock:GroupRolePicker ID="rpSiblingRelationship" runat="server" Label="An optional known relationship that should be added between the new adult and their sibling(s)" />
                            <Rock:RockCheckBox ID="cbSameAddress" runat="server" Label="Should the new adult's home address be the same as their current family?" Text="Yes" />
                            <Rock:RockCheckBox ID="cbSamePhone" runat="server" Label="If the new adult does not have a home phone, should they use same number as their parent?" Text="Yes" />
                            <Rock:WorkflowTypePicker ID="wfWorkflows" runat="server" AllowMultiSelect="true" Label="The workflow type(s) to launch for each person that is processed."
                                Help="The person will be passed to the workflow as the entity. If the workflow has an 'OldFamily' Group attribute it will set this to the person's primary family before processing the person. If the workflow has a 'NewFamily' Group attribute it will set to the family that the person was updated or added as an adult to." />
                            <Rock:NumberBox ID="nbMaxRecords" runat="server" Label="The maximum number of records that should be processed at a time." AppendText="records" CssClass="input-width-lg" />

                        </asp:Panel>

                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="pwUpdatePersonConnectionStatus" runat="server" Title="Update Person Connection Status">
                        <Rock:RockCheckBox ID="cbUpdatePersonConnectionStatus" runat="server"
                            Label="Enable" Text="Enable the automatic updating of connection status."
                            AutoPostBack="true" OnCheckedChanged="cbDataAutomationEnabled_CheckedChanged" />

                        <hr />

                        <asp:Panel ID="pnlUpdatePersonConnectionStatus" runat="server" Enabled="false" CssClass="data-integrity-options">
                            <Rock:RockControlWrapper ID="rcwPersonConnectionStatusDataView" runat="server" Label="Update Connection Status based on Data View">
                                <asp:Repeater ID="rptPersonConnectionStatusDataView" runat="server" OnItemDataBound="rptPersonConnectionStatusDataView_ItemDataBound">
                                    <ItemTemplate>
                                        <asp:HiddenField ID="hfPersonConnectionStatusValueId" runat="server" />
                                        <Rock:DataViewItemPicker ID="dvpPersonConnectionStatusDataView" runat="server" />
                                    </ItemTemplate>
                                </asp:Repeater>
                              </Rock:RockControlWrapper>
                        </asp:Panel>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="pwUpdateFamilyStatus" runat="server" Title="Update Family Status">
                        <Rock:RockCheckBox ID="cbUpdateFamilyStatus" runat="server"
                            Label="Enable" Text="Enable the automatic updating of family status."
                            AutoPostBack="true" OnCheckedChanged="cbDataAutomationEnabled_CheckedChanged" />

                        <hr />

                        <asp:Panel ID="pnlUpdateFamilyStatus" runat="server" Enabled="false" CssClass="data-integrity-options">
                            <Rock:RockControlWrapper ID="rcwUpdateFamilyStatus" runat="server" Label="Update Family Status based on Data View">
                                <asp:Repeater ID="rptFamilyStatusDataView" runat="server" OnItemDataBound="rptFamilyStatusDataView_ItemDataBound">
                                    <ItemTemplate>
                                        <asp:HiddenField ID="hfGroupStatusValueId" runat="server" />
                                        <Rock:DataViewItemPicker ID="dvpGroupStatusDataView" runat="server" />
                                    </ItemTemplate>
                                </asp:Repeater>
                              </Rock:RockControlWrapper>
                        </asp:Panel>
                    </Rock:PanelWidget>

                </fieldset>

                <div class="actions">
                    <Rock:BootstrapButton ID="bbtnSaveConfig" runat="server" CssClass="btn btn-primary" data-shortcut-key="s" ToolTip="Alt+s" OnClick="bbtnSaveConfig_Click" Text="Save"
                        DataLoadingText="&lt;i class='fa fa-refresh fa-spin'&gt;&lt;/i&gt; Saving"
                        CompletedText="Success" CompletedMessage="<div class='margin-t-md alert alert-success'>Changes have been saved.</div>" CompletedDuration="3"></Rock:BootstrapButton>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
