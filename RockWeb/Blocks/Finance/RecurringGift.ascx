<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RecurringGift.ascx.cs" Inherits="RockWeb.Blocks.Finance.RecurringGift" %>

<asp:UpdatePanel ID="upRecurringGift" runat="server">
<ContentTemplate>
    
    <asp:Panel ID="pnlDetails" runat="server">
        
        <div class="container-fluid">     
                                
            <div class="row-fluid">
                        
                <div class="span6 well">

                    <div class="control-group">

                        <div class="btn-group">

                            <button class="btn dropdown-toggle" data-toggle="dropdown">Select your Campus <span class="caret"></span> </button>
                            
                            <ul class="dropdown-menu">
                                <li><a href="#">Anderson</a></li>
                                <li><a href="#">Greenville</a></li>
                                <li><a href="#">Spartanburg</a></li>
                            </ul>

                        </div>
                        
                    </div>

                    <div class="control-group">

                        <div class="clearfix">

                            <div class="input-prepend">

                                <div class="btn-group inline pull-left">

                                    <button class="btn dropdown-toggle" data-toggle="dropdown">Recurrence <span class="caret"></span> </button>
                            
                                    <ul class="dropdown-menu">

                                        <li><a href="#">Weekly</a></li>
                                        <li><a href="#">Bi-Weekly</a></li>
                                        <li><a href="#">Monthly</a></li>
                                        <li><a href="#">Quarterly</a></li>
                                        <li><a href="#">Yearly</a></li>

                                    </ul>

                                </div>

                                <div class="input-append date" data-date="1-02-2013" data-date-format="dd-mm-yyyy">
                            
                                    <input class="input-small" size="8" type="text" placeholder="Starting on">
                            
                                    <span class="add-on"><i class="icon-calendar"></i></span>

                                </div>

                            </div>                            
                        </div>
                                                
                    </div>

                    <div class="control-group">

                        <div class="clearfix">

                            <div class="input-prepend">
                        
                                <label class="checkbox inline">
                                    <input type="checkbox"> Limit number of gifts &nbsp
                                </label>

                                <input class="input-mini" size="4" type="text" placeholder="0">
                            </div>
                        </div>
                        
                    </div>  

                    <div class="control-group">

                        <div class="input-prepend">

                            <div class="btn-group">
                            
                                <button class="btn dropdown-toggle" data-toggle="dropdown">General Fund <span class="caret"></span> </button>
                            
                                <ul class="dropdown-menu">
                                    <li><a href="#">General Fund</a></li>
                                    <li><a href="#">Building Fund</a></li>
                                    <li><a href="#">Special Giving</a></li>                            
                                </ul>

                                <span class="add-on">$</span>
                                <input class="input-mini" type="text" value="400.00">

                            </div>
                           
                        </div>
                    </div>

                    <div class="control-group">
                        
                        <div class="input-prepend">

                            <div class="btn-group">
                            
                                <button class="btn dropdown-toggle" data-toggle="dropdown">Building Fund <span class="caret"></span> </button>
                            
                                <ul class="dropdown-menu">
                                    <li><a href="#">General Fund</a></li>
                                    <li><a href="#">Building Fund</a></li>
                                    <li><a href="#">Special Giving</a></li>                            
                                </ul>

                                <span class="add-on">$</span>
                                <input class="input-mini" id="Text2" type="text" value="50.00">

                            </div>
                           
                        </div>

                    </div>

                    <div class="control-group">
                        
                        <div class="btn-group">

                            <a href="#" class="btn btn-success"><i class="icon-white icon-plus"></i> Add Another Gift</a>

                        </div>

                    </div>                                                                
                    
                </div>
                
            
                <div class="span6 well">

                    <div class="control-group">

                        <input type="text" class="input-small" value="John" />
                        <input type="text" class="input-medium" value="Doe" />

                    </div>

                    <div class="control-group">
                        
                        <input type="text" class="input-large" value="1 Test Street" />                        
                        
                    </div>

                    <div class="control-group">
                        
                        <input type="text" class="input-small" value="Metrocity" />
                        <input type="text" class="input-small" value="NY" />                        
                        <input type="text" class="input-mini" value="12345" />                   

                    </div>                    

                    <div class="control-group">
                        
                        <input type="text" class="input-large" value="johndoe@gmail.com" />                               
                        
                    </div>

                </div>
                                
            </div><!-- row-fluid -->

            <div class="divider"><hr width="50%" size="3"></div>
            
            <div class="row-fluid">

                <div class="span6 well">

                    <div class="tabbable">

                        <ul class="nav nav-tabs">

                            <li class="active"><a href="#tab1" data-toggle="tab">Credit Card</a></li>

                            <li><a href="#tab2" data-toggle="tab">Checking/ACH</a></li>

                        </ul>

                        <div class="tab-content">

                            <div class="tab-pane active" id="tab1">
                    
                                <div class="control-group">

                                    <div class="input-append">

                                        <div class="controls">
                            
                                            <input class="input-large" type="text" value="1234567890123456">

                                        </div>
                           
                                    </div>

                                </div>
                           
                                <div class="control-group">
                        
                                    <div class="clearfix">

                                        <div class="input-prepend">

                                            <div class="btn-group inline pull-left">

                                                <button class="btn dropdown-toggle" data-toggle="dropdown">October <span class="caret"></span> </button>
                            
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

                                            <div class="btn-group inline pull-left">

                                                <button class="btn dropdown-toggle" data-toggle="dropdown">2014 <span class="caret"></span> </button>

                                                <ul class="dropdown-menu" >

                                                    <li><a href="#">2012</a></li>
                                                    <li><a href="#">2013</a></li>
                                                    <li><a href="#">2014</a></li>
                                                    <li><a href="#">2015</a></li>
                                                    <li><a href="#">2016</a></li>
                                                    <li><a href="#">2017</a></li>                                       

                                                </ul>                           

                                            </div>  

                                            <input class="input-mini inline" size="3" type="text" value="787">
                            
                                        </div>

                                    </div>
                        
                                </div>

                                <div class="control-group">

                                    <div class="text">
                            
                                        <input class="input-large" type="text" size="30" value="John Doe" />                                    
                        
                                    </div>

                                </div>
                            
                            </div>

                            <div class="tab-pane" id="tab2">
                                
                                <div class="control-group">
                                
                                    <input type="text" class="input-large" placeholder="Bank" />                                        

                                </div>

                                <div class="control-group">

                                    <input type="text" class="input-large" placeholder="Routing #" />                                        

                                </div>


                                <div class="control-group">

                                    <input type="text" class="input-large" placeholder="Account #" />

                                </div>

                                <div class="control-group">

                                    <div class="btn-group">

                                        <label class="radio inline">
                                            <input type="radio" name="optionsRadios" value="option1" checked>Checking 
                                        </label>

                                        <label class="radio inline">
                                            <input type="radio" name="optionsRadios" value="option2" checked="checked">Savings 
                                        </label>                                                      

                                    </div>

                                </div>

                            </div>

                        </div>

                    </div>

                </div>

                <div class="span6 well">

                    <div class="spacer"><br /></div>

                    <div class="control-group">

                        <div class="btn-group">

                            <label class="checkbox inline">

                                <input type="checkbox" id="cbxSaveDetails" value="option1"> Save My Information

                            </label>                                                        

                        </div>

                    </div>

                </div>

            </div>

        </div>        

        <div class="actions">
            <asp:LinkButton ID="btnNext" runat="server" Text="Next" CssClass="btn btn-primary" OnClick="btnNext_Click" />
        </div>
            
    </asp:Panel>       

    <asp:Panel ID="pnlConfirm" runat="server" Visible="false">
        
        <div class="row-fluid ">     
                                
            <div class="span6 well">

                <h3 class="header-text" >Confirm your Contribution: </h3>

                <label><b>John Doe</b>, you're about to give <b>$400.00</b> to the <b>General Fund</b> and <b>$50.00</b> to the <b>Building Fund</b>.
                
                <br /><br /> Your total gift of <b>$450.00</b> will be given using a <b>Visa</b> credit card ending in <b>3456</b>.</label><br />
               

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

