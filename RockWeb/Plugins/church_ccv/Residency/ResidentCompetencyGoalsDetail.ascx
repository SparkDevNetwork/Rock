<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ResidentCompetencyGoalsDetail.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Residency.ResidentCompetencyGoalsDetail" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server" Visible="false">
            <p>
                <asp:Literal ID="lblGoals" runat="server" />
            </p>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
