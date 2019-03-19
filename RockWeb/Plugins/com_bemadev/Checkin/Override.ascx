<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Override.ascx.cs" Inherits="RockWeb.Plugins.com_bemadev.CheckIn.Override" %>
<style>
    .override-btn {
        position: absolute;
        left: 20px;
        width:72px;
    }

</style>


<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>

        <asp:LinkButton runat="server" ID="btnOverrideCustom" CssClass="kioskmanager-activate override-btn" OnClick="btnOverrideCustom_Click"><i class="fa fa-exchange fa-4x"></i></asp:LinkButton>
        <script>
            function startOverrideListener() {
                var mutationObserver = new MutationObserver(function (mutations) {
                    mutations.forEach(function (mutation) {
                        for (var i = 0; i < mutation.addedNodes.length; i++) {
                            if ($(".js-start-button-container").length && $("[id$='btnOverrideCustom']").is(':hidden')) {
                                $("[id$='btnOverrideCustom']").show();
                            }

                            if (!$(".js-start-button-container").length && $("[id$='btnOverrideCustom']").is(':visible')) {
                                $("[id$='btnOverrideCustom']").hide();
                            }
                        }
                    });
                }); mutationObserver.observe(document.documentElement, {
                    childList: true, subtree: true
                });
            }

            $(document).ready(function () {
                startOverrideListener();
            });

        </script>
    </ContentTemplate>
</asp:UpdatePanel>
