<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FileEditor.ascx.cs" Inherits="RockWeb.Blocks.Cms.FileEditor" %>

<asp:UpdatePanel runat="server" ID="upnlContent">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <asp:HiddenField ID="hfRelativePath" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-pencil"></i> File Editor</h1>
            </div>
            <div class="panel-body">
                <asp:ValidationSummary ID="valValidation" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" Title="Warning" Visible="false" />

                <div id="pnlEditDetails" runat="server">
                    <div class="row">
                        <div class="col-sm-12">
                            <Rock:CodeEditor ID="ceFilerEditor" runat="server" Label="File Content" EditorTheme="Rock" EditorHeight="500" />
                        </div>
                    </div>

                    <div class="actions">
                        <Rock:BootstrapButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" DataLoadingText="Saving..." ValidationGroup="ShortLinkDetail"  CompletedDuration="2" CompletedText="Done"></Rock:BootstrapButton>
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Back" CssClass="btn btn-link" OnClick="btnCancel_Click" CausesValidation="false"></asp:LinkButton>
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>

