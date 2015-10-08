<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionReport.ascx.cs" Inherits="RockWeb.Blocks.Finance.TransactionReport" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-credit-card"></i> Transaction Report</h1>
            </div>
            <div class="panel-body">

                <div class="well">
            
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DateRangePicker ID="drpFilterDates" Label="Date Range" runat="server" />
                            <Rock:BootstrapButton ID="bbtnApply" CssClass="btn btn-primary" runat="server" Text="Apply" OnClick="bbtnApply_Click" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBoxList ID="cblAccounts" runat="server" />
                        </div>
                    </div>
         
                </div>

                <asp:Panel ID="pnlSummary" CssClass="well account-summary" runat="server">
                    <strong>Summary:</strong>
                    <ul><asp:Literal ID="lAccountSummary" runat="server" /></ul>
                </asp:Panel>

                <div class="grid">
                    <Rock:Grid ID="gTransactions" runat="server" >
                        <Columns>
                            <Rock:RockBoundField DataField="TransactionDateTime" DataFormatString="{0:d}" HeaderText="Date" />
                            <Rock:RockBoundField DataField="CurrencyType" HeaderText="Currency Type" HtmlEncode="false" />
                            <Rock:RockBoundField DataField="Summary" HeaderText="Summary" HtmlEncode="false" />
                            <Rock:CurrencyField DataField="TotalAmount" HeaderText="Amount" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
