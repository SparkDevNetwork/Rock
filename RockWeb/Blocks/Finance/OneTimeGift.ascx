<%@ Control Language="C#" AutoEventWireup="true" CodeFile="OneTimeGift.ascx.cs" Inherits="RockWeb.Blocks.Finance.OneTimeGift" %>

<script type="text/javascript" src="../scripts/jquery.creditCardTypeDetector.js"></script>

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

        $('.togglePanel').on('change', checkboxChange);

        $('select').selectpicker();
        
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

                            <div class="input-prepend btn-group">
                            
                                <button class="btn select dropdown-toggle" data-toggle="dropdown">Select your Campus <span class="caret"></span> </button>
                                
                                <ul id="listCampuses" class="dropdown-menu" runat="server">
                                    <asp:PlaceHolder ID="plcCampus" runat="server"> </asp:PlaceHolder>
                                </ul>
                            
                            </div>
                        
                        </div>

                        <% } %>
                    
                        <div class="row-fluid">

                            <div class="input-prepend">

                                <div class="btn-group bootstrap-select">
                            
                                    <button ID="btnFund1" class="btn dropdown-toggle clearfix" data-toggle="dropdown">Select Fund &nbsp;<span class="caret"></span> </button>
                                    
                                    <div class="dropdown-menu">
                                        <ul ID="listFunds" runat="server" style="max-height: none; overflow-y: auto;">
                                            <asp:PlaceHolder ID="plcFunds" runat="server"> </asp:PlaceHolder>                                        
                                        </ul>    
                                    </div>
                                    <span class="add-on">$</span>
                                    <input class="input-small calc" title="Enter a number" type="text" placeholder="0.00" pattern="[0-9]*" >

                                </div>
                           
                            </div>

                        </div>
                        
                        <div class="row-fluid">

                            <div class="input-prepend">

                                <div class="btn-group">
                                    
                                    <select class="selectpicker" id="fundSelect" runat="server">                                                             
                                         <option>Building</option>
                                        <option>General</option>
                                    </select>
                                    

                                    <span class="add-on">$</span>
                                    <input class="input-small calc" title="Enter a number" type="text" placeholder="0.00" pattern="[0-9]*" >

                                </div>
                           
                            </div>

                        </div>

                        <div class="row-fluid">
                        
                            <div class="btn-group">

                                <p><input id="btnAddFund" type="submit" value="Add Another Gift" class="btn btn-primary" runat="server"></p>
                                

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
    !function($) {
        var Selectpicker = function(element, options, e) {
            if (e ) {
                e.stopPropagation();
                e.preventDefault();
            }
            this.$element = $(element);
            this.$newElement = null;
            this.button = null;
            //this.options = $.extend({}, $.fn.selectpicker.defaults, this.$element.data(), typeof options == 'object' && options);
            //this.style = this.options.style;
            //this.size = this.options.size;
            this.init();
        };

        Selectpicker.prototype = {

            constructor: Selectpicker,

            init: function (e) {
                this.$element.hide();
                var classList = this.$element.attr('class') !== undefined ? this.$element.attr('class').split(/\s+/) : '';
                //var template = this.getTemplate();
                var id = this.$element.attr('id');
                //template = this.createLi(template);
                this.$element.after(template);
                this.$newElement = this.$element.next('.bootstrap-select');
                var select = this.$newElement;
                var menu = this.$newElement.find('.dropdown-menu');
                var menuA = this.$newElement.find('.dropdown-menu ul li > a');
                var liHeight = parseInt(menuA.css('line-height')) + menuA.outerHeight();
                var selectOffset_top = this.$newElement.offset().top;
                var size = 0;
                var menuHeight = 0;
                var selectHeight = this.$newElement.outerHeight();
                this.button = this.$newElement.find('> button');
                if (id !== undefined) {
                    this.button.attr('id', id);
                    $('label[for="' + id + '"]').click(function(){ select.find('button#'+id).focus(); })
                }
                for (var i = 0; i < classList.length; i++) {
                    if(classList[i] != 'selectpicker') {
                        this.$newElement.addClass(classList[i]);
                    }
                }
                //this.button.addClass(this.style);
                this.clickListener();
                //this.$element.find('optgroup').each(function() {
                //    if ($(this).attr('label')) {
                //        menu.find('.opt'+$(this).index()).eq(0).before('<dt>'+$(this).attr('label')+'</dt>');
                //    }
                //    menu.find('.opt'+$(this).index()).eq(0).parent().prev().addClass('optgroup-div');
                //});
                if (this.size == 'auto') {
                    function getSize() {
                        var selectOffset_top_scroll = selectOffset_top - $(window).scrollTop();
                        var windowHeight = window.innerHeight;
                        var menuExtras = parseInt(menu.css('padding-top')) + parseInt(menu.css('padding-bottom')) + parseInt(menu.css('border-top-width')) + parseInt(menu.css('border-bottom-width')) + parseInt(menu.css('margin-top')) + parseInt(menu.css('margin-bottom')) + 2;
                        var selectOffset_bot = windowHeight - selectOffset_top_scroll - selectHeight - menuExtras;
                        if (!select.hasClass('dropup')) {
                        size = Math.floor(selectOffset_bot/liHeight);
                        } else {
                        size = Math.floor((selectOffset_top_scroll - menuExtras)/liHeight);
                        }
                        if (size < 4) {size = 3};
                        menuHeight = liHeight*size;
                        if (menu.find('ul li').length + menu.find('dt').length > size) {
                            menu.find('ul').css({'max-height' : menuHeight + 'px', 'overflow-y' : 'scroll'});
                        } else {
                            menu.find('ul').css({'max-height' : 'none', 'overflow-y' : 'auto'});
                        }
                }
                    getSize();
                    $(window).resize(getSize);
                    $(window).scroll(getSize);
                } else if (this.size && this.size != 'auto' && menu.find('ul li').length > this.size) {
                    menuHeight = liHeight*this.size;
                    if (this.size == 1) {menuHeight = menuHeight + 8}
                    menu.find('ul').css({'max-height' : menuHeight + 'px', 'overflow-y' : 'scroll'});
                }

                this.$element.bind('DOMNodeInserted', $.proxy(this.reloadLi, this));
            },

            //getTemplate: function() {
            //    var template =
            //        "<div class='btn-group bootstrap-select'>" +
            //            "<button class='btn dropdown-toggle clearfix' data-toggle='dropdown'>" +
            //                "<span class='filter-option pull-left'>__SELECTED_OPTION</span>&nbsp;" +
            //                "<span class='caret'></span>" +
            //            "</button>" +
            //            "<div class='dropdown-menu' role='menu'>" +
            //                "<ul>" +
            //                    "__ADD_LI" +
            //                "</ul>" +
            //            "</div>" +
            //        "</div>";

            //    return template;
            //},

            reloadLi: function() {
                var _li = [];
                var _liHtml = '';

                this.$newElement.find('li').remove();

                this.$element.find('option').each(function(){
                    _li.push($(this).text());
                });

                if(_li.length > 0) {
                    for (var i = 0; i < _li.length; i++) {
                        this.$newElement.find('ul').append(
                            '<li rel=' + i + '><a tabindex="-1" href="#">' + _li[i] + '</a></li>'
                        );
                    }
                }
            },

            createLi: function(template) {

                var _li = [];
                var _liA = [];
                var _liHtml = '';
                var opt_index = null;
                var _this = this;
                var _selected_index = this.$element[0].selectedIndex ? this.$element[0].selectedIndex : 0;

                this.$element.find('option').each(function(){
                    _li.push($(this).text());
                });

                this.$element.find('option').each(function() {
                    if ($(this).parent().is('optgroup')) {
                        opt_index = String($(this).parent().index());
                        var optgroup = $(this).parent();
                        for (var i = 0; i < optgroup.length; i++) {
                            _liA.push('<a class="opt'+opt_index[i]+'" tabindex="-1" href="#">'+$(this).text()+'</a>');
                        }

                    } else {
                        _liA.push('<a tabindex="-1" href="#">'+$(this).text()+'</a>');
                    }
                });

                if (_li.length > 0) {
                    template = template.replace('__SELECTED_OPTION', _li[_selected_index]);
                    for (var i = 0; i < _li.length; i++) {
                        _liHtml += "<li rel=" + i + ">" + _liA[i] + "</li>";
                    }
                }

                this.$element.find('option').eq(_selected_index).prop('selected',true);

                template = template.replace('__ADD_LI', _liHtml);

                return template;
            },
      
            clickListener: function() {
                $('body').on('touchstart.dropdown', '.dropdown-menu', function (e) { e.stopPropagation(); });
                $('.dropdown-menu').find('li dt').on('click', function(e) {
                    e.stopPropagation();
                });
                $(this.$newElement).on('click', 'li a', function(e){
                    e.preventDefault();
                    var selected = $(this).parent().index(),
                        $this = $(this).parent(),
                        $select = $this.parents('.bootstrap-select');

                    if ($select.prev('select').not(':disabled')){

                        $select.prev('select').find('option').removeAttr('selected');

                        $select.prev('select').find('option').eq(selected).prop('selected', true).attr('selected', 'selected');
                        $select.find('.filter-option').html($this.text());
                        $select.find('button').focus();

                        // Trigger select 'change'
                        $select.prev('select').trigger('change');
                    }

                });
                this.$element.on('change', function(e) {
                    if($(this).find('option:selected').attr('title')!=undefined){
                        $(this).next('.bootstrap-select').find('.filter-option').html($(this).find('option:selected').attr('title'));
                    }else{
                        $(this).next('.bootstrap-select').find('.filter-option').html($(this).find('option:selected').text());
                    }
                });
            }

        };

        $.fn.selectpicker = function(option, event) {
            return this.eaach(function () {
                var $this = $(this),
                    data = $this.data('selectpicker'),
                    options = typeof option == 'object' && option;
                if (!data) {
                    $this.data('selectpicker', (data = new Selectpicker(this, options, event)));
                }
                if (typeof option == 'string') {
                    data[option]();
                }
            });
        };

        $.fn.selectpicker.defaults = {
            style: null,
            size: 'auto'
        }

    }(window.jQuery);

</script>