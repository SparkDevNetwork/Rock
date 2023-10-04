<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ConnectionOpportunitySignup.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Connections.ConnectionOpportunitySignup" %>
<asp:UpdatePanel ID="upConnectionOpportunitySignup" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Danger" />

        <asp:Panel ID="pnlSignupForm" runat="server" Visible="false">

            <asp:Literal ID="lIntro" runat="server" />
            <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="vgSignup" />
            <div class="row">
                <div class="col-md-4">
                    <Rock:RockLiteral ID="lCampus" runat="server" Label="Campus" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-4">
                    <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" Visible="true" Required="true" ValidationGroup="vgSignup" RequiredErrorMessage="First Name is required." />
                </div>
                <div class="col-md-4">
                    <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" Visible="true" Required="true" ValidationGroup="vgSignup" RequiredErrorMessage="Last Name is required." />
                </div>
                <div class="col-md-4">
                    <Rock:DatePartsPicker ID="dpBirthDate" runat="server" Label="Birth Date" Visible="true" Required="true" ValidationGroup="vgSignup" AllowFutureDates="false" RequiredErrorMessage="Birth Date is required." FutureDatesErrorMessage="" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-4">
                    <Rock:PhoneNumberBox ID="tbPhone" runat="server" Label="Phone" Visible="true" Required="true" ValidationGroup="vgSignup" RequiredErrorMessage="Phone Number is required." />
                </div>
                <div class="col-md-4">
                    <Rock:EmailBox ID="tbEmail" runat="server" Label="Email" Visible="true" Required="true" ValidationGroup="vgSignup" RequiredErrorMessage="Email Address is required." />
                </div>
            </div>
            <div class="row">
                <div class="col-md-6">
                    <Rock:RockDropDownList ID="dlOpportunities" runat="server" Label="I would like to serve as" Visible="true" Required="true"
                        ValidationGroup="vgSignup" RequiredErrorMessage="Serving Role is required." />
                </div>
            </div>
            <div class="row">
                <div class="col-md-12">
                    <Rock:RockTextBox ID="tbAdditionalInfo" runat="server" Label="Is there anything that we need to know?" TextMode="MultiLine" Rows="4" Visible="true" Required="false" />
                </div>
            </div>
            <div class="row">
                <div class="col-xs-12">
                    <asp:LinkButton ID="lbSubmit" runat="server" Visible="true" CausesValidation="true" ValidationGroup="vgSignup" CssClass="btn btn-default">Submit</asp:LinkButton>
                    <asp:LinkButton ID="lbReset" runat="server" Visible="true" CausesValidation="false" CssClass="btn btn-default">Reset</asp:LinkButton>
                </div>
            </div>
        </asp:Panel>
        <asp:Panel ID="pnlConfirmation" runat="server" Visible="false">
            <asp:Literal ID="lConfirmation" runat="server" />
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>