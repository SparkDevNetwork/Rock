<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RunSQLOnClick.ascx.cs" Inherits="RockWeb.Plugins.com_bemadev.Finance.RunSQLOnClick" %>

<asp:UpdatePanel ID="upnlRunSQL" runat="server">
    <Triggers>
        <asp:PostBackTrigger ControlID="lbRunSQL" />
    </Triggers>	
    <ContentTemplate>
        <asp:Panel ID="pnlMain" runat="server">
            <asp:LinkButton ID="lbRunSQL" runat="server" CssClass="btn btn-default hidden" CausesValidation="false" OnClick="lbRunSQL_Click"></asp:LinkButton>
        </asp:Panel>

        <script type="text/javascript">
            (function ($) {

                function setup() {
                    var $button = $('#<%= lbRunSQL.ClientID %>');

                    $button.insertBefore('a.btn-default:contains("Match Transactions")').after(' ').removeClass('hidden');
                }

                $(document).ready(function () {
                    setup();
                });

                var prm = Sys.WebForms.PageRequestManager.getInstance();
                prm.add_endRequest(function () {
                    setup();
                });

            })(jQuery);
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
