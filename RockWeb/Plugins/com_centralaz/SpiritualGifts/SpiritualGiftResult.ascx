<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SpiritualGiftResult.ascx.cs" Inherits="Rockweb.Plugins.com_centralaz.SpiritualGifts.SpiritualGiftResult" ViewStateMode="Enabled" EnableViewState="true" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbError" runat="server" Visible="false" NotificationBoxType="Danger">You have to be signed to view a spiritual gift result.</Rock:NotificationBox>

        <asp:Panel ID="pnlResults" runat="server" CssClass="panel panel-block" Visible="false">
            <div class="panel-heading">
                <h1 class="panel-title margin-t-sm"><i class="fa fa-bar-chart"></i>Spiritual Gifts for
                    <asp:Literal ID="lPersonName" runat="server" /></h1>
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlTestDate" runat="server" LabelType="Info" />
                </div>
            </div>
            <div class="panel-body">
                <asp:Literal ID="lHeading" runat="server"></asp:Literal>
                <ul class="discchart" style="text-align: center">
                    <li style="height: 100%; width: 0px;"></li>
                    <li id="giftScore_Prophecy" runat="server" class="discbar">
                        <div class="discbar-label" style="text-transform: initial; font-size: 19px">Prophecy</div>
                    </li>
                    <li id="giftScore_Ministry" runat="server" class="discbar">
                        <div class="discbar-label" style="text-transform: initial; font-size: 19px">Ministry</div>
                    </li>
                    <li id="giftScore_Teaching" runat="server" class="discbar">
                        <div class="discbar-label" style="text-transform: initial; font-size: 19px">Teaching</div>
                    </li>
                    <li id="giftScore_Encouragement" runat="server" class="discbar">
                        <div class="discbar-label" style="text-transform: initial; font-size: 19px">Encouragement</div>
                    </li>
                    <li id="giftScore_Giving" runat="server" class="discbar">
                        <div class="discbar-label" style="text-transform: initial; font-size: 19px">Giving</div>
                    </li>
                    <li id="giftScore_Leadership" runat="server" class="discbar">
                        <div class="discbar-label" style="text-transform: initial; font-size: 19px">Leadership</div>
                    </li>
                    <li id="giftScore_Mercy" runat="server" class="discbar">
                        <div class="discbar-label" style="text-transform: initial; font-size: 19px">Mercy</div>
                    </li>
                </ul>

                <h3>Description</h3>
                <asp:Literal ID="lDescription" runat="server"></asp:Literal>

                <div class="disc-attribution margin-t-lg">
                    <small>Spiritual Gifts test courtesy of Jackson Snyder at <a href="http://positivepublications.com/">positivepublications.com/</a>.</small>
                </div>
            </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
