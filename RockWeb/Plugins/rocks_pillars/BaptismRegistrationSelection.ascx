<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BaptismRegistrationSelection.ascx.cs" Inherits="RockWeb.Plugins.rocks_pillars.Event.BaptismRegistrationSelection" Debug="True" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $('div.js-family-select').click(function () {

            var $familySelectDiv = $(this);

            // get the postback url
            var postbackUrl = $familySelectDiv.attr('data-target');

            // remove the postbackUrls from the other div to prevent multiple clicks
            $('div.js-family-select').attr('data-target', '');

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
    });
</script>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="panel panel-default list-as-blocks clearfix">
            <div class="panel-body" style="padding: 25px">
                <Rock:BootstrapButton ID="bbtnStart" runat="server" CssClass="btn btn-default" Visible="true" OnClick="bbtnStart_Click"><i class="fa fa-chevron-left"></i>Start Over</Rock:BootstrapButton>
                <asp:Label ID="lWarning" runat="server" CssClass="alert alert-warning" Visible="false"></asp:Label>
                <asp:Panel ID="pnlSelectBapType" runat="server" Visible="false" CssClass="margin-t-md">
                    <asp:Literal ID="lSelectBapType" runat="server"></asp:Literal>
                    <div class="row">
                        <asp:Repeater ID="rBapTypeSelection" runat="server" OnItemDataBound="rBapTypeSelection_ItemDataBound">
                            <ItemTemplate>
                                <%-- pnlSelectBapTypePostback will take care of firing the postback, and lSelectBapTypeHtml will be the button HTML from Lava  --%>
                                <asp:Panel ID="pnlSelectBapTypePostback" runat="server" CssClass="col-sm-6 js-family-select">
                                    <asp:Literal ID="lSelectBapTypeHtml" runat="server" />
                                </asp:Panel>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlCampusSelection" runat="server" Visible="false" CssClass="margin-t-md">
                    <asp:Literal ID="lCampusSelection" runat="server"></asp:Literal>
                    <div class="row">
                        <asp:Repeater ID="rCampusSelection" runat="server" OnItemDataBound="rCampusSelection_ItemDataBound">
                            <ItemTemplate>
                                <%-- pnlCampusSelectionPostback will take care of firing the postback, and lCampusSelectionHtml will be the button HTML from Lava  --%>
                                <asp:Panel ID="pnlCampusSelectionPostback" runat="server" CssClass="col-sm-4 js-family-select">
                                    <asp:Literal ID="lCampusSelectionHtml" runat="server" />
                                </asp:Panel>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlDateSelection" runat="server" Visible="false" CssClass="margin-t-md">
                    <asp:Literal ID="lDateSelection" runat="server"></asp:Literal>
                    <div class="row">
                        <asp:Repeater ID="rDateSelection" runat="server" OnItemDataBound="rDateSelection_ItemDataBound">
                            <ItemTemplate>
                                <%-- pnlDateSelectionPostback will take care of firing the postback, and lDateSelectionHtml will be the button HTML from Lava  --%>
                                <asp:Panel ID="pnlDateSelectionPostback" runat="server" CssClass="col-sm-4 js-family-select">
                                    <asp:Literal ID="lDateSelectionHtml" runat="server" />
                                </asp:Panel>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlTimeSelection" runat="server" Visible="false" CssClass="margin-t-md">
                    <asp:Literal ID="lTimeSelection" runat="server"></asp:Literal>
                    <div class="row">
                        <asp:Repeater ID="rTimeSelection" runat="server" OnItemDataBound="rTimeSelection_ItemDataBound">
                            <ItemTemplate>
                                <%-- pnlTimeSelectionPostback will take care of firing the postback, and lTimeSelectionHtml will be the button HTML from Lava  --%>
                                <asp:Panel ID="pnlTimeSelectionPostback" runat="server" CssClass="col-sm-4 js-family-select">
                                    <asp:Literal ID="lTimeSelectionHtml" runat="server" />
                                </asp:Panel>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </asp:Panel>
            </div>
        </div>


    </ContentTemplate>
</asp:UpdatePanel>
