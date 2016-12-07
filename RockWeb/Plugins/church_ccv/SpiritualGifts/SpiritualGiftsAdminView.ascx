<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SpiritualGiftsAdminView.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.SpiritualGifts.SpiritualGiftsAdminView" %>

<asp:UpdatePanel ID="upnlSettings" runat="server">
    <ContentTemplate>

        <section class="panel panel-persondetails">
            <div class="panel-heading rollover-container clearfix">
                <h3 class="panel-title pull-left">
                    <i class='fa fa-gift'></i> <asp:Literal Text="Spiritual Gifts" runat="server" />
                </h3>
            </div>

            <div class="panel-body">
                <asp:Panel ID="pnlView" runat="server" Visible="true">
                    <asp:Literal ID="lSpiritualGifts" runat="server"></asp:Literal>
                </asp:Panel>
            </div>
        </section>

    </ContentTemplate>
</asp:UpdatePanel>
