<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BusinessDetail.ascx.cs" Inherits="RockWeb.Blocks.Finance.BusinessDetail" %>

<asp:UpdatePanel ID="upnlBusinesses" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">

            <div class="banner"><h1><asp:Literal ID="lTitle" runat="server" /></h1></div>

            <div id="pnlEditDetails" runat="server">
                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                <asp:HiddenField ID="hfBusinessId" runat="server" />
                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

                <div class="row">
                    <div class="col-md-3">
                        <fieldset>
                            <Rock:RockDropDownList ID="ddlRecordStatus" runat="server" Label="Record Status" AutoPostBack="true" OnSelectedIndexChanged="ddlRecordStatus_SelectedIndexChanged" />
                            <Rock:RockDropDownList ID="ddlReason" runat="server" Label="Reason" Visible="false"></Rock:RockDropDownList>
                        </fieldset>
                    </div>
                    <div class="col-md-9">
                        <div class="row">
                            <div class="col-md-12">
                                <Rock:DataTextBox ID="tbBusinessName" runat="server" Label="Name" TabIndex="1" 
                                    SourceTypeName="Rock.Model.Person, Rock" PropertyName="BusinessName" ValidationGroup="businessDetail" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12">
                                <Rock:RockTextBox ID="tbStreet1" runat="server" Label="Address Line 1" TabIndex="2" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12">
                                <Rock:RockTextBox ID="tbStreet2" runat="server" Label="Address Line 2" TabIndex="3" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12">
                                <div class="row">
                                    <div class="col-lg-7">
                                        <Rock:RockTextBox ID="tbCity" Label="City"  runat="server" TabIndex="4" />
                                    </div>
                                    <div class="col-lg-2">
                                        <Rock:StateDropDownList ID="ddlState" Label="State" runat="server" UseAbbreviation="true" CssClass="input-mini" TabIndex="5" />
                                    </div>
                                    <div class="col-lg-3">
                                        <Rock:RockTextBox ID="tbZipCode" Label="Zip Code" runat="server" CssClass="input-small" TabIndex="6" />
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12">
                                <div class="control-label">Phone Number</div>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12">
                                <div class="row">
                                    <div class="col-sm-7">
                                        <Rock:PhoneNumberBox ID="pnbPhone" runat="server" CountryCode='<%# Eval("CountryCode") %>' Number='<%# Eval("NumberFormatted")  %>' TabIndex="7" />
                                    </div>
                                    <div class="col-sm-2">
                                        <div class="row">
                                            <div class="col-xs-6">
                                                <asp:CheckBox ID="cbSms"  runat="server" Text="Sms" Checked='<%# (bool)Eval("IsMessagingEnabled") %>' TabIndex="8" />
                                            </div>
                                            <div class="col-xs-6">
                                                <asp:CheckBox ID="cbUnlisted" runat="server" Text="Unlisted" Checked='<%# (bool)Eval("IsUnlisted") %>' TabIndex="9" />
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-sm-3"></div>
                                </div>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12">
                                <Rock:RockTextBox ID="tbEmail" runat="server" PrependText="<i class='fa fa-envelope'></i>" Label="Email Address" TabIndex="10" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12">
                                <Rock:RockRadioButtonList ID="rblEmailPreference" runat="server" RepeatDirection="Horizontal" Label="Email Preference" TabIndex="11">
                                    <asp:ListItem Text="Email Allowed" Value="EmailAllowed" Selected="True" />
                                    <asp:ListItem Text="No Mass Emails" Value="NoMassEmails" />
                                    <asp:ListItem Text="Do Not Email" Value="DoNotEmail" />
                                </Rock:RockRadioButtonList>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12">
                                <Rock:RockDropDownList ID="ddlCampus" runat="server" Label="Campus" TabIndex="12" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12">
                                <legend>Contribution Info</legend>
                                <Rock:RockDropDownList ID="ddlGivingGroup" runat="server" Label="Combine Giving With" Help="The business or person that this businesses gifts should be combined with for contribution statements and reporting." TabIndex="13" /> 
                            </div>
                        </div>
                        <div class="actions">
                            <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" TabIndex="14" />
                            <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" TabIndex="15" />
                        </div>
                    </div>
                </div>
            </div>

            <fieldset id="fieldsetViewSummary" runat="server">
                <div class="row">
                    <div class="col-md-12">
                        <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <asp:Literal ID="lDetailsLeft" runat="server" />
                    </div>
                    <div class="col-md-6">
                        <asp:Literal ID="lDetailsRight" runat="server" />
                    </div>
                </div>
                <div class="actions">
                    <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CssClass="btn btn-primary" CausesValidation="false" OnClick="lbEdit_Click" />
                </div>
            </fieldset>

            <br />
            
            <div class="grid">
                <Rock:Grid ID="gContactList" runat="server" EmptyDataText="No Contacts Found" AllowSorting="true" ShowConfirmDeleteDialog="false" >
                    <Columns>
                        <asp:BoundField DataField="FullName" HeaderText="Contact Name" SortExpression="FullName" />
                        <Rock:DeleteField OnClick="gContactList_Delete" />
                    </Columns>
                </Rock:Grid>
            </div>

            <Rock:ModalDialog ID="mdAddContact" runat="server" Title="Add Contact" ValidationGroup="AddContact">
                <Content>
                    <asp:HiddenField ID="hfModalOpen" runat="server" />
                    <asp:ValidationSummary ID="valSummaryAddContact" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="AddContact"/>
                    <div class="row col-md-12">
                        <Rock:PersonPicker ID="ppContact" runat="server" Label="Contact" Required="true" ValidationGroup="AddContact" />
                    </div>
                </Content>
            </Rock:ModalDialog>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>