<%@ Control Language="C#" AutoEventWireup="true" CodeFile="UtilityPaymentEntry.ascx.cs" Inherits="RockWeb.Blocks.Finance.UtilityPaymentEntry" %>

<style>    
    .dropdown-header {
        display: block;
        padding: 5px 16px;
        font-size: 14px;
        line-height: 1.5;
        color: #535353;
        white-space: nowrap;
        font-weight: 700;
    }
    .dropdown-menu .dropdown-submenu-toggle {
        display: flex;
        align-items: center;
    }
    .dropdown-submenu-toggle > .caret {
        color: rgb(52 58 64 / 50%);
        margin-left: auto;
        transform: rotate(-90deg);
        transition: transform 125ms;
    }
    .dropdown-submenu-toggle.open > .caret {
        transform: rotate(0deg);
    }
    
    .dropdown-submenu > ul.dropdown-menu {
        position: relative;
        padding: 0;
        margin: 0;
        border-radius: 0;
        box-shadow: none;
        list-style: none;
        border: 0;
        width: 100%;
    }
    
    .dropdown-submenu > .dropdown-menu>li>a {
        padding-left: 40px;
    }
</style>

<script>
    Sys.Application.add_load(function () {
        // jquery ready
        $(document).ready(function () {
            // Make Dropdown Submenus possible
            $('.dropdown-submenu-toggle').on("click", function (e) {
                e.stopPropagation();
                e.preventDefault();
                $(this).toggleClass('open').next('ul').toggle();
            });
        });
    });
</script>

