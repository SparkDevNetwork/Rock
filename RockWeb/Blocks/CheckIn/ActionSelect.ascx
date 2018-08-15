<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ActionSelect.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.ActionSelect" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $('a.btn-checkin-select').click(function () {
            $(this).siblings().attr('onclick', 'return false;');
        });
    });
</script>

<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <Rock:ModalAlert ID="maWarning" runat="server" />

    <div class="checkin-header">
        <h1><asp:Literal ID="lTitle" runat="server" /></h1>
    </div>

    <div class="checkin-body">

        <div class="checkin-scroll-panel">
            <div class="scroller">

                <div class="control-group checkin-body-container">
                    <label class="control-label"><asp:Literal ID="lCaption" runat="server"></asp:Literal></label>
                    <div class="controls">
                        <Rock:BootstrapButton ID="lbCheckOut" runat="server" Text="Check Out" CssClass="btn btn-primary btn-large btn-block btn-checkin-select" OnClick="lbCheckOut_Click" DataLoadingText="Loading..." />
                       <Rock:BootstrapButton ID="lbCheckIn" runat="server" Text="Check In" CssClass="btn btn-default btn-large btn-block btn-checkin-select" OnClick="lbCheckIn_Click" DataLoadingText="Loading..." />
                    </div>
                </div>

            </div>
        </div>

    </div>

    <div class="checkin-footer">
        <div class="checkin-actions">
            <asp:LinkButton CssClass="btn btn-default btn-back" ID="lbBack" runat="server" OnClick="lbBack_Click" Text="Back" />
            <asp:LinkButton CssClass="btn btn-default btn-cancel" ID="lbCancel" runat="server" OnClick="lbCancel_Click" Text="Cancel" />
        </div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>
