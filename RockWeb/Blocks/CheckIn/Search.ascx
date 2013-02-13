<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Search.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Search" %>
<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <script>
        $(document).ready(function (e) {
            $('div.phone-buttons a.digit').click(function () {
                $phoneNumber = $("input[id$='tbPhone']");
                $phoneNumber.val($phoneNumber.val() + $(this).html());
            });
            $('div.phone-buttons a.back').click(function () {
                $phoneNumber = $("input[id$='tbPhone']");
                $phoneNumber.val($phoneNumber.val().slice(0,-1));
            });
            $('div.phone-buttons a.clear').click(function () {
                $phoneNumber = $("input[id$='tbPhone']");
                $phoneNumber.val('');
            });
        });
    </script>

    <Rock:ModalAlert ID="maWarning" runat="server" />

    <fieldset>
    <legend>Search</legend>

        <Rock:LabeledTextBox ID="tbPhone" runat="server" LabelText="Phone Number" />
        
    </fieldset>

    <div class="phone-buttons">
        <div>
            <a href="#" class="btn btn-large btn-inverse digit">1</a>
            <a href="#" class="btn btn-large btn-inverse digit">2</a>
            <a href="#" class="btn btn-large btn-inverse digit">3</a>
        </div>
        <div>
            <a href="#" class="btn btn-large btn-inverse digit">4</a>
            <a href="#" class="btn btn-large btn-inverse digit">5</a>
            <a href="#" class="btn btn-large btn-inverse digit">6</a>
        </div>
        <div>
            <a href="#" class="btn btn-large btn-inverse digit">7</a>
            <a href="#" class="btn btn-large btn-inverse digit">8</a>
            <a href="#" class="btn btn-large btn-inverse digit">9</a>
        </div>
        <div>
            <a href="#" class="btn btn-large back">Back</a>
            <a href="#" class="btn btn-large btn-inverse digit">0</a>
            <a href="#" class="btn btn-large clear">Clear</a>
        </div>
    </div>

    <div class="actions">
        <asp:LinkButton CssClass="btn btn-primary" ID="lbSearch" runat="server" OnClick="lbSearch_Click" Text="Search" />
        <asp:LinkButton CssClass="btn btn-secondary" ID="lbBack" runat="server" OnClick="lbBack_Click" Text="Back" />
        <asp:LinkButton CssClass="btn btn-secondary" ID="lbCancel" runat="server" OnClick="lbCancel_Click" Text="Cancel" />
    </div>

</ContentTemplate>
</asp:UpdatePanel>
