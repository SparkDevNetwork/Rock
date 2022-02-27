<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowEntityLaunch.ascx.cs" Inherits="RockWeb.Plugins.org_secc.Workflow.WorkflowEntityLaunch" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title">Workflow Status</h1>
            </div>
            <div class="panel-body">
                <div class="clearfix">
                    <Rock:NotificationBox NotificationBoxType="Info" ID="nbInformation" runat="server"></Rock:NotificationBox>
                    <Rock:BootstrapButton ID="bbtnLaunch" runat="server" Enabled="false" OnClick="Launch_Click" Text="Launch" CssClass="btn btn-primary pull-right" />
                </div>
                <h3>Output</h3>
                <div class="well" style="background-color: #fff">
                    <asp:Literal ID="litOutput" runat="server"></asp:Literal>
                </div>
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
