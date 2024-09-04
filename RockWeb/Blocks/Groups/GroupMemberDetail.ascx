<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupMemberDetail.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupMemberDetail" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfGroupId" runat="server" />
        <asp:HiddenField ID="hfGroupMemberId" runat="server" />
        <asp:HiddenField ID="hfCampusId" runat="server" />
        <asp:HiddenField ID="hfLocationId" runat="server" />
        <asp:HiddenField ID="hfScheduleId" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lGroupIconHtml" runat="server" />
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlArchived" runat="server" CssClass="js-archived-label" LabelType="Danger" Text="Archived" />
                    <Rock:HighlightLabel ID="hlDateAdded" runat="server" LabelType="Default" />
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <asp:CustomValidator ID="cvGroupMember" runat="server" Display="None" />
                <Rock:NotificationBox ID="nbErrorMessage" runat="server" NotificationBoxType="Danger" />

                <asp:Panel ID="pnlRequiredSignatureDocument" runat="server" CssClass="alert alert-warning" Visible="false" >

                    <div class="row">
                        <div class="col-md-9">
                            <asp:Literal ID="lRequiredSignatureDocumentMessage" runat="server" />
                        </div>
                        <div class="col-md-3 text-right">
                            <asp:LinkButton ID="lbResendDocumentRequest" runat="server" Text="Send Signature Request" CssClass="btn btn-warning btn-sm" OnClick="lbResendDocumentRequest_Click" />
                        </div>
                    </div>
                    <Rock:ModalAlert ID="maSignatureRequestSent" runat="server" Text="A Signature Request Has Been Sent." Visible="false" />
                </asp:Panel>

                <div id="pnlEditDetails" runat="server">

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:PersonPicker runat="server" ID="ppGroupMemberPerson" Label="Person" CssClass="js-authorizedperson" Required="true" OnSelectPerson="ppGroupMemberPerson_SelectPerson" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox runat="server" ID="cbIsNotified" Label="Notified" Help="If this box is unchecked and a <a href='http://www.rockrms.com/Rock/BookContent/7/#servicejobsrelatingtogroups'>group leader notification job</a> is enabled then a notification will be sent to the group's leaders when this group member is saved." />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockDropDownList runat="server" ID="ddlGroupRole" DataTextField="Name" DataValueField="Id" Label="Role" Required="true" AutoPostBack="true" OnSelectedIndexChanged="ddlGroupRole_SelectedIndexChanged" />
                            <Rock:RockTextBox ID="tbNote" runat="server" Label="Note" TextMode="MultiLine" Rows="4" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockRadioButtonList ID="rblStatus" runat="server" Label="Status" RepeatDirection="Horizontal" />
                            <Rock:RockRadioButtonList ID="rblCommunicationPreference" runat="server" RepeatDirection="Horizontal" Label="Communication Preference">
                                <asp:ListItem Text="No Preference" Value="0" />
                                <asp:ListItem Text="Email" Value="1" />
                                <asp:ListItem Text="SMS" Value="2" />
                            </Rock:RockRadioButtonList>
                            <Rock:RockControlWrapper id="rcwLinkedRegistrations" runat="server" Label="Registration">
                                <ul class="list-unstyled">
                                    <asp:Repeater ID="rptLinkedRegistrations" runat="server">
                                        <ItemTemplate>
                                            <li><a href='<%# RegistrationUrl( (int)Eval("Id" ) ) %>'><%# Eval("Name") %></a></li>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </ul>
                            </Rock:RockControlWrapper>
                            <asp:HiddenField ID="hfSignedDocumentId" runat="server" />
                            <Rock:FileUploader ID="fuSignedDocument" runat="server" Label="Signed Document" />
                        </div>
                    </div>
                    <asp:Panel ID="pnlScheduling" runat="server" Visible="false">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockDropDownList ID="ddlGroupMemberScheduleTemplate" runat="server" Label="Schedule Template" AutoPostBack="true" OnSelectedIndexChanged="ddlGroupMemberScheduleTemplate_SelectedIndexChanged" />
                            </div>
                            <div class="col-md-6">
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-6">
                                <label class="control-label">
                                    <asp:Literal runat="server" ID="lGroupPreferenceAssignmentLabel" Text="Assignment" />
                                </label>
                                <p>
                                    <asp:Literal runat="server" ID="lGroupPreferenceAssignmentHelp" Text="Please select a time and optional location that you would like to be scheduled for." />
                                </p>

                                <%-- NOTE: This gGroupPreferenceAssignments (and these other controls in the ItemTemplate) is in a repeater and is configured in rptGroupPreferences_ItemDataBound--%>
                                <Rock:Grid ID="gGroupPreferenceAssignments" runat="server" DisplayType="Light" RowItemText="Group Preference Assignment" AllowPaging="false">
                                    <Columns>
                                        <Rock:RockBoundField DataField="FormattedScheduleName" HeaderText="Schedule" />
                                        <Rock:RockBoundField DataField="LocationName" HeaderText="Location" />
                                        <Rock:LinkButtonField ID="btnEditGroupPreferenceAssignment" CssClass="btn btn-default btn-sm" Text="<i class='fa fa-pencil'></i>" OnClick="btnEditGroupPreferenceAssignment_Click" />
                                        <Rock:DeleteField OnClick="btnDeleteGroupPreferenceAssignment_Click" />
                                    </Columns>
                                </Rock:Grid>
                                <br />
                                <Rock:DatePicker ID="dpScheduleStartDate" runat="server" Label="Schedule Start Date" />
                                <Rock:NumberBox ID="nbScheduleReminderEmailOffsetDays" runat="server" NumberType="Integer" Label="Schedule Reminder Email Offset Days" Help="The number of days prior to the schedule to send a reminder email or leave blank to use the default." Placeholder="Use default" />
                            </div>
                            <div class="col-md-6">
                            </div>
                        </div>
                    </asp:Panel>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:AttributeValuesContainer ID="avcGroupMemberAttributes" runat="server" NumberOfColumns="2" />
                            <Rock:AttributeValuesContainer ID="avcGroupMemberAttributesReadOnly" runat="server" NumberOfColumns="2" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:AttributeValuesContainer ID="avcGroupMemberAssignmentAttributes" runat="server" NumberOfColumns="2" />
                            <Rock:AttributeValuesContainer ID="avcGroupMemberAssignmentAttributesReadOnly" runat="server" NumberOfColumns="2" />
                        </div>
                    </div>

                    <asp:Panel ID="pnlRequirements" runat="server">
                        <Rock:GroupMemberRequirementsContainer ID="gmrcRequirements" runat="server" Visible="true" />
                        <Rock:NotificationBox ID="nbRequirementsErrors" runat="server" Dismissable="true" NotificationBoxType="Warning" />
                    </asp:Panel>
                    <Rock:NotificationBox runat="server" ID="nbCommunicationSuccess" NotificationBoxType="Success" Dismissable="true" Text="Communication was sent." Visible="false" />
                    <Rock:NotificationBox runat="server" ID="nbRecheckedNotification" NotificationBoxType="Success" Dismissable="true" Text="Successfully re-checked requirements at {0}" Visible="false" />
                    <Rock:ModalDialog ID="mdRestoreArchivedPrompt" runat="server" Visible="false" Title="Restore Group Member" CancelLinkVisible="false">
                        <Content>
                            <asp:HiddenField ID="hfRestoreGroupMemberId" runat="server" />
                            <Rock:NotificationBox ID="nbRestoreError" runat="server" NotificationBoxType="Danger" Visible="false"/>
                            <Rock:NotificationBox ID="nbRestoreArchivedGroupMember" runat="server" NotificationBoxType="Info" Text="There is an archived record for the person in this role in this group. Do you want to restore the previous settings? Notes will be retained." />
                        </Content>
                        <Footer>
                            <asp:LinkButton ID="btnRestoreArchivedGroupMember" runat="server" CssClass="btn btn-primary" Text="Restore" OnClick="btnRestoreArchivedGroupMember_Click" />
                            <asp:LinkButton ID="btnDontRestoreArchiveGroupmember" runat="server" CssClass="btn btn-default" Text="Don't Restore" OnClick="btnDontRestoreArchiveGroupmember_Click" />
                            <asp:LinkButton ID="btnCancelRestore" runat="server" CssClass="btn btn-link" Text="Cancel" OnClick="btnCancelRestore_Click" />
                        </Footer>
                    </Rock:ModalDialog>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" Text="Save" ToolTip="Alt+s" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnRefreshRequirements" runat="server" data-shortcut-key="r" ToolTip="Alt+r" Text="Refresh Requirements" CssClass="btn btn-default" OnClick="btnRefreshRequirements_Click" CausesValidation="false" />
                        <asp:LinkButton ID="btnSaveThenAdd" runat="server" data-shortcut-key="a" ToolTip="Alt+a" Text="Save Then Add" CssClass="btn btn-link" OnClick="btnSaveThenAdd_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_Click" CausesValidation="false" />
                        <asp:LinkButton ID="btnShowMoveDialog" runat="server" CssClass="btn btn-default btn-square btn-sm pull-right" OnClick="btnShowMoveDialog_Click" ToolTip="Move to another group" CausesValidation="false"><i class="fa fa-external-link"></i></asp:LinkButton>
                        <asp:LinkButton ID="btnShowCommunicationDialog" runat="server" CssClass="btn btn-default btn-square btn-sm pull-right" ToolTip="Quick Communication" OnClick="btnShowCommunicationDialog_Click"><i class="fa fa-envelope-o"></i></asp:LinkButton>
                    </div>

                </div>

            </div>
        </div>

        <Rock:ModalDialog ID="mdMoveGroupMember" runat="server" Title="Move Group Member" ValidationGroup="vgMoveGroupMember" Visible="false" CancelLinkVisible="false">
            <Content>
                <asp:ValidationSummary ID="vsMoveGroupMember" runat="server" ValidationGroup="vgMoveGroupMember" HeaderText="Please correct the following:" CssClass="alert alert-validation"  />
                        <Rock:RockLiteral ID="lCurrentGroup" runat="server" Label="Current Group" />
                        <Rock:GroupPicker ID="gpMoveGroupMember" runat="server" Required="true" Label="Destination Group" ValidationGroup="vgMoveGroupMember" OnSelectItem="gpMoveGroupMember_SelectItem" />
                        <Rock:GroupRolePicker ID="grpMoveGroupMember" runat="server" Label="Role" ValidationGroup="vgMoveGroupMember" GroupTypeId="0" />
                        <Rock:RockCheckBox ID="cbMoveGroupMemberMoveNotes" runat="server" ValidationGroup="vgMoveGroupMember" Label="Move Notes" Help="If this group member has notes, move these notes with them to their new group." />
                        <Rock:RockCheckBox ID="cbMoveGroupMemberFundraisingTransactions" runat="server" ValidationGroup="vgMoveGroupMember" Label="Move Fundraising Financial Transactions" Help="If enabled the related fundraising financial transactions will be re-linked to the new fundraising group. If the new group has different financial accounts configured adjustment financial transactions may be created to move the dollars from one account to another.  This will only occur if the batches for the original accounts are closed. The new transactions will be placed in a new batch." />
                        <Rock:NotificationBox ID="nbMoveGroupMemberWarning" runat="server" NotificationBoxType="Warning" Visible="false" />
            </Content>
            <Footer>
                <asp:LinkButton ID="btnMoveGroupMember" runat="server" CssClass="btn btn-primary" Text="Move" ValidationGroup="vgMoveGroupMember" OnClick="btnMoveGroupMember_Click" />
            </Footer>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="mdQuickCommunication" runat="server" Title="Send Group Member Communication" ValidationGroup="vgSendGroupMemberCommunication" Visible="false" SaveButtonText="Send" SaveButtonCausesValidation="true" OnSaveClick="mdQuickCommunication_SaveClick" CancelLinkVisible ="true">
            <Content>
                <asp:ValidationSummary ID="vsSendGroupMemberCommunication" runat="server" ValidationGroup="vgSendGroupMemberCommunication" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <div class="row">
                    <div class="col-md-12">
                        <Rock:Toggle ID="tglCommunicationPreference" runat="server" OnText="SMS" OffText="Email" Checked="false" ButtonSizeCssClass="btn-xs" OnCssClass="btn-primary" OffCssClass="btn-default" OnCheckedChanged="tglCommunicationPreference_CheckedChanged" />
                        <Rock:RockLiteral ID="lCommunicationTo" runat="server"></Rock:RockLiteral>
                        <asp:Panel ID="pnlEmailControls" runat="server">
                            <Rock:EmailBox ID="ebEmailCommunicationFrom" runat="server" Label="From" Required="true" ValidationGroup="vgEmailGroupMemberCommunication"></Rock:EmailBox>
                            <Rock:RockTextBox ID="tbEmailCommunicationSubject" runat="server" Label="Subject" Required="true" ValidationGroup="vgEmailGroupMemberCommunication" />
                        </asp:Panel>
                        <asp:Panel ID="pnlSMSControls" runat="server">
                            <asp:HiddenField ID="hfFromSMSNumber" runat="server" Visible="false" />
                            <Rock:RockDropDownList ID="ddlSmsNumbers" runat="server" Label="From" />
                            <Rock:PersonPicker ID="ppSMSCommunicationFrom" runat="server" Label="From" Required="true" Visible="false" ValidationGroup="vgSMSGroupMemberCommunication" />
                            <asp:HiddenField ID="hfToSMSNumber" runat="server" Visible="false" />
                        </asp:Panel>
                        <Rock:RockTextBox ID="tbCommunicationMessage" runat="server" Label="Message" Required="true" TextMode="MultiLine" Rows="4" ValidationGroup="vgSendGroupMemberCommunication"></Rock:RockTextBox>

                        <Rock:NotificationBox ID="nbSendGroupMemberCommunication" runat="server" NotificationBoxType="Warning" Visible="false" />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>
        <%-- Preferences Add/Edit GroupScheduleAssignment modal --%>
        <Rock:ModalDialog ID="mdGroupScheduleAssignment" runat="server" OnSaveClick="mdGroupScheduleAssignment_SaveClick"
            Title="Add/Edit Assignment" ValidationGroup="GroupScheduleAssignment">
            <Content>
                <asp:HiddenField ID="hfGroupScheduleAssignmentGuid" runat="server" />
                <Rock:RockDropDownList ID="ddlGroupScheduleAssignmentSchedule" runat="server" Label="Schedule"
                    AutoPostBack="true" OnSelectedIndexChanged="ddlGroupScheduleAssignmentSchedule_SelectedIndexChanged"
                    Required="true" ValidationGroup="GroupScheduleAssignment" />
                <Rock:RockDropDownList ID="ddlGroupScheduleAssignmentLocation" runat="server" Label="Location" ValidationGroup="GroupScheduleAssignment" />
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
