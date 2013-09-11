<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Search.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Attended.Search" %>

<script>        
    function SetKeyEvents() {
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
        <div class="span3">
            <asp:LinkButton ID="lbAdmin" runat="server" CssClass="btn btn-large btn-primary xl-font" OnClick="lbAdmin_Click" Text="Admin" />
            <asp:LinkButton ID="lbBack" runat="server" CssClass="btn btn-large btn-primary xl-font" OnClick="lbBack_Click" Text="Back" Visible="false" />
        </div>
        <div class="span6">
            <h1 class="xl-font">Check In</h1>
        </div>
        <div class="span3">
            <asp:LinkButton ID="lbSearch" runat="server" CssClass="btn btn-large btn-primary pull-right xl-font" OnClick="lbSearch_Click" Text="Search" />
        </div>
    </div>

    <div class="row-fluid attended-checkin-body">
        <div class="span12">
            <Rock:LabeledTextBox ID="tbSearchBox" MaxLength="50" CssClass="attended-checkin-keyboard-entry" runat="server" LabelText="" TabIndex="0" />

            <div class="keyboard attended-checkin-keyboard">
                <div class="row-fluid">
                    <a href="#" class="btn digit keyboard-font">1</a>
                    <a href="#" class="btn digit keyboard-font">2</a>
                    <a href="#" class="btn digit keyboard-font">3</a>
                    <a href="#" class="btn digit keyboard-font">4</a>
                    <a href="#" class="btn digit keyboard-font">5</a>
                    <a href="#" class="btn digit keyboard-font">6</a>
                    <a href="#" class="btn digit keyboard-font">7</a>
                    <a href="#" class="btn digit keyboard-font">8</a>
                    <a href="#" class="btn digit keyboard-font">9</a>
                    <a href="#" class="btn digit keyboard-font">0</a>
                </div>
                <div class="row-fluid">
                    <a href="#" class="btn digit keyboard-font">Q</a>
                    <a href="#" class="btn digit keyboard-font">W</a>
                    <a href="#" class="btn digit keyboard-font">E</a>
                    <a href="#" class="btn digit keyboard-font">R</a>
                    <a href="#" class="btn digit keyboard-font">T</a>
                    <a href="#" class="btn digit keyboard-font">Y</a>
                    <a href="#" class="btn digit keyboard-font">U</a>
                    <a href="#" class="btn digit keyboard-font">I</a>
                    <a href="#" class="btn digit keyboard-font">O</a>
                    <a href="#" class="btn digit keyboard-font">P</a>
                </div>
                <div class="row-fluid">
                    <a href="#" class="btn digit keyboard-font">A</a>
                    <a href="#" class="btn digit keyboard-font">S</a>
                    <a href="#" class="btn digit keyboard-font">D</a>
                    <a href="#" class="btn digit keyboard-font">F</a>
                    <a href="#" class="btn digit keyboard-font">G</a>
                    <a href="#" class="btn digit keyboard-font">H</a>
                    <a href="#" class="btn digit keyboard-font">J</a>
                    <a href="#" class="btn digit keyboard-font">K</a>
                    <a href="#" class="btn digit keyboard-font">L</a>
                    <a href="#" class=""></a>
                </div>
                <div class="row-fluid">
                    <a href="#" class="btn digit keyboard-font">Z</a>
                    <a href="#" class="btn digit keyboard-font">X</a>
                    <a href="#" class="btn digit keyboard-font">C</a>
                    <a href="#" class="btn digit keyboard-font">V</a>
                    <a href="#" class="btn digit keyboard-font">B</a>
                    <a href="#" class="btn digit keyboard-font">N</a>
                    <a href="#" class="btn digit keyboard-font">M</a>
                    <a href="#" class=""></a>
                    <a href="#" class=""></a>
                    <a href="#" class="btn back keyboard-font"><i class='icon-arrow-left'></i></a>
                </div>
                <div class="row-fluid">
                    <a href="#" class="btn spacebar keyboard-font">SPACE</a>
                </div>
            </div>

            <div class="keypad checkin-phone-keypad">
                <div class="row-fluid">
                    <a href="#" class="btn digit keyboard-font">1</a>
                    <a href="#" class="btn digit keyboard-font">2</a>
                    <a href="#" class="btn digit keyboard-font">3</a>
                </div>
                <div class="row-fluid">
                    <a href="#" class="btn digit keyboard-font">4</a>
                    <a href="#" class="btn digit keyboard-font">5</a>
                    <a href="#" class="btn digit keyboard-font">6</a>
                </div>
                <div class="row-fluid">
                    <a href="#" class="btn digit keyboard-font">7</a>
                    <a href="#" class="btn digit keyboard-font">8</a>
                    <a href="#" class="btn digit keyboard-font">9</a>
                </div>
                <div class="row-fluid">
                    <a href="#" class="btn clear keyboard-font">C</a>
                    <a href="#" class="btn digit keyboard-font">0</a>
                    <a href="#" class="btn back keyboard-font"><i class='icon-arrow-left'></i></a>
                </div>
            </div>
        </div>
    </div>
    </asp:Panel>
</ContentTemplate>
</asp:UpdatePanel>