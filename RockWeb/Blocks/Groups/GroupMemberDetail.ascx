<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupMemberDetail.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupMemberDetail" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfGroupId" runat="server" />
        <asp:HiddenField ID="hfGroupMemberId" runat="server" />


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

                    <asp:Panel ID="pnlScheduling" runat="server">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockDropDownList ID="ddlGroupMemberScheduleTemplate" runat="server" Label="Schedule Template" AutoPostBack="true" OnSelectedIndexChanged="ddlGroupMemberScheduleTemplate_SelectedIndexChanged" />
                                <Rock:DatePicker ID="dpScheduleStartDate" runat="server" Label="Schedule Start Date" />
                                <Rock:NumberBox ID="nbScheduleReminderEmailOffsetDays" runat="server" NumberType="Integer" Label="Schedule Reminder Email Offset Days" Help="The number of days prior to the schedule to send a reminder email or leave blank to use the default." Placeholder="Use default" />
                            </div>
                            <div class="col-md-6">
                            </div>
                        </div>
                    </asp:Panel>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:AttributeValuesContainer ID="avcAttributes" runat="server" />
                            <Rock:AttributeValuesContainer ID="avcAttributesReadOnly" runat="server" />
                        </div>
                    </div>

                    <asp:Panel ID="pnlRequirements" runat="server">
                        <div class="row">
                            <div class="col-md-6">
                            </div>
                            <div class="col-md-6">
                                <Rock:RockControlWrapper ID="rcwRequirements" runat="server" Label="Requirements">
                                    <Rock:NotificationBox ID="nbRequirementsErrors" runat="server" Dismissable="true" NotificationBoxType="Warning" />
                                    <Rock:RockCheckBoxList ID="cblManualRequirements" RepeatDirection="Vertical" runat="server" Label="" />
                                    <div class="labels">
                                        <asp:Literal ID="lRequirementsLabels" runat="server" />
                                    </div>
                                </Rock:RockControlWrapper>
                            </div>
                        </div>
                    </asp:Panel>

                    <Rock:NotificationBox runat="server" ID="nbRecheckedNotification" NotificationBoxType="Success" Dismissable="true" Text="Successfully re-checked requirements at {0}" Visible="false" />
                    <Rock:ModalDialog ID="mdRestoreArchivedPrompt" runat="server" Visible="false" Title="Restore Group Member" CancelLinkVisible="false">
                        <Content>
                            <div class="row">
                                <div class="col-md-12">
                                    <asp:HiddenField ID="hfRestoreGroupMemberId" runat="server" />
                                    <Rock:NotificationBox ID="nbRestoreArchivedGroupMember" runat="server" NotificationBoxType="Info" Text="There is an archived record for the person in this role in this group. Do you want to restore the previous settings? Notes will be retained." />
                                </div>
                            </div>
                            <br />
                            <div class="actions">
                                <asp:LinkButton ID="btnRestoreArchivedGroupMember" runat="server" CssClass="btn btn-primary" Text="Restore" OnClick="btnRestoreArchivedGroupMember_Click" />
                                <asp:LinkButton ID="btnDontRestoreArchiveGroupmember" runat="server" CssClass="btn btn-default" Text="Don't Restore" OnClick="btnDontRestoreArchiveGroupmember_Click" />
                            </div>
                        </Content>
                    </Rock:ModalDialog>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" ToolTip="Alt+S" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnReCheckRequirements" runat="server" AccessKey="r" ToolTip="Alt+R" Text="Re-Check Requirements" CssClass="btn btn-default" OnClick="btnReCheckRequirements_Click" CausesValidation="false" />
                        <asp:LinkButton ID="btnSaveThenAdd" runat="server" AccessKey="a" ToolTip="Alt+A" Text="Save Then Add" CssClass="btn btn-link" OnClick="btnSaveThenAdd_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+C" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_Click" CausesValidation="false" />
                        <asp:LinkButton ID="btnShowMoveDialog" runat="server" CssClass="btn btn-default btn-sm pull-right" OnClick="btnShowMoveDialog_Click" ToolTip="Move to another group" CausesValidation="false"><i class="fa fa-external-link"></i></asp:LinkButton>
                    </div>

                </div>

            </div>
        </div>

        <Rock:ModalDialog ID="mdMoveGroupMember" runat="server" Title="Move Group Member" ValidationGroup="vgMoveGroupMember" Visible="false" CancelLinkVisible="false">
            <Content>
                <asp:ValidationSummary ID="vsMoveGroupMember" runat="server" ValidationGroup="vgMoveGroupMember" HeaderText="Please correct the following:" CssClass="alert alert-validation"  />
                <div class="row">
                    <div class="col-md-12">
                        <Rock:RockLiteral ID="lCurrentGroup" runat="server" Label="Current Group" />
                        <Rock:GroupPicker ID="gpMoveGroupMember" runat="server" Required="true" Label="Destination Group" ValidationGroup="vgMoveGroupMember" OnSelectItem="gpMoveGroupMember_SelectItem" />
                        <Rock:GroupRolePicker ID="grpMoveGroupMember" runat="server" Label="Role" ValidationGroup="vgMoveGroupMember" GroupTypeId="0" />   
                        <Rock:RockCheckBox ID="cbMoveGroupMemberMoveNotes" runat="server" ValidationGroup="vgMoveGroupMember" Label="Move Notes" Help="If this group member has notes, move these notes with them to their new group." />
                        <Rock:NotificationBox ID="nbMoveGroupMemberWarning" runat="server" NotificationBoxType="Warning" Visible="false" />
                    </div>
                </div>
                <br />
                <div class="actions">
                    <asp:LinkButton ID="btnMoveGroupMember" runat="server" CssClass="btn btn-primary" Text="Move" ValidationGroup="vgMoveGroupMember" OnClick="btnMoveGroupMember_Click" />
                </div>
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
