<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PhoneNumberIdentification.ascx.cs" Inherits="RockWeb.Blocks.Security.PhoneNumberIdentification" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlPhoneNumberLookup" runat="server">
            <h3 class="mt-0"><asp:Literal ID="litTitle" runat="server" /></h3>
            <fieldset>
                <Rock:NotificationBox ID="nbConfigurationError" runat="server" NotificationBoxType="Danger" Text="This block is not yet configured for use." Visible="false" />
                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Danger" Visible="false" />

                <asp:Panel ID="pnlPhoneNumberEntry" runat="server">
                    <asp:ValidationSummary ID="valValidation"
                        runat="server"
                        HeaderText="Please correct the following:"
                        CssClass="alert alert-validation"
                        ValidationGroup="vgPrimary" />
                    <div class="mb-3">
                        <asp:Literal ID="litInitialInstructions" runat="server" />
                    </div>
                    <Rock:PhoneNumberBox ID="pbPhoneNumberLookup" runat="server" ValidationGroup="vgPrimary" CountryCode="1" CssClass="input-group-lg" />
                    <Rock:BootstrapButton ID="btnLookup" runat="server" Text="Lookup" CssClass="btn btn-lg btn-primary btn-block mt-3" OnClick="btnLookup_Click" />
                </asp:Panel>

                <asp:Panel ID="pnlVerificationCodeEntry" runat="server" Visible="false">
                    <div class="mb-3">
                        <asp:Literal ID="litVerificationInstructions" runat="server" />
                    </div>

                    <div class="d-flex justify-content-center">
                        <div class="mobile-auth-row d-flex">
                            <div class="col">
                                <Rock:NumberBox ID="nbVerificationCodeBox1"
                                    runat="server"
                                    minValue="0" maxValue="9"
                                    data-box-number="1"
                                    CssClass="input-lg px-0 text-center js-verification-code js-code-1" />
                            </div>
                            <div class="col">
                                <Rock:NumberBox ID="nbVerificationCodeBox2"
                                    runat="server"
                                    data-box-number="2"
                                    CssClass="input-lg px-0 text-center js-verification-code js-code-2" />
                            </div>
                            <div class="col">
                                <Rock:NumberBox ID="nbVerificationCodeBox3"
                                    runat="server"
                                    data-box-number="3"
                                    CssClass="input-lg px-0 text-center js-verification-code js-code-3" />
                            </div>
                            <div class="col-auto align-self-center">
                                <span>-</span>
                            </div>
                            <div class="col">
                                <Rock:NumberBox ID="nbVerificationCodeBox4"
                                    runat="server"
                                    data-box-number="4"
                                    CssClass="input-lg px-0 text-center js-verification-code js-code-4" />
                            </div>
                            <div class="col">
                                <Rock:NumberBox ID="nbVerificationCodeBox5"
                                    runat="server"
                                    data-box-number="5"
                                    CssClass="input-lg px-0 text-center js-verification-code js-code-5" />
                            </div>
                            <div class="col">
                                <Rock:NumberBox ID="nbVerificationCodeBox6"
                                    runat="server"
                                    data-box-number="6"
                                    CssClass="input-lg px-0 text-center js-verification-code js-code-6" />
                            </div>
                        </div>
                    </div>


                    <Rock:BootstrapButton ID="btnVerify"
                        runat="server"
                        Text="Next"
                        OnClick="btnVerify_Click"
                        CssClass="btn btn-lg btn-primary btn-block mt-4 js-verify-button" />

                    <Rock:BootstrapButton ID="btnResend"
                        runat="server"
                        Text="Resend"
                        OnClick="btnResend_Click"
                        CssClass="btn btn-lg btn-link btn-block" />
                </asp:Panel>
                <asp:Panel ID="pnlPersonChooser" runat="server" Visible="false">
                    <div class="mb-3">
                        <asp:Literal ID="litIndividualSelectionInstructions" runat="server" />
                    </div>
                    <asp:Repeater runat="server" ID="rptPeople">
                        <ItemTemplate>
                            <Rock:BootstrapButton ID="btnPersonChooser"
                                CssClass="btn btn-lg btn-primary btn-block mt-3"
                                runat="server"
                                OnCommand="btnPersonChooser_Command"
                                Text='<%# Eval("FullName") %>'
                                CommandArgument='<%# Eval("Id") %>' />
                        </ItemTemplate>
                    </asp:Repeater>
                </asp:Panel>
            </fieldset>
        </asp:Panel>
        <asp:Panel ID="pnlNotFound" runat="server" Visible="false">
            <Rock:RockLiteral ID="litNotFoundInstructions" runat="server" />
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>


