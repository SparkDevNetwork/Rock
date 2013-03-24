<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FinancialBatch.ascx.cs" Inherits="RockWeb.Blocks.Administration.FinancialBatch" %>

<asp:UpdatePanel ID="upFinancialBatch" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert alert-error block-message error" />

        <Rock:GridFilter ID="rFBFilter" runat="server">
            <Rock:DateTimePicker ID="dtFromDate" runat="server" SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="BatchDateStart" LabelText="From Date" />
            <Rock:DateTimePicker ID="dtThroughDate" runat="server" SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="BatchDateEnd" LabelText="Through Date" />
            <Rock:LabeledTextBox ID="txtTitle" runat="server" LabelText="Title"></Rock:LabeledTextBox>
            <Rock:LabeledCheckBox ID="cbIsClosedFilter" runat="server" LabelText="Is Closed" />
            <Rock:LabeledDropDownList ID="ddlBatchType" runat="server" LabelText="Batch Type" />
        </Rock:GridFilter>

        <Rock:Grid ID="grdFinancialBatch" runat="server" EmptyDataText="No Batches Found" OnRowDataBound="grdFinancialBatch_RowDataBound"
             ShowConfirmDeleteDialog="true">
            <Columns>
                <asp:BoundField DataField="Id" HeaderText="Id" SortExpression="Id" />
                <asp:BoundField DataField="Name" HeaderText="Title" SortExpression="Name" />
                <asp:TemplateField HeaderText="Date Range"><%--SortExpression="BatchDateStart"--%>
                    <ItemTemplate>
                        <span><%# Eval("BatchDateStart") %> to <%# Eval("BatchDateEnd") %></span>

                    </ItemTemplate>
                </asp:TemplateField>
                <Rock:BoolField DataField="IsClosed" HeaderText="Is Closed" SortExpression="IsClosed" />

                <asp:BoundField DataField="ControlAmount" HeaderText="Control Amount" />
                <asp:TemplateField HeaderText="Transaction Total">
                    <ItemTemplate>
                        <asp:Literal ID="TransactionTotal" runat="server" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Variance">
                    <ItemTemplate>
                        <asp:Literal ID="Variance" runat="server" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Transaction Count">
                    <ItemTemplate>
                        <asp:Literal ID="TransactionCount" runat="server" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="BatchType.Name" HeaderText="Batch Type" />

                <Rock:EditValueField OnClick="rGrid_EditValue" />
                <Rock:DeleteField OnClick="grdFinancialBatch_Delete" />


            </Columns>
        </Rock:Grid>


    </ContentTemplate>
</asp:UpdatePanel>





