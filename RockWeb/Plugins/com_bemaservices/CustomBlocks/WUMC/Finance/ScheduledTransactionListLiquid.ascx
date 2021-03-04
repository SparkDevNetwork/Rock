<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduledTransactionListLiquid.ascx.cs" Inherits="RockWeb.Plugins.com_bemadev.Finance.ScheduledTransactionListLiquid" %>

<style>


</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
<h3 class="mt-0">Recurring Contributions</h3>
        <div class="scheduledtransaction-list">
            <asp:Repeater ID="rptScheduledTransactions" OnItemDataBound="rptScheduledTransactions_ItemDataBound" runat="server">
                <ItemTemplate>
                    <div class="row">
						<div class="col-md-12">
							<div class="clearfix gift-box" style="padding-left:0px !important;">
                        <asp:Literal ID="lLiquidContent" runat="server" />
                        <asp:HiddenField ID="hfScheduledTransactionId" runat="server"></asp:HiddenField>
						
                            <asp:Button ID="btnEdit" runat="server" AccessKey="m" Text="Edit" OnClick="btnEdit_Click" CssClass="btn btn-default hidden"  />
                            <Rock:BootstrapButton ID="bbtnDelete" runat="server" OnClick="bbtnDelete_Click" Text="Delete" CssClass="btn btn-danger" />
							</div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>        
        
        <asp:Panel ID="pnlNoScheduledTransactions" runat="server" CssClass="alert alert-info" Visible="false">
            <asp:Literal ID="lNoScheduledTransactionsMessage" runat="server" />
        </asp:Panel>

        <asp:LinkButton ID="lbAddScheduledTransaction" runat="server" CssClass="btn btn-primary hidden" OnClick="lbAddScheduledTransaction_Click" />

        <asp:Literal ID="lDebug" runat="server" />
        
    </ContentTemplate>
</asp:UpdatePanel>
