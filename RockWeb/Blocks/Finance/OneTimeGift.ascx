<%@ Control Language="C#" AutoEventWireup="true" CodeFile="OneTimeGift.ascx.cs" Inherits="RockWeb.Blocks.Finance.OneTimeGift" %>

<script type="text/javascript">

    $(document).ready(function () {

        $('.calc').change(function () {

            var total = 0;

            $('.calc').each(function () {
                if ($(this).val() != '') {
                    total += parseFloat($(this).val());
                }
            });

            $('#lblTotal').html(total);
        });

        $('#cbxSaveCard').change(function () {

            $('#grpCardNick').toggle();

        });

    });


</script>


<asp:UpdatePanel ID="upOneTimeGift" runat="server">
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

                            <div class="input-prepend">
                            <div class="btn-group">

                                <button class="btn dropdown-toggle" data-toggle="dropdown">Select your Campus <span class="caret"></span> </button>
                            
                                <ul class="dropdown-menu">
                                    <li><a href="#">Anderson</a></li>
                                    <li><a href="#">Greenville</a></li>
                                    <li><a href="#">Spartanburg</a></li>
                                </ul>

                            </div>
                            </div>
                        
                        </div>

                        <% } %>
                    
                        <div class="row-fluid">

                            <div class="input-prepend">

                                <div class="btn-group">
                            
                                    <button class="btn dropdown-toggle" data-toggle="dropdown">Select Fund <span class="caret"></span> </button>
                            
                                    <ul class="dropdown-menu">
                                        <li><a href="#">General Fund</a></li>
                                        <li><a href="#">Building Fund</a></li>
                                        <li><a href="#">Special Giving</a></li>                            
                                    </ul>

                                    <span class="add-on">$</span>
                                    <input class="input-small calc" title="Enter a number" type="text" placeholder="0.00" pattern="[0-9]*" >

                                </div>
                           
                            </div>

                        </div>
                        
                        <div class="row-fluid">

                            <div class="input-prepend">

                                <div class="btn-group">
                            
                                    <button class="btn dropdown-toggle" data-toggle="dropdown">Select Fund <span class="caret"></span> </button>
                            
                                    <ul class="dropdown-menu">
                                        <li><a href="#">General Fund</a></li>
                                        <li><a href="#">Building Fund</a></li>
                                        <li><a href="#">Special Giving</a></li>                            
                                    </ul>

                                    <span class="add-on">$</span>
                                    <input class="input-small calc" title="Enter a number" type="text" placeholder="0.00" pattern="[0-9]*" >

                                </div>
                           
                            </div>

                        </div>

                        <div class="row-fluid">
                        
                            <div class="btn-group">

                                <p><a href="#" class="btn btn-success"><i class="icon-white icon-plus"></i> Add Another Gift</a></p>

                            </div>

                        </div>                  

                        <div class="row-fluid">

                            <div class="span12 ">
                                <p>
                                <b>Total Amount $ </b>
                                <span id="lblTotal">0</span>

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
                            
                            <div class="span4">
                                <label for="txtFirstName">First Name</label>
                                <input id="txtFirstName" type="text" class="span12" />
                            </div>             

                            <div class="span4">
                                <label for="txtLastName">Last Name</label>
                                <input id="txtLastName" type="text" class="span12" />
                            </div>        
                            
                        </div>

                        <div class="row-fluid">
                                
                            <div class="span8">
                                <label for="txtAddress">Address</label>
                                <input id="txtAddress" type="text" class="span12" /> 
                            </div>
                                
                        </div>

                        <div class="row-fluid">

                            <div class="span4">
                                <label for="txtCity" >City</label>
                                <input id="txtCity" type="text" class="span12" />                        

                            </div>
                        
                            <div class="span2">
                                <label for="txtState" >State</label>
                                <input id="txtState" type="text" class="span12" />                        
                            
                            </div>

                            <div class="span2">
                                <label for="txtZipcode" >Zipcode</label>
                                <input id="txtZipcode" type="text" class="span12" />                        
                            </div>

                        </div>                    

                        <div class="row-fluid">
                            
                            <div class="span8">
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

                                        <div class="input-append">

                                            <div class="controls">
                                                
                                                <label for="numCreditCard">Credit Card #</label>
                                                <input id="numCreditCard" class="input-large" type="text" >

                                            </div>
                           
                                        </div>

                                        <div class="payment-icons">
                                            <div class="payment-icon visa"></div>
                                            <div class="payment-icon mastercard"></div>
                                            <div class="payment-icon discover"></div>
                                            <div class="payment-icon amex"></div>
                                            <div class="payment-icon paypal"></div>
                                        </div>

                                    </div>
                           
                                    <div class="row-fluid">
                        
                                        <div class="span4">
                                            
                                            <div class="input-prepend">

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
                                            <input name="numCVV" class="input-mini inline" size="3" type="text" >
                            
                                        </div>

                                    </div>

                                    <div class="row-fluid">

                                        <label for="txtCardName" >Name on Card</label>
                                        <input name="txtCardName" class="input-medium" type="text" size="30" />
                                        


                                    </div>

                                    <% if ( ShowSaveCard ) { %>

                                    <div class="row-fluid">

                                        <label class="checkbox">
                                            
                                            <input type="checkbox" id="cbxSaveCard" value="option1"> Save My Card

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

                                    <% if ( ShowSaveCard ) { %>

                                    <div class="row-fluid">

                                        <label class="checkbox">
                                            
                                            <p><input type="checkbox" id="cbxSaveACH" value="option1"> Save My Account #</p>

                                        </label>                                         
                                        
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

        <div class="row-fluid ">     
                                
            <div class="<%= spanClass %> well">

                <h3 class="header-text" >Confirm your Contribution: </h3>

                <label><b>John Doe</b>, you're about to give <b>$400.00</b> to the <b>General Fund</b> and <b>$50.00</b> to the <b>Building Fund</b>.
                
                <br /><br /> Your total gift of <b>$450.00</b> will be given using a <b>Visa</b> credit card ending in <b>3456</b>.</label><br />
               
            </div>

        </div>

        <div class="row-fluid">

            <label class="checkbox">
                                            
                <input type="checkbox" id="Checkbox1" value="option1"> Save My Information

            </label>                                         
                                        
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

