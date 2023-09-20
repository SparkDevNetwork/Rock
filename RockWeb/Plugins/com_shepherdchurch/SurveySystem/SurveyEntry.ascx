<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SurveyEntry.ascx.cs" Inherits="RockWeb.Plugins.com_shepherdchurch.SurveySystem.SurveyEntry" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbUnauthorized" runat="server" NotificationBoxType="Warning"></Rock:NotificationBox>
        <asp:ValidationSummary ID="vSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

        <asp:Panel ID="pnlDetails" runat="server" Visible="false">
            <div class="instructions margin-b-md">
                <asp:Literal ID="lInstructions" runat="server" />
            </div>

            <asp:PlaceHolder ID="phAttributes" runat="server" />

            <div class="actions">
                <asp:LinkButton ID="btnSubmit" runat="server" Text="Submit" CssClass="btn btn-primary" OnClick="btnSubmit_Click" />
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlResults" runat="server" Visible="false">
            <asp:Literal ID="lResults" runat="server" />
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
