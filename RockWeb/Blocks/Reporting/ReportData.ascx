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
                        <asp:PlaceHolder ID="phFilters" runat="server" />

                        <div class="actions margin-t-md">
                            <asp:LinkButton ID="btnFilter" runat="server" AccessKey="m" Text="Filter" CssClass="btn btn-primary btn-sm" OnClick="btnFilter_Click" />
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
                    <Rock:HelpBlock ID="hbDataFilters" runat="server" Text="Select which filters that will be visible to the user.  If Configurable is selected for a visible filter, the user will be able to change the filter, otherwise, the filter will presented as checkbox where user can choose to use the filter or not." />
                    <Rock:Grid ID="grdDataFilters" runat="server" DisplayType="Light" DataKeyNames="Guid">
                        <Columns>
                            <Rock:SelectField HeaderText="Show as a Filter" DataSelectedField="ShowAsFilter" ShowSelectAll="false" ItemStyle-CssClass="js-select-show-filter" />
                            <Rock:SelectField HeaderText="Configurable" DataSelectedField="IsConfigurable" ShowSelectAll="false" ItemStyle-CssClass="js-select-configure-filter" />
                            <asp:BoundField DataField="Title" HeaderText="Title" />
                            <asp:BoundField DataField="Summary" HeaderText="Summary" />
                        </Columns>
                    </Rock:Grid>
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>

        <script>
            var updateConfigureCheckbox = function (t) {
                var $cbConfigurable = $(t).closest('tr').find('.js-select-configure-filter input');
                if ($(t).is(':checked')) {
                    $cbConfigurable.removeAttr('disabled');
                }
                else {
                    $cbConfigurable.removeAttr('checked');
                    $cbConfigurable.attr('disabled', 'disabled');
                }
            }

            Sys.Application.add_load(function () {
                $('.js-select-show-filter input').each(function (i, cb) {
                    updateConfigureCheckbox(cb)
                });

                $('.js-select-show-filter input').on('click', function () {
                    updateConfigureCheckbox(this)
                });
            })
        </script>


    </ContentTemplate>
</asp:UpdatePanel>
