<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Search.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Attended.Search" %>

<asp:Panel ID="pnlContent" runat="server" >
    
    <asp:PlaceHolder ID="phScript" runat="server"></asp:PlaceHolder>
    <Rock:ModalAlert ID="maWarning" runat="server" />

    <asp:Panel ID="pnlSearch" runat="server" DefaultButton="lbSearch" CssClass="attended">
            
        <div class="row checkin-header">
            <div class="col-sm-3 checkin-actions">
                <Rock:BootstrapButton ID="lbAdmin" runat="server" CssClass="btn btn-lg btn-primary" OnClick="lbAdmin_Click" Text="Admin" EnableViewState="false" />
                <Rock:BootstrapButton ID="lbBack" runat="server" CssClass="btn btn-lg btn-primary" OnClick="lbBack_Click" Text="Back" Visible="false" EnableViewState="false" />
            </div>
            <div class="col-sm-6">
                <h1>Search</h1>
            </div>
            <div class="col-sm-3 checkin-actions">
                <Rock:BootstrapButton ID="lbSearch" runat="server" CssClass="btn btn-lg btn-primary" OnClick="lbSearch_Click" Text="Search" EnableViewState="false" />
            </div>
        </div>
            
        <div class="row checkin-body">
            <div class="col-md-3"></div>
            <div class="col-md-6">
                <Rock:RockTextBox ID="tbSearchBox" MaxLength="50" CssClass="checkin-phone-entry" runat="server" Label="" TabIndex="0" />

                <asp:Panel id="pnlKeyPad" runat="server" Visible="false" CssClass="tenkey">
                    <div>
                        <a href="#" class="btn btn-default btn-lg digit">1</a>
                        <a href="#" class="btn btn-default btn-lg digit">2</a>
                        <a href="#" class="btn btn-default btn-lg digit">3</a>
                    </div>
                    <div>
                        <a href="#" class="btn btn-default btn-lg digit">4</a>
                        <a href="#" class="btn btn-default btn-lg digit">5</a>
                        <a href="#" class="btn btn-default btn-lg digit">6</a>
                    </div>
                    <div>
                        <a href="#" class="btn btn-default btn-lg digit">7</a>
                        <a href="#" class="btn btn-default btn-lg digit">8</a>
                        <a href="#" class="btn btn-default btn-lg digit">9</a>
                    </div>
                    <div>
                        <a href="#" class="btn btn-default btn-lg digit back"><i class='icon-arrow-left'></i></a>
                        <a href="#" class="btn btn-default btn-lg digit">0</a>
                        <a href="#" class="btn btn-default btn-lg digit clear">C</a>
                    </div>
                </asp:Panel>
            </div>
            <div class="col-md-3"></div>
            
        </div>
    </asp:Panel>

</asp:Panel>

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