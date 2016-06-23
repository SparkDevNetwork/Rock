<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionEntry.Ccv.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Finance.TransactionEntryCcv" %>

<asp:UpdatePanel ID="upPayment" runat="server">
    <ContentTemplate>

        <div class="container">

            <div class="form-horizontal" id="givingForm">

                <div class="form-group">
                    <label for="amount" class="col-sm-3 control-label">Amount</label>
                    <div class="col-sm-9">
                        <div class="input-group input-group-lg">
                            <span class="input-group-addon">$</span>
                            <input id="amount" runat="server" type="text" class="form-control" placeholder="0.00" v-model="amount" />
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
                            <input id="cbRepeating" runat="server" v-model="repeating" type="checkbox" class="js-repeating-toggle toggle"
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
                                    <input id="dpStartDate" runat="server" class="js-firstgift form-control" min="{{ todaysDate }}" type="date" name="firstGift" v-model="firstGift" required="true">
                                </div>
                            </div>
                            <div class="form-group">
                                <div class="col-sm-3">
                                    <label class="control-label">Schedule</label>
                                </div>
                                <div class="col-sm-9">
                                    <div class="radio">
                                        <label>
                                            <input id="rbWeekly" runat="server" type="radio" v-model="schedule" value="weekly">
                                            Weekly
                                        </label>
                                    </div>
                                    <div class="radio">
                                        <label>
                                            <input id="rbBiWeekly" runat="server" type="radio" v-model="schedule" value="biweekly">
                                            Every other week
                                        </label>
                                    </div>
                                    <div class="radio">
                                        <label>
                                            <input id="rbTwiceMonthly" runat="server" type="radio" v-model="schedule" value="twicemonthly">
                                            Twice a month
                                        </label>
                                    </div>
                                    <div class="radio">
                                        <label>
                                            <input id="rbMonthly" runat="server" type="radio" v-model="schedule" value="monthly">
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
                        <input id="tbFullName" runat="server" type="text" class="form-control" placeholder="Full Name" v-on:blur="splitFullName" v-if="showSplitNameField == false" v-model="firstName" />
                        <div class="row" v-if="showSplitNameField" v-cloak>
                            <div class="col-xs-6">
                                <input runat="server" type="text" class="js-update-name form-control" id="firstName" placeholder="First Name" v-model="firstName" />
                            </div>
                            <div class="col-xs-6">
                                <input runat="server" type="text" class="js-update-name form-control" id="lastName" placeholder="Last Name" v-model="lastName" v-on:blur="refreshNameSplit" />
                            </div>
                        </div>
                    </div>
                </div>

                <div class="form-group">
                    <label for="email" class="col-sm-3 control-label">Email</label>
                    <div class="col-sm-9">
                        <input runat="server" v-model="email" type="email" class="form-control" id="email" placeholder="Email">
                    </div>
                </div>

                <div class="form-group">
                    <label for="fund" class="control-label col-sm-3">Fund</label>
                    <div class="col-sm-9">
                        <select v-model="fund" id="fund" runat="server" class="form-control" options="givingFunds">
                            <option v-for="option in givingFunds" v-bind:value="option">{{ option }}
                            </option>
                        </select>
                    </div>
                </div>

                <div class="form-group js-card-group">
                    <label class="control-label col-sm-3">Card Info</label>
                    <div class="col-sm-9">
                        <div class="cardinput">
                            <input runat="server" v-model="card.number" type="text" pattern="[0-9]*" id="card_number" placeholder="Card Number" class="form-control cardinput-number">
                            <input runat="server" v-model="card.exp" type="text" pattern="[0-9]*" id="card_expiry" placeholder="MM/YY" class="form-control cardinput-exp">
                            <input runat="server" v-model="card.cvc" type="text" pattern="[0-9]*" id="card_cvc" placeholder="CVC" class="form-control cardinput-cvc">
                        </div>
                        <input type="hidden" id="fullName" runat="server" v-model="fullName" />

                        <div class="js-card-graphic-holder cardholder"></div>
                    </div>
                </div>

                <div class="form-group">
                    <label for="phone" class="control-label col-sm-3">Phone <small class="text-muted">(optional)</small></label>
                    <div class="col-sm-9">
                        <input runat="server" v-model="phone" type="tel" id="phone" placeholder="(000) 000-0000" class="form-control" data-inputmask="'mask': '(999) 999-9999', 'greedy': false">
                    </div>
                </div>

                <div class="form-group">
                    <label class="control-label col-sm-3">Address <small class="text-muted">(optional)</small></label>
                    <div class="col-sm-9">
                        <div class="addressinput">
                            <input runat="server" v-model="address.street" type="text" id="street" class="form-control addressinput-street" placeholder="Street">
                            <input runat="server" v-model="address.city" type="text" id="city" class="form-control addressinput-city" placeholder="City">
                            <input runat="server" v-model="address.state" type="text" id="state" class="form-control addressinput-state" placeholder="State">
                            <input runat="server" v-model="address.zip" type="text" id="zip" class="form-control addressinput-zip" placeholder="Zip">
                        </div>
                    </div>
                </div>

                <div class="form-group">
                    <div class="col-sm-offset-3 col-sm-9">
                        <button runat="server" id="btnClear" click="resetData" class="btn btn-default js-reset-form" type="button" name="clear">Clear</button>
                    </div>
                </div>
            </div>

        </div>

        <script type="text/javascript">
            var givingFunds = [
              'Anthem Campus Fund',
              'Avondale Campus Fund',
              'East Valley Campus Fund',
              'Peoria Campus Fund',
              'Scottsdale Campus Fund',
              'Surprise Campus Fund'
            ]
        </script>

        <script src="<%=ResolveUrl( "~/Plugins/church_ccv/Finance/scripts/vendor.js" ) %>"></script>
        <script src="<%=ResolveUrl( "~/Plugins/church_ccv/Finance/scripts/main.js" ) %>"></script>

        <!-- TODO: add ccv api key -->
        <script src="https://maps.googleapis.com/maps/api/js?libraries=places&callback=initAutocomplete" async defer></script>

    </ContentTemplate>
</asp:UpdatePanel>
