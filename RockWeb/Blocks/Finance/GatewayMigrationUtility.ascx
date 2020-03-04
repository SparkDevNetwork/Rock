<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GatewayMigrationUtility.ascx.cs" Inherits="RockWeb.Blocks.Finance.GatewayMigrationUtility" %>

<script src="/SignalR/hubs"></script>
<script type="text/javascript">
    $(function () {
        var proxy = $.connection.rockMessageHub;

        proxy.client.showProgress = function (name, message, results, jsTarget) {
            if (name == '<%=this.SignalRNotificationKey %>') {

                var $notificationBox = $(jsTarget);

                $notificationBox.show();

                $('.js-notification-text', $notificationBox).html(message);
                if (results != '') {
                    $('.js-notification-details', $notificationBox).html('<pre>' + results + '</pre>');
                }
                else {
                    $('.js-notification-details', $notificationBox).html('');
                }
            }
        }

        proxy.client.setMigrateScheduledTransactionsButtonVisibility = function (name, visible) {
            if (name == '<%=this.SignalRNotificationKey %>') {
                if (visible) {
                    $('.js-migrate-scheduled-transactions-button').show();
                    $('.js-migrate-scheduled-download-button').show();
                }
                else {
                    $('.js-migrate-scheduled-transactions-button').hide();
                    $('.js-migrate-scheduled-download-button').hide();
                }
            }
        }

        $.connection.hub.start().done(function () {
            // hub started... do stuff here if you want to let the user know something
        });
    })
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <Triggers>
        <asp:PostBackTrigger ControlID="btnDownloadSavedAccountsResultsJSON" />
        <asp:PostBackTrigger ControlID="btnDownloadScheduledTransactionsResultsJSON" />
        <asp:PostBackTrigger ControlID="btnExportVaultTransferRequestFiles" />
    </Triggers>
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-credit-card"></i>
                    Gateway Migration Utility
                </h1>
            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col-md-12">
                        <h3>Preparing for the NMI to My Well Migration</h3>
                        <p>
                            To prepare for the migration, you'll need to <b>contact My Well to get the process started.</b>
                            <br />
                            <br />
                            <b>You will need to carefully plan all these steps to prevent any NMI scheduled transactions or new NMI saved accounts from happening during this process:</b>
                            <br />
                        </p>

                        <ol>
                            <li>Make sure you have contacted My Well to do the Vault Transfer. If you want to limit which vault records are transfered, do the Export Vault Transfer Request files step below</li>
                            <li>Enable the My Well Gateway on your system.</li>
                            <li>Replace any Transaction Entry blocks and Scheduled Transaction Edit blocks with the "V2" versions of these blocks that you want to use the My Well Gateway.
                                Registration will need to continue to use the NMI gateway, but people's saved accounts will no longer be available to choose from (they'll have to enter Credit Card information every time).</li>
                            <li>Disable saved account and scheduled transaction options on any remaining Transaction Entry blocks that use the NMI gateway.</li>
                            <li>Get confirmation from My Well that VaultIds have been transferred from NMI to My Well</li>
                            <li>Run the 'Download Payments' job to get any NMI scheduled transactions that have occurred.</li>
                            <li>Run the migration operations
                            <ul>
                                <li>The migration operations can be run in any order, and can safely be run multiple times. They are not dependent on each other.</li>
                            </ul>
                            </li>
                            <li>Monitor scheduled transactions for the next couple of days to ensure that no new NMI scheduled transactions occur. If they do, refund any duplicate schedule transactions that might have happened to prevent people from getting double billed.</li>
                        </ol>

                        <hr />

                        <Rock:RockDropDownList ID="ddlNMIGateway" Label="NMI Gateway" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlNMIGateway_SelectedIndexChanged" />
                        <Rock:RockDropDownList ID="ddlMyWellGateway" Label="My Well Gateway" runat="server" />

                        <h4>Vault Transfer Request Files</h4>
                        <p>
                            Use this to export a zip file that will contain the Ids that MyWell will use to limit which customer vault records are transferred. This will export vault IDs for any scheduled transactions or saved accounts that have been active or created within the selected date range.
                        </p>
                        <Rock:SlidingDateRangePicker ID="sdrDateRange" runat="server" DelimitedValues="Last|12|Month||" Help="Select the date range " />
                        <div class="actions margin-b-md">
                            <asp:LinkButton ID="btnExportVaultTransferRequestFiles" runat="server" CssClass="btn btn-primary" OnClick="btnExportVaultTransferRequestFiles_Click" Enabled="true" Text="Export Vault Transfer Request Files" />
                        </div>

                        <Rock:RockControlWrapper ID="rcbNMIPersonFiles" runat="server" Label="Profiles" Help="Select the Profiles to migrate, or leave all un-selected to migrate all">
                            <Rock:Grid ID="gNMIPersonProfiles" runat="server" OnRowDataBound="gNMIPersonProfiles_RowDataBound" DataKeyNames="Id" AllowSorting="true" Label="People with NMI Profiles">
                                <Columns>
                                    <Rock:SelectField />
                                    <Rock:RockLiteralField ID="lPerson" SortExpression="Person.LastName,Person.FirstName" HeaderText="Person" />
                                    <Rock:RockLiteralField ID="lPersonSavedAccounts" HeaderText="Saved Accounts" />
                                    <Rock:RockLiteralField ID="lPersonScheduledTransactions" HeaderText="Scheduled Transactions" />
                                </Columns>
                            </Rock:Grid>
                        </Rock:RockControlWrapper>

                        <hr />

                        <h4>Saved Accounts</h4>
                        <p>
                            This will migrate Saved Accounts from NMI to My Well. It does this by modifying each Saved Account to use the My Well CustomerId instead of the NMI CustomerId (nothing in either gateway is modified).
                            Click the 'Migrate Saved Accounts' button to start.
                            It is safe to run this import more than once. It will only affect records that weren't successfully migrated previously.
                        </p>

                        <div class="actions margin-b-md">
                            <Rock:BootstrapButton ID="btnMigrateSavedAccounts" runat="server" CssClass="btn btn-primary" OnClick="btnMigrateSavedAccounts_Click" Enabled="true">Migrate Saved Accounts</Rock:BootstrapButton>
                        </div>

                        <asp:Panel ID="pnlMigrateSavedAccountsResults" runat="server">
                            <Rock:NotificationBox ID="nbMigrateSavedAccountsResults" runat="server" NotificationBoxType="Success" Dismissable="true" />
                            <asp:HiddenField ID="hfMigrateSavedAccountsResultFileURL" runat="server" />
                            <asp:LinkButton ID="btnDownloadSavedAccountsResultsJSON" runat="server" CssClass="btn btn-link" Text="Download Log" OnClick="btnDownloadSavedAccountsResultsJSON_Click" />
                        </asp:Panel>

                        <hr />

                        <h4>Scheduled Transactions</h4>
                        <p>
                            This will migrate Schedule Transactions from NMI to My Well. <b>After each scheduled transaction is migrated to My Well, the NMI subscription will be deleted from the NMI gateway.</b>
                            First, click the 'Download NMI Payments' to get any recent NMI payments that haven't been downloaded to Rock yet.
                            Then click the 'Migrate Scheduled Transactions' button to start. Depending on the number of scheduled transactions on your system, this may take a few minutes.
                            It is safe to run this import more than once. It will only affect records that weren't successfully migrated previously.
                        </p>

                        <Rock:NotificationBox ID="nbScheduledTransactionsDownloadPayments" runat="server" NotificationBoxType="Warning" />

                        <div class="actions margin-b-md">
                            <Rock:BootstrapButton ID="btnMigrateScheduledTransactions" runat="server" CssClass="btn btn-primary margin-t-md js-migrate-scheduled-transactions-button" OnClick="btnMigrateScheduledTransactions_Click" Enabled="true">Migrate Scheduled Transactions</Rock:BootstrapButton>
                        </div>
                        <asp:Panel ID="pnlMigrateScheduledTransactionsResults" runat="server">
                            <Rock:NotificationBox ID="nbMigrateScheduledTransactionsResult" runat="server" CssClass="js-migrate-scheduled-notification" NotificationBoxType="Info" Visible="true" Dismissable="true" />
                            <asp:HiddenField ID="hfMigrateScheduledTransactionsResultFileURL" runat="server" />
                            
                            <asp:LinkButton ID="btnDownloadScheduledTransactionsResultsJSON" runat="server" CssClass="btn btn-link js-migrate-scheduled-download-button" Text="Download Log" OnClick="btnDownloadScheduledTransactionsResultsJSON_Click" />
                        </asp:Panel>

                        <hr />

                        <h4>Cleanup Customer Vault</h4>
                        <p>
                            This will remove email addresses from all the customer vault records in My Well that have a subscription.
                            Since Rock has already has customizable email notifications for gateway transactions, Rock doesn't include email addresses in the data that is sent to My Well.
                            However the customer vault transfer does include email, so we want to delete those email addresses.
                            NOTE: This needs to be down after the scheduled transactions are migrated to MyWell.
                        </p>
                        <div class="actions margin-b-md">
                            <Rock:BootstrapButton ID="btnRemoveEmailAddresses" runat="server" CssClass="btn btn-primary " OnClick="btnRemoveEmailAddresses_Click" Enabled="true">Remove email addresses from Customer Vault</Rock:BootstrapButton>
                            <Rock:NotificationBox ID="nbRemoveEmailAddressesResult" runat="server" CssClass="js-remove-emails-notification" NotificationBoxType="Info" Visible="true" Dismissable="true" />
                        </div>

                        <hr />

                        <h4>Update One-Time Schedule Status</h4>
                        <p>
                            This will go thru all active One-Time schedules (both NMI and MyWell) and get the most recent status. If the one-time schedule has been completed, the schedule will be updated to inactive.
                        </p>

                        <Rock:NotificationBox ID="nbUpdateOneTimeScheduleStatus" runat="server" NotificationBoxType="Info" Visible="true" Dismissable="true" />
                        <div class="actions margin-b-md">
                            <Rock:BootstrapButton ID="btnUpdateOneTimeScheduleStatus" runat="server" CssClass="btn btn-primary" OnClick="btnUpdateOneTimeScheduleStatus_Click" >Update One-Time schedule status</Rock:BootstrapButton>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
