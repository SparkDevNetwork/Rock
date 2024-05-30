<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrationDetail.ascx.cs" Inherits="RockWeb.Blocks.Event.RegistrationDetail" %>
<%@ Reference Control="~/Blocks/Finance/TransactionList.ascx" %>

<asp:UpdatePanel ID="upnlRegistrationDetail" runat="server">
    <ContentTemplate>

        <div class="wizard">

            <div class="wizard-item complete">
                <asp:LinkButton ID="lbWizardTemplate" runat="server" OnClick="lbWizardTemplate_Click" CausesValidation="false">
                    <%-- Placeholder needed for bug. See: http://stackoverflow.com/questions/5539327/inner-image-and-text-of-asplinkbutton-disappears-after-postback--%>
                    <asp:PlaceHolder runat="server">
                        <div class="wizard-item-icon">
                            <i class="fa fa-fw fa-clipboard"></i>
                        </div>
                        <div class="wizard-item-label">
                            <asp:Literal ID="lWizardTemplateName" runat="server" Text="Template" />
                        </div>
                    </asp:PlaceHolder>
                </asp:LinkButton>
            </div>

            <div class="wizard-item complete">
                <asp:LinkButton ID="lbWizardInstance" runat="server" OnClick="lbWizardInstance_Click" CausesValidation="false">
                    <asp:PlaceHolder runat="server">
                        <div class="wizard-item-icon">
                            <i class="fa fa-fw fa-file-o"></i>
                        </div>
                        <div class="wizard-item-label">
                            <asp:Literal ID="lWizardInstanceName" runat="server" Text="Instance" />
                        </div>
                    </asp:PlaceHolder>
                </asp:LinkButton>
            </div>

            <div class="wizard-item active">
                <div class="wizard-item-icon">
                    <i class="fa fa-fw fa-group"></i>
                </div>
                <div class="wizard-item-label">
                    <asp:Literal ID="lWizardRegistrationName" runat="server" Text="Registration" />
                </div>
            </div>

            <div class="wizard-item">
                <div class="wizard-item-icon">
                    <i class="fa fa-fw fa-user"></i>
                </div>
                <div class="wizard-item-label">
                    Registrant
                </div>
            </div>
        </div>

        <asp:Panel ID="pnlDetails" runat="server">

            <div class="panel panel-block">

                <div class="panel-heading">
                    <h1 class="panel-title pull-left">Registration Details</h1>
                    <div class="panel-labels">
                        <Rock:HighlightLabel ID="hlCost" runat="server" LabelType="Info" ToolTip="Cost" />
                        <Rock:HighlightLabel ID="hlBalance" runat="server" LabelType="Success" ToolTip="Balance Due" />
                    </div>
                </div>

                <div class="panel-body">

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                    <div id="pnlEditDetails" runat="server">

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:PersonPicker ID="ppPerson" runat="server" Label="Registered By" OnSelectPerson="ppPerson_SelectPerson" EnableSelfSelection="true" />
                                <Rock:EmailBox ID="ebConfirmationEmail" runat="server" Label="Confirmation Email" />
                                <Rock:CampusPicker ID="cpRegistrationCampus" runat="server" Label="Campus" />
                                <Rock:RockDropDownList ID="ddlGroup" runat="server" Label="Target Group" Visible="false" />
                                <Rock:AttributeValuesContainer ID="avcEditAttributes" runat="server" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="tbFirstName" runat="server" Label="Registered by First Name" Required="true" />
                                <Rock:RockTextBox ID="tbLastName" runat="server" Label="Registered by Last Name" Required="true" />
                                <Rock:RockDropDownList ID="ddlDiscountCode" runat="server" Label="Discount Code" DataValueField="Key" DataTextField="Value" AutoPostBack="true" OnSelectedIndexChanged="ddlDiscountCode_SelectedIndexChanged" />
                                <Rock:NumberBox ID="nbDiscountPercentage" runat="server" AppendText="%" CssClass="input-width-md" Label="Discount Percentage" NumberType="Integer" AutoPostBack="true" OnTextChanged="nbDiscountPercentage_TextChanged" />
                                <Rock:CurrencyBox ID="cbDiscountAmount" runat="server" CssClass="input-width-md" Label="Discount Amount (per Registrant)" AutoPostBack="true" OnTextChanged="cbDiscountAmount_TextChanged" />
                            </div>
                        </div>

                        <div class="actions">
                            <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                            <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                        </div>

                    </div>

                    <div id="pnlViewDetails" runat="server">

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockLiteral ID="lName" runat="server" Label="Registered By" />
                                <Rock:RockLiteral ID="lConfirmationEmail" runat="server" Label="Confirmation Email" />
                                <asp:LinkButton ID="lbResendConfirmation" runat="server" CssClass="btn btn-default btn-xs margin-b-sm" Text="Resend Confirmation" OnClick="lbResendConfirmation_Click" CausesValidation="false"></asp:LinkButton>
                                <Rock:NotificationBox ID="nbConfirmationQueued" runat="server" NotificationBoxType="Success" Text="A new confirmation email has been sent." Visible="false" Dismissable="true" />
                                <Rock:RockLiteral ID="lGroup" runat="server" Label="Target Group" />
                                <Rock:AttributeValuesContainer ID="avcDisplayAttributes" runat="server" />
                            </div>
                            <div class="col-md-6">

                                <Rock:RockLiteral ID="lDiscountCode" runat="server" Label="Discount Code" />
                                <Rock:RockLiteral ID="lDiscountPercent" runat="server" Label="Discount Percentage" />
                                <Rock:RockLiteral ID="lDiscountAmount" runat="server" Label="Discount Amount" />

                                <ul class="nav nav-tabs">
                                    <li id="tabAccount" runat="server"><a id="lnkTabAccount" runat="server" data-toggle="tab" href="#tabPaneAccount">Account</a></li>
                                    <li id="tabFees" runat="server"><a id="lnkTabFees" runat="server" data-toggle="tab" href="#tabPaneFees">Fees</a></li>
                                    <li id="tabPayments" runat="server"><a id="lnkTabPayments" runat="server" data-toggle="tab" href="#tabPanePayments">Payments</a></li>
                                </ul>
                                <div class="tab-content">
                                    <div id="tabPaneAccount" runat="server" class="tab-pane fade in">
                                        <Rock:NotificationBox ID="nbPaneAccountError" runat="server" CssClass="margin-t-md" NotificationBoxType="Danger" Visible="false" />
                                        <Rock:NotificationBox ID="nbPaneAccountWarning" runat="server" CssClass="margin-t-md" NotificationBoxType="Warning" Visible="false" />
                                        <div class="row fee-totals">
                                            <div class="col-sm-6 fee-totals-options">
                                                <asp:Panel ID="pnlPaymentPlanSummary" runat="server" CssClass="well" Visible="false">
                                                    <h4>Payment Plan</h4>

                                                    <Rock:RockLiteral ID="lFrequencyPaymentAmount" runat="server" />
                                                    <Rock:RockLiteral ID="lNextPaymentDate" runat="server" Label="Next Payment Date" />

                                                    <span id="spanChangeButtonWrapper" runat="server" data-toggle="tooltip" data-placement="top" data-original-title=""><asp:LinkButton ID="lbChangePaymentPlan" runat="server" CssClass="btn btn-default btn-xs margin-t-sm" Text="Change" OnClick="lbChangePaymentPlan_Click" CausesValidation="false" /></span>
                                                    <asp:LinkButton ID="lbDeletePaymentPlan" runat="server" CssClass="btn btn-link btn-xs margin-t-sm text-primary" Text="Delete" OnClick="lbDeletePaymentPlan_Click" CausesValidation="false" />

                                                    <Rock:ModalDialog ID="mdUpdatePaymentPlan" runat="server" Title="Update Payment Plan" ValidationGroup="vgUpdatePaymentPlan" OnSaveClick="mdUpdatePaymentPlan_SaveClick" SaveButtonCausesValidation="true">
                                                        <Content>
                                                            <Rock:NotificationBox ID="nbUpdatePaymentPlanError" runat="server" CssClass="margin-t-md" NotificationBoxType="Danger" Visible="false" />
                                                            <Rock:NotificationBox ID="nbUpdatePaymentPlanWarning" runat="server" CssClass="margin-t-md" NotificationBoxType="Warning" Visible="false" />
                                                            <asp:ValidationSummary ID="vsUpdatePaymentPlan" runat="server" ValidationGroup="vgUpdatePaymentPlan" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                                                            <div class="row">
                                                                <div class="col">
                                                                    <asp:Literal ID="lUpdatePaymentPlanMessage" runat="server" />
                                                                </div>
                                                            </div>

                                                            <div class="row">
                                                                <div class="col-md-7">
                                                                    <Rock:RockDropDownList ID="ddlPaymentPlanFrequencies" runat="server" Label="Desired Payment Frequency" ValidationGroup="vgUpdatePaymentPlan" OnSelectedIndexChanged="ddlPaymentPlanFrequencies_SelectedIndexChanged" Required="true" AutoPostBack="true" />
                                                                </div>
                                                            </div>

                                                            <div class="row">
                                                                <div class="col-md-2">
                                                                    <Rock:DatePicker ID="dpPaymentPlanStartDate" runat="server" Label="Start Date" ValidationGroup="vgUpdatePaymentPlan" Required="true" AllowPastDateSelection="false" OnSelectDate="dpPaymentPlanStartDate_SelectDate" />
                                                                </div>
                                                                <div class="col-md-2">
                                                                    <Rock:NumberBox ID="nbUpdatePaymentPlanNumberOfPayments" runat="server" Label="Number of Payments" ValidationGroup="vgUpdatePaymentPlan" Required="true" OnTextChanged="nbUpdatePaymentPlanNumberOfPayments_TextChanged" AutoPostBack="true" />
                                                                </div>
                                                            </div>


                                                            <asp:Panel ID="pnlUpdatePaymentPlanSummary" runat="server">
                                                                <label class="control-label">Plan Summary</label>
                                                                <div class="row">
                                                                    <div class="col-md-8">
                                                                        <Rock:RockLiteral ID="lPaymentPlanSummaryPaymentAmount" runat="server" Label="Payment Amount" />
                                                                    </div>
                                                                </div>
                                                            </asp:Panel>
                                                        </Content>
                                                    </Rock:ModalDialog>

                                                    <Rock:ModalDialog ID="mdDeletePaymentPlan" runat="server" Title="Are you sure?" OnSaveClick="mdDeletePaymentPlan_SaveClick" SaveButtonText="Delete" Visible="false">
                                                        <Content>
                                                            <p>Are you sure you want to delete this payment plan?</p>
                                                        </Content>
                                                    </Rock:ModalDialog>
                                                </asp:Panel>
                                            </div>
                                            <div class="col-sm-2">
                                            </div>
                                            <div class="col-sm-4 fee-totals-options">
                                                <Rock:RockLiteral ID="lTotalCost" runat="server" Label="Total Cost" />
                                                <Rock:RockLiteral ID="lPreviouslyPaid" runat="server" Label="Paid" />
                                                <Rock:RockLiteral ID="lRemainingDue" runat="server" Label="Amount Remaining" />
                                            </div>
                                        </div>
                                    </div>
                                    <div id="tabPaneFees" runat="server" class="tab-pane fade in">
                                        <Rock:NotificationBox ID="nbNoCost" runat="server" CssClass="margin-t-md" NotificationBoxType="Info" Text="There are no fees associated with this registration." Visible="false" />
                                        <asp:Panel ID="pnlCosts" runat="server" Visible="false" CssClass="well">

                                            <div class="fee-table">
                                                <h4 class="mt-0">Cost Summary</h4>
                                                <div class="registrationentry-summary">
                                                    <asp:Repeater ID="rptFeeSummary" runat="server">
                                                        <HeaderTemplate>
                                                            <div class="row hidden-xs fee-header">
                                                                <div class="col-sm-6">
                                                                    <strong>Description</strong>
                                                                </div>

                                                                <div runat="server" class="col-sm-3 fee-value" visible='<%# PercentageDiscountExists %>'>
                                                                    <strong>Discounted Amount</strong>
                                                                </div>

                                                                <div class="col-sm-3 fee-value">
                                                                    <strong>Amount</strong>
                                                                </div>

                                                            </div>
                                                        </HeaderTemplate>
                                                        <ItemTemplate>
                                                            <div class="row fee-row-<%# Eval("Type").ToString().ToLower() %>">
                                                                <div class="col-sm-6 fee-caption">
                                                                    <%# Eval("Description") %>
                                                                </div>

                                                                <div runat="server" class="col-sm-3 fee-value" visible='<%# PercentageDiscountExists %>'>
                                                                    <span class="visible-xs-inline">Discounted Amount:</span> <%# Rock.ExtensionMethods.FormatAsCurrency((decimal)Eval("DiscountedCost")) %>
                                                                </div>

                                                                <div class="col-sm-3 fee-value">
                                                                    <span class="visible-xs-inline">Amount:</span> <%# Rock.ExtensionMethods.FormatAsCurrency((decimal)Eval("Cost")) %>
                                                                </div>

                                                            </div>
                                                        </ItemTemplate>
                                                    </asp:Repeater>
                                                </div>
                                            </div>

                                        </asp:Panel>
                                    </div>
                                    <div id="tabPanePayments" runat="server" class="tab-pane fade in">
                                        <asp:Panel ID="pnlPaymentInfo" runat="server" CssClass="well">

                                            <h4>Payment Information</h4>

                                            <Rock:NotificationBox ID="nbPaymentError" runat="server" NotificationBoxType="Danger" Visible="true" />

                                            <asp:PlaceHolder ID="phPaymentAmount" runat="server">
                                                <Rock:PersonPicker ID="ppPayer" runat="server" Label="Payer" Required="true" ValidationGroup="Payment" Help="The person who is making the payment." />
                                                <Rock:CurrencyBox ID="cbPaymentAmount" runat="server" Label="Payment Amount" Required="true" ValidationGroup="Payment"></Rock:CurrencyBox>
                                                <Rock:RockTextBox ID="tbComments" runat="server" Label="Comments" TextMode="MultiLine" Rows="2" />
                                            </asp:PlaceHolder>

                                            <asp:PlaceHolder ID="phManualDetails" runat="server">
                                                <Rock:DefinedValuePicker ID="dvpCurrencyType" runat="server" Label="Currency Type" AutoPostBack="true" OnSelectedIndexChanged="dvpCurrencyType_SelectedIndexChanged" />
                                                <Rock:DefinedValuePicker ID="dvpCreditCardType" runat="server" Label="Credit Card Type" />
                                                <Rock:RockTextBox ID="tbTransactionCode" runat="server" Label="Transaction Code" />
                                                <div class="actions">
                                                    <asp:LinkButton ID="obSubmitPaymentCancel" runat="server" Text ="Cancel" CssClass="btn btn-link" OnClick="lbCancelPayment_Click" CausesValidation="false"></asp:LinkButton>
                                                    <asp:LinkButton ID="lbSubmitPayment" runat="server" Text="Submit" CssClass="btn btn-primary" OnClick="lbSubmitPayment_Click" CausesValidation="true" ValidationGroup="Payment" />
                                                </div>
                                            </asp:PlaceHolder>

                                              <%-- IHostedGateway Control goes here. - Collect Payment Info --%>

                                            <asp:Panel ID="pnlCCDetails" runat="server" Visible="false">
                                                <div class="hosted-payment-control">
                                                    <Rock:DynamicPlaceholder ID="phHostedPaymentControl" runat="server" />
                                                </div>

                                                <Rock:NotificationBox ID="nbPaymentTokenError" runat="server" NotificationBoxType="Validation" Visible="false" />

                                                <Rock:AddressControl ID="acBillingAddress" runat="server" UseStateAbbreviation="true" UseCountryAbbreviation="false" Label="" ShowAddressLine2="false" Required="true" />

                                                <div class="navigation actions">
                                                    <asp:LinkButton ID="btnGetPaymentInfoNextCancel" runat="server" Text ="Cancel" CssClass="btn btn-link" OnClick="lbCancelPayment_Click" CausesValidation="false"></asp:LinkButton>
                                                    <%-- NOTE: btnGetPaymentInfoNext ends up telling the HostedPaymentControl (via the js-submit-hostedpaymentinfo hook) to request a token, which will cause the _hostedPaymentInfoControl_TokenReceived postback
                                                        Even though this is a LinkButton, btnGetPaymentInfoNext won't autopostback  (see $('.js-submit-hostedpaymentinfo').off().on('click').. )
                                                    --%>
                                                    <Rock:BootstrapButton ID="btnGetPaymentInfoNext" runat="server" Text="Submit" CssClass="btn btn-primary js-submit-hostedpaymentinfo" DataLoadingText="Processing..." />
                                                </div>
                                            </asp:Panel>

                                        </asp:Panel>

                                        <asp:Panel ID="pnlPaymentDetails" runat="server" CssClass="well" Visible="false">

                                            <h4>Payments</h4>

                                            <Rock:Grid ID="gPayments" runat="server" DisplayType="Light" AllowSorting="false" RowItemText="Payment" ExportSource="ColumnOutput">
                                                <Columns>
                                                    <asp:HyperLinkField DataTextField="TransactionDateTime" DataNavigateUrlFields="Id" HeaderText="Date / Time" />
                                                    <Rock:RockBoundField DataField="Details" HeaderText="Details" HtmlEncode="false" />
                                                    <Rock:CurrencyField DataField="TotalAmount" HeaderText="Amount" />
                                                </Columns>
                                            </Rock:Grid>

                                            <div class="row">
                                                <div class="col-md-12">
                                                    <Rock:NotificationBox ID="nbNoAssociatedPerson" runat="server" CssClass="margin-t-md" NotificationBoxType="Info" Text="In order to process a payment you will need to link this registration to an individual." Visible="false" />

                                                    <%-- Have the tooltip in a div around the button so that it'll show even if the button is disabled
                                                        see https://stackoverflow.com/a/19938049
                                                    --%>
                                                    <span ID="lbProcessPaymentButtonTooltipWrapper" runat="server">
                                                        <asp:LinkButton ID="lbProcessPayment" runat="server" CssClass="btn btn-primary btn-xs margin-t-sm" Text="Process New Payment" OnClick="lbProcessPayment_Click" CausesValidation="false" />
                                                    </span>

                                                    <asp:LinkButton ID="lbAddPayment" runat="server" CssClass="btn btn-default btn-xs margin-t-sm margin-l-sm" Text="Add Manual Payment" OnClick="lbAddPayment_Click" CausesValidation="false"></asp:LinkButton>
                                                </div>
                                            </div>

                                        </asp:Panel>
                                    </div>
                                </div>

                            </div>
                        </div>

                        <div class="actions">
                            <asp:LinkButton ID="btnEdit" runat="server" data-shortcut-key="e" AccessKey="m" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                            <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                            <Rock:HiddenFieldWithClass ID="hfHasPayments" runat="server" CssClass="js-has-payments" />
                            <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link js-delete-registration" OnClick="btnDelete_Click" CausesValidation="false" />

                            <div class="pull-right">
                                <asp:LinkButton ID="lbHistory" runat="server" CssClass="btn btn-default btn-sm" CausesValidation="false" ToolTip="View Audit Log" OnClick="lbHistory_Click"><i class="fa fa-file-text-o"></i> Audit Log</asp:LinkButton>
                                <asp:LinkButton ID="lbShowMoveRegistrationDialog" runat="server" CssClass="btn btn-default btn-sm btn-square margin-l-sm" ToolTip="Move Registration" CausesValidation="false" OnClick="lbShowMoveRegistrationDialog_Click"><i class="fa fa-external-link"></i></asp:LinkButton>
                            </div>

                        </div>

                    </div>

                </div>
            </div>

            <asp:PlaceHolder ID="phDynamicControls" runat="server" />

            <div class="row">
                <div class="col-md-12">
                    <asp:LinkButton ID="lbAddRegistrant" runat="server" CssClass="btn btn-default btn-xs pull-right margin-b-sm" OnClick="lbAddRegistrant_Click"><i class="fa fa-plus"></i> Add New Registrant</asp:LinkButton>
                </div>
            </div>

            <Rock:ModalAlert ID="maSignatureRequestSent" runat="server" Text="A Signature Request Has Been Sent." Visible="false" />

        </asp:Panel>

        <Rock:ModalDialog ID="mdMoveRegistration" runat="server" Title="Move Registration" ValidationGroup="vgMoveRegistration" CancelLinkVisible="false">
            <Content>
                <asp:ValidationSummary ID="vsMoveRegistration" runat="server" ValidationGroup="vgMoveRegistration" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockLiteral ID="lCurrentRegistrationInstance" runat="server" Label="Current Registration Instance" />
                        <Rock:RockTextBox ID="tbComment" runat="server" Label="Comment" Help="Optional comment that will be added to the audit history." TextMode="MultiLine" Rows="2" />
                    </div>
                    <div class="col-md-6">
                        <div class="row">
                            <div class="col-sm-7">
                                <Rock:RockDropDownList ID="ddlNewRegistrationInstance" runat="server" Label="New Registration Instance" ValidationGroup="vgMoveRegistration" Required="true"
                                    DataValueField="Value" DataTextField="Text" AutoPostBack="true" OnSelectedIndexChanged="ddlNewRegistrationInstance_SelectedIndexChanged" />
                            </div>
                            <div class="col-sm-5">
                                <Rock:RockCheckBox ID="cbShowAll" runat="server" Label="Show All Instances" ValidationGroup="vgMoveRegistration"
                                    Help="By default, only active instances are listed. Select this option to show all instances."
                                    AutoPostBack="true" OnCheckedChanged="cbShowAll_CheckedChanged" />
                            </div>
                        </div>
                        <Rock:RockDropDownList ID="ddlMoveGroup" runat="server" Label="Move Registrants To Group" ValidationGroup="vgMoveRegistration" Required="false" Visible="false"
                            Help="Select a group here to remove all the registrants from their existing associated group, and add them to this group."
                            DataValueField="Value" DataTextField="Text" />
                    </div>
                </div>
                <br />
                <div class="actions">
                    <asp:LinkButton ID="btnMoveRegistration" runat="server" CssClass="btn btn-primary" Text="Move" ValidationGroup="vgMoveRegistration" OnClick="btnMoveRegistration_Click" />
                </div>
            </Content>
        </Rock:ModalDialog>
        <script type="text/javascript">
            Sys.Application.add_load(function () {
                $('.js-submit-hostedpaymentinfo').off().on('click', function (e) {
                    // Prevent the btnGetPaymentInfoNext autopostback event from firing by doing stopImmediatePropagation and returning false
                    e.stopImmediatePropagation();

                    <%=HostPaymentInfoSubmitScript%>

                    return false;
                });
            });
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
