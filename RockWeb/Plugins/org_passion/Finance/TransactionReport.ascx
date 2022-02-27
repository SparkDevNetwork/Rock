<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionReport.ascx.cs" Inherits="RockWeb.Blocks.Finance.TransactionReport" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-credit-card"></i>Transaction Report</h1>
            </div>
            <div class="panel-body">

                <div class="row">

                    <div class="col-xs-12 col-sm-8 col-sm-offset-2 text-center">

                        <div style="padding-top: 25px;" class="form-group">
                            <p class="statcard-desc">
                                Total Giving
                            <br class="visible-xs" />
                                Year To Date
                            </p>

                            <div id="gyTD" style="padding-bottom: 5px; color: #666; line-height: 1;" class="text-jumbo" runat="server"></div>
                        </div>

                        <div class="form-group col-xs-12 col-sm-6 col-sm-offset-3">
                            <a runat="server" id="btnGiveNow" href="/give-online" class="btn btn-primary btn-block">Give Now // Manage Giving</a>
                        </div>

                    </div>

                    <%--/* Defined in template in code behind */--%>
                    <asp:Literal Visible="false" ID="lCommitmentResults" runat="server" />

                    <div class="col-xs-12">
                        <hr style="opacity: .5;" />
                    </div>

                </div>

                <div class="row">
                    <div class="col-xs-12 col-sm-6 form-group mobile-text-center">
                        <h3>Date Range:</h3>
                        <div class="form-group">
                            <Rock:DateRangePicker ID="drpFilterDates" runat="server" />
                        </div>
                        <Rock:BootstrapButton ID="bbtnApply" CssClass="btn btn-gray" runat="server" Text="Apply" OnClick="bbtnApply_Click" />
                    </div>
                    <div class="col-xs-12 col-sm-6 hidden">
                        <Rock:RockCheckBoxList ID="cblAccounts" runat="server" />
                    </div>
                    <div class="col-xs-12 col-sm-6 form-group">
                        <h3>Gift Summary:</h3>
                        <ul class="list-unstyled">
                            <asp:Literal ID="lAccountSummary" runat="server" />
                        </ul>
                    </div>
                </div>






                <asp:Panel ID="pnlSummary" CssClass="well account-summary hidden" runat="server">
                </asp:Panel>

                <div class="grid">
                    <Rock:Grid ID="gTransactions" BorderStyle="None" runat="server">
                        <Columns>
                            <Rock:RockBoundField DataField="TransactionDateTime" DataFormatString="{0:d}" HeaderText="Date" />
                            <Rock:RockBoundField DataField="CurrencyType" Visible="false" HeaderText="Currency Type" HtmlEncode="false" />
                            <Rock:RockBoundField DataField="TransactionCode" Visible="false" HeaderText="Transaction Code" HtmlEncode="false" />
                            <Rock:RockBoundField DataField="ForeignKey" Visible="false" HeaderText="Foreign Key" HtmlEncode="false" />
                            <Rock:RockBoundField DataField="Summary" HeaderText="Fund" HtmlEncode="false" />
                            <Rock:CurrencyField DataField="TotalAmount" HeaderText="Amount" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
