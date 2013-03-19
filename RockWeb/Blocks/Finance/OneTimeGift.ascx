<%@ Control Language="C#" AutoEventWireup="true" CodeFile="OneTimeGift.ascx.cs" Inherits="RockWeb.Blocks.Finance.OneTimeGift" %>

<script type="text/javascript" src="../../Scripts/jquery.creditCardTypeDetector.js"></script>

<script type="text/javascript">

    function pageLoad(sender, args) {

        $('.calc').on('change', function () {

            var total = 0.00;
            
            $('.calc').each(function () {
                if ($(this).val() != '') {
                    total += parseFloat($(this).val());
                    console.log(total);
                }
            });
            
            this.value = parseFloat($(this).val()).toFixed(2);
            $('#lblTotal').html(total.toFixed(2));
        });

        $('.calc').trigger('change');

        $('.CreditCard').creditCardTypeDetector({ 'credit_card_logos': '.card_logos' });
    };

</script>

<asp:UpdatePanel ID="upOneTimeGift" runat="server" >
<ContentTemplate>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert alert-error block-message error"/>

    <asp:Panel ID="pnlDetails" runat="server">

        <% spanClass = ( _UseStackedLayout ) ? "span12" : "span6"; %>
        
        <div class="container-fluid">     
                                
            <div class="row-fluid">
                                
                <div class="<%= spanClass %> well">

                    <asp:ValidationSummary ID="valSummaryDetails" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error"/>

                    <fieldset>

                        <h4>Contribution Information</h4>

                        <% if ( _ShowCampusSelect ) { %>
                            
                        <div class="row-fluid">

                            <div class="input-prepend btn-group">
                                
                                <Rock:ButtonDropDownList ID="ddlCampusList" runat="server" CssClass="btn btn-mini" ></Rock:ButtonDropDownList>
                            
                            </div>
                        
                        </div>

                        <% } %>
                        
                        <div class="row-fluid">
            
                            <div id="alertFunds" class="alert alert-error" style="display: none;">

                                No funds are currently active.  Please add or activate a fund.

                            </div>

                        </div>

                        <asp:Repeater ID="rptFundList" runat="server" ClientIdMode="Predictable">
                            <ItemTemplate>  
                                <div class="row-fluid">
                                    <div class="input-prepend">
                                        <div class="btn-group">
                                            <input id="btnFundName" name="btnFundName" type="button" tabindex="-1" class="btn dropdown-toggle" value="<%# ((KeyValuePair<string,decimal>)Container.DataItem).Key %>" runat="server"/>
                                            <span class="add-on">$</span>
                                            <input id="inputFundAmount" name="inputFundAmount" class="input-small calc" title="Enter a number" type="text" value="<%# ((KeyValuePair<string,decimal>)Container.DataItem).Value %>" runat="server" pattern="\d+(\.\d*)?" />
                                        </div>                           
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                        
                        <div ID="divAddFund" class="row-fluid" runat="server" visible="false">
                        
                            <div class="btn-group">

                                <Rock:ButtonDropDownList ID="btnAddFund" runat="server" CssClass="btn btn-primary" OnSelectionChanged="btnAddFund_SelectionChanged" Title="Add Another Gift" ></Rock:ButtonDropDownList> 
                                
                            </div>

                        </div>                  

                        <div class="row-fluid">

                            <br /><b>Total Amount $ <span id="lblTotal">0.00</span></b>

                        </div>

                        <div class="row-fluid">
            
                            <div id="alertContribution" class="alert alert-error" style="display: none;">

                                Please enter a contribution amount in at least one fund.

                            </div>

                        </div>

                    </fieldset>
                    
                </div>
            
                <% if ( _UseStackedLayout ) { %>
                    
                </div>

                <div class="row-fluid">

                <% } %>

                <div class="<%= spanClass %> well">

                    <asp:ValidationSummary ID="valSummaryAddress" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error"/>

                    <fieldset>

                        <h4>Address Information</h4>

                        <div class="row-fluid">
                            
                            <div class="span5">
                                <label for="txtFirstName">First Name</label>
                                <input id="txtFirstName" type="text" class="span12" runat="server"/>
                            </div>             

                            <div class="span5">
                                <label for="txtLastName">Last Name</label>
                                <input id="txtLastName" type="text" class="span12" runat="server"/>
                            </div>        
                            
                        </div>

                        <div class="row-fluid">
                                
                            <div class="span10">
                                <label for="txtAddress">Address</label>
                                <input id="txtAddress" type="text" class="span12" runat="server"/> 
                            </div>
                                
                        </div>

                        <div class="row-fluid">

                            <div class="span5">
                                <label for="txtCity" >City</label>
                                <input id="txtCity" type="text" class="span12" runat="server"/>                        

                            </div>
                        
                            <div class="span2">
                                <label for="txtState" >State</label>
                                <input id="txtState" type="text" class="span12" runat="server" />                        
                            
                            </div>

                            <div class="span3">
                                <label for="txtZipcode" >Zipcode</label>
                                <input id="txtZipcode" type="text" class="span12" runat="server" />                        
                            </div>

                        </div>                    

                        <div class="row-fluid">
                            
                            <div class="span10">
                                <label for="txtEmail" >Email</label>
                                <input id="txtEmail" type="text" class="span12" runat="server" />                                                      
                            </div>
                        
                        </div>

                    </fieldset>

                </div>
                
            </div>
            
            <div class="row-fluid">
              
                <div class="<%= spanClass %> well">

                    <asp:ValidationSummary ID="valSummaryPayment" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error"/>

                    <fieldset>

                        <h4>Payment Information</h4>

                        <div class="tabbable">

                            <ul class="nav nav-tabs">

                                <li class="active"><a href="#tab1" data-toggle="tab">Credit Card</a></li>

                                <li><a href="#tab2" data-toggle="tab">Checking/ACH</a></li>

                            </ul>

                            <div class="tab-content" style="overflow: visible">

                                <div class="tab-pane active" id="tab1">
                                    
                                    <div class="row-fluid"></div>

                                    <div class="row-fluid">
                                              
                                        <label for="numCreditCard" >Credit Card #</label>
                                        <input id="numCreditCard" class="input-large CreditCard" type="text" title="Credit Card Number" pattern="\d+" size="20" style="float: left" runat="server">

                                        <ul class="card_logos">
	                                        <li class="card_visa"></li>
	                                        <li class="card_mastercard"></li>
	                                        <li class="card_amex"></li>
	                                        <li class="card_discover"></li>
                                        </ul>
                                    
                                    </div>
                           
                                    <div class="row-fluid">
                        
                                        <div class="span5" >

                                            <label>Expiration Date</label>

                                            <Rock:ButtonDropDownList ID="btnMonthExpiration" runat="server" CssClass="btn btn-primary" Title="Month" ></Rock:ButtonDropDownList> 

                                            <Rock:ButtonDropDownList ID="btnYearExpiration" runat="server" CssClass="btn btn-primary" Title="Year" ></Rock:ButtonDropDownList> 

                                        </div>

                                        <div class="span7">
                                                    
                                            <label for="numCVV" >CVV #</label>
                                            <input name="numCVV" title="CVV" class="input-mini" size="3" type="text" pattern="\d+" runat="server">

                                        </div> 
                            
                                    </div>

                                    <div class="row-fluid">

                                        <label for="txtCardName" >Name on Card</label>
                                        <input name="txtCardName" class="input-medium" type="text" size="30" runat="server"/>
                                        
                                    </div>

                                    <% if ( _ShowSaveDetails ) { %>

                                    <div class="row-fluid">

                                        <label class="checkbox">
                                            
                                            <input id="cbxSaveCard" class="togglePanel" type="checkbox" onclick="javascript: $('#grpCardNick').toggle()" value="option1" runat="server"> Save My Card

                                        </label>                                         
                                        
                                        <div id="grpCardNick" style="display: none">
                                            
                                            <label for="txtCardNick">Enter a card nickname </label>
                                            
                                            <input id="txtCardNick" name="txtCardNick" class="input-medium"  type="text" size="30" runat="server"/>
                                        </div>                                        
                                        
                                    </div>

                                    <% } %>
                            
                                </div>

                                <div class="tab-pane" id="tab2" >
                                    
                                    <div class="row-fluid"></div>

                                    <div class="row-fluid">
                                
                                        <label for="txtBankName" >Bank Name</label>
                                        <input id="txtBankName" type="text" class="input-medium" runat="server"/>                                        

                                    </div>

                                    <div class="row-fluid">

                                        <label for="numRouting" >Routing #</label>
                                        <input id="numRouting" type="text" class="input-medium" runat="server"/>

                                    </div>


                                    <div class="row-fluid">

                                        <label for="numAccount" >Account #</label>
                                        <input id="numAccount" type="text" class="input-medium" runat="server"/>

                                    </div>

                                    <div class="row-fluid">

                                        <div class="btn-group">

                                            <label class="radio inline">
                                                <input type="radio" name="optionsRadios" value="option1" checked="checked" > Checking 
                                            </label>

                                            <label class="radio inline">
                                                <input type="radio" name="optionsRadios" value="option2"> Savings 
                                            </label>

                                        </div>

                                    </div>

                                    <% if ( _ShowSaveDetails ) { %>

                                    <div class="row-fluid">

                                        <label class="checkbox">
                                            
                                            <br />
                                            <input id="cbxSaveCheck" class="togglePanel" type="checkbox" onclick="javascript: $('#grpCheckNick').toggle()" value="option1"> Save My Account #

                                        </label>                                         
                                        
                                        <div id="grpCheckNick" style="display: none">
                                            
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
        
        <% spanClass = ( _UseStackedLayout ) ? "span12" : "span6"; %>

        <div class="row-fluid">     
                                
            <div class="<%= spanClass %> well">

                <h3 class="header-text" >Confirm your Contribution: </h3>
                
                <p><b><asp:Literal ID="lName" runat="server" /></b>, thank you for your generosity! You're about to give a total of <b><asp:Literal ID="lTotal" runat="server"/></b> to the following funds:</p>
                
                <asp:Repeater ID="rptGiftConfirmation" runat="server">
                    <HeaderTemplate>
                        <ul>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <li></li>
                    </ItemTemplate>
                    <FooterTemplate>
                        </ul>
                    </FooterTemplate>
                </asp:Repeater>
                
                <p>Your gift will be paid using your <b><asp:Literal ID="lCardType" runat="server"/></b> credit card ending in <b><asp:Literal ID="lLastFour" runat="server"/></b>.</p>

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
            <asp:LinkButton ID="btnGive" runat="server" Text="Give" CssClass="btn btn-primary" />
            <asp:LinkButton ID="btnBack" runat="server" Text="Back" CssClass="btn btn-cancel"  OnClick="btnBack_Click" />
        </div>

    </asp:Panel>

    <asp:HiddenField ID="hfCampusId" runat="server" />

    <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Error" Visible="false" />

</ContentTemplate>
</asp:UpdatePanel>
