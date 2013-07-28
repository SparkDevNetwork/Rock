<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Admin.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Attended.Admin" %>

<script type="text/javascript">
    function setControlEvents() {
        //$('.btn-checkin-select').click(function () {
        $('.btn-checkin-select').unbind('click').on('click', function () {
            $(this).toggleClass('active');
            var selectedIds = $('#hfParentTypes').val();
            if ($('#hfParentTypes').val().indexOf(this.getAttribute('data-id') + ',') >= 0) {
                $('#hfParentTypes').val($('#hfParentTypes').val().replace(this.getAttribute('data-id') + ',', ''));
            }
            else {
                $('#hfParentTypes').val(selectedIds + this.getAttribute('data-id') + ',');
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
    <asp:HiddenField ID="hfKiosk" runat="server" />
    <asp:HiddenField ID="hfParentTypes" runat="server" />    
    <span style="display:none">
        <asp:LinkButton ID="lbRefresh" runat="server" OnClick="lbRefresh_Click"></asp:LinkButton>
    </span>
    <Rock:ModalAlert ID="maWarning" runat="server" />

    <div class="row-fluid attended-checkin-header">
        <div class="span3 attended-checkin-actions"></div>
        <div class="span6">
            <h1>Admin</h1>
        </div>
        <div class="span3 attended-checkin-actions">
            <asp:LinkButton ID="lbOk" runat="server" CssClass="btn btn-large last btn-primary" OnClick="lbOk_Click" Text="Ok"></asp:LinkButton>
        </div>
    </div>

    <div class="row-fluid checkin-admin-body">
        <div class="span4"></div>
        <div class="span4">
            <label class="control-label">Ministry</label>
            <asp:Repeater ID="repMinistry" runat="server" OnItemDataBound="repMinistry_ItemDataBound">
                <ItemTemplate>
                    <asp:Button ID="lbMinistry" runat="server" data-id='<%# Eval("Id") %>' CommandArgument='<%# Eval("Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select" Text='<%# Eval("Name") %>' />                
                </ItemTemplate>
            </asp:Repeater>
        </div>
        <div class="span4"></div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>
