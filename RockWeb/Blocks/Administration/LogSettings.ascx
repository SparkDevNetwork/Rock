<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LogSettings.ascx.cs" Inherits="RockWeb.Blocks.Administration.LogSettings" %>
<asp:UpdatePanel runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdAlert" runat="server" />
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-wrench"></i>
                    Log Settings
                </h1>
            </div>
            <div class="panel-body">
                <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <Rock:NotificationBox ID="nbLoggingMessage" runat="server" NotificationBoxType="Warning" Visible="false" />

                <asp:Panel runat="server" ID="pnlReadOnlySettings">
                    <div class="row">
                        <Rock:RockLiteral runat="server" ID="litVerbosityLevel" Label="Verbosity Level" CssClass="col-md-3 col-lg-6" />
                        <Rock:RockLiteral runat="server" ID="litCategories" Label="Categories" CssClass="col-md-9 col-lg-6" />
                    </div>

                    <div class="row">
                        <Rock:RockLiteral runat="server" ID="litLocalFileSystem" Label="Local File System" CssClass="col-md-6" />
                        <Rock:RockLiteral runat="server" ID="litObservability" Label="Observability" CssClass="col-md-6" />
                    </div>

                    <div class="actions">
                        <asp:Button runat="server" ID="btnEdit" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />

                        <Rock:BootstrapButton
                            ID="btnDeleteLog"
                            runat="server"
                            CssClass="btn btn-link"
                            Text="Delete Log"
                            DataLoadingText="Deleting Log ..."
                            OnClick="btnDeleteLog_Click" />

                    </div>
                </asp:Panel>

                <asp:Panel runat="server" ID="pnlEditSettings" Visible="false">
                    <Rock:RockRadioButtonList ID="rblVerbosityLevel" runat="server"
                        Label="Verbosity Level"
                        Help="The specified value indicates which logging level events should be written to the log file."
                        RepeatDirection="Horizontal"
                        ValidationGroup="LoggingSettings">
                    </Rock:RockRadioButtonList>

                    <Rock:RockListBox ID="rlbCategoriesToLog" runat="server"
                        Label="Categories to Log"
                        ValidationGroup="LoggingSettings"
                        DisplayDropAsAbsolute="true" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbLogToLocal" runat="server"
                                Label="Log to Local File System"
                                Help="Enables writing logs to the local file system of the Rock server."
                                OnCheckedChanged="cbLogToLocal_CheckedChanged"
                                CausesValidation="false"
                                AutoPostBack="true"/>
                        </div>

                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbLogToObservability" runat="server"
                                Label="Log to Observability"
                                Help="Enables writing logs to the observability framework, this is recommended when running in a web farm." />
                        </div>
                    </div>

                    <Rock:PanelWidget ID="wpLocalSettings" runat="server" Title="Local File System Settings">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:NumberBox runat="server" ID="nbMaxFileSize" Label="Max File Size (MB)"
                                    NumberType="Integer" MinimumValue="1"
                                    Help="The maximum size that the output file is allowed to reach before being rolled over to backup files."
                                    CssClass="input-width-md js-max-file-size"
                                    ValidationGroup="LoggingSettings"></Rock:NumberBox>
                            </div>

                            <div class="col-md-6">
                                <Rock:NumberBox runat="server" ID="nbFilesToRetain" Label="Retained Backup Files"
                                    NumberType="Integer" MinimumValue="1"
                                    Help="The maximum number of backup files that are kept before the oldest is erased."
                                    CssClass="input-width-md js-files-to-retain"
                                    ValidationGroup="LoggingSettings"></Rock:NumberBox>
                            </div>
                        </div>

                        <p>Logs could take up to <span id="maxLogSize">400</span> MB on the server's filesystem.</p>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpAdvanced" runat="server" Title="Advanced">
                        <Rock:CodeEditor ID="ceCustomConfiguration"
                            runat="server"
                            Label="Custom Configuration"
                            Help="This allows custom configuration by way of writing a JSON object that will be passed to the configuration parser. Example:<pre><small><small>{
  &quot;LogLevel&quot;: {
    &quot;CMS&quot;: &quot;Information&quot;,
    &quot;org.rsc.MyClass&quot;: &quot;Error&quot;
  }
}</small></small></pre>"
                            EditorMode="JavaScript"
                            />
                    </Rock:PanelWidget>

                    <div class="actions">
                        <Rock:BootstrapButton
                            ID="btnLoggingSave"
                            runat="server"
                            CssClass="btn btn-primary"
                            Text="Save"
                            DataLoadingText="Saving..."
                            ValidationGroup="LoggingSetting"
                            OnClick="btnLoggingSave_Click" />
                        <asp:LinkButton
                            ID="btnCancel"
                            runat="server"
                            AccessKey="c"
                            ToolTip="Alt+c"
                            Text="Cancel"
                            CssClass="btn btn-link"
                            CausesValidation="false"
                            OnClick="btnCancel_Click" />
                    </div>
                </asp:Panel>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
