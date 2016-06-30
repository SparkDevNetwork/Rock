<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionEntry.ascx.cs" Inherits="RockWeb.Blocks.Finance.TransactionEntry" %>

<asp:UpdatePanel ID="upPayment" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfCurrentPage" runat="server" Value="1" />

        <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false"></Rock:NotificationBox>

        <asp:Panel ID="pnlSelection" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-credit-card"></i> <asp:Literal ID="lPanelTitle1" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <asp:Panel ID="pnlContributionInfo" runat="server">

                    <% if ( FluidLayout )
                    { %>
                    <div class="row">
                        <div class="col-md-6">
                    <% } %>

                            <div class="panel panel-default contribution-info">
                                <div class="panel-heading"><h3 class="panel-title"><asp:Literal ID="lContributionInfoTitle" runat="server" /></h3></div>
                                <div class="panel-body">
                                    <fieldset>

                                        <asp:Repeater ID="rptAccountList" runat="server">
                                            <ItemTemplate>
                                                <Rock:CurrencyBox ID="txtAccountAmount" runat="server" Label='<%# Eval("PublicName") %>' Text='<%# ((decimal)Eval("Amount")).ToString("N2") %>' Placeholder="0.00" CssClass="account-amount" />
                                            </ItemTemplate>
                                        </asp:Repeater>
                                        <Rock:ButtonDropDownList ID="btnAddAccount" runat="server" Visible="false" Label=" "
                                            DataTextField="PublicName" DataValueField="Id" OnSelectionChanged="btnAddAccount_SelectionChanged" />

                                        <div class="form-group">
                                            <label>Total</label>
                                            <asp:Label ID="lblTotalAmount" runat="server" CssClass="form-control-static total-amount" />
                                        </div>

                                        <div id="divRepeatingPayments" runat="server" visible="false">
                                            <Rock:ButtonDropDownList ID="btnFrequency" runat="server" Label="Frequency"
                                                DataTextField="Value" DataValueField="Id" AutoPostBack="true" OnSelectionChanged="btnFrequency_SelectionChanged" />
                                            <Rock:DatePicker ID="dtpStartDate" runat="server" Label="First Gift" />
                                        </div>

                                        <Rock:RockTextBox ID="txtCommentEntry" runat="server" Required="true" Label="Comment" />

                                    </fieldset>
                                </div>
                            </div>

                        <% if ( FluidLayout )
                        { %>
                        </div>
                        <div class="col-md-6">
                        <% } %>

                            <div class="panel panel-default contribution-personal">
                                <div class="panel-heading"><h3 class="panel-title"><asp:Literal ID="lPersonalInfoTitle" runat="server" /></h3></div>
                                <div class="panel-body">
                                    <fieldset>
                                        <Rock:RockLiteral ID="txtCurrentName" runat="server" Label="Name" Visible="false" />
                                        <Rock:RockTextBox ID="txtFirstName" runat="server" Label="First Name" />
                                        <Rock:RockTextBox ID="txtLastName" runat="server" Label="Last Name" />
                                        <Rock:PhoneNumberBox ID="pnbPhone" runat="server" Label="Phone"></Rock:PhoneNumberBox>
                                        <Rock:RockTextBox ID="txtEmail" runat="server" Label="Email"></Rock:RockTextBox>
                                        <Rock:AddressControl ID="acAddress" runat="server" UseStateAbbreviation="true" UseCountryAbbreviation="false" />
                                    </fieldset>
                                </div>
                            </div>

                    <% if ( FluidLayout )
                    { %>
                        </div>
                    </div>
                    <% } %>

                </asp:Panel>

                <asp:Panel ID="pnlContributionPayment" runat="server">

                    <% if ( FluidLayout )
                    { %>
                    <div class="row">
                        <div class="col-md-6">
                    <% } %>

                    <asp:Panel ID="pnlPayment" runat="server" CssClass="panel panel-default contribution-payment">

                        <div class="panel-heading"><h3 class="panel-title"><asp:Literal ID="lPaymentInfoTitle" runat="server" /></h3></div>
                        <div class="panel-body">   

                            <Rock:RockRadioButtonList ID="rblSavedAccount" runat="server" CssClass="radio-list margin-b-lg" RepeatDirection="Vertical" DataValueField="Id" DataTextField="Name" />

                            <div id="divNewPayment" runat="server" class="radio-content">

                                <asp:HiddenField ID="hfPaymentTab" runat="server" />
                                <asp:PlaceHolder ID="phPills" runat="server" Visible="false">
                                    <ul class="nav nav-pills">
                                        <li id="liCreditCard" runat="server"><a href='#<%=divCCPaymentInfo.ClientID%>' data-toggle="pill">Credit Card</a></li>
                                        <li id="liACH" runat="server"><a href='#<%=divACHPaymentInfo.ClientID%>' data-toggle="pill">Bank Account</a></li>
                                    </ul>
                                </asp:PlaceHolder>

                                <div class="tab-content">

                                    <div id="divCCPaymentInfo" runat="server" visible="false" class="tab-pane">
                                        <Rock:RockTextBox ID="txtCardFirstName" runat="server" Label="First Name on Card" Visible="false"></Rock:RockTextBox>
                                        <Rock:RockTextBox ID="txtCardLastName" runat="server" Label="Last Name on Card" Visible="false"></Rock:RockTextBox>
                                        <Rock:RockTextBox ID="txtCardName" runat="server" Label="Name on Card" Visible="false"></Rock:RockTextBox>
                                        <Rock:RockTextBox ID="txtCreditCard" runat="server" Label="Credit Card #" MaxLength="19" CssClass="credit-card" />
                                        <ul class="card-logos list-unstyled">
                                            <li class="card-visa"></li>
                                            <li class="card-mastercard"></li>
                                            <li class="card-amex"></li>
                                            <li class="card-discover"></li>
                                        </ul>
                                        <div class="row">
                                            <div class="col-md-6">
                                                <Rock:MonthYearPicker ID="mypExpiration" runat="server" Label="Expiration Date" />
                                            </div>
                                            <div class="col-md-6">
                                                <Rock:RockTextBox ID="txtCVV" Label="Card Security Code" CssClass="input-width-xs" runat="server" MaxLength="4" />
                                            </div>
                                        </div>
                                        <Rock:RockCheckBox ID="cbBillingAddress" runat="server" Text="Enter a different billing address" CssClass="toggle-input" />
                                        <div id="divBillingAddress" runat="server" class="toggle-content">
                                            <Rock:AddressControl ID="acBillingAddress" runat="server" UseStateAbbreviation="true" UseCountryAbbreviation="false" />
                                        </div>                                    
                                    </div>

                                    <div id="divACHPaymentInfo" runat="server" visible="false" class="tab-pane">
                                        <Rock:RockTextBox ID="txtAccountName" runat="server" Label="Account Name" />
                                        <Rock:RockTextBox ID="txtRoutingNumber" runat="server" Label="Routing #" />
                                        <Rock:RockTextBox ID="txtAccountNumber" runat="server" Label="Account #" />
                                        <Rock:RockRadioButtonList ID="rblAccountType" runat="server" RepeatDirection="Horizontal" Label="Account Type">
                                            <asp:ListItem Text="Checking" Value="checking" Selected="true" />
                                            <asp:ListItem Text="Savings" Value="savings" />
                                        </Rock:RockRadioButtonList>
                                        <asp:Image ID="imgCheck" CssClass="img-responsive" runat="server" ImageUrl="<%$ Fingerprint:~/Assets/Images/check-image.png %>" />                                    
                                    </div>

                                </div>

                            </div>
                        </div> 
                    </asp:Panel>

                    <% if ( FluidLayout )
                    { %>
                        </div>
                    </div>            
                    <% } %>

                </asp:Panel>

            </div>

            <Rock:NotificationBox ID="nbSelectionMessage" runat="server" Visible="false"></Rock:NotificationBox>

            <div class="actions clearfix margin-b-lg">
                <asp:LinkButton ID="btnPaymentInfoNext" runat="server" Text="Next" CssClass="btn btn-primary pull-right" OnClick="btnPaymentInfoNext_Click" />
                <asp:LinkButton ID="btnStep2PaymentPrev" runat="server" Text="Previous" CssClass="btn btn-link" OnClick="btnStep2PaymentPrev_Click" />
                <asp:Label ID="aStep2Submit" runat="server" ClientIDMode="Static" CssClass="btn btn-primary pull-right" Text="Next" />
            </div>

            <iframe id="iframeStep2" src="<%=this.Step2IFrameUrl%>" style="display:none"></iframe>

            <asp:HiddenField ID="hfStep2AutoSubmit" runat="server" Value="false" />
            <asp:HiddenField ID="hfStep2Url" runat="server" />
            <asp:HiddenField ID="hfStep2ReturnQueryString" runat="server" />
            <span style="display:none" >
                <asp:LinkButton ID="lbStep2Return" runat="server" Text="Step 2 Return" OnClick="lbStep2Return_Click" CausesValidation="false" ></asp:LinkButton>
            </span>

        </asp:Panel>

        <asp:Panel ID="pnlConfirmation" CssClass="panel panel-block" runat="server" Visible="false">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-credit-card"></i> <asp:Literal ID="lPanelTitle2" runat="server" /></h1>
            </div>

            <div class="panel-body">
                <div class="panel panel-default">

                    <div class="panel-heading">
                        <h1 class="panel-title"><asp:Literal ID="lConfirmationTitle" runat="server" /></h1>
                    </div>
                    <div class="panel-body">
                        <asp:Literal ID="lConfirmationHeader" runat="server" />
                        <dl class="dl-horizontal gift-confirmation margin-b-md">
                            <Rock:TermDescription ID="tdNameConfirm" runat="server" Term="Name" />
                            <Rock:TermDescription ID="tdPhoneConfirm" runat="server" Term="Phone" />
                            <Rock:TermDescription ID="tdEmailConfirm" runat="server" Term="Email" />
                            <Rock:TermDescription ID="tdAddressConfirm" runat="server" Term="Address" />
                            <Rock:TermDescription runat="server" />
                            <asp:Repeater ID="rptAccountListConfirmation" runat="server">
                                <ItemTemplate>
                                    <Rock:TermDescription ID="tdAmount" runat="server" Term='<%# Eval("PublicName") %>' Description='<%# Eval("AmountFormatted") %>' />
                                </ItemTemplate>
                            </asp:Repeater>
                            <Rock:TermDescription ID="tdTotalConfirm" runat="server" Term="Total" />
                            <Rock:TermDescription runat="server" />
                            <Rock:TermDescription ID="tdPaymentMethodConfirm" runat="server" Term="Payment Method" />
                            <Rock:TermDescription ID="tdAccountNumberConfirm" runat="server" Term="Account Number" />
                            <Rock:TermDescription ID="tdWhenConfirm" runat="server" Term="When" />
                        </dl>
                
                        <asp:Literal ID="lConfirmationFooter" runat="server" />
                        <asp:Panel ID="pnlDupWarning" runat="server" CssClass="alert alert-block">
                            <h4>Warning!</h4>
                            <p>
                                You have already submitted a similar transaction that has been processed.  Are you sure you want
                            to submit another possible duplicate transaction?
                            </p>
                            <asp:LinkButton ID="btnConfirm" runat="server" Text="Yes, submit another transaction" CssClass="btn btn-danger margin-t-sm" OnClick="btnConfirm_Click" />
                        </asp:Panel>
                    </div>
                </div>
            </div>

            <Rock:NotificationBox ID="nbConfirmationMessage" runat="server" Visible="false"></Rock:NotificationBox>

            <div class="actions clearfix margin-b-lg">
                <asp:LinkButton ID="btnConfirmationPrev" runat="server" Text="Previous" CssClass="btn btn-link" OnClick="btnConfirmationPrev_Click" Visible="false" />
                <asp:LinkButton ID="btnConfirmationNext" runat="server" Text="Finish" CssClass="btn btn-primary pull-right" OnClick="btnConfirmationNext_Click" />
            </div>

        </asp:Panel>

        <asp:Panel ID="pnlSuccess" runat="server" Visible="false">

            <div class="well">
                <legend><asp:Literal ID="lSuccessTitle" runat="server" /></legend>
                <asp:Literal ID="lSuccessHeader" runat="server"></asp:Literal>
                <dl class="dl-horizontal gift-success">
                    <Rock:TermDescription ID="tdScheduleId" runat="server" Term="Payment Schedule ID" />
                    <Rock:TermDescription ID="tdTransactionCodeReceipt" runat="server" Term="Confirmation Code" />
                    <Rock:TermDescription runat="server" />
                    <Rock:TermDescription ID="tdNameReceipt" runat="server" Term="Name" />
                    <Rock:TermDescription ID="tdPhoneReceipt" runat="server" Term="Phone" />
                    <Rock:TermDescription ID="tdEmailReceipt" runat="server" Term="Email" />
                    <Rock:TermDescription ID="tdAddressReceipt" runat="server" Term="Address" />
                    <Rock:TermDescription runat="server" />
                    <asp:Repeater ID="rptAccountListReceipt" runat="server">
	                    <ItemTemplate>
		                    <Rock:TermDescription ID="tdAccountAmountReceipt" runat="server" Term='<%# Eval("PublicName") %>' Description='<%# Eval("AmountFormatted") %>' />
	                    </ItemTemplate>
                    </asp:Repeater>
                    <Rock:TermDescription ID="tdTotalReceipt" runat="server" Term="Total" />
                    <Rock:TermDescription runat="server" />
                    <Rock:TermDescription ID="tdPaymentMethodReceipt" runat="server" Term="Payment Method" />
                    <Rock:TermDescription ID="tdAccountNumberReceipt" runat="server" Term="Account Number" />
                    <Rock:TermDescription ID="tdWhenReceipt" runat="server" Term="When" />
                </dl>


                <dl class="dl-horizontal gift-confirmation margin-b-md">
                            
                </dl>
            </div>

            <asp:Panel ID="pnlSaveAccount" runat="server" Visible="false">
                <div class="well">
                    <legend><asp:Literal ID="lSaveAcccountTitle" runat="server" /></legend>
                    <fieldset>
                        <Rock:RockCheckBox ID="cbSaveAccount" runat="server" Text="Save account information for future gifts" CssClass="toggle-input" />
                        <div id="divSaveAccount" runat="server" class="toggle-content">
                            <Rock:RockTextBox ID="txtSaveAccount" runat="server" Label="Name for this account" CssClass="input-large"></Rock:RockTextBox>

                            <asp:PlaceHolder ID="phCreateLogin" runat="server" Visible="false">

                                <div class="control-group">
                                    <div class="controls">
                                        <div class="alert alert-info">
                                            <b>Note:</b> For security purposes you will need to login to use your saved account information.  To create
	    			                    a login account please provide a user name and password below. You will be sent an email with the account 
	    			                    information above as a reminder.
                                        </div>
                                    </div>
                                </div>

                                <Rock:RockTextBox ID="txtUserName" runat="server" Label="Username" CssClass="input-medium" />
                                <Rock:RockTextBox ID="txtPassword" runat="server" Label="Password" CssClass="input-medium" TextMode="Password" />
                                <Rock:RockTextBox ID="txtPasswordConfirm" runat="server" Label="Confirm Password" CssClass="input-medium" TextMode="Password" />

                            </asp:PlaceHolder>

                            <Rock:NotificationBox ID="nbSaveAccount" runat="server" Visible="false" NotificationBoxType="Danger"></Rock:NotificationBox>

                            <div id="divSaveActions" runat="server" class="actions">
                                <asp:LinkButton ID="lbSaveAccount" runat="server" Text="Save Account" CssClass="btn btn-primary" OnClick="lbSaveAccount_Click" />
                            </div>
                        </div>
                    </fieldset>                    
                </div>
            </asp:Panel>

            <asp:Literal ID="lSuccessFooter" runat="server" />

            <Rock:NotificationBox ID="nbSuccessMessage" runat="server" Visible="false"></Rock:NotificationBox>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
