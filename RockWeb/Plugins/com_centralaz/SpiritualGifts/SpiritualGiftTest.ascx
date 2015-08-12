<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SpiritualGiftTest.ascx.cs" Inherits="Rockweb.Plugins.com_centralaz.SpiritualGifts.SpiritualGiftTest" ViewStateMode="Enabled" EnableViewState="true" %>

<script type="text/javascript">
    ///<summary>
    /// Fade-in effect for the panel.
    ///</summary>
    function fadePanelIn() {
        $("[id$='upnlContent']").rockFadeIn();
    }

    ///<summary>
    /// Returns true if the test is complete.
    ///</summary>
    function isComplete() {
        var $completedQuestions = $('input[type=radio]:checked');
        if ($completedQuestions.length < 30) {
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
    function moveToNextQuestion() {
        $('input[type=radio]').change(function () {
            var selectedId = this.id;
            var selectedIdx = this.id.charAt(this.id.length - 1);
            var rowIdx = this.id.charAt(this.id.length - 3);

            $('[id$="divError"]').fadeOut();

            var $nextPanel = $(this).closest(".gift-test").next();
            if ($nextPanel.length > 0) {
                $("body").animate({ scrollTop: $nextPanel.offset().top - 20 }, 400);
            }
        });
    }

    ///<summary>
    /// Standard .Net handler that's called when the page is loaded (including Ajax).
    ///</summary>
    function pageLoad(sender, args) {
        moveToNextQuestion();
    }

    $(document).ready(function () {
        fadePanelIn();
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(fadePanelIn);
    });
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbError" runat="server" Visible="false" NotificationBoxType="Danger">You have to be signed in to take the test.</Rock:NotificationBox>

        <asp:Panel ID="pnlInstructions" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title margin-t-sm"><i class="fa fa-bar-chart"></i>Spiritual Gift Test</h1>

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
                <h1 class="panel-title margin-t-sm"><i class="fa fa-bar-chart"></i>Spiritual Gift Test</h1>

            </div>
            <div class="panel-body">
                <asp:Repeater ID="rQuestions" runat="server" OnItemDataBound="rQuestions_ItemDataBound">
                    <ItemTemplate>
                        <asp:Panel ID="pnlQuestion" CssClass="panel panel-block gift-test" runat="server">
                            <div class="panel-heading">
                                <h1 class="panel-title margin-t-sm">Question <%# Container.ItemIndex + 1 %></h1>
                            </div>
                            <div class="panel panel-body">
                                <asp:HiddenField ID="hfGiftType" runat="server" />

                                <asp:Literal ID="lQuestion" runat="server"></asp:Literal></td>

                                <Rock:RockRadioButtonList ID="rblAnswer" runat="server" RepeatDirection="Horizontal">
                                    <asp:ListItem Text="Never" Value="0" />
                                    <asp:ListItem Text="Rarely" Value="0" />
                                    <asp:ListItem Text="Sometimes" Value="1" />
                                    <asp:ListItem Text="Often" Value="3" />
                                    <asp:ListItem Text="Almost Always" Value="5" />
                                </Rock:RockRadioButtonList>
                            </div>
                        </asp:Panel>

                    </ItemTemplate>
                </asp:Repeater>
                <div style="display: none" class="alert alert-danger" id="divError">
                    Please answer all questions before scoring.
                </div>
                <div class="actions clearfix">
                    <asp:LinkButton ID="btnScoreTest" runat="server" CssClass="btn btn-primary pull-right" OnClick="btnScoreTest_Click" OnClientClick="if (!isComplete()) { return false; }"><i class="fa fa-check-circle-o"></i> Score</asp:LinkButton>
                </div>

                <div class="disc-attribution">
                    <small>Spiritual Gifts test courtesy of Jackson Snyder at <a href="http://positivepublications.com/">positivepublications.com/</a>.</small>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlResults" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title margin-t-sm"><i class="fa fa-bar-chart"></i>DISC Assessment</h1>
            </div>
            <div class="panel-body">

                <asp:Literal ID="lPrintTip" runat="server" Text="<div class='alert alert-success' role='alert'><strong>Tip!</strong> Consider printing this page out for future reference.</div>" Visible="false"></asp:Literal>
                <asp:Literal ID="lHeading" runat="server"></asp:Literal>

                <ul class="discchart" style="text-align: center">
                    <li style="height: 100%; width: 0px;"></li>
                    <li id="giftScore_Prophecy" runat="server" class="discbar" >
                        <div class="discbar-label" style="text-transform: initial; font-size: 19px">Prophecy</div>
                    </li>
                    <li id="giftScore_Ministry" runat="server" class="discbar" >
                        <div class="discbar-label" style="text-transform: initial; font-size: 19px">Ministry</div>
                    </li>
                    <li id="giftScore_Teaching" runat="server" class="discbar" >
                        <div class="discbar-label" style="text-transform: initial; font-size: 19px">Teaching</div>
                    </li>
                    <li id="giftScore_Encouragement" runat="server" class="discbar" >
                        <div class="discbar-label" style="text-transform: initial; font-size: 19px">Encouragement</div>
                    </li>
                    <li id="giftScore_Giving" runat="server" class="discbar">
                        <div class="discbar-label" style="text-transform: initial; font-size: 19px">Giving</div>
                    </li>
                    <li id="giftScore_Leadership" runat="server" class="discbar" >
                        <div class="discbar-label" style="text-transform: initial; font-size: 19px">Leadership</div>
                    </li>
                    <li id="giftScore_Mercy" runat="server" class="discbar" >
                        <div class="discbar-label" style="text-transform: initial; font-size: 19px">Mercy</div>
                    </li>
                </ul>

                <h3>Description</h3>
                <asp:Literal ID="lDescription" runat="server"></asp:Literal>

                <div class="actions margin-t-lg">
                    <asp:Button ID="btnRetakeTest" runat="server" Visible="false" Text="Retake Test" CssClass="btn btn-default" OnClick="btnRetakeTest_Click" />
                </div>

                <div class="disc-attribution">
                    <small>Spiritual Gifts test courtesy of Jackson Snyder at <a href="http://positivepublications.com/">positivepublications.com/</a>.</small>
                </div>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
