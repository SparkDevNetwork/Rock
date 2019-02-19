<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReprintLabelButton.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.Checkin.ReprintLabelButton" %>

<asp:UpdatePanel runat="server" ID="upContent">
    <ContentTemplate>

        <asp:LinkButton runat="server" ID="btnReprint" CssClass="btn btn-default btn-large btn-block btn-checkin-select" Text="Reprint Label" OnClick="btnReprint_Click"></asp:LinkButton>
    </ContentTemplate>
</asp:UpdatePanel>

<script>

    function pageLoad() {
        $("[id$='btnReprint']").hide();
        
        if ($(".controls.kioskmanager-actions.checkin-actions").length) {
            
            $("[id$='btnReprint']").prependTo($(".controls.kioskmanager-actions.checkin-actions"));
            $("[id$='btnReprint']").show();
        }
    };

</script>
