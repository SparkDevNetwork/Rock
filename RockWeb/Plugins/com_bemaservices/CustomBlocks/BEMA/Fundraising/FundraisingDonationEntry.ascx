<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FundraisingDonationEntry.ascx.cs" Inherits="RockWeb.Blocks.Fundraising.FundraisingDonationEntry" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlView" runat="server">
            <Rock:RockLiteral ID="lFundraisingOpportunity" runat="server" Label="Fundraising Opportunity" />
            <Rock:RockDropDownList ID="ddlFundraisingOpportunity" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlFundraisingOpportunity_SelectedIndexChanged" Label="Fundraising Opportunity" Required="true" />
            <Rock:RockDropDownList ID="ddlParticipant" runat="server" Label="Participant" Required="true" />

            <div class="actions pull-right">
                <asp:LinkButton ID="btnNext" runat="server" CssClass="btn btn-primary" Text="Next" OnClick="btnNext_Click" />
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
