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
                <h1 class="panel-title"><i class="fa fa-user"></i> <asp:Literal ID="lTitle" runat="server" /></h1>
            </div>

            <div class="panel-body">
                <asp:ValidationSummary ID="valValidation" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <div class="row">

                    <div class="col-md-3">
                        <div class="well">
                            <Rock:RockDropDownList ID="ddlRecordStatus" runat="server" Label="Record Status" AutoPostBack="true" OnSelectedIndexChanged="ddlRecordStatus_SelectedIndexChanged" />
                            <Rock:RockDropDownList ID="ddlReason" runat="server" Label="Reason" Visible="false"></Rock:RockDropDownList>
                            <Rock:RockTextBox ID="tbInactiveReasonNote" runat="server" Label="Inactive Reason Note" TextMode="MultiLine" Rows="2" Visible="false" ></Rock:RockTextBox>
                            <Rock:ImageEditor ID="imgPhoto" runat="server" Label="Photo" BinaryFileTypeGuid="03BD8476-8A9F-4078-B628-5B538F967AFC" />
                        </div>
                    </div>

                    <div class="col-md-9">

                        <div class="well">
                            <fieldset>

                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:RockDropDownList ID="ddlTitle" runat="server" CssClass="input-width-md" Label="Title"/>
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:DataTextBox ID="tbFirstName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="FirstName" />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:DataTextBox ID="tbNickName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="NickName" Label="Nickname" />
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:DataTextBox ID="tbMiddleName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="MiddleName" />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:DataTextBox ID="tbLastName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="LastName" />
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:RockDropDownList ID="ddlSuffix" CssClass="input-width-md" runat="server" Label="Suffix"/>
                                    </div>
                                </div>
                                
                                
                                
                                
                                
                                
                                <div class="row">
                                    <div class="col-sm-6">
                                        <Rock:RockDropDownList ID="ddlConnectionStatus" runat="server" Label="Connection Status" Required="true" />
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
                                <div class="row">
                                    <div class="col-sm-3">
                                        <Rock:GradePicker ID="ddlGradePicker" runat="server" UseAbbreviation="true" UseGradeOffsetAsValue="true" CssClass="input-width-md" />
                                    </div>
                                    <div class="col-sm-3">
                                        <Rock:YearPicker ID="ypGraduation" runat="server" Label="Graduation Year" Help="High School Graduation Year." />
                                    </div>
                                    <div class="col-sm-6">
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-sm-3">
                                        <Rock:RockDropDownList ID="ddlMaritalStatus" runat="server" Label="Marital Status" />
                                    </div>
                                    <div class="col-sm-3">
                                        <Rock:DatePicker ID="dpAnniversaryDate" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="AnniversaryDate" StartView="decade" />
                                    </div>
                                    <div class="col-sm-6">
                                    </div>
                                </div>
                            </fieldset>
                        </div>

                        <div class="well">
                            <fieldset>
                            <legend>Contact Info</legend>

                            <div class="row">
                                <asp:Repeater ID="rContactInfo" runat="server">
                                    <ItemTemplate>
                                        <div class="form-group phonegroup">
                                            <div class="control-label col-sm-1 phonegroup-label"><%# Rock.Web.Cache.DefinedValueCache.Read( (int)Eval("NumberTypeValueId")).Value  %></div>
                                            <div class="controls col-sm-11 phonegroup-number">
                                                <div class="row">
                                                    <div class="col-sm-7">
                                                        <asp:HiddenField ID="hfPhoneType" runat="server" Value='<%# Eval("NumberTypeValueId")  %>' />
                                                        <Rock:PhoneNumberBox ID="pnbPhone" runat="server" CountryCode='<%# Eval("CountryCode") %>' Number='<%# Eval("NumberFormatted")  %>' />
                                                    </div>    
                                                    <div class="col-sm-5">
                                                        <div class="row">
                                                            <div class="col-xs-6">
                                                                <asp:CheckBox ID="cbSms" runat="server" Text="SMS" Checked='<%# (bool)Eval("IsMessagingEnabled") %>' CssClass="js-sms-number" />
                                                            </div>
                                                            <div class="col-xs-6">
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

                            <div class="row">
                                <div class="col-sm-6">
                                    <Rock:DataTextBox ID="tbEmail" PrependText="<i class='fa fa-envelope'></i>" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="Email" />
                                </div>
                                <div class="col-sm-3">
                                    <Rock:RockCheckBox ID="cbIsEmailActive" runat="server" Label="Email Status" Text="Is Active" />
                                </div>
                            </div>

                            <Rock:RockRadioButtonList ID="rblEmailPreference" runat="server" RepeatDirection="Horizontal" Label="Email Preference">
                                <asp:ListItem Text="Email Allowed" Value="EmailAllowed" />
                                <asp:ListItem Text="No Mass Emails" Value="NoMassEmails" />
                                <asp:ListItem Text="Do Not Email" Value="DoNotEmail" />
                            </Rock:RockRadioButtonList>

                        </fieldset>
                        </div>


                        <Rock:PanelWidget runat="server" ID="pwAdvanced" Title="Advanced Settings">
                            <div class="row">
                                <div class="col-md-6">
                                    <asp:HiddenField ID="hfGivingEnvelopeNumberConfirmed" runat="server" />
                                    <Rock:RockDropDownList ID="ddlGivingGroup" runat="server" Label="Combine Giving With" Help="The family that this person's gifts should be combined with for contribution statements and reporting.  If left blank, their contributions will not be grouped with their family" /> 
                                    <Rock:RockControlWrapper ID="rcwEnvelope" runat="server" Label="Envelope #" Help="The Giving Envelope Number that is associated with this Person" >
                                        <Rock:NumberBox ID="tbGivingEnvelopeNumber" CssClass="input-width-sm pull-left" runat="server" Help="" NumberType="Integer" />
                                        <asp:LinkButton ID="btnGenerateEnvelopeNumber" runat="server" Text="Generate Envelope #" CssClass="btn btn-default margin-l-sm" OnClick="btnGenerateEnvelopeNumber_Click" />
                                    </Rock:RockControlWrapper>
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockControlWrapper ID="rcwPreviousNames" runat="server" Label="Previous Last Names">
                                        <Rock:Grid ID="grdPreviousNames" runat="server" DisplayType="Light" DataKeyNames="Guid" ShowConfirmDeleteDialog="false" >
                                            <Columns>
                                                <Rock:RockBoundField DataField="LastName" />
                                                <Rock:DeleteField OnClick="grdPreviousNames_Delete" />
                                            </Columns>
                                        </Rock:Grid>
                                    </Rock:RockControlWrapper>
                                </div>
                            </div>
                        </Rock:PanelWidget>

                        <Rock:ModalDialog runat="server" ID="mdPreviousName" Title="Add Previous Last Name" ValidationGroup="vgPreviousName" OnSaveClick="mdPreviousName_SaveClick">
                            <Content>
                                <Rock:RockTextBox ID="tbPreviousLastName" runat="server" Required="true" ValidationGroup="vgPreviousName" />
                            </Content>
                        </Rock:ModalDialog>

                        <div class="actions">
                            <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                            <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                        </div>

                </div>

            </div>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
