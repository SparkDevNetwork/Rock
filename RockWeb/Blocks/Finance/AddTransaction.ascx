<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AddTransaction.ascx.cs" Inherits="RockWeb.Blocks.Finance.AddTransaction" %>

<asp:UpdatePanel ID="upPayment" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlPaymentInfo" runat="server">

            <% if ( FluidLayout )
               { %>
            <div class="row">
                <div class="col-md-6">
                    <% } %>

                    <div class="panel panel-default contribution-info">
                        <div class="panel-heading"><h3 class="panel-title">Contribution Information</h3></div>
                        <div class="panel-body">
                            <fieldset>

                                <asp:Repeater ID="rptAccountList" runat="server">
                                    <ItemTemplate>
                                        <Rock:RockTextBox ID="txtAccountAmount" runat="server" PrependText="$" Label='<%# Eval("Name") %>' Text='<%# Eval("AmountFormatted") %>' Placeholder="0.00" CssClass="account-amount" />
                                    </ItemTemplate>
                                </asp:Repeater>
                                <Rock:ButtonDropDownList ID="btnAddAccount" runat="server" CssClass="btn btn-primary" Visible="false" Label=" "
                                    DataTextField="Name" DataValueField="Id" OnSelectionChanged="btnAddAccount_SelectionChanged" />

                                <div class="form-group">
                                    <label>Total</label>
                                    <asp:Label ID="lblTotalAmount" runat="server" CssClass="form-control-static total-amount" />
                                </div>

                                <div id="divRepeatingPayments" runat="server" visible="false">
                                    <Rock:ButtonDropDownList ID="btnFrequency" runat="server" CssClass="btn btn-primary" Label="Frequency"
                                        DataTextField="Name" DataValueField="Id" />
                                    <Rock:DatePicker ID="dtpStartDate" runat="server" Label="First Payment" />
                                </div>

                            </fieldset>
                        </div>
                    </div>

                    <% if ( FluidLayout )
                       { %>
                </div>
                <div class="col-md-6">
                    <% } %>

                    <div class="panel panel-default contribution-personal">
                        <div class="panel-heading"><h3 class="panel-title">Personal Information</h3></div>
                        <div class="panel-body">
                            <fieldset>
                                <Rock:RockLiteral ID="txtCurrentName" runat="server" Label="Name" Visible="true" />
                                <Rock:RockTextBox ID="txtFirstName" runat="server" Label="First Name"></Rock:RockTextBox>
                                <Rock:RockTextBox ID="txtLastName" runat="server" Label="Last Name"></Rock:RockTextBox>
                                <Rock:RockTextBox ID="txtPhone" runat="server" Label="Phone"></Rock:RockTextBox>
                                <Rock:RockTextBox ID="txtEmail" runat="server" Label="Email"></Rock:RockTextBox>
                                <Rock:RockTextBox ID="txtStreet" runat="server" Label="Address"></Rock:RockTextBox>
                                
                                <div class="row">
                                    <div class="col-md-7">
                                        <Rock:RockTextBox Label="City" ID="txtCity" runat="server" />
                                    </div>
                                    <div class="col-md-2">
                                        <Rock:StateDropDownList Label="State" ID="ddlState" runat="server" UseAbbreviation="true" />
                                    </div>
                                    <div class="col-md-3">
                                        <Rock:RockTextBox  ID="txtZip" runat="server" />
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

            <div class="panel panel-default contribution-payment">

                <asp:HiddenField ID="hfPaymentTab" runat="server" />

                <div class="panel-heading"><h3 class="panel-title">Payment Information</h3></div>
                
                <div class="panel-body">   
                    <asp:PlaceHolder ID="phPills" runat="server" Visible="false">
                        <ul class="nav nav-pills remove-margin">
                            <li id="liCreditCard" runat="server"><a href='#<%=divCCPaymentInfo.ClientID%>' data-toggle="pill">Credit Card</a></li>
                            <li id="liACH" runat="server"><a href='#<%=divACHPaymentInfo.ClientID%>' data-toggle="pill">Bank Account</a></li>
                        </ul>
                    </asp:PlaceHolder>


                    <div class="tab-content">

                        <div id="divCCPaymentInfo" runat="server" visible="false">
                            <fieldset>
                                <Rock:RockRadioButtonList ID="rblSavedCC" runat="server" Label=" " CssClass="radio-list" RepeatDirection="Vertical" DataValueField="Id" DataTextField="Name" />
                                <div id="divNewCard" runat="server" class="radio-content">
                                    <Rock:RockTextBox ID="txtCardFirstName" runat="server" Label="First Name on Card" Visible="false"></Rock:RockTextBox>
                                    <Rock:RockTextBox ID="txtCardLastName" runat="server" Label="Last Name on Card" Visible="false"></Rock:RockTextBox>
                                    <Rock:RockTextBox ID="txtCardName" runat="server" Label="Name on Card" Visible="false"></Rock:RockTextBox>
                                    <Rock:RockTextBox ID="txtCreditCard" runat="server" Label="Credit Card #" MaxLength="19" CssClass="credit-card" />
                                    <ul class="card-logos">
                                        <li class="card-visa"></li>
                                        <li class="card-mastercard"></li>
                                        <li class="card-amex"></li>
                                        <li class="card-discover"></li>
                                    </ul>
                                    <Rock:MonthYearPicker ID="mypExpiration" runat="server" Label="Expiration Date" />
                                    <Rock:NumberBox ID="txtCVV" Label="Card Security Code" runat="server" MaxLength="3" />
                                    <Rock:RockCheckBox ID="cbBillingAddress" runat="server" Label=" " Text="Enter a different billing address" CssClass="toggle-input" />
                                    <div id="divBillingAddress" runat="server" class="toggle-content">
                                        <Rock:RockTextBox ID="txtBillingStreet" runat="server" Label="Billing Address"></Rock:RockTextBox>
                                        <div class="control-group">
                                            <div class="control-label">&nbsp;</div>
                                            <div class="controls">
                                                <asp:TextBox ID="txtBillingCity" runat="server" />
                                                ,&nbsp;
                                                <Rock:StateDropDownList ID="ddlBillingState" runat="server" UseAbbreviation="true" />&nbsp;
                                                <asp:TextBox ID="txtBillingZip" runat="server" />
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </fieldset>
                        </div>

                        <div id="divACHPaymentInfo" runat="server" visible="false" class="form-horizontal">
                            <fieldset>
                                <Rock:RockRadioButtonList ID="rblSavedAch" runat="server" RepeatDirection="Vertical" DataValueField="Id" DataTextField="Name" />
                                <div id="divNewBank" runat="server" class="radio-content row-fluid">
                                    <div class="span7">
                                        <Rock:RockTextBox ID="txtBankName" runat="server" Label="Bank Name" CssClass="input-medium" />
                                        <Rock:RockTextBox ID="txtRoutingNumber" runat="server" Label="Routing #" CssClass="input-large" />
                                        <Rock:RockTextBox ID="txtAccountNumber" runat="server" Label="Account #" CssClass="input-large" />
                                        <Rock:RockRadioButtonList ID="rblAccountType" runat="server" RepeatDirection="Horizontal" Label="Account Type">
                                            <asp:ListItem Text="Checking" Selected="true" />
                                            <asp:ListItem Text="Savings" />
                                        </Rock:RockRadioButtonList>
                                    </div>
                                    <div class="span5">
                                        <asp:Image ID="imgCheck" runat="server" ImageUrl="~/Assets/Images/check-image.png" />
                                    </div>
                                </div>
                            </fieldset>
                        </div>

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
                            <Rock:RockCheckBox ID="cbSaveAccount" runat="server" Label=" " Text="Save account information for future gifts" CssClass="toggle-input" />
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

                                <Rock:NotificationBox ID="nbSaveAccount" runat="server" Visible="false" NotificationBoxType="Error"></Rock:NotificationBox>

                                <div id="divSaveActions" runat="server" class="actions">
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


