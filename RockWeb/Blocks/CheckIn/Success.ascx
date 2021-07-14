<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Success.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Success" %>


<style>
    /* Only Needed for Testing */
    .checkin-summary.checkin-body-container {
        margin-top: 72px;
    }

    h3 {
        margin-top: 0;
        margin-bottom: 24px;
    }

    .checkin-summary .card {
        text-align: left;
        margin-bottom: 30px;
    }

    .checkin-summary .person-name {
        display: block;
        font-size: 20px;
        font-weight: 700;
    }

    .checkin-celebrations > .row,
    .checkin-confirmations > .row {
        display: flex;
        flex-wrap: wrap;
        justify-content: center;
    }

    /* Celebration Adds */

    .checkin-celebrations + .checkin-confirmations {
        border-top: 1px solid #ddd;
        padding: 48px 0 0;
        margin: 36px 0 0;
    }

    .checkin-celebrations .card {
        box-shadow: 0 0 12px 0px rgb(126 205 239);
    }

    .checkin-celebrations .card-body {
        display: flex;
        flex-direction: row;
        align-items: center;
        padding: 0;
    }

    .checkin-celebrations .person-name {
        -webkit-animation: fade-in-bottom 0.6s cubic-bezier(0.25,0.1,0.25,1) 200ms both;
        animation: fade-in-bottom 0.6s cubic-bezier(0.25,0.1,0.25,1) 200ms both;
    }


    .checkin-celebrations .person-checkin-details {
        -webkit-animation: fade-in 1.2s cubic-bezier(0.25,0.1,0.25,1) 200ms both;
        animation: fade-in 1.2s cubic-bezier(0.25,0.1,0.25,1) 200ms both;
    }

    .left-icon {
        padding-left: 1rem;
        margin-right: 1rem;
        -webkit-animation: scale-in-center 0.5s cubic-bezier(0.250, 0.460, 0.450, 0.940) both;
        animation: scale-in-center 0.5s cubic-bezier(0.250, 0.460, 0.450, 0.940) both;
    }

    .right-icon {
        margin-left: auto;
        margin-right: 1rem;
    }

        .right-icon svg {
            -webkit-animation: rotate-in-diag-1 0.5s cubic-bezier(0.250, 0.460, 0.450, 0.940) 650ms both;
            animation: rotate-in-diag-1 0.5s cubic-bezier(0.250, 0.460, 0.450, 0.940) 650ms both;
        }



    .celebration-progress {
        display: flex;
        flex-wrap: wrap;
        border-top: 1px solid #ddd;
        margin: 16px 0 0;
        padding: 16px 0 0;
    }

    .celebration-progress-name {
        display: block;
        font-weight: 700;
        margin-bottom: 4px;
    }

    .celebration-progress-stat {
        font-size: 12px;
        margin-left: auto;
        align-self: flex-end;
    }

    .checkin-details {
        margin-top: 1rem;
        margin-bottom: 1rem;
    }

    .checkin-confirmations .checkin-details {
        margin-top: 0;
    }

    /* Icon Incomplete */
    .checkin-confirmations .left-icon {
        padding-left: 0;
        opacity: .6;
        -webkit-animation: none;
        animation: none;
    }

        .checkin-confirmations .left-icon.complete {
            opacity: 1;
        }


    /**
 * ----------------------------------------
 * animation rotate-in-diag-1
 * ----------------------------------------
 */
    @-webkit-keyframes rotate-in-diag-1 {
        0% {
            -webkit-transform: rotate3d(1, 1, 0, -360deg);
            transform: rotate3d(1, 1, 0, -360deg);
            opacity: 0;
        }

        100% {
            -webkit-transform: rotate3d(1, 1, 0, 0deg);
            transform: rotate3d(1, 1, 0, 0deg);
            opacity: 1;
        }
    }

    @keyframes rotate-in-diag-1 {
        0% {
            -webkit-transform: rotate3d(1, 1, 0, -360deg);
            transform: rotate3d(1, 1, 0, -360deg);
            opacity: 0;
        }

        100% {
            -webkit-transform: rotate3d(1, 1, 0, 0deg);
            transform: rotate3d(1, 1, 0, 0deg);
            opacity: 1;
        }
    }



    /**
 * ----------------------------------------
 * animation scale-in-center
 * ----------------------------------------
 */
    @-webkit-keyframes scale-in-center {
        0% {
            -webkit-transform: scale(0);
            transform: scale(0);
            opacity: 1;
        }

        100% {
            -webkit-transform: scale(1);
            transform: scale(1);
            opacity: 1;
        }
    }

    @keyframes scale-in-center {
        0% {
            -webkit-transform: scale(0);
            transform: scale(0);
            opacity: 1;
        }

        100% {
            -webkit-transform: scale(1);
            transform: scale(1);
            opacity: 1;
        }
    }

    /**
 * ----------------------------------------
 * animation fade-in-bottom
 * ----------------------------------------
 */
    @-webkit-keyframes fade-in-bottom {
        0% {
            -webkit-transform: translateY(18px);
            transform: translateY(18px);
            opacity: 0;
        }

        100% {
            -webkit-transform: translateY(0);
            transform: translateY(0);
            opacity: 1;
        }
    }

    @keyframes fade-in-bottom {
        0% {
            -webkit-transform: translateY(18px);
            transform: translateY(18px);
            opacity: 0;
        }

        100% {
            -webkit-transform: translateY(0);
            transform: translateY(0);
            opacity: 1;
        }
    }

    /**
 * ----------------------------------------
 * animation fade-in
 * ----------------------------------------
 */
    @-webkit-keyframes fade-in {
        0% {
            opacity: 0;
        }

        100% {
            opacity: 1;
        }
    }

    @keyframes fade-in {
        0% {
            opacity: 0;
        }

        100% {
            opacity: 1;
        }
    }
