<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DynamicReport.ascx.cs" Inherits="RockWeb.Blocks.Reporting.DynamicReport" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbConfigurationWarning" runat="server" NotificationBoxType="Warning" Visible="false" />

        <%-- View Panel --%>
        <asp:Panel ID="pnlView" runat="server">

            <asp:Panel ID="pnlFilter" runat="server">
                <div class="panel panel-block margin-t-md">
                    <div class="panel-heading">
                        <h1 class="panel-title pull-left">
                            <asp:Literal ID="lFilterIconCssClass" runat="server" />
                            <asp:Literal ID="lFilterTitle" runat="server" />
                        </h1>
                    </div>
                    <div class="panel-body">
                        <Rock:NotificationBox ID="nbFiltersError" runat="server" NotificationBoxType="Danger" Visible="false" />

                        <div class="filter-list">
                            <asp:PlaceHolder ID="phFilters" runat="server" />
                        </div>

                        <div class="actions margin-t-md">
                            <asp:LinkButton ID="btnFilter" runat="server" AccessKey="m" ToolTip="Alt+m" Text="Filter" CssClass="btn btn-primary btn-sm" OnClick="btnFilter_Click" />
                            <asp:LinkButton ID="btnFilterSetDefault" runat="server" Text="Reset Filters" ToolTip="Set the filter to its default values" CssClass="btn btn-link btn-sm pull-right" OnClick="btnFilterSetDefault_Click" />
                        </div>
                    </div>
                </div>
            </asp:Panel>

            <div class="panel panel-block margin-t-md">
                <div class="panel-heading">
                    <h1 class="panel-title pull-left">
                        <asp:Literal ID="lResultsIconCssClass" runat="server" />
                        <asp:Literal ID="lResultsTitle" runat="server" />
                    </h1>
                </div>
                <div class="panel-body">
                    <Rock:NotificationBox ID="nbReportErrors" runat="server" NotificationBoxType="Info" Visible="false" />
                    <div class="grid grid-panel">
                        <Rock:Grid ID="gReport" runat="server" AllowSorting="true" EmptyDataText="No Results" />
                    </div>
                </div>
            </div>

        </asp:Panel>

        <%-- Configuration Panel --%>
        <asp:Panel ID="pnlConfigure" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdConfigure" runat="server" ValidationGroup="vgConfigure" OnSaveClick="mdConfigure_SaveClick">
                <Content>
                    <Rock:RockTextBox ID="txtResultsTitle" runat="server" Label="Results Title" />
                    <Rock:RockTextBox ID="txtResultsIconCssClass" runat="server" Label="Results Icon CSS Class" />
                    <Rock:RockTextBox ID="txtFilterIconCssClass" runat="server" Label="Filter Icon CSS Class" />
                    <Rock:RockTextBox ID="txtFilterTitle" runat="server" Label="Filter Title" />

                    <Rock:ReportPicker ID="rpReport" runat="server" Label="Report" Help="Select the report to present to the user. Then set which of the report's dataview's filters to show." Required="false" ValidationGroup="vgConfigure" OnSelectItem="rpReport_SelectItem" />
                    <Rock:RockDropDownList ID="ddlPersonIdField" runat="server" Label="PersonID Field" Help="If this report has a field for the PersonId, what is the name of that field" />

                    <Rock:RockControlWrapper ID="rcwDataFilters" runat="server" Label="Filters" Help="<p>Select which filters that will be visible to the user.</p><p>If Configurable is selected for a visible filter, the user will be able to change the filter, otherwise, the filter will presented as checkbox where user can choose to use the filter or not. Select 'Toggle Filter' to present a checkbox with the configurable filter.</p>" >
                        <asp:Repeater ID="rptDataFilters" runat="server" OnItemDataBound="rptDataFilters_ItemDataBound">
                            <ItemTemplate>
                                <asp:HiddenField ID="hfDataFilterGuid" runat="server" />
                                <hr />
                                <div class="row js-filterconfig-row">
                                    <div class="col-md-6">
                                        <asp:Literal ID="lFilterDetails" runat="server" />
                                        <Rock:RockCheckBox ID="cbIsVisible" runat="server" Text="Visible" Help="Show filter to user" CssClass="js-settings-show-filter" />
                                        <Rock:RockCheckBox ID="cbIsConfigurable" runat="server" Text="Configurable" Help="Allow this filter to be configured by the user" CssClass="js-settings-configure-filter" />
                                        <Rock:RockCheckBox ID="cbIsTogglable" runat="server" Text="Toggle Filter" Help="Show checkbox so user can choose to include the filter or not" CssClass="js-settings-togglable-filter" />
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:RockTextBox ID="tbPreHtml" runat="server" Label="Pre-HTML" Help="HTML Content to render before the filter <span class='tip tip-lava'></span>" CssClass="js-settings-pre-html" ValidateRequestMode="Disabled" TextMode="MultiLine" Rows="2" />
                                        <Rock:RockTextBox ID="tbLabelHtml" runat="server" Label="Label" Help="The Label (or Checkbox Text) for each filter <span class='tip tip-lava'></span>" CssClass="js-settings-label-html" ValidateRequestMode="Disabled" />
                                        <Rock:RockTextBox ID="tbPostHtml" runat="server" Label="Post-HTML" Help="HTML Content to render after the filter <span class='tip tip-lava'></span>" CssClass="js-settings-post-html" ValidateRequestMode="Disabled" TextMode="MultiLine" Rows="2" />
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </Rock:RockControlWrapper>

                </Content>
            </Rock:ModalDialog>
        </asp:Panel>

        <script>
            var updateConfigControls = function (row) {
                var $cbShowAsFilter = row.find('.js-settings-show-filter');
                var $cbConfigurable = row.find('.js-settings-configure-filter');
                var $cbTogglable = row.find('.js-settings-togglable-filter');
                var $tbLabel = row.find('.js-settings-label-html');
                var $tbPreHtml = row.find('.js-settings-pre-html');
                var $tbPostHtml = row.find('.js-settings-post-html');

                if ($cbShowAsFilter.is(':checked')) {
                    $cbConfigurable.removeAttr('disabled');
                    $tbLabel.removeAttr('disabled');
                    $tbPreHtml.removeAttr('disabled');
                    $tbPostHtml.removeAttr('disabled');

                    if ($cbConfigurable.is(':checked')) {
                        // if showfilter and configurable, let them choose whether to show the filter checkbox
                        $cbTogglable.removeAttr('disabled');
                    }
                    else
                    {
                        // if showfilter and not configurable, the filter checkbox will be shown regardless
                        $cbTogglable.prop('checked', true);
                        $cbTogglable.attr('disabled', 'disable');
                    }
                }
                else {
                    $cbConfigurable.removeAttr('checked');
                    $cbConfigurable.attr('disabled', 'disabled');
                    $cbTogglable.removeAttr('checked');
                    $cbTogglable.attr('disabled', 'disabled');

                    $tbLabel.attr('disabled', 'disabled');
                    $tbPreHtml.attr('disabled', 'disabled');
                    $tbPostHtml.attr('disabled', 'disabled');
                }
            }

            Sys.Application.add_load(function () {
                $('.js-settings-show-filter, .js-settings-configure-filter').on('click', function () {
                    updateConfigControls($(this).closest('.js-filterconfig-row'));
                });
            })
        </script>


    </ContentTemplate>
</asp:UpdatePanel>
