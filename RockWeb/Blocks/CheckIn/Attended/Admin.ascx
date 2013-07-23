<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Admin.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Attended.Admin" %>

<script type="text/javascript">
    function setControlEvents() {
        //$('.btn-checkin-select').click(function () {
        //$('.btn-checkin-select').unbind('click').on('click', function () {
        //    $(this).toggleClass('active');
        //    return false;
        //});
    };
    $(document).ready(function () { setControlEvents(); });
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(setControlEvents);

</script>

<asp:UpdatePanel ID="upContent" runat="server" UpdateMode="Conditional">
<ContentTemplate>

    <asp:HiddenField ID="hfKioskId" runat="server" />
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
        <div class="span4">
            <div id="campusDiv" runat="server">
            <h3>Campus</h3>
            <asp:DropDownList ID="ddlCampus" runat="server" OnSelectedIndexChanged="ddlCampus_SelectedIndexChanged" AutoPostBack="true"></asp:DropDownList>
            </div>
        </div>
        <div class="span4">
            <h3>Ministry</h3>
            <asp:Repeater ID="repMinistry" runat="server" OnItemCommand="repMinistry_ItemCommand">
                <ItemTemplate>
                    <asp:LinkButton ID="lbMinistry" runat="server" CommandArgument='<%# Eval("Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select" ><%# Eval("Name") %></asp:LinkButton>
                    <%--<asp:Button ID="lbMinistry" runat="server" CommandArgument='<%# Eval("Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select" Text='<%# Eval("Name") %>' ></asp:Button>--%>
                </ItemTemplate>
            </asp:Repeater>
        </div>
        <div class="span4"></div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>
