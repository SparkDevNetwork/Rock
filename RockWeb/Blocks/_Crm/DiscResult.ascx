<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DiscResult.ascx.cs" Inherits="Rockweb.Blocks.Crm.DiscResult" ViewStateMode="Enabled" EnableViewState="true" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox id="nbError" runat="server" Visible="false" NotificationBoxType="Danger">You have to be signed to view a DISC assessment result.</Rock:NotificationBox>

        <asp:Panel ID="pnlResults" runat="server" CssClass="panel panel-block" Visible="false">
            <div class="panel-heading">
                    <h1 class="panel-title margin-t-sm"><i class="fa fa-bar-chart"></i> DISC Assessment for <asp:Literal ID="lPersonName" runat="server" /></h1>
                    <div class="panel-labels">
                        <Rock:HighlightLabel ID="hlAssessmentDate" runat="server" LabelType="Info" />
                    </div>
                </div>
                <div class="panel-body">
                    <asp:Literal ID="lHeading" runat="server"></asp:Literal>
                    <ul class="discchart">
                        <li class="discchart-midpoint"></li>
                        <li style="height: 100%; width:0px;"></li>
                        <li id="discNaturalScore_D" runat="server" class="discbar discbar-d">
                            <div class="discbar-label">D</div>
                        </li>
                        <li id="discNaturalScore_I" runat="server" class="discbar discbar-i">
                            <div class="discbar-label">I</div>
                        </li>
                        <li id="discNaturalScore_S" runat="server" class="discbar discbar-s">
                            <div class="discbar-label">S</div>
                        </li>
                        <li id="discNaturalScore_C" runat="server" class="discbar discbar-c">
                            <div class="discbar-label">C</div>
                        </li>
                    </ul>

                    <h3>Description</h3>
                    <asp:Literal ID="lDescription" runat="server"></asp:Literal>

                    <h3>Strengths</h3>
                    <asp:Literal ID="lStrengths" runat="server"></asp:Literal>

                    <h3>Challenges</h3>
                    <asp:Literal ID="lChallenges" runat="server"></asp:Literal>

                    <h3>Under Pressure</h3>
                    <asp:Literal ID="lUnderPressure" runat="server"></asp:Literal>

                    <h3>Motivation</h3>
                    <asp:Literal ID="lMotivation" runat="server"></asp:Literal>

                    <h3>Team Contribution</h3>
                    <asp:Literal ID="lTeamContribution" runat="server"></asp:Literal>

                    <h3>Leadership Style</h3>
                    <asp:Literal ID="lLeadershipStyle" runat="server"></asp:Literal>

                    <h3>Follower Style</h3>
                    <asp:Literal ID="lFollowerStyle" runat="server"></asp:Literal>

                    <div class="disc-attribution margin-t-lg">
                        <small>DISC assessment courtesy of Dr Gregory Wiens at <a href="http://www.healthygrowingleaders.com">healthygrowingleaders.com</a>.</small>
                    </div>
                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>