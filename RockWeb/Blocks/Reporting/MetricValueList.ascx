<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MetricValueList.ascx.cs" Inherits="RockWeb.Blocks.Reporting.MetricValueList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" CssClass="panel panel-block" runat="server">
            <asp:HiddenField ID="hfMetricId" runat="server" />
            <asp:HiddenField ID="hfMetricCategoryId" runat="server" />
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
            
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-list"></i> Metric Values</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gMetricValues" runat="server" AllowSorting="true" OnRowSelected="gMetricValues_Edit" OnRowDataBound="gMetricValues_RowDataBound">
                        <Columns>
                            <Rock:DateField DataField="MetricValueDateTime" HeaderText="Date/Time" SortExpression="MetricValueDateTime" />
                            <Rock:EnumField DataField="MetricValueType" HeaderText="Type" SortExpression="MetricValueType" />
                            <asp:BoundField DataField="YValue" HeaderText="Value" SortExpression="YValue" ItemStyle-HorizontalAlign="Right" />
                            <asp:BoundField DataField="EntityId" HeaderText="EntityTypeName" />
                            <asp:BoundField DataField="XValue" HeaderText="X Value" SortExpression="XValue" ItemStyle-HorizontalAlign="Right" />
                    
                            <Rock:DeleteField OnClick="gMetricValues_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>

