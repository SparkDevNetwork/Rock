<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BioSummary.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.BioSummary" %>

<asp:UpdatePanel ID="pnlContent" runat="server">
    <ContentTemplate>

        <div id="divBio" runat="server" class="card card-profile card-profile-bio">

            <div id="profile-image" class="img-card-top profile-squish">
                <asp:Literal ID="lImage" runat="server" />
            </div>

            <%-- Name and actions --%>
            <div class="card-section position-relative">

                <%-- Person Name --%>
                <asp:Literal ID="lName" runat="server" />

                <%-- Badges --%>
                <div class="d-flex flex-wrap justify-content-center align-items-center mt-2">
                    <Rock:BadgeListControl ID="blStatus" runat="server" />
                </div>

            </div>

        </div>
    </ContentTemplate>
</asp:UpdatePanel>


