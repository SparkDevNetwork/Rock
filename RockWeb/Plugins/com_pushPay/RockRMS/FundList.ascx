<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FundList.ascx.cs" Inherits="RockWeb.Plugins.com_pushPay.RockRMS.FundList" %>

<asp:UpdatePanel ID="upnlList" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlFunds" runat="server" CssClass="panel panel-block" >
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-building-o"></i> <asp:Literal ID="lMerchantName" runat="server" Text="Pushpay Merchant Listing" /> Funds</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:Grid ID="gFunds" runat="server" AllowSorting="true" >
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Pushpay Fund Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="FinancialAccount" HeaderText="Rock Financial Account" SortExpression="FinancialAccountName" HtmlEncode="false" />
                            <Rock:EditField OnClick="gFunds_Edit" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>

        <Rock:ModalDialog runat="server" ID="mdEditFundSettings" SaveButtonText="Save" Title="Pushpay Fund Settings" OnSaveClick="mdEditFundSettings_SaveClick" ValidationGroup="FundEdit">
            <Content>
                <asp:HiddenField runat="server" ID="hfFundId" />
                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="MerchantEdit" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:AccountPicker ID="apRockAccount" runat="server" Label="Rock Financial Account" Help="The Rock Financial Account that should be used when downloading transactions from Pushpay with this fund." />
                    </div>
                    <div class="col-md-6">
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

        <script type="text/javascript">
            Sys.Application.add_load(function () {
                $('[data-toggle="tooltip"]').tooltip()
            })
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
