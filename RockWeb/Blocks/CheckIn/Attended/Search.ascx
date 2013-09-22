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
            
        <div class="row-fluid checkin-header ">
            <div class="span3 checkin-actions">
                <asp:LinkButton ID="lbAdmin" runat="server" CssClass="btn btn-large btn-primary" OnClick="lbAdmin_Click" Text="Admin" />
                <asp:LinkButton ID="lbBack" runat="server" CssClass="btn btn-large btn-primary" OnClick="lbBack_Click" Text="Back" Visible="false" />
            </div>
            <div class="span6">
                <h1>Search</h1>
            </div>
            <div class="span3 checkin-actions">
                <asp:LinkButton ID="lbSearch" runat="server" CssClass="btn btn-large btn-primary" OnClick="lbSearch_Click" Text="Search" />
            </div>
        </div>
            
        <div class="row-fluid checkin-body keyboard">
            <Rock:LabeledTextBox ID="tbSearchBox" MaxLength="50" CssClass="input" runat="server" LabelText="" TabIndex="0" />            
            <%--<div class="tenkey checkin-phone-keypad">--%>
                <div class="row-fluid">
                    <a href="#" class="btn btn-large digit">1</a>
                    <a href="#" class="btn btn-large digit">2</a>
                    <a href="#" class="btn btn-large digit">3</a>
                </div>
                <div class="row-fluid">
                    <a href="#" class="btn btn-large digit">4</a>
                    <a href="#" class="btn btn-large digit">5</a>
                    <a href="#" class="btn btn-large digit">6</a>
                </div>
                <div class="row-fluid">
                    <a href="#" class="btn btn-large digit">7</a>
                    <a href="#" class="btn btn-large digit">8</a>
                    <a href="#" class="btn btn-large digit">9</a>
                </div>
                <div class="row-fluid">
                    <a href="#" class="btn btn-large back">Del</a>
                    <a href="#" class="btn btn-large digit">0</a>
                    <a href="#" class="btn btn-large clear">Clear</a>
                </div>
            <%--</div>--%>
        </div>
    </asp:Panel>
</ContentTemplate>
</asp:UpdatePanel>