<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RosterButton.ascx.cs" Inherits="RockWeb.Plugins.com_bemadev.Checkin.RosterButton" %>

<asp:UpdatePanel runat="server" ID="upContent">
    <ContentTemplate>

        <asp:LinkButton runat="server" ID="btnPrintRoster" CssClass="btn btn-default btn-large btn-block btn-checkin-select" Text="Print Roster"></asp:LinkButton>
    </ContentTemplate>
</asp:UpdatePanel>

<script>
function startRosterListener() {
        var mutationObserver = new MutationObserver(function (mutations) {
            mutations.forEach(function (mutation) {
                for (var i = 0; i < mutation.addedNodes.length; i++) {
                    if ($(".controls.kioskmanager-actions.checkin-actions").length && $("[id$='btnPrintRoster']").is(':hidden')) {
                        $("[id$='btnPrintRoster']").show();
                    }

                    if (!$(".controls.kioskmanager-actions.checkin-actions").length && $("[id$='btnPrintRoster']").is(':visible')) {
                        $("[id$='btnPrintRoster']").hide();
                    }

                    if ($(".controls.kioskmanager-actions.checkin-actions").children("[id$='btnPrintRoster']").length == 0) {
                        $("[id$='btnPrintRoster']").prependTo($(".controls.kioskmanager-actions.checkin-actions"));
                    }
                }
            });
        }); mutationObserver.observe(document.documentElement, {
            childList: true, subtree: true
        });
    }

    $(document).ready(function () {
        startRosterListener();
    });
</script>
