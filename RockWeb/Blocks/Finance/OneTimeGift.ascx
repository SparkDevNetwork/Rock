<%@ Control Language="C#" AutoEventWireup="true" CodeFile="OneTimeGift.ascx.cs" Inherits="RockWeb.Blocks.Finance.OneTimeGift" %>

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

<asp:UpdatePanel ID="upOneTimeGift" runat="server" >
<ContentTemplate>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert alert-error block-message error"/>

    <asp:Panel ID="pnlDetails" runat="server">

        <% spanClass = ( _UseStackedLayout ) ? "span12" : "span6"; %>
        
        <div class="container-fluid">     
                                
            <div class="row-fluid">
                                
                <div class="<%= spanClass %> well">

                    <fieldset>
                            
                        <legend>Contribution Information</legend>

                        <div class="form-horizontal">

                            <% if ( _ShowCampusSelect ) { %>
                                
                                <div class="row-fluid">
                                    <div class="control-group controls">
                                        <Rock:ButtonDropDownList ID="ddlCampusList" runat="server" CssClass="btn btn-mini" ></Rock:ButtonDropDownList>
                                    </div>
                                </div>

                            <% } %>
                        
                            <asp:Repeater ID="rptFundList" runat="server" ClientIdMode="Predictable">
                                <ItemTemplate>                                          
                                    <div class="row-fluid">                                    
                                        <div class="control-group">                                        
                                            <label id="lblFundName" class="control-label" name="lblFundName" tabindex="-1" runat="server"><%# ((KeyValuePair<string,decimal>)Container.DataItem).Key %></label> 
                                            <div class="controls">
                                                <div class="input-prepend">
                                                <span class="add-on">$</span>
                                                <input id="inputFundAmount" name="inputFundAmount" class="input-small calc" title="Enter a number" type="text" value="<%# ((KeyValuePair<string,decimal>)Container.DataItem).Value %>" runat="server" pattern="\d+(\.\d*)?" />
                                                </div>
                                            </div>
                                        </div>
                                    </div>                                        
                                </ItemTemplate>                           
                            </asp:Repeater>                        

                            <div ID="divAddFund" class="row-fluid" runat="server" visible="false">
                                <div class="control-group controls">
                                    <Rock:ButtonDropDownList ID="btnAddFund" runat="server" CssClass="btn btn-primary" OnSelectionChanged="btnAddFund_SelectionChanged" Title="Add Another Gift" ></Rock:ButtonDropDownList>                                 
                                </div>
                            </div>

                            <div class="row-fluid">
                                <div class="control-group">     
                                    <label id="lblTotalAmount" class="control-label" for="spnTotal"><b>Total Amount</b></label>
                                    <div class="controls amount-right">
                                        <b>$ <span id="spnTotal">0.00</span></b>
                                    </div>                                                        
                                </div>
                            </div>
                                                    
                        </div>

                    </fieldset>

                    <asp:ValidationSummary ID="valSummaryDetails" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error"/>

                    <%--<h4>Contribution Information</h4>--%>

                    <% if ( _ShowCampusSelect ) { %>
                            
<%--                    <div class="row-fluid">
                                
                        <Rock:ButtonDropDownList ID="ddlCampusList" runat="server" CssClass="btn btn-mini" ></Rock:ButtonDropDownList>

                    </div>--%>

                    <% } %>
                        
                    <%--<asp:Repeater ID="rptFundList" runat="server" ClientIdMode="Predictable">
                        <ItemTemplate>  
                            <div class="row-fluid">
                                <div class="control-group">
                                    <label id="lblFundName" name="lblFundName" class="control-label" for="inputFundAmount" tabindex="-1" runat="server"><%# ((KeyValuePair<string,decimal>)Container.DataItem).Key %></label> 
                                    <div class="controls input-prepend">
                                            
                                            <span class="add-on">$</span>
                                            <input id="inputFundAmount" name="inputFundAmount" class="input-small calc" title="Enter a number" type="text" value="<%# ((KeyValuePair<string,decimal>)Container.DataItem).Value %>" runat="server" pattern="\d+(\.\d*)?" />
                                            
                                    </div>
                                </div>
                            </div>
                        </ItemTemplate>                           
                    </asp:Repeater>--%>
                    
                        
                    <div class="row-fluid">
            
                        <div id="alertContribution" class="alert alert-error" style="display: none;">

                            Please enter a contribution amount in at least one fund.

                        </div>

                    </div>
                                        
                </div>
            
                <% if ( _UseStackedLayout ) { %>
                    
                </div>

                <div class="row-fluid">

                <% } %>

                <div class="<%= spanClass %> well">

                    <asp:ValidationSummary ID="valSummaryAddress" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error"/>

                    <fieldset>

                        <legend>Address Information</legend>
                        
                        <div class="row-fluid">
                            
                            <div class="span6">
                                <label for="txtFirstName">First Name</label>
                                <input id="txtFirstName" type="text" class="span12" runat="server" required />
                            </div>             

                            <div class="span6">
                                <label for="txtLastName">Last Name</label>
                                <input id="txtLastName" type="text" class="span12" runat="server" required />
                            </div>        
                            
                        </div>

                        <div class="row-fluid">
                                
                            <div class="span12">
                                <label for="txtAddress">Address</label>
                                <input id="txtAddress" type="text" class="span12" runat="server" required /> 
                            </div>
                                
                        </div>

                        <div class="row-fluid">

                            <div class="span5">
                                <label for="txtCity" >City</label>
                                <input id="txtCity" type="text" class="span12" runat="server" required />                        

                            </div>
  
                            <div class="span4">
                                <label for="ddlState">State</label>

                                <select class="state-select" id="ddlState" name="ddlState" runat="server">
	                                <option value="AL">Alabama</option>
	                                <option value="AK">Alaska</option>
	                                <option value="AZ">Arizona</option>
	                                <option value="AR">Arkansas</option>
	                                <option value="CA">California</option>
	                                <option value="CO">Colorado</option>
	                                <option value="CT">Connecticut</option>
	                                <option value="DE">Delaware</option>
	                                <option value="DC">District of Columbia</option>
	                                <option value="FL">Florida</option>
	                                <option value="GA">Georgia</option>
	                                <option value="HI">Hawaii</option>
	                                <option value="ID">Idaho</option>
	                                <option value="IL">Illinois</option>
	                                <option value="IN">Indiana</option>
	                                <option value="IA">Iowa</option>
	                                <option value="KS">Kansas</option>
	                                <option value="KY">Kentucky</option>
	                                <option value="LA">Louisiana</option>
	                                <option value="ME">Maine</option>
	                                <option value="MD">Maryland</option>
	                                <option value="MA">Massachusetts</option>
	                                <option value="MI">Michigan</option>
	                                <option value="MN">Minnesota</option>
	                                <option value="MS">Mississippi</option>
	                                <option value="MO">Missouri</option>
	                                <option value="MT">Montana</option>
	                                <option value="NE">Nebraska</option>
	                                <option value="NV">Nevada</option>
	                                <option value="NH">New Hampshire</option>
	                                <option value="NJ">New Jersey</option>
	                                <option value="NM">New Mexico</option>
	                                <option value="NY">New York</option>
	                                <option value="NC">North Carolina</option>
	                                <option value="ND">North Dakota</option>
	                                <option value="OH">Ohio</option>
	                                <option value="OK">Oklahoma</option>
	                                <option value="OR">Oregon</option>
	                                <option value="PA">Pennsylvania</option>
	                                <option value="RI">Rhode Island</option>
	                                <option value="SC">South Carolina</option>
	                                <option value="SD">South Dakota</option>
	                                <option value="TN">Tennessee</option>
	                                <option value="TX">Texas</option>
	                                <option value="UT">Utah</option>
	                                <option value="VT">Vermont</option>
	                                <option value="VA">Virginia</option>
	                                <option value="WA">Washington</option>
	                                <option value="WV">West Virginia</option>
	                                <option value="WI">Wisconsin</option>
	                                <option value="WY">Wyoming</option>
                                </select>
                                
                                <%--<input id="txtState" type="text" class="span12" runat="server" size="2" required />--%>                        
                            
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
                                            <Rock:ButtonDropDownList ID="btnMonthExpiration" runat="server" CssClass="btn btn-primary" Title="Month" ></Rock:ButtonDropDownList>                                             
                                            <Rock:ButtonDropDownList ID="btnYearExpiration" runat="server" CssClass="btn btn-primary" Title="Year" ></Rock:ButtonDropDownList> 
                                        
                                        </div>

                                        <div>
                                            <label class="control-label" for="numCVV" >CVV #</label>
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

                                <div class="tab-pane" id="tab2" <% if ( !_ShowChecking ) { %> style="display:none" <% } %> >
                                    
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

                                            <!-- <label class="radio inline">
                                                <input type="radio" name="optionsRadios" value="option1" checked="checked" > Checking 
                                            </label>

                                            <label class="radio inline">
                                                <input type="radio" name="optionsRadios" value="option2"> Savings 
                                            </label> -->

                                            <asp:RadioButtonList ID="radioAccountType" RepeatDirection="Horizontal" runat="server">
                                                <asp:ListItem text="Checking" Selected="true"></asp:ListItem>
                                                <asp:ListItem text="Savings"></asp:ListItem>
                                            </asp:RadioButtonList>

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
                
                <p><b><asp:Literal ID="cfrmName" runat="server" /></b>, you're about to give a total of <b><asp:Literal ID="litGiftTotal" Visible="true" runat="server"/></b>
                    <asp:Literal ID="litMultiGift" Visible="true" runat="server">to the following funds: </asp:Literal>
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
        
        <div class="actions">
            <asp:LinkButton ID="btnGive" runat="server" Text="Give" CssClass="btn btn-primary" OnClick="btnGive_Click" />
            <asp:LinkButton ID="btnBack" runat="server" Text="Back" CssClass="btn btn-cancel"  OnClick="btnBack_Click" />
        </div>

    </asp:Panel>

    <asp:Panel ID="pnlComplete" runat="server" Visible="false">
        
        <% spanClass = ( _UseStackedLayout ) ? "span12" : "span6"; %>

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
                
            </div>

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
        
        <div class="actions">
            <asp:LinkButton ID="btnPrint" runat="server" Text="Print" CssClass="btn btn-primary" OnClientClick="javascript:window.print();" />
        </div>

    </asp:Panel>

    <asp:HiddenField ID="hfCampusId" runat="server" />

    <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Error" Visible="false" />

</ContentTemplate>
</asp:UpdatePanel>
