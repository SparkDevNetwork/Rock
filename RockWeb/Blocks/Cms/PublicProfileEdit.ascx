<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PublicProfileEdit.ascx.cs" Inherits="RockWeb.Blocks.Cms.PublicProfileEdit" %>

<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-user"></i>
                    My Account
                </h1>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbNotAuthorized" runat="server" Text="You must be logged in to view your account." NotificationBoxType="Danger" Visible="false" />

                <asp:HiddenField ID="hfGroupId" runat="server" />

                <%-- View Panel --%>
                <asp:Panel ID="pnlView" CssClass="panel-view" runat="server">
                    <asp:Literal ID="lViewPersonContent" runat="server" />
                </asp:Panel>

                <asp:HiddenField ID="hfEditPersonGuid" runat="server" />

                <%-- Edit Panel --%>
                <asp:Panel ID="pnlEdit" runat="server">
                    <asp:ValidationSummary ID="valValidation" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                    <div class="row">
                        <div class="col-md-4 pull-right">
                            <Rock:CampusPicker ID="cpCampus" runat="server" Label="Campus" />
                        </div>
                    </div>
                    <div class="row">

                        <div class="col-md-3">
                            <Rock:ImageEditor ID="imgPhoto" runat="server" Label="Photo" ButtonText="<i class='fa fa-camera'></i>" BinaryFileTypeGuid="03BD8476-8A9F-4078-B628-5B538F967AFC" />
                        </div>

                        <div class="col-md-9">
                            <h3>Personal Information</h3>
                            <hr class="mt-0" />
                            <asp:Panel ID="pnlPersonalTitle" runat="server" CssClass="row">
                                <div class="col-md-4">
                                    <Rock:DefinedValuePicker ID="dvpTitle" runat="server" CssClass="input-width-md" Label="Title" AutoPostBack="true" />
                                </div>
                            </asp:Panel>
                            <div class="row form-row">
                                <div class="col-md-6">
                                    <Rock:DataTextBox ID="tbFirstName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="FirstName" Required="true" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:DataTextBox ID="tbLastName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="LastName" Required="true" />
                                </div>
                            </div>
                            <div class="row form-row">
                                <div class="col-md-6">
                                    <Rock:DataTextBox ID="tbNickName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="NickName" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:DefinedValuePicker ID="dvpSuffix" CssClass="input-width-md" runat="server" Label="Suffix" />
                                </div>
                            </div>
                            <div class="row form-row">
                                <div class="col-md-6">
                                    <Rock:BirthdayPicker ID="bpBirthDay" runat="server" Label="Birthday" />
                                </div>
                                <div class="col-md-6">
                                    <%-- This YearPicker is needed for the GradePicker to work --%>
                                    <div style="display: none;">
                                        <Rock:YearPicker ID="ypGraduation" runat="server" Label="Graduation Year" Help="High School Graduation Year." />
                                    </div>
                                    <Rock:GradePicker ID="ddlGradePicker" runat="server" Label="Grade" UseAbbreviation="true" UseGradeOffsetAsValue="true" Visible="false" />

                                </div>
                                <div class="col-md-6">

                                    <Rock:RockRadioButtonList ID="rblRole" runat="server" RepeatDirection="Horizontal" Label="Role" Visible="false" AutoPostBack="true" OnSelectedIndexChanged="rblRole_SelectedIndexChanged" />
                                    <Rock:RockRadioButtonList ID="rblGender" runat="server" RepeatDirection="Horizontal" Label="Gender" FormGroupCssClass="gender-picker" Required="true">
                                        <asp:ListItem Text="Male" Value="Male" />
                                        <asp:ListItem Text="Female" Value="Female" />
                                    </Rock:RockRadioButtonList>
                                </div>
                            </div>

                            <div class="row form-row">
                                <div class="col-md-6">
                                    <Rock:RacePicker ID="rpRace" runat="server" />
                                </div>

                                <div class="col-md-6">
                                    <Rock:EthnicityPicker ID="epEthnicity" runat="server"/>
                                </div>
                            </div>

                            <asp:Panel ID="pnlPersonAttributes" runat="server">
                                <Rock:AttributeValuesContainer ID="avcPersonAttributesAdult" runat="server" NumberOfColumns="2" />
                                <Rock:AttributeValuesContainer ID="avcPersonAttributesChild" runat="server" NumberOfColumns="2" />
                            </asp:Panel>

                            <asp:Panel ID="pnlFamilyAttributes" runat="server">
                                <h3>Family Information</h3>
                                <hr class="mt-0" />
                                <Rock:AttributeValuesContainer ID="avcFamilyAttributes" runat="server" NumberOfColumns="2" />
                            </asp:Panel>

                            <h3>Contact Information</h3>
                            <hr class="mt-0" />
                            <asp:Panel ID="pnlPhoneNumbers" class="form-section" runat="server">
                                <asp:Repeater ID="rContactInfo" runat="server">
                                    <ItemTemplate>
                                        <div id="divPhoneNumberContainer" runat="server" class="form-group">
                                            <asp:Literal ID="litHighlightTitle" runat="server" Visible="false"></asp:Literal>
                                            <asp:Literal ID="litHighlightText" runat="server" Visible="false"></asp:Literal>
                                            <label class="control-label"><%# Eval("NumberTypeValue.Value") %> Phone</label>
                                            <div class="controls">
                                                <asp:HiddenField ID="hfPhoneType" runat="server" Value='<%# Eval("NumberTypeValueId")  %>' />
                                                <Rock:PhoneNumberBox ID="pnbPhone" runat="server" CountryCode='<%# Eval("CountryCode")  %>' Number='<%# Eval("NumberFormatted")  %>' />
                                                <Rock:RockCheckBox ID="cbSms" runat="server" Text="I would like to receive important text messages" Checked='<%# (bool)Eval("IsMessagingEnabled") %>' ContainerCssClass="mb-0" CssClass="js-sms-number" Visible='<%# (int?)Eval("NumberTypeValueId") == Rock.Web.Cache.DefinedValueCache.GetId( new Guid(Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ) ) %>' />
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </asp:Panel>

                            <div class="form-section">
                                <Rock:DataTextBox ID="tbEmail" PrependText="<i class='fa fa-envelope'></i>" FormGroupCssClass="email-box" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="Email" Label="Email Address" />

                                <Rock:RockRadioButtonList ID="rblEmailPreference" runat="server" RepeatDirection="Horizontal" Label="Email Preference">
                                    <asp:ListItem Text="All Emails" Value="EmailAllowed" />
                                    <asp:ListItem Text="Only Personalized" Value="NoMassEmails" />
                                    <asp:ListItem Text="Do Not Email" Value="DoNotEmail" />
                                </Rock:RockRadioButtonList>

                                <Rock:RockRadioButtonList ID="rblCommunicationPreference" runat="server" RepeatDirection="Horizontal" Label="Communication Preference">
                                    <asp:ListItem Text="Email" Value="1" />
                                    <asp:ListItem Text="SMS" Value="2" />
                                </Rock:RockRadioButtonList>

                                <Rock:NotificationBox ID="nbCommunicationPreferenceWarning" runat="server" NotificationBoxType="Warning" Visible="false" />
                            </div>

                            <asp:Panel ID="pnlAddress" runat="server">
                                <div class="clearfix mb-2">
                                    <span class="control-label pull-left">
                                        <asp:Literal ID="lAddressTitle" runat="server" /></span>
                                    <asp:Literal ID="lPreviousAddress" runat="server" />
                                    <asp:LinkButton ID="lbMoved" CssClass="btn btn-default btn-xs pull-right" runat="server" OnClick="lbMoved_Click"><i class="fa fa-truck"></i> Moved</asp:LinkButton>
                                </div>
                                <asp:HiddenField ID="hfStreet1" runat="server" />
                                <asp:HiddenField ID="hfStreet2" runat="server" />
                                <asp:HiddenField ID="hfCity" runat="server" />
                                <asp:HiddenField ID="hfState" runat="server" />
                                <asp:HiddenField ID="hfPostalCode" runat="server" />
                                <asp:HiddenField ID="hfCountry" runat="server" />

                                <Rock:AddressControl ID="acAddress" runat="server" RequiredErrorMessage="Your Address is Required" />

                                <div class="row form-row -mt-form-group">
                                    <div class="col-md-6">
                                        <Rock:RockCheckBox ID="cbIsMailingAddress" runat="server" Text="This is my mailing address" Checked="true" /></div>
                                    <div class="col-md-6">
                                        <Rock:RockCheckBox ID="cbIsPhysicalAddress" runat="server" Text="This is my physical address" Checked="true" /></div>
                                </div>
                            </asp:Panel>

                            <div class="actions">
                                <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                                <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />

                                <div class="pull-right">
                                    <Rock:HighlightLabel ID="hlblSuccess" runat="server" LabelType="Success" Text="Saved" Visible="false" />
                                </div>
                            </div>

                        </div>
                    </div>

                </asp:Panel>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
