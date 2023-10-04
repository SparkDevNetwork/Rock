<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountList.ascx.cs" Inherits="RockWeb.Plugins.com_pushPay.RockRMS.AccountList" %>

<asp:UpdatePanel ID="upnlList" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbVersionError" runat="server" NotificationBoxType="Danger" Heading="Version Incompatible" Visible="false" />

        <asp:Panel ID="pnlNew" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-bank"></i> Pushpay Integration</h1>
            </div>
            <div class="panel-body">
                <div class="col-md-6">
                    <asp:Image ID="imgPromotion" runat="server" CssClass="img-responsive" />
                    <div class="actions margin-t-md">
                        <asp:HyperLink ID="hlRequestAPI" runat="server" CssClass="btn btn-primary btn-block" >
                            <strong>Existing Pushpay Customer</strong><br />
                            <small>( Request API Keys From Pushpay )</small>
                        </asp:HyperLink>
                        <asp:HyperLink ID="hlGetStarted" runat="server" CssClass="btn btn-default btn-block">
                            <strong>New Pushpay Customer</strong><br />
                            <small>( Register For a Pushpay Account )</small>
                        </asp:HyperLink>
                    </div>
                </div>
                <div class="col-md-1">
                </div>
                <div class="col-md-5">
                    <asp:ValidationSummary ID="ValidationSummaryNew" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="NewAccount" />
                    <Rock:RockTextBox ID="tbNewClientId" runat="server" Label="Pushpay API Client Id" Required="true"  ValidationGroup="NewAccount"
                        Help="The Client ID provided by Pushpay to access the Pushpay API" />
                    <Rock:RockTextBox ID="tbNewClientSecret" runat="server" Label="Pushpay API Client Secret" TextMode="Password" Required="true"  ValidationGroup="NewAccount"
                        Help="The Client Secret provided by Pushpay to access the Pushpay API" />
                    <div class="actions">
                        <asp:LinkButton ID="lbSaveNew" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSaveNew_Click" ValidationGroup="NewAccount" />
                    </div>
                </div>
            </div>
        </asp:Panel>

        <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />

        <asp:Panel ID="pnlAccounts" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-bank"></i> Pushpay Accounts</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:Grid ID="gAccounts" runat="server" AllowSorting="true" OnRowSelected="gAccounts_RowSelected" RowItemText="Account">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Account Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="ClientId" HeaderText="API Client Id" SortExpression="" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
                            <Rock:BoolField DataField="Authorized" HeaderText="Authorized" SortExpression="Authorized" />
                            <Rock:RockBoundField DataField="MerchantHtmlBadge" HeaderText="Merchant Listings" HtmlEncode="false" />
                            <Rock:EditField OnClick="gAccounts_Edit" />
                            <Rock:EditField OnClick="gAccounts_Refresh" IconCssClass="fa fa-refresh" ToolTip="Refresh Merchant Listings and Funds" />
                            <Rock:DeleteField OnClick="gAccounts_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>

        <Rock:ModalDialog runat="server" ID="mdEditAccountSettings" SaveButtonText="Save" Title="Pushpay Account Settings" OnSaveClick="mdEditAccountSettings_SaveClick" ValidationGroup="AccountEdit">
            <Content>
                <asp:HiddenField runat="server" ID="hfAccountId" />
                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="AccountEdit" />

                <ul class="nav nav-pills margin-b-md">
                    <li id="liSettings" runat="server"><a href='#<%=divSettings.ClientID%>' data-toggle="pill">Settings</a></li>
                    <li id="liAdvancedSettings" runat="server"><a href='#<%=divAdvancedSettings.ClientID%>' data-toggle="pill">Advanced Settings</a></li>
                </ul>

                <div class="tab-content">

                    <div id="divSettings" runat="server" class="tab-pane">
                        <div class="row">
                            <div class="col-sm-6">
                                <Rock:RockTextBox ID="tbName" runat="server" Label="Account Name" Required="true" ValidationGroup="AccountEdit" />
                                <Rock:RockTextBox ID="tbClientId" runat="server" Label="API Client Id" Required="true" ValidationGroup="AccountEdit"
                                    Help="The Client ID provided by Pushpay to access the Pushpay API" />
                                <Rock:RockTextBox ID="tbClientSecret" runat="server" Label="API Client Secret" TextMode="Password" Required="true" ValidationGroup="AccountEdit"
                                    Help="The Client Secret provided by Pushpay to access the Pushpay API" />
                                <Rock:RockTextBox ID="tbEventRegistrationRedirectToken" runat="server" Label="Event Registration Redirect Token" ValidationGroup="AccountEdit" 
                                    Help="The Pushpay redirect URL token to use during event registration to return the individual back to Rock once payment is completed. This token is configured in Pushpay with a matching URL." />
                            </div>
                            <div class="col-sm-6">
                                <Rock:RockCheckBox ID="cbActive" runat="server" Label="Active" Text="Yes" />
                                <Rock:RockCheckBox ID="cbDownloadSettledOnly" runat="server" Label="Limit Download to Settled Transactions" Text="Yes" Help="Select this option if you do not want transactions downloaded prior to them being settled by Pushpay." />
                                <Rock:DatePicker ID="dpActiveDate" runat="server" Label="Cutover Date" Help="The earliest transaction date that should be downloaded. Any transactions that occurred prior to this date will not be downloaded and imported into Rock." />
                            </div>
                        </div>
                    </div>

                    <div id="divAdvancedSettings" runat="server" class="tab-pane">

                        <Rock:PanelWidget ID="wpAdvancedBatch" runat="server" Title="Batch Options" Expanded="true">
                            <div class="alert alert-info">When transactions are downloaded from Pushpay, the following settings determine how transactions will be grouped into batches.</div>
                            <div class="row">
                                <div class="col-sm-4">
                                    <Rock:RockCheckBox ID="cbUseTransactionDate" runat="server" Label="Batch by Transaction Date" Text="Yes" Checked="false"
                                        Help="Enable this option if transactions should be grouped into batches based on the date of the transaction. If this option is not selected, the transaction date 
                                        will be ignored and transactions will only be grouped by campus (optional) and by the batch name." />
                                </div>
                                <div class="col-sm-4">
                                    <Rock:RockCheckBox ID="cbUseCampus" runat="server" Label="Batch by Campus" Text="Yes" Checked="true"
                                        Help="Enable this option if transactions should be grouped into batches based on the campus that the transaction's account is configured for (if any)." />                                    
                                </div>
                                <div class="col-sm-4">
                                    <Rock:RockCheckBox ID="cbIncludeCurrencyType" runat="server" Label="Batch by Name" Text="Include Currency Type in Batch Name" Checked="true"
                                        Help="Transactions will always be grouped by a batch name. The name consists of a prefix set by the Pushpay download job (default is 'Pushpay') and a suffix 
                                        defined by the merchant's Batch Name Suffix setting (default is the transaction's settlement Name). Enable this option if the name should also include the 
                                        transaction's currency or credit card type abbreviation. Enabling this option will result in transactions with different currency types being grouped into
                                        different batches (see examples below)." />
                                </div>
                            </div>
                            <div class="well">
                                <h4>Example Batch Names</h4>
                                <div class="row">
                                    <div class="col-sm-6">
                                        <p><strong>With Currency Type in name</strong><br />
                                            Pushpay ACH - ACH201800701<br />
                                            Pushpay VISA - FD20180701
                                        </p>
                                    </div>
                                    <div class="col-sm-6">
                                        <p><strong>Without Currency Type in name</strong><br />
                                            Pushpay - ACH201800701<br />
                                            Pushpay - FD20180701
                                        </p>
                                    </div>
                                </div>
                            </div>
                            <Rock:RockCheckBox ID="cbMoveUpdatedTxns" runat="server" Label="Move Updated Transactions" Text="Yes" Checked="true"
                                Help="When a transaction is updated by Pushpay (i.e. it settles), the calculated batch name may be different than the transaction's
                                original calculated batch name. If you enable this option, the transaction will be moved to a new batch based on it's new calculated 
                                batch name and removed from the previous batch. This only affects transactions that are in an open batch. If the updated transaction 
                                is in a batch that has already been closed, it will not be moved to a new batch." />
                        </Rock:PanelWidget>

                        <Rock:PanelWidget ID="pwAdvancedAPI" runat="server" Title="API Urls">
                            <Rock:RockTextBox ID="tbAuthorizationUrl" runat="server" Label="Authorization Server URL" Required="true" ValidationGroup="AccountEdit"
                                Help="The URL of the Pushpay Authorization Server (default: https://auth.pushpay.com/pushpay/)" />
                            <Rock:RockTextBox ID="tbApiUrl" runat="server" Label="API Server URL" Required="true" ValidationGroup="AccountEdit"
                                Help="The URL of the Pushpay API (default: https://api.pushpay.com)"  />
                            <Rock:RockTextBox ID="tbAuthorizationRedirectUrl" runat="server" Label="Redirect URL" Required="true" ValidationGroup="AccountEdit"
                                Help="The URL on your server that Pushpay should redirect user back to after granting access on the Pushpay site (i.e. https://rock.rocksolidchurchdemo.com/pushpayredirect)."  />
                        </Rock:PanelWidget>

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
