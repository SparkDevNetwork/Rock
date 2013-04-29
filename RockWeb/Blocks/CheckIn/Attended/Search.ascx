<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Search.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Attended.Search" %>
<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <script>
        
        Sys.Application.add_load(function () {
            $('.keyboard a.digit').click(function () {
                $name = $("input[id$='tbSearchBox']");
                $name.val($name.val() + $(this).html());
            });
            $('.keyboard a.back').click(function () {
                $name = $("input[id$='tbSearchBox']");
                $name.val($name.val().slice(0,-1));
            });
            $('.keyboard a.clear').click(function () {
                $name = $("input[id$='tbSearchBox']");
                $name.val('');
            });
            $('.keyboard a.spacebar').click(function () {
                $name = $("input[id$='tbSearchBox']");
                $name.val($name.val() + ' ');
            });
        });

    </script>

    <Rock:ModalAlert ID="maWarning" runat="server" />

    <div class="row-fluid attended-checkin-header">
        <div class="span3 attended-checkin-actions">
            <asp:LinkButton ID="lbAdmin" runat="server" CssClass="btn btn-large btn-primary" OnClick="lbAdmin_Click" Text="Admin"></asp:LinkButton>
        </div>
        <div class="span6">
            <h1>Check In</h1>
        </div>
        <div class="span3 attended-checkin-actions">
            <asp:LinkButton ID="lbSearch" runat="server" CssClass="btn btn-large last btn-primary" OnClick="lbSearch_Click" Text="Search"></asp:LinkButton>
        </div>
    </div>
                
    <div class="row-fluid checkin-body">
        <div class="span12">
            <div class="attended-checkin-search-body">
                <Rock:LabeledTextBox ID="tbSearchBox" MaxLength="50" CssClass="attended-checkin-keyboard-entry" runat="server" LabelText="" />

                <div class="keyboard attended-checkin-keyboard">
                    <div>
                        <a href="#" class="btn btn-large digit">1</a>
                        <a href="#" class="btn btn-large digit">2</a>
                        <a href="#" class="btn btn-large digit">3</a>
                        <a href="#" class="btn btn-large digit">4</a>
                        <a href="#" class="btn btn-large digit">5</a>
                        <a href="#" class="btn btn-large digit">6</a>
                        <a href="#" class="btn btn-large digit">7</a>
                        <a href="#" class="btn btn-large digit">8</a>
                        <a href="#" class="btn btn-large digit">9</a>
                        <a href="#" class="btn btn-large digit">0</a>
                    </div>
                    <div>
                        <a href="#" class="btn btn-large digit">Q</a>
                        <a href="#" class="btn btn-large digit">W</a>
                        <a href="#" class="btn btn-large digit">E</a>
                        <a href="#" class="btn btn-large digit">R</a>
                        <a href="#" class="btn btn-large digit">T</a>
                        <a href="#" class="btn btn-large digit">Y</a>
                        <a href="#" class="btn btn-large digit">U</a>
                        <a href="#" class="btn btn-large digit">I</a>
                        <a href="#" class="btn btn-large digit">O</a>
                        <a href="#" class="btn btn-large digit">P</a>
                    </div>
                    <div>
                        <a href="#" class="btn btn-large digit">A</a>
                        <a href="#" class="btn btn-large digit">S</a>
                        <a href="#" class="btn btn-large digit">D</a>
                        <a href="#" class="btn btn-large digit">F</a>
                        <a href="#" class="btn btn-large digit">G</a>
                        <a href="#" class="btn btn-large digit">H</a>
                        <a href="#" class="btn btn-large digit">J</a>
                        <a href="#" class="btn btn-large digit">K</a>
                        <a href="#" class="btn btn-large digit">L</a>
                        <a href="#" class=""></a>
                    </div>
                    <div>
                        <a href="#" class="btn btn-large digit">Z</a>
                        <a href="#" class="btn btn-large digit">X</a>
                        <a href="#" class="btn btn-large digit">C</a>
                        <a href="#" class="btn btn-large digit">V</a>
                        <a href="#" class="btn btn-large digit">B</a>
                        <a href="#" class="btn btn-large digit">N</a>
                        <a href="#" class="btn btn-large digit">M</a>
                        <a href="#" class=""></a>
                        <a href="#" class=""></a>
                        <a href="#" class="btn btn-large back"><i class='icon-arrow-left'></i></a>
                    </div>
                    <div>
                        <a href="#" class="btn btn-large spacebar">SPACE</a>
                    </div>
                </div>
            </div>
        </div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>