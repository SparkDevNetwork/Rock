<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ServiceMetricsEntry.ascx.cs" Inherits="RockWeb.Blocks.Reporting.ServiceMetricsEntry" %>



<asp:UpdatePanel ID="upnlContent" runat="server">
<ContentTemplate>

    <div class="panel panel-block">

        <div class="panel-heading">
            <h1 class="panel-title"><i class="fa fa-signal"></i> Metric Entry</h1>
        </div>

        <div class="panel-body">

            <asp:Panel ID="pnlSelection" runat="server">

                <h3><asp:Literal ID="lSelection" runat="server"></asp:Literal></h3>

                <asp:Repeater ID="rptrSelection" runat="server" OnItemCommand="rptrSelection_ItemCommand" >
                    <ItemTemplate>
                        <asp:LinkButton ID="lbSelection" runat="server" CommandName='<%# Eval("CommandName") %>'  CommandArgument='<%# Eval("CommandArg") %>' Text='<%# Eval("OptionText") %>' CssClass="btn btn-default btn-block" />
                    </ItemTemplate>
                </asp:Repeater>       

            </asp:Panel>

            <asp:Panel ID="pnlMetrics" runat="server" Visible="false">

                <div class="btn-group btn-group-justified margin-b-lg panel-settings-group" >
                    <Rock:ButtonDropDownList ID="bddlCampus" runat="server" OnSelectionChanged="bddl_SelectionChanged" />
                    <Rock:ButtonDropDownList ID="bddlWeekend" runat="server" OnSelectionChanged="bddl_SelectionChanged" />
                    <Rock:ButtonDropDownList ID="bddlService" runat="server" OnSelectionChanged="bddl_SelectionChanged" />
                </div>

                <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                <Rock:NotificationBox ID="nbMetricsSaved" runat="server" Text="Metric Values Have Been Updated" NotificationBoxType="Success" Visible="false" />

                <div class="form-horizontal label-md" >
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

        </div>

    </div>

</ContentTemplate>
</asp:UpdatePanel>