<asp:UpdatePanel ID="upPayment" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfCurrentPage" runat="server" Value="1" />

        <%-- hidden field to store the Transaction.Guid to use for the transaction. This is to help prevent duplicate transactions.   --%>
        <asp:HiddenField ID="hfTransactionGuid" runat="server" Value="" />

        <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false"></Rock:NotificationBox>
        <Rock:NotificationBox ID="nbInvalidPersonWarning" runat="server" Visible="false"></Rock:NotificationBox>

        <%-- Message for any configuration warnings --%>
        <Rock:NotificationBox ID="nbConfigurationNotification" runat="server" Visible="false" />

        <%-- Friendly Help if there is no Gateway configured --%>
        <asp:Panel ID="pnlGatewayHelp" runat="server" Visible="false">
            <h4>Welcome to Rock's On-line Giving Experience</h4>
            <p>There is currently no gateway configured. Below are a list of supported gateways installed on your server. You can also add additional gateways through the Rock Shop.</p>
            <asp:Repeater ID="rptInstalledGateways" runat="server" OnItemDataBound="rptInstalledGateways_ItemDataBound">
                <ItemTemplate>
                    <div class="panel panel-block">
                        <div class="panel-body">
                            <asp:HiddenField ID="hfGatewayEntityTypeId" runat="server" />
                            <h4>
                                <asp:Literal ID="lGatewayName" runat="server" /></h4>
                            <p>
                                <asp:Literal ID="lGatewayDescription" runat="server" />
                            </p>
                            <div class="actions">
                                <asp:HyperLink ID="aGatewayConfigure" runat="server" CssClass="btn btn-xs btn-success" Text="Configure" />
                                <asp:HyperLink ID="aGatewayLearnMore" runat="server" CssClass="btn btn-xs btn-link" Text="Learn More" />
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </asp:Panel>

        <asp:Panel ID="pnlTransactionEntry" runat="server">

            <asp:Panel ID="pnlSelection" CssClass="panel panel-block" runat="server">

                <asp:Panel ID="pnlHeadingSelection" runat="server" CssClass="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-credit-card"></i>
                        <asp:Literal ID="lPanelTitleSelection" runat="server" /></h1>
                </asp:Panel>
                <div class="panel-body">

                    <asp:Panel ID="pnlContributionInfo" runat="server">

                        <% if ( FluidLayout )
                            { %>
                        <div class="row">
                            <div class="col-md-6">
                                <% } %>
                                <asp:Literal ID="lTransactionHeader" runat="server" />
                                <div class="panel panel-default contribution-info">
                                    <asp:Panel ID="pnlHeadingContributionInfoTitle" runat="server" CssClass="panel-heading">
                                        <h3 class="panel-title"><asp:Literal ID="lContributionInfoTitle" runat="server" /></h3>
                                    </asp:Panel>
                                    <div class="panel-body">
                                        <fieldset>
                                            <Rock:CampusAccountAmountPicker ID="caapPromptForAccountAmounts" runat="server" />

                                            <div class="form-group">
                                                <asp:PlaceHolder ID="phbtnAddAccount" runat="server" Visible="false" />
                                            </div>

                                            <div id="divRepeatingPayments" runat="server" visible="false">
                                                <Rock:RockLiteral ID="txtFrequency" runat="server" Label="Frequency" Visible="false" />
                                                <Rock:ButtonDropDownList ID="btnFrequency" runat="server" Label="Frequency"
                                                    DataTextField="Value" DataValueField="Id" AutoPostBack="true" OnSelectionChanged="btnFrequency_SelectionChanged" />
                                                <Rock:DatePicker ID="dtpStartDate" runat="server" Label="First Gift" AutoPostBack="true" AllowPastDateSelection="false" OnTextChanged="btnFrequency_SelectionChanged" />
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
                                    <asp:Panel ID="pnlHeadingPersonalInfoTitle" runat="server" CssClass="panel-heading">
                                        <h3 class="panel-title">
                                            <asp:Literal ID="lPersonalInfoTitle" runat="server" />
                                            <div class="panel-labels">
                                                <asp:PlaceHolder ID="phGiveAsOption" runat="server">
                                                    <span class="panel-text">Give As &nbsp;</span>
                                                    <Rock:Toggle ID="tglGiveAsOption" runat="server" CssClass="pull-right" OnText="Person" OffText="Business" ButtonSizeCssClass="btn-xs" OnCheckedChanged="tglGiveAsOption_CheckedChanged" />
                                                </asp:PlaceHolder>
                                            </div>
                                        </h3>
                                    </asp:Panel>
                                    <div class="panel-body">
                                        <fieldset>

                                            <asp:PlaceHolder ID="phGiveAsPerson" runat="server">
                                                <div class="row">
                                                    <div class="col-sm-6">
                                                        <Rock:RockLiteral ID="txtCurrentName" runat="server" Label="Name" Visible="false" />
                                                        <Rock:FirstNameTextBox ID="txtFirstName" runat="server" Label="First Name" ValidationGroup="vgFirstName" />
                                                    </div>
                                                    <div class="col-sm-6">
                                                        <Rock:RockTextBox ID="txtLastName" runat="server" Label="Last Name" />
                                                    </div>
                                                </div>
                                            </asp:PlaceHolder>

                                            <asp:PlaceHolder ID="phGiveAsBusiness" runat="server" Visible="false">
                                                <asp:HiddenField ID="hfBusinessesLoaded" runat="server" />
                                                <Rock:RockRadioButtonList ID="cblBusiness" runat="server" Label="Business" RepeatDirection="Horizontal" AutoPostBack="true" OnSelectedIndexChanged="cblBusinessOption_SelectedIndexChanged" />
                                                <Rock:RockTextBox ID="txtBusinessName" runat="server" Label="Business Name" />
                                            </asp:PlaceHolder>

                                            <Rock:AddressControl ID="acAddress" runat="server" UseStateAbbreviation="true" UseCountryAbbreviation="false" Label="Address" />

                                            <div class="row">
                                                <div class="col-sm-6">
                                                    <Rock:EmailBox ID="txtEmail" runat="server" Label="Email"></Rock:EmailBox>
                                                </div>
                                                <div class="col-sm-6">
                                                    <Rock:PhoneNumberBox ID="pnbPhone" runat="server" Label="Phone"></Rock:PhoneNumberBox>
                                                </div>
                                            </div>
                                            
                                            <Rock:RockCheckBox ID="cbSmsOptIn" runat="server" Visible="false"/>
                                            <Rock:RockCheckBox ID="cbGiveAnonymously" runat="server" Text="Give Anonymously" />

                                            <asp:PlaceHolder ID="phBusinessContact" runat="server" Visible="false">
                                                <hr />
                                                <h4>Business Contact</h4>

                                                <div class="row">
                                                    <div class="col-sm-6">
                                                        <Rock:RockTextBox ID="txtBusinessContactFirstName" runat="server" Label="First Name" />
                                                    </div>
                                                    <div class="col-sm-6">
                                                        <Rock:RockTextBox ID="txtBusinessContactLastName" runat="server" Label="Last Name" />
                                                    </div>
                                                </div>

                                                <div class="row">
                                                    <div class="col-sm-6">
                                                        <Rock:RockTextBox ID="txtBusinessContactEmail" runat="server" Label="Email"></Rock:RockTextBox>
                                                    </div>
                                                    <div class="col-sm-6">
                                                        <Rock:PhoneNumberBox ID="pnbBusinessContactPhone" runat="server" Label="Phone"></Rock:PhoneNumberBox>
                                                    </div>
                                                </div>

                                                <Rock:RockCheckBox ID="cbBusinessContactSmsOptIn" runat="server" Visible="false" />

                                            </asp:PlaceHolder>
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

                                    <asp:Panel ID="pnlHeadingPaymentInfoTitle" runat="server" CssClass="panel-heading">
                                        <h3 class="panel-title"><asp:Literal ID="lPaymentInfoTitle" runat="server" /></h3>
                                    </asp:Panel>
                                    <div class="panel-body">
                                        <Rock:RockRadioButtonList ID="rblSavedAccount" runat="server" CssClass="radio-list margin-b-lg" RepeatDirection="Vertical" AutoPostBack="true" OnSelectedIndexChanged="rblSavedAccount_SelectedIndexChanged" />

                                         <%-- Collect Payment Info (step 2). Skip this if they using a saved giving method. --%>
                                        <asp:Panel ID="pnlPaymentInfo" runat="server" Visible="true">
                                            <div class="hosted-payment-control js-hosted-payment-control">
                                                <Rock:DynamicPlaceholder ID="phHostedPaymentControl" runat="server" />
                                            </div>

                                            <Rock:NotificationBox ID="nbPaymentTokenError" runat="server" NotificationBoxType="Validation" Visible="false" />
                                            
                                        </asp:Panel>

                                    </div>
                                </asp:Panel>

                                <% if ( FluidLayout )
                                    { %>
                            </div>
                        </div>
                        <% } %>                      

                    </asp:Panel>

                </div>

                <div class="panel panel-default no-border">
                    <div class="panel-body">
                        <Rock:NotificationBox ID="nbSelectionMessage" runat="server" Visible="false"></Rock:NotificationBox>
                        
                        <div class="actions clearfix">
                            <a id="lHistoryBackButton" runat="server" class="btn btn-link" href="javascript: window.history.back();">Previous</a>

                            <Rock:Captcha ID="cpCaptcha" runat="server" CssClass="pull-left" />
                            <Rock:HiddenFieldWithClass ID="hfHostPaymentInfoSubmitScript" runat="server" CssClass="js-hosted-payment-script" />
                            <Rock:BootstrapButton ID="btnSavedAccountPaymentInfoNext" runat="server" Text="Next" CssClass="btn btn-primary pull-right" DataLoadingText="Processing..." Visible="false" OnClick="btnSavedAccountPaymentInfoNext_Click" />

                            <%-- NOTE: btnHostedPaymentInfoNext ends up telling the HostedPaymentControl (via the js-submit-hostedpaymentinfo hook) to request a token, which will cause the _hostedPaymentInfoControl_TokenReceived postback
                               	Even though this is a LinkButton, btnHostedPaymentInfoNext won't autopostback  (see $('.js-submit-hostedpaymentinfo').off().on('click').. ) unless a saved account is selected
                            --%>
                            <Rock:BootstrapButton ID="btnHostedPaymentInfoNext" runat="server" Text="Next" CssClass="btn btn-primary js-submit-hostedpaymentinfo pull-right" DataLoadingText="Processing..." />
                        </div>
                    </div>
                </div>

            </asp:Panel>

            <asp:Panel ID="pnlConfirmation" CssClass="panel panel-block contribution-confirmation" runat="server" Visible="false">

                <asp:Panel ID="pnlHeadingConfirmation" runat="server" CssClass="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-credit-card"></i>
                        <asp:Literal ID="lPanelTitleConfirmation" runat="server" /></h1>
                </asp:Panel>

                <div class="panel-body">
                    <div class="panel panel-default">

                        <asp:Panel ID="pnlHeadingConfirmationTitle" runat="server" CssClass="panel-heading">
                            <h1 class="panel-title">
                                <asp:Literal ID="lConfirmationTitle" runat="server" /></h1>
                        </asp:Panel>
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
                                <asp:LinkButton ID="btnConfirmDuplicateTransaction" runat="server" Text="Yes, submit another transaction" CssClass="btn btn-danger margin-t-sm" OnClick="btnConfirmDuplicateTransaction_Click" />
                            </asp:Panel>

                            <Rock:NotificationBox ID="nbConfirmationMessage" runat="server" Visible="false"></Rock:NotificationBox>

                            <div class="actions clearfix">
                                <asp:LinkButton ID="btnConfirmationPrev" runat="server" Text="Previous" CssClass="btn btn-link" OnClick="btnConfirmationPrev_Click" Visible="false" />
                                <Rock:BootstrapButton ID="btnProcessTransactionFromConfirmationPage" runat="server" Text="Finish" CssClass="btn btn-primary pull-right" OnClick="btnProcessTransactionFromConfirmationPage_Click" />
                            </div>
                        </div>
                    </div>
                </div>

            </asp:Panel>

            <asp:Panel ID="pnlSuccess" runat="server" Visible="false">

                <asp:Literal ID="lTransactionSummaryHTML" runat="server" />

                <asp:Panel ID="pnlSaveAccount" runat="server" Visible="false">
                    <div class="well">
                        <legend>
                            <asp:Literal ID="lSaveAccountTitle" runat="server" /></legend>
                        <fieldset>
                            <Rock:RockCheckBox ID="cbSaveAccount" runat="server" Text="Save account information for future gifts" CssClass="toggle-input" />
                            <div id="divSaveAccount" runat="server" class="toggle-content">
                                <Rock:RockTextBox ID="txtSaveAccount" runat="server" Label="Name for this account" CssClass="input-large"></Rock:RockTextBox>

                                <asp:PlaceHolder ID="phCreateLogin" runat="server" Visible="false">

                                    <div class="control-group">
                                        <div class="controls">
                                            <div class="alert alert-info">
                                                <b>Note:</b> For security purposes you will need to log in to use your saved account information.  To create
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

        </asp:Panel>

        <script type="text/javascript">
            Sys.Application.add_load(function () {
                $('.js-submit-hostedpaymentinfo').off().on('click', function (e) {

                    // Prevent the btnHostedPaymentInfoNext autopostback event from firing by doing stopImmediatePropagation and returning false
                    e.stopImmediatePropagation();

                    const hfHostedPaymentScript = document.querySelector(".js-hosted-payment-script");
                    window.location = "javascript: " + hfHostedPaymentScript.value;

                    return false;
                });
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
