<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ConnectionCardEntry.ascx.cs" Inherits="RockWeb.Plugins.com_bemadev.CheckIn.ConnectionCardEntry" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" />

        <asp:Panel ID="pnlEntry" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <div class="pull-right">
                    <Rock:HighlightLabel ID="hlCurrentCount" runat="server" LabelType="Info" Text="Current Attendance: 0" />
                </div>
                <h3 class="panel-title"><i class="fa fa-calendar-check-o"></i>Connection Card Entry</h3>
            </div>

            <div class="panel-body">
                <div class="row">
                    <asp:Panel ID="pnlGroupPicker" runat="server" CssClass="col-md-3">
                        <Rock:RockDropDownList ID="ddlGroup" runat="server" Label="Group" Required="true" OnSelectedIndexChanged="ddlGroup_SelectedIndexChanged" AutoPostBack="true" />
                    </asp:Panel>

                    <asp:Panel ID="pnlLocationPicker" runat="server" CssClass="col-md-3">
                        <Rock:RockDropDownList ID="ddlLocation" runat="server" Label="Location" Required="true" OnSelectedIndexChanged="ddlLocation_SelectedIndexChanged" AutoPostBack="true" />
                    </asp:Panel>

                    <asp:Panel ID="pnlSchedulePicker" runat="server" CssClass="col-md-3">
                        <Rock:RockDropDownList ID="ddlSchedule" runat="server" Label="Schedule" Required="true" OnSelectedIndexChanged="ddlSchedule_SelectedIndexChanged" AutoPostBack="true" />
                    </asp:Panel>

                    <asp:Panel ID="pnlDatePicker" runat="server" CssClass="col-md-3">
                        <Rock:DatePicker ID="dpAttendanceDate" runat="server" Required="true" Label="Attendance Date" Help="The date to use when recording the person as having attended." OnTextChanged="dpAttendanceDate_TextChanged" AutoPostBack="true" />
                    </asp:Panel>
                </div>

                <hr />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:PersonPicker ID="ppAttendee" runat="server" Label="Attendee" Required="false" CssClass="js-attendee" OnSelectPerson="ppAttendee_SelectPerson" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBoxList ID="cblFamilyMembers" Label="Mark Attendance for the following:" ValidateRequestMode="Disabled" Visible="false" runat="server" RepeatDirection="Horizontal" />
                    </div>
                </div>
                <br />
                <div class="row">
                    <div class="col-md-6">
                        <div class="row">
                            <div class="col-md-4">
                                <Rock:RockTextBox ID="tbFirstName" Label="First Name" runat="server" />
                            </div>
                            <div class="col-md-4">
                                <Rock:RockTextBox ID="tbLastName" Label="Last Name" runat="server" />
                            </div>
                            <div class="col-md-4">
                                <Rock:RockTextBox ID="tbSpouseName" Label="Spouse" runat="server" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:EmailBox ID="tbEmail" Label="Email" runat="server" />
                            </div>
                            <div class="col-md-6">
                                <Rock:PhoneNumberBox ID="pnPhone" Label="Phone" runat="server" />
                            </div>
                        </div>

                        <Rock:BirthdayPicker ID="bpBirthDay" runat="server" Label="Birthday" />

                        <h4>Address</h4>
                        <asp:Panel ID="pnlAddress" runat="server">
                            <fieldset>
                                <legend>
                                    <asp:Literal ID="lAddressTitle" runat="server" /></legend>

                                <div class="clearfix">
                                    <div class="pull-left margin-b-md">
                                        <asp:Literal ID="lPreviousAddress" runat="server" />
                                    </div>
                                    <div class="pull-right">
                                        <asp:LinkButton ID="lbMoved" CssClass="btn btn-default btn-xs" runat="server" OnClick="lbMoved_Click"><i class="fa fa-truck"></i> Moved</asp:LinkButton>
                                    </div>
                                </div>

                                <asp:HiddenField ID="hfStreet1" runat="server" />
                                <asp:HiddenField ID="hfStreet2" runat="server" />
                                <asp:HiddenField ID="hfCity" runat="server" />
                                <asp:HiddenField ID="hfState" runat="server" />
                                <asp:HiddenField ID="hfPostalCode" runat="server" />
                                <asp:HiddenField ID="hfCountry" runat="server" />

                                <Rock:AddressControl ID="acAddress" runat="server" RequiredErrorMessage="Your Address is Required" />

                                <div class="margin-b-md">
                                    <Rock:RockCheckBox ID="cbIsMailingAddress" runat="server" Text="This is my mailing address" Checked="true" />
                                    <Rock:RockCheckBox ID="cbIsPhysicalAddress" runat="server" Text="This is my physical address" Checked="true" />
                                </div>
                            </fieldset>
                        </asp:Panel>

                        <h4>Children</h4>
                        <div class="grid">
                            <Rock:ModalAlert ID="maChildrenGridWarning" runat="server" />
                            <Rock:Grid ID="gChildren" runat="server" AllowPaging="false" RowItemText="Location" ShowConfirmDeleteDialog="false">
                                <Columns>
                                    <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                                    <Rock:RockBoundField DataField="BirthDate" HeaderText="Birth Date" />
                                    <Rock:RockBoundField DataField="Grade.Description" HeaderText="Grade" />
                                    <Rock:EditField OnClick="gChildren_Edit" />
                                </Columns>
                            </Rock:Grid>
                        </div>

                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBoxList ID="cblInterests" runat="server" Label="I'm interested in" RepeatDirection="Vertical" RepeatColumns="2" AutoPostBack="true" OnSelectedIndexChanged="cblInterests_SelectedIndexChanged">
                            <asp:ListItem Text="First Steps" Value="FirstSteps" />
                            <asp:ListItem Text="Growth Track: Journey" Value="Journey" />
                            <asp:ListItem Text="Growth Track: Bible Discovery" Value="Discovery" />
                            <asp:ListItem Text="Growth Track: Story" Value="Story" />
                            <asp:ListItem Text="Baptism" Value="Baptism" />
                            <asp:ListItem Text="Serving" Value="Serving" />
                            <asp:ListItem Text="LIFE Groups" Value="LifeGroups" />
                        </Rock:RockCheckBoxList>

                        <Rock:RockTextBox ID="tbPrayerRequests" runat="server" Label="Prayer Requests" TextMode="MultiLine" Rows="3" />
                        <Rock:RockTextBox ID="tbComments" runat="server" Label="Comments" TextMode="MultiLine" Rows="3" />

                        <div class="well">
                            <Rock:RockRadioButtonList ID="rblLifeChoice" runat="server">
                                <asp:ListItem Text="I've made the decision to start trusting Jesus" Value="Committed" />
                                <asp:ListItem Text="I still have questions about what it means to trust Jesus" Value="Questioning" />
                            </Rock:RockRadioButtonList>

                            <Rock:RockTextBox ID="tbContactInfo" runat="server" Label="Name and Contact" TextMode="MultiLine" Rows="2" />
                        </div>
                    </div>
                </div>

                <div class="action pull-right">
                    <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-primary" AccessKey="s" Text="Save and Add New Person" OnClick="btnSave_Click" />
                </div>

                <div class="col-md-8">
                    <Rock:NotificationBox ID="nbInactivePerson" runat="server" NotificationBoxType="Warning" Text="This person record is currently inactive." Visible="false" />
                    <Rock:NotificationBox ID="nbAttended" runat="server" NotificationBoxType="Success" Text="" />
                </div>
            </div>
        </asp:Panel>

        <div class="row">
            <div class="col-md-12">
                <div class="pull-right padding-r-sm">
                    <asp:CheckBox ID="cbShowCurrent" runat="server" OnCheckedChanged="cbShowCurrent_CheckedChanged" Text="Show Current Attendees" AutoPostBack="true" />
                </div>
            </div>
        </div>

        <asp:Panel ID="pnlCurrentAttendees" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h3 class="panel-title">Current Attendees</h3>
            </div>

            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:Grid ID="gAttendees" runat="server" DataKeyNames="Id">
                        <Columns>
                            <asp:BoundField DataField="PersonAlias.Person.FullNameReversed" HeaderText="Name" />
                            <asp:BoundField DataField="CreatedByPersonName" HeaderText="Entered By" />
                            <Rock:DeleteField OnClick="gAttendeesDelete_Click"></Rock:DeleteField>
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>

        <Rock:ModalDialog ID="dlgChild" runat="server" Title="Select Location" OnSaveThenAddClick="dlgChild_SaveThenAddClick" OnSaveClick="dlgChild_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="Child">
            <Content>
                <asp:HiddenField ID="hfChildGuid" runat="server" />
                <asp:ValidationSummary ID="valChildSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="Child" />
                <div class="row">
                    <div class="col-md-4">
                        <Rock:RockTextBox ID="tbChildName" runat="server" Label="Name" />
                    </div>
                    <div class="col-md-4">
                        <Rock:DatePicker ID="bpChildBirthdate" Label="Birthdate" runat="server" />
                    </div>
                    <div class="col-md-4">
                        <%-- This YearPicker is needed for the GradePicker to work --%>
                        <div style="display: none;">
                            <Rock:YearPicker ID="ypGraduation" runat="server" Label="Graduation Year" Help="High School Graduation Year." />
                        </div>
                        <Rock:GradePicker ID="gpChildGrade" Label="Grade" runat="server" />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
