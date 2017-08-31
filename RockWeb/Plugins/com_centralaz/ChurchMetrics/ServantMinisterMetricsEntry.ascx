<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ServantMinisterMetricsEntry.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.ChurchMetrics.ServantMinisterMetricsEntry" %>
<meta name="viewport" content="width=device-width, initial-scale=0.75, user-scalable=no" />

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-body">

                <asp:Panel ID="pnlSelection" runat="server">

                    <h3>
                        <asp:Literal ID="lSelection" runat="server"></asp:Literal></h3>

                    <asp:Repeater ID="rptrSelection" runat="server" OnItemCommand="rptrSelection_ItemCommand">
                        <ItemTemplate>
                            <asp:LinkButton ID="lbSelection" runat="server" CommandName='<%# Eval("CommandName") %>' CommandArgument='<%# Eval("CommandArg") %>' Text='<%# Eval("OptionText") %>' CssClass="btn btn-default btn-block" />
                        </ItemTemplate>
                    </asp:Repeater>

                </asp:Panel>

                <asp:Panel ID="pnlMetrics" runat="server" Visible="false">
                    <h3>
                        <asp:Literal ID="lWeekend" runat="server" /></h3>
                    <div class="btn-group btn-group-justified margin-b-lg panel-settings-group">
                        <Rock:ButtonDropDownList ID="bddlCampus" runat="server" OnSelectionChanged="bddlCampus_SelectionChanged" />
                        <Rock:ButtonDropDownList ID="bddlWeekend" runat="server" OnSelectionChanged="bddl_SelectionChanged" Visible="false" />
                        <Rock:ButtonDropDownList ID="bddlService" runat="server" OnSelectionChanged="bddl_SelectionChanged" />
                    </div>
                    <br />
                    <br />
                    <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                    <Rock:NotificationBox ID="nbMetricsSaved" runat="server" Text="Metric Values Have Been Updated" NotificationBoxType="Success" Visible="false" />
                    <Rock:NotificationBox ID="nbMetricsSkipped" runat="server" NotificationBoxType="Warning" Visible="false" />

                    <div class="form-horizontal label-xs">
                        <asp:Repeater ID="rptrMetric" runat="server" OnItemDataBound="rptrMetric_ItemDataBound">
                            <ItemTemplate>
                                <asp:HiddenField ID="hfMetricId" runat="server" Value='<%# Eval("Id") %>' />
                                <asp:HiddenField ID="hfModifiedDateTime" runat="server" Value='<%# Eval("ModifiedDateTime") %>' />
                                <div class="row">
                                    <div class="col-xs-3" style="text-align: right">
                                        <asp:Label ID="lMetricTitle" runat="server" Text='<%# Eval( "Name") %>' />
                                    </div>
                                    <div class="col-xs-9">
                                        <Rock:NumberBox ID="nbMetricValue" runat="server" NumberType="Integer" Text='<%# Eval( "Value") %>' />
                                    </div>
                                </div>
                                <br />
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>

                    <Rock:RockTextBox ID="tbNote" runat="server" Label="Note" TextMode="MultiLine" Rows="4" />

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" Text="Save" AccessKey="s" CssClass="btn btn-primary" Font-Size="XX-Large" OnClick="btnSave_Click" />
                        <div class="pull-right">
                            <asp:LinkButton ID="btnLogout" runat="server" Text="Logout" AccessKey="s" CssClass="btn btn-default" Font-Size="XX-Large" OnClick="btnLogout_Click" />
                        </div>
                    </div>

                </asp:Panel>

                <asp:Panel ID="pnlUnauthorized" runat="server" Visible="false">
                    <div class="alert alert-danger">
                        <p>
                            You do not have the privileges to enter metrics. If you should have access to this page, please contact your supervisor to get access.
                        </p>
                    </div>
                </asp:Panel>

            </div>

        </div>

    </ContentTemplate>
</asp:UpdatePanel>
