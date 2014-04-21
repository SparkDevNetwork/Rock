<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MetricValueList.ascx.cs" Inherits="RockWeb.Blocks.Reporting.MetricValueList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" runat="server">
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
            <Rock:Grid ID="gMetricValues" runat="server" AllowSorting="true" OnRowSelected="gMetricValues_Edit">
                <Columns>
                    <Rock:EnumField DataField="MetricValueType" HeaderText="Type" SortExpression="MetricValueType" />
                    <asp:BoundField DataField="XValue" HeaderText="XValue" SortExpression="XValue" ItemStyle-HorizontalAlign="Right" />
                    <asp:BoundField DataField="YValue" HeaderText="YValue" SortExpression="YValue" ItemStyle-HorizontalAlign="Right" />
                    <Rock:DeleteField OnClick="gMetricValues_Delete" />
                </Columns>
            </Rock:Grid>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>

