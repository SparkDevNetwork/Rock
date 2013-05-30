<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GivingProfileDetail.ascx.cs" Inherits="RockWeb.Blocks.Finance.GivingProfileDetail" %>

<script type="text/javascript" src="../../Scripts/jquery.creditCardTypeDetector.js"></script>

<script type="text/javascript">

    function pageLoad(sender, args) {

        // payment totals script
        $('.calc').on('change', function () {
            var total = 0.00;

            $('.calc').each(function () {
                if ($(this).val() != '') {
                    total += parseFloat($(this).val());
                    console.log(total);
                }
            });

            this.value = parseFloat($(this).val()).toFixed(2);
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

<asp:UpdatePanel ID="pnlContribution" runat="server" >
<ContentTemplate>
        
    <asp:HiddenField ID="hfCampusId" runat="server" />

    <asp:Panel ID="pnlDetails" runat="server">

        <div class="container-fluid">     
                                
            <div class="row-fluid">
                                
                <div class="<%= _spanClass %> well">

                    <fieldset>
                            
                        <legend>Contribution Information</legend>

                        <div class="form-horizontal">

                            <div id="divCampus" runat="server" visible="false">                                                            
                                <div class="row-fluid">
                                    <Rock:ButtonDropDownList ID="btnCampusList" runat="server" CssClass="btn btn-primary" Title="Select Campus" LabelText="Campus" />
                                </div>                                
                            </div>
                            
                            <div id="divAccountList">                                
                                <asp:Repeater ID="rptAccountList" runat="server" >
                                <ItemTemplate>       
                                    <div class="row-fluid">
	                                    <Rock:NumberBox ValidationDataType="Double" ID="txtAccountAmount" runat="server" CssClass="input-small calc" PrependText="$" 
                                            LabelText='<%# DataBinder.Eval(Container.DataItem, "Account.PublicName") %>' 
                                            Text='<%# DataBinder.Eval(Container.DataItem, "Amount") %>'>
	                                    </Rock:NumberBox>
                                    </div>
                                </ItemTemplate>                                
                                </asp:Repeater>
                            </div>                     

                            <div ID="divAddAccount" runat="server">
                                <div class="row-fluid">
                                    <Rock:ButtonDropDownList ID="btnAddAccount" runat="server" CssClass="btn btn-primary" OnSelectionChanged="btnAddAccount_SelectionChanged" Title="Add Another Gift" />
                                </div>                                
                            </div>

                            <div id="divFrequency" runat="server" visible="false">
                                
                                <div class="row-fluid">
                                    <div class="control-group">                                    
                                        <label id="lblTotalAmount" class="control-label" for="spnTotal"><b>Total Amount</b></label>
                                        <div class="controls amount-right">
                                            <b>$ <span id="spnTotal">0.00</span></b>
                                        </div>                     
                                    </div>
                                </div>

                                <div class="row-fluid">
                                    <Rock:ButtonDropDownList ID="btnFrequency" runat="server" CssClass="btn btn-primary" OnSelectionChanged="btnFrequency_SelectionChanged" LabelText="Frequency" />			                        
                                </div>                                    
                                    
                                <div class="row-fluid">                                        
                                    <Rock:DateTimePicker ID="dtpStartingDate" runat="server" LabelText="Starting On" data-date-format="dd-mm-yyyy" DatePickerType="Date" />                                        
                                </div>

                                <div class="row-fluid">
                                    <Rock:LabeledCheckBox id="chkLimitGifts" runat="server" Text="Limit number of gifts" OnCheckedChanged="chkLimitGifts_CheckedChanged" />
                                    
                                    <div id="divLimitGifts" runat="server" Visible="false">
                                        <Rock:NumberBox ID="txtLimitGifts" runat="server" Text="0" ValidationDataType="Integer" />
                                    </div>                                    
                                </div>

                            </div>
                        
                        </div>

                    </fieldset>                                     
                                        
                </div>
            
            <% if ( _spanClass == "span12" ) { %>
                    
            </div>

            <div class="row-fluid">

            <% } %>

                <div class="<%= _spanClass %> well">

                    <fieldset>

                        <legend>Address Information</legend>
                        
                        <div class="row-fluid">
                            
                            <div class="<%= _spanClass %>" >
                                <Rock:LabeledTextBox ID="txtFirstName" LabelText="First Name" runat="server" Required="true" />
                            </div>             

                            <div class="<%= _spanClass %>" >
                                <Rock:LabeledTextBox ID="txtLastName" LabelText="Last Name" runat="server" Required="true" />
                            </div>        
                            
                        </div>

                        <div class="row-fluid">
                            <div class="span12" >
                                <Rock:LabeledTextBox ID="txtAddress" LabelText="Address" runat="server" Required="true" CssClass="input-xlarge" />
                            </div>
                            
                        </div>

                        <div class="row-fluid">

                            <div class="span6">
                                <Rock:LabeledTextBox ID="txtCity" LabelText="City" runat="server" Required="true" CssClass="input-large" />
                            </div>
  
                            <div class="span4">
                                <Rock:StateDropDownList ID="ddlState" runat="server" LabelText="State" CssClass="state-select" Required="true" />
                            
                            </div>

                            <div class="span2">
                                <Rock:LabeledTextBox ID="txtZipcode" LabelText="Zip" runat="server" Required="true" CssClass="input-small" />
                            </div>

                        </div>                    

                        <div class="row-fluid">   
                            <div class="span12" >
                                <Rock:LabeledTextBox ID="txtEmail" LabelText="Email" runat="server" Required="true" CssClass="input-xlarge" />
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
            
            <div class="row-fluid">
              
                <div class="<%= _spanClass %> well">

                    <fieldset>
                    
                        <legend>Payment Information</legend>

                        <asp:Panel ID="pnlPayment" runat="server">

                            <asp:PlaceHolder ID="phContent" runat="server">

                                <ul class="nav nav-pills">
                                    <asp:Repeater ID="rptPaymentType" runat="server" >
                                    <ItemTemplate >
                                        <li class='<%# GetTabClass(Container.DataItem) %>'>
                                            <asp:LinkButton ID="lbPaymentType" runat="server" Text='<%# Container.DataItem %>' OnClick="lbPaymentType_Click" />
                                        </li>
                                    </ItemTemplate>
                                    </asp:Repeater>
                                </ul>

                                <div class="tabContent" >
                                
                                    <asp:Panel ID="pnlCreditCard" runat="server" Visible="true" > 

                                        <div class="row-fluid">
                                        
                                            <fieldset>      

                                                <Rock:NumberBox ID="txtCreditCard" runat="server" LabelText="Credit Card #" CssClass="credit-card" MaxLength="20" ValidationDataType="Double" />

                                                <ul id="ulCardType" class="card-logos">
	                                                <li class="card-visa"></li>
	                                                <li class="card-mastercard"></li>
	                                                <li class="card-amex"></li>
	                                                <li class="card-discover"></li>
                                                </ul>

                                                <asp:HiddenField id="hfCardType" runat="server" ClientIDMode="Static" />

                                            </fieldset>
                                    
                                        </div>
                           
                                        <div class="row-fluid">
                                        
                                            <div class="expiration-group">                                                                                        
                                                <label class="credit-label">Expiration Date</label>
                                                <Rock:ButtonDropDownList ID="btnMonthExpiration" runat="server" CssClass="btn btn-primary" Title="Month" />
                                                <Rock:ButtonDropDownList ID="btnYearExpiration" runat="server" CssClass="btn btn-primary" Title="Year" />                                        
                                            </div>

                                            <div>
                                                <Rock:NumberBox ID="txtCVV" LabelText="CVV #" runat="server" MaxLength="3" CssClass="input-mini" ValidationDataType="Integer" />
                                            </div> 

                                        </div>

                                        <div class="row-fluid">
                                            <Rock:LabeledTextBox ID="txtCardName" runat="server" LabelText="Name on Card" />
                                        </div>

                                    </asp:Panel>

                                    <asp:Panel ID="pnlChecking" runat="server" Visible="false">
                                    
                                        <div class="row-fluid">
                                            <div class="span6">
                                                <fieldset>
                                                    <Rock:LabeledTextBox ID="txtBankName" runat="server" LabelText="Bank Name" />
                                                    <Rock:NumberBox ID="txtRouting" runat="server" LabelText="Routing #" ValidationDataType="Double" />
                                                    <Rock:NumberBox ID="txtAccount" runat="server" LabelText="Account #" ValidationDataType="Double" />
                                                </fieldset>
                                            </div>
                                        </div>

                                        <div class="row-fluid">
                                            <div class="span6">
                                                <fieldset>
                                                    <img class="check-image" src="../../Assets/Images/check-image.png" />
                                                </fieldset>
                                            </div>
                                        </div>
                                        
                                        <div class="row-fluid">
                                            <fieldset>
                                                <Rock:LabeledRadioButtonList ID="rblAccountType" runat="server" RepeatDirection="Horizontal" LabelText="Account Type">
                                                    <asp:ListItem text="Checking" Selected="true" />
                                                    <asp:ListItem text="Savings" />
                                                </Rock:LabeledRadioButtonList>
                                            </fieldset>
                                        </div>

                                    </asp:Panel>
                                
                                </div>

                                <div id="divConfirmAddress" runat="server">

                                    <div class="row-fluid">
                                        <Rock:LabeledCheckBox ID="chkDefaultAddress" runat="server" Text="Use address from above" Checked="true"  OnCheckedChanged="chkDefaultAddress_CheckedChanged"/>

                                        <div ID="divNewAddress" runat="server" visible="false">

                                            <!-- insert new address panel here -->

                                        </div>

                                    </div>

                                </div>

                            </asp:PlaceHolder>

                        </asp:Panel>

                    </fieldset>                    

                </div>
                
            </div>

        </div>

        <div class="actions">
            <asp:LinkButton ID="btnNext" runat="server" Text="Next" CssClass="btn btn-primary" OnClick="btnNext_Click" />
        </div>
            
    </asp:Panel>       

    <asp:Panel ID="pnlConfirm" runat="server" Visible="false">
        
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

    </asp:Panel>

    <asp:Panel ID="pnlComplete" runat="server" Visible="false">
        
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

    </asp:Panel>
    
</ContentTemplate>
</asp:UpdatePanel>
