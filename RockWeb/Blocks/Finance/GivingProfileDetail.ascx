<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GivingProfileDetail.ascx.cs" Inherits="RockWeb.Blocks.Finance.GivingProfileDetail" %>

<script type="text/javascript" src="../../Scripts/jquery.creditCardTypeDetector.js"></script>

<script type="text/javascript">

    function pageLoad(sender, args) {

        // payment totals script
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

        $('.calc').trigger('change');

        // payment types script
        $('.credit-card').creditCardTypeDetector({ 'credit_card_logos': '.card-logos' });

        $('.credit-card').on('change', function () {
            $('#hfCardType').val(ulCardType.className);
        });

    };

</script>

<asp:Panel ID="pnlContribution" runat="server">
        
    <asp:UpdatePanel ID="pnlDetails" runat="server" UpdateMode="Conditional">
    <ContentTemplate>

        <div class="container-fluid">     
                                
            <div class="row-fluid">
                                
                <div class="<%= _spanClass %> well">

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
                                            MinimumValue="0.0" >
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
                                    <Rock:ButtonDropDownList ID="btnFrequency" runat="server" CssClass="btn btn-primary" OnSelectionChanged="btnFrequency_SelectionChanged" LabelText="Frequency" />
                                </div>                                    
                            
                                <div id="divRecurrence" runat="server" visible="false">          

                                    <div class="row-fluid">                                        
                                        <Rock:DatePicker ID="dtpStartDate" runat="server" LabelText="Starting On" data-date-format="dd-mm-yyyy" DatePickerType="Date" />
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
                            
                            <div class="">
                                <Rock:LabeledTextBox ID="txtFirstName" LabelText="First Name" runat="server" />
                            </div>             

                            <div class="" >
                                <Rock:LabeledTextBox ID="txtLastName" LabelText="Last Name" runat="server" />
                            </div>        
                            
                        </div>

                        <div class="row-fluid">
                            <div class="span12" >
                                <Rock:LabeledTextBox ID="txtAddress" LabelText="Address" runat="server" />
                            </div>
                            
                        </div>

                        <div class="row-fluid">

                            <div class="span4">
                                <Rock:LabeledTextBox ID="txtCity" LabelText="City" runat="server" />
                            </div>
  
                            <div class="span3">
                                <Rock:StateDropDownList ID="ddlState" runat="server" LabelText="State" CssClass="state-select"  />
                            
                            </div>

                            <div class="span3">
                                <Rock:LabeledTextBox ID="txtZip" LabelText="Zip" runat="server" CssClass="input-mini" />
                            </div>

                        </div>                    

                        <div class="row-fluid">   
                            <div class="span12" >
                                <Rock:LabeledTextBox ID="txtEmail" LabelText="Email" runat="server" TextMode="Email" />
                            </div>
                        </div>

                        <div class="row-fluid" >   
                            <div class="span12" >
                                <Rock:LabeledTextBox ID="txtPhone" LabelText="Phone" runat="server" />
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
                            <ItemTemplate >
                                <li ID="liSelectedTab" runat="server" class='<%# ( Container.ItemIndex == 0) ? "active" : "" %>' >
                                    <asp:LinkButton ID="lbPaymentType" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "Name") %>' OnClick="lbPaymentType_Click" />
                                </li>
                            </ItemTemplate>
                            </asp:Repeater>
                        </ul>

                        <div class="tabContent">
                                
                            <asp:Panel ID="pnlCreditCard" runat="server" Visible="true" CssClass="fade in"> 

                                <div class="row-fluid">
                                    
                                    <div class="span6">
                                        <Rock:LabeledTextBox ID="txtCreditCard" runat="server" LabelText="Credit Card #" CssClass="credit-card" MaxLength="16" MinimumValue="0.0" MaximumValue="9999999999999999" />
                                    </div>

                                    <div class="span6">
                                        <ul id="ulCardType" class="card-logos">
	                                        <li class="card-visa"></li>
	                                        <li class="card-mastercard"></li>
	                                        <li class="card-amex"></li>
	                                        <li class="card-discover"></li>
                                        </ul>
                                        <asp:HiddenField ID="hfCardType" runat="server" />
                                    </div>
                                    
                                </div>
                           
                                <div class="row-fluid">
                                    
                                    <div class="">
                                        <Rock:DatePicker ID="dtpExpiration" runat="server" LabelText="Expiration Date" />
                                    </div>

                                    <div class="">
                                        <Rock:NumberBox ID="txtCVV" LabelText="CVV #" runat="server" MaxLength="3" CssClass="input-mini" />
                                    </div>
                                                                                                            
                                </div>

                                <div class="row-fluid">
                                    <Rock:LabeledTextBox ID="txtCardName" runat="server" LabelText="Name on Card" />
                                </div>

                            </asp:Panel>

                            <asp:Panel ID="pnlChecking" runat="server" Visible="false" CssClass="fade in">
                                    
                                <div class="row-fluid">
                                    <div class="span6">
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

                                    <div class="span6">
                                        <fieldset>
                                            <img class="check-image" src="../../Assets/Images/check-image.png" />
                                        </fieldset>
                                    </div>
                                </div>
                                
                            </asp:Panel>
                                
                        </div>

                        <div id="divConfirmAddress" runat="server">

                            <div class="row-fluid">
                                <Rock:LabeledCheckBox ID="chkDefaultAddress" runat="server" Text="Use address from above" Checked="true" OnCheckedChanged="chkDefaultAddress_CheckedChanged" AutoPostBack="true" />

                                <div id="divNewAddress" class="fade in" runat="server" visible="false">

                                    <div class="row-fluid">

                                        <div class="">
                                            <Rock:LabeledTextBox ID="diffFirstName" LabelText="First Name" runat="server" />
                                        </div>

                                        <div class="">
                                            <Rock:LabeledTextBox ID="diffLastName" LabelText="Last Name" runat="server" />
                                        </div>

                                    </div>

                                    <div class="row-fluid">
                                        <div class="span12">
                                            <Rock:LabeledTextBox ID="diffAddress" LabelText="Address" runat="server" />
                                        </div>

                                    </div>

                                    <div class="row-fluid control-group">

                                        <div class="span4">
                                            <Rock:LabeledTextBox ID="diffCity" LabelText="City" runat="server" />
                                        </div>

                                        <div class="span3 state-select">
                                            <Rock:StateDropDownList ID="diffState" runat="server" LabelText="State" />
                                        </div>

                                        <div class="span3">
                                            <Rock:LabeledTextBox ID="diffZip" LabelText="Zipcode" runat="server" CssClass="input-mini" />
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
            <asp:LinkButton ID="btnNext" runat="server" Text="Next" CssClass="btn btn-primary" OnClick="btnNext_Click"  />
        </div>
    
    </ContentTemplate>
    </asp:UpdatePanel>       

    <asp:UpdatePanel ID="pnlConfirm" runat="server" Visible="false" UpdateMode="conditional">
    <ContentTemplate>
        
        <div class="row-fluid">     
                                
            <div class="<%= _spanClass %> well">

                <h3 class="header-text" >Confirm your Contribution: </h3>
                
                <div id="divPaymentConfirmation" runat="server">

                    <p><b><asp:Literal ID="cfrmName" runat="server" /></b>, you're about to give a total of 
                        <b><asp:Literal ID="litGiftTotal" Visible="true" runat="server"/></b>
                        <asp:Literal ID="litMultiGift" Visible="true" runat="server">to the following accounts: </asp:Literal>
                        <asp:Repeater ID="rptGiftConfirmation" runat="server">
                            <ItemTemplate>
                                <b><%# ((KeyValuePair<string,decimal>)Container.DataItem).Value %></b> 
                                to the <b><%# ((KeyValuePair<string,decimal>)Container.DataItem).Key %></b>
                                <asp:Literal ID="litSpacer" Visible=<%# (litMultiGift.Visible) %> runat="server" >, </asp:Literal>
                            </ItemTemplate>
                            <FooterTemplate>.</FooterTemplate>
                        </asp:Repeater>
                    </p>                                
                
                    <p>Your gift will be paid using your <b><asp:Literal ID="litPaymentType" runat="server"/></b>
                        <asp:Literal ID="litAccountType" runat="server"/> ending in <b><asp:Literal ID="lblPaymentLastFour" runat="server"/></b>.
                    </p>

                </div>

            </div>

        </div>
        
        <div class="actions">
            <asp:LinkButton ID="btnGive" runat="server" Text="Give" CssClass="btn btn-primary" OnClick="btnGive_Click" />
            <asp:LinkButton ID="btnBack" runat="server" Text="Back" CssClass="btn btn-cancel"  OnClick="btnBack_Click" />
        </div>

    </ContentTemplate>
    </asp:UpdatePanel>

    <asp:UpdatePanel ID="pnlComplete" runat="server" Visible="false" UpdateMode="conditional">
    <ContentTemplate>
        
        <div class="row-fluid">     
                                
            <div class="<%= _spanClass %> well">

                <h3 class="header-text" >Contribution Complete! </h3>
                
                <p>
                    <b><asp:Literal ID="litDateGift" runat="server"/></b>
                </p>
                
                <p> Thank you for your generosity! 
                    You just gave a total of <asp:Literal ID="litGiftTotal2" Visible="true" runat="server"/>
                    to NewSpring Church using your <asp:Literal ID="litPaymentType2" runat="server"/>.                                       
                </p>  
                 
                 <div id="divSavePayment" runat="server" visible="false">

                    <div class="row-fluid">

                        <Rock:LabeledCheckBox ID="chkSavePayment" runat="server" Text="Save my payment information"  OnCheckedChanged="chkSavePayment_CheckedChanged" />
                                                                                
                        <div id="divPaymentNick" runat="server" visible="false">
                            <Rock:LabeledTextBox ID="txtPaymentNick" runat="server" LabelText="Enter a nickname for your payment account" />
                        </div>                                        
                                        
                    </div>

                </div>

                <div id="divSaveInformation" runat="server" visible="true">

                    <Rock:LabeledCheckBox ID="chkCreateAccount" runat="server" LabelText="Save My Information" OnCheckedChanged="chkCreateAccount_CheckedChanged"  />
                
                    <div id="divCreateAccount" runat="server" visible="false" >

				        <div class="row-fluid">
                            <Rock:LabeledTextBox ID="txtUserName" runat="server" LabelText="Enter a username" />
                            <Rock:LabeledTextBox ID="txtPassword" runat="server" TextMode="Password" LabelText="Enter a password" />	                
                        </div>                             

                    </div>

                </div>
                
            </div>            

        </div>
        
        <div class="actions">
            <asp:LinkButton ID="btnPrint" runat="server" Text="Print" CssClass="btn btn-primary" OnClientClick="javascript: window.print();" />
        </div>

    </ContentTemplate>
    </asp:UpdatePanel>

</asp:Panel>
