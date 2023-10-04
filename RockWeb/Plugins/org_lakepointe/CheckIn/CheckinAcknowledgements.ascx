<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CheckinAcknowledgements.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.CheckIn.CheckinAcknowledgements" %>

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
            <h1>
                <asp:Literal ID="ltitle" runat="server" /></h1>
        </div>
        <div class="checkin-body">
            <div class="checkin-scroll-panel">
                <div class="scroller">
                    <div class="control-group checkin-body-container">
                        <label class="control-label">
                            <asp:Literal ID="lCaption" runat="server" />
                        </label>
                        <div class="checkin-AcknowledgementText">
                            <asp:Literal ID="lAcknowledgement" runat="server" />
                        </div>
                        <div class="controls">
                            <Rock:BootstrapButton ID="lbAcknowledge" runat="server" Text="I Agree" CssClass="btn btn-primary btn-large btn-block btn-checkin-start" DataLoadingText="Loading..." OnClick="lbAcknowledge_Click" />
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
