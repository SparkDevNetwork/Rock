<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MetricValueList.ascx.cs" Inherits="RockWeb.Blocks.Administration.MetricValueList" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" runat="server">

            <asp:Panel ID="pnlList" runat="server">
                <asp:HiddenField ID="hfMetricId" runat="server" />
                <legend>
                    <asp:Literal ID="lMetric" runat="server">Metric Values</asp:Literal>
                </legend>
                <Rock:Grid ID="gMetricValues" runat="server" AllowSorting="true" EmptyDataText="No Metric Values Found" OnRowSelected="gMetricValues_Edit">
                    <Columns>
                        <asp:BoundField DataField="Id" HeaderText="ID" SortExpression="Id" />
                        <asp:BoundField DataField="Value" HeaderText="Value" SortExpression="Value" />
                        <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                        <asp:BoundField DataField="xValue" HeaderText="X-Value" SortExpression="xValue" />
                        <asp:BoundField DataField="Label" HeaderText="Label" SortExpression="Label" />
                        <asp:BoundField DataField="isDateBased" HeaderText="IsDateBased"
                            SortExpression="isDateBased" />
                        <Rock:DeleteField OnClick="gMetricValues_Delete" />
                    </Columns>
                </Rock:Grid>

            </asp:Panel>

            <Rock:ModalDialog ID="modalValue" runat="server" Title="Metric Value" ValidationGroup="Value"  >
                <Content>
                    <asp:HiddenField ID="hfMetricValueId" runat="server" />
                    <asp:ValidationSummary ID="valSummaryValue" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="Value" />

                    <legend>
                        <asp:Literal ID="lActionTitle" runat="server" />
                    </legend>
                    <fieldset>
                        <div class="span6">
                            <Rock:RockDropDownList ID="ddlMetricFilter" runat="server" Label="Metric" ValidationGroup="Value" />
                            <Rock:DataTextBox ID="tbValue" runat="server" LabelText="Value"
                                SourceTypeName="Rock.Model.MetricValue, Rock" PropertyName="Value" ValidationGroup="Value" />
                            <Rock:DataTextBox ID="tbValueDescription" runat="server" LabelText="Description"
                                SourceTypeName="Rock.Model.MetricValue, Rock" PropertyName="Description" ValidationGroup="Value" />
                            <Rock:DataTextBox ID="tbXValue" runat="server" LabelText="X-Value"
                                SourceTypeName="Rock.Model.MetricValue, Rock" PropertyName="xValue" ValidationGroup="Value" />
                            <Rock:DataTextBox ID="tbLabel" runat="server" LabelText="Label"
                                SourceTypeName="Rock.Model.MetricValue, Rock" PropertyName="Label" ValidationGroup="Value" />
                            <Rock:RockCheckBox ID="cbIsDateBased" runat="server" Text="Is Date Based"
                                SourceTypeName="Rock.Model.MetricValue, Rock" PropertyName="isDateBased" ValidationGroup="Value" />
                        </div>
                    </fieldset>

                </Content>
            </Rock:ModalDialog>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
