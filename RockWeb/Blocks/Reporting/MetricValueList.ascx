<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MetricValueList.ascx.cs" Inherits="RockWeb.Blocks.Reporting.MetricValueList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" runat="server">
            <h4>Values</h4>
            <asp:HiddenField ID="hfMetricId" runat="server" />
            <asp:HiddenField ID="hfMetricCategoryId" runat="server" />
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
            <Rock:Grid ID="gMetricValues" runat="server" AllowSorting="true" OnRowSelected="gMetricValues_Edit">
                <Columns>
                    <Rock:DateField DataField="MetricValueDateTime" HeaderText="Date/Time" SortExpression="MetricValueDateTime" />
                    <Rock:EnumField DataField="MetricValueType" HeaderText="Type" SortExpression="MetricValueType" />
                    <asp:BoundField DataField="XValue" HeaderText="Value" SortExpression="XValue" ItemStyle-HorizontalAlign="Right" />
                    <asp:BoundField DataField="YValue" HeaderText="Y Value" SortExpression="YValue" ItemStyle-HorizontalAlign="Right" />
                    <Rock:DeleteField OnClick="gMetricValues_Delete" />
                </Columns>
            </Rock:Grid>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>

