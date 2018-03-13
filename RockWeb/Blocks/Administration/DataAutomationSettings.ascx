<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DataAutomationSettings.ascx.cs" Inherits="RockWeb.Blocks.Administration.DataAutomationSettings" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-tachometer"></i>
                    Data Automation
                </h1>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />

                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <Rock:PanelWidget ID="pwReactivatePeople" runat="server" title="Reactivate People">

                    <Rock:RockCheckBox ID="cbReactivatePeople" runat="server" 
                        Label="Enable" Text="Enable the automatic activating of individuals who are currently inactive and who meet any of the following selected criteria. (deceased individuals will never be reactivated)."
                        AutoPostBack="true" OnCheckedChanged="cbReactivatePeople_CheckedChanged" />

                    <hr />

                    <asp:Panel ID="pnlReactivatePeople" runat="server" Enabled="false" CssClas="data-integrity-options">

                        <Rock:RockCheckBox ID="cbLastContribution" runat="server"/>
                        <Rock:NumberBox ID="nbLastContribution" runat="server" Label="Any family member has made a contribution in the last" AppendText="days" CssClass="input-width-md" Text="90" />

                        <Rock:RockCheckBox ID="cbAttendanceInServiceGroup" runat="server" />
                        <Rock:NumberBox ID="nbAttendanceInServiceGroup" runat="server" Label="Any family member has attended a group that is considered a service in the last" AppendText="days" CssClass="input-width-md" Text="90" />

                        <Rock:RockCheckBox ID="cbAttendanceInGroupType" runat="server" />
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

                        <Rock:RockCheckBox ID="cbPrayerRequest" runat="server" />
                        <Rock:NumberBox ID="nbPrayerRequest" runat="server" Label="Any family member has submitted a prayer request in the last" AppendText="days" CssClass="input-width-md" Text="90" />

                        <Rock:RockCheckBox ID="cbPersonAttributes" runat="server" />
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

                        <Rock:RockCheckBox ID="cbInteractions" runat="server" />
                        <Rock:RockControlWrapper ID="rcwInteractions" runat="server" Label="Any family member has an interaction of the following type in the last">
                            <asp:Repeater ID="rInteractions" runat="server">
                                <ItemTemplate>
                                    <div class="row margin-b-sm">
                                        <asp:HiddenField ID="hfInteractionTypeId" runat="server" Value='<%# Eval("Guid") %>' />
                                        <div class="col-md-2 col-sm-3 col-xs-4">
                                            <Rock:RockCheckBox ID="cbInterationType" runat="server" SelectedIconCssClass="fa fa-check-square-o" UnSelectedIconCssClass="fa fa-square-o" Text='<%# Eval("Name") %>' Checked='<%# (bool)Eval("IsInteractionTypeEnabled") %>' />
                                        </div>
                                        <div class="col-md-10 col-sm-9 col-xs-6">
                                            <Rock:NumberBox ID="nbInteractionDays" runat="server" AppendText="days" CssClass="input-width-md" Text='<%#Eval("LastInteractionDays") %>' />
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </Rock:RockControlWrapper>

                        <Rock:RockCheckBox ID="cbIncludeDataView" runat="server" />
                        <Rock:DataViewPicker ID="dvIncludeDataView" runat="server" Label="The person is in the following data view" CssClass="input-width-xl" />

                        <Rock:RockCheckBox ID="cbExcludeDataView" runat="server" />
                        <Rock:DataViewPicker ID="dvExcludeDataView" runat="server" Label="Exclude any person in the following data view" CssClass="input-width-xl" />

                    </asp:Panel>

                </Rock:PanelWidget>

                <Rock:PanelWidget ID="pwInactivatePeople" runat="server" Title="Inactivate People">

                    <Rock:RockCheckBox ID="cbInactivatePeople" runat="server" 
                        Label="Enable" Text="Enable the automatic inactivating of individuals who are currently active and who meet all of the following selected criteria."
                        AutoPostBack="true" OnCheckedChanged="cbInactivatePeople_CheckedChanged" />

                    <hr />

                    <asp:Panel ID="pnlInactivatePeople" runat="server" Enabled="false" CssClas="data-integrity-options">

                        <Rock:RockCheckBox ID="cbNoLastContribution" runat="server" />
                        <Rock:NumberBox ID="nbNoLastContribution" runat="server" Label="No family member has made a contribution in the last" AppendText="days" CssClass="input-width-md" Text="500" />

                        <Rock:RockCheckBox ID="cbNoAttendanceInServiceGroup" runat="server" />
                        <Rock:NumberBox ID="nbNoAttendanceInServiceGroup" runat="server" Label="No family member has attended a group that is considered a service in the last" AppendText="days" CssClass="input-width-md" Text="500" />

                        <Rock:RockCheckBox ID="cbNoAttendanceInGroupType" runat="server" />
                        <Rock:RockControlWrapper ID="rcwNoAttendanceInGroupType" runat="server" Label="No family member has attended a group of this type in the last">
                            <div class="row">
                                <div class="col-xs-12 col-sm-6 col-md-4">
                                    <Rock:RockListBox ID="rlbNoAttendanceInGroupType" runat="server" DataTextField="text" DataValueField="value" />
                                </div>
                                <div class="col-xs-12 col-sm-6 col-md-4">
                                    <Rock:NumberBox ID="nbNoAttendanceInGroupType" runat="server" AppendText="days" CssClass="input-width-md" Text="500" />
                                </div>
                            </div>
                        </Rock:RockControlWrapper>

                        <Rock:RockCheckBox ID="cbNoPrayerRequest" runat="server" />
                        <Rock:NumberBox ID="nbNoPrayerRequest" runat="server" Label="No family member has submitted a prayer request in the last" AppendText="days" CssClass="input-width-md" Text="500" />

                        <Rock:RockCheckBox ID="cbNoPersonAttributes" runat="server" />
                        <Rock:RockControlWrapper ID="rcwNoPersonAttributes" runat="server" Label="Has no new values in the following person attributes in the last">
                            <div class="row">
                                <div class="col-xs-12 col-sm-6 col-md-4">
                                    <Rock:RockListBox ID="rlbNoPersonAttributes" runat="server" DataTextField="text" DataValueField="value">
                                    </Rock:RockListBox>
                                </div>
                                <div class="col-xs-12 col-sm-6 col-md-4">
                                    <Rock:NumberBox ID="nbNoPersonAttributes" runat="server" AppendText="days" CssClass="input-width-md" Text="500" />
                                </div>
                            </div>
                        </Rock:RockControlWrapper>

                        <Rock:RockCheckBox ID="cbNoInteractions" runat="server" />
                        <Rock:RockControlWrapper ID="rcwNoInteractions" runat="server" Label="No family member has an interaction of the following type in the last">
                            <asp:Repeater ID="rNoInteractions" runat="server">
                                <ItemTemplate>
                                    <div class="row margin-b-sm">
                                        <asp:HiddenField ID="hfInteractionTypeId" runat="server" Value='<%# Eval("Guid") %>' />
                                        <div class="col-md-2 col-sm-3 col-xs-4">
                                            <Rock:RockCheckBox ID="cbInterationType" runat="server" SelectedIconCssClass="fa fa-check-square-o" UnSelectedIconCssClass="fa fa-square-o" Text='   <%# Eval("Name") %>' Checked='<%# (bool)Eval("IsInteractionTypeEnabled") %>' />
                                        </div>
                                        <div class="col-md-10 col-sm-9 col-xs-8">
                                            <Rock:NumberBox ID="nbNoInteractionDays" runat="server" AppendText="days" CssClass="input-width-md" Text='<%#Eval("LastInteractionDays") %>' />
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </Rock:RockControlWrapper>

                        <Rock:RockCheckBox ID="cbNotInDataView" runat="server" />
                        <Rock:DataViewPicker ID="dvNotInDataView" runat="server" Label="The person is not in the following data view" CssClass="input-width-xl" />

                    </asp:Panel>

                </Rock:PanelWidget>

                <Rock:PanelWidget ID="pwUpdateCampus" runat="server" Title="Upate Family Campus">

                    <Rock:RockCheckBox ID="cbCampusUpdate" runat="server" 
                        Label="Enable" Text="Enable the automatic updating of campus for families who currently have a different campus that what is determined by the following selected criteria."
                        AutoPostBack="true" OnCheckedChanged="cbCampusUpdate_CheckedChanged" />

                    <hr />

                    <asp:Panel ID="pnlCampusUpdate" runat="server" Enabled="false" CssClas="data-integrity-options">

                        <Rock:RockCheckBox ID="cbMostFamilyAttendance" runat="server" />
                        <Rock:NumberBox ID="nbMostFamilyAttendance" runat="server" Label="Calculate campus based on the most family attendance to a campus-specified location in the last" AppendText="days" CssClass="input-width-md" Text="90" />
                            
                        <Rock:RockCheckBox ID="cbMostFamilyGiving" runat="server" />
                        <Rock:NumberBox ID="nbMostFamilyGiving" runat="server" Label="Calculate campus based on the most family giving to a campus-specified account in the last" AppendText="days" CssClass="input-width-md" Text="90" />

                        <Rock:RockDropDownList ID="ddlAttendanceOrGiving" CssClass="input-width-md" runat="server" Label="If the calcualted campus for most attendance and most giving are different" />
                            
                        <Rock:RockCheckBox ID="cbIgnoreIfManualUpdate" runat="server" />
                        <Rock:NumberBox ID="nbIgnoreIfManualUpdate" runat="server" Label="Ignore any family that has had a manual campus update in the last" AppendText="days" CssClass="input-width-md" Text="90" />
                            
                        <Rock:RockCheckBox ID="cbIgnoreCampusChanges" runat="server" />
                        <Rock:RockControlWrapper ID="rcwIgnoreCampusChanges" runat="server" Label="Ignore any update that would change the campus">
                            <asp:Repeater ID="rIgnoreCampusChanges" runat="server" OnItemDataBound="rIgnoreCampusChanges_ItemDataBound" OnItemCommand="rIgnoreCampusChanges_ItemCommand">
                                <ItemTemplate>
                                    <asp:HiddenField ID="hfRowId" runat="server" Value='<%# Eval("Id") %>' />
                                    <div class="row margin-l-sm margin-b-sm form-inline">
                                        <Rock:CampusPicker ID="cpFromCampus" runat="server" Label="From" CssClass="margin-r-sm" />
                                        <Rock:CampusPicker ID="cpToCampus" runat="server" Label="To" CssClass="margin-r-sm" />
                                        <Rock:RockDropDownList ID="ddlAttendanceOrGiving" runat="server" Label="Based On" CssClass="margin-r-sm" >
                                            <asp:ListItem Text="" Selected="True" />
                                            <asp:ListItem Text="Giving" Value="1" />
                                            <asp:ListItem Text="Attendance" Value="2" />
                                        </Rock:RockDropDownList>
                                        <asp:LinkButton ID="lbDelete" runat="server" CssClass="btn btn-xs btn-danger form-action-remove" CommandName="delete" CommandArgument='<%# Eval("Id") %>'><i class="fa fa-minus-circle"></i></asp:LinkButton>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                            <asp:LinkButton ID="lbAdd" CssClass="btn btn-xs btn-action margin-l-sm" runat="server" OnClick="lbAdd_Click">
                                <i class="fa fa-plus-circle"></i>
                            </asp:LinkButton>
                        </Rock:RockControlWrapper>

                    </asp:Panel>

                </Rock:PanelWidget>

                <Rock:PanelWidget ID="pwAdultChildren" runat="server" Title="Move Adult Children">

                    <Rock:RockCheckBox ID="cbAdultChildren" runat="server" 
                        Label="Enable" Text="Enable the automatic moving of children in a family to their own new family when they become adults."
                        AutoPostBack="true" OnCheckedChanged="cbAdultChildren_CheckedChanged" />

                    <hr />

                    <asp:Panel ID="pnlAdultChildren" runat="server" Enabled="false" CssClas="data-integrity-options">

                        <Rock:NumberBox ID="nbAdultAge" runat="server" Label="The age a child should be considered an adult" AppendText="years" CssClass="input-width-md" />
                        <Rock:GroupRolePicker ID="rpParentRelationship" runat="server" Label="An optional known relationship that should be added between the new adult and their parent(s)" />
                        <Rock:GroupRolePicker ID="rpSiblingRelationship" runat="server" Label="An optional known relationship that should be added between the new adult and their sibling(s)" />
                        <Rock:RockCheckBox ID="cbSameAddress" runat="server" Label="Should the new adult's home address be the same as their current family?" Text="Yes" />
                        <Rock:RockCheckBox ID="cbSamePhone" runat="server" Label="If the new adult does not have a home phone, should they use same number as their parent?" Text="Yes" />
                        <Rock:WorkflowTypePicker ID="wfWorkflows" runat="server" AllowMultiSelect="true" Label="The workflow type(s) to launch for each person that is processed."
                            Help="The person will be passed to the workflow as the entity. If the workflow has an 'OldFamily' Group attribute it will set this to the person's primary family before processing the person. If the workflow has a 'NewFamily' Group attribute it will set to the family that the person was updated or added as an adult to." />
                        <Rock:NumberBox ID="nbMaxRecords" runat="server" Label="The maximum number of records that should be processed at a time." AppendText="records" CssClass="input-width-md" /> 

                    </asp:Panel>

                </Rock:PanelWidget>

                <div class="actions margin-t-lg">
                    <Rock:BootstrapButton ID="bbtnSaveConfig" runat="server" CssClass="btn btn-primary" AccessKey="s" ToolTip="Alt+s" OnClick="bbtnSaveConfig_Click" Text="Save" 
                        DataLoadingText="&lt;i class='fa fa-refresh fa-spin'&gt;&lt;/i&gt; Saving" 
                        CompletedText="Success" CompletedMessage="&nbsp;Changes Have Been Saved!" CompletedDuration="2"></Rock:BootstrapButton>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
