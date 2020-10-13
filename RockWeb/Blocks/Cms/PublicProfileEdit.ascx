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

                        <div class="col-md-3">
                            <Rock:ImageEditor ID="imgPhoto" runat="server" Label="Photo" ButtonText="<i class='fa fa-camera'></i>" BinaryFileTypeGuid="03BD8476-8A9F-4078-B628-5B538F967AFC" />
                        </div>

                        <div class="col-md-9">
                            <Rock:DefinedValuePicker ID="dvpTitle" runat="server" CssClass="input-width-md" Label="Title" AutoPostBack="true" />
                            <Rock:DataTextBox ID="tbFirstName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="FirstName" Required="true" />
                            <Rock:DataTextBox ID="tbNickName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="NickName" />
                            <Rock:DataTextBox ID="tbLastName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="LastName" Required="true" />
                            <Rock:DefinedValuePicker ID="dvpSuffix" CssClass="input-width-md" runat="server" Label="Suffix" />
                            <Rock:BirthdayPicker ID="bpBirthDay" runat="server" Label="Birthday" />
                            <Rock:RockRadioButtonList ID="rblRole" runat="server" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal" Label="Role" Visible="false" AutoPostBack="true" OnSelectedIndexChanged="rblRole_SelectedIndexChanged" />
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockRadioButtonList ID="rblGender" runat="server" RepeatDirection="Horizontal" Label="Gender" FormGroupCssClass="gender-picker" Required="true">
                                        <asp:ListItem Text="Male" Value="Male" />
                                        <asp:ListItem Text="Female" Value="Female" />
                                        <asp:ListItem Text="Unknown" Value="Unknown" />
                                    </Rock:RockRadioButtonList>
                                </div>
                                <div class="col-md-6">
                                    <%-- This YearPicker is needed for the GradePicker to work --%>
                                    <div style="display: none;">
                                        <Rock:YearPicker ID="ypGraduation" runat="server" Label="Graduation Year" Help="High School Graduation Year." />
                                    </div>
                                    <Rock:GradePicker ID="ddlGradePicker" runat="server" Label="Grade" UseAbbreviation="true" UseGradeOffsetAsValue="true" Visible="false" />
                                </div>
                            </div>

                            <Rock:CampusPicker ID="cpCampus" runat="server" Label="Campus" />
                        </div>
                    </div>
                    <hr />

                    <asp:Panel ID="pnlPersonAttributes" runat="server">
                            <h3>Additional Information</h3>
                            <Rock:DynamicPlaceholder ID="phPersonAttributes" runat="server" />
                        <hr />
                    </asp:Panel>

                    <asp:Panel ID="pnlFamilyAttributes" runat="server">
                            <h3>Family Information</h3>
                            <Rock:DynamicPlaceholder ID="phFamilyAttributes" runat="server" />
                        <hr />
                    </asp:Panel>

                    <h3>Contact Info</h3>
                    <div class="row">
                        <div class="col-md-12">
                            <div class="form-group">
                                <Rock:DataTextBox ID="tbEmail" PrependText="<i class='fa fa-envelope'></i>" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="Email" Label="Email Address" />
                            </div>

                            <div class="form-group">
                                <Rock:RockRadioButtonList ID="rblEmailPreference" runat="server" RepeatDirection="Horizontal" Label="Email Preference">
                                    <asp:ListItem Text="Email Allowed" Value="EmailAllowed" />
                                    <asp:ListItem Text="No Mass Emails" Value="NoMassEmails" />
                                    <asp:ListItem Text="Do Not Email" Value="DoNotEmail" />
                                </Rock:RockRadioButtonList>

                                <Rock:RockRadioButtonList ID="rblCommunicationPreference" runat="server" RepeatDirection="Horizontal" Label="Communication Preference">
                                    <asp:ListItem Text="Email" Value="1" />
                                    <asp:ListItem Text="SMS" Value="2" />
                                </Rock:RockRadioButtonList>

                                <Rock:NotificationBox ID="nbCommunicationPreferenceWarning" runat="server" NotificationBoxType="Warning" Visible="false" />
                            </div>
                        </div>
                    </div>

                    <asp:Panel ID="pnlPhoneNumbers" runat="server">
                        <h3>Phone Numbers</h3>
                        <div class="form-horizontal">
                            <asp:Repeater ID="rContactInfo" runat="server">
                                <ItemTemplate>
                                    <div id="divPhoneNumberContainer" runat="server" class="form-group">
                                        <div class="control-label col-md-2 mb-2 mb-md-0 text-left text-md-right"><%# Eval("NumberTypeValue.Value") %></div>
                                        <div class="controls col-md-10">
                                            <div class="row">
                                                <div class="col-sm-7">
                                                    <asp:HiddenField ID="hfPhoneType" runat="server" Value='<%# Eval("NumberTypeValueId")  %>' />
                                                    <Rock:PhoneNumberBox ID="pnbPhone" runat="server" CountryCode='<%# Eval("CountryCode")  %>' Number='<%# Eval("NumberFormatted")  %>' />
                                                </div>
                                                <div class="col-sm-5 text-right text-sm-left">
                                                    <Rock:RockCheckBox ID="cbSms" runat="server" Text="SMS" Checked='<%# (bool)Eval("IsMessagingEnabled") %>' DisplayInline="true" CssClass="js-sms-number" />
                                                    <Rock:RockCheckBox ID="cbUnlisted" runat="server" Text="Unlisted" Checked='<%# (bool)Eval("IsUnlisted") %>' DisplayInline="true" />
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>

                        </div>
                    </asp:Panel>

                    <asp:Panel ID="pnlAddress" runat="server">
                        <fieldset>
                            <h3>
                                <asp:Literal ID="lAddressTitle" runat="server" />
                            </h3>

                            <div class="clearfix">
                                <div class="pull-left mb-3">
                                    <asp:Literal ID="lPreviousAddress" runat="server" />
                                </div>
                                <div class="pull-right mb-3">
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

                            <div class="mb-3">
                                <Rock:RockCheckBox ID="cbIsMailingAddress" runat="server" Text="This is my mailing address" Checked="true" />
                                <Rock:RockCheckBox ID="cbIsPhysicalAddress" runat="server" Text="This is my physical address" Checked="true" />
                            </div>
                        </fieldset>
                    </asp:Panel>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </asp:Panel>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
