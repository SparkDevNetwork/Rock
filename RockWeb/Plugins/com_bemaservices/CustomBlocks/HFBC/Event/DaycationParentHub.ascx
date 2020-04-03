<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DaycationParentHub.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.Event.DaycationParentHub" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="col-md-8">
            <h3>Upcoming Payments</h3>
            <div class="scheduledtransaction-list">
                <asp:Repeater ID="rptScheduledTransactions" OnItemDataBound="rptScheduledTransactions_ItemDataBound" runat="server">
                    <ItemTemplate>
                        <div class="scheduledtransaction-item">
                            <asp:Literal ID="lScheduledContent" runat="server" />
                            <asp:HiddenField ID="hfScheduledTransactionId" runat="server"></asp:HiddenField>
                            <div class="actions">
                                <Rock:BootstrapButton ID="bbtnDelete" runat="server" OnClick="bbtnDelete_Click" Text="Delete" CssClass="btn btn-danger" />
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>

            <asp:Panel ID="pnlNoScheduledTransactions" runat="server" CssClass="alert alert-info" Visible="false">
                <asp:Literal ID="lNoScheduledTransactionsMessage" runat="server" />
            </asp:Panel>

            </br>

            <h3>Past Payments</h3>
            <div class="scheduledtransaction-list">
                <asp:Repeater ID="rptTransactions" OnItemDataBound="rptTransactions_ItemDataBound" runat="server">
                    <ItemTemplate>
                        <div class="scheduledtransaction-item">
                            <asp:Literal ID="lTransactionContent" runat="server" />
                            <asp:HiddenField ID="hfTransactionId" runat="server"></asp:HiddenField>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>

            <asp:Panel ID="pnlNoTransactions" runat="server" CssClass="alert alert-info" Visible="false">
                <asp:Literal ID="lNoTransactionsMessage" runat="server" />
            </asp:Panel>
        </div>
        <div class="col-md-4">
            <h3>Registered Children</h3>
            <asp:Repeater ID="rptRegistrants" OnItemDataBound="rptRegistrants_ItemDataBound" runat="server">
                <ItemTemplate>
                    <asp:Literal ID="lRegistrantContent" runat="server" />
                    <asp:HiddenField ID="hfPersonId" runat="server"></asp:HiddenField>
                </ItemTemplate>
            </asp:Repeater>

            <asp:Panel ID="pnlNoRegistrants" runat="server" CssClass="alert alert-info" Visible="false">
                <asp:Literal ID="lNoRegistrantsMessage" runat="server" />
            </asp:Panel>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
