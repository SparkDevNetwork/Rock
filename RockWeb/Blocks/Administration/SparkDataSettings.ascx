<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SparkDataSettings.ascx.cs" Inherits="RockWeb.Blocks.Administration.SparkDataSettings" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=mdRunNcoa.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        <Rock:ModalDialog ID="mdRunNcoa" runat="server" Title="Run NCOA Manually" SaveButtonText="Run" OnSaveClick="mdRunNcoa_SaveClick" OnCancelScript="clearActiveDialog();">
            <Content>
                <p><asp:Label ID="lbNcoaCount" runat="server"></asp:Label></p>
                <p>Are you sure you want to run the NCOA service?</p>
                <small>
                    Note:
                    <ul>
                        <li>This service will charge the card on file for each run.</li>
                        <li>All the addresses that match the person data view will be automatically updated.</li>
                    </ul>
                </small>
            </Content>
        </Rock:ModalDialog>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-database"></i> Spark Data
                </h1>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />

                <fieldset>

                    <asp:Panel ID="pnlSignIn" runat="server" Visible="false">
                        <div class="row">
                            <div class="col-md-12">
                                <div class="row">
                                    <div class="col-md-2">
                                        <img src='<%= ResolveRockUrl( "~/Blocks/Administration/Assets/spark.png") %>' class="img-responsive" alt="Spark Data" />
                                    </div>
                                    <div class="col-md-10">
                                        <h2 class="margin-t-none">Enhance Your Data</h2>

                                        <p> Spark Data is a set of services that allows you to easily clean and enhance your data with
                                        little effort. Before you can begin you’ll need to get an API key from the Rock RMS website
                                        and ensure that a credit card is on file for use with paid services.</p>
                                        <p><a href="https://www.rockrms.com/sparkdatalink" target="_blank">Sign Up Now</a></p>

                                        <asp:ValidationSummary ID="vsSignIn" runat="server" HeaderText="Please Correct the Following" ValidationGroup="SignInValidationGroup" CssClass="alert alert-validation" />
                                        <div class="row">
                                            <div class="col-md-6">
                                                <Rock:RockTextBox ID="txtSparkDataApiKeyLogin" runat="server" Label="Spark Data API Key" Required="true" ValidationGroup="SignInValidationGroup"/>
                                            </div>
                                            <div class="col-md-6">
                                                <Rock:GroupPicker ID="grpNotificationGroupLogin" runat="server" Label="Notification Group" Help="Members of this group will receive notifications when specific jobs and tasks complete." />
                                            </div>
                                        </div>
                                        <asp:LinkButton ID="btnSaveLogin" runat="server" CssClass="btn btn-primary" OnClick="btnSaveLogin_Click" ValidationGroup="SignInValidationGroup" >Save</asp:LinkButton>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </asp:Panel>

                    <asp:Panel ID="pnlSparkDataEdit" runat="server" Visible="false">
                        <p> For more information about your account, or to update your payment information please visit your organization's profile on the
                        Rock RMS website.</p>
                        <p><a href="https://www.rockrms.com/sparkdatalink" target="_blank">Organization Profile</a></p>
                        <asp:ValidationSummary ID="vsSparkDataEdit" runat="server" HeaderText="Please Correct the Following" ValidationGroup="SparkDataEditValidationGroup" CssClass="alert alert-validation" />
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="txtSparkDataApiKeyEdit" runat="server" Label="Spark Data API Key" Required="true" ValidationGroup="SparkDataEditValidationGroup" />
                            </div>
                            <div class="col-md-6">
                                <Rock:GroupPicker ID="grpNotificationGroupEdit" runat="server" Label="Notification Group" Help="Members of this group will receive notifications when specific jobs and tasks complete." />
                            </div>
                        </div>
                        <asp:LinkButton ID="btnSaveEdit" runat="server" CssClass="btn btn-primary" OnClick="btnSaveEdit_Click" ValidationGroup="SparkDataEditValidationGroup" >Save</asp:LinkButton>
                        <asp:LinkButton ID="btnCancelEdit" runat="server" CssClass="btn btn-default" OnClick="btnCancelEdit_Click">Cancel</asp:LinkButton>
                    </asp:Panel>

                    <asp:Panel ID="pnlAccountStatus" runat="server" CssClass="panel panel-widget">
                        <header class="panel-heading clearfix">
                            <div class="js-header-title pull-left">
                            <h3 class="panel-title pull-left margin-r-sm">Spark Data Status</h3>
                                    <Rock:HighlightLabel ID="hlAccountStatus" runat="server" LabelType="Success" Text="" />
                            </div>
                        <div class="pull-right">
                        <asp:LinkButton ID="btnUpdateSettings" runat="server" CssClass="btn btn-default btn-xs" OnClick="btnUpdateSettings_Click" >Update Settings</asp:LinkButton>
                            </div>
                            </header>
                    </asp:Panel>

                    <Rock:PanelWidget ID="pwNcoaConfiguration" runat="server" Title="National Change of Address (NCOA)">
                        <Rock:NotificationBox ID="nbNcoaCreditCard" runat="server" NotificationBoxType="Warning"
                            Heading="Note" Text=" This service requires a credit card on file to process payments for running the files."/>
                        <asp:ValidationSummary ID="vsNcoa" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-validation" ValidationGroup="NcoaValidationGroup" />
                        <div class="row">
                            <div class="col-md-12">
                                <div class="pull-left">
                                    <Rock:RockCheckBox ID="cbNcoaConfiguration" runat="server"
                                        Label="Enable" Text="Enable the automatic updating of change of addresses via NCOA services."
                                        AutoPostBack="true" OnCheckedChanged="cbNcoaConfiguration_CheckedChanged" />
                                </div>
                                <div class="pull-right">
                                    <asp:LinkButton ID="lbStartNcoa" runat="server" CssClass="btn btn-default" OnClick="btnStartNcoa_Click" ToolTip="Start NCOA"><i class="fa fa-play"></i> Run Manually</asp:LinkButton>
                                </div>
                            </div>
                        </div>

                        <hr />
                        <asp:Panel ID="pnlNcoaConfiguration" runat="server" Enabled="false" CssClass="data-integrity-options">
                            <asp:CheckBox ID="cbNcoaAcceptTerms" runat="server" AutoPostBack="true" OnCheckedChanged="cbNcoaAcceptTerms_CheckedChanged" Text="By accepting these terms, you agree that Rock RMS may share your data with TrueNCOA for NCOA processing. You understand that through your use
                                of the Services you consent to the collection and use of this information, including the storage, processing and use by TrueNCOA and its affiliates. Customer
                                information will only be shared by TrueNCOA to provide or improve our products, services and advertising; it will not be shared with third parties for their
                                marketing purposes. Read TrueNCOA’s full Terms of Service here, and read TrueNCOA’s Privacy Policy &lt;a href='http://truencoa.com/privacy-policy/' target='_blank' &gt;here&lt;/a&gt;." />
                            <asp:CheckBox ID="cbNcoaAckPrice" runat="server" AutoPostBack="true" OnCheckedChanged="cbNcoaAckPrice_CheckedChanged" Text="I acknowledge that running this service will charge the card on file &#36;xx for each file run." />
                            <br />
                            <div class="row">
                                <div class="col-md-4">
                                    <Rock:DataViewItemPicker ID="dvpNcoaPersonDataView" runat="server" Label="Person Data View" Required="true" ValidationGroup="NcoaValidationGroup" Help="Person data view filter to apply." OnSelectItem="dvpNcoaPersonDataView_SelectItem" AutoPostBack="true" />
                                </div>
                            </div>

                            <hr />

                            <div class="row">
                                <div class="col-md-4">
                                    <Rock:NumberBox ID="nbNcoaMinMoveDistance" runat="server" AppendText="miles" CssClass="input-width-md" Label="Minimum Move Distance to Inactivate" NumberType="Double" Text="250" Help="Minimum move distance that a person moved before marking the person's account to inactivate. Leaving the value blank disables this feature." OnTextChanged="nbNcoaMinMoveDistance_TextChanged" AutoPostBack="true" />
                                </div>
                                <div class="col-md-4">
                                    <Rock:RockCheckBox ID="cbNcoa48MonAsPrevious" runat="server" Label="Mark 48 Month Move as Previous Addresses" Help="Mark moves in the 19-48 month catagory as a previous address." OnCheckedChanged="cbNcoa48MonAsPrevious_CheckedChanged" AutoPostBack="true" />
                                </div>
                                <div class="col-md-4">
                                    <Rock:RockCheckBox ID="cbNcoaInvalidAddressAsPrevious" runat="server" Label="Mark Invalid Addresses as Previous Addresses" Help="Mark Invalid Addresses as Previous Addresses" OnCheckedChanged="cbNcoaInvalidAddressAsPrevious_CheckedChanged" AutoPostBack="true" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-4">
                                    <Rock:DefinedValuePicker ID="dvpNcoaInactiveRecordReason" runat="server" Label="Inactive Record Reason" Help="The reason to use when inactivating people due to moving beyond the configured number of miles." Required="true" ValidationGroup="NcoaValidationGroup" OnSelectedIndexChanged="dvpNcoaInactiveRecordReason_SelectedIndexChanged" AutoPostBack="true" />
                                </div>
                            </div>

                            <hr />

                            <div class="row">
                                <div class="col-md-4">
                                    <Rock:RockCheckBox ID="cbNcoaRecurringEnabled" runat="server" Label="Recurring Enabled" OnCheckedChanged="cbNcoaRecurringEnabled_CheckedChanged" AutoPostBack="true" Help="Should the job run periodically"/>
                                </div>
                                <div class="col-md-4">
                                    <Rock:NumberBox ID="nbNcoaRecurrenceInterval" runat="server" AppendText="days" CssClass="input-width-md" Label="Recurrence Interval" NumberType="Integer" Text="95" Required="true" ValidationGroup="NcoaValidationGroup" Help="After how many days should the job automatically start after the last successful run" OnTextChanged="nbNcoaRecurrenceInterval_TextChanged" AutoPostBack="true" />
                                </div>
                            </div>
                            <div class="actions margin-t-lg">
                                <Rock:BootstrapButton ID="bbtnNcoaSaveConfig" runat="server" CssClass="btn btn-primary" AccessKey="s" ToolTip="Alt+s" OnClick="bbtnNcoaSaveConfig_Click" Text="Save" Enabled="false"
                                    DataLoadingText="&lt;i class='fa fa-refresh fa-spin'&gt;&lt;/i&gt; Saving" ValidationGroup="NcoaValidationGroup"
                                    CompletedText="Done" CompletedMessage="<div class='margin-t-md alert alert-success'>Changes have been saved.</div>" CompletedDuration="3">
                                </Rock:BootstrapButton>
                            </div>
                        </asp:Panel>
                    </Rock:PanelWidget>

                </fieldset>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
