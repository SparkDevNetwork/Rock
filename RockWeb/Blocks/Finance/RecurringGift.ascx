<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RecurringGift.ascx.cs" Inherits="RockWeb.Blocks.Finance.RecurringGift" %>

<asp:UpdatePanel ID="upRecurringGift" runat="server">
 
    <ContentTemplate>
    
        <asp:Panel ID="pnlDetails" runat="server">
        
        <div class="container-fluid">     
                                
            <div class="row-fluid span8">
                  
                <div class="row-fluid">
                        
                    <div class="span4">

                        <div class="row">

                            <div class="btn-group">

                                <button class="btn dropdown-toggle" data-toggle="dropdown">Select your Campus <span class="caret"></span> </button>
                            
                                <ul class="dropdown-menu">
                                    <li><a href="#">Anderson</a></li>
                                    <li><a href="#">Greenville</a></li>
                                    <li><a href="#">Spartanburg</a></li>
                                </ul>

                            </div>
                        
                        </div>

                        <div class="row">

                            <div class="btn-group span5">

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
                            
                                <input class="span4" size="16" type="text" placeholder="Starting on">
                            
                                <span class="add-on"><i class="icon-calendar"></i></span>

                            </div>
                                                
                        </div>

                        <div class="row">
                        
                            <label class="checkbox inline">
                                <input type="checkbox"> Limit number of gifts &nbsp
                            </label>

                            <input class="span2" size="12" type="text" placeholder="0">
                        
                        </div>  

                        <div class="row">

                            <div class="input-prepend">

                                <div class="btn-group">
                            
                                    <button class="btn dropdown-toggle" data-toggle="dropdown">Select Fund <span class="caret"></span> </button>
                            
                                    <ul class="dropdown-menu">
                                        <li><a href="#">General Fund</a></li>
                                        <li><a href="#">Building Fund</a></li>
                                        <li><a href="#">Special Giving</a></li>                            
                                    </ul>

                                    <span class="add-on">$</span>
                                    <input class="span3" type="text" placeholder="0.00">

                                </div>
                           
                            </div>
                        </div>

                        <div class="row">
                        
                            <div class="input-prepend">

                                <div class="btn-group">
                            
                                    <button class="btn dropdown-toggle" data-toggle="dropdown">Select Fund <span class="caret"></span> </button>
                            
                                    <ul class="dropdown-menu">
                                        <li><a href="#">General Fund</a></li>
                                        <li><a href="#">Building Fund</a></li>
                                        <li><a href="#">Special Giving</a></li>                            
                                    </ul>

                                    <span class="add-on">$</span>
                                    <input class="span3" id="Text2" type="text" placeholder="0.00">

                                </div>
                           
                            </div>

                        </div>

                        <div class="row">
                        
                            <div class="btn-group">

                                <a href="#" class="btn btn-success"><i class="icon-white icon-plus"></i> Add Another Gift</a>

                            </div>

                        </div>                                                                
                    
                    </div>
                
                    <div class="span4">

                        <div class="row"><br /></div>

                        <div class="row">

                            <input type="text" class="input-small uneditable-input" value="John" />
                            <input type="text" class="input-medium uneditable-input" value="Doe" />

                        </div>

                        <div class="row">
                        
                            <input type="text" class="input-xlarge uneditable-input" value="1 Test Street" />                        
                        
                        </div>

                        <div class="row">
                        
                            <input type="text" class="input-small uneditable-input" value="Metrocity" />
                            <input type="text" class="input-small uneditable-input" value="NY" />                        
                            <input type="text" class="input-mini uneditable-input" value="12345" />                   

                        </div>                    

                        <div class="row">
                        
                            <input type="text" class="input-xlarge uneditable-input" value="johndoe@gmail.com" />                               
                        
                        </div>

                    </div>
                        
                </div>
                                
            </div><!-- row-fluid span8 -->

            <div class="divider"><hr width="40%" size="3"></div>
            
            <div class="row-fluid">
                
                <div class="span4">
                
                    <div class="tabbable">

                        <ul class="nav nav-tabs">

                            <li class="active"><a href="#tab1" data-toggle="tab">Credit Card</a></li>
                            <li><a href="#tab2" data-toggle="tab">Checking/ACH</a></li>

                        </ul>
                    
                        <div class="tab-content">

                            <div class="tab-pane active" id="tab1">
                            
                                <div class="row-fluid">
                            
                                    <div class="row span10">

			                            <div class="input-append">

				                            <div class="btn-group">

					                            <input class="span8" type="text" value="123456789012">

					                            <button class="btn dropdown-toggle" data-toggle="dropdown">Visa <span class="caret"></span> </button>

					                            <ul class="dropdown-menu">
						                            <li><a href="#">Visa</a></li>
							                        <li><a href="#">Discover</a></li>
							                        <li><a href="#">American Express</a></li>                            
					                            </ul>

				                            </div>

			                            </div>

			                        </div>
                                 
                                    <div class="row span10">
                                
                                        <div class="btn-group span3">

					                        <button class="btn dropdown-toggle" data-toggle="dropdown">May <span class="caret"></span> </button>

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

				                        <div class="btn-group span3">

					                        <button class="btn dropdown-toggle" data-toggle="dropdown">01 <span class="caret"></span> </button>

					                        <ul class="dropdown-menu">

						                        <li><a href="#">01</a></li>
						                        <li><a href="#">02</a></li>
						                        <li><a href="#">03</a></li>
						                        <li><a href="#">04</a></li>
						                        <li><a href="#">05</a></li>
						                        <li><a href="#">06</a></li>
						                        <li><a href="#">07</a></li>
						                        <li><a href="#">08</a></li>
						                        <li><a href="#">09</a></li>
						                        <li><a href="#">10</a></li>
						                        <li><a href="#">11</a></li>
						                        <li><a href="#">12</a></li>
						                        <li><a href="#">13</a></li>
						                        <li><a href="#">14</a></li>
						                        <li><a href="#">15</a></li>
						                        <li><a href="#">16</a></li>
						                        <li><a href="#">17</a></li>
						                        <li><a href="#">18</a></li>
						                        <li><a href="#">19</a></li>
						                        <li><a href="#">20</a></li>
						                        <li><a href="#">21</a></li>
						                        <li><a href="#">22</a></li>
						                        <li><a href="#">23</a></li>
						                        <li><a href="#">24</a></li>
						                        <li><a href="#">25</a></li>
						                        <li><a href="#">26</a></li>
						                        <li><a href="#">27</a></li>
						                        <li><a href="#">28</a></li>
						                        <li><a href="#">29</a></li>
						                        <li><a href="#">30</a></li>
						                        <li><a href="#">31</a></li>

					                        </ul>

				                        </div>    

				                        <div class="btn-group span3">

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

                                    </div>

                                </div>

                            </div>

                            <div class="tab-pane" id="tab2">
                                
                                <div class="row-fluid">
                            
                                    <div class="row span8">

                                        <input type="text" class="input-large" placeholder="Bank" />                                        

                                    </div>

                                    <div class="row span8">

                                        <input type="text" class="input-large" placeholder="Routing #" />                                        

                                    </div>

                                    <div class="row span8">

                                        <input type="text" class="input-large" placeholder="Account #" />

                                    </div>

                                </div>

                                <div class="row-fluid">

                                    <div class="btn-group">

                                        <label class="radio inline">
                                            <input type="radio" name="optionsRadios" value="option1" checked>Checking 
                                        </label>

                                        <label class="radio inline">
                                            <input type="radio" name="optionsRadios" value="option2">Savings 
                                        </label>                                                      

                                    </div>

                                </div>

                            </div>

                        </div>

                    </div><!-- tabbable -->

                </div><!-- span4 -->
                
            </div>         

        </div><!-- container-fluid -->
        
        <div class="row"><br /></div>

        <div class="btn-group">

            <label class="checkbox inline">
                <input type="checkbox" id="cbxSaveDetails" value="option1"> Save my information
            </label>                                                        

        </div>

        <div class="spacer"><br></div>

        <div class="actions">
            <asp:LinkButton ID="btnNext" runat="server" Text="Next" CssClass="btn btn-primary" OnClick="btnNext_Click" />
        </div>
            
    </asp:Panel>       

    <asp:Panel ID="pnlConfirm" runat="server" Visible="false">
        
        <div class="row-fluid">     
                                
            <form class="well form-inline">

                <div class="span4">

                    <h4 class="header-text" >Confirm your Contribution: </h4>

                    <div class="row span8">

                        <div class="btn-group">

                            <button class="btn dropdown-toggle" data-toggle="dropdown">Anderson <span class="caret"></span> </button>
                            
                            <ul class="dropdown-menu">
                                <li><a href="#">Anderson</a></li>
                                <li><a href="#">Greenville</a></li>
                                <li><a href="#">Spartanburg</a></li>
                            </ul>

                        </div>
                        
                    </div>

                    <div class="row span8">

                        <div class="btn-group span5">

                            <button class="btn dropdown-toggle" data-toggle="dropdown">Monthly <span class="caret"></span> </button>
                            
                            <ul class="dropdown-menu">
                                <li><a href="#">Weekly</a></li>
                                <li><a href="#">Bi-Weekly</a></li>
                                <li><a href="#">Monthly</a></li>
                                <li><a href="#">Quarterly</a></li>
                                <li><a href="#">Yearly</a></li>
                            </ul>

                        </div>

                        <div class="input-append date" data-date="1-02-2013" data-date-format="dd-mm-yyyy">
                            
                            <input class="span4" size="16" type="text" value="02/01/2013">
                            
                            <span class="add-on"><i class="icon-calendar"></i></span>

                        </div>
                                                
                    </div>

                    <div class="row span8">
                        
                        <label class="checkbox inline">
                            <input type="checkbox"> Limit number of gifts &nbsp
                        </label>

                        <input class="span2" size="12" type="text" placeholder="0">
                        
                    </div>
                    
                    <div class="row span8">

                       <div class="input-prepend">

                          <div class="btn-group">
                            
                              <button class="btn dropdown-toggle" data-toggle="dropdown">General Fund <span class="caret"></span> </button>
                            
                              <ul class="dropdown-menu">
                                  <li><a href="#">General Fund</a></li>
                                  <li><a href="#">Building Fund</a></li>
                                  <li><a href="#">Special Giving</a></li>                            
                              </ul>

                              <span class="add-on">$</span>
                              <input class="span3" type="text" value="400.00">

                          </div>
                           
                       </div>
                        
                       <div class="input-prepend">

                          <div class="btn-group">
                            
                              <button class="btn dropdown-toggle" data-toggle="dropdown">Building Fund <span class="caret"></span> </button>
                            
                              <ul class="dropdown-menu">
                                  <li><a href="#">General Fund</a></li>
                                  <li><a href="#">Building Fund</a></li>
                                  <li><a href="#">Special Giving</a></li>                            
                              </ul>

                              <span class="add-on">$</span>
                              <input class="span3" type="text" value="50.00">

                          </div>
                           
                       </div>

                    </div>

                    <div><hr width="60%" size="3"></div>

                    <div class="row span10">

                       <div class="input-prepend">
                           
                           <label class="label inline span4">Total Gift </label>
                           <span class="add-on">$</span>
                           <input class="input-mini uneditable-input" type="text" value="450.00">
                            
                       </div>

                    </div>

                </div>

            </form>

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

