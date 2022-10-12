<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CsvImport.ascx.cs" Inherits="RockWeb.Blocks.BulkImport.CsvImport" %>

<script src="/SignalR/hubs"></script>
<script type="text/javascript">
    $(function () {
        var proxy = $.connection.rockMessageHub;

        proxy.client.receiveUploadedCSVInvalidException = function (name, exceptionMessage) {
            if (name != '<%=this.SignalRNotificationKey %>' || !exceptionMessage) {
                return;
            }
            $('#upload-csv-invalid-exception-notification').html(exceptionMessage);
            $('#upload-csv-invalid-exception-notification').show();
        }

        proxy.client.receiveCSVNotification = function (name, message, results, hasErrors) {
            if (name == '<%=this.SignalRNotificationKey %>') {
                if (message) {
                    $('#<%=lImportDataMessage.ClientID %>').html(message);
                    $("#import-data-message-container").show();

                    // Motive: As of the writing there was no easy way to get the totalCount and the CompletedCount from
                    // the server to display the progress bar. Thus, we decided to parse the message string to get the values

                    var messageCharArray = message.split(' ');
                    var completedCount = parseInt(messageCharArray[3]);
                    var totalCount = parseInt(messageCharArray[5]);

                    var $bar = $('#<%= pnlImportDataProgress.ClientID %> .js-progress-bar');

                    if (!isNaN(completedCount) && !isNaN(totalCount)) {
                        let percentageComplete = (completedCount / totalCount * 100).toFixed();

                        $bar.prop('aria-valuenow', completedCount);
                        $bar.prop('aria-valuemax', totalCount);
                        $bar.css('width', percentageComplete + '%');
                        $bar.text(percentageComplete + '%');
                    }
                }

                if (results) {
                    $("#import-data-message-container").hide();

                    // Show full progress bar - Motive: as of the writing the Slingshot does not call onProgress event handler
                    // when the import completes execution.
                    var $bar = $('#<%= pnlImportDataProgress.ClientID %> .js-progress-bar');
                    $bar.prop('aria-valuenow', totalCount);
                    $bar.css('width', '100%');
                    $bar.text('100%');

                    if (hasErrors) {
                        $('#import-csv-error').show();
                    }
                    else {
                        $('#import-csv-success').show();
                    }
                }
            }
        }

        proxy.client.receiveCSVLineReadNotification = function (name, readLineCount, totalCount) {
            if (name != '<%=this.SignalRNotificationKey %>') {
                return;
            }
            let percentageComplete = (readLineCount / totalCount * 100).toFixed();

            var $bar = $('#<%= pnlImportPreaprationProgress.ClientID %> .js-progress-bar');
            $bar.prop('aria-valuenow', readLineCount);
            $bar.prop('aria-valuemax', totalCount);
            $bar.css('width', percentageComplete + '%');
            $bar.text(percentageComplete + '%');
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
                <h1 class="panel-title pull-left">
                    <asp:Literal ID="lHeading" runat="server" Text="CSV Import" />
                </h1>
            </div>

            <div class="panel-body">

                <%-- The very first page which accepts the csv file as the input ---%>
                <asp:Panel ID="pnlLandingPage" runat="server" Visible="true">

                    <div class="row">
                        <div class="col-md-7">

                             <div class="rock-header">
                                <h3 class="title">Comma Separated File Import</h3>
                                <span class="description">The first step is to upload your comma delimited file. We'll then allow you to map the columns to fields in Rock. The first
                                    row of your file must contain headers for each column.
                                </span>
                                <hr class="section-header-hr">
                            </div>

                            <Rock:RockDropDownList
                                ID="ddlDataType"
                                runat="server"
                                Label="Data Type" />

                            <Rock:RockRadioButtonList
                                ID="rblpreviousSourceDescription"
                                runat="server"
                                Label="Previous Source Descriptions"
                                RepeatDirection="Horizontal"
                                Required="true"
                                Help="If you are importing data from a source that has already been run once, select the matching description below. Otherwise, if this is a different source than those shown, chose to add a new source description." />

                            <asp:LinkButton ID="lbAddSourceDescription"
                                runat="server"
                                CssClass="mb-4"
                                Text="<small>Add Additional Source Description</small>"
                                OnClick="lbAddSourceDescription_Click"
                                CausesValidation="false" />

                            <Rock:RockTextBox
                                ID="tbpreviousSourceDescription"
                                runat="server"
                                Label="Source Description"
                                Help="Describe where this data came from. We'll store what you type as a setting so if you import data from this system again we'll be able to match people you've already imported. (e.g.: 'former-chms', 'mailchimp', etc.)"
                                Visible="false" />

                            <asp:LinkButton ID="lbChooseSourceDescription"
                                runat="server"
                                CssClass="mb-4"
                                Text="<small>Choose Previous Source Description</small>"
                                OnClick="lbChooseSourceDescription_Click"
                                Visible="false"
                                CausesValidation="false" />

                            <Rock:NotificationBox ID="nbDuplicateHeadersInFile"
                                runat="server"
                                CssClass="mt-4"
                                NotificationBoxType="Danger"
                                Visible="false" />

                            <Rock:FileUploader
                                ID="fupCSVFile"
                                runat="server"
                                FormGroupCssClass="mt-4"
                                OnFileUploaded="fupCSVFile_FileUploaded"
                                OnFileRemoved="fupCSVFile_FileRemoved"
                                RootFolder="~/App_Data/SlingshotFiles"
                                IsBinaryFile="false"
                                Label="CSV File" />

                            <asp:HiddenField ID="hfCSVFileName" runat="server" />

                            <Rock:RockCheckBox
                                ID="cbAllowUpdatingExisting"
                                runat="server"
                                FormGroupCssClass="mt-4"
                                Label="Allow Updating Existing Matched Records"
                                Checked="true"
                                Help="When checked, existing records that exist in the database that match date being imported (first name, last name and email match) will be updated. Otherwise only new records will be added to the database." />

                            <Rock:BootstrapButton ID="btnStart" runat="server" CssClass="btn btn-primary mt-3" Text="Start" OnClick="btnStart_Click" />
                        </div>

                        <%-- The description text on the right side of the page --%>
                        <div class="col-md-5">
                            <div class="well">
                                <h4 class="mt-0 mb-1">Required Fields </h4>
                                When uploading data about people you'll need to include the following information. These should be separate columns on your CSV
                                file. The name of the field doesn't matter as you'll be able to map it later.

                                <ol>
                                    <li>Id - Some form of unique identifier for the person. This should come from your former system.</li>
                                    <li>Family Id - This field should be an unique value for each family. This tells us who is in the same family.</li>
                                    <li>Family Role - This column should have the values of Adult or Child.</li>
                                    <li>First Name - The individual's first name.</li>
                                    <li>Last Name - The individual's last name.</li>
                                </ol>

                                <h4 class="mt-4 mb-1">Optional Fields</h4>
                                <p>You may optionally add the following fields.</p>

                                <ol>
                                    <li>Nick Name - The individual's nick name.</li>
                                    <li>Middle Name - The individual's middle name.</li>
                                    <li>Suffix - The person's suffix.
                                    <br />
                                        <asp:Label ID="lsuffixlist" runat="server" /></li>
                                    <li>Home Phone - The individual's home phone number.
                                    <br />
                                        (480-555-1234)</li>
                                    <li>Mobile Phone - The individuals's mobile phone number.
                                    <br />
                                        (480-555-1234)</li>
                                    <li>Is SMS Enabled - Whether the individual allows SMS messages to be sent to them.</li>
                                    <li>Email - The individual's email address.</li>
                                    <li>Email Preference - The permissions you have been granted to email the individual. 
                                    <br />
                                        <asp:Label ID="lemailPreferenceList" runat="server" /></li>
                                    <li>Gender - The gender of the individual.
                                    <br />
                                        <asp:Label ID="lgenderList" runat="server" /></li>
                                    <li>Marital Status - The marital status of the individual.
                                    <br />
                                        <asp:Label ID="lmaritalStatusList" runat="server" /></li>
                                    <li>Birthdate - The individual's birthdate. </li>
                                    <li>Anniversary Date - The marriage anniversary date of the individual.</li>
                                    <li>Record Status - Whether the person is active or not.
                                    <br />
                                        <asp:Label ID="lrecordStatusList" runat="server" /></li>
                                    <li>Inactive Reason - The reason why the person is inactive.</li>
                                    <li>Is Deceased - Determines whether the person is deceased.
                                    <br />
                                        (True,False)</li>
                                    <li>Connection Status - The connection type the individual has to your organization. This must match the connection statuses you have in Rock.
                                    <br />
                                        <asp:Label ID="lconnectionStatusList" runat="server" /></li>
                                    <li>Grade - The grade of the individual.
                                    <br />
                                        <asp:Label ID="lgrade" runat="server" /></li>
                                    <li>Home Address Street 1 - The first line of their home street address.</li>
                                    <li>Home Address Street 2 - The second line of their home street address.</li>
                                    <li>Home Address City - The city of their home address.</li>
                                    <li>Home Address State - The state of their home address.
                                    <br />
                                        (CA, AZ, etc.)</li>
                                    <li>Home Address Postal Code - The postal code of their home address.</li>
                                    <li>Home Address Country -  The country of their home address.
                                    <br />
                                        (US)</li>
                                    <li>Created Date Time - The date and time the record was originally created.
                                    <br />
                                        (1/1/2020 9:12:34 AM)</li>
                                    <li>Modified Date Time - The date and time the record was last modified.
                                    <br />
                                        (5/1/2020 9:12:34 AM)</li>
                                    <li>Note - A note that you would like to add to the person.</li>
                                    <li>Campus Id - The Rock campus id for the individual. Must supply the Campus Name if the Campus Id is given.</li>
                                    <li>Campus Name - The Rock campus name for the individual. Must supply the Campus Id if the Campus Name is given. This will update if it does not exist.</li>
                                    <li>Give Individually - Determines if the person gives with the family or as an individual.
                                    <br />
                                        (True, False) </li>
                                </ol>

                                <h4 class="mt-4 mb-1">Additional Fields</h4>
                                <p>
                                    You can provide as many other additional fields as you'd like. We'll allow you to match these to attributes in
                                    Rock. You'll want to make sure that these Rock attributes exist.
                                </p>
                                <p>
                                    You can also choose to ignore columns on your CSV file.
                                </p>
                            </div>
                        </div>

                        <%-- Pad some extra space to the right of the panel after the description text ---%>
                        <div class="col-md-6" />
                    </div>
                </asp:Panel>

                <%-- The very second page which helps map the CSV fields to the database schema ---%>
                <asp:Panel ID="pnlFieldMappingPage" runat="server" Visible="false">
                    <h2>Field Mapping</h2>
                    <p>We've uploaded your file to the server. Below is a listing of the fields you uploaded. You'll need to map these fields to those in Rock.</p>
                    <Rock:NotificationBox ID="nbImportMappingWarning"
                        runat="server"
                        NotificationBoxType="Warning"
                        Text="Please take your time on this screen and map the fields you want carefully.  There is no way to undo the import once you've finished." />
                    <hr>

                    <Rock:TermDescription ID="tdRecordCount" runat="server" Term="Record Count" />

                    <br />
                    <br />

                    <Rock:NotificationBox ID="nbRequiredFieldsNotPresentWarning"
                        runat="server"
                        NotificationBoxType="Validation"
                        Visible="false" />
                    <asp:Repeater ID="rptCSVHeaders" runat="server"
                        OnItemDataBound="rptCSVHeaders_ItemDataBound">
                        <ItemTemplate>
                            <div class="row">
                                <div class="col-md-4">
                                    <Rock:RockDropDownList
                                        ID="ddlCSVHeader"
                                        runat="server"
                                        Label='<%# Container.DataItem %>'
                                        AutoPostBack="True"
                                        OnSelectedIndexChanged="ddlCSVHeader_SelectedIndexChanged"
                                        EnhanceForLongLists="true" />
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>

                    <div class="footer-actions">
                        <Rock:BootstrapButton ID="btnImport"
                            runat="server"
                            CssClass="btn btn-primary"
                            Text="Import"
                            OnClick="btnImport_Click" />
                    </div>
                </asp:Panel>

                <%-- The third page that would display the progress bars ---%>
                <asp:Panel ID="pnlProgress" runat="server" CssClass="js-messageContainer" Visible="false">
                    <h2>Import</h2>
                    We'll now start the import process with the data and mappings you have provided.

                <hr>
                    <div>
                        <span id="upload-csv-invalid-exception-notification"
                            class="js-notification-text alert alert-danger mb-5" hidden></span>
                    </div>

                    <asp:Panel ID="pnlImportPreaprationProgress" runat="server" CssClass="mb-5">
                        <h4>Step 1: Import Preparation</h4>
                        <div>First, we'll arrange your data into a format we need to import. </div>

                        <div class="progress-bar js-progress-bar" role="progressbar" style="width: 0%" aria-valuenow="0" aria-valuemax="0">0%</div>
                    </asp:Panel>

                    <asp:Panel ID="pnlImportDataProgress" runat="server" CssClass="mb-5">

                        <h4>Step 2: Import Data</h4>
                        Then, we'll import the data into the database.

                        <div id="import-data-message-container" class="alert alert-info" hidden>
                            <asp:Label ID="lImportDataMessage" CssClass="js-progressMessage" runat="server" />
                        </div>

                        <div class="progress-bar js-progress-bar" role="progressbar" style="width: 0%" aria-valuenow="0" aria-valuemax="0">0%</div>
                        
                    </asp:Panel>
                    <div id="import-csv-success" hidden>
                        <Rock:NotificationBox ID="nbImportSuccess"
                            runat="server"
                            NotificationBoxType="Success"
                            Heading="Success!"
                            Text="Your data has been imported into Rock." />
                    </div>
                    <div id="import-csv-error" hidden>
                        <Rock:NotificationBox ID="nbImportError"
                            runat="server"
                            NotificationBoxType="Warning"
                            Text="Some records could not be imported.  Click the button to download a .csv file that includes a new “CSV Import Errors” column.  You can correct those rows and retry importing. " />
                        <asp:LinkButton ID="btnDownloadErrorCSV"
                            runat="server"
                            CssClass="btn btn-primary"
                            Text="Download CSV File With the Errors"
                            OnClick="btnDownloadErrorCSV_Click"
                            CausesValidation="false" />
                    </div>
                </asp:Panel>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
