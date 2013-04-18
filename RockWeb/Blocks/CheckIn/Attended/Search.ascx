<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Search.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Attended.Search" %>
<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

<%--    <script>
        
        Sys.Application.add_load(function () 
            $('.tenkey a.digit').click(function () {
                $phoneNumber = $("input[id$='tbPhone']");
                $phoneNumber.val($phoneNumber.val() + $(this).html());
            });
            $('.tenkey a.back').click(function () {
                $phoneNumber = $("input[id$='tbPhone']");
                $phoneNumber.val($phoneNumber.val().slice(0,-1));
            });
            $('.tenkey a.clear').click(function () {
                $phoneNumber = $("input[id$='tbPhone']");
                $phoneNumber.val('');
            });
        });

    </script>--%>

    <Rock:ModalAlert ID="maWarning" runat="server" />

    <div class="row-fluid checkin-header">
        <div class="span3">
            <asp:LinkButton ID="lbAdmin" runat="server" CssClass="btn btn-large" OnClick="lbAdmin_Click" Text="ADMIN"></asp:LinkButton>
        </div>
        <div class="span6">
            <h1>Check In</h1>
        </div>
        <div class="span3">
            <asp:LinkButton ID="lbSearch" runat="server" CssClass="btn btn-large" OnClick="lbSearch_Click" Text="SEARCH"></asp:LinkButton>
        </div>
    </div>
                
    <div class="row-fluid checkin-body">
        <div class="span12">
            <div class="checkin-search-body">
                <Rock:LabeledTextBox ID="tbSearchBox" MaxLength="50" CssClass="checkin-phone-entry" runat="server" LabelText="" />

                <div class="checkin-phone-keypad">
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
                        <a href="#" class="btn btn-large digit"></a>
                    </div>
                    <div>
                        <a href="#" class="btn btn-large digit">Z</a>
                        <a href="#" class="btn btn-large digit">X</a>
                        <a href="#" class="btn btn-large digit">C</a>
                        <a href="#" class="btn btn-large digit">V</a>
                        <a href="#" class="btn btn-large digit">B</a>
                        <a href="#" class="btn btn-large digit">N</a>
                        <a href="#" class="btn btn-large digit">M</a>
                        <a href="#" class="btn btn-large digit"></a>
                        <a href="#" class="btn btn-large digit"></a>
                        <a href="#" class="btn btn-large digit"><i class='icon-arrow-left'></i></a>
                    </div>
                </div>

<%--                <div class="tenkey checkin-phone-keypad">
                    <div>
                        <a href="#" class="btn btn-large digit">1</a>
                        <a href="#" class="btn btn-large digit">2</a>
                        <a href="#" class="btn btn-large digit">3</a>
                    </div>
                    <div>
                        <a href="#" class="btn btn-large digit">4</a>
                        <a href="#" class="btn btn-large digit">5</a>
                        <a href="#" class="btn btn-large digit">6</a>
                    </div>
                    <div>
                        <a href="#" class="btn btn-large digit">7</a>
                        <a href="#" class="btn btn-large digit">8</a>
                        <a href="#" class="btn btn-large digit">9</a>
                    </div>
                    <div>
                        <a href="#" class="btn btn-large back">Del</a>
                        <a href="#" class="btn btn-large digit">0</a>
                        <a href="#" class="btn btn-large clear">Clear</a>
                    </div>
                </div>--%>

<%--                <div class="checkin-actions">
                    <asp:LinkButton CssClass="btn btn-primary" ID="lbSearch" runat="server" OnClick="lbSearch_Click" Text="Search" />
                </div>--%>

            </div>
            
        </div>
    </div>


<%--    <div class="row-fluid checkin-footer">   
        <div class="checkin-actions">
            <asp:LinkButton CssClass="btn btn-secondary" ID="lbBack" runat="server" OnClick="lbBack_Click" Text="Back" />
        </div>
    </div>--%>

</ContentTemplate>
</asp:UpdatePanel>