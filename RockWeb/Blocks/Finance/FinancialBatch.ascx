<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FinancialBatch.ascx.cs" Inherits="RockWeb.Blocks.Administration.FinancialBatch" %>

<asp:UpdatePanel ID="upFinancialBatch" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert alert-error block-message error"/>

        <Rock:GridFilter ID="rFBFilter" runat="server">
            <Rock:DateTimePicker ID="dtFromDate" runat="server" SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="BatchDateStart" LabelText="From Date" />
            <Rock:DateTimePicker ID="dtThroughDate" runat="server"  SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="BatchDateEnd" LabelText="Through Date" />
            <Rock:LabeledTextBox ID="txtTitle" runat="server" LabelText="Title"></Rock:LabeledTextBox>            
            <Rock:LabeledCheckBox ID="cbIsClosedFilter" runat="server" LabelText="Is Closed" />        
            <Rock:LabeledDropDownList ID="ddlBatchType" runat="server" LabelText="Batch Type" />
        </Rock:GridFilter>
        
        <Rock:Grid ID="grdFinancialBatch" runat="server" EmptyDataText="No Batch Transactions Found">
            <Columns> 
                <asp:BoundField DataField="Id" HeaderText="Id" SortExpression="Id" />
                <asp:BoundField DataField="Name" HeaderText="Title" SortExpression="Name" />
                <asp:TemplateField  HeaderText="Date Range"  />  <%--SortExpression="BatchDateStart"--%>
                <Rock:BoolField DataField="IsClosed" HeaderText="Is Closed" SortExpression="IsClosed" />
                
                <asp:TemplateField HeaderText="Control Amount" />
                <asp:TemplateField HeaderText="Transaction Total" />
                <asp:TemplateField HeaderText="Variance" />
                <asp:TemplateField HeaderText="Transaction Count" />
                <asp:TemplateField HeaderText="Batch Type" />
                <asp:TemplateField HeaderText="Funds listed w/ transaction totals" />


                
                <Rock:EditValueField OnClick="rGrid_EditValue" />
                <Rock:DeleteField OnClick="grdFinancialBatch_Delete" />
               

            </Columns>
        </Rock:Grid>
        
        <Rock:ModalDialog ID="modalValue" runat="server" Title="Financial Batch">
            <Content>
                <asp:HiddenField ID="hfIdValue" runat="server" />
                <asp:ValidationSummary ID="valSummaryValue" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error" />

                <legend>
                    <asp:Literal ID="lValue" runat="server">Financial Batch</asp:Literal></legend>

                <fieldset>
                    <div class="span6">
                        <Rock:DataTextBox ID="tbName" runat="server" LabelText="Title"
                            SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="Name" />
                         <Rock:DateTimePicker ID="dtBatchDateStart" runat="server" SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="BatchDateStart" LabelText="Batch Date Start" />
                         <Rock:DateTimePicker ID="dtBatchDateEnd" runat="server" SourceTypeName="Rock.Model.FinancialBatch, Rock" PropertyName="BatchDateEnd" LabelText="Batch Date End" />
           
                        <Rock:LabeledDropDownList ID="ddlCampus" runat="server" LabelText="Campus" />
                        <Rock:LabeledDropDownList ID="ddlEntity" runat="server" LabelText="Entity" />
                        
                        <Rock:LabeledCheckBox ID="cbIsClosed" runat="server" LabelText="Is Closed"  />

                    </div>
                </fieldset>


            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>