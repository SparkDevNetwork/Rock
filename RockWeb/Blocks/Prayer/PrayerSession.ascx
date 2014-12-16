<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PrayerSession.ascx.cs" Inherits="RockWeb.Blocks.Prayer.PrayerSession" %>

<asp:UpdatePanel ID="upPrayerSession" runat="server" UpdateMode="Always" >
    <ContentTemplate>

         <!-- Start session -->
        <asp:Panel ID="pnlChooseCategories" runat="server">
            <asp:Literal ID="lWelcomeInstructions" runat="server"></asp:Literal>
            <p>Select one or more categories to begin your prayer session:</p>
            <Rock:NotificationBox id="nbSelectCategories" runat="server" NotificationBoxType="Danger" Visible="false" Heading="I'm Sorry...">Please select at least one prayer category.</Rock:NotificationBox>
            <Rock:RockCheckBoxList ID="cblCategories" runat="server" RepeatColumns="2"></Rock:RockCheckBoxList>
            <asp:LinkButton ID="lbStart" runat="server" Text="Start" CssClass="btn btn-primary" OnClick="lbStart_Click" />
        </asp:Panel>

        <asp:Panel ID="pnlNoPrayerRequestsMessage" runat="server" Visible="false">
            <Rock:NotificationBox id="nbNoPrayerRequests" runat="server" NotificationBoxType="Info" Heading="No Prayers">There are no active prayer requests at this time.</Rock:NotificationBox>
        </asp:Panel>

        <!-- The detail for each prayer -->
        
        <asp:Panel ID="pnlPrayer" runat="server" Visible="false">
           
            <div class="clearfix margin-b-md">
                <Rock:HighlightLabel ID="hlblNumber" runat="server" CssClass="pull-right" LabelType="Default" Text="# 0" />
            </div>

            <div class="panel panel-default">
                 <div class="panel-heading clearfix">
                    <h3 class="panel-title pull-left">
                        <asp:Literal ID="lPersonIconHtml" runat="server" />
                        <asp:Literal ID="lTitle" runat="server" />
                    </h3>
                    <div class="panel-labels">
                        <Rock:HighlightLabel ID="hlblPrayerCountTotal" runat="server" IconCssClass="fa fa-users" LabelType="Info" Text="team: 0" ToolTip="The number of prayers offered by the team for this request." />
                        <Rock:HighlightLabel ID="hlblUrgent" runat="server" LabelType="Warning" Text="Urgent" Visible="false" />
                        <Rock:HighlightLabel ID="hlblCategory" runat="server" LabelType="Type" />

                    </div>
                </div>
            
                <div class="panel-body">

                    <asp:HiddenField ID="hfPrayerIndex" runat="server"/>


                        <strong>Prayer Request</strong>
                        <br />
                        <asp:Literal ID="lPrayerText" runat="server" />

                        <div id="divPrayerAnswer" runat="server" class="margin-t-lg">
                            <strong>Update</strong> 
                            <br />
                            <asp:Literal ID="lPrayerAnswerText" runat="server"></asp:Literal>
                        </div>


                    <div class="actions margin-b-md">
                        <asp:LinkButton ID="lbNext" TabIndex="1" runat="server" Text="Next <i class='fa fa-chevron-right'></i>" CssClass="btn btn-primary pull-right" OnClick="lbNext_Click" />
                        <asp:LinkButton ID="lbFlag" runat="server" Text="<i class='fa fa-flag'></i> Flag" CssClass="btn btn-warning" ToolTip="Flag as inappropriate so that an administrator can review the content." CausesValidation="false" OnClick="lbFlag_Click" />
                        <asp:LinkButton ID="lbStop" runat="server" Text="End Session" CssClass="btn btn-link" CausesValidation="false" OnClick="lbStop_Click" />
                    </div>

                    <!-- Comments -->
                    <h4>Comments</h4>
                    <Rock:NoteContainer ID="notesComments" runat="server" Term="Comment" ShowHeading="false"
                         DisplayType="Full" UsePersonIcon="true" ShowAlertCheckBox="false" 
                        ShowPrivateCheckBox="false" ShowSecurityButton="false" 
                        AllowAnonymousEntry="false" AddAlwaysVisible="true" SortDirection="Ascending" />

                </div>
            </div>
           
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
        $("[id$='upPrayerSession']").rockFadeIn();
    }
    $(document).ready(function () { FadePanelIn(); });
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(FadePanelIn);

</script>