<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Search.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Search" %>
<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <script>
        
        Sys.Application.add_load(function () {
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

    </script>

    <Rock:ModalAlert ID="maWarning" runat="server" />

    <div class="row checkin-header">
        <div class="col-md-12">
            <h1>Search By Phone</h1>
        </div>
    </div>
                
    <div class="row checkin-body">
        <div class="col-md-12">
            <div class="checkin-search-body">
                <Rock:LabeledTextBox ID="tbPhone" MaxLength="10" CssClass="checkin-phone-entry" runat="server" LabelText="Phone Number" />

                <div class="tenkey checkin-phone-keypad">
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
                        <a href="#" class="btn btn-default btn-lg command back">Back</a>
                        <a href="#" class="btn btn-default btn-lg digit">0</a>
                        <a href="#" class="btn btn-default btn-lg command clear">Clear</a>
                    </div>
                </div>

                <div class="checkin-actions">
                    <Rock:BootstrapButton CssClass="btn btn-primary" ID="lbSearch" runat="server" OnClick="lbSearch_Click" Text="Search" DataLoadingText="Searching..." ></Rock:BootstrapButton>
                </div>

            </div>
            
        </div>
    </div>


    <div class="checkin-footer">   
        <div class="checkin-actions">
            <asp:LinkButton CssClass="btn btn-default" ID="lbBack" runat="server" OnClick="lbBack_Click" Text="Back" />
        </div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>
