<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionEntry.Ccv.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Finance.TransactionEntryCcv" %>

<asp:UpdatePanel ID="upPayment" runat="server">
    <ContentTemplate>

        <%-- hidden field to store the Transaction.Guid to use for the transaction. This is to help prevent duplicate transactions.   --%>
        <asp:HiddenField ID="hfTransactionGuid" runat="server" Value="" />

        <div class="form-horizontal" id="givingForm" runat="server" clientidmode="Static">

            <div class="form-group">
                <label for="amount" class="col-sm-3 control-label">Amount</label>
                <div class="col-sm-9">
                    <div class="input-group input-group-lg">
                        <span class="input-group-addon">$</span>
                        <asp:TextBox ID="tbAmount" runat="server" class="form-control js-amount" placeholder="0.00" v-model="amount" />
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

            <div class="js-repeating-options" v-cloak v-show="repeating" transition="slidedown">
                <div class="form-group">
                    <div class="col-sm-9 col-sm-push-3">
                        <div class="form-group">
                            <div class="col-sm-3">
                                <label for="firstGift" class="control-label">First Gift</label>
                            </div>
                            <div class="col-sm-9">
                                <input id="dpStartDate" runat="server" class="js-firstgift form-control" min="{{ tomorrowsDate }}" value="{{ tomorrowsDate }}" type="date" name="firstGift" v-model="firstGift" required="true">
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
                    <Rock:HiddenFieldWithClass ID="hfHideFullNameInput" CssClass="js-hide-full-input" runat="server" />
                    <input id="tbFullName" type="text" class="form-control js-input-fullname" placeholder="Full Name" v-on:blur="splitFullName" v-if="showSplitNameField == false" v-model="firstName" style="display:none" />

                    <asp:Literal runat="server" ID="lFullName" />
                    <div class="row js-splitname-inputs" v-if="showSplitNameField" v-cloak>
                        <div class="col-xs-6">
                            <input runat="server" type="text" class="js-firstname js-update-name form-control" id="tbFirstName" placeholder="First Name" v-model="firstName" />
                        </div>
                        <div class="col-xs-6">
                            <input runat="server" type="text" class="js-lastname js-update-name form-control" id="tbLastName" placeholder="Last Name" v-model="lastName" v-on:blur="refreshNameSplit" />
                        </div>
                    </div>
                </div>
            </div>

            <div class="form-group">
                <label for="email" class="col-sm-3 control-label">Email</label>
                <div class="col-sm-9">
                    <input runat="server" v-model="email" type="email" class="js-email form-control" id="tbEmail" placeholder="Email">
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
                    <Rock:RockRadioButtonList ID="rblSavedAccount" runat="server" CssClass="radio-list js-saved-payment-option" RepeatDirection="Vertical" DataValueField="Id" DataTextField="Name" AutoPostBack="false" />
                    <div class="cardinput js-new-cardinput" runat="server" id="pnlCardInput">
                        <input runat="server" v-model="card.number" type="text" pattern="[0-9]*" id="tbCardNumber" placeholder="Card Number" class="form-control cardinput-number">
                        <input runat="server" v-model="card.exp" type="text" pattern="[0-9]*" id="tbCardExpiry" placeholder="MM/YY" class="form-control cardinput-exp">
                        <input runat="server" v-model="card.cvc" type="text" pattern="[0-9]*" id="tbCardCvc" placeholder="CVC" class="form-control cardinput-cvc">
                    </div>
                    <input type="hidden" id="hfFullName" class="js-hf-fullname" runat="server" v-model="fullName" />

                    <div class="js-card-graphic-holder cardholder" runat="server" id="pnlCardGraphicHolder"></div>
                </div>
            </div>

            <div class="form-group">
                <label for="phone" class="control-label col-sm-3">Phone <small class="text-muted">(optional)</small></label>
                <div class="col-sm-9">
                    <input runat="server" v-model="phone" type="tel" id="tbPhone" placeholder="(000) 000-0000" class="form-control js-phone" data-inputmask="'mask': '(999) 999-9999', 'greedy': false">
                </div>
            </div>

            <div class="form-group">
                <label class="control-label col-sm-3">Address <small class="text-muted">(optional)</small></label>
                <div class="col-sm-9">
                    <div class="addressinput">
                        <input runat="server" v-model="address.street" type="text" id="tbStreet" class="form-control addressinput-street js-street-input" placeholder="Street">
                        <input runat="server" v-model="address.city" type="text" id="tbCity" class="form-control addressinput-city" placeholder="City">
                        <input runat="server" v-model="address.state" type="text" id="tbState" class="form-control addressinput-state" placeholder="State">
                        <input runat="server" v-model="address.zip" type="text" id="tbZip" class="form-control addressinput-zip" placeholder="Zip">
                    </div>
                </div>
            </div>

            <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false"></Rock:NotificationBox>

            <div class="form-group">
                <div class="col-sm-offset-3 col-sm-9">
                    <button runat="server" id="btnClear" onclick="giveForm.resetData();" class="btn btn-default js-reset-form" type="button" name="clear">Clear</button>
                    <asp:LinkButton ID="btnSubmit" runat="server" Text="Submit" CssClass="btn btn-primary pull-right js-submit-giving" OnClick="btnSubmit_Click" />
                </div>
            </div>
        </div>

        <asp:Panel ID="pnlSuccess" runat="server" Visible="false">
            <asp:Literal ID="lSuccessContent" runat="server" />
        </asp:Panel>

        <%-- pnlSaveAccount is copy/paste of the core TransactionEntry block --%>

        <asp:Panel ID="pnlSaveAccount" runat="server" Visible="false">
            <asp:HiddenField ID="hfSaveAccountTransactionCode" runat="server" />
            <asp:HiddenField ID="hfSaveAccountScheduleId" runat="server" />
            <div class="well">
                <legend><asp:Literal ID="lSaveAcccountTitle" runat="server" /></legend>
                <fieldset>
                    <Rock:RockCheckBox ID="cbSaveAccount" runat="server" Text="Save account information for future gifts" CssClass="toggle-input" />
                    <div id="divSaveAccount" runat="server" class="toggle-content">
                        <Rock:RockTextBox ID="txtSaveAccount" runat="server" Label="Name for this account" CssClass="input-large"></Rock:RockTextBox>

                        <asp:PlaceHolder ID="phCreateLogin" runat="server" Visible="false">

                            <div class="control-group">
                                <div class="controls">
                                    <div class="alert alert-info">
                                        <b>Note:</b> For security purposes you will need to login to use your saved account information.  To create
                                                a login account please provide a user name and password below. You will be sent an email with the account
                                                information above as a reminder.
                                    </div>
                                </div>
                            </div>

                            <Rock:RockTextBox ID="txtUserName" runat="server" Label="Username" CssClass="input-medium" />
                            <Rock:RockTextBox ID="txtPassword" runat="server" Label="Password" CssClass="input-medium" TextMode="Password" />
                            <Rock:RockTextBox ID="txtPasswordConfirm" runat="server" Label="Confirm Password" CssClass="input-medium" TextMode="Password" />

                        </asp:PlaceHolder>

                        <Rock:NotificationBox ID="nbSaveAccount" runat="server" Visible="false" NotificationBoxType="Danger"></Rock:NotificationBox>

                        <div id="divSaveActions" runat="server" class="actions">
                            <asp:LinkButton ID="lbSaveAccount" runat="server" Text="Save Account" CssClass="btn btn-primary" OnClick="lbSaveAccount_Click" />
                        </div>
                    </div>
                </fieldset>
            </div>
        </asp:Panel>

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

        <script>
            Sys.Application.add_load(function() {

                function showCardInputOption() {
                    if (!$('#givingForm .js-saved-payment-option input:checked').length || $('#givingForm .js-saved-payment-option input:checked').val() == "0") {
                        $('#givingForm .js-new-cardinput').show();
                        $('#givingForm .js-card-graphic-holder').show();
                    } else {
                        $('#givingForm .js-new-cardinput').hide();
                        $('#givingForm .js-card-graphic-holder').hide();
                    }
                }

                showCardInputOption();

                // show/hide card inputs based on selected saved-payment option
                $('#givingForm .js-saved-payment-option input').on('click', function () {
                    showCardInputOption();
                });

                // do the input validation on the client side
                $('#givingForm .js-submit-giving').on('click', function() {

                    $('#givingForm').find('*').removeClass("has-error");
                    var isValid = true;
                    if (!giveForm.amount)
                    {
                        isValid = false;
                        $('#givingForm .js-amount').closest('.form-group').addClass("has-error");
                    }

                    if (!giveForm.email.trim())
                    {
                        isValid = false;
                        $('#givingForm .js-email').closest('.form-group').addClass("has-error");
                    }


                    if ($('#givingForm .js-input-fullname').is(':visible') && !giveForm.fullName)
                    {
                        isValid = false;
                        $('#givingForm .js-full-name').addClass("has-error");
                    }

                    var givingWithNewCard = !$('#givingForm .js-saved-payment-option input:checked').length || $('#givingForm .js-saved-payment-option input:checked').val() == "0";

                    if (givingWithNewCard && (!giveForm.card.number || !giveForm.card.cvc || !giveForm.card.exp))
                    {
                        isValid = false;
                        $('#givingForm .js-new-cardinput').addClass("has-error");
                    }

                    return isValid;
                });
            });

        </script>

    </ContentTemplate>
</asp:UpdatePanel>
