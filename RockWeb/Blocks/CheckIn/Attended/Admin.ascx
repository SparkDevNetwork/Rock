<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Admin.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Attended.Admin" %>

<script type="text/javascript">
    function setControlEvents() {
        $('.btn-checkin-select').unbind('click').on('click', function () {
            $(this).toggleClass('active');
            var selectedIds = $('#hfParentTypes').val();
            var buttonId = this.getAttribute('data-id') + ',';
            if (typeof selectedIds == "string" && (selectedIds.indexOf(buttonId) >= 0) ) {
                $('#hfParentTypes').val(selectedIds.replace(buttonId, ''));
            } else {
                $('#hfParentTypes').val(buttonId + selectedIds);     
            }
            return false;
        });
    };
    $(document).ready(function () { setControlEvents(); });
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(setControlEvents);

</script>

<asp:UpdatePanel ID="upContent" runat="server" UpdateMode="Conditional">
<ContentTemplate>

    <asp:PlaceHolder ID="phScript" runat="server"></asp:PlaceHolder>
    <asp:HiddenField ID="hfLatitude" runat="server" />
    <asp:HiddenField ID="hfLongitude" runat="server" />
    <asp:HiddenField ID="hfKiosk" runat="server" />
    <asp:HiddenField ID="hfGroupTypes" runat="server" />
    <asp:HiddenField ID="hfParentTypes" runat="server" Value="" ClientIDMode="static"/>
    <span style="display: none">
        <asp:LinkButton ID="lbRefresh" runat="server" OnClick="lbRefresh_Click"></asp:LinkButton>
        <asp:LinkButton ID="lbCheckGeoLocation" runat="server" OnClick="lbCheckGeoLocation_Click"></asp:LinkButton>
    </span>
    <Rock:ModalAlert ID="maWarning" runat="server" />

    <asp:Panel ID="pnlAdmin" runat="server" DefaultButton="lbOk" CssClass="attended">
        <div class="row-fluid checkin-header">
            <div class="span3"></div>
            <div class="span6">
                <h1>Admin</h1>
            </div>
            <div class="span3 checkin-actions">
                <asp:LinkButton ID="lbOk" runat="server" CssClass="btn btn-large btn-primary" OnClick="lbOk_Click" Text="Ok"></asp:LinkButton>
            </div>
        </div>

        <div class="row-fluid checkin-body">
            <div class="span4"></div>
            <div class="span4">
                <h2>Checkin Type(s)</h2>
                <asp:Repeater ID="repMinistry" runat="server" OnItemDataBound="repMinistry_ItemDataBound">
                    <ItemTemplate>
                        <asp:Button ID="lbMinistry" runat="server" data-id='<%# Eval("Id") %>' CommandArgument='<%# Eval("Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select" Text='<%# Eval("Name") %>' />
                    </ItemTemplate>
                </asp:Repeater>
            </div>
            <div class="span4"></div>
        </div>

        <div class="row-fluid checkin-footer">
            <div class="checkin-actions">
                <a id="lbRetry" runat="server" class="btn btn-primary" visible="false" href="javascript:window.location.href=window.location.href">Retry</a>
            </div>
        </div>
    </asp:Panel>

</ContentTemplate>
</asp:UpdatePanel>
