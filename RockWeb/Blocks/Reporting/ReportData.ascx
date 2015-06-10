<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReportData.ascx.cs" Inherits="RockWeb.Blocks.Reporting.ReportData" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbConfigurationWarning" runat="server" NotificationBoxType="Warning" Visible="false" />

        <%-- View Panel --%>
        <asp:Panel ID="pnlView" runat="server">


            <asp:Panel ID="pnlFilter" runat="server">
                <div class="panel panel-block margin-t-md">
                    <div class="panel-heading clearfix">
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
                            <asp:LinkButton ID="btnFilter" runat="server" AccessKey="m" Text="Filter" CssClass="btn btn-primary btn-sm" OnClick="btnFilter_Click" />
                            <asp:LinkButton ID="btnFilterSetDefault" runat="server" Text="Set Default" ToolTip="Set the filter to its default values" CssClass="btn btn-link btn-sm pull-right" OnClick="btnFilterSetDefault_Click" />
                        </div>
                    </div>
                </div>
            </asp:Panel>

            <div class="panel panel-block margin-t-md">
                <div class="panel-heading clearfix">
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

                    <Rock:RockDropDownList ID="ddlReport" runat="server" Label="Report" Help="Select the report to present to the user. Then set which of the report's dataview's filters to show." Required="false" ValidationGroup="vgConfigure" OnSelectedIndexChanged="ddlReport_SelectedIndexChanged" AutoPostBack="true" />
                    <Rock:NotificationBox ID="nbMultipleFilterGroupsWarning" runat="server" NotificationBoxType="Warning" Text="This report has multiple filter groups. This block currently only supports non-grouped filters" Dismissable="true" Visible="false" />
                    <Rock:HelpBlock ID="hbDataFilters" runat="server" 
                        Text="Select which filters that will be visible to the user. 
                        If Configurable is selected for a visible filter, the user will be able to change the filter, otherwise, the filter will presented as checkbox where user can choose to use the filter or not.
                        Select 'Toggle Filter' to present a checkbox with the configurable filter" />
                    <Rock:Grid ID="grdDataFilters" runat="server" DisplayType="Light" DataKeyNames="Guid" RowItemText="Filter">
                        <Columns>
                            <Rock:SelectField HeaderText="Show as a Filter" DataSelectedField="ShowAsFilter" ShowSelectAll="false" ItemStyle-CssClass="js-select-show-filter" Tooltip="Show filter to user" />
                            <Rock:SelectField HeaderText="Configurable" DataSelectedField="IsConfigurable" ShowSelectAll="false" ItemStyle-CssClass="js-select-configure-filter" Tooltip="Allow this filter to be configured" />
                            <Rock:SelectField HeaderText="Toggle Filter" DataSelectedField="IsTogglable" ShowSelectAll="false" ItemStyle-CssClass="js-select-togglable-filter" Tooltip="Show checkbox so user can choose to include the filter or not"/>
                            <asp:BoundField DataField="Title" HeaderText="Title" />
                            <asp:BoundField DataField="Summary" HeaderText="Summary" />
                        </Columns>
                    </Rock:Grid>
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>

        <script>
            var updateCheckboxes = function (row) {
                var $cbShowAsFilter = row.find('.js-select-show-filter input');
                var $cbConfigurable = row.find('.js-select-configure-filter input');
                var $cbTogglable = row.find('.js-select-togglable-filter input');
                if ($cbShowAsFilter.is(':checked')) {
                    $cbConfigurable.removeAttr('disabled');
                    
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
                }
            }

            Sys.Application.add_load(function () {
                $('.js-select-show-filter input').each(function (i, cb) {
                    updateCheckboxes($(cb).closest('tr'))
                });

                $('.js-select-show-filter input, .js-select-configure-filter input').on('click', function () {
                    updateCheckboxes($(this).closest('tr'))
                });
            })
        </script>


    </ContentTemplate>
</asp:UpdatePanel>
