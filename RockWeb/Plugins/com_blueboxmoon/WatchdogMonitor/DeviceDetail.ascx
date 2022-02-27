<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DeviceDetail.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.WatchdogMonitor.DeviceDetail" %>
<%@ Register Namespace="com.blueboxmoon.WatchdogMonitor.Web.UI.Controls" Assembly="com.blueboxmoon.WatchdogMonitor" TagPrefix="WM" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

        <asp:Panel ID="pnlDetails" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lDetailsIcon" runat="server" />
                    <asp:Literal ID="lDetailsName" runat="server" />
                </h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlDetailsInactive" runat="server" LabelType="Default" Text="Inactive" />
                    <Rock:HighlightLabel ID="hlDetailsProfile" runat="server" LabelType="Type" />
                </div>
            </div>

            <div class="panel-body">
                <fieldset>
                    <div class="row">
                        <div class="col-md-6">
                            <dl>
                                <dt>Address</dt>
                                <dd><asp:Literal ID="lDetailsAddress" runat="server" /></dd>
                            </dl>
                            <dl>
                                <dt>Parent</dt>
                                <dd><asp:Literal ID="lDetailsParent" runat="server" /></dd>
                            </dl>
                            <dl>
                                <dt>Groups</dt>
                                <dd><asp:Literal ID="lDetailsGroups" runat="server" /></dd>
                            </dl>
                        </div>

                        <div class="col-md-6">
                            <dl>
                                <dt>Overall State</dt>
                                <dd>
                                    <asp:Literal ID="lDetailsOverallState" runat="server" />
                                </dd>
                            </dl>
                            <dl>
                                <dt>Device State</dt>
                                <dd>
                                    <asp:Literal ID="lDetailsDeviceState" runat="server" />
                                </dd>
                            </dl>
                            <dl>
                                <dt>Collector</dt>
                                <dd><asp:Literal ID="lDetailsCollector" runat="server" /></dd>
                            </dl>
                        </div>
                    </div>

                    <dl>
                        <dt>Description</dt>
                        <dd><asp:Literal ID="lDetailsDescription" runat="server" /></dd>
                    </dl>
                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="lbDetailsEdit" runat="server" CssClass="btn btn-primary" Text="Edit" OnClick="lbDetailsEdit_Click" />
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlServiceChecks" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">Service Checks</h1>
            </div>

            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:Grid ID="gServiceChecks" runat="server" CssClass="js-service-checks" AllowPaging="false" AllowSorting="false" RowItemText="Service Check" OnRowSelected="gServiceChecks_RowSelected" OnGridRebind="gServiceChecks_GridRebind">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Service Check" />
                            <Rock:RockBoundField DataField="StateHtml" HeaderText="State" HtmlEncode="false" />
                            <Rock:RockBoundField DataField="FormattedValue" HeaderText="Value" />
                            <Rock:DateTimeField DataField="LastCheckDateTime" HeaderText="Last Check" />
                            <Rock:RockBoundField DataField="Summary" HeaderText="Summary" />
                            <Rock:RockTemplateField ItemStyle-CssClass="grid-columncommand" HeaderStyle-CssClass="grid-columncommand" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <a href="#" class="btn btn-default js-run-service-check" title="Run Service Check Now">
                                        <i class="fa fa-play"></i>
                                    </a>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlRecentEvents" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">Recent Events</h1>
                <div class="pull-right" style="margin: -4px 0px;">
                    <asp:LinkButton ID="lbShowAllEvents" runat="server" CssClass="btn btn-default btn-xs" Text="Show All" OnClick="lbShowAllEvents_Click" />
                </div>
            </div>

            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:Grid ID="gRecentEvents" runat="server" AllowSorting="false" RowItemText="Event" OnGridRebind="gRecentEvents_GridRebind" OnRowDataBound="gRecentEvents_RowDataBound">
                        <Columns>
                            <Rock:RockBoundField DataField="ServiceCheckName" HeaderText="Service Check" />
                            <Rock:RockBoundField DataField="State" HeaderText="State" HtmlEncode="false" />
                            <Rock:DateTimeField DataField="StartDateTime" HeaderText="Start Date Time" />
                            <Rock:DateTimeField DataField="EndDateTime" HeaderText="End Date Time" />
                            <Rock:RockBoundField DataField="LastSummary" HeaderText="Last Message" />
                            <Rock:EditField ButtonCssClass="js-toggle-silence btn btn-default" IconCssClass="fa fa-bell" OnClick="gRecentEvents_ToggleSilenceClick" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlGraphs" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">Graphs</h1>
            </div>

            <div class="panel-body">
                <div class="row">
                    <asp:Repeater ID="rpGraphs" runat="server">
                        <ItemTemplate>
                            <div class="col-sm-12 col-md-6 col-lg-4">
                                <div class="js-service-check-chart" data-id='<%# Eval( "Id" ) %>' data-range="1h">
                                    <div style="height: 200px; width: 100%; position: relative;">
                                        <canvas></canvas>
                                        <div style="position: absolute; top: 0; bottom: 0; left: 0; right: 0; background-color: rgba(0,0,0,0.02);" class="text-center js-service-check-loading">
                                            <i class="fa fa-spinner fa-spin fa-3x" style="line-height: 200px;"></i>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlEdit" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">Edit Device</h1>
            </div>

            <div class="panel-body">
                <asp:ValidationSummary ID="vsEdit" runat="server" ValidationGroup="Edit" HeaderText="Please correct the following:" />
                <Rock:NotificationBox ID="nbEditError" runat="server" NotificationBoxType="Danger" />
                <asp:HiddenField ID="hfEditId" runat="server" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbEditName" runat="server" Label="Name" MaxLength="50" Required="true" ValidationGroup="Edit" />
                        <Rock:RockTextBox ID="tbEditAddress" runat="server" Label="Address" MaxLength="100" ValidationGroup="Edit" />
                        <Rock:RockDropDownList ID="ddlEditProfile" runat="server" Label="Profile" Required="true" ValidationGroup="Edit" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbEditIsActive" runat="server" Label="Active" ValidationGroup="Edit" />
                        <Rock:RockDropDownList ID="ddlEditParentDevice" runat="server" Label="Parent" Help="If a parent device is down, none of it's descendants will be checked." ValidationGroup="Edit" />
                        <Rock:DefinedValuePicker ID="dvCollector" runat="server" Label="Collector" Help="Specifies which data collector will be used for this device's service checks." ValidationGroup="Edit" Required="true" />
                    </div>
                </div>

                <Rock:RockTextBox ID="tbEditDescription" runat="server" Label="Description" ValidationGroup="Edit" TextMode="MultiLine" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockControlWrapper ID="rcwGroups" runat="server" Label="Groups">
                            <asp:Repeater ID="rpGroups" runat="server" OnItemCommand="rpGroups_ItemCommand">
                                <ItemTemplate>
                                    <div class="control-static"><%# Eval( "Name" ) %> <asp:LinkButton ID="lbDeleteGroup" runat="server" CssClass="btn btn-link" CommandArgument='<%# Eval( "Id" ) %>' CommandName="Delete" CausesValidation="false"><i class="fa fa-times"></i></asp:LinkButton></div>
                                </ItemTemplate>
                            </asp:Repeater>

                            <Rock:RockDropDownList ID="ddlGroups" runat="server" OnSelectedIndexChanged="ddlGroups_SelectedIndexChanged" AutoPostBack="true" CausesValidation="false" />
                        </Rock:RockControlWrapper>
                    </div>
                </div>

                <h4>SNMP Settings</h4>

                <Rock:RockCheckBox ID="cbEditOverrideSnmp" runat="server" Label="Override SNMP Settings" ValidationGroup="Edit" CausesValidation="false" OnCheckedChanged="cbEditOverrideSnmp_CheckedChanged" AutoPostBack="true" />

                <WM:SnmpSettingsEditor ID="sseEditSnmpSettings" runat="server" ValidationGroup="Edit" />

                <h4>NRPE Settings</h4>

                <Rock:RockCheckBox ID="cbEditOverrideNrpe" runat="server" Label="Override NRPE Settings" ValidationGroup="Edit" CausesValidation="false" OnCheckedChanged="cbEditOverrideNrpe_CheckedChanged" AutoPostBack="true" />

                <WM:NrpeSettingsEditor ID="nseEditNrpeSettings" runat="server" ValidationGroup="Edit" />

                <div class="actions">
                    <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                    <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="lbCancel_Click" />
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>

