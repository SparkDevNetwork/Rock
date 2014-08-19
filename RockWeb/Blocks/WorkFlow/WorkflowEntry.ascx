<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowEntry.ascx.cs" Inherits="RockWeb.Blocks.WorkFlow.WorkflowEntry" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-gear"></i> Workflow Entry</h1>
            </div>
            <div class="panel-body">

                <asp:Panel ID="pnlForm" CssClass="panel panel-block" runat="server">

                    <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <asp:Literal ID="lheadingText" runat="server" />

                    <asp:PlaceHolder ID="phAttributes" runat="server" />
            
                    <asp:Literal ID="lFootingText" runat="server" />

                    <div class="actions">
                        <asp:PlaceHolder ID="phActions" runat="server" />
                    </div>

                </asp:Panel>

                <Rock:NotificationBox ID="nbMessage" runat="server" Dismissable="true" />

            </div>

        </div>

    </ContentTemplate>
</asp:UpdatePanel>
