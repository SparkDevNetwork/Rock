<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GivingProfileDetail.ascx.cs" Inherits="RockWeb.Blocks.Finance.GivingProfileDetail" %>

<script type="text/javascript" src="../../Scripts/jquery.creditCardTypeDetector.js"></script>

<script type="text/javascript">
    function SetControlEvents() {
        $('.contribution-calculate').on('change', function () {
            var total = 0;
            $('.contribution-calculate').each(function () {
                if ($(this).val() != '' && $(this).val() != null && $(this).val() > 0) {
                    total += parseFloat($(this).val());
                    console.log(total);
                    this.value = parseFloat($(this).val()).toFixed(2);
                }
            });
            $('.total-amount').html(total.toFixed(2));
            return false;
        });

        $('.credit-card').creditCardTypeDetector({ 'credit_card_logos': '.card-logos' });           
        $('.radio-list input:radio').unbind('click').on('click', function () {
            var radioDisplay = $(this).parents().next('.radio-content').css("display");            
            if ($(this).val() == 0 && radioDisplay == "none") {
                $(this).parents().next('.radio-content').slideToggle();
            }
            else if ($(this).val() != 0 && radioDisplay != "none") {
                $(this).parents().next('.radio-content').slideToggle();
            }
        });                
        $('.toggle-input').unbind('click').on('click', function () {
            $(this).parents().next('.toggle-content').slideToggle();
        });
    };

    function cvAccountValidator_ClientValidate(sender, args) {
        args.IsValid = false;
        if ($('.total-amount').text() > 0) {
            args.IsValid = true;
            return;
        }
    };

    $(document).ready(function () { SetControlEvents(); });
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(SetControlEvents);
</script>

