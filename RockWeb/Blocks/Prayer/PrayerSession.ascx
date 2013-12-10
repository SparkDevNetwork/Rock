<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PrayerSession.ascx.cs" Inherits="RockWeb.Blocks.Prayer.PrayerSession" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<asp:UpdatePanel ID="upPrayerSession" runat="server" UpdateMode="Always" >
    <ContentTemplate>

         <!-- Start session -->
        <asp:Panel ID="pnlChooseCategories" runat="server">
            <asp:Literal ID="lWelcomeInstructions" runat="server"></asp:Literal>
            <h3>Select one or more categories to begin your prayer session:</h3>
            <Rock:NotificationBox id="nbSelectCategories" runat="server" NotificationBoxType="Danger" Visible="false" Heading="I'm Sorry...">Please select at least one prayer category.</Rock:NotificationBox>
            <Rock:RockCheckBoxList ID="cblCategories" runat="server" RepeatColumns="2"></Rock:RockCheckBoxList>
            <asp:LinkButton ID="lbStart" runat="server" Text="Start" CssClass="btn btn-primary" OnClick="lbStart_Click" />
        </asp:Panel>

        <asp:Panel ID="pnlNoPrayerRequestsMessage" runat="server" Visible="false">
            <Rock:NotificationBox id="nbNoPrayerRequests" runat="server" NotificationBoxType="Info" Heading="No Prayers">There are no active prayer requests at this time.</Rock:NotificationBox>
        </asp:Panel>

        <!-- The detail for each prayer -->
        <asp:Panel ID="pnlPrayer" runat="server" Visible="false">
            <asp:HiddenField ID="hfPrayerIndex" runat="server"/>
            <div class="banner">
                <h1>
                    <asp:Literal ID="lPersonIconHtml" runat="server" />
                    <asp:Literal ID="lTitle" runat="server" />
                </h1>
                <Rock:Badge ID="badgePrayerCountTotal" runat="server" BadgeType="Info" ToolTip="number of prayers offered by the team for this request"></Rock:Badge>
                <Rock:HighlightLabel ID="hlblPrayerCountTotal" runat="server" IconCssClass="fa fa-users" LabelType="Info" Text="team: 0" ToolTip="number of prayers offered by the team for this request" />
                <Rock:HighlightLabel ID="hlblUrgent" runat="server" LabelType="Warning" Text="Urgent" Visible="false" ToolTip="This request has been flagged as urgently needing prayers." />
                <Rock:HighlightLabel ID="hlblCategory" runat="server" LabelType="Type" />
            </div>
            <div class="well" style="min-height:100px">
            <asp:Literal ID="lPrayerText" runat="server" />

            <p id="pPrayerAnswer" runat="server"><span class="label label-success">Update!</span> <asp:Literal ID="lPrayerAnswerText" runat="server"></asp:Literal></p>
            </div>
            <div class="actions">
                <asp:LinkButton ID="lbNext" runat="server" Text="Next <i class='fa fa-chevron-right'></i>" CssClass="btn btn-primary" OnClick="lbNext_Click" />
                <asp:LinkButton ID="lbFlag" runat="server" Text="<i class='fa fa-flag'></i> Flag" CssClass="btn btn-warning" ToolTip="Flag as inappropriate so that an administrator can review the content." CausesValidation="false" OnClick="lbFlag_Click" />
                <asp:LinkButton ID="lbStop" runat="server" Text="Quit" CssClass="btn btn-link" CausesValidation="false" OnClick="lbStop_Click" />
                <p class="text-right"><Rock:HighlightLabel ID="hlblNumber" runat="server" LabelType="Default" Text="# 0" ToolTip="x of y" /></p>
            </div>

            <!-- Comments -->

            <asp:Panel ID="pnlComments" runat="server" DefaultButton="bbtnSaveComment">
                <div class="media">
                    <a class="pull-left" href="#"><asp:Literal ID="lMeIconHtml" runat="server" /></a>
                    <Rock:BootstrapButton ID="bbtnSaveComment" runat="server" CssClass="btn btn-default pull-right"  OnClick="bbtnSaveComment_Click" >Post</Rock:BootstrapButton>
                    <div class="media-body">
                        <Rock:RockTextBox ID="tbComment" runat="server" Placeholder="Write a comment..."></Rock:RockTextBox>
                    </div>
                </div>

                <asp:Repeater ID="rptComments" runat="server">
                    <ItemTemplate>
                        <%-- <Rock:NoteEditor ID="noteEditor" runat="server" Note='<%# Container.DataItem as Rock.Model.Note %>'></Rock:NoteEditor>--%>
                        <div class="media">
                            <a class="pull-left" href="#"><asp:Literal ID="lCommenterIcon" runat="server" /></a>
                            <Rock:BootstrapButton ID="bbtnDeleteComment" CssClass="pull-right" runat="server" Text="<i class='fa fa-trash-o'></i>" ></Rock:BootstrapButton>
                            <div class="media-body">
                                <h5 class="media-heading">
                                    <asp:Literal ID="lCommentBy" runat="server"></asp:Literal>
                                    <small>
                                        <asp:Literal ID="lCommentDate" runat="server"></asp:Literal></small></h5>
                                <asp:Literal ID="lCommentText" runat="server"></asp:Literal>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </asp:Panel>
        </asp:Panel>

        <!-- Modal for flag confirmation -->
        <Rock:ModalDialog ID="mdFlag" runat="server" Title="Flag as Inappropriate?" ValidationGroup="EntityTypeName" >
            <Content>
                <asp:HiddenField ID="hfIdValue" runat="server" />
                <p>Once a request is flagged a certain number of times it will be temporarily removed and presented to the prayer administrator for review.</p>
            </Content>
        </Rock:ModalDialog>

        <!-- Finished session -->
        <asp:Panel ID="pnlFinished" runat="server" Visible="false">
            <h2>Thanks for Praying!</h2>
            <p>If you'd like, you can start a new prayer session.</p>
            <asp:LinkButton ID="lbStartAgain" runat="server" Text="Start Again" CssClass="btn btn-primary" OnClick="lbStartAgain_Click" />
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
<script>
    // fade-in effect for the panel
    function FadePanelIn() {
        $("[id$='upPrayerSession']").css("display", "none");
        $("[id$='upPrayerSession']").fadeIn(400);
    }
    $(document).ready(function () { FadePanelIn(); });
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(FadePanelIn);

</script>