<%@ Control Language="C#" AutoEventWireup="true" CodeFile="VersionDetail.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.Rockumentation.VersionDetail" %>
<%@ Register Namespace="com.blueboxmoon.Rockumentation.UI" Assembly="com.blueboxmoon.Rockumentation" TagPrefix="RM" %>

<script src="/SignalR/hubs"></script>
<script type="text/javascript">
    $(function () {
        var proxy = $.connection.rockMessageHub;

        proxy.client.versionDuplicateProgress = function (completed, total, text) {
            var $bar = $('#<%= upnlContent.ClientID %> .js-scan-progress-bar');

            $bar.prop('aria-valuenow', completed);
            $bar.prop('aria-valuemax', total);
            if (total !== 0) {
                $bar.css('width', (completed / total * 100) + '%');
                $bar.text(completed + '/' + total);
            }
            else {
                $bar.css('width', '0%');
                $bar.text('');
            }

            $('#<%= upnlContent.ClientID %> .js-progress-text').text(text);

            $('#<%= pnlProgress.ClientID %>').show();
        };

        proxy.client.versionDuplicateStatus = function (status, error) {
            if (error) {
                $('#<%= upnlContent.ClientID %> .js-progress-text').text(error);
            }
            else {
                window.location = '?VersionId=' + status;
            }
        };

        $.connection.hub.start().done(function () {
            $('#<%= hfConnectionId.ClientID %>').val($.connection.hub.id);
        });
    });
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfConnectionId" runat="server" />

        <Rock:NotificationBox ID="nbUnauthorized" runat="server" NotificationBoxType="Warning"></Rock:NotificationBox>
        <Rock:NotificationBox ID="nbErrorMessage" runat="server" NotificationBoxType="Danger" />
        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

        <asp:Panel ID="pnlDetails" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-code-branch"></i>
                    <asp:Literal ID="lTitle" runat="server" />
                </h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlLocked" runat="server" LabelType="Danger" Text="Locked" />
                    <Rock:HighlightLabel ID="hlNotPublished" runat="server" LabelType="Warning" Text="Not Published" />
                    <Rock:HighlightLabel ID="hlPublished" runat="server" LabelType="Success" Text="Published" />
                </div>
            </div>

            <div class="panel-body">
                <fieldset>
                    <dl>
                        <dt>Description</dt>
                        <dd><asp:Literal ID="lDescription" runat="server" /></dd>
                    </dl>

                    <Rock:AttributeValuesContainer ID="avcAttributes" runat="server" ShowCategoryLabel="false" />

                    <div class="actions">
                        <a id="lbViewBook" runat="server" class="btn btn-primary">View Book</a>
                        <asp:LinkButton ID="lbEdit" runat="server" CssClass="btn btn-link" Text="Edit" OnClick="lbEdit_Click" />

                        <div class="pull-right">
                            <a id="lbSecurity" runat="server" class="btn btn-default btn-sm">
                                <i class="fa fa-lock"></i>
                            </a>
                            <asp:LinkButton ID="lbDuplicate" runat="server" CssClass="btn btn-default btn-sm" OnClick="lbDuplicate_Click" ToolTip="Copy this version">
                                <i class="fa fa-clone"></i>
                            </asp:LinkButton>
                        </div>
                    </div>
                </fieldset>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlEdit" CssClass="panel panel-block" runat="server">
            <asp:HiddenField ID="hfId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-code-branch"></i>
                    <asp:Literal ID="lEditTitle" runat="server" />
                </h1>
            </div>

            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>

            <div class="panel-body">

                <asp:ValidationSummary ID="vSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbEditVersion" runat="server" SourceTypeName="com.blueboxmoon.Rockumentation.Model.DocumentationVersion, com.blueboxmoon.Rockumentation" PropertyName="Version" />
                    </div>

                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbEditIsPublished" runat="server" Label="Published" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbEditIsLocked" runat="server" Label="Locked" Help="If a version is locked, then editing articles is not allowed regardless of security permissions." />
                    </div>

                    <div class="col-md-6">
                    </div>
                </div>

                <Rock:DataTextBox ID="tbEditDescription" runat="server" SourceTypeName="com.blueboxmoon.Rockumentation.Model.DocumentationVersion, com.blueboxmoon.Rockumentation" PropertyName="Description" TextMode="MultiLine" Rows="3" ValidateRequestMode="Disabled" />

                <Rock:AttributeValuesContainer ID="avcEditAttributes" runat="server" />

                <div class="actions">
                    <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                    <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                </div>

            </div>
        </asp:Panel>

        <asp:Panel ID="pnlWorking" runat="server">
            <asp:Panel ID="pnlProgress" runat="server" style="display: none;">
                <p class="js-progress-text">Preparing to duplicate</p>
                <div class="progress">
                    <div class="progress-bar progress-bar-info js-scan-progress-bar" role="progressbar" style="width: 0%" aria-valuenow="0" aria-valuemax="0"></div>
                </div>
            </asp:Panel>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>