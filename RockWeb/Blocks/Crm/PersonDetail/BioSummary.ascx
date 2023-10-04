<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BioSummary.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.BioSummary" %>

<asp:UpdatePanel ID="pnlContent" runat="server">
    <ContentTemplate>
        <script>
            $(function () {
                $(".fluidbox").fluidbox({ rescale: true });
            });
        </script>

        <div id="profile-image" class="profile-image profile-image-sm">
            <!-- LPC CODE -->
            <asp:Literal ID="litPersonAlerts" runat="server" />
            <!-- END LPC CODE -->
            <div class="fluid-crop">
                <a href="#" class="fluidbox fluidbox-closed">
                    <asp:Literal ID="lImage" runat="server" />
                </a>
            </div>
        </div>

        <%-- Name and actions --%>
        <div class="profile-data">
            <%-- Account Protection Level --%>
            <asp:Literal ID="litAccountProtectionLevel" runat="server" />

            <%-- Person Name --%>
            <asp:Literal ID="lName" runat="server" />

            <%-- Badges --%>
            <div class="rockbadge-container rockbadge-xs mt-2">
                <Rock:BadgeListControl ID="blStatus" runat="server" />
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>


