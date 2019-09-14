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
            var $nextPanel = $pRow.closest(".disc-assessment").next(".disc-assessment");
            if ($nextPanel.length > 0) {
                $("html, body").animate({ scrollTop: $nextPanel.offset().top - 20 }, 400);
            }
        }
    }

    ///<summary>
    /// Returns true if the test is complete.
    ///</summary>
    function isComplete() {
        var $completedQuestions = $('.js-disc-questions input[type=radio]:checked');
        var totalQuestions = $('.js-disc-questions').length;
        if ($completedQuestions.length < totalQuestions * 2) {
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

            $(this).closest(".radio").removeClass('deselected');
            // uncheck each of the same type except the one that was just checked
            $sameType.each(function (index, element) {
                if (!$(this).is("#" + selectedId)) {
                    $(element).removeAttr('checked').closest('.radio').addClass('deselected');
                }
            });

            // find the oppositeType
            // when this name is of the form: name="ctl00$main$ctl23$bid_568$ctl11$ctl00$rQuestions$ctl00$rblMore1"
            var oppositeType = (answerType == "rblMore") ? "_rblLess" : "_rblMore";

            // if row 1 More was selected, the selector will be like: input[type=radio][id$="_rblLess1_0"]
            var selector = 'input[type=radio][id$="' + oppositeType + rowIdx + '_' + selectedIdx + '"]';

            var oppositeSelector = 'input[type=radio][id*="' + oppositeType + '"]';
            var $oppositeType = $parentTable.find(oppositeSelector);

            // uncheck the corresponding inverse variable
            var $correspondingItem = $parentTable.find(selector);
            if ($correspondingItem) {
                if($correspondingItem.is(':checked')) {
                    $oppositeType.each(function (index, element) {
                        if (!$(this).is("#" + selectedId)) {
                            console.log($(element).closest('.radio'));
                            $(element).closest('.radio').removeClass('deselected');
                        }
                    });
                    $correspondingItem.closest('.radio').siblings().removeClass('deselected');
                    $correspondingItem.removeAttr('checked').closest('.radio').addClass('deselected');
                }
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

    function scrollToTop() {
        window.scrollTo(0, 0);
    }

    $(document).ready(function () {
        fadePanelIn();
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(fadePanelIn);
    });
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlAssessment" CssClass="panel panel-block assessment" runat="server" meta:resourcekey="pnlAssessmentResource1">
            <div class="panel-heading">
                <h1 class="panel-title">
                 <i runat="server" id="iIcon"></i>
                    <asp:Literal ID="lTitle" runat="server" meta:resourcekey="lTitleResource1" />
                </h1>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbError" runat="server" Visible="False" NotificationBoxType="Danger" Dismissable="False" meta:resourcekey="nbErrorResource1" ></Rock:NotificationBox>
                <asp:HiddenField ID="hfAssessmentId" runat="server" />

                <%-- Instructions --%>
                <asp:Panel ID="pnlInstructions" runat="server" meta:resourcekey="pnlInstructionsResource1">
                    <Rock:RockDropDownList ID="ulLanguages" runat="server" class=" nav nav-actions action action-extended dropdown-menu-right" visible="false" AutoPostBack="True" OnSelectedIndexChanged="ulLanguages_SelectedIndexChanged">
                        <asp:ListItem Value="en-US">English</asp:ListItem>
                        <asp:ListItem Value="es">Español</asp:ListItem>
                    </Rock:RockDropDownList>
            
                    <asp:Literal ID="lInstructions" runat="server" meta:resourcekey="lInstructionsResource2"></asp:Literal>

                    <div class="actions">
                        <asp:LinkButton ID="btnStart" runat="server" CssClass="btn btn-primary pull-right" OnClick="btnStart_Click" meta:resourcekey="btnStartResource1"><i class="fa fa-chevron-right"></i></asp:LinkButton>
                    </div>
                </asp:Panel>

                <%-- Questions --%>
                <asp:Panel ID="pnlQuestion" runat="server" meta:resourcekey="pnlQuestionResource1">
                    <asp:HiddenField ID="hfPageNo" runat="server" />
                    <Rock:NotificationBox ID="nbInfo" runat="server" NotificationBoxType="Info" Dismissable="False" meta:resourcekey="nbInfoResource1" ></Rock:NotificationBox>

                    <div class="progress">
                        <div class="progress-bar" role="progressbar" aria-valuenow="<%=this.PercentComplete%>" aria-valuemin="0" aria-valuemax="100" style="min-width: 2em; width: <%=this.PercentComplete%>%;">
                            <%=this.PercentComplete.ToString("F0") %>%
                        </div>
                    </div>

                    <asp:Repeater ID="rQuestions" runat="server" OnItemDataBound="rQuestions_ItemDataBound">
                        <ItemTemplate>
                            <div class="disc-row">
                                <table class="table table-striped table-hover assessment disc-assessment js-disc-questions">
                                    <thead>
                                        <tr>
                                            <th class="disc-question"><asp:HiddenField ID="hfQuestionCode" runat="server" Value='<%# Eval( "QuestionNumber") %>' /></th>
                                            <th class="disc-answer grid-select-field disc-more"><asp:Label runat="server" meta:resourcekey="rblMoreHeader"></asp:Label></th>
                                            <th class="disc-answer grid-select-field disc-less"><asp:Label runat="server" meta:resourcekey="rblLessHeader"></asp:Label></th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <tr>
                                            <td class="disc-question">
                                                <asp:Literal ID="lQuestion1" runat="server"></asp:Literal></td>
                                            <td class="disc-answer grid-select-field disc-more">
                                                <Rock:RockRadioButtonList ID="rblMore1" runat="server" /></td>
                                            <td class="disc-answer grid-select-field disc-less">
                                                <Rock:RockRadioButtonList ID="rblLess1" runat="server" /></td>
                                        </tr>
                                        <tr>
                                            <td class="disc-question">
                                                <asp:Literal ID="lQuestion2" runat="server"></asp:Literal></td>
                                            <td class="disc-answer grid-select-field disc-more">
                                                <Rock:RockRadioButtonList ID="rblMore2" runat="server" /></td>
                                            <td class="disc-answer grid-select-field disc-less">
                                                <Rock:RockRadioButtonList ID="rblLess2" runat="server" /></td>
                                        </tr>
                                        <tr>
                                            <td class="disc-question">
                                                <asp:Literal ID="lQuestion3" runat="server"></asp:Literal></td>
                                            <td class="disc-answer grid-select-field disc-more">
                                                <Rock:RockRadioButtonList ID="rblMore3" runat="server" /></td>
                                            <td class="disc-answer grid-select-field disc-less">
                                                <Rock:RockRadioButtonList ID="rblLess3" runat="server" /></td>
                                        </tr>
                                        <tr>
                                            <td class="disc-question">
                                                <asp:Literal ID="lQuestion4" runat="server"></asp:Literal></td>
                                            <td class="disc-answer grid-select-field disc-more">
                                                <Rock:RockRadioButtonList ID="rblMore4" runat="server" /></td>
                                            <td class="disc-answer grid-select-field disc-less">
                                                <Rock:RockRadioButtonList ID="rblLess4" runat="server" /></td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                    <div style="display: none" class="alert alert-danger" id="divError">
                        <asp:Label runat="server" meta:resourcekey="divErrorResource1" ></asp:Label>
                    </div>
                    <div class="actions clearfix">
                        <asp:LinkButton ID="btnPrevious" runat="server" AccessKey="p" ToolTip="Alt+p" CssClass="btn btn-default js-wizard-navigation" CausesValidation="False" OnClick="btnPrevious_Click" OnClientClick="scrollToTop();" meta:resourcekey="btnPreviousResource1" />
                        <asp:LinkButton ID ="btnNext" runat="server" AccessKey="n" OnClientClick="if (!isComplete()) { return false; } scrollToTop();" DataLoadingText="Next" CssClass="btn btn-primary pull-right js-wizard-navigation" OnClick="btnNext_Click" meta:resourcekey="btnNextResource1" />
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlResult" runat="server" meta:resourcekey="pnlResultResource1">
                    <asp:Literal ID="lPrintTip" runat="server"  Visible="False" meta:resourcekey="lPrintTipResource1"></asp:Literal>
                    <asp:Literal ID="lHeading" runat="server" meta:resourcekey="lHeadingResource1"></asp:Literal>

                    <ul class="discchart">
                        <li class="discchart-midpoint"></li>
                        <li style="height: 100%; width: 0px;"></li>
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

                    <h3><asp:Label ID="lDescriptionHead" meta:resourcekey="lDescriptionHeadResource1" runat="server"></asp:Label></h3>
                    <asp:Literal ID="lDescription" runat="server" ></asp:Literal>

                    <h3><asp:Label ID="lStrengthsHead" meta:resourcekey="lStrengthsHeadResource1" runat="server"></asp:Label></h3>
                    <asp:Literal ID="lStrengths" runat="server"></asp:Literal>

                    <h3><asp:Label ID="lChallengesHead" meta:resourcekey="lChallengesHeadResource1" runat="server"></asp:Label></h3>
                    <asp:Literal ID="lChallenges" runat="server" ></asp:Literal>

                    <div class="actions margin-t-lg margin-b-lg">
                        <asp:Button ID="btnRetakeTest" runat="server" Visible="False" CssClass="btn btn-default" OnClick="btnRetakeTest_Click" meta:resourcekey="btnRetakeTestResource1" />
                    </div>

                </asp:Panel>

            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>