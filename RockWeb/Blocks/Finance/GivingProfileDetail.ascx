<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GivingProfileDetail.ascx.cs" Inherits="RockWeb.Blocks.Finance.GivingProfileDetail" %>

<script type="text/javascript" src="../../Scripts/jquery.creditCardTypeDetector.js"></script>

<script type="text/javascript">
    function pageLoad(sender, args) {        
        $('.calc').on('change', function () {
            var total = 0.00;

            $('.calc').each(function () {                
                if ($(this).val() != '' && $(this).val() != null) {
                    total += parseFloat($(this).val());
                    console.log(total);
                    this.value = parseFloat($(this).val()).toFixed(2);
                }

            });
            $('#spnTotal').html(total.toFixed(2));
            $('.total-label').css('width', $(this).parent().width());
            return false;
        });
        
        $('.credit-card').creditCardTypeDetector({ 'credit_card_logos': '.card-logos' });
        $('.credit-card').trigger('keyup');
        $('.calc').trigger('change');        
    };
</script>

<asp:UpdatePanel ID="pnlContribution" runat="server">
<ContentTemplate>

    <asp:UpdatePanel ID="pnlDetails" runat="server" UpdateMode="Conditional">
    <ContentTemplate>

        <div class="container-fluid">     
                                
            <div class="row-fluid">
                                
                <div class="<%= _spanClass %> well">

                    <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error alert" />
                    <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

                    <fieldset>
                            
                        <legend>Contribution Information</legend>
                        
                        <div class="form-horizontal">

                            <div id="divCampus" runat="server" visible="false">                                                            
                                <div class="row-fluid">
                                    <asp:HiddenField ID="hfCampusId" runat="server" />
                                    <Rock:ButtonDropDownList ID="btnCampusList" runat="server" CssClass="btn btn-primary" LabelText="Campus" />                                    
                                </div>                                
                            </div>
                                                        
                            <asp:Panel ID="pnlAccountList" runat="server">
                                <asp:Repeater ID="rptAccountList" runat="server" >
                                <ItemTemplate>       
                                    <div class="row-fluid">
                                        <asp:HiddenField ID="hfAccountId" runat="server" Value='<%# DataBinder.Eval(Container.DataItem, "AccountId") %>'/>
	                                    <Rock:NumberBox ID="txtAccountAmount" runat="server" CssClass="input-small calc" PrependText="$"
                                            LabelText='<%# DataBinder.Eval(Container.DataItem, "Account.PublicName") %>'
                                            Text='<%# Convert.ToDecimal(DataBinder.Eval(Container.DataItem, "Amount")) > 0 ? DataBinder.Eval(Container.DataItem, "Amount") : "" %>'
                                            MinimumValue="0.0" Required="true"  >
	                                    </Rock:NumberBox>
                                    </div>
                                </ItemTemplate>                                
                                </asp:Repeater>
                            </asp:Panel>                            

                            <div ID="divAddAccount" runat="server">
                                <div class="row-fluid">
                                    <div class="control-group align-right">
                                        <Rock:ButtonDropDownList ID="btnAddAccount" runat="server" CssClass="btn btn-primary" OnSelectionChanged="btnAddAccount_SelectionChanged" Title="Add Another Gift" />
                                    </div>
                                </div>                                
                            </div>

                            <div id="divFrequency" runat="server" visible="false">
                                
                                <div class="row-fluid">
                                    <div class="control-group">                                                                            
                                        <label id="lblTotalAmount" class="control-label" for="spnTotal"><b>Total Amount</b></label>
                                        <div class="controls align-right">
                                            <b>$ <span id="spnTotal">0.00</span></b>
                                        </div>                     
                                    </div>
                                </div>

                                <div class="row-fluid">
                                    <Rock:ButtonDropDownList ID="btnFrequency" runat="server" CssClass="btn btn-primary" OnSelectionChanged="btnFrequency_SelectionChanged" LabelText="Frequency" CausesValidation="true" />
                                </div>                                    
                            
                                <div id="divRecurrence" runat="server" visible="false">          

                                    <div class="row-fluid"> 
                                        <div class="">
                                            <Rock:DatePicker ID="dtpStartDate" runat="server" LabelText="Starting On" data-date-format="dd-mm-yyyy" DatePickerType="Date" />
                                        </div>
                                    </div>
                                    
                                    <div class="row-fluid" id="divLimitGifts" runat="server" Visible="false">
                                        <Rock:LabeledCheckBox id="chkLimitGifts" runat="server" Text="Limit number of gifts" OnCheckedChanged="chkLimitGifts_CheckedChanged" AutoPostBack="true" />
                                    
                                        <div id="divLimitNumber" class="fade in" runat="server" visible="false">
                                            <Rock:NumberBox ID="txtLimitNumber" runat="server" class="input-small align-right" Text="0" />
                                        </div>                                    

                                    </div>

                                </div>

                            </div>

                        </div>                            

                    </fieldset>                                     
                                        
                </div>
            
            <% if ( _spanClass != "span6" ) { %>
                    
            </div>

            <div class="row-fluid form-horizontal">

            <% } %>

                <div class="<%= _spanClass %> well">
                    
                    <fieldset>
                        
                        <legend>Address Information</legend>
                        
                        <div class="row-fluid">
                            
                            <div class="<%= _spanClass != "span6" ? "" : "span6" %>">
                                <Rock:DataTextBox ID="txtFirstName" LabelText="First Name" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="GivenName" Required="true" />
                            </div>             

                            <div class="<%= _spanClass != "span6" ? "" : "span6" %>">
                                <Rock:DataTextBox ID="txtLastName" LabelText="Last Name" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="LastName" Required="true"  />
                            </div>        
                            
                        </div>

                        <div class="row-fluid">
                            <div class="<%= _spanClass != "span6" ? "" : "span12" %>">
                                <Rock:DataTextBox ID="txtStreet" LabelText="Address" runat="server" SourceTypeName="Rock.Model.Location, Rock" PropertyName="Street1" Required="true" />
                            </div>
                            
                        </div>

                        <div class="row-fluid">                            
                            <div class="<%= _spanClass != "span6" ? "span7" : "span5" %>">
                                <Rock:DataTextBox ID="txtCity" LabelText="City" runat="server" SourceTypeName="Rock.Model.Location, Rock" PropertyName="City" Required="true" CssClass="input-small address-line"/>
                            </div>
                            <div class="<%= _spanClass != "span6" ? "span3" : "span5" %>">
                                <Rock:StateDropDownList ID="ddlState" runat="server" LabelText="State" SourceTypeName="Rock.Model.Location, Rock" PropertyName="State" Required="true" CssClass="input-medium address-line" />
                            </div>
                            <div class="span2">
                                <Rock:DataTextBox ID="txtZip" LabelText="Zip" runat="server" SourceTypeName="Rock.Model.Location, Rock" PropertyName="Zip" Required="true" CssClass="input-mini address-line" />
                            </div>
                        </div>                    

                        <div class="row-fluid">   
                            <div class="span12" >
                                <Rock:DataTextBox ID="txtEmail" LabelText="Email" runat="server" TextMode="Email" SourceTypeName="Rock.Model.Person, Rock" PropertyName="Email" Required="true" />
                            </div>
                        </div>

                        <div class="row-fluid" >   
                            <div class="span12" >
                                <Rock:DataTextBox ID="txtPhone" LabelText="Phone" runat="server" SourceTypeName="Rock.Model.PhoneNumber, Rock" PropertyName="Number" />
                            </div>
                        </div>

                    </fieldset>

                </div>
                
            </div>

        </div>
    
    </ContentTemplate>
    </asp:UpdatePanel>

    <asp:UpdatePanel ID="pnlPayment" runat="server" UpdateMode="Conditional">
    <ContentTemplate>

        <div class="container-fluid">

            <div class="row-fluid <%= _spanClass != "span6" ? "form-horizontal" : "" %>">

                <div class="<%= _spanClass %> well">

                    <fieldset>

                        <legend>Payment Information</legend>

                        <ul class="nav nav-pills">
                            <asp:Repeater ID="rptPaymentType" runat="server">
                                <ItemTemplate>
                                    <li id="liSelectedTab" runat="server" class='<%# ( Container.ItemIndex == 0) ? "active" : "" %>'>
                                        <asp:LinkButton ID="lbPaymentType" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "Name") %>' OnClick="lbPaymentType_Click" />
                                    </li>
                                </ItemTemplate>
                            </asp:Repeater>
                        </ul>

                        <div class="tabContent">

                            <asp:Panel ID="pnlCreditCard" runat="server" Visible="true" CssClass="tab-pane fade in">

                                <div class="row-fluid">

                                    <div class="span6">
                                        <Rock:LabeledTextBox ID="txtCreditCard" runat="server" LabelText="Credit Card #" CssClass="credit-card" MaxLength="16" MinimumValue="0000000000000000" MaximumValue="9999999999999999" />
                                    </div>

                                    <div class="span6">
                                        <ul id="ulCardType" class="card-logos">
                                            <li class="card-visa"></li>
                                            <li class="card-mastercard"></li>
                                            <li class="card-amex"></li>
                                            <li class="card-discover"></li>
                                        </ul>                                        
                                    </div>

                                </div>

                                <div class="row-fluid">

                                    <div class="<%= _spanClass != "span6" ? "" : "span6" %>">
                                        <Rock:DatePicker ID="dtpExpiration" runat="server" LabelText="Expiration Date" />
                                    </div>

                                    <div class="<%= _spanClass != "span6" ? "" : "span6" %>">
                                        <Rock:NumberBox ID="txtCVV" LabelText="CVV #" runat="server" MaxLength="3" CssClass="input-mini" />
                                    </div>

                                </div>

                                <div class="row-fluid">
                                    <Rock:LabeledTextBox ID="txtCardName" runat="server" LabelText="Name on Card" />
                                </div>

                            </asp:Panel>

                            <asp:Panel ID="pnlChecking" runat="server" Visible="false" CssClass="tab-pane fade in">

                                <div class="row-fluid">
                                    <div class="<%= _spanClass != "span6" ? "span7" : "span6" %>">
                                        <fieldset>
                                            <Rock:LabeledTextBox ID="txtBankName" runat="server" LabelText="Bank Name" />
                                            <Rock:NumberBox ID="txtRouting" runat="server" LabelText="Routing #" MinimumValue="0.0" />
                                            <Rock:NumberBox ID="txtAccount" runat="server" LabelText="Account #" MinimumValue="0.0" />

                                            <Rock:LabeledRadioButtonList ID="rblAccountType" runat="server" RepeatDirection="Horizontal" LabelText="Account Type">
                                                <asp:ListItem Text="Checking" Selected="true" />
                                                <asp:ListItem Text="Savings" />
                                            </Rock:LabeledRadioButtonList>
                                        </fieldset>
                                    </div>

                                    <div class="<%= _spanClass != "span6" ? "span5" : "span6" %>">
                                        <fieldset>
                                            <img class="check-image" src="../../Assets/Images/check-image.png" />
                                        </fieldset>
                                    </div>
                                </div>

                            </asp:Panel>

                        </div>

                        <div id="divConfirmAddress" runat="server">

                            <div class="row-fluid">
                                <Rock:LabeledCheckBox ID="chkDefaultAddress" runat="server" Text="Use address from above" Checked="true" OnCheckedChanged="chkDefaultAddress_CheckedChanged" AutoPostBack="true" CausesValidation="true" />

                                <div id="divNewAddress" class="fade in" runat="server" Visible="False">

                                    <div class="row-fluid">
                                        <div class="span12">
                                            <Rock:DataTextBox ID="diffStreet" LabelText="Address" runat="server" SourceTypeName="Rock.Model.Location, Rock" PropertyName="Street1" />
                                        </div>
                                    </div>

                                    <div class="row-fluid">
                                        <div class="<%= _spanClass != "span6" ? "span7" : "span5" %>">
                                            <Rock:DataTextBox ID="diffCity" LabelText="City" runat="server" SourceTypeName="Rock.Model.Location, Rock" PropertyName="City" CssClass="input-small  address-line" />
                                        </div>
                                        <div class="<%= _spanClass != "span6" ? "span3" : "span5" %>">
                                            <Rock:StateDropDownList ID="diffState" runat="server" LabelText="State" SourceTypeName="Rock.Model.Location, Rock" PropertyName="State" CssClass="input-small  address-line" />                                            
                                        </div>
                                        <div class="span2">
                                            <Rock:DataTextBox ID="diffZip" LabelText="Zip" runat="server" SourceTypeName="Rock.Model.Location, Rock" PropertyName="Zip" CssClass="input-mini address-line" />                                            
                                        </div>
                                    </div>

                                </div>

                            </div>

                        </div>

                    </fieldset>

                </div>

            </div>

        </div>       

        <div class="<%= _spanClass %> actions">
            <asp:LinkButton ID="btnNext" runat="server" Text="Next" CssClass="btn btn-primary" OnClick="btnNext_Click" CausesValidation="true" />
        </div>    

    </ContentTemplate>
    </asp:UpdatePanel>   

    <asp:Panel ID="pnlConfirm" runat="server" Visible="false">    
    
        <div class="container-fluid">

            <div class="row-fluid">     
                                
                <div class="<%= _spanClass %> well">

                    <h3 class="header-text" >Confirm your Contribution: </h3>                

                    <asp:Literal ID="lPaymentConfirmation" runat="server" />                    

                </div>

            </div>

        </div>
        
        <div class="<%= _spanClass %> actions">
            <asp:LinkButton ID="btnGive" runat="server" Text="Give" CssClass="btn btn-primary" OnClick="btnGive_Click" />
            <asp:LinkButton ID="btnBack" runat="server" Text="Back" CssClass="btn btn-cancel"  OnClick="btnBack_Click" />
        </div>

    </asp:Panel>

    <asp:Panel ID="pnlComplete" runat="server" Visible="false">
    
        <div class="container-fluid">

            <div class="row-fluid">     
                                
                <div class="<%= _spanClass %> well">

                    <div id="divShowReceipt" runat="server">

                        <asp:Literal ID="lReceipt" runat="server" />

                    </div>                 

                    <div id="divSavePayment" runat="server" Visible="false">

                        <div class="row-fluid">

                            <Rock:LabeledCheckBox ID="chkSavePayment" runat="server" Text="Save My Payment"  OnCheckedChanged="chkSavePayment_CheckedChanged" AutoPostBack="true" />                                                                                
                            <div id="divPaymentNick" runat="server" visible="false" class="fade in">
                                <Rock:LabeledTextBox ID="txtPaymentNick" runat="server" LabelText="Enter a nickname for your account:" CssClass="input-medium" />
                            </div>                                        
                                        
                        </div>

                    </div>

                    <div id="divCreateAccount" runat="server" Visible="true">

                        <Rock:LabeledCheckBox ID="chkCreateAccount" runat="server" LabelText="Create An Account" OnCheckedChanged="chkCreateAccount_CheckedChanged" AutoPostBack="true" />
                
                        <div id="divCredentials" runat="server" visible="false" class="fade in">

				            <div class="row-fluid">
                                <Rock:LabeledTextBox ID="txtUserName" runat="server" LabelText="Enter a username" />
                                <Rock:LabeledTextBox ID="txtPassword" runat="server" TextMode="Password" LabelText="Enter a password" />
                            </div>                             
                            <div class="row-fluid">
                                <asp:LinkButton ID="btnCreateAccount" runat="server" Text="Create Account" CssClass="btn btn-primary" OnClick="btnCreateAccount_Click" />
                            </div>
                        </div>

                    </div>
                
                </div>            

            </div>

        </div>

        <div class="<%= _spanClass %> actions">
            <asp:LinkButton ID="btnPrint" runat="server" Text="Print" CssClass="btn btn-primary" OnClientClick="javascript: window.print();" />
        </div>

    </asp:Panel>

</ContentTemplate>
</asp:UpdatePanel>