<asp:UpdatePanel ID="pnlContribution" runat="server" UpdateMode="Conditional">
<ContentTemplate>

    <asp:Panel ID="pnlDetailsAndAddress" runat="server">

        <div class="container-fluid">     
                                
            <div class="row-fluid form-horizontal">
                    
                <div ID="divDetails" runat="server" class="well">

                    <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following:" CssClass="alert alert-error block-message error alert" />
                    <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

                    <fieldset>
                            
                        <legend>Contribution Information</legend>
                        
                        <asp:UpdatePanel ID="pnlAmounts" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>

                            <div id="divCampus" runat="server">                                                            
                                <div class="row-fluid">
                                    <Rock:ButtonDropDownList ID="btnCampusList" runat="server" CssClass="btn btn-primary" LabelText="Campus" />
                                </div>                                
                            </div>
                                                        
                            <asp:Panel ID="pnlAccountList" runat="server">
                                <asp:CustomValidator ID="cvAccountValidator" runat="server"
                                    ErrorMessage="At least one contribution is Required"
                                    CssClass="align-middle" EnableClientScript="true" Display="None"
                                    ClientValidationFunction="cvAccountValidator_ClientValidate"
                                    OnServerValidate="cvAccountValidator_ServerValidate" />

                                <asp:Repeater ID="rptAccountList" runat="server" >
                                <ItemTemplate>       
                                    <div class="row-fluid">
                                        <asp:HiddenField ID="hfAccountId" runat="server" Value='<%# DataBinder.Eval(Container.DataItem, "Key.Id") %>'/>
	                                    <Rock:NumberBox ID="txtAccountAmount" runat="server" CssClass="input-small contribution-calculate" PrependText="$"
                                            LabelText='<%# DataBinder.Eval(Container.DataItem, "Key.Name") %>'
                                            Text='<%# Decimal.Parse(DataBinder.Eval(Container.DataItem, "Value").ToString()) != 0 ? DataBinder.Eval(Container.DataItem, "Value", "{0:f2}") : "" %>'
                                            NumberType="Double">
	                                    </Rock:NumberBox>                                        
                                    </div>
                                </ItemTemplate>                                
                                </asp:Repeater>                                            
                            </asp:Panel>              
                            
                            <div ID="divAddAccount" runat="server">
                                <div class="row-fluid">
                                    <div class="control-group align-middle">
                                        <Rock:ButtonDropDownList ID="btnAddAccount" runat="server" CssClass="btn btn-primary" OnSelectionChanged="btnAddAccount_SelectionChanged" Title="Add Another Gift" />
                                    </div>
                                </div>                                
                            </div>

                            <div class="row-fluid">
                                <div class="control-group">
                                    <label id="lblTotalAmount" class="control-label" for="spnTotal"><b>Total Amount</b></label>
                                    <div class="controls text-padding">
                                        <b>$ <span id="spnTotal" runat="server" class="total-amount">0.00</span></b>
                                    </div>
                                </div>
                            </div>

                            <div id="divFrequency" runat="server" visible="false">                                
                                
                                <div class="row-fluid">
                                    <Rock:ButtonDropDownList ID="btnFrequency" runat="server" CssClass="btn btn-primary" OnSelectionChanged="btnFrequency_SelectionChanged" LabelText="Frequency" CausesValidation="true" />
                                </div>                                    
                            
                                <div id="divRecurrence" runat="server" visible="false">

                                    <div class="row-fluid"> 
                                        <Rock:DatePicker ID="dtpStartDate" runat="server" LabelText="Starting On" data-date-format="dd-mm-yyyy" DatePickerType="Date" />
                                    </div>
                                    
                                    <div id="divLimitGifts" runat="server" class="row-fluid align-middle">
                                        <Rock:LabeledCheckBox ID="chkLimitGifts" runat="server" Text="Limit number of gifts" CssClass="toggle-input" />
                                    
                                        <div id="divLimitNumber" runat="server" class="toggle-content label-padding" style="display: none">
                                            <Rock:NumberBox ID="txtLimitNumber" runat="server" class="input-small" Text="0" />
                                        </div>                                    

                                    </div>

                                </div>

                            </div>

                        </ContentTemplate>
                        </asp:UpdatePanel>

                    </fieldset>                                     
                                        
                </div>
                        
            <% if ( _spanClass != "span6" ) { %>
                    
            </div>

            <div class="row-fluid form-horizontal">

            <% } %>

                <div ID="divAddress" runat="server" class="well">
                    
                    <fieldset>
                        
                        <legend>Address Information</legend>
                        
                        <div class="row-fluid">
                            
                            <div class="span6">
                                <Rock:DataTextBox ID="txtFirstName" LabelText="First Name" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="GivenName" Required="true" CssClass="input-inherit" AutoCompleteType="FirstName" />
                            </div>

                        <% if ( _spanClass != "span6" ) { %>
                        </div>

                        <div class="row-fluid">
                        <% } %>

                            <div class="span6">
                                <Rock:DataTextBox ID="txtLastName" LabelText="Last Name" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="LastName" Required="true" CssClass="input-inherit calc-name" AutoCompleteType="LastName" />
                            </div>        
                            
                        </div>

                        <div class="row-fluid">
                            <div class="span12">
                                <Rock:DataTextBox ID="txtStreet" LabelText="Street" runat="server" SourceTypeName="Rock.Model.Location, Rock" PropertyName="Street1" Required="true" AutoCompleteType="HomeStreetAddress" />
                            </div>
                        </div>

                        <div class="row-fluid">

                            <div ID="divCity" runat="server">
                                <Rock:DataTextBox ID="txtCity" LabelText="City" runat="server" SourceTypeName="Rock.Model.Location, Rock" PropertyName="City" Required="true" CssClass="input-inherit" AutoCompleteType="HomeCity" />    
                            </div>
                            <div id="divState" runat="server">
                                <Rock:StateDropDownList ID="ddlState" runat="server" LabelText="State" SourceTypeName="Rock.Model.Location, Rock" PropertyName="State" Required="true" CssClass="input-inherit address-line" AutoCompleteType="HomeState"/>
                            </div>
                            <div id="divZip" runat="server">
                                <Rock:DataTextBox ID="txtZip" LabelText="Zip" runat="server" SourceTypeName="Rock.Model.Location, Rock" PropertyName="Zip" Required="true" CssClass="input-mini input-inherit address-line" AutoCompleteType="HomeZipCode" />
                            </div>
                            
                        </div>                    

                        <div class="row-fluid">   
                            <div class="span12" >
                                <Rock:DataTextBox ID="txtEmail" LabelText="Email" runat="server" TextMode="Email" SourceTypeName="Rock.Model.Person, Rock" PropertyName="Email" Required="true" AutoCompleteType="Email" />
                            </div>
                        </div>

                        <div class="row-fluid">   
                            <div class="span12" >
                                <Rock:DataTextBox ID="txtPhone" LabelText="Phone" runat="server" SourceTypeName="Rock.Model.PhoneNumber, Rock" PropertyName="Number" CssClass="input-medium" Required="true" AutoCompleteType="HomePhone" />
                            </div>
                        </div>

                    </fieldset>

                </div>

            </div>

        </div>

    </asp:Panel>
    
    <asp:Panel ID="pnlPayment" runat="server">

        <div class="container-fluid">
            
            <div ID="divPayment" runat="server" class="well">

                <fieldset>

                    <div class="tabHeader">

                        <asp:UpdatePanel ID="pnlPaymentTabs" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>

                            <legend>Payment Information</legend>
    
                            <ul class="nav nav-pills remove-margin">
                                <asp:Repeater ID="rptPaymentType" runat="server">
                                    <ItemTemplate>
                                        <li id="liSelectedTab" runat="server">
                                            <asp:LinkButton ID="lbPaymentType" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "Name") %>' OnClick="lbPaymentType_Click" CausesValidation="false" />
                                        </li>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </ul>

                        </ContentTemplate>
                        </asp:UpdatePanel>

                    </div>
    
                    <div class="tabContent checkbox-align">

                        <asp:UpdatePanel ID="pnlPaymentContent" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>

                            <div ID="divCreditCard" runat="server" CssClass="tab-pane">
                                                       
                                <div id="divSavedCard" runat="server" class="radio-list">
                                    <Rock:LabeledRadioButtonList ID="rblSavedCard" runat="server" RepeatDirection="Vertical" />
                                </div>
                                
                                <div id="divNewCard" runat="server" class="radio-content">

                                    <div class="row-fluid">

                                        <div id="divCardNumber" runat="server">
                                            <Rock:LabeledTextBox ID="txtCreditCard" runat="server" LabelText="Credit Card #" MaxLength="19" MinimumValue="1000000000" MaximumValue="9999999999999999" CssClass="credit-card input-inherit" />
                                        </div>
                                                                        
                                        <div ID="divCardType" runat="server">
                                            <ul id="ulCardType" class="card-logos remove-margin">
                                                <li class="card-visa"></li>
                                                <li class="card-mastercard"></li>
                                                <li class="card-amex"></li>
                                                <li class="card-discover"></li>
                                            </ul>                                        
                                        </div>

                                    </div>

                                    <div class="row-fluid">

                                        <div id="divExpiration" runat="server">
                                            <Rock:MonthYearPicker ID="mypExpiration" runat="server" LabelText="Expiration Date" />
                                        </div>

                                        <div id="divCVV" runat="server">
                                            <Rock:NumberBox ID="txtCVV" LabelText="CVV #" runat="server" MaxLength="3" CssClass="input-mini" />
                                        </div>

                                    </div>

                                    <div class="row-fluid">
                                        <Rock:LabeledTextBox ID="txtCardName" runat="server" LabelText="Name on Card" />
                                    </div>

                                </div>                            

                            </div>

                            <div ID="divChecking" runat="server" CssClass="tab-pane">
                                
                                <div id="divSavedCheck" runat="server" class="radio-list">                                                              
                                    <Rock:LabeledRadioButtonList ID="rblSavedCheck" runat="server" RepeatDirection="Vertical" />
                                </div>
                            
                                <div id="divNewCheck" runat="server" class="row-fluid radio-content">
                                    
                                    <div ID="divCheckDetail" runat="server">
                                        <fieldset>
                                            <Rock:LabeledTextBox ID="txtBankName" runat="server" LabelText="Bank Name" CssClass="input-inherit" />
                                            <Rock:NumberBox ID="txtRoutingNumber" runat="server" LabelText="Routing #" MinimumValue="0.0" CssClass="input-inherit" />
                                            <Rock:NumberBox ID="txtAccountNumber" runat="server" LabelText="Account #" MinimumValue="0.0" CssClass="input-inherit" />

                                            <Rock:LabeledRadioButtonList ID="rblAccountType" runat="server" RepeatDirection="Horizontal" LabelText="Account Type" CssClass="remove-margin">
                                                <asp:ListItem Text="Checking" Selected="true"  />
                                                <asp:ListItem Text="Savings" />
                                            </Rock:LabeledRadioButtonList>
                                        </fieldset>
                                    </div>

                                    <div ID="divCheckImage" runat="server">
                                        <img class="check-image" src="../../Assets/Images/check-image.png" />
                                    </div>
                                </div>

                            </div>

                        </ContentTemplate>                        
                        </asp:UpdatePanel>
                                
                    </div>
                    
                    <div class="tabFooter">

                        <div ID="divDefaultAddress" runat="server" class="row-fluid">
                            <Rock:LabeledCheckBox ID="chkNewAddress" runat="server" Text="Enter a different billing address" CssClass="toggle-input" />
                        </div>

                        <div id="divNewAddress" runat="server" class="toggle-content label-padding" style="display:none">

                            <div class="row-fluid">
                                <div class="span12">
                                    <Rock:DataTextBox ID="txtNewStreet" LabelText="Street" runat="server" SourceTypeName="Rock.Model.Location, Rock" PropertyName="Street1" />
                                </div>
                            </div>

                            <div class="row-fluid">
                                <div ID="divNewCity" runat="server">
                                    <Rock:DataTextBox ID="txtNewCity" LabelText="City" runat="server" SourceTypeName="Rock.Model.Location, Rock" PropertyName="City" CssClass="input-inherit" />
                                </div>
                                <div ID="divNewState" runat="server" >
                                    <Rock:StateDropDownList ID="ddlNewState" runat="server" LabelText="State" SourceTypeName="Rock.Model.Location, Rock" PropertyName="State" CssClass="input-inherit" />
                                </div>
                                <div ID="divNewZip" runat="server" >
                                    <Rock:DataTextBox ID="txtNewZip" LabelText="Zip" runat="server" SourceTypeName="Rock.Model.Location, Rock" PropertyName="Zip" CssClass="input-inherit" />
                                </div>
                            </div>

                        </div>                        
                                               
                    </div>

                </fieldset>

            </div>
            
        </div>

        <div id="divNext" runat="server" class="actions">
            <asp:LinkButton ID="btnNext" runat="server" Text="Next" CssClass="btn btn-primary" OnClick="btnNext_Click" CausesValidation="true" />
        </div>

    </asp:Panel>

    <asp:Panel ID="pnlConfirm" runat="server" Visible="false">    
    
        <div class="container-fluid">

            <div class="row-fluid">     
                                
                <div ID="divConfirm" runat="server" class="well">

                    <asp:Literal ID="lPaymentConfirmation" runat="server" />                    

                </div>

            </div>

        </div>

        <div id="divGiveBack" runat="server" class="actions">
            <asp:LinkButton ID="btnGive" runat="server" Text="Give" CssClass="btn btn-primary" OnClick="btnGive_Click" />
            <asp:LinkButton ID="btnBack" runat="server" Text="Back" CssClass="btn btn-cancel" OnClick="btnBack_Click" />
        </div>
        
    </asp:Panel>

    <asp:Panel ID="pnlComplete" runat="server" Visible="false">
    
        <div class="container-fluid well" >

            <div class="row-fluid">     
                                
                <div ID="divReceipt" runat="server">

                    <asp:Literal ID="lReceipt" runat="server" />

                </div>

            </div>

            <div class="row-fluid">

                <asp:UpdatePanel id="pnlSavePayment" runat="server" UpdateMode="Conditional" Visible="false">
                <ContentTemplate>

                    <Rock:LabeledCheckBox ID="chkSavePayment" runat="server" Text="Save My Payment Information" CssClass="toggle-input" />                                                                                
                    <div id="divPaymentNick" runat="server" class="toggle-content label-padding" style="display: none">
                        <div class="span6">
                            <Rock:LabeledTextBox ID="txtPaymentNick" runat="server" LabelText="Account nickname:" CssClass="input-medium" />                                    
                        </div>
                        <div class="span6">
                            <asp:LinkButton ID="btnSavePaymentInfo" runat="server" Text="Save" CssClass="btn btn-primary padding-label" OnClick="btnSavePaymentInfo_Click" />                                    
                        </div>
                    </div>             

                </ContentTemplate>
                </asp:UpdatePanel>

                <asp:UpdatePanel id="pnlCreateAccount" runat="server" updatemode="Conditional">
                <ContentTemplate>

                    <Rock:LabeledCheckBox ID="chkCreateAccount" runat="server" LabelText="Create An Account" CssClass="toggle-input"/>                
                    <div id="divCredentials" runat="server" class="toggle-content" style="display:none">

				        <div class="span6">
                            <div class="row-fluid">
                                <Rock:LabeledTextBox ID="txtUserName" runat="server" LabelText="Enter a username" />
                            </div>
                            <div class="row-fluid">
                                <Rock:LabeledTextBox ID="txtPassword" runat="server" TextMode="Password" LabelText="Enter a password" />
                            </div>
                        </div>    
                                                 
                        <div class="span6">
                            <asp:LinkButton ID="btnCreateAccount" runat="server" Text="Create Account" CssClass="btn btn-primary" OnClick="btnCreateAccount_Click" />
                        </div>
                    </div>

                </ContentTemplate>
                </asp:UpdatePanel>
                
            </div>
            
        </div>

        <div id="divPrint" runat="server" class="actions">
            <asp:LinkButton ID="btnPrint" runat="server" Text="Print" CssClass="btn btn-primary" OnClientClick="javascript: window.print();" />
        </div>
        
    </asp:Panel>

</ContentTemplate>
</asp:UpdatePanel>
