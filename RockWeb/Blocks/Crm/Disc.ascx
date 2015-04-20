<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Disc.ascx.cs" Inherits="Rockweb.Blocks.Crm.Disc" ViewStateMode="Enabled" EnableViewState="true" %>

<script type="text/javascript">
    ///<summary>
    /// Fade-in effect for the panel.
    ///</summary>
    function fadePanelIn() {
        $("[id$='upnlContent']").rockFadeIn();
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
            // answerType will be rblMore or rblLess
            var answerType = this.name.substr(this.name.length - 8, 7);
            // so the selector will be like: input[type=radio][id*="rblMore"]
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
    /// Standard .Net handler that's called when the page is loaded (including Ajax).
    ///</summary>
    function pageLoad(sender, args) {
        initQuestionValidation();
    }

    $(document).ready(function () {
        fadePanelIn();
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(fadePanelIn);
    });
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox id="nbError" runat="server" Visible="false" NotificationBoxType="Danger">You have to be signed in to take the assessment.</Rock:NotificationBox>

        <asp:Panel ID="pnlInstructions" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title margin-t-sm"><i class="fa fa-bar-chart"></i> DISC Assessment</h1>
                   
             </div>
             <div class="panel-body">
                <asp:Literal ID="lInstructions" runat="server"></asp:Literal>

                <div class="actions">
                    <asp:LinkButton ID="btnStart" runat="server" CssClass="btn btn-primary pull-right" OnClick="btnStart_Click">Start <i class="fa fa-chevron-right"></i></asp:LinkButton>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlQuestions" CssClass="panel panel-block" runat="server">
             <div class="panel-heading">
                <h1 class="panel-title margin-t-sm"><i class="fa fa-bar-chart"></i> DISC Assessment</h1>
                   
             </div>
             <div class="panel-body">
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

                <div class="disc-attribution">
                    <small>DISC assessment courtesy of Dr Gregory Wiens at <a href="http://www.healthygrowingleaders.com">healthygrowingleaders.com</a>.</small>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlResults" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title margin-t-sm"><i class="fa fa-bar-chart"></i> DISC Assessment</h1>
            </div>
            <div class="panel-body">

                <asp:Literal ID="lPrintTip" runat="server" Text="<div class='alert alert-success' role='alert'><strong>Tip!</strong> Consider printing this page out for future reference.</div>" Visible="false"></asp:Literal>
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

                <div class="actions margin-t-lg">
                    <asp:Button ID="btnRetakeTest" runat="server" Visible="false" Text="Retake Test" CssClass="btn btn-default" OnClick="btnRetakeTest_Click" />
                </div>

                <div class="disc-attribution">
                    <small>DISC assessment courtesy of Dr Gregory Wiens at <a href="http://www.healthygrowingleaders.com">healthygrowingleaders.com</a>.</small>
                </div>
        </div>

    </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>