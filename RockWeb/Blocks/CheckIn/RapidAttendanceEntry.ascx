<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RapidAttendanceEntry.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.RapidAttendanceEntry" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" />
        <script type="text/javascript">
            Sys.Application.add_load(function () {
                $('.js-attendance-item').find('input').change(function () {
                    $('#<%= hfAttendanceDirty.ClientID %>').val('true')
                });

                $('.js-person-item').find('input').change(function () {
                    $('#<%= hfPersonDirty.ClientID %>').val('true')
                });

                $('.js-person-item').find('textarea').blur(function () {
                    $('#<%= hfPersonDirty.ClientID %>').val('true')
                });

                $('.js-main-event').click(function () {
                    if (isDirty()) {
                        return false;
                    }
                });

                 $('.js-person-event').click(function () {
                    if (isContactItemDirty()) {
                        return false;
                    }
                });
            });

               // handle onkeypress for the account amount input boxes
            function handleSearchKeyPress(element, keyCode) {
                // if Enter was pressed, click the Go button.
                if (keyCode == 13) {
                    $('#<%=btnGo.ClientID%>')[0].click();
                    return false;
                }
            }

            function isDirty() {
                if ($('#<%= hfAttendanceDirty.ClientID %>').val() == 'true' || $('#<%= hfPersonDirty.ClientID %>').val() == 'true') {
                    if (confirm('You have not saved your changes. Are you sure you want to continue?')) {
                        return false;
                    }
                    return true;
                }
                return false;
            }

            function isContactItemDirty() {
                if ($('#<%= hfPersonDirty.ClientID %>').val() == 'true') {
                    if (confirm('You have not saved your changes. Are you sure you want to continue?')) {
                        return false;
                    }
                    return true;
                }
                return false;
            }

        </script>

        <asp:Panel ID="pnlStart" runat="server" Visible="false">
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-user-friends"></i> Contact Entry</h1>
                </div>
                <div class="panel-body">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:GroupPicker ID="gpGroups" runat="server" Label="Group" Visible="false" Required="true" OnSelectItem="ddlGroup_SelectedIndexChanged" ValidationGroup="AttendanceSetting" />
                            <asp:Panel ID="pnlGroupPicker" runat="server">
                                <Rock:RockDropDownList ID="ddlGroup" runat="server" Label="Group" Visible="false" Required="true" OnSelectedIndexChanged="ddlGroup_SelectedIndexChanged" AutoPostBack="true" ValidationGroup="AttendanceSetting" />
                            </asp:Panel>
                            <asp:Panel ID="pnlLocationPicker" runat="server">
                                <Rock:RockDropDownList ID="ddlLocation" runat="server" Label="Location" Required="true" OnSelectedIndexChanged="ddlLocation_SelectedIndexChanged" AutoPostBack="true" ValidationGroup="AttendanceSetting" />
                            </asp:Panel>
                            <asp:Panel ID="pnlSchedulePicker" runat="server">
                                <Rock:RockDropDownList ID="ddlSchedule" runat="server" Label="Schedule" Required="true" ValidationGroup="AttendanceSetting" />
                            </asp:Panel>
                            <Rock:DatePicker ID="dpAttendanceDate" runat="server" Required="true" Label="Attendance Date" ValidationGroup="AttendanceSetting" />
                        </div>
                    </div>
                    <div class="actions">
                        <Rock:BootstrapButton ID="lbStart" runat="server" Text="Start" CssClass="btn btn-primary pull-right" CausesValidation="true" OnClick="lbStart_Click" ValidationGroup="AttendanceSetting" />
                    </div>
                </div>
            </div>
        </asp:Panel>
        <asp:Panel ID="pnlMainPanel" runat="server" Visible="false">
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-user-friends"></i> Contact Entry</h1>
                    <div class="panel-labels">
                        <Rock:HighlightLabel ID="hlAttendance" runat="server" LabelType="Info" />
                        <Rock:HighlightLabel ID="hlCurrentCount" runat="server" LabelType="Success" />
                    </div>
                    <asp:LinkButton ID="lbSetting" runat="server" CssClass="btn btn-default btn-square margin-l-sm btn-xs" OnClick="lbSetting_Click" CausesValidation="false"><i class="fa fa-cog"></i></asp:LinkButton>
                </div>
                <div>
                    <asp:HiddenField ID="hfPersonGuid" runat="server" />
                    <div class="panel-inline-heading">
                        <div class="row d-flex no-gutters">
                            <div class="col-md-3 d-flex align-items-center panel-sidebar">
                                <div class="d-flex width-full padding-all-sm">
                                    <Rock:RockTextBox ID="tbSearch" runat="server" CssClass="resource-search js-resource-search flex-grow-1" PrependText="<i class='fa fa-search'></i>" Placeholder="Search" spellcheck="false" onkeydown="javascript:return handleSearchKeyPress(this, event.keyCode);" onkeyup="javascript:handleSearchKeyPress(event.keyCode)" />
                                    <asp:LinkButton ID="btnGo" runat="server" CssClass="btn btn-primary js-main-event" Text="Go" OnClick="btnGo_Click" />
                                </div>
                            </div>
                            <div class="col-md-9">
                                <div class="padding-all-md clearfix">
                                    <div class="pull-left">
                                        <asp:Literal ID="lFamilyDetail" runat="server" />
                                    </div>
                                    <div class="pull-right">
                                        <asp:LinkButton ID="lbEditFamily" runat="server" CssClass="btn btn-xs btn-square btn-default js-main-event" CausesValidation="false" OnClick="lbEditFamily_Click"> <i title="Edit Family" class="fa fa-pencil"></i></asp:LinkButton>
                                        <asp:LinkButton ID="lbAddMember" runat="server" CssClass="btn btn-xs btn-square btn-default js-main-event" CausesValidation="false" OnClick="lbAddMember_Click"> <i title="Add Member" class="fa fa-user-plus"></i></asp:LinkButton>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="row d-flex no-gutters">
                        <div class="col-md-3 panel-sidebar styled-scroll mh-300">
                            <div class="d-flex flex-column h-100">
                                <div class="position-relative flex-column overflow-auto flex-shrink-1 flex-grow-1 h-100">
                                <div class="abs-scroll-list padding-all-sm">
                                    <asp:Repeater ID="rptResults" runat="server" OnItemCommand="rptResults_ItemCommand">
                                        <ItemTemplate>
                                            <div class="family-select-item <%# (bool)Eval("IsActive") ? "" :"is-inactive" %> <%# SelectedPersonId.HasValue && (int)Eval("Id") == SelectedPersonId.Value ? "active" :"" %>">
                                                <asp:LinkButton ID="lbPerson" runat="server" CommandArgument='<%# Eval("Id") %>' CommandName="Display" CssClass="js-main-event">
                                                <div>
                                                        <h5 class="strong d-inline"><%# Eval("Name") %></h5> <span class="age"><%# Eval("Age") %></span>

                                                        <% if ( IsAttendanceEnabled )
                                                            { %>   <i class="fa fa-circle <%# (bool)Eval("IsAttended") ? "text-success":"text-muted" %> pull-right" aria-hidden="true"></i> <% } %>
                                                        <%# !string.IsNullOrEmpty((string)Eval("CampusName")) ? "<span class='label label-campus pull-right margin-r-sm'>"+ Eval("CampusName").ToString() +"</span>" :"" %>
                                                </div>
                                                        <%# !string.IsNullOrEmpty((string)Eval("FamilyMemberNames")) ?  "<span class='family-member-names'>"+Eval("FamilyMemberNames").ToString() +"</span>" :"" %>
                                                    <div class="margin-t-sm small">
                                                        <%# !string.IsNullOrEmpty((string)Eval("Email")) ? Eval("Email").ToString() +"<br/>" :"" %>
                                                        <%# Eval("Address") != null ? Eval("Address.FormattedAddress").ToString() +"<br/>" :"" %>
                                                        <%# !string.IsNullOrEmpty((string)Eval("Mobile")) ? "M: "+Eval("Mobile").ToString() +"<br/>" :"" %>
                                                    </div>
                                                </asp:LinkButton>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                    </div>
                                </div>
                                <div class="actions padding-all-sm padding-t-md">
                                    <asp:LinkButton ID="lbAddFamily" runat="server" CssClass="btn btn-default btn-block js-main-event" OnClick="lbAddFamily_Click"><i class="fa fa-users"></i> Add Family</asp:LinkButton>
                                </div>
                            </div>
                        </div>
                        <div class="col-md-9">
                            <asp:Panel ID="pnlMainEntry" runat="server" Visible="false" CssClass="padding-all-md">
                                <Rock:RockControlWrapper ID="rcwAttendance" runat="server">
                                    <asp:HiddenField ID="hfAttendanceDirty" runat="server" Value="false" />
                                    <Rock:RockCheckBoxList ID="rcbAttendance" runat="server" Label="Attendance" RepeatDirection="Horizontal" CssClass="js-attendance-item" />
                                    <Rock:BootstrapButton ID="lbSaveAttendance" CssClass="btn btn-xs btn-primary" runat="server" DataLoadingText="&lt;i class='fa fa-refresh fa-spin'&gt;&lt;/i&gt; Saving" CompletedText="Saved" CompletedDuration="3" OnClick="lbSaveAttendance_Click" Text="Save Attendance" />
                                </Rock:RockControlWrapper>
                                <hr />
                                <ul class="nav nav-tabs">
                                    <asp:Repeater ID="rptPersons" runat="server" OnItemDataBound="rptPersons_ItemDataBound" OnItemCommand="rptPersons_ItemCommand">
                                        <ItemTemplate>
                                            <li class="<%# ((Guid)Eval("Guid")).ToString() == hfPersonGuid.Value ? "active" :"" %>">
                                                <asp:LinkButton ID="lbSelectedPerson" runat="server" CommandArgument='<%# Eval("Guid") %>' CommandName="Display" CssClass="js-person-event">
                                                    <div class="d-flex align-items-center">
                                                        <div class="photo-round photo-round-sm margin-r-sm" id="divPersonImage" runat="server"></div>
                                                        <span>
                                                            <%#Eval("NickName") %>
                                                        </span>
                                                    </div>
                                                </asp:LinkButton>
                                            </li>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </ul>
                                <asp:HiddenField ID="hfPersonDirty" runat="server" Value="false" />
                                <asp:LinkButton ID="lbEditPerson" runat="server" CssClass="btn btn-xs btn-square btn-default pull-right margin-t-lg js-main-event" CausesValidation="false" OnClick="lbEditPerson_Click"> <i title="Edit Person" class="fa fa-pencil"></i></asp:LinkButton>
                                <asp:Literal ID="lPersonDetail" runat="server" />

                                <Rock:RockCheckBoxList ID="rcbWorkFlowTypes" runat="server" RepeatDirection="Horizontal" RepeatColumns="3" FormGroupCssClass="margin-b-lg" CssClass="js-person-item" />
                                <Rock:RockControlWrapper ID="rcwNotes" runat="server" CssClass="margin-b-lg">
                                    <Rock:RockDropDownList ID="ddlNoteType" runat="server" CssClass="input-width-xl input-xs pull-right" OnSelectedIndexChanged="ddlNoteType_SelectedIndexChanged" AutoPostBack="true"/>
                                    <Rock:RockTextBox ID="tbNote" runat="server" Label="Note" TextMode="MultiLine" Rows="3" CssClass="js-person-item" />
                                </Rock:RockControlWrapper>
                                <asp:Panel ID="pnlPrayerRequest" runat="server">
                                    <Rock:RockTextBox ID="tbPrayerRequest" runat="server"  Label="Prayer Request" TextMode="MultiLine" Rows="3" CssClass="js-person-item" />
                                    <div class="row">
                                        <asp:Panel runat="server" ID="pnlIsUrgent" CssClass="col-md-4">
                                            <Rock:RockCheckBox ID="cbIsUrgent" Label="Urgent" runat="server" />
                                        </asp:Panel>
                                        <asp:Panel runat="server" ID="pnlIsPublic" CssClass="col-md-4">
                                            <Rock:RockCheckBox ID="cbIsPublic" Label="Public" runat="server" />
                                        </asp:Panel>
                                        <div class="col-md-4">
                                            <Rock:CategoryPicker ID="cpPrayerCategory" runat="server" Label="Prayer Category"  EntityTypeName="Rock.Model.PrayerRequest" />
                                        </div>
                                    </div>
                                </asp:Panel>
                                <div class="actions margin-t-md">
                                    <Rock:BootstrapButton ID="bbtnSaveContactItems" CssClass="btn btn-primary btn-xs" runat="server" AccessKey="s" ToolTip="Alt+s" DataLoadingText="&lt;i class='fa fa-refresh fa-spin'&gt;&lt;/i&gt; Saving"
                                    CompletedText="Success" CompletedMessage="<div class='margin-t-md alert alert-success'>Changes have been saved.</div>" Text="Save Contact Items" OnClick="bbtnSaveContactItems_Click" />
                                </div>
                        </asp:Panel>
                        <asp:Panel ID="pnlEditFamily" runat="server" Visible="false" CssClass="padding-all-md">
                                <fieldset>
                                    <h4>Family Edit</h4>
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

                                    <Rock:AddressControl ID="acAddress" runat="server" RequiredErrorMessage="Your Address is Required" ValidationGroup="vgEditFamily" />

                                    <div class="margin-b-md row">
                                        <div class="col-md-4">
                                            <Rock:RockCheckBox ID="cbIsMailingAddress" runat="server" Text="Mailing Address" Checked="true" />
                                        </div>
                                        <div class="col-md-4">
                                            <Rock:RockCheckBox ID="cbIsPhysicalAddress" runat="server" Text="Physical Address" Checked="true" />
                                        </div>
                                    </div>

                                    <Rock:AttributeValuesContainer ID="avcFamilyAttributes" runat="server" ValidationGroup="vgEditFamily" />
                                </fieldset>
                                <%-- Edit Family Buttons --%>
                                <div class="actions">
                                    <asp:LinkButton ID="lbSaveFamily" runat="server" CssClass="btn btn-primary" Text="Save" CausesValidation="true" ValidationGroup="vgEditFamily" OnClick="lbSaveFamily_Click" />
                                    <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click">Cancel</asp:LinkButton>
                                </div>
                        </asp:Panel>
                        <asp:Panel ID="pnlEditMember" runat="server" Visible="false" CssClass="padding-all-md">
                            <Rock:RockRadioButtonList ID="rblRole" runat="server" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal" Label="Role" Visible="false" AutoPostBack="true" OnSelectedIndexChanged="rblRole_SelectedIndexChanged" ValidationGroup="vgEditMember" />
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:DataTextBox ID="tbFirstName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="FirstName" Required="true" ValidationGroup="vgEditMember" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:DataTextBox ID="tbLastName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="LastName" Required="true" ValidationGroup="vgEditMember" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:DefinedValuePicker ID="dvpSuffix" CssClass="input-width-md" runat="server" Label="Suffix" ValidationGroup="vgEditMember" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:BirthdayPicker ID="bpBirthDay" runat="server" Label="Birthday" ValidationGroup="vgEditMember" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockRadioButtonList ID="rblGender" runat="server" RepeatDirection="Horizontal" Label="Gender" FormGroupCssClass="gender-picker" Required="true" ValidationGroup="vgEditMember">
                                        <asp:ListItem Text="Male" Value="Male" />
                                        <asp:ListItem Text="Female" Value="Female" />
                                        <asp:ListItem Text="Unknown" Value="Unknown" />
                                    </Rock:RockRadioButtonList>
                                </div>
                                <div class="col-md-6">
                                    <Rock:DefinedValuePicker ID="dvpMaritalStatus" runat="server" Label="Marital Status" Visible="false" Required="false" ValidationGroup="vgEditMember" />
                                    <Rock:GradePicker ID="gpGradePicker" runat="server" Label="Grade" UseAbbreviation="true" UseGradeOffsetAsValue="true" Visible="false" ValidationGroup="vgEditMember" />
                                </div>
                            </div>
                            <hr />
                            <asp:Panel ID="pnlPersonAttributes" runat="server">
                                <h4>
                                    <asp:Literal ID="lPersonAttributeTitle" runat="server" />
                                </h4>
                                <Rock:AttributeValuesContainer ID="avcPersonAttributes" runat="server" ValidationGroup="vgEditMember" ShowCategoryLabel="false" />
                                <hr />
                            </asp:Panel>
                            <h4>Contact Information</h4>
                            <asp:Panel ID="pnlPhoneNumbers" runat="server">
                                <div class="form-horizontal">
                                    <asp:Repeater ID="rContactInfo" runat="server">
                                        <ItemTemplate>
                                            <div id="divPhoneNumberContainer" runat="server" class="form-group">
                                                <div class="control-label col-md-1"><%# Eval("NumberTypeValue.Value")  %></div>
                                                <div class="controls col-md-11">
                                                    <div class="row">
                                                        <div class="col-md-7">
                                                            <asp:HiddenField ID="hfPhoneType" runat="server" Value='<%# Eval("NumberTypeValueId")  %>' />
                                                            <Rock:PhoneNumberBox ID="pnbPhone" runat="server" CountryCode='<%# Eval("CountryCode")  %>' Number='<%# Eval("NumberFormatted")  %>' />
                                                        </div>
                                                        <div class="col-md-5">
                                                            <div class="row">
                                                                <div class="col-md-6">
                                                                    <asp:CheckBox ID="cbSms" runat="server" Text="SMS" Checked='<%# (bool)Eval("IsMessagingEnabled") %>' CssClass="js-sms-number" />
                                                                </div>
                                                                <div class="col-md-6">
                                                                    <asp:CheckBox ID="cbUnlisted" runat="server" Text="Unlisted" Checked='<%# (bool)Eval("IsUnlisted") %>' />
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </div>
                            </asp:Panel>
                            <asp:Panel ID="pnlEmail" runat="server">
                                <div class="row">
                                    <div class="col-md-12">
                                        <div class="form-group emailgroup">
                                            <div class="form-row">
                                                <div class="col-sm-6">
                                                    <Rock:EmailBox ID="tbEmail" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="Email" />
                                                </div>
                                                <div class="col-sm-3 form-align">
                                                    <Rock:RockCheckBox ID="cbIsEmailActive" runat="server" Text="Email Is Active" DisplayInline="true" />
                                                </div>
                                            </div>
                                        </div>
                                        <div class="form-group">
                                            <Rock:RockRadioButtonList ID="rblCommunicationPreference" runat="server" RepeatDirection="Horizontal" Label="Communication Preference" ValidationGroup="vgEditMember">
                                                <asp:ListItem Text="Email" Value="1" />
                                                <asp:ListItem Text="SMS" Value="2" />
                                            </Rock:RockRadioButtonList>
                                        </div>
                                    </div>
                                </div>
                            </asp:Panel>
                            <div class="actions">
                                <asp:LinkButton ID="lbSaveMember" runat="server" CssClass="btn btn-primary" Text="Save" CausesValidation="true" ValidationGroup="vgEditMember" OnClick="lbSaveMember_Click" />
                                <asp:LinkButton ID="lbCancelMember" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click">Cancel</asp:LinkButton>
                            </div>
                        </asp:Panel>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
