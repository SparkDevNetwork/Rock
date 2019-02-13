<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RosterButton.ascx.cs" Inherits="RockWeb.Plugins.com_bemadev.Checkin.RosterButton" %>

<asp:UpdatePanel runat="server" ID="upContent">
    <ContentTemplate>

        <asp:LinkButton runat="server" ID="btnPrintRoster" CssClass="btn btn-default btn-large btn-block btn-checkin-select" Text="Print Roster"></asp:LinkButton>
    </ContentTemplate>
</asp:UpdatePanel>

<script>

    function pageLoad() {
        $("[id$='btnPrintRoster']").hide();
        
        if ($(".controls.kioskmanager-actions.checkin-actions").length) {
            
            $("[id$='btnPrintRoster']").prependTo($(".controls.kioskmanager-actions.checkin-actions"));
            $("[id$='btnPrintRoster']").show();
        }
    };

</script>
