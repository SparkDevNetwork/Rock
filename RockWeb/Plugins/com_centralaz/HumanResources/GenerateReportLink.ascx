<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GenerateReportLink.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.HumanResources.GenerateReportLink" %>
<asp:UpdatePanel ID="upnlContent" runat="server">

    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">

            <asp:HiddenField ID="hfGroupTypeId" runat="server" />

            <div class="panel panel-block">
                <div class="panel-body">
                    <div class="pull-right">
                        <asp:Button ID="btnLastYear" runat="server" CssClass="btn btn-primary" OnClick="btnLastYear_Click" />
                        <asp:Button ID="btnCurrentYear" runat="server" CssClass="btn btn-primary" OnClick="btnCurrentYear_Click" />
                        <asp:Button ID="btnNextYear" runat="server" CssClass="btn btn-primary" OnClick="btnNextYear_Click" />
                    </div>
                </div>
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
