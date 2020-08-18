<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FamilySelect.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.FamilySelect" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $('div.js-family-select').on('click', function () {

            var $familySelectDiv = $(this);

            // get the postback url
            var postbackUrl = $familySelectDiv.attr('data-target');

            // remove the postbackUrls from the other div to prevent multiple clicks
            $familySelectDiv.attr('data-target', '');

            // if the postbackUrl has been cleared, another button has already been pressed, so ignore
            if (!postbackUrl || postbackUrl == '') {
                return;
            }

            // make the btn in the template a bootstrap button to show the loading... message
            var $familySelectBtn = $familySelectDiv.find('a');
            $familySelectBtn.attr('data-loading-text', 'Loading...');
            Rock.controls.bootstrapButton.showLoading($familySelectBtn);

            window.location = 'javascript: ' + postbackUrl;
        });

        if ($('#<%=hfShowEditFamilyPrompt.ClientID%>').val() == "1") {

                Rock.dialogs.confirm('<%=this.ConditionMessage + " Do you want to edit this family?" %>', function (result) {
                    if (result) {
                        window.location = "javascript:__doPostBack('<%=upContent.ClientID %>', 'EditFamily')";
                    }
                });

            }
    });
</script>

<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <Rock:ModalAlert ID="maWarning" runat="server" />
    <asp:HiddenField ID="hfShowEditFamilyPrompt" runat="server" Value="0" />

    <%-- Hidden field to keep track of which family was clicked on in the rSelection repeater --%>
    <asp:HiddenField ID="hfSelectedFamilyGroupId" runat="server" />

    <div class="checkin-header">
        <h1><asp:Literal ID="lTitle" runat="server" /></h1>
    </div>

    <div class="checkin-body">

        <div class="checkin-scroll-panel">
            <div class="scroller">

                <div class="control-group checkin-body-container">
                    <label class="control-label"><asp:Literal ID="lCaption" runat="server" /></label>
                    <asp:LinkButton CssClass="btn btn-link pull-right" ID="lbAddFamily" runat="server" OnClick="lbAddFamily_Click" Text="<i class='fa fa-plus'></i> Add Family"/>
                    <div class="controls">
                        <asp:Repeater ID="rSelection" runat="server" OnItemDataBound="rSelection_ItemDataBound">
                            <ItemTemplate>
                                <%-- pnlSelectFamilyPostback will take care of firing the postback, and lSelectFamilyButtonHtml will be the button HTML from Lava  --%>
                                <asp:Panel ID="pnlSelectFamilyPostback" runat="server"  CssClass="js-family-select">
                                    <asp:Literal ID="lSelectFamilyButtonHtml" runat="server" />
                                </asp:Panel>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </div>

            </div>
        </div>

    </div>


     <div class="checkin-footer">
        <div class="checkin-actions">
            <asp:LinkButton CssClass="btn btn-default btn-back" ID="lbBack" runat="server" OnClick="lbBack_Click" Text="Back" />
            <asp:LinkButton CssClass="btn btn-default btn-cancel" ID="lbCancel" runat="server" OnClick="lbCancel_Click" Text="Cancel" />
        </div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>
