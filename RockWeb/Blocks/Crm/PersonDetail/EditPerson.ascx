<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EditPerson.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.EditPerson" %>


<%--
    ******************************************************************************************************************************
    * NOTE: The Security/AccountEdit.ascx block has very similar functionality.  If updating this block, make sure to check
    * that block also.  It may need the same updates.
    ******************************************************************************************************************************
--%>


<asp:UpdatePanel ID="upEditPerson" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-user"></i>
                    <asp:Literal ID="lTitle" runat="server" /></h1>
            </div>

            <div class="panel-body">
                <asp:ValidationSummary ID="valValidation" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <div class="row">

                    <div class="col-md-3">
                        <div class="well form-well">
                            <Rock:RockLiteral ID="lRecordStatusReadOnly" runat="server" Label="Record Status" />
                            <Rock:RockLiteral ID="lReasonReadOnly" runat="server" Label="Reason" />
                            <Rock:RockLiteral ID="lReasonNoteReadOnly" runat="server" Label="Inactive Reason Note" />
                            <Rock:DefinedValuePicker ID="dvpRecordStatus" runat="server" Label="Record Status" AutoPostBack="true" OnSelectedIndexChanged="ddlRecordStatus_SelectedIndexChanged" />
                            <Rock:DefinedValuePicker ID="dvpReason" runat="server" Label="Reason" Visible="false"></Rock:DefinedValuePicker>
                            <Rock:RockTextBox ID="tbInactiveReasonNote" runat="server" Label="Inactive Reason Note" TextMode="MultiLine" Rows="2" Visible="false" autocomplete="off"></Rock:RockTextBox>
                            <Rock:ImageEditor ID="imgPhoto" runat="server" Label="Photo" BinaryFileTypeGuid="03BD8476-8A9F-4078-B628-5B538F967AFC" />
                        </div>
                    </div>

                    <div class="col-md-9">

                        <div class="well form-well">
                            <fieldset>

                                <div class="form-row">
                                    <div class="col-md-6">
                                        <Rock:DefinedValuePicker ID="dvpTitle" runat="server" CssClass="input-width-md" Label="Title" />
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:DataTextBox ID="tbFirstName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="FirstName" autocomplete="off" />
                                    </div>
                                </div>
                                <div class="form-row">
                                    <div class="col-md-6">
                                        <Rock:DataTextBox ID="tbNickName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="NickName" Label="Nickname" autocomplete="off" />
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:DataTextBox ID="tbMiddleName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="MiddleName" autocomplete="off" />
                                    </div>
                                </div>
                                <div class="form-row">
                                    <div class="col-md-6">
                                        <Rock:DataTextBox ID="tbLastName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="LastName" autocomplete="off" />
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:DefinedValuePicker ID="dvpSuffix" CssClass="input-width-md" runat="server" Label="Suffix" />
                                    </div>
                                </div>

                                <div class="form-row">
                                    <div class="col-sm-6">
                                        <Rock:RockLiteral ID="lConnectionStatusReadOnly" runat="server" Label="Connection Status" />
                                        <Rock:DefinedValuePicker ID="dvpConnectionStatus" runat="server" Label="Connection Status" Required="true" />
                                    </div>
                                    <div class="col-sm-6">
                                    </div>
                                </div>
                                <Rock:RockRadioButtonList ID="rblGender" runat="server" RepeatDirection="Horizontal" Label="Gender">
                                    <asp:ListItem Text="Male" Value="Male" />
                                    <asp:ListItem Text="Female" Value="Female" />
                                    <asp:ListItem Text="Unknown" Value="Unknown" />
                                </Rock:RockRadioButtonList>
                                <Rock:BirthdayPicker ID="bpBirthDay" runat="server" Label="Birthday" />
                                <asp:Panel ID="pnlGradeGraduation" runat="server" CssClass="form-row">
                                    <div class="col-xs-6 col-sm-3">
                                        <Rock:GradePicker ID="ddlGradePicker" runat="server" UseAbbreviation="true" UseGradeOffsetAsValue="true" />
                                    </div>
                                    <div class="col-xs-6 col-sm-3">
                                        <Rock:YearPicker ID="ypGraduation" runat="server" Label="Graduation Year" Help="High School Graduation Year." />
                                    </div>
                                </asp:Panel>
                                <div class="form-row">
                                    <div class="col-sm-3">
                                        <Rock:DefinedValuePicker ID="dvpMaritalStatus" runat="server" Label="Marital Status" AutoPostBack="true" OnSelectedIndexChanged="ddlMaritalStatus_SelectedIndexChanged" />
                                    </div>
                                    <div class="col-sm-3">
                                        <Rock:DatePicker ID="dpAnniversaryDate" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="AnniversaryDate" StartView="decade" />
                                    </div>
                                </div>
                            </fieldset>
                        </div>

                        <div class="well form-well">
                            <fieldset>
                                <legend>Contact Info</legend>

                                <div class="row">
                                    <asp:Repeater ID="rContactInfo" runat="server">
                                        <ItemTemplate>
                                            <div class="form-group phonegroup clearfix">
                                                <div class="control-label col-sm-1 phonegroup-label"><%# Rock.Web.Cache.DefinedValueCache.Get( (int)Eval("NumberTypeValueId")).Value  %></div>
                                                <div class="controls col-sm-11 phonegroup-number">
                                                    <div class="form-row">
                                                        <div class="col-sm-7 col-lg-4">
                                                            <asp:HiddenField ID="hfPhoneType" runat="server" Value='<%# Eval("NumberTypeValueId")  %>' />
                                                            <Rock:PhoneNumberBox ID="pnbPhone" runat="server" CountryCode='<%# Eval("CountryCode") %>' Number='<%# Eval("NumberFormatted")  %>' autocomplete="off" />
                                                        </div>
                                                        <div class="col-sm-5 col-lg-5 form-align">
                                                            <Rock:RockCheckBox ID="cbSms" runat="server" Text="SMS" Checked='<%# (bool)Eval("IsMessagingEnabled") %>' DisplayInline="true" CssClass="js-sms-number" />
                                                            <Rock:RockCheckBox ID="cbUnlisted" runat="server" Text="Unlisted" Checked='<%# (bool)Eval("IsUnlisted") %>' DisplayInline="true" />
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </div>

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

                                <Rock:RockRadioButtonList ID="rblEmailPreference" runat="server" RepeatDirection="Horizontal" Label="Email Preference">
                                    <asp:ListItem Text="Email Allowed" Value="EmailAllowed" />
                                    <asp:ListItem Text="No Mass Emails" Value="NoMassEmails" />
                                    <asp:ListItem Text="Do Not Email" Value="DoNotEmail" />
                                </Rock:RockRadioButtonList>

                                <Rock:RockRadioButtonList ID="rblCommunicationPreference" runat="server" RepeatDirection="Horizontal" Label="Communication Preference">
                                    <asp:ListItem Text="Email" Value="1" />
                                    <asp:ListItem Text="SMS" Value="2" />
                                </Rock:RockRadioButtonList>

                            </fieldset>
                        </div>

                        <Rock:PanelWidget runat="server" ID="PanelWidget1" Title="Alternate Identifiers">
                            <div class="row">
                                <div class="col-md-12">
                                    <Rock:RockControlWrapper ID="rcwAlternateIds" runat="server" Label="Alternate Identifiers" Help="Alternate Ids are used by things like check-in to allow easily checking in. This may include a barcode id or a fingerprint id for example.">
                                        <Rock:Grid ID="gAlternateIds" runat="server" DisplayType="Light" DataKeyNames="Guid" RowItemText="Alternate Id" ShowConfirmDeleteDialog="false">
                                            <Columns>
                                                <Rock:RockBoundField DataField="SearchValue" HeaderText="Value"/>
                                                <Rock:DeleteField OnClick="gAlternateIds_Delete" />
                                            </Columns>
                                        </Rock:Grid>
                                    </Rock:RockControlWrapper>
                                    <asp:CustomValidator ID="cvAlternateIds" runat="server" OnServerValidate="cvAlternateIds_ServerValidate" Display="None" />
                                </div>
                            </div>
                        </Rock:PanelWidget>

                        <Rock:PanelWidget runat="server" ID="pwAdvanced" Title="Advanced Settings">
                            <div class="row">
                                <div class="col-md-6">
                                    <asp:Panel ID="pnlGivingGroup" runat="server">
                                        <asp:HiddenField ID="hfGivingEnvelopeNumberConfirmed" runat="server" />
                                        <Rock:RockDropDownList ID="ddlGivingGroup" runat="server" Label="Combine Giving With" Help="The family that this person's gifts should be combined with for contribution statements and reporting.  If left blank, their contributions will not be grouped with their family" />
                                        <Rock:RockControlWrapper ID="rcwEnvelope" runat="server" Label="Envelope #" Help="The Giving Envelope Number that is associated with this Person">
                                            <Rock:NumberBox ID="tbGivingEnvelopeNumber" CssClass="input-width-sm pull-left" runat="server" Help="" NumberType="Integer" />
                                            <asp:LinkButton ID="btnGenerateEnvelopeNumber" runat="server" Text="Generate Envelope #" CssClass="btn btn-default margin-l-sm" OnClick="btnGenerateEnvelopeNumber_Click" />
                                        </Rock:RockControlWrapper>
                                    </asp:Panel>
                                    <Rock:RockCheckBox ID="cbLockAsChild" runat="server" Label="Lock as Child" Text="Yes" Help="By default individuals will be considered an adult when they are over 18 or are marked as an adult in a family. This setting will override this logic and lock the individual as a child."/>
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockControlWrapper ID="rcwPreviousNames" runat="server" Label="Previous Last Names">
                                        <Rock:Grid ID="grdPreviousNames" runat="server" DisplayType="Light" DataKeyNames="Guid" ShowConfirmDeleteDialog="false">
                                            <Columns>
                                                <Rock:RockBoundField DataField="LastName" />
                                                <Rock:DeleteField OnClick="grdPreviousNames_Delete" />
                                            </Columns>
                                        </Rock:Grid>
                                    </Rock:RockControlWrapper>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-12">
                                    <Rock:RockControlWrapper ID="rcwSearchKeys" runat="server" Label="Search Keys" Help="Search keys provide alternate ways to search for an individual.">
                                        <Rock:Grid ID="gSearchKeys" runat="server" DisplayType="Light" DataKeyNames="Guid" RowItemText="Search Key" ShowConfirmDeleteDialog="false">
                                            <Columns>
                                                <Rock:DefinedValueField DataField="SearchTypeValueId" HeaderText="Search Type" />
                                                <Rock:RockBoundField DataField="SearchValue" HeaderText="Search Value" />
                                                <Rock:DeleteField OnClick="gSearchKeys_Delete" />
                                            </Columns>
                                        </Rock:Grid>
                                    </Rock:RockControlWrapper>
                                </div>
                            </div>
                        </Rock:PanelWidget>

                        <Rock:ModalDialog runat="server" ID="mdPreviousName" Title="Add Previous Last Name" ValidationGroup="vgPreviousName" OnSaveClick="mdPreviousName_SaveClick">
                            <Content>
                                <Rock:RockTextBox ID="tbPreviousLastName" runat="server" Required="true" ValidationGroup="vgPreviousName" autocomplete="off" />
                            </Content>
                        </Rock:ModalDialog>

                        <Rock:ModalDialog runat="server" ID="mdAlternateId" Title="Add Alternate Identifier" ValidationGroup="vgAlternateId" OnSaveClick="mdAlternateId_SaveClick">
                            <Content>
                                <Rock:RockTextBox ID="tbAlternateId" runat="server" Label="Alternate Id" Required="true" ValidationGroup="vgAlternateId" autocomplete="off" />
                            </Content>
                        </Rock:ModalDialog>

                        <Rock:ModalDialog runat="server" ID="mdSearchKey" Title="Add Search Key" ValidationGroup="vgSearchKey" OnSaveClick="mdSearchKey_SaveClick">
                            <Content>
                                <Rock:RockDropDownList ID="ddlSearchValueType" runat="server" Label="Search Type" Required="true" ValidationGroup="vgSearchKey" />
                                <Rock:RockTextBox ID="tbSearchValue" runat="server" Label="Search Value" Required="true" ValidationGroup="vgSearchKey" autocomplete="off" />
                            </Content>
                        </Rock:ModalDialog>

                        <div class="actions">
                            <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                            <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                        </div>

                    </div>

                </div>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
