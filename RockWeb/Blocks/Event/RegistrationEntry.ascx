<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrationEntry.ascx.cs" Inherits="RockWeb.Blocks.Event.RegistrationEntry" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
<ContentTemplate>

    <asp:ValidationSummary ID="vsSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

    <asp:Panel ID="pnlHowMany" runat="server" Visible="false">

        <div class="text-center form-inline">
            <h1>How many people will you be registering?</h1>
            <asp:HiddenField ID="hfMinRegistrants" runat="server" />
            <asp:HiddenField ID="hfMaxRegistrants" runat="server" />
            <asp:HiddenField ID="hfHowMany" runat="server" />
            <asp:Label ID="lblHowMany" runat="server" Text="1" CssClass="form-control input-lg input-width-sm" />
            <a class="btn btn-default btn-lg js-how-many-add"><i class="fa fa-plus fa-lg"></i></a>
            <a class="btn btn-default btn-lg js-how-many-subtract"><i class="fa fa-minus fa-lg"></i></a>
        </div>

        <div class="actions">
            <asp:LinkButton ID="lbHowManyNext" runat="server" AccessKey="n" Text="Next" CssClass="btn btn-primary pull-right" CausesValidation="true" OnClick="lbHowManyNext_Click" />
        </div>

    </asp:Panel>

    <asp:Panel ID="pnlRegistrant" runat="server" Visible="false">

        <h1><asp:Literal ID="lRegistrantTitle" runat="server" /></h1>
        
        <div class="js-registration-same-family">
            <Rock:RockCheckBoxList ID="cblFamilyOptions" runat="server" Label="Individual is in the same family as" RepeatDirection="Vertical" />
        </div>
        
        <asp:PlaceHolder ID="phRegistrantControls" runat="server" />
        
        <asp:PlaceHolder ID="phFees" runat="server" />
        
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
        
        <div id="divDiscountCode" runat="server" class="form-group">
            <label class="control-label">Discount Code</label>
            <asp:TextBox ID="tbDiscountCode" runat="server" CssClass="form-control input-width-md"></asp:TextBox>
            <asp:LinkButton ID="lbDiscountApply" runat="server" CssClass="btn btn-default" Text="Apply"></asp:LinkButton>
        </div>

        <asp:PlaceHolder ID="phSummaryControls" runat="server" />
        
        <div id="divPaymentInfo" runat="server" class="well">

            <h4>Payment Information</h4>

            <Rock:RockTextBox ID="txtCardFirstName" runat="server" Label="First Name on Card" Visible="false"></Rock:RockTextBox>
            <Rock:RockTextBox ID="txtCardLastName" runat="server" Label="Last Name on Card" Visible="false"></Rock:RockTextBox>
            <Rock:RockTextBox ID="txtCardName" runat="server" Label="Name on Card" Visible="false"></Rock:RockTextBox>
            <Rock:RockTextBox ID="txtCreditCard" runat="server" Label="Credit Card #" MaxLength="19" CssClass="credit-card" />

            <ul class="card-logos list-unstyled">
                <li class="card-visa"></li>
                <li class="card-mastercard"></li>
                <li class="card-amex"></li>
                <li class="card-discover"></li>
            </ul>
                                        
            <div class="row">
                <div class="col-sm-6">
                    <Rock:MonthYearPicker ID="mypExpiration" runat="server" Label="Expiration Date" />
                </div>
                <div class="col-sm-6">
                    <Rock:NumberBox ID="txtCVV" Label="Card Security Code" CssClass="input-width-xs" runat="server" MaxLength="4" />
                </div>
            </div>

            <Rock:AddressControl ID="acBillingAddress" runat="server" UseStateAbbreviation="true" UseCountryAbbreviation="false" ShowAddressLine2="false" />

        </div>

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
