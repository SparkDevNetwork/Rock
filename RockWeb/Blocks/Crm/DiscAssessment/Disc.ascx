<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Disc.ascx.cs" Inherits="Rockweb.Blocks.Crm.Disc" ViewStateMode="Enabled" EnableViewState="true" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox id="nbError" runat="server" Visible="false" NotificationBoxType="Danger">You have to be signed in to take the assessment.</Rock:NotificationBox>

        <asp:Panel ID="pnlInstructions" runat="server">
            <asp:Literal ID="lInstructions" runat="server"></asp:Literal>

            <div class="actions">
                <asp:Button ID="btnStart" Text="Start" runat="server" CssClass="btn btn-primary" OnClick="btnStart_Click" />
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlQuestions" runat="server">
            <div class="container">
                <asp:Repeater ID="rQuestions" runat="server" OnItemDataBound="rQuestions_ItemDataBound">
                    <ItemTemplate>
                        <div class="js-disc-questions panel panel-default">
                            <div class="panel-heading">
                                Question <%# Container.ItemIndex + 1 %>
                            </div>
                            <div class="panel-body">
                                <div class="row">
                                    <div class="col-md-1">
                                        MORE
                                    </div>
                                    <div class="col-md-11">
                                        LESS
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-1">
                                        <Rock:RockRadioButtonList ID="rblMore" runat="server" />
                                    </div>
                                    <div class="col-md-11">
                                        <Rock:RockRadioButtonList ID="rblLess" runat="server" />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
                <div style="display: none" class="alert alert-danger" id="divError">
                    Please answer all questions before scoring.
                </div>
                <div class="actions">
                    <asp:Button ID="btnScoreTest" Text="Score Test" runat="server" CssClass="btn btn-primary" OnClick="btnScoreTest_Click" OnClientClick="if (!isComplete()) { return false; }" />
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlResults" runat="server">

            <style>
            .disc-chart {
                clear: both;
                padding: 0;
                width: 50%;
                height: 425px;
                margin: 0 auto;
            }

            .disc-chart li {
                display: inline-block;
                height: 425px;
                margin: 0 10px 0 0;
                width: 20%;
                padding: 0;
                position: relative;
                text-align: center;
                vertical-align: bottom;
                -moz-border-radius: 4px 4px 0 0;
                -webkit-border-radius: 4px;
                border-radius: 4px 4px 0 0;
                background: #e1e1e8;
            }

            .disc-chart .discbar-value {
                letter-spacing: -3px;
                opacity: .4;
                width: 100%;
                font-size: 30px;
                position: absolute;
            }

            .disc-chart .discbar-label {
                font-weight: 500;
                opacity: .5;
                overflow: hidden;
                text-transform: uppercase;
                width: 100%;
                font-size: 28px;
                bottom: 20px;
                position: absolute;
            }

            .disc-chart .discbar-d, .disc-chart .discbar-i, .disc-chart .discbar-s, .disc-chart .discbar-c {
                border: 1px solid #ddd;
            }
            .disc-chart .discbar-d:before, .disc-chart .discbar-i:before, .disc-chart .discbar-s:before, .disc-chart .discbar-c:before {
                position: absolute;
                left: 0;
                right: 0;
                top: 5%;
                letter-spacing: -3px;
                opacity: .4;
                font-size: 20px;
                padding: 5px 1em 0;
                display: block;
                text-align: center;
                content: attr(title);
                word-wrap: break-word;
            }

             .disc-chart .discbar-primary {
                background: #ccc;
            }
            </style>

            <p>Here are the "natural behavior" scores according to the DISC assessment:</p>

            <ul class="disc-chart">
                <li style="height: 100%; width:0px;"></li>
                <li id="discNaturalScore_D" runat="server" class="discbar-d">
                    <div class="discbar-label">D</div>
                </li>
                <li id="discNaturalScore_I" runat="server" class="discbar-i">
                    <div class="discbar-label">I</div>
                </li>
                <li id="discNaturalScore_S" runat="server" class="discbar-s">
                    <div class="discbar-label">S</div>
                </li>
                <li id="discNaturalScore_C" runat="server" class="discbar-c">
                    <div class="discbar-label">C</div>
                </li>
            </ul>

            <h3>Description</h3>

            <h3>Strengths</h3>
            <asp:Literal ID="lStrength" runat="server"></asp:Literal>

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

            <div class="actions">
                <asp:Button ID="btnRetakeTest" runat="server" Visible="false" Text="Retake Test" CssClass="btn btn-default" OnClick="btnRetakeTest_Click" />
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>