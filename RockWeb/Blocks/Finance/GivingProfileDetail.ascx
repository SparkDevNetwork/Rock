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

<asp:UpdatePanel ID="upRecurringGift" runat="server" >
<ContentTemplate>

    <asp:Panel ID="pnlDetails" runat="server">

        <div class="container-fluid">     
                                
            <div class="row-fluid">
                                
                <div class="<%= spanClass %> well">

                    <fieldset>
                            
                        <legend>Contribution Information</legend>

                        <div class="form-horizontal">

                            <div id="divCampus" class="row-fluid" runat="server" visible="false">                                                            
                                
                                <Rock:LabeledButtonDropDownList ID="btnCampusList" runat="server" CssClass="btn btn-primary" Title="Select Campus" LabelText="Campus" />
                                
                            </div>
                            
                            <div id="divAccountList">                                
                                <asp:Repeater ID="rptAccountList" runat="server" >
                                <ItemTemplate>       
                                    <div class="row-fluid">
	                                    <Rock:LabeledTextBox ID="txtAccountNumber" runat="server" CssClass="input-small calc" PrependText="$" 
                                            LabelText='<%# DataBinder.Eval(Container.DataItem, "Account.PublicName") %>' 
                                            Text='<%# DataBinder.Eval(Container.DataItem, "Amount") %>'>
	                                    </Rock:LabeledTextBox>
                                    </div>
                                </ItemTemplate>                                
                                </asp:Repeater>
                            </div>                     

                            <div ID="divAddAccount" class="row-fluid" runat="server">
                                <div class="control-group controls">
                                    <Rock:ButtonDropDownList ID="btnAddAccount" runat="server" CssClass="btn btn-primary" OnSelectionChanged="btnAddAccount_SelectionChanged" Title="Add Another Gift" />
                                </div>
                            </div>

                            <% if ( rptAccountList.Items.Count != 0 ) { %>

                            <div id="divRecurrence" runat="server" visible="false">
                                
                                <div class="row-fluid">
                                    <div class="control-group">
                                        <%-- <Rock:LabeledText ID="lblTotalAmount" CssClass="control-label" LabelText="Total Amount" /> --%>
                                    
                                        <label id="lblTotalAmount" class="control-label" for="spnTotal"><b>Total Amount</b></label>
                                        <div class="controls amount-right">
                                            <b>$ <span id="spnTotal">0.00</span></b>
                                        </div>                     
                                    </div>
                                </div>

                                <div class="row-fluid">
                                    <Rock:LabeledButtonDropDownList ID="btnRecurrence" runat="server" CssClass="btn btn-primary" Title="Select Recurrence" OnSelectionChanged="btnRecurrence_SelectionChanged"  LabelText="Recurs" />			                        
                                </div>                                    
                                    
                                <div class="row-fluid">                                        
                                    <Rock:DateTimePicker ID="dtStartingDate" runat="server" LabelText="Starting On" data-date-format="dd-mm-yyyy" DatePickerType="Date" />                                        
                                </div>

                                <div class="row-fluid">
                                    <Rock:LabeledCheckBox id="cbLimitGifts" runat="server" LabelText="Limit number of gifts" />
                                    <Rock:DataTextBox ID="tbLimitGifts" runat="server" Text="0" />

                                    <%--<div class="control-group controls">
                                        <label class="checkbox">                                            
                                            <input id="cboxLimitGifts" class="togglePanel" type="checkbox" onclick="javascript: $('#grpLimitGifts').toggle()"> Limit number of gifts
                                        </label>                                      

                                        <div id="grpLimitGifts" style="display: none">                                                                                                                                        
                                            <input class="input-mini" size="4" type="text" placeholder="0">
                                        </div>

                                    </div>--%>
                                </div>

                            </div>

                            <% } %>
                        
                        </div>

                    </fieldset>                                     
                                        
                </div>
            
                <% if ( _VerticalLayout ) { %>
                    
                </div>

                <div class="row-fluid">

                <% } %>

                <div class="<%= spanClass %> well">

                    <asp:ValidationSummary ID="valSummaryAddress" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error"/>

                    <fieldset>

                        <legend>Address Information</legend>
                        
                        <div class="row-fluid">
                            
                            <div class="span6">
                                <Rock:LabeledTextBox ID="txtFirstName" LabelText="First Name" runat="server" Required="true"></Rock:LabeledTextBox>                                
                            </div>             

                            <div class="span6">
                                <Rock:LabeledTextBox ID="txtLastName" LabelText="Last Name" runat="server" Required="true"></Rock:LabeledTextBox>                                                                
                            </div>        
                            
                        </div>

                        <div class="row-fluid">
                                
                            <div class="span12">
                                <Rock:LabeledTextBox ID="txtAddress" LabelText="Address" runat="server" Required="true"></Rock:LabeledTextBox>                                                                
                            </div>
                                
                        </div>

                        <div class="row-fluid">

                            <div class="span5">
                                <Rock:LabeledTextBox ID="txtCity" LabelText="City" runat="server" Required="true"></Rock:LabeledTextBox>                                

                            </div>
  
                            <div class="span4">
                                <Rock:StateDropDownList ID="ddlState" runat="server" LabelText="State" CssClass="state-select" />
                            
                            </div>

                            <div class="span3">
                                <label for="txtZipcode" >Zipcode</label>
                                <input id="txtZipcode" type="text" class="span12" runat="server" required />                        
                            </div>

                        </div>                    

                        <div class="row-fluid">
                            
                            <div class="span12">
                                <label for="txtEmail" >Email</label>
                                <input id="txtEmail" type="text" class="span12" runat="server" required />                                                      
                            </div>
                        
                        </div>

                    </fieldset>

                </div>
                
            </div>
            
            <div class="row-fluid">
              
                <div class="<%= spanClass %> well">

                    <asp:ValidationSummary ID="valSummaryPayment" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error"/>

                    <fieldset>

                        <legend>Payment Information</legend>

                        <div class="tabbable">

                            <ul class="nav nav-tabs">

                                <li <% if ( !_ShowCreditCard ) { %> style="display:none" <% } %> class="active">
                                    <a href="#tab1" data-toggle="tab">Credit Card</a></li>

                                <li <% if ( !_ShowChecking ) { %> style="display:none" <% } %>>
                                    <a href="#tab2" data-toggle="tab">Checking/ACH</a></li>

                            </ul>

                            <div class="tab-content payment-details">

                                <div class="tab-pane active" id="tab1" <% if ( !_ShowCreditCard ) { %> style="display:none" <% } %> >
                                    
                                    <div class="row-fluid"></div>

                                    <div class="row-fluid">
                                              
                                        <label for="numCreditCard" >Credit Card #</label>
                                        <input id="numCreditCard" class="input-large credit-card" type="text" title="Credit Card Number" pattern="\d+" size="20" style="float: left" runat="server">

                                        <ul id="ulCardType" class="card-logos">
	                                        <li class="card-visa"></li>
	                                        <li class="card-mastercard"></li>
	                                        <li class="card-amex"></li>
	                                        <li class="card-discover"></li>
                                        </ul>

                                        <asp:HiddenField id="hfCardType" runat="server" ClientIDMode="Static" />
                                    
                                    </div>
                           
                                    <div class="row-fluid">
                        
                                        <div class="expiration-group">

                                            <label>Expiration Date</label>
                                            <Rock:ButtonDropDownList ID="btnMonthExpiration" runat="server" CssClass="btn btn-primary" Title="Month"></Rock:ButtonDropDownList>
                                            <Rock:ButtonDropDownList ID="btnYearExpiration" runat="server" CssClass="btn btn-primary" Title="Year"></Rock:ButtonDropDownList>
                                        
                                        </div>

                                        <div>
                                            <label class="control-label" for="numCVV" >CVV #</label>
                                            <input id="Text1" name="numCVV" title="CVV" class="input-mini" size="3" type="text" pattern="\d+" runat="server">

                                        </div> 

                                    </div>

                                    <div class="row-fluid">

                                        <label for="txtCardName" >Name on Card</label>
                                        <input id="Text2" name="txtCardName" class="input-medium" type="text" size="30" runat="server"/>
                                        
                                    </div>

                                    <% if ( _ShowSaveDetails ) { %>

                                    <div class="row-fluid">

                                        <label class="checkbox">
                                            
                                            <input id="cbxSaveCard" class="togglePanel" type="checkbox" onclick="javascript: $('#grpCardNick').toggle()" value="option1" runat="server"> Save My Card

                                        </label>                                         
                                        
                                        &nbsp;&nbsp;&nbsp;&nbsp;<div id="grpCardNick" style="display: none">
                                            
                                            <label for="txtCardNick">Enter a card nickname </label>
                                            
                                            <input id="txtCardNick" name="txtCardNick" class="input-medium"  type="text" size="30" runat="server"/>
                                        </div>                                        
                                        
                                    </div>

                                    <% } %>
                            
                                </div>

                                <div class="tab-pane" id="tab2" <% if ( !_ShowChecking ) { %> style="display:none" <% } %> >
                                    
                                    <div class="row-fluid span6">
                                
                                        <label for="txtBankName" >Bank Name</label>
                                        <input id="txtBankName" type="text" class="input-medium" runat="server"/>                                        

                                        <label for="numRouting" >Routing #</label>
                                        <input id="numRouting" type="text" class="input-medium" runat="server"/>

                                        <label for="numAccount" >Account #</label>
                                        <input id="numAccount" type="text" class="input-medium" runat="server"/>

                                    </div>

                                    <div class="row-fluid span6">                                        
                                        <img class="check-image" src="../../Assets/Images/check-image.png" data-at2x="../../Assets/Images/check-image@2x.png" />
                                    </div>
                                        
                                    <div class="row-fluid btn-group">

                                        <asp:RadioButtonList ID="radioAccountType" RepeatDirection="Horizontal" runat="server">
                                            <asp:ListItem text="Checking" Selected="true"></asp:ListItem>
                                            <asp:ListItem text="Savings"></asp:ListItem>
                                        </asp:RadioButtonList>                                       

                                    </div>  

                                    <% if ( _ShowSaveDetails ) { %>

                                    <div class="row-fluid">

                                        <label class="checkbox">
                                            
                                            <br />
                                            <input id="cbxSaveCheck" class="togglePanel" type="checkbox" onclick="javascript: $('#grpCheckNick').toggle()" value="option1"> Save My Account #

                                        </label>                                         
                                        
                                        &nbsp;&nbsp;&nbsp;&nbsp;<div id="grpCheckNick" style="display: none">
                                            
                                            <label for="txtCheckNick">Enter an account nickname </label>
                                            
                                            <input id="txtCheckNick" name="txtCheckNick" class="input-medium"  type="text" size="30" />
                                        </div>                                        
                                        
                                    </div>

                                    <% } %>

                                </div>

                            </div>

                        </div>

                    </fieldset>

                </div>
                
            </div>

        </div>

        <div class="actions">
            <asp:LinkButton ID="btnNext" runat="server" Text="Next" CssClass="btn btn-primary" OnClick="btnNext_Click" />
        </div>
            
    </asp:Panel>       

    <asp:Panel ID="pnlConfirm" runat="server" Visible="false">
        
        <% spanClass = ( _VerticalLayout ) ? "span12" : "span6"; %>

        <div class="row-fluid">     
                                
            <div class="<%= spanClass %> well">

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

                <div id="divPaymentIncomplete" runat="server" visible="false" CssClass="alert alert-error block-message error" >
                    <p><b><asp:Literal ID="payeeName" runat="server" /></b>, your payment details were not complete.                         
                        The <% Eval(litPaymentType.Text); %> number was blank or non-numeric.
                        <b><asp:Literal ID="litError" runat="server"/></b></p>                    
                </div>

            </div>

        </div>
        
        <div class="actions">
            <asp:LinkButton ID="btnGive" runat="server" Text="Give" CssClass="btn btn-primary" OnClick="btnGive_Click" />
            <asp:LinkButton ID="btnBack" runat="server" Text="Back" CssClass="btn btn-cancel"  OnClick="btnBack_Click" />
        </div>

    </asp:Panel>

    <asp:Panel ID="pnlComplete" runat="server" Visible="false">
        
        <% spanClass = ( _VerticalLayout ) ? "span12" : "span6"; %>

        <div class="row-fluid">     
                                
            <div class="<%= spanClass %> well">

                <h3 class="header-text" >Contribution Complete! </h3>
                
                <p>
                    <b><asp:Literal ID="litDateGift" runat="server"/></b>
                </p>
                
                <p> Thank you for your generosity! 
                    You just gave a total of <asp:Literal ID="litGiftTotal2" Visible="true" runat="server"/>
                    to NewSpring Church using your <asp:Literal ID="litPaymentType2" runat="server"/>.                                       
                </p>  
                 
                <label class="checkbox">
                                            
	                <p><input type="checkbox" id="cbxCreateAcct" onclick="javascript: $('#grpCreateAcct').toggle()" /> Save My Information</p> 

                </label>

                <div id="grpCreateAcct" style="display: none" >
							
	                <label for="txtUserName">Enter a Username</label>
							
	                <input id="txtUserName" name="txtUserName" class="input-medium"  type="text" size="30" />

	                <label for="txtPassword">Enter a Password</label>
							
	                <input id="txtPassword" name="txtPassword" class="input-medium"  type="password" size="30" />

                </div>                             
                
            </div>            

        </div>
        
        <div class="actions">
            <asp:LinkButton ID="btnPrint" runat="server" Text="Print" CssClass="btn btn-primary" OnClientClick="javascript:window.print();" />
        </div>

    </asp:Panel>

    <asp:HiddenField ID="hfCampusId" runat="server" />

    <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Error" Visible="false" />

</ContentTemplate>
</asp:UpdatePanel>
