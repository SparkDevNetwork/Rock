<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionEntry.Ccv.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Finance.TransactionEntryCcv" %>

<asp:UpdatePanel ID="upPayment" runat="server">
    <ContentTemplate>

        <div class="container">

            <div class="form-horizontal" id="givingForm" runat="server" clientidmode="Static">

                <div class="form-group">
                    <label for="amount" class="col-sm-3 control-label">Amount</label>
                    <div class="col-sm-9">
                        <div class="input-group input-group-lg">
                            <span class="input-group-addon">$</span>
                            <input id="amount" runat="server" clientidmode="Static" type="text" class="form-control" placeholder="0.00" v-model="amount" />
                        </div>
                    </div>
                </div>

                <hr>

                <div class="form-group">
                    <div class="col-sm-9 col-sm-push-3">
                        <label class="toggle-label">
                            Repeat this gift?
                        </label>
                        <div class="pull-right">
                            <input id="cbRepeating" runat="server" clientidmode="Static" v-model="repeating" type="checkbox" class="js-repeating-toggle toggle"
                                data-on-text="Yes" data-off-text="No" />
                        </div>
                    </div>
                </div>

                <div v-cloak v-show="repeating" transition="slidedown">
                    <div class="form-group">
                        <div class="col-sm-9 col-sm-push-3">
                            <div class="form-group">
                                <div class="col-sm-3">
                                    <label for="firstGift" class="control-label">First Gift</label>
                                </div>
                                <div class="col-sm-9">
                                    <input id="dpStartDate" runat="server" clientidmode="Static" class="js-firstgift form-control" min="{{ todaysDate }}" type="date" name="firstGift" v-model="firstGift" required="true">
                                </div>
                            </div>
                            <div class="form-group">
                                <div class="col-sm-3">
                                    <label class="control-label">Schedule</label>
                                </div>
                                <div class="col-sm-9">
                                    <div class="radio">
                                        <label>
                                            <input id="rbWeekly" runat="server" clientidmode="Static" type="radio" v-model="schedule" value="weekly">
                                            Weekly
                                        </label>
                                    </div>
                                    <div class="radio">
                                        <label>
                                            <input id="rbBiWeekly" runat="server" clientidmode="Static" type="radio" v-model="schedule" value="biweekly">
                                            Every other week
                                        </label>
                                    </div>
                                    <div class="radio">
                                        <label>
                                            <input id="rbTwiceMonthly" runat="server" clientidmode="Static" type="radio" v-model="schedule" value="twicemonthly">
                                            Twice a month
                                        </label>
                                    </div>
                                    <div class="radio">
                                        <label>
                                            <input id="rbMonthly" runat="server" clientidmode="Static" type="radio" v-model="schedule" value="monthly">
                                            Monthly
                                        </label>
                                    </div>
                                </div>
                            </div>
                            <hr />
                        </div>
                    </div>
                </div>

                <div class="form-group js-full-name">
                    <label for="fullName" class="col-sm-3 control-label">Full Name</label>
                    <div class="col-sm-9">
                        <input type="text" class="form-control" placeholder="Full Name" v-on:blur="splitFullName" v-if="showSplitNameField == false" v-model="firstName" />
                        <div class="row" v-if="showSplitNameField" v-cloak>
                            <div class="col-xs-6">
                                <input runat="server" type="text" class="js-update-name form-control" id="firstName" clientidmode="Static" placeholder="First Name" v-model="firstName" />
                            </div>
                            <div class="col-xs-6">
                                <input runat="server" type="text" class="js-update-name form-control" id="lastName" clientidmode="Static" placeholder="Last Name" v-model="lastName" v-on:blur="refreshNameSplit" />
                            </div>
                        </div>
                    </div>
                </div>

                <div class="form-group">
                    <label for="email" class="col-sm-3 control-label">Email</label>
                    <div class="col-sm-9">
                        <input runat="server" v-model="email" type="email" class="form-control" id="email" clientidmode="Static" placeholder="Email">
                    </div>
                </div>

                <div class="form-group">
                    <label for="fund" class="control-label col-sm-3">Fund</label>
                    <div class="col-sm-9">
                        <asp:DropDownList ID="ddlAccounts" runat="server" CssClass="form-control" v-model="fund">
                            <asp:ListItem Value="609" Text="Surprise General Fund" />
                            <asp:ListItem Value="498" Text="Peoria General Fund" />
                            <asp:ListItem Value="690" Text="Scottsdale General Fund" />
                        </asp:DropDownList>
                    </div>
                </div>

                <div class="form-group js-card-group">
                    <label class="control-label col-sm-3">Card Info</label>
                    <div class="col-sm-9">
                        <div class="cardinput">
                            <input runat="server" v-model="card.number" type="text" pattern="[0-9]*" id="card_number" clientidmode="Static" placeholder="Card Number" class="form-control cardinput-number">
                            <input runat="server" v-model="card.exp" type="text" pattern="[0-9]*" id="card_expiry" clientidmode="Static" placeholder="MM/YY" class="form-control cardinput-exp">
                            <input runat="server" v-model="card.cvc" type="text" pattern="[0-9]*" id="card_cvc" clientidmode="Static" placeholder="CVC" class="form-control cardinput-cvc">
                        </div>
                        <input type="hidden" id="fullName" clientidmode="Static" runat="server" v-model="fullName" />

                        <div class="js-card-graphic-holder cardholder"></div>
                    </div>
                </div>

                <div class="form-group">
                    <label for="phone" class="control-label col-sm-3">Phone <small class="text-muted">(optional)</small></label>
                    <div class="col-sm-9">
                        <input runat="server" v-model="phone" type="tel" id="phone" clientidmode="Static" placeholder="(000) 000-0000" class="form-control" data-inputmask="'mask': '(999) 999-9999', 'greedy': false">
                    </div>
                </div>

                <div class="form-group">
                    <label class="control-label col-sm-3">Address <small class="text-muted">(optional)</small></label>
                    <div class="col-sm-9">
                        <div class="addressinput">
                            <input runat="server" v-model="address.street" type="text" id="street" clientidmode="Static" class="form-control addressinput-street" placeholder="Street">
                            <input runat="server" v-model="address.city" type="text" id="city" clientidmode="Static" class="form-control addressinput-city" placeholder="City">
                            <input runat="server" v-model="address.state" type="text" id="state" clientidmode="Static" class="form-control addressinput-state" placeholder="State">
                            <input runat="server" v-model="address.zip" type="text" id="zip" clientidmode="Static" class="form-control addressinput-zip" placeholder="Zip">
                        </div>
                    </div>
                </div>

                <div class="form-group">
                    <div class="col-sm-offset-3 col-sm-9">
                        <button runat="server" id="btnClear" clientidmode="Static" onclick="giveForm.resetData();" class="btn btn-default js-reset-form" type="button" name="clear">Clear</button>
                    </div>
                </div>
            </div>

        </div>

        <script type="text/javascript">

            // list of funds that can be selected in the account picker.  Also used to select fund based on geo location
            var givingFunds = <%= this.GivingFundsJSON %>;

            // based on their location, pick the default fund
            var campusFundLocations = <%= this.CampusFundLocationsJSON %>;
        </script>

        <script src="<%= RockPage.ResolveRockUrl( "~/Plugins/church_ccv/Finance/scripts/vendor.js", true ) %>"></script>
        <script src="<%= RockPage.ResolveRockUrl( "~/Plugins/church_ccv/Finance/scripts/main.js", true ) %>"></script>
        <script src="<%= RockPage.ResolveRockUrl( "~/Plugins/church_ccv/Finance/scripts/location-detection.js", true ) %>"></script>
        
        <script src="https://maps.googleapis.com/maps/api/js?key=AIzaSyCj2sJQbEBvz626mHM3dMHoO2H6HrWP6_M&libraries=places&callback=initAutocomplete" async defer></script>

    </ContentTemplate>
</asp:UpdatePanel>
