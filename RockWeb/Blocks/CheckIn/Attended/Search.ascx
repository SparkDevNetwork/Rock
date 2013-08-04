<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Search.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Attended.Search" %>

<script>        
    function SetKeyEvents() {
        $(document).keyup(function (event) {
            if (event.which == 13) {
                $("[id$='lbSearch']").click();
                event.preventDefault();
            }
        });
        $('.keyboard a.digit').unbind('click').click(function () {
            $name = $("input[id$='tbSearchBox']");
            $name.val($name.val() + $(this).html());
        });
        $('.keyboard a.back').unbind('click').click(function () {
            $name = $("input[id$='tbSearchBox']");
            $name.val($name.val().slice(0, -1));
        });
        $('.keyboard a.clear').unbind('click').click(function () {
            $name = $("input[id$='tbSearchBox']");
            $name.val('');
        });
        $('.keyboard a.spacebar').unbind('click').click(function () {
            $name = $("input[id$='tbSearchBox']");
            $name.val($name.val() + ' ');
        });        
    };

    $(document).ready(function () { SetKeyEvents(); });
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(SetKeyEvents);

</script>

<asp:UpdatePanel ID="upContent" runat="server" UpdateMode="Conditional">
<ContentTemplate>
    
    <asp:PlaceHolder ID="phScript" runat="server"></asp:PlaceHolder>
    <Rock:ModalAlert ID="maWarning" runat="server" />

    <asp:Panel ID="pnlSearch" runat="server" DefaultButton="lbSearch">
    <div class="row-fluid attended-checkin-header">
        <div class="span3 attended-checkin-actions">
            <asp:LinkButton ID="lbAdmin" runat="server" CssClass="btn btn-large btn-primary" OnClick="lbAdmin_Click" Text="Admin" />
            <asp:LinkButton ID="lbBack" runat="server" CssClass="btn btn-large btn-primary" OnClick="lbBack_Click" Text="Back" Visible="false" />
        </div>
        <div class="span6">
            <h1>Check In</h1>
        </div>
        <div class="span3 attended-checkin-actions">
            <asp:LinkButton ID="lbSearch" runat="server" CssClass="btn btn-large last btn-primary" OnClick="lbSearch_Click" Text="Search" />
        </div>
    </div>

    <div class="row-fluid checkin-body">
        <div class="span12">
            <div class="attended-checkin-search-body">
                <Rock:LabeledTextBox ID="tbSearchBox" MaxLength="50" CssClass="attended-checkin-keyboard-entry" runat="server" LabelText="" TabIndex="0" />

                <div class="keyboard attended-checkin-keyboard">
                    <div class="row-fluid">
                        <a class="btn digit">1</a>
                        <a class="btn digit">2</a>
                        <a class="btn digit">3</a>
                        <a class="btn digit">4</a>
                        <a class="btn digit">5</a>
                        <a class="btn digit">6</a>
                        <a class="btn digit">7</a>
                        <a class="btn digit">8</a>
                        <a class="btn digit">9</a>
                        <a class="btn digit">0</a>
                    </div>
                    <div class="row-fluid">
                        <a class="btn digit">Q</a>
                        <a class="btn digit">W</a>
                        <a class="btn digit">E</a>
                        <a class="btn digit">R</a>
                        <a class="btn digit">T</a>
                        <a class="btn digit">Y</a>
                        <a class="btn digit">U</a>
                        <a class="btn digit">I</a>
                        <a class="btn digit">O</a>
                        <a class="btn digit">P</a>
                    </div>
                    <div class="row-fluid">
                        <a class="btn digit">A</a>
                        <a class="btn digit">S</a>
                        <a class="btn digit">D</a>
                        <a class="btn digit">F</a>
                        <a class="btn digit">G</a>
                        <a class="btn digit">H</a>
                        <a class="btn digit">J</a>
                        <a class="btn digit">K</a>
                        <a class="btn digit">L</a>
                        <a class=""></a>
                    </div>
                    <div class="row-fluid">
                        <a class="btn digit">Z</a>
                        <a class="btn digit">X</a>
                        <a class="btn digit">C</a>
                        <a class="btn digit">V</a>
                        <a class="btn digit">B</a>
                        <a class="btn digit">N</a>
                        <a class="btn digit">M</a>
                        <a class=""></a>
                        <a class=""></a>
                        <a class="btn back"><i class='icon-arrow-left'></i></a>
                    </div>
                    <div class="row-fluid">
                        <a href="#" class="btn spacebar">SPACE</a>
                    </div>
                </div>

                <div class="keypad checkin-phone-keypad">
                    <div class="row-fluid">
                        <a class="btn digit">1</a>
                        <a class="btn digit">2</a>
                        <a class="btn digit">3</a>
                    </div>
                    <div class="row-fluid">
                        <a class="btn digit">4</a>
                        <a class="btn digit">5</a>
                        <a class="btn digit">6</a>
                    </div>
                    <div class="row-fluid">
                        <a class="btn digit">7</a>
                        <a class="btn digit">8</a>
                        <a class="btn digit">9</a>
                    </div>
                    <div class="row-fluid">
                        <a class="btn clear">C</a>
                        <a class="btn digit">0</a>
                        <a class="btn back"><i class='icon-arrow-left'></i></a>
                    </div>
                </div>
            </div>
        </div>
    </div>
    </asp:Panel>
</ContentTemplate>
</asp:UpdatePanel>