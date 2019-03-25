<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReprintLabelButton.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.Checkin.ReprintLabelButton" %>

<asp:UpdatePanel runat="server" ID="upContent">
    <ContentTemplate>

        <asp:LinkButton runat="server" ID="btnReprint" CssClass="btn btn-default btn-large btn-block btn-checkin-select" Text="Reprint Label" OnClick="btnReprint_Click"></asp:LinkButton>
    </ContentTemplate>
</asp:UpdatePanel>

<script>

    function startReprintListener() {
        var mutationObserver = new MutationObserver(function (mutations) {
            mutations.forEach(function (mutation) {
                for (var i = 0; i < mutation.addedNodes.length; i++) {
                    if ($(".controls.kioskmanager-actions.checkin-actions").length && $("[id$='btnReprint']").is(':hidden')) {
                        $("[id$='btnReprint']").show();
                    }

                    if (!$(".controls.kioskmanager-actions.checkin-actions").length && $("[id$='btnReprint']").is(':visible')) {
                        $("[id$='btnReprint']").hide();
                    }

                    if ($(".controls.kioskmanager-actions.checkin-actions").children("[id$='btnReprint']").length == 0) {
                        $("[id$='btnReprint']").prependTo($(".controls.kioskmanager-actions.checkin-actions"));
                    }
                }
            });
        }); mutationObserver.observe(document.documentElement, {
            childList: true, subtree: true
        });
    }

    $(document).ready(function () {
        startReprintListener();
    });

</script>
