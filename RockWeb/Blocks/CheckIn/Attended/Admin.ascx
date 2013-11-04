<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Admin.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Attended.Admin" %>

<asp:Panel id="pnlContent" runat="server">

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
        <div class="row checkin-header">
            <div class="col-sm-3 checkin-actions">
                <a id="lbRetry" runat="server" class="btn btn-lg btn-primary" visible="false" href="javascript:window.location.href=window.location.href">Retry</a>
            </div>
            <div class="col-sm-6">
                <h1>Admin</h1>
            </div>
            <div class="col-sm-3 checkin-actions">
                <Rock:BootstrapButton ID="lbOk" runat="server" CssClass="btn btn-lg btn-primary" OnClick="lbOk_Click" Text="Ok" EnableViewState="false" />
            </div>
        </div>

        <div class="row checkin-body">
            <div class="col-md-4"></div>
            <div class="col-md-4">
                <h3>Checkin Type(s)</h3>
                <asp:Repeater ID="repMinistry" runat="server" OnItemDataBound="repMinistry_ItemDataBound">
                    <ItemTemplate>
                        <asp:Button ID="lbMinistry" runat="server" data-id='<%# Eval("Id") %>' CommandArgument='<%# Eval("Id") %>' CssClass="btn btn-primary btn-lg btn-block btn-checkin-select" Text='<%# Eval("Name") %>' />
                    </ItemTemplate>
                </asp:Repeater>
            </div>
            <div class="col-md-4"></div>
        </div>

    </asp:Panel>

</asp:Panel>

<script type="text/javascript">
    function setControlEvents() {
        $('.btn-checkin-select').unbind('click').on('click', function () {
            $(this).toggleClass('active');
            var selectedIds = $('#hfParentTypes').val();
            var buttonId = this.getAttribute('data-id') + ',';
            if (typeof selectedIds == "string" && (selectedIds.indexOf(buttonId) >= 0)) {
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