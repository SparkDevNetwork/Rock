<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionReport.ascx.cs" Inherits="RockWeb.Blocks.Finance.TransactionReport" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

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
        <asp:Literal ID="lTest" runat="server"></asp:Literal>
        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-star"></i> Blank List Block</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gTransactions" runat="server" OnRowDataBound="gTransactions_RowDataBound">
                        <Columns>
                            <asp:BoundField DataField="TransactionDateTime" DataFormatString="{0:d}" HeaderText="Date" SortExpression="TransactionDateTime" />
                            <asp:TemplateField HeaderText="Currency Type" >
                                <ItemTemplate>
                                    <asp:Literal ID="lCurrencyType" runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Summary" >
                                <ItemTemplate>
                                    <asp:Literal ID="lSummaryText" runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="TotalAmount" HeaderText="Amount" SortExpression="TotalAmount" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
