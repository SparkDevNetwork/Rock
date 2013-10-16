<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Search.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Attended.Search" %>

<script>

    function SetKeyEvents() {
        $('.tenkey a.digit').unbind('click').click(function () {
            $name = $("input[id$='tbSearchBox']");
            $name.val($name.val() + $(this).html());
        });
        $('.tenkey a.back').unbind('click').click(function () {
            $name = $("input[id$='tbSearchBox']");
            $name.val($name.val().slice(0, -1));
        });
        $('.tenkey a.clear').unbind('click').click(function () {
            $name = $("input[id$='tbSearchBox']");
            $name.val('');
        });
    };

    $(document).ready(function () { SetKeyEvents(); });
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(SetKeyEvents);

</script>

<asp:UpdatePanel ID="upContent" runat="server" UpdateMode="Conditional">
<ContentTemplate>
    
    <asp:PlaceHolder ID="phScript" runat="server"></asp:PlaceHolder>
    <Rock:ModalAlert ID="maWarning" runat="server" />

    <asp:Panel ID="pnlSearch" runat="server" DefaultButton="lbSearch" CssClass="attended">
            
        <div class="row checkin-header ">
            <div class="col-sm-3 checkin-actions">
                <asp:LinkButton ID="lbAdmin" runat="server" CssClass="btn btn-lg btn-primary" OnClick="lbAdmin_Click" Text="Admin" />
                <asp:LinkButton ID="lbBack" runat="server" CssClass="btn btn-lg btn-primary" OnClick="lbBack_Click" Text="Back" Visible="false" />
            </div>
            <div class="col-sm-6">
                <h1>Search</h1>
            </div>
            <div class="col-sm-3 checkin-actions">
                <asp:LinkButton ID="lbSearch" runat="server" CssClass="btn btn-lg btn-primary" OnClick="lbSearch_Click" Text="Search" />
            </div>
        </div>
            
        <div class="row checkin-body">
            <div class="col-md-3"></div>
            <div class="col-md-6">
            <Rock:RockTextBox ID="tbSearchBox" MaxLength="50" CssClass="checkin-phone-entry" runat="server" Label="" TabIndex="0" />
            </div>
            <div class="col-md-3"></div>
        </div>
    </asp:Panel>
</ContentTemplate>
</asp:UpdatePanel>