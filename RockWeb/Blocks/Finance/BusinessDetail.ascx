<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BusinessDetail.ascx.cs" Inherits="RockWeb.Blocks.Finance.BusinessDetail" %>

<asp:UpdatePanel ID="upnlBusinesses" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-briefcase"></i> <asp:Literal ID="lTitle" runat="server" /></h1>
                </div>
                <div class="panel-body">

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

                                <Rock:DataTextBox ID="tbBusinessName" runat="server" Label="Name" SourceTypeName="Rock.Model.Person, Rock" PropertyName="FirstName" ValidationGroup="businessDetail" />
                            
                                <Rock:AddressControl ID="acAddress" runat="server" UseStateAbbreviation="false" UseCountryAbbreviation="false" />

                                <div class="row">
                                    <div class="col-sm-6">
                                        <Rock:PhoneNumberBox ID="pnbPhone" runat="server" Label="Phone Number" CountryCode='<%# Eval("CountryCode") %>' Number='<%# Eval("NumberFormatted")  %>' />
                                    </div>
                                    <div class="col-sm-6">
                                        <div class="row">
                                            <div class="col-xs-6">
                                                <Rock:RockCheckBox ID="cbSms" runat="server" Text="Sms" Label="&nbsp;" Checked='<%# (bool)Eval("IsMessagingEnabled") %>' />
                                            </div>
                                            <div class="col-xs-6">
                                                <Rock:RockCheckBox ID="cbUnlisted" runat="server" Text="Unlisted" Label="&nbsp;" Checked='<%# (bool)Eval("IsUnlisted") %>' />
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <Rock:RockTextBox ID="tbEmail" runat="server" PrependText="<i class='fa fa-envelope'></i>" Label="Email Address" />

                                <Rock:RockRadioButtonList ID="rblEmailPreference" runat="server" RepeatDirection="Horizontal" Label="Email Preference" >
                                    <asp:ListItem Text="Email Allowed" Value="EmailAllowed" Selected="True" />
                                    <asp:ListItem Text="No Mass Emails" Value="NoMassEmails" />
                                    <asp:ListItem Text="Do Not Email" Value="DoNotEmail" />
                                </Rock:RockRadioButtonList>

                                <Rock:RockDropDownList ID="ddlCampus" runat="server" Label="Campus" />

                                <div class="actions">
                                    <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                                    <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
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
                </div>
            </div>

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-users"></i> Business Contacts</h1>
                </div>
                <div class="panel-body">
                    <div class="grid grid-panel">
                        <Rock:Grid ID="gContactList" runat="server" RowItemText="Contact" EmptyDataText="No Contacts Found" AllowSorting="true" OnRowSelected="gContactList_RowSelected" ShowConfirmDeleteDialog="false" >
                            <Columns>
                                <Rock:RockBoundField DataField="FullName" HeaderText="Contact Name" SortExpression="FullName" />
                                <Rock:DeleteField OnClick="gContactList_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
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