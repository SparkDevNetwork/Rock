<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SmtpServerRedirect.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Communications.SmtpServerRedirect" %>
<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <div class="panel panel-block" runat="server" id="pnlMain">
            <div class="panel-heading">
                <h1 class="panel-title">SMTP Redirect</h1>
            </div>
            <div class="panel-body">
                <div class="row" runat="server" id="pnlAlert">
                    <div class="col-xs-12">
                        <div class="alert alert-warning" role="alert">
                            <h4 class="alert-heading">SMTP Not Configured Correctly</h4>
                            <asp:Literal runat="server" ID="lWarning" Text="" />
                            <br>
                            <asp:Button runat="server" ID="btnConfigureSmtp" Text="Fix It" OnClick="btnConfigureSmtp_Click" CssClass="btn btn-warning" />
                        </div>
                    </div>
                </div>
                <div class="row">
                    <div class="col-xs-12">
                        <asp:Literal runat="server" ID="lContent" Text="If the page did not automatically redirect you, click the button below:" />
                        <br />
                        <a href="#" class="btn btn-primary" runat="server" id="btnGoToSmtpUi">Go to UI</a>
                    </div>
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