<script src="/SignalR/hubs"></script>
<script type="text/javascript">
    (function () {
        var watchdogProxy = $.connection['com.blueboxmoon.WatchdogMonitor.MessageHub'];

        $.connection.hub.start().done(function () {
        });

        Sys.Application.add_load(function () {
            $('.js-run-service-check').on('click', function (e) {
                e.preventDefault();

                if ($(this).find('.fa').hasClass('fa-spin')) {
                    return;
                }

                var serviceCheckId = $(this).closest('tr').attr('datakey');

                $(this).find('.fa').addClass('fa-spin fa-cog').removeClass('fa-play');

                watchdogProxy.server.runCheckNow(serviceCheckId, true).then(function (response) {
                    if (typeof response === "string") {
                        alert(error);
                    }
                    else {
                        watchdogProxy.server.getServiceCheckDisplayDetails(response.Id).then(function (details) {
                            var $tr = $('.js-service-checks').find('tr[datakey="' + response.Id + '"]');
                            $icon = $tr.find('.js-run-service-check').find('.fa');

                            if ($icon.hasClass('fa-play')) {
                                return;
                            }

                            $icon.removeClass('fa-spin fa-cog').addClass('fa-play');

                            $tr.children('td:nth-child(2)').html(details.StateHtml);
                            $tr.children('td:nth-child(3)').html(details.FormattedValue);
                            $tr.children('td:nth-child(4)').html(details.LastCheckDateTime);
                            $tr.children('td:nth-child(5)').html(details.Summary);
                        });
                    }
                });
            });
        });
    })();
</script>
