<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BioSummary.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.BioSummary" %>

<asp:UpdatePanel ID="pnlContent" runat="server" class="card card-profile card-profile-bio card-profile-bio-condensed overflow-hidden h-100 m-0 flex-row align-items-center">
    <ContentTemplate>
        <div id="profile-image" class="flex-shrink-0 h-100 w-25 position-relative">
            <asp:Literal ID="lImage" runat="server" />
        </div>

        <%-- Name and actions --%>
        <div class="w-75 px-3 border-0">
            <%-- Person Name --%>
            <asp:Literal ID="lName" runat="server" />

            <%-- Badges --%>
            <div class="rockbadge-container rockbadge-xs mt-2">
                <Rock:BadgeListControl ID="blStatus" runat="server" />
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>


