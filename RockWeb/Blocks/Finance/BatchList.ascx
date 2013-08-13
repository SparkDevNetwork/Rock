<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BatchList.ascx.cs" Inherits="RockWeb.Blocks.Finance.Administration.BatchList" %>

<asp:UpdatePanel ID="upFinancialBatch" runat="server">
    <ContentTemplate>

        <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error alert" />
        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

        <Rock:GridFilter ID="rFBFilter" runat="server">
            <Rock:DatePicker ID="dtBatchDate" runat="server" SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="BatchDate" LabelText="Date" />
            <Rock:LabeledTextBox ID="txtTitle" runat="server" LabelText="Title"></Rock:LabeledTextBox>
            <Rock:LabeledDropDownList ID="ddlStatus" runat="server" LabelText="Status" />
            <Rock:CampusPicker ID="ddlCampus" runat="server" />
        </Rock:GridFilter>

        <Rock:Grid ID="rGridBatch" runat="server" OnRowDataBound="rGridBatch_RowDataBound"
             ShowConfirmDeleteDialog="true" OnRowSelected="rGridBatch_Edit">
            <Columns>
                <asp:BoundField DataField="Id" HeaderText="Id" SortExpression="Id" />
                <asp:BoundField DataField="Name" HeaderText="Title" SortExpression="Name" />
                <asp:TemplateField HeaderText="Date">
                    <ItemTemplate>
                        <span><%# Eval("BatchDate") %></span>
                    </ItemTemplate>
                </asp:TemplateField>
                <Rock:EnumField DataField="Status" HeaderText="Status" SortExpression="Status" />
               
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
                <asp:BoundField DataField="Status" HeaderText="Status" />
                <asp:BoundField DataField="Campus" HeaderText="Campus" />

                <Rock:DeleteField OnClick="rGridBatch_Delete" />

            </Columns>

        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>





