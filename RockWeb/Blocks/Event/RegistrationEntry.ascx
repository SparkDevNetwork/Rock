<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrationEntry.ascx.cs" Inherits="RockWeb.Blocks.Event.RegistrationEntry" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
<ContentTemplate>

    <asp:HiddenField ID="hfTriggerScroll" runat="server" Value="" />

    <asp:ValidationSummary ID="vsSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

    <asp:Panel ID="pnlHowMany" runat="server" Visible="false">

        <h1>How many people will you be registering?</h1>
        <Rock:NumberUpDown ID="numHowMany" runat="server" CssClass="text-center" />

        <div class="actions">
            <asp:LinkButton ID="lbHowManyNext" runat="server" AccessKey="n" Text="Next" CssClass="btn btn-primary pull-right" CausesValidation="true" OnClick="lbHowManyNext_Click" />
        </div>

    </asp:Panel>

    <asp:Panel ID="pnlRegistrant" runat="server" Visible="false">

        <h1><asp:Literal ID="lRegistrantTitle" runat="server" /></h1>
        
        <div class="js-registration-same-family">
            <Rock:RockRadioButtonList ID="rblFamilyOptions" runat="server" Label="Individual is in the same family as" RepeatDirection="Vertical" Required="true" DataTextField="Value" DataValueField="Key" />
        </div>
        
        <asp:PlaceHolder ID="phRegistrantControls" runat="server" />
        
        <div id="divFees" runat="server" class="well registration-additional-options">
            <h4>Additional Options</h4>
            <asp:PlaceHolder ID="phFees" runat="server" />
        </div>

        <div class="actions">
            <asp:LinkButton ID="lbRegistrantPrev" runat="server" AccessKey="p" Text="Previous" CssClass="btn btn-default" CausesValidation="false" OnClick="lbRegistrantPrev_Click"  />
            <asp:LinkButton ID="lbRegistrantNext" runat="server" AccessKey="n" Text="Next" CssClass="btn btn-primary pull-right" CausesValidation="true" OnClick="lbRegistrantNext_Click" />
        </div>

    </asp:Panel>

    <asp:Panel ID="pnlSummaryAndPayment" runat="server" Visible="false" >
        
        <h1>Summary</h1>
        
        <div class="well">
            <h4>Your Information</h4>
            <div class="row">
                <div class="col-md-6">
                    <Rock:RockTextBox ID="tbYourFirstName" runat="server" Label="First Name" />
                </div>
                <div class="col-md-6">
                    <Rock:RockTextBox ID="tbYourLastName" runat="server" Label="First Name" />
                </div>
            </div>
        </div>
        
        <Rock:EmailBox ID="tbConfirmationEmail" runat="server" Label="Send Confirmation Emails To" />
        
        <asp:Panel ID="pnlMoney" runat="server">

            <Rock:NotificationBox ID="nbDiscountCode" runat="server" Visible="false" NotificationBoxType="Warning"></Rock:NotificationBox>
            <div id="divDiscountCode" runat="server" class="form-group">
                <label class="control-label">Discount Code</label>
                <div class="input-group">
                    <asp:TextBox ID="tbDiscountCode" runat="server" CssClass="form-control input-width-md"></asp:TextBox>
                    <asp:LinkButton ID="lbDiscountApply" runat="server" CssClass="btn btn-default margin-l-sm" Text="Apply" OnClick="lbDiscountApply_Click" CausesValidation="false"></asp:LinkButton>
                </div>
            </div>

            <h4>Summary of Fees</h4>
            <div class="grid registration-fee-summary">
                <Rock:Grid ID="gFeeSummary" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Fees" GridLines="None" OnRowDataBound="gFeeSummary_RowDataBound" >
                    <Columns>
                        <Rock:RockBoundField DataField="Description" HtmlEncode="false" HeaderText="Description" ItemStyle-BorderStyle="None" />
                        <Rock:CurrencyField DataField="Cost" HeaderText="Amount" ItemStyle-BorderStyle="None"  />
                        <Rock:CurrencyField DataField="DiscountedCost" HeaderText="Discounted Amount" ItemStyle-BorderStyle="None" />
                    </Columns>
                </Rock:Grid>
            </div>

            <asp:HiddenField ID="hfTotalCost" runat="server" />
            <Rock:RockLiteral ID="lTotalCost" runat="server" Label="Total Cost" />

            <asp:HiddenField ID="hfPreviouslyPaid" runat="server" />
            <Rock:RockLiteral ID="lPreviouslyPaid" runat="server" Label="Previously Paid" />

            <asp:HiddenField ID="hfMinimumDue" runat="server" />
            <Rock:RockLiteral ID="lMinimumDue" runat="server" Label="Minimum Due Today" />

            <Rock:NumberBox ID="nbAmountPaid" runat="server" NumberType="Currency" Label="Amount To Pay Today" Required="true" />
            
            <Rock:RockLiteral ID="lRemainingDue" runat="server" Label="Amount Remaining" />

            <asp:PlaceHolder ID="phSummaryControls" runat="server" />
        
            <div class="well">

                <h4>Payment Information</h4>

                <Rock:RockTextBox ID="txtCardFirstName" runat="server" Label="First Name on Card" Visible="false" Required="true"></Rock:RockTextBox>
                <Rock:RockTextBox ID="txtCardLastName" runat="server" Label="Last Name on Card" Visible="false" Required="true"></Rock:RockTextBox>
                <Rock:RockTextBox ID="txtCardName" runat="server" Label="Name on Card" Visible="false" Required="true"></Rock:RockTextBox>
                <Rock:RockTextBox ID="txtCreditCard" runat="server" Label="Credit Card #" MaxLength="19" CssClass="credit-card" Required="true" />

                <ul class="card-logos list-unstyled">
                    <li class="card-visa"></li>
                    <li class="card-mastercard"></li>
                    <li class="card-amex"></li>
                    <li class="card-discover"></li>
                </ul>
                                        
                <div class="row">
                    <div class="col-sm-6">
                        <Rock:MonthYearPicker ID="mypExpiration" runat="server" Label="Expiration Date" Required="true" />
                    </div>
                    <div class="col-sm-6">
                        <Rock:NumberBox ID="txtCVV" Label="Card Security Code" CssClass="input-width-xs" runat="server" MaxLength="4" Required="true"/>
                    </div>
                </div>

                <Rock:AddressControl ID="acBillingAddress" runat="server" UseStateAbbreviation="true" UseCountryAbbreviation="false" ShowAddressLine2="false" 
                    Required="true" RequiredErrorMessage="Billing Address is required" />

            </div>

        </asp:Panel>

        <div class="actions">
            <asp:LinkButton ID="lbSummaryPrev" runat="server" AccessKey="p" Text="Previous" CssClass="btn btn-default" CausesValidation="false" OnClick="lbSummaryPrev_Click" />
            <asp:LinkButton ID="lbSummaryNext" runat="server" AccessKey="n" Text="Next" CssClass="btn btn-primary pull-right" CausesValidation="true" OnClick="lbSummaryNext_Click" />
        </div>

    </asp:Panel>

    <asp:Panel ID="pnlSuccess" runat="server" Visible="false" >
        
        <h1>Success</h1>
        
        <asp:PlaceHolder ID="phSuccessControls" runat="server" />

    </asp:Panel>

</ContentTemplate>
</asp:UpdatePanel>
