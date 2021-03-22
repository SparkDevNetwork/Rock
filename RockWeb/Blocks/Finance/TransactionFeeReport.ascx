<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionFeeReport.ascx.cs" Inherits="RockWeb.Blocks.Finance.TransactionFeeReport" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-file-invoice-dollar"></i>
                    Transaction Fee Report
                </h1>
            </div>
            <div class="panel-body">

                <%-- Filter --%>
                <div class="row">
                    <div class="col-md-4">
                        <Rock:AccountPicker ID="apAccounts" AllowMultiSelect="true" Label="Account" runat="server" />
                    </div>
                    <div class="col-md-8 d-flex flex-wrap align-items-end">
                        <Rock:SlidingDateRangePicker ID="srpFilterDates" Label="Date Range" runat="server" FormGroupCssClass="mb-2" />
                        <div class="pb-2" style="margin-bottom: 6px">
                            <Rock:BootstrapButton ID="bbtnApply" CssClass="btn btn-primary" CausesValidation="true" runat="server" Text="Apply" OnClick="bbtnApply_Click" />
                        </div>
                    </div>
                </div>

                <%-- Report Output --%>
                <asp:Literal runat="server" ID="lKPIHtml" />

            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
