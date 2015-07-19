<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ExternalConnectionOpportunitySignup.ascx.cs" Inherits="RockWeb.Blocks.Connection.ExternalConnectionOpportunitySignup" %>

<asp:UpdatePanel ID="upnlOpportunityDetail" runat="server">
    <ContentTemplate>
        <Rock:RockLiteral ID="lResponseMessage" runat="server" Visible="false" />
        <asp:Literal ID="lDebug" Visible="false" runat="server"></asp:Literal>

        <asp:Panel ID="pnlSignup" runat="server">
            <div class="panel panel-block">
                <h3>
                    <asp:Literal ID="lIcon" runat="server" />
                    <asp:Literal ID="lTitle" runat="server" />
                </h3>
                <div class="panel-body">
                    <Rock:NotificationBox ID="nbInvalidMessage" runat="server" NotificationBoxType="Danger" />
                    <Rock:NotificationBox ID="NotificationBox1" runat="server" NotificationBoxType="Danger" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" ValidationGroup="Request" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" ValidationGroup="Request" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:EmailBox ID="tbEmail" runat="server" Label="Email" ValidationGroup="Request" />
                        </div>
                        <div class="col-md-6">
                            <Rock:PhoneNumberBox ID="pnHome" runat="server" Label="Home Phone" ValidationGroup="Request" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:PhoneNumberBox ID="pnMobile" runat="server" Label="Mobile Phone" ValidationGroup="Request" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockDropDownList ID="ddlCampus" runat="server" Label="Campus" ValidationGroup="Request" />
                        </div>
                    </div>

                    <Rock:RockTextBox ID="tbComments" runat="server" Label="Comments" TextMode="MultiLine" Rows="4" ValidationGroup="Request" />

                    <div class="actions">
                        <asp:LinkButton ID="btnConnect" runat="server" AccessKey="m" Text="Connect" CssClass="btn btn-primary" OnClick="btnConnect_Click" CausesValidation="false" />
                    </div>
                </div>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
