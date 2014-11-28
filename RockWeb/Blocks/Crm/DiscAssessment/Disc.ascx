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
            var $nextPanel = $pRow.closest(".disc-assessment").next();
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
                <asp:LinkButton ID="btnStart" runat="server" CssClass="btn btn-primary pull-right" OnClick="btnStart_Click">Start <i class="fa fa-chevron-right"></i></asp:LinkButton>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlQuestions" runat="server">
                <asp:Repeater ID="rQuestions" runat="server" OnItemDataBound="rQuestions_ItemDataBound">
                    <ItemTemplate>
                        <table class="table table-condensed table-striped table-hover disc-assessment js-disc-questions margin-b-lg">
                            <thead>
                                <tr>
                                    <th class="disc-question">Question <%# Container.ItemIndex + 1 %></th>
                                    <th class="disc-answer disc-more">Most</th>
                                    <th class="disc-answer disc-less">Least</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    <td class="disc-question">
                                        <asp:Literal ID="lQuestion1" runat="server"></asp:Literal></td>
                                    <td class="disc-answer disc-more">
                                        <Rock:RockRadioButtonList ID="rblMore1" runat="server" /></td>
                                    <td class="disc-answer disc-less">
                                        <Rock:RockRadioButtonList ID="rblLess1" runat="server" /></td>
                                </tr>
                                <tr>
                                    <td class="disc-question">
                                        <asp:Literal ID="lQuestion2" runat="server"></asp:Literal></td>
                                    <td class="disc-answer disc-more">
                                        <Rock:RockRadioButtonList ID="rblMore2" runat="server" /></td>
                                    <td class="disc-answer disc-less">
                                        <Rock:RockRadioButtonList ID="rblLess2" runat="server" /></td>
                                </tr>
                                <tr>
                                    <td class="disc-question">
                                        <asp:Literal ID="lQuestion3" runat="server"></asp:Literal></td>
                                    <td class="disc-answer disc-more">
                                        <Rock:RockRadioButtonList ID="rblMore3" runat="server" /></td>
                                    <td class="disc-answer disc-less">
                                        <Rock:RockRadioButtonList ID="rblLess3" runat="server" /></td>
                                </tr>
                                <tr>
                                    <td class="disc-question">
                                        <asp:Literal ID="lQuestion4" runat="server"></asp:Literal></td>
                                    <td class="disc-answer disc-more">
                                        <Rock:RockRadioButtonList ID="rblMore4" runat="server" /></td>
                                    <td class="disc-answer disc-less">
                                        <Rock:RockRadioButtonList ID="rblLess4" runat="server" /></td>
                                </tr>
                            </tbody>
                        </table>

                    </ItemTemplate>
                </asp:Repeater>
                <div style="display: none" class="alert alert-danger" id="divError">
                    Please answer all questions before scoring.
                </div>
                <div class="actions clearfix">
                    <asp:LinkButton ID="btnScoreTest" runat="server" CssClass="btn btn-primary pull-right" OnClick="btnScoreTest_Click" OnClientClick="if (!isComplete()) { return false; }"><i class="fa fa-check-circle-o"></i> Score</asp:LinkButton>
                </div>

                <div>
                    <small>DISC assessment courtesy of Dr Gregory Wiens at <a href="http://www.healthygrowingleaders.com">healthygrowingleaders.com</a>.</small>
                </div>
        </asp:Panel>

        <asp:Panel ID="pnlResults" runat="server">
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