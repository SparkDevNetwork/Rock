<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrationHtmlChecker.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Event.RegistrationHtmlChecker" %>
<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <div class="panel panel-block" runat="server" id="pnlMain">
            <div class="panel-heading">
                <h1 class="panel-title">Registration HTML Checker</h1>
            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col-xs-12">
                        <asp:Literal runat="server" ID="lContent" Text="" />
                    </div>
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
