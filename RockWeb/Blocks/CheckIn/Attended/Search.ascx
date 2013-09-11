<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Search.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Attended.Search" %>

<script>        
    function SetKeyEvents() {
        $('.keyboard a.character').unbind('click').click(function () {
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

    <asp:Panel ID="pnlSearch" runat="server" DefaultButton="lbSearch" CssClass="attended">
            
        <div class="row-fluid checkin-header ">
            <div class="span3 checkin-actions">
                <asp:LinkButton ID="lbAdmin" runat="server" CssClass="btn btn-large btn-primary" OnClick="lbAdmin_Click" Text="Admin" />
                <asp:LinkButton ID="lbBack" runat="server" CssClass="btn btn-large btn-primary" OnClick="lbBack_Click" Text="Back" Visible="false" />
            </div>
            <div class="span6">
                <h1>Check In</h1>
            </div>
            <div class="span3 checkin-actions">
                <asp:LinkButton ID="lbSearch" runat="server" CssClass="btn btn-large btn-primary" OnClick="lbSearch_Click" Text="Search" />
            </div>
        </div>
            
        <div class="row-fluid checkin-body keyboard">
            <div class="span12">
                <Rock:LabeledTextBox ID="tbSearchBox" MaxLength="50" CssClass="input" runat="server" LabelText="" TabIndex="0" />

                <div class="row-fluid">
                    <a href="#" class="btn character">1</a>
                    <a href="#" class="btn character">2</a>
                    <a href="#" class="btn character">3</a>
                    <a href="#" class="btn character">4</a>
                    <a href="#" class="btn character">5</a>
                    <a href="#" class="btn character">6</a>
                    <a href="#" class="btn character">7</a>
                    <a href="#" class="btn character">8</a>
                    <a href="#" class="btn character">9</a>
                    <a href="#" class="btn character">0</a>
                </div>
                <div class="row-fluid">
                    <a href="#" class="btn character">Q</a>
                    <a href="#" class="btn character">W</a>
                    <a href="#" class="btn character">E</a>
                    <a href="#" class="btn character">R</a>
                    <a href="#" class="btn character">T</a>
                    <a href="#" class="btn character">Y</a>
                    <a href="#" class="btn character">U</a>
                    <a href="#" class="btn character">I</a>
                    <a href="#" class="btn character">O</a>
                    <a href="#" class="btn character">P</a>
                </div>
                <div class="row-fluid">
                    <a href="#" class="btn character">A</a>
                    <a href="#" class="btn character">S</a>
                    <a href="#" class="btn character">D</a>
                    <a href="#" class="btn character">F</a>
                    <a href="#" class="btn character">G</a>
                    <a href="#" class="btn character">H</a>
                    <a href="#" class="btn character">J</a>
                    <a href="#" class="btn character">K</a>
                    <a href="#" class="btn character">L</a>
                    <a href="#" class=""></a>
                </div>
                <div class="row-fluid">
                    <a href="#" class="btn character">Z</a>
                    <a href="#" class="btn character">X</a>
                    <a href="#" class="btn character">C</a>
                    <a href="#" class="btn character">V</a>
                    <a href="#" class="btn character">B</a>
                    <a href="#" class="btn character">N</a>
                    <a href="#" class="btn character">M</a>
                    <a href="#" class=""></a>
                    <a href="#" class=""></a>
                    <a href="#" class="btn back"><i class='icon-arrow-left'></i></a>
                </div>

                <div class="row-fluid">
                    <a href="#" class="btn spacebar">SPACE</a>
                </div>
           
            </div>
        </div>
    </asp:Panel>
</ContentTemplate>
</asp:UpdatePanel>