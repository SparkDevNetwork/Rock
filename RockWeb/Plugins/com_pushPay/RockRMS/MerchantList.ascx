<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MerchantList.ascx.cs" Inherits="RockWeb.Plugins.com_pushPay.RockRMS.MerchantList" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upnlList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlMerchants" runat="server" CssClass="panel panel-block" >
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-building"></i> <asp:Literal ID="lAccountName" runat="server" Text="Pushpay" /> Merchant Listings</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:Grid ID="gMerchants" runat="server" AllowSorting="true" OnRowSelected="gMerchants_RowSelected">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Merchant Listing" SortExpression="Name" />
                            <Rock:RockBoundField DataField="MemoField" HeaderText="Memo Field" SortExpression="MemoField"  />
                            <Rock:RockBoundField DataField="Funds" HeaderText="Funds" HtmlEncode="false" />
                            <Rock:RockBoundField DataField="DefaultAccount" HeaderText="Default Account" HtmlEncode="false" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
                            <Rock:EditField OnClick="gMerchants_Edit" />
                            <Rock:EditField OnClick="gMerchants_Download" IconCssClass="fa fa-cloud-download" ToolTip="Download Payments" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>

        <div class="actions">
            <asp:LinkButton id="btnRefresh" runat="server" CssClass="btn btn-default" CausesValidation="false" OnClick="btnRefresh_Click"><i class="fa fa-refresh"></i> Refresh</asp:LinkButton>
        </div>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />
        <asp:HiddenField runat="server" ID="hfMerchantId" />

        <Rock:ModalDialog runat="server" ID="mdEditMerchantSettings" SaveButtonText="Save" Title="Pushpay Merchant Listing Settings" OnSaveClick="mdEditMerchantSettings_SaveClick" OnCancelScript="clearActiveDialog();"  ValidationGroup="MerchantEdit">
            <Content>
                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="AccountEdit" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:AccountPicker ID="apDefaultAccount" runat="server" Label="Default Account" Help="The default Rock Financial Account to use when a payment is downloaded from Pushpay with a fund that has not been tied to a Rock account." />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbActive" runat="server" Label="Active" Text="Yes" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:DefinedValuePicker ID="dvTransactionType" runat="server" Label="Transaction Type" Help="The transaction type that should be used for transactions downloaded from this merchant." />
                        <Rock:RockDropDownList ID="ddlMemoReferenceField" runat="server" Label="Pushpay Field for Memo" ValidationGroup="AccountEdit" Help="Optional Pushpay reference field for this merchant listing that contains a memo value." />
                        <Rock:RockTextBox ID="tbGivingUrl" runat="server" Label="Giving URL" ValidationGroup="AccountEdit" Help="The base URL where people can give to this merchant. Example: https://sandbox.pushpay.io/g/rockrms-multicampus-alpha" />
                    </div>
                    <div class="col-md-6">
                        <Rock:CodeEditor ID="ceBatchSuffix" runat="server" Label="Batch Name Suffix" EditorMode="Lava" EditorTheme="Rock" EditorHeight="120"
                            Help="When a transaction is downloaded from Pushpay and added to Rock it will be added to a batch for the date of the transaction that has a suffix matching this template. If a matching batch name is not found, a new batch will be created." />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog runat="server" ID="mdDownload" Title="Download Payments" OnCancelScript="clearActiveDialog();"  ValidationGroup="Download" SaveButtonText="Close" OnSaveClick="mdDownload_SaveClick" CancelLinkVisible="false" >
            <Content>
                <div>
                    After the download completes we will email you the results summary.
                </div>

                <Rock:DateRangePicker ID="drpDates" runat="server" Label="Date Range" Required="true" ValidationGroup="Download" />

                <Rock:EmailBox ID="rebResultsEmail" runat="server" Label="Email to Send the Results Summary" Required="true" ValidationGroup="Download" />

                <div class="actions">
                    <Rock:BootstrapButton ID="btnDownload" runat="server" CssClass="btn btn-primary" Text="Download" DataLoadingText="Downloading..." CausesValidation="true" OnClick="btnDownload_Click" ValidationGroup="Download" />
                </div>
                <br />
                <Rock:NotificationBox ID="nbDownload" runat="server" NotificationBoxType="Success" Heading="Payment Download:" Visible="false" />
            </Content>
        </Rock:ModalDialog>

        <script type="text/javascript">
            Sys.Application.add_load(function () {
                $('[data-toggle="tooltip"]').tooltip()
            })
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
