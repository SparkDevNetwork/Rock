<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GatewayMigrationUtility.ascx.cs" Inherits="RockWeb.Blocks.Finance.GatewayMigrationUtility" %>

<script src="/SignalR/hubs"></script>
<script type="text/javascript">
    $(function () {
        var proxy = $.connection.rockMessageHub;

        proxy.client.showProgress = function (name, message, results) {
            if (name == '<%=this.SignalRNotificationKey %>') {

                $('.js-migrate-log').show();
                $('.js-migrate-log').html('<p>'+ message + '</p>' + results);
            }
        }

        proxy.client.setButtonVisibilty = function (name, visible) {
            if (name == '<%=this.SignalRNotificationKey %>') {

                if (visible) {
                    $('.js-import-button').show();
                }
                else {
                    $('.js-import-button').hide();
                }
            }
        }

        $.connection.hub.start().done(function () {
            // hub started... do stuff here if you want to let the user know something
        });
    })
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
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
                        <h3>Preparing for the NMI to Pi Migration</h3>
                        <p>
                            To prepare for the migration, you'll need to contact Pi to get the process started. They will create two files that you'll use to import: a Vault import and a Scheduled Transaction import.
                            <br />
                            <b>You will need to carefully plan all these steps to prevent any NMI scheduled transactions or new NMI saved accounts from happening during this process:</b>
                            <br />
                        </p>

                        <ol>
                            <li>Enable the Pi Gateway on your system.</li>
                            <li>Replace any Transaction Entry blocks and Scheduled Transaction Edit blocks with the "V2" versions of these blocks that you want to use the Pi Gateway.
                                Registration will need to continue to use the NMI gateway, but people's saved accounts will no longer be available to choose from (they'll have to enter Credit Card information every time).</li>
                            <li>Disable saved account and scheduled transaction options on any remaining Transaction Entry blocks that use the NMI gateway.</li>
                            <li>Run the 'Download Payments' job to get any NMI scheduled transactions that have occurred.</li>
                            <li>Get freshly generated Import files from Pi.</li>
                            <li>Run the migrations immediately after getting the import files.</li>
                            <li>Monitor scheduled transactions for the next couple of days to ensure that no new NMI scheduled transactions occur. If they do, refund any duplicate schedule transactions that might have happened to prevent people from getting double billed.</li>
                        </ol>

                        <hr />

                        <Rock:RockDropDownList ID="ddlNMIGateway" Label="NMI Gateway" runat="server" />
                        <Rock:RockDropDownList ID="ddlPiGateway" Label="Pi Gateway" runat="server" />

                        <hr />

                        <h4>Saved Accounts</h4>
                        <p>
                            This will migrate Saved Accounts from NMI to Pi. To start the migration, select the Vault Import file that was sent to you from Pi.
                            Make sure the Vault Import file was generated <b>after</b> the last NMI Saved Account was saved.
                            Click the 'Migrate Saved Accounts' button to start.
                            It is safe to run this import more than once. It will only affect records that weren't successfully migrated previously.
                        </p>

                        <Rock:FileUploader ID="fuCustomerVaultImportFile" runat="server" Label="Select Customer Vault Import File" IsBinaryFile="true" UploadAsTemporary="true" DisplayMode="DropZone" OnFileUploaded="fuCustomerVaultImportFile_FileUploaded" />
                        <div class="actions margin-b-md">
                            <asp:LinkButton ID="btnMigrateSavedAccounts" runat="server" CssClass="btn btn-primary" OnClick="btnMigrateSavedAccounts_Click" Enabled="false">Migrate Saved Accounts</asp:LinkButton>
                        </div>
                        <Rock:NotificationBox ID="nbMigrateSavedAccounts" runat="server" NotificationBoxType="Success" Dismissable="true" />

                        <hr />

                        <h4>Scheduled Transactions</h4>
                        <p>
                            This will migrate Schedule Transactions from NMI to Pi. <b>After each scheduled transaction is migrated to Pi, the NMI subscription will be deleted from the NMI gateway.</b>. To start the migration, select the Schedule Transactions Import file that was sent to you from Pi.
                            Make sure the Scheduled Import file was generated <b>after</b> the last NMI Scheduled Transaction was created.
                            Click the 'Migrate Scheduled Transactions' button to start. Depending on the number of scheduled transactions on your system, this may take a few minutes.
                            It is safe to run this import more than once. It will only affect records that weren't successfully migrated previously.
                        </p>
                        <Rock:FileUploader ID="fuScheduleImportFile" runat="server" Label="Select Schedule Import File" IsBinaryFile="true" UploadAsTemporary="true" DisplayMode="DropZone" OnFileUploaded="fuScheduleImportFile_FileUploaded" />
                        <div class="actions margin-b-md">
                            <asp:LinkButton ID="btnMigrateScheduledTransactions" runat="server" CssClass="btn btn-primary js-import-button" OnClick="btnMigrateScheduledTransactions_Click" Enabled="false">Migrate Scheduled Transactions</asp:LinkButton>
                        </div>
                        <Rock:NotificationBox ID="nbMigrateScheduledTransactions" runat="server" CssClass="js-migrate-scheduled-notification" NotificationBoxType="Info" Visible="true" Dismissable="true" />
                        <pre class="js-migrate-log" style="display:none"></pre>
                    </div>
                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
