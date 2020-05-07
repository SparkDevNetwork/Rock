<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrationCart.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.Event.RegistrationCart" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlMain" CssClass="panel panel-block" runat="server">
            <div class="well" style="margin-bottom: 0">
                <asp:Literal runat="server" ID="lErrorMessages"></asp:Literal>

                <asp:Repeater runat="server" ID="rData">
                    <ItemTemplate>
                        <div class="row">
                            <div class="col-md-6">
                                <strong>Event:</strong> <%# Eval("EventName") %> <%# ((bool)Eval( "AllowUpdates" )) ? "<a style='font-size: 75%' target='_blank' href='/Registration?RegistrationId=" + Eval( "RegistrationId" ) + "'>(View Event Registration)</a>" : "" %>
                                <br />
                                <strong>Registrants:</strong> <%# Eval("Registrants") %>
                            </div>
                            <div class="col-md-2">
                                <strong>Total Cost:</strong> <%# Eval("TotalCost") %>
                            </div>
                            <div class="col-md-2">
                                <strong>Total Paid:</strong> <%# Eval("TotalPaid") %>
                            </div>
                            <div class="col-md-2">
                                <Rock:CurrencyBox runat="server" ID="cPaymentAmount" MaximumValue='<%# Eval("TotalRemaining") %>' MinimumValue="1" CssClass="registration-amount" NumberType="Currency" OnTextChanged="cPaymentAmount_TextChanged" Label="Pay Amount" Text='<%# Eval( "TotalRemaining" ) %>'></Rock:CurrencyBox>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12">
                                <hr />
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>

                <div class="row">
                    <div class="col-md-offset-10 col-md-2">
                        <Rock:CurrencyBox runat="server" NumberType="Currency" CssClass="total-amount" ID="cTotal" Enabled="false" Label="Total"></Rock:CurrencyBox>
                    </div>
                </div>

            </div>



            <div class="well">
                <asp:HiddenField ID="hfPaymentTab" runat="server" />
                <asp:PlaceHolder ID="phPills" runat="server">
                    <ul class="nav nav-pills" style="padding-bottom: 20px">
                        <li id="liCreditCard" runat="server">
                            <Rock:BootstrapButton ID="btnCreditCard" runat="server" Text="Card" OnClick="btnCreditCard_Click"></Rock:BootstrapButton>
                        </li>
                        <li id="liACH" runat="server">
                            <Rock:BootstrapButton ID="btnACH" runat="server" Text="Bank Account" OnClick="btnACH_Click"></Rock:BootstrapButton>
                        </li>
                    </ul>
                </asp:PlaceHolder>

                <div id="divNewCard" runat="server" class="radio-content">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="txtCardFirstName" runat="server" Label="First Name on Card"></Rock:RockTextBox>
                        </div>
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="txtCardLastName" runat="server" Label="Last Name on Card"></Rock:RockTextBox>
                        </div>
                    </div>
                    <Rock:RockTextBox ID="txtCreditCard" runat="server" Label="Card Number" MaxLength="19" CssClass="credit-card" />
                    <ul class="card-logos list-unstyled">
                        <li class="card-visa"></li>
                        <li class="card-mastercard"></li>
                        <li class="card-amex"></li>
                        <li class="card-discover"></li>
                    </ul>
                    <div class="row">
                        <div class="col-sm-6">
                            <Rock:MonthYearPicker ID="mypExpiration" runat="server" MinimumYear="2019" Label="Expiration Date" />
                        </div>
                        <div class="col-sm-6">
                            <Rock:RockTextBox ID="txtCVV" Label="Card Security Code" CssClass="input-width-xs" runat="server" MaxLength="4" />
                        </div>
                    </div>
                    <Rock:AddressControl ID="acBillingAddress" runat="server" Label="Billing Address" UseStateAbbreviation="true" UseCountryAbbreviation="false" ShowAddressLine2="false" />
                </div>

                <div id="divACHPaymentInfo" runat="server" class="tab-pane">
                    <Rock:RockTextBox ID="txtAccountName" runat="server" Label="Name on Account" />
                    <Rock:RockTextBox ID="txtRoutingNumber" runat="server" Label="Routing Number" />
                    <Rock:RockTextBox ID="txtAccountNumber" runat="server" Label="Account Number" />
                    <Rock:RockRadioButtonList ID="rblAccountType" runat="server" RepeatDirection="Horizontal" Label="Account Type">
                        <asp:ListItem Text="Checking" Value="checking" Selected="true" />
                        <asp:ListItem Text="Savings" Value="savings" />
                    </Rock:RockRadioButtonList>
                    <asp:Image ID="imgCheck" CssClass="img-responsive" runat="server" ImageUrl="<%$ Fingerprint:~/Assets/Images/check-image.png %>" />
                </div>
            </div>
            <div class="panel panel-default no-border" style="margin-bottom: 0">
                <div class="panel-body">
                    <Rock:BootstrapButton runat="server" CssClass="btn btn-primary pull-right" ID="btnProcess" OnClick="btnProcess_Click" Text="Process"></Rock:BootstrapButton>
                </div>
            </div>

            <script>
                // This script updates the total value
                $('.registration-amount').on('input', function () {
                    {
                        var totalAmt = Number(0);
                        $('.registration-amount .form-control').each(function (index) {
                            {
                                var itemValue = $(this).val();
                                if (itemValue != null && itemValue != '') {
                                    {
                                        if (isNaN(itemValue)) {
                                            {
                                                $(this).parents('div.input-group').addClass('has-error');
                                            }
                                        }
                                        else {
                                            {
                                                $(this).parents('div.input-group').removeClass('has-error');
                                                var num = Number(itemValue);
                                                $(this).val(num.toFixed(2));
                                                totalAmt = totalAmt + num;
                                            }
                                        }
                                    }
                                }
                                else {
                                    {
                                        $(this).parents('div.input-group').removeClass('has-error');
                                    }
                                }
                            }
                        });
                        $('.total-amount input').val(totalAmt.toFixed(2));
                        return false;
                    }
                });
                // Detecting Card Type
                $('.credit-card').creditCardTypeDetector({ 'credit_card_logos': '.card-logos' });
            </script>

            </div>
        </asp:Panel>
        <asp:Panel runat="server" ID="pnlSuccess" Visible="false" CssClass="well">
            <h3 style="padding-bottom: 20px">You have successfully made payments for the following events:</h3>

            <asp:Repeater runat="server" ID="rptSuccessItems">
                <ItemTemplate>
                    <h5><%# Eval("EventName") %> - <%# Eval("Registrants") %> <%# ((bool)Eval( "AllowUpdates" )) ? "<a style='font-size: 75%' target='_blank' href='/Registration?RegistrationId=" + Eval( "RegistrationId" ) + "'>(View Event Registration)</a>" : "" %></h5>
                    <ul>
                        <li><strong>Payment Amount:</strong> $<%# Eval("PayingAmount") %></li>
                        <li><strong>Remaining Balance:</strong> $<%# Eval("TotalRemaining") %></li>
                    </ul>
                </ItemTemplate>
            </asp:Repeater>
        </asp:Panel>

        <asp:Panel runat="server" ID="pnlError" Visible="false" CssClass="well">
            <asp:Repeater runat="server" ID="rptErrors">
                <ItemTemplate>
                    <div class="alert alert-danger"><%# Container.DataItem %></div>
                </ItemTemplate>
            </asp:Repeater>
        </asp:Panel>

        <asp:Panel runat="server" ID="pnlNoEvents" Visible="false">
            <div class="alert alert-info">There are currently no events that require your attention.</div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