</style>


<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="maWarning" runat="server" />

        <div class="checkin-header">
            <h1>
                <asp:Literal ID="lTitle" runat="server" /></h1>
        </div>

        <div class="checkin-body">
            <div class="checkin-scroll-panel">
                <div class="scroller">

                    <ol class="checkin-messages checkin-body-container">
                    </ol>

                    <ol class="checkin-summary checkin-body-container">

                        <%-- The default Success Results --%>
                        <asp:Panel ID="pnlDefaultCheckinSuccessResults" runat="server">

                            <asp:Panel ID="pnlCheckinCelebrations" runat="server" Visible="false" CssClass="checkin-celebrations">

                                <h3>Celebrations</h3>
                                <div class="row">

                                    <asp:Repeater ID="rptAchievementsSuccess" runat="server" OnItemDataBound="rptAchievementsSuccess_ItemDataBound">
                                        <ItemTemplate>
                                            <div class="col-xs-12 col-lg-4">
                                                <div class="card">
                                                    <div class="card-body">

                                                        <asp:Literal ID="lAchievementSuccessHtml" runat="server" />

                                                    </div>
                                                </div>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>

                                </div>

                            </asp:Panel>

                            <%-- List of Attendances' Checkin Results, and any in-progress Achievements for each--%>
                            <asp:Panel ID="pnlCheckinConfirmations" runat="server" Visible="true" CssClass="checkin-confirmations">
                                <h3>Check-in Confirmation</h3>

                                <div class="row">
                                    <asp:Repeater ID="rptCheckinResults" runat="server" OnItemDataBound="rptCheckinResults_ItemDataBound">
                                        <ItemTemplate>
                                            <div class="col-xs-12 col-md-6 col-lg-4">
                                                <div class="card">
                                                    <div class="card-body">
                                                        <div class="checkin-details">
                                                            <%-- Person Name and Checkin Message (ex: Noah, Group in Location at Time)  --%>
                                                            <span class="person-name">
                                                                <asp:Literal ID="lCheckinResultsPersonName" runat="server" /></span>
                                                            <span>
                                                                <asp:Literal ID="lCheckinResultsCheckinMessage" runat="server" /></span>
                                                        </div>

                                                        <%-- List of InProgress and Completed Achievements for each CheckinResult --%>
                                                        <asp:Panel ID="pnlCheckinResultsAchievementsScoreboard" runat="server" Visible="false">
                                                            <asp:Repeater ID="rptCheckinResultsAchievementsScoreboard" runat="server" OnItemDataBound="rptCheckinResultsAchievementsScoreboard_ItemDataBound">
                                                                <ItemTemplate>
                                                                    <%-- HTML for the AchievmentType's Custom Summary Lava Template --%>
                                                                    <asp:Literal ID="lCheckinResultsAchievementScoreboardHtml" runat="server" />
                                                                </ItemTemplate>
                                                            </asp:Repeater>
                                                        </asp:Panel>

                                                    </div>

                                                </div>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </div>

                            </asp:Panel>
                        </asp:Panel>

                        <%-- If CustomSuccessLavaTemplateHtml option is enabled, show it here --%>
                        <asp:Literal ID="lCustomSuccessLavaTemplateHtml" runat="server" />


                        <%-- The QR Code (for mobile self-checkin) --%>
                        <asp:Literal ID="lCheckinQRCodeHtml" runat="server" />

                    </ol>

                    <ol class="checkin-error">
                        <asp:Literal ID="lCheckinLabelErrorMessages" runat="server" Visible="false" />
                    </ol>
                </div>
            </div>
        </div>

        <div class="checkin-footer">
            <div class="checkin-actions">
                <asp:LinkButton CssClass="btn btn-primary" ID="lbDone" runat="server" OnClick="lbDone_Click" Text="Done" />
                <asp:LinkButton CssClass="btn btn-default" ID="lbAnother" runat="server" OnClick="lbAnother_Click" Text="Another Person" />
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
