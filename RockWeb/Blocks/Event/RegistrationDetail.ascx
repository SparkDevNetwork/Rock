<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrationDetail.ascx.cs" Inherits="RockWeb.Blocks.Event.RegistrationDetail" %>
<%@ Reference Control="~/Blocks/Finance/TransactionList.ascx" %>

<asp:UpdatePanel ID="upnlRegistrationDetail" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server">

            <div class="panel panel-block">

                <div class="panel-heading clearfix">
                    <h1 class="panel-title pull-left">Registration Details</h1>
                    <div class="panel-labels">
                        <Rock:HighlightLabel ID="hlCost" runat="server" LabelType="Info" ToolTip="Cost" />
                        <Rock:HighlightLabel ID="hlBalance" runat="server" LabelType="Success" ToolTip="Balance Due" />
                    </div>
                </div>

                <div class="panel-body">

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <div id="pnlEditDetails" runat="server">

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:PersonPicker ID="ppPerson" runat="server" Label="Registered By" OnSelectPerson="ppPerson_SelectPerson" />
                                <Rock:EmailBox ID="ebConfirmationEmail" runat="server" Label="Confirmation Email" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="tbFirstName" runat="server" Label="Registered by First Name" Required="true" />
                                <Rock:RockTextBox ID="tbLastName" runat="server" Label="Registered by Last Name" Required="true" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                            </div>
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="tbDiscountCode" runat="server" Label="Discount Code" />
                                <Rock:NumberBox ID="nbDiscountPercentage" runat="server" AppendText="%" CssClass="input-width-md" Label="Discount Percentage" NumberType="Integer" />
                                <Rock:CurrencyBox ID="cbDiscountAmount" runat="server" CssClass="input-width-md" Label="Discount Amount (per Registrant)" />
                            </div>
                        </div>

                        <div class="actions">
                            <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                            <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                        </div>

                    </div>

                    <div id="pnlViewDetails" runat="server">

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockLiteral ID="lName" runat="server" Label="Registered By" />
                                <Rock:RockLiteral ID="lConfirmationEmail" runat="server" Label="Confirmation Email" />
                                <asp:LinkButton ID="lbResendConfirmation" runat="server" CssClass="btn btn-default btn-xs" Text="Resend Confirmation" OnClick="lbResendConfirmation_Click" CausesValidation="false"></asp:LinkButton>
                                <Rock:NotificationBox ID="nbConfirmationQueued" runat="server" NotificationBoxType="Success" Text="A new confirmation email has been sent." Visible="false" Dismissable="true"  />
                                <Rock:RockLiteral ID="lGroup" runat="server" Label="Group" />
                            </div>
                            <div class="col-md-6">

                                <Rock:RockLiteral ID="lDiscountCode" runat="server" Label="Discount Code" />
                                <Rock:RockLiteral ID="lDiscountPercent" runat="server" Label="Discount Percentage" />
                                <Rock:RockLiteral ID="lDiscountAmount" runat="server" Label="Discount Amount" />

                                <asp:Panel ID="pnlCosts" runat="server" Visible="false" CssClass="well">

                                    <div class="fee-table">
                                        <h4>Cost/Fee Summary</h4>
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
                                                        <span class="visible-xs-inline">Discounted Amount:</span> <%# Rock.Web.Cache.GlobalAttributesCache.Value( "CurrencySymbol" )%> <%# string.Format("{0:N}", Eval("DiscountedCost")) %>
                                                    </div>

                                                    <div class="col-sm-3 fee-value">
                                                        <span class="visible-xs-inline">Amount:</span> <%# Rock.Web.Cache.GlobalAttributesCache.Value( "CurrencySymbol" )%> <%# string.Format("{0:N}", Eval("Cost")) %>
                                                    </div>

                                                </div>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </div>

                                    <div class="row fee-totals">
                                        <div class="col-sm-offset-8 col-sm-4 fee-totals-options">
                                            <Rock:RockLiteral ID="lTotalCost" runat="server" Label="Total Cost" />
                                            <Rock:RockLiteral ID="lPreviouslyPaid" runat="server" Label="Paid" />
                                            <Rock:RockLiteral ID="lRemainingDue" runat="server" Label="Amount Remaining" />
                                        </div>
                                    </div>

                                    <div class="row">
                                        <div class="col-md-12">
                                            <asp:LinkButton ID="lbAddPayment" runat="server" CssClass="btn btn-primary btn-xs pull-right" Text="Add Payment" OnClick="lbAddPayment_Click" CausesValidation="false"></asp:LinkButton>
                                        </div>
                                    </div>

                                </asp:Panel>

                                <asp:Panel id="pnlPaymentInfo" runat="server" Visible="false" class="well">

                                    <h4>Payment Information</h4>

                                    <Rock:NotificationBox ID="nbPaymentError" runat="server" NotificationBoxType="Danger" Visible="true" />

                                    <Rock:CurrencyBox ID="cbPaymentAmount" runat="server" Label="Payment Amount" Required="true" ValidationGroup="Payment" ></Rock:CurrencyBox>
                                    <Rock:RockTextBox ID="txtCardFirstName" runat="server" Label="First Name on Card" Visible="false" Required="true" ValidationGroup="Payment" ></Rock:RockTextBox>
                                    <Rock:RockTextBox ID="txtCardLastName" runat="server" Label="Last Name on Card" Visible="false" Required="true" ValidationGroup="Payment"></Rock:RockTextBox>
                                    <Rock:RockTextBox ID="txtCardName" runat="server" Label="Name on Card" Visible="false" Required="true" ValidationGroup="Payment" ></Rock:RockTextBox>
                                    <Rock:RockTextBox ID="txtCreditCard" runat="server" Label="Credit Card #" MaxLength="19" CssClass="credit-card" Required="true" ValidationGroup="Payment" />

                                    <ul class="card-logos list-unstyled">
                                        <li class="card-visa"></li>
                                        <li class="card-mastercard"></li>
                                        <li class="card-amex"></li>
                                        <li class="card-discover"></li>
                                    </ul>
                                        
                                    <div class="row">
                                        <div class="col-sm-6">
                                            <Rock:MonthYearPicker ID="mypExpiration" runat="server" Label="Expiration Date" Required="true" ValidationGroup="Payment" />
                                        </div>
                                        <div class="col-sm-6">
                                            <Rock:NumberBox ID="txtCVV" Label="Card Security Code" CssClass="input-width-xs" runat="server" MaxLength="4" Required="true" ValidationGroup="Payment" />
                                        </div>
                                    </div>

                                    <Rock:AddressControl ID="acBillingAddress" runat="server" UseStateAbbreviation="true" UseCountryAbbreviation="false" ShowAddressLine2="false" 
                                        Required="true" ValidationGroup="Payment" RequiredErrorMessage="Billing Address is required" />

                                    <div class="actions">
                                        <asp:LinkButton ID="lbSubmitPayment" runat="server" Text="Submit" CssClass="btn btn-primary" OnClick="lbSubmitPayment_Click" CausesValidation="true" ValidationGroup="Payment" />
                                        <asp:LinkButton ID="lbCancelPayment" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="lbCancelPayment_Click" CausesValidation="false" />
                                    </div>

                                </asp:Panel>

                            </div>
                        </div>

                        <div class="actions">
                            <asp:LinkButton ID="btnEdit" runat="server" AccessKey="m" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                            <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                            <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" CausesValidation="false" />
                        </div>

                    </div>

                </div>
            </div>

            <asp:PlaceHolder ID="phDynamicControls" runat="server" />

            <div class="row">
                <div class="col-md-12">
                    <asp:LinkButton ID="lbAddRegistrant" runat="server" CssClass="btn btn-default btn-xs pull-right" OnClick="lbAddRegistrant_Click"><i class="fa fa-plus"></i> Add New Registrant</asp:LinkButton>
                </div>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
