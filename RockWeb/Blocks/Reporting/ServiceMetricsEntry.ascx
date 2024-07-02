<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ServiceMetricsEntry.ascx.cs" Inherits="RockWeb.Blocks.Reporting.ServiceMetricsEntry" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-signal"></i>Metric Entry</h1>
            </div>

            <div class="panel-body">

                <asp:Panel ID="pnlConfigurationError" runat="server" Visible="false">
                    <Rock:NotificationBox ID="nbNoCampuses" runat="server" NotificationBoxType="Warning"
                        Heading="No Campuses Available"
                        Text="There are no campuses available based on the campus filter settings."
                        Visible="false" />
                </asp:Panel>

                <asp:Panel ID="pnlSelection" runat="server">

                    <h3 class="mt-0">
                        <asp:Literal ID="lSelection" runat="server"></asp:Literal></h3>

                    <asp:Repeater ID="rptrSelection" runat="server" OnItemCommand="rptrSelection_ItemCommand">
                        <ItemTemplate>
                            <asp:LinkButton ID="lbSelection" runat="server" CommandName='<%# Eval("CommandName") %>' CommandArgument='<%# Eval("CommandArg") %>' Text='<%# Eval("OptionText") %>' CssClass="btn btn-default btn-block" />
                        </ItemTemplate>
                    </asp:Repeater>

                </asp:Panel>

                <asp:Panel ID="pnlMetrics" runat="server" Visible="false">

                    <div class="panel-settings-group margin-b-lg">
                        <Rock:ButtonDropDownList ID="bddlCampus" runat="server" Label="Campus" FormGroupCssClass="m-0" OnSelectionChanged="bddl_SelectionChanged" />
                        <div id="divCampusLabel" runat="server" class="form-group button-drop-down-list m-0">
                            <label class="control-label">Campus</label>
                            <div class="mt-4 ml-3">
                              <span><asp:Literal ID="lCampus" runat="server"></asp:Literal></span>
                            </div>
                        </div>
                        <Rock:ButtonDropDownList ID="bddlWeekend" runat="server" Label="Week of" FormGroupCssClass="m-0" OnSelectionChanged="bddl_SelectionChanged" />
                        <asp:Panel ID="pnlNoServices" CssClass="btn" runat="server">
                            No Services Available
                        </asp:Panel>
                        <Rock:ButtonDropDownList ID="bddlService" runat="server" Label="Service" FormGroupCssClass="m-0" OnSelectionChanged="bddl_SelectionChanged" />
                    </div>

                    <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                    <Rock:NotificationBox ID="nbMetricsSaved" runat="server" Text="Metric Values Have Been Updated" NotificationBoxType="Success" Visible="false" />
                    <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" Visible="false" />
                    <asp:Panel ID="pnlMetricEdit" runat="server">
                        <div class="form-horizontal label-md">
                            <asp:Repeater ID="rptrMetric" runat="server" OnItemDataBound="rptrMetric_ItemDataBound">
                                <ItemTemplate>
                                    <asp:HiddenField ID="hfMetricId" runat="server" Value='<%# Eval("Id") %>' />
                                    <Rock:NumberBox ID="nbMetricValue" runat="server" NumberType="Double" Label='<%# Eval( "Name") %>' Text='<%# Eval( "Value") %>' />
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>

                        <Rock:RockTextBox ID="tbNote" runat="server" Label="Note" TextMode="MultiLine" Rows="4" />

                        <div class="actions">
                            <asp:LinkButton ID="btnSave" runat="server" Text="Save" AccessKey="s" ToolTip="Alt+s" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        </div>
                    </asp:Panel>
                </asp:Panel>

            </div>

        </div>

    </ContentTemplate>
</asp:UpdatePanel>
