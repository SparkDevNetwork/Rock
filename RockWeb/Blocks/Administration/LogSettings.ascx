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
                <Rock:NotificationBox ID="nbLoggingMessage" runat="server" NotificationBoxType="Warning" Visible="false" />

                <asp:Panel runat="server" ID="pnlReadOnlySettings">
                    <Rock:RockLiteral runat="server" ID="litVerbosityLevel" Label="Verbosity Level" CssClass="col-sm-3" />
                    <Rock:RockLiteral runat="server" ID="litDomains" Label="Domains" CssClass="col-sm-9" />
                    <asp:Button runat="server" ID="btnEdit" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                </asp:Panel>

                <asp:Panel runat="server" ID="pnlEditSettings" Visible="false">
                    <Rock:RockRadioButtonList ID="rblVerbosityLevel" runat="server"
                        Label="Verbosity Level"
                        Help="The specified value indicates which logging level events should be written to the log file."
                        RepeatDirection="Horizontal"
                        ValidationGroup="LoggingSettings">
                    </Rock:RockRadioButtonList>

                    <Rock:RockCheckBoxList ID="cblDomainsToLog" runat="server"
                        Label="Domains to Output"
                        ValidationGroup="LoggingSettings"
                        RepeatColumns="5"
                        RepeatDirection="Horizontal" />

                    <Rock:NumberBox runat="server" ID="txtMaxFileSize" Label="Max File Size (MB)"
                        Help="The maximum size that the output file is allowed to reach before being rolled over to backup files."
                        CssClass="input-width-md js-max-file-size"
                        ValidationGroup="LoggingSettings"></Rock:NumberBox>

                    <Rock:NumberBox runat="server" ID="txtFilesToRetain" Label="Retained Backup Files"
                        Help="The maximum number of backup files that are kept before the oldest is erased."
                        CssClass="input-width-md js-files-to-retain"
                        ValidationGroup="LoggingSettings"></Rock:NumberBox>

                    <p>Logs could take up to <span id="maxLogSize">400</span> MB on the server's filesystem.</p>

                    <div class="actions margin-t-lg">
                        <Rock:BootstrapButton
                            ID="btnLoggingSave"
                            runat="server"
                            CssClass="btn btn-primary"
                            Text="Save"
                            DataLoadingText="Saving..."
                            ValidationGroup="LoggingSetting"
                            OnClick="btnLoggingSave_Click" />

                        <Rock:BootstrapButton
                            ID="btnDeleteLog"
                            runat="server"
                            CssClass="btn btn-link"
                            Text="Delete Log"
                            DataLoadingText="Deleting Log ..."
                            OnClick="btnDeleteLog_Click" />
                    </div>
                </asp:Panel>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
