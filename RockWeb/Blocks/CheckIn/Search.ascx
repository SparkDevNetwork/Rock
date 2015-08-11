<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Search.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Search" %>
<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <style>
        input.namesearch{
            height: 60px;
            font-weight: 700;
            font-size: 36px;
            padding: 6px 20px;
            width: 360px;
            float: left;
            margin-bottom: 10px;
        }
    </style>

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

            // set focus to the input unless on a touch device
            var isTouchDevice = 'ontouchstart' in document.documentElement;
            if (!isTouchDevice) {
                $('.checkin-phone-entry').focus();
            }
        });
        
    </script>

    <Rock:ModalAlert ID="maWarning" runat="server" />

    <div class="checkin-header">
        <h1>Search By Phone</h1>
    </div>
                
    <div class="checkin-body">
        
        <div class="checkin-scroll-panel">
            <div class="scroller">

                <div class="checkin-search-body">

                <asp:Panel ID="pnlSearchPhone" runat="server">
                    <Rock:RockTextBox ID="tbPhone" MaxLength="10" CssClass="checkin-phone-entry" runat="server" Label="Phone Number" />

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
                </asp:Panel>

                <asp:Panel ID="pnlSearchName" runat="server">
                    <Rock:RockTextBox ID="txtName" runat="server" Label="Name" CssClass="namesearch" />
                </asp:Panel>

                <div class="checkin-actions">
                    <Rock:BootstrapButton CssClass="btn btn-primary" ID="lbSearch" runat="server" OnClick="lbSearch_Click" Text="Search" DataLoadingText="Searching..." ></Rock:BootstrapButton>
                </div>

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
