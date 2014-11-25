<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Disc.ascx.cs" Inherits="Rockweb.Blocks.Crm.Disc" ViewStateMode="Enabled" EnableViewState="true" %>

<script type="text/javascript">
    ///<summary>
    /// Fade-in effect for the panel.
    ///</summary>
    function FadePanelIn() {
        $("[id$='upnlContent']").rockFadeIn();
    }

    ///<summary>
    /// Adjust the given color by a certain amount
    ///</summary>
    function AdjustColor(col, amt) {
        var hash = false;

        if (col[0] == "#") {
            col = col.slice(1);
            hash = true;
        }

        var num = parseInt(col, 16);

        var r = (num >> 16) + amt;
        if (r > 255) r = 255;
        else if (r < 0) r = 0;

        var g = (num & 0x0000FF) + amt;
        if (g > 255) g = 255;
        else if (g < 0) g = 0;

        var b = ((num >> 8) & 0x00FF) + amt;
        if (b > 255) b = 255;
        else if (b < 0) b = 0;

        return (hash ? "#" : "") + (g | (b << 8) | (r << 16)).toString(16);
    }

    ///<summary>
    /// Converts an rbg color value to hex.
    ///</summary>
    function rgb2hex(rgb) {
        rgb = rgb.match(/^rgb\((\d+),\s*(\d+),\s*(\d+)\)$/);
        return "#" +
         ("0" + parseInt(rgb[1], 10).toString(16)).slice(-2) +
         ("0" + parseInt(rgb[2], 10).toString(16)).slice(-2) +
         ("0" + parseInt(rgb[3], 10).toString(16)).slice(-2);
    }

    ///<summary>
    /// Verifies that both a less and a more were selected, then
    /// scrolls to the next item.
    ///</summary>
    function checkAndMoveToNext($pRow, partialSelector) {
        var $checkedItem = $pRow.find('input[type=radio][id*="' + partialSelector + '"]:checked');
        //If both RBLs have a selected item
        if ($checkedItem.length > 0) {
            var $nextPanel = $pRow.closest(".panel-default").next();
            if ($nextPanel.length > 0) {
                $("body").animate({ scrollTop: $nextPanel.offset().top - 20 }, 400);
            }
        }
    }

    ///<summary>
    /// Returns true if the test is complete.
    ///</summary>
    function isComplete() {
        var $completedQuestions = $('.js-disc-questions input[type=radio]:checked');
        if ($completedQuestions.length < 60) {
            $('[id$="divError"]').fadeIn();
            return false;
        }
        else {
            return true;
        }
    }

    ///<summary>
    /// Unchecks the corresponding item from the other rbl
    /// and moves to next question.
    ///</summary>
    function initQuestionValidation() {
        $('.js-disc-questions input[type=radio]').change(function () {
            var selectedId = this.id;
            var selectedIdx = this.id.charAt(this.id.length - 1);
            var rowIdx = this.id.charAt(this.id.length - 3);

            var $parentTable = $(this).closest("table.disc-assessment");

            // find the other items of the same type (More or Less)
            var answerType = this.name.substr(length - 9, 7);
            // so the selector will be like: input[type=radio][id*="_rblMore"]
            var selector = 'input[type=radio][id*="' + answerType + '"]';
            var $sameType = $parentTable.find(selector);

            // uncheck each of the same type except the one that was just checked
            $sameType.each(function (index, element) {
                if (!$(this).is("#" + selectedId)) {
                    $(element).removeAttr('checked');
                }
            });

            // find the oppositeType
            // when this name is of the form: name="ctl00$main$ctl23$bid_568$ctl11$ctl00$rQuestions$ctl00$rblMore1"
            var oppositeType = (answerType == "rblMore") ? "_rblLess" : "_rblMore";

            // if row 1 More was selected, the selector will be like: input[type=radio][id$="_rblLess1_0"]
            var selector = 'input[type=radio][id$="' + oppositeType + rowIdx + '_' + selectedIdx + '"]';

            // uncheck the corresponding inverse variable
            var $correspondingItem = $parentTable.find(selector);
            if ($correspondingItem) {
                $correspondingItem.removeAttr('checked');
            }

            $('[id$="divError"]').fadeOut();

            checkAndMoveToNext($parentTable, oppositeType);
        });
    }

    ///<summary>
    /// Darkens the color of the discbar-primary color to 20% of the label-default
    ///</summary>
    function DarkenPrimaryDiscScore() {
        var bgcolor = $(".label-default").css("background-color");
        if (bgcolor != null) {
            var color = rgb2hex(bgcolor);
            var newColor = AdjustColor(color, -30);
            $(".discbar-primary").css("background-color", newColor);
        }
    }
    ///<summary>
    /// Standard .Net handler that's called when the page is loaded (including Ajax).
    ///</summary>
    function pageLoad(sender, args) {
        initQuestionValidation();
        DarkenPrimaryDiscScore();
    }

    $(document).ready(function () {
        FadePanelIn();
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(FadePanelIn);
    });
</script>

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

                                <table class="table table-condensed table-hover disc-assessment">
                                    <thead>
                                        <tr>
                                            <td></td>
                                            <td>MOST</td>
                                            <td>LEAST</td>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <tr>
                                            <td class="disc-question">
                                                <asp:Literal ID="lQuestion1" runat="server"></asp:Literal></td>
                                            <td>
                                                <Rock:RockRadioButtonList ID="rblMore1" runat="server" /></td>
                                            <td>
                                                <Rock:RockRadioButtonList ID="rblLess1" runat="server" /></td>
                                        </tr>
                                        <tr>
                                            <td class="disc-question">
                                                <asp:Literal ID="lQuestion2" runat="server"></asp:Literal></td>
                                            <td>
                                                <Rock:RockRadioButtonList ID="rblMore2" runat="server" /></td>
                                            <td>
                                                <Rock:RockRadioButtonList ID="rblLess2" runat="server" /></td>
                                        </tr>
                                        <tr>
                                            <td class="disc-question">
                                                <asp:Literal ID="lQuestion3" runat="server"></asp:Literal></td>
                                            <td>
                                                <Rock:RockRadioButtonList ID="rblMore3" runat="server" /></td>
                                            <td>
                                                <Rock:RockRadioButtonList ID="rblLess3" runat="server" /></td>
                                        </tr>
                                        <tr>
                                            <td class="disc-question">
                                                <asp:Literal ID="lQuestion4" runat="server"></asp:Literal></td>
                                            <td>
                                                <Rock:RockRadioButtonList ID="rblMore4" runat="server" /></td>
                                            <td>
                                                <Rock:RockRadioButtonList ID="rblLess4" runat="server" /></td>
                                        </tr>
                                    </tbody>
                                </table>
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
                .disc-question {
                    text-align:right;
                }

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