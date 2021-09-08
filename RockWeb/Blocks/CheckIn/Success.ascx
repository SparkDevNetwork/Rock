<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Success.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Success" %>

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
                        <asp:Literal ID="lMessages" runat="server" Visible="false" />
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
