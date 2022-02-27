<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ServiceCheckTest.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.WatchdogMonitor.ServiceCheckTest" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

        <asp:Panel ID="pnlDetails" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">Service Check Options</h1>
            </div>

            <div class="panel-body">
                <asp:ValidationSummary ID="vsErrors" runat="server" CssClass="alert alert-danger" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlProvider" runat="server" Label="Provider" Required="true" AutoPostBack="true" OnSelectedIndexChanged="ddlProvider_SelectedIndexChanged" CausesValidation="false" />
                        <Rock:DefinedValuePicker ID="dvpCollector" runat="server" Label="Collector" Required="false" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlDevice" runat="server" Label="Device" Required="true" />
                    </div>
                </div>

                <asp:PlaceHolder ID="phAttributes" runat="server" />

                <div class="actions">
                    <asp:LinkButton ID="lbRun" runat="server" CssClass="btn btn-primary" Text="Run" OnClick="lbRun_Click" />
                </div>

                <Rock:NotificationBox ID="nbResultError" runat="server" CssClass="margin-t-md" NotificationBoxType="Danger" />

                <asp:Panel ID="pnlResult" runat="server" CssClass="margin-t-md well" Visible="false">
                    <div class="row">
                        <div class="col-md-2">
                            <Rock:RockLiteral ID="ltResultStatus" runat="server" Label="Status" />
                        </div>
                        <div class="col-md-2">
                            <Rock:RockLiteral ID="ltResultValue" runat="server" Label="Value" />
                        </div>
                        <div class="col-md-8">
                            <Rock:RockLiteral ID="ltResultSummary" runat="server" Label="Summary" />
                        </div>
                    </div>
                </asp:Panel>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>

<script type="text/javascript">
    (function () {
        Sys.Application.add_load(function () {
            $('#<%= lbRun.ClientID %>').on('click', function () {
                $('#<%= nbResultError.ClientID %>').hide();
                $('#<%= pnlResult.ClientID %>').hide();
            })
        });
    })();
</script>
