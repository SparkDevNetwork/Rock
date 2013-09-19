<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GatewayTest.ascx.cs" Inherits="RockWeb.Blocks.Finance.Administration.GatewayTest" %>

<asp:UpdatePanel ID="upPayment" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlPaymentInfo" runat="server">

            <% if ( FluidLayout )
               { %>
            <div class="row-fluid">
                <div class="span6">
                    <% } %>

                    <div class="well">
                        <legend>Contribution Information</legend>
                        <div class="form-horizontal">
                            <fieldset>

                                <asp:Repeater ID="rptAccountList" runat="server">
                                    <ItemTemplate>
                                        <Rock:LabeledTextBox ID="txtAccountAmount" runat="server" PrependText="$" LabelText='<%# Eval("Name") %>' Text='<%# Eval("AmountFormatted") %>' CssClass="input-medium account-amount" />
                                    </ItemTemplate>
                                </asp:Repeater>
                                <Rock:ButtonDropDownList ID="btnAddAccount" runat="server" CssClass="btn btn-primary" Visible="false" LabelText=" "
                                    DataTextField="Name" DataValueField="Id" OnSelectionChanged="btnAddAccount_SelectionChanged" />

                                <div class="control-group">
                                    <span class="control-label">Total</span>
                                    <div class="controls">
                                        <asp:Label ID="lblTotalAmount" runat="server" CssClass="total-amount" />
                                    </div>
                                </div>

                                <div id="divRepeatingPayments" runat="server" visible="false">
                                    <Rock:ButtonDropDownList ID="btnFrequency" runat="server" CssClass="btn btn-primary" LabelText="Frequency"
                                        DataTextField="Name" DataValueField="Id" />
                                    <Rock:DatePicker ID="dtpStartDate" runat="server" LabelText="First Payment" />
                                </div>

                            </fieldset>
                        </div>
                    </div>

                    <% if ( FluidLayout )
                       { %>
                </div>
                <div class="span6">
                    <% } %>

                    <div class="well">
                        <legend>Personal Information</legend>
                        <div class="form-horizontal">
                            <fieldset>
                                <Rock:LabeledText ID="txtCurrentName" runat="server" LabelText="Name" Visible="true" />
                                <Rock:LabeledTextBox ID="txtFirstName" runat="server" LabelText="First Name" CssClass="input-small"></Rock:LabeledTextBox>
                                <Rock:LabeledTextBox ID="txtLastName" runat="server" LabelText="Last Name" CssClass="input-small"></Rock:LabeledTextBox>
                                <Rock:LabeledTextBox ID="txtPhone" runat="server" LabelText="Phone" CssClass="input-medium"></Rock:LabeledTextBox>
                                <Rock:LabeledTextBox ID="txtEmail" runat="server" LabelText="Email" CssClass="input-large"></Rock:LabeledTextBox>
                                <Rock:LabeledTextBox ID="txtStreet" runat="server" LabelText="Address" CssClass="input-large"></Rock:LabeledTextBox>
                                <div class="control-group">
                                    <div class="control-label">&nbsp;</div>
                                    <div class="controls">
                                        <asp:TextBox ID="txtCity" runat="server" CssClass="input-small" />
                                        ,&nbsp;
                                            <Rock:StateDropDownList ID="ddlState" runat="server" UseAbbreviation="true" CssClass="input-mini" />&nbsp;
                                            <asp:TextBox ID="txtZip" runat="server" CssClass="input-small" />
                                    </div>
                                </div>
                            </fieldset>
                        </div>
                    </div>

                    <% if ( FluidLayout )
                       { %>
                </div>
            </div>
            <% } %>

            <div class="well">

                <asp:HiddenField ID="hfPaymentTab" runat="server" />

                <asp:Panel ID="pnlPaymentHeading" runat="server" CssClass="tabHeader">
                    <legend>Payment Information</legend>
                    <asp:PlaceHolder ID="phPills" runat="server" Visible="false">
                        <ul class="nav nav-pills remove-margin">
                            <li id="liCreditCard" runat="server"><a href='#<%=divCCPaymentInfo.ClientID%>' data-toggle="pill">Credit Card</a></li>
                            <li id="liACH" runat="server"><a href='#<%=divACHPaymentInfo.ClientID%>' data-toggle="pill">Bank Account</a></li>
                        </ul>
                    </asp:PlaceHolder>
                </asp:Panel>

                <div class="tab-content">

                    <div id="divCCPaymentInfo" runat="server" visible="false" class="form-horizontal">
                        <fieldset>
                            <Rock:LabeledRadioButtonList ID="rblSavedCC" runat="server" LabelText=" " CssClass="radio-list" RepeatDirection="Vertical" DataValueField="Id" DataTextField="Name" />
                            <div id="divNewCard" runat="server" class="radio-content">
                                <Rock:LabeledTextBox ID="txtCardFirstName" runat="server" LabelText="First Name on Card" CssClass="input-small" Visible="false"></Rock:LabeledTextBox>
                                <Rock:LabeledTextBox ID="txtCardLastName" runat="server" LabelText="Last Name on Card" CssClass="input-small" Visible="false"></Rock:LabeledTextBox>
                                <Rock:LabeledTextBox ID="txtCardName" runat="server" LabelText="Name on Card" CssClass="input-large" Visible="false"></Rock:LabeledTextBox>
                                <Rock:LabeledTextBox ID="txtCreditCard" runat="server" LabelText="Credit Card #" MaxLength="19" CssClass="credit-card input-large" />
                                <ul class="card-logos">
                                    <li class="card-visa"></li>
                                    <li class="card-mastercard"></li>
                                    <li class="card-amex"></li>
                                    <li class="card-discover"></li>
                                </ul>
                                <Rock:MonthYearPicker ID="mypExpiration" runat="server" LabelText="Expiration Date" />
                                <Rock:NumberBox ID="txtCVV" LabelText="Card Security Code" runat="server" MaxLength="3" CssClass="input-mini" />
                                <Rock:LabeledCheckBox ID="cbBillingAddress" runat="server" LabelText=" " Text="Enter a different billing address" CssClass="toggle-input" />
                                <div id="divBillingAddress" runat="server" class="toggle-content">
                                    <Rock:LabeledTextBox ID="txtBillingStreet" runat="server" LabelText="Billing Address" CssClass="input-large"></Rock:LabeledTextBox>
                                    <div class="control-group">
                                        <div class="control-label">&nbsp;</div>
                                        <div class="controls">
                                            <asp:TextBox ID="txtBillingCity" runat="server" CssClass="input-small" />
                                            ,&nbsp;
                                            <Rock:StateDropDownList ID="ddlBillingState" runat="server" UseAbbreviation="true" CssClass="input-mini" />&nbsp;
                                            <asp:TextBox ID="txtBillingZip" runat="server" CssClass="input-small" />
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </fieldset>
                    </div>

                    <div id="divACHPaymentInfo" runat="server" visible="false" class="form-horizontal">
                        <fieldset>
                            <Rock:LabeledRadioButtonList ID="rblSavedAch" runat="server" RepeatDirection="Vertical" DataValueField="Id" DataTextField="Name" />
                            <div id="divNewBank" runat="server" class="radio-content row-fluid">
                                <div class="span7">
                                    <Rock:LabeledTextBox ID="txtBankName" runat="server" LabelText="Bank Name" CssClass="input-medium" />
                                    <Rock:LabeledTextBox ID="txtRoutingNumber" runat="server" LabelText="Routing #" CssClass="input-large" />
                                    <Rock:LabeledTextBox ID="txtAccountNumber" runat="server" LabelText="Account #" CssClass="input-large" />
                                    <Rock:LabeledRadioButtonList ID="rblAccountType" runat="server" RepeatDirection="Horizontal" LabelText="Account Type">
                                        <asp:ListItem Text="Checking" Selected="true" />
                                        <asp:ListItem Text="Savings" />
                                    </Rock:LabeledRadioButtonList>
                                </div>
                                <div class="span5">
                                    <asp:Image ID="imgCheck" runat="server" ImageUrl="~/Assets/Images/check-image.png" />
                                </div>
                            </div>
                        </fieldset>
                    </div>

                </div>

            </div>

        </asp:Panel>

        <asp:Panel ID="pnlConfirmation" runat="server" Visible="false">

            <div class="well">
                <legend>Confirm Information</legend>
                <asp:PlaceHolder ID="phConfirmationHeader" runat="server"></asp:PlaceHolder>
                <dl class="dl-horizontal gift-confirmation">
                    <Rock:TermDescription ID="tdName" runat="server" Term="Name" />
                    <Rock:TermDescription ID="tdPhone" runat="server" Term="Phone" />
                    <Rock:TermDescription ID="tdEmail" runat="server" Term="Email" />
                    <Rock:TermDescription ID="tdAddress" runat="server" Term="Address" />
                    <Rock:TermDescription runat="server" />
                    <asp:Repeater ID="rptAccountListConfirmation" runat="server">
                        <ItemTemplate>
                            <Rock:TermDescription ID="tdAddress" runat="server" Term='<%# Eval("Name") %>' Description='<%# "$" + Eval("AmountFormatted") %>' />
                        </ItemTemplate>
                    </asp:Repeater>
                    <Rock:TermDescription ID="tdTotal" runat="server" Term="Total" />
                    <Rock:TermDescription runat="server" />
                    <Rock:TermDescription ID="tdPaymentMethod" runat="server" Term="Payment Method" />
                    <Rock:TermDescription ID="tdAccountNumber" runat="server" Term="Account Number" />
                    <Rock:TermDescription ID="tdWhen" runat="server" Term="When" />
                </dl>
            </div>
            <asp:PlaceHolder ID="phConfirmationFooter" runat="server" />
            <asp:Panel ID="pnlDupWarning" runat="server" CssClass="alert alert-block">
                <h4>Warning!</h4>
                <p>
                    You have already submitted a transaction that has been processed.  Are you sure you want
                to submit another possible duplicate transaction?
                </p>
                <asp:LinkButton ID="btnConfirm" runat="server" Text="Yes, submit another transaction" CssClass="btn btn-primary" OnClick="btnConfirm_Click" />
            </asp:Panel>
        </asp:Panel>

        <asp:Panel ID="pnlSuccess" runat="server" Visible="false">
            <div class="well">
                <legend>Gift Information</legend>
                <asp:PlaceHolder ID="phSuccessHeader" runat="server"></asp:PlaceHolder>
                <dl class="dl-horizontal gift-success">
                    <Rock:TermDescription ID="tdScheduleId" runat="server" Term="Payment Schedule ID" />
                    <Rock:TermDescription ID="tdTransactionCode" runat="server" Term="Transaction Confirmation Code" />
                </dl>
            </div>

            <asp:Panel ID="pnlSaveAccount" runat="server" Visible="false">
                <div class="well">
                    <legend>Make Giving Even Easier</legend>
                    <div class="form-horizontal">
                        <fieldset>
                            <Rock:LabeledCheckBox ID="cbSaveAccount" runat="server" LabelText=" " Text="Save account information for future gifts" CssClass="toggle-input" />
                            <div id="divSaveAccount" runat="server" class="toggle-content">
                                <Rock:LabeledTextBox ID="txtSaveAccount" runat="server" LabelText="Name for this account" CssClass="input-large"></Rock:LabeledTextBox>

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

                                    <Rock:LabeledTextBox ID="txtUserName" runat="server" LabelText="Username" CssClass="input-medium" />

                                    <Rock:LabeledTextBox ID="txtPassword" runat="server" LabelText="Password" CssClass="input-medium" TextMode="Password" />

                                </asp:PlaceHolder>

                                <Rock:NotificationBox ID="nbSaveAccount" runat="server" Visible="false" NotificationBoxType="Error"></Rock:NotificationBox>

                                <div class="actions">
                                    <asp:LinkButton ID="lbSaveAccount" runat="server" Text="Save Account" CssClass="btn btn-primary" OnClick="lbSaveAccount_Click" />
                                </div>

                            </div>

                        </fieldset>
                    </div>
                </div>
            </asp:Panel>

            <asp:PlaceHolder ID="phSuccessFooter" runat="server" />

        </asp:Panel>

        <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false"></Rock:NotificationBox>

        <div id="divActions" runat="server" class="actions">
            <asp:LinkButton ID="btnPrev" runat="server" Text="Previous" CssClass="btn" OnClick="btnPrev_Click" Visible="false" />
            <asp:LinkButton ID="btnNext" runat="server" Text="Next" CssClass="btn btn-primary" OnClick="btnNext_Click" />
        </div>

        <asp:HiddenField ID="hfCurrentPage" runat="server" />

    </ContentTemplate>
</asp:UpdatePanel>


