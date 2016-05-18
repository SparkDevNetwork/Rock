<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SpiritualGiftResult.ascx.cs" Inherits="Rockweb.Plugins.com_centralaz.SpiritualGifts.SpiritualGiftResult" ViewStateMode="Enabled" EnableViewState="true" %>
<style>
    .spiritual-assessment th {
        padding: 6px;
    }

        .spiritual-assessment th:last-child {
            border-radius: 0 4px 0 0;
        }

        .spiritual-assessment th.spiritual-question {
            font-weight: 700;
            padding: 6px;
            border-radius: 4px 0 0 0;
        }

    .spiritual-assessment .spiritual-question {
        width: 100%;
    }

    .spiritual-assessment .spiritual-answer {
        width: 100px;
    }

    .spiritual-heading {
        text-align: center;
    }

    .spiritualchart {
        clear: both;
        padding: 0;
        width: 80%;
        height: 425px;
        margin: 0 auto;
        max-width: 650px;
        position: relative;
    }

        .spiritualchart li {
            display: inline-block;
            height: 425px;
            margin: 0 2% 0 0;
            width: 20%;
            padding: 0;
            position: relative;
            text-align: center;
            vertical-align: bottom;
            border-radius: 6px 6px 0 0;
            background-color: #92bce0;
            color: #fff;
        }

            .spiritualchart li.spiritualchart-midpoint {
                height: 70%;
                width: 100%;
                background-color: transparent;
                position: absolute;
                bottom: 0;
                border-top: 2px solid red;
                border-radius: 0;
            }

        .spiritualchart .spiritualbar-label {
            font-weight: 500;
            overflow: hidden;
            text-transform: uppercase;
            width: 100%;
            font-size: 28px;
            bottom: -25px;
            position: absolute;
            color: black;
        }

        .spiritualchart .spiritualbar {
            border: 1px solid #ddd;
        }

            .spiritualchart .spiritualbar:before {
                position: absolute;
                left: 0;
                right: 0;
                top: 5%;
                letter-spacing: -3px;
                font-size: 20px;
                padding: 5px 1em 0;
                display: block;
                text-align: center;
                content: attr(title);
                word-wrap: break-word;
                line-height: 1em;
            }

            .spiritualchart .spiritualbar.spiritualbar-primary {
                background: #6aa3d5;
            }

    .spiritual-chart .spiritualbar-value {
        letter-spacing: -3px;
        opacity: .4;
        width: 100%;
        font-size: 30px;
        position: absolute;
    }
</style>
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
                <ul class="spiritualchart" style="text-align: center">
                    <li style="height: 100%; width: 0px;"></li>
                    <li id="giftScore_Prophecy" runat="server" class="spiritualbar">
                        <div class="spiritualbar-label" style="text-transform: initial; font-size: 19px">Prophecy</div>
                    </li>
                    <li id="giftScore_Ministry" runat="server" class="spiritualbar">
                        <div class="spiritualbar-label" style="text-transform: initial; font-size: 19px">Ministry</div>
                    </li>
                    <li id="giftScore_Teaching" runat="server" class="spiritualbar">
                        <div class="spiritualbar-label" style="text-transform: initial; font-size: 19px">Teaching</div>
                    </li>
                    <li id="giftScore_Encouragement" runat="server" class="spiritualbar">
                        <div class="spiritualbar-label" style="text-transform: initial; font-size: 17px">Encouragement</div>
                    </li>
                    <li id="giftScore_Giving" runat="server" class="spiritualbar">

                        <div class="spiritualbar-label" style="text-transform: initial; font-size: 19px">Giving</div>
                    </li>
                    <li id="giftScore_Leadership" runat="server" class="spiritualbar">
                        <div class="spiritualbar-label" style="text-transform: initial; font-size: 19px">Leadership</div>
                    </li>
                    <li id="giftScore_Mercy" runat="server" class="spiritualbar">
                        <div class="spiritualbar-label" style="text-transform: initial; font-size: 19px">Mercy</div>
                    </li>
                </ul>

                <h3>Description</h3>
                <asp:Literal ID="lDescription" runat="server"></asp:Literal>

                <div class="actions margin-t-lg">
                    <asp:Button ID="btnRetakeTest" runat="server" Visible="false" Text="Retake Test" CssClass="btn btn-default" OnClick="btnRetakeTest_Click" />
                </div>

                <div class="spiritual-attribution margin-t-lg">
                    <small>Spiritual Gifts test courtesy of Jackson Snyder at <a href="http://positivepublications.com/">positivepublications.com/</a>.</small>
                </div>
            </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
