<%@ Control Language="C#" AutoEventWireup="true" CodeFile="OneTimeGift.ascx.cs" Inherits="RockWeb.Blocks.Finance.OneTimeGift" %>

<script type="text/javascript" src="../../Scripts/jquery.creditCardTypeDetector.js"></script>

<script type="text/javascript">

    $(document).ready(function () {

        $('.calc').change(function () {

            var total = 0;

            $('.calc').each(function () {
                if ($(this).val() != '') {
                    total += parseFloat($(this).val());
                }
            });

            $('#lblTotal').html(total.toFixed(2));
        });

        $('#numCreditCard').creditCardTypeDetector({ 'credit_card_logos': '.card_logos' });

        var checkboxChange = function () {
            $(this).parent().next('div').toggle();
        }

    });

</script>

<asp:UpdatePanel ID="upOneTimeGift" runat="server" UpdateMode="Conditional">
<ContentTemplate>

    <asp:Panel ID="pnlDetails" runat="server">

        <% spanClass = ( UseStackedLayout ) ? "span12" : "span6"; %>
        
        <div class="container-fluid">     
                                
            <div class="row-fluid">
                                
                <div class="<%= spanClass %> well">

                    <fieldset>

                        <h4>Contribution Information</h4>

                        <% if ( ShowCampusSelect ) { %>
                            
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

                        <asp:PlaceHolder ID="plcNewFunds" runat="server" Visible="false"></asp:PlaceHolder>

                        <!--<div class="row-fluid" runat="server">

                            <div class="input-prepend">

                                <div class="btn-group">
                                    
                                    <input type="button" value="General Fund" readonly="true" tabindex="-1" class="btn dropdown-toggle" />
                                    <span class="add-on">$</span>
                                    <input class="input-small calc" title="Enter a number" type="text" placeholder="0.00" pattern="[0-9]*" >

                                </div>
                           
                            </div>

                        </div>-->
                        
                        <div ID="divAddFund" class="row-fluid" runat="server" visible="false">
                        
                            <div class="btn-group">

                                <Rock:ButtonDropDownList ID="btnAddFund" runat="server" CssClass="btn btn-primary" OnClick="btnAddFund_Click" ></Rock:ButtonDropDownList> 
                                <!-- <p><input id="btnNewFund" type="submit" value="Add Another Gift" class="btn btn-primary" runat="server" onClick="btnAddFund_Click" ></p> -->
                                
                            </div>

                        </div>                  

                        <div class="row-fluid">

                            <div class="span12 ">
                                <p>
                                    <b>Total Amount $ 
                                    <span id="lblTotal">0.00</span>
                                    </b>
                                </p>                                
                            </div>

                        </div>

                        <div class="row-fluid">
            
                            <div id="alertContribution" class="alert alert-error" style="display: none;">

                                Please enter a contribution amount in at least one fund.

                            </div>

                        </div>
                        

                    </fieldset>
                    
                </div>
            
                <% if ( UseStackedLayout ) { %>
                    
                </div>

                <div class="row-fluid">

                <% } %>

                <div class="<%= spanClass %> well">

                    <fieldset>

                        <h4>Address Information</h4>

                        <div class="row-fluid">
                            
                            <div class="span5">
                                <label for="txtFirstName">First Name</label>
                                <input id="txtFirstName" type="text" class="span12" />
                            </div>             

                            <div class="span5">
                                <label for="txtLastName">Last Name</label>
                                <input id="txtLastName" type="text" class="span12" />
                            </div>        
                            
                        </div>

                        <div class="row-fluid">
                                
                            <div class="span10">
                                <label for="txtAddress">Address</label>
                                <input id="txtAddress" type="text" class="span12" /> 
                            </div>
                                
                        </div>

                        <div class="row-fluid">

                            <div class="span5">
                                <label for="txtCity" >City</label>
                                <input id="txtCity" type="text" class="span12" />                        

                            </div>
                        
                            <div class="span2">
                                <label for="txtState" >State</label>
                                <input id="txtState" type="text" class="span12" />                        
                            
                            </div>

                            <div class="span3">
                                <label for="txtZipcode" >Zipcode</label>
                                <input id="txtZipcode" type="text" class="span12" />                        
                            </div>

                        </div>                    

                        <div class="row-fluid">
                            
                            <div class="span10">
                                <label for="txtEmail" >Email</label>
                                <input id="txtEmail" type="text" class="span12"  />                                                      
                            </div>
                        
                        </div>

                    </fieldset>

                </div>
                
            </div>
            
            <div class="row-fluid">
              
                <div class="<%= spanClass %> well">

                    <fieldset>

                        <h4>Payment Information</h4>

                        <div class="tabbable">

                            <ul class="nav nav-tabs">

                                <li class="active"><a href="#tab1" data-toggle="tab">Credit Card</a></li>

                                <li><a href="#tab2" data-toggle="tab">Checking/ACH</a></li>

                            </ul>

                            <div class="tab-content" style="overflow: visible">

                                <div class="tab-pane active" id="tab1">
                    
                                    <div class="row-fluid">
                                              
                                        <label for="numCreditCard" >Credit Card #</label>
                                        <input id="numCreditCard" class="input-large" type="text" title="Credit Card Number" pattern="[0-9]*" size="20" style="float: left">

                                        <ul class="card_logos">
	                                        <li class="card_visa"></li>
	                                        <li class="card_mastercard"></li>
	                                        <li class="card_amex"></li>
	                                        <li class="card_discover"></li>
                                        </ul>
                                    
                                    </div>
                           
                                    <div class="row-fluid">
                        
                                        <div class="span4">
                                            
                                            <div class="input-prepend input-append">

                                                <label >Expiration Date</label>

                                                <div class="btn-group inline">

                                                    <button class="btn dropdown-toggle" data-toggle="dropdown">Month <span class="caret"></span> </button>
                            
                                                    <ul class="dropdown-menu">
                                                        <li><a href="#">January</a></li>
                                                        <li><a href="#">February</a></li>
                                                        <li><a href="#">March</a></li> 
                                                        <li><a href="#">April</a></li>
                                                        <li><a href="#">May</a></li>
                                                        <li><a href="#">June</a></li>
                                                        <li><a href="#">July</a></li>
                                                        <li><a href="#">August</a></li>
                                                        <li><a href="#">September</a></li>
                                                        <li><a href="#">October</a></li>
                                                        <li><a href="#">November</a></li>
                                                        <li><a href="#">December</a></li>
                                                    </ul>

                                                </div>

                                                <div class="btn-group inline">

                                                    <button class="btn dropdown-toggle" data-toggle="dropdown">Year <span class="caret"></span> </button>

                                                    <ul class="dropdown-menu" >

                                                        <li><a href="#">2012</a></li>
                                                        <li><a href="#">2013</a></li>
                                                        <li><a href="#">2014</a></li>
                                                        <li><a href="#">2015</a></li>
                                                        <li><a href="#">2016</a></li>
                                                        <li><a href="#">2017</a></li>                                       

                                                    </ul>                           

                                                </div>  

                                            </div> 
                                            
                                        </div>                                           

                                        <div class="span4">

                                            <label for="numCVV" >CVV #</label>
                                            <input name="numCVV" class="input-mini" size="3" type="text" >

                                        </div>    
                            
                                    </div>

                                    <div class="row-fluid">

                                        <label for="txtCardName" >Name on Card</label>
                                        <input name="txtCardName" class="input-medium" type="text" size="30" />
                                        
                                    </div>

                                    <% if ( ShowSaveDetails ) { %>

                                    <div class="row-fluid">

                                        <label class="checkbox">
                                            
                                            <input id="cbxSaveCard" class="togglePanel" type="checkbox" value="option1"> Save My Card

                                        </label>                                         
                                        
                                        <div id="grpCardNick" style="display: none">
                                            
                                            <label for="txtCardNick">Enter a Nickname </label>
                                            
                                            <input id="txtCardNick" name="txtCardNick" class="input-medium"  type="text" size="30" />
                                        </div>                                        
                                        
                                    </div>

                                    <% } %>
                            
                                </div>

                                <div class="tab-pane" id="tab2" >
                                
                                    <div class="row-fluid">
                                
                                        <label for="txtBankName" >Bank Name</label>
                                        <input id="txtBankName" type="text" class="input-medium" />                                        

                                    </div>

                                    <div class="row-fluid">

                                        <label for="numRouting" >Routing #</label>
                                        <input id="numRouting" type="text" class="input-medium" />

                                    </div>


                                    <div class="row-fluid">

                                        <label for="numAccount" >Account #</label>
                                        <input id="numAccount" type="text" class="input-medium" />

                                    </div>

                                    <div class="row-fluid">

                                        <div class="btn-group">

                                            <label class="radio inline">
                                                <input type="radio" name="optionsRadios" value="option1" checked="checked"> Checking 
                                            </label>

                                            <label class="radio inline">
                                                <input type="radio" name="optionsRadios" value="option2"> Savings 
                                            </label>

                                        </div>

                                    </div>

                                    <% if ( ShowSaveDetails ) { %>

                                    <div class="row-fluid">

                                        <label class="checkbox">
                                            
                                            <br />
                                            <input id="cbxSaveCheck" class="togglePanel" type="checkbox" value="option1"> Save My Account #

                                        </label>                                         
                                        
                                        <div id="grpCheckNick" style="display: none">
                                            
                                            <label for="txtCheckNick">Enter a Nickname </label>
                                            
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
        
        <% spanClass = ( UseStackedLayout ) ? "span12" : "span6"; %>

        <div class="row-fluid">     
                                
            <div class="<%= spanClass %> well">

                <h3 class="header-text" >Confirm your Contribution: </h3>
                <p>

                    <b>John Doe</b>, you're about to give <b>$400.00</b> to the <b>General Fund</b> and <b>$50.00</b> to the <b>Building Fund</b>.
                
                    Your total gift of <b>$450.00</b> will be given using a <b>Visa</b> credit card ending in <b>3456</b>.

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
            <asp:LinkButton ID="btnGive" runat="server" Text="Give" CssClass="btn btn-primary" />
            <asp:LinkButton ID="btnBack" runat="server" Text="Back" CssClass="btn btn-cancel"  OnClick="btnBack_Click" />
        </div>

    </asp:Panel>

    <asp:HiddenField ID="hfCampusId" runat="server" />

    <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Error" Visible="false" />

</ContentTemplate>
</asp:UpdatePanel>

<script type="text/javascript">


</script>