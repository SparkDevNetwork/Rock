<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SpiritualGiftTest.ascx.cs" Inherits="Rockweb.Plugins.com_centralaz.SpiritualGifts.SpiritualGiftTest" ViewStateMode="Enabled" EnableViewState="true" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbError" runat="server" Visible="false" NotificationBoxType="Danger">You must be signed in to take the test.</Rock:NotificationBox>

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
                <asp:HiddenField ID="hfStartIndex" runat="server" />
                <asp:HiddenField ID="hfEndIndex" runat="server" />

                <h2>
                    <asp:Literal ID="lProgress" runat="server" />
                </h2>
                <asp:Panel ID="pnlRegistrantProgressBar" runat="server">
                    <div class="progress">
                        <div class="progress-bar" role="progressbar" aria-valuenow="<%=this.PercentComplete%>" aria-valuemin="0" aria-valuemax="100" style="width: <%=this.PercentComplete%>%;">
                            <span class="sr-only"><%=this.PercentComplete%>% Complete</span>
                        </div>
                    </div>
                </asp:Panel>
                <Rock:NotificationBox ID="nbWarning" runat="server" Visible="false" NotificationBoxType="Danger">Please answer all questions.</Rock:NotificationBox>

                <asp:Repeater ID="rQuestions" runat="server" OnItemDataBound="rQuestions_ItemDataBound">
                    <ItemTemplate>
                        <asp:Panel ID="pnlQuestion" CssClass="panel panel-block gift-test" runat="server">
                            <div class="panel panel-body">
                                <asp:HiddenField ID="hfQuestionIndex" runat="server" />
                                <asp:HiddenField ID="hfQuestionGifting" runat="server" />

                                <asp:Literal ID="lQuestion" runat="server"></asp:Literal></td>

                                <Rock:RockRadioButtonList ID="rblAnswer" runat="server" RepeatDirection="Horizontal">
                                    <asp:ListItem Text="Never" Value="1" />
                                    <asp:ListItem Text="Rarely" Value="2" />
                                    <asp:ListItem Text="Sometimes" Value="5" />
                                    <asp:ListItem Text="Often" Value="7" />
                                    <asp:ListItem Text="Almost Always" Value="9" />
                                </Rock:RockRadioButtonList>
                            </div>
                        </asp:Panel>

                    </ItemTemplate>
                </asp:Repeater>

                <div class="actions">
                    <asp:LinkButton ID="lbPrev" runat="server" AccessKey="p" Text="Previous" CssClass="btn btn-default" CausesValidation="false" OnClick="lbPrev_Click" />
                    <Rock:BootstrapButton ID="lbNext" runat="server" AccessKey="n" Text="Next" DataLoadingText="Next" CssClass="btn btn-primary pull-right" CausesValidation="true" OnClick="lbNext_Click" />
                    <Rock:BootstrapButton ID="lbFinish" runat="server" AccessKey="f" Text="Finish" DataLoadingText="Finish" CssClass="btn btn-primary pull-right" CausesValidation="true" OnClick="lbFinish_Click" Visible="false" />

                </div>

                <div class="disc-attribution">
                    <small>Spiritual Gifts test courtesy of Jackson Snyder at <a href="http://positivepublications.com/">positivepublications.com/</a>.</small>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
