<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Metrics.ascx.cs" Inherits="RockWeb.Blocks.Administration.Metrics" %>

<asp:UpdatePanel ID="upPanel" runat="server">
<ContentTemplate>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert-message block-message error"/>

    <asp:Panel ID="pnlMetricList" runat="server">

        <div class="grid-filter">
        <fieldset>
            <legend>Filter Options
            <Rock:LabeledDropDownList ID="ddlCategoryFilter" runat="server" LabelText="Category" AutoPostBack="true" OnSelectedIndexChanged="ddlCategoryFilter_SelectedIndexChanged" />
            </legend>
        </fieldset>
        </div>

        <Rock:Grid ID="rGridMetric" runat="server" AllowSorting="true" EmptyText="No Metrics Found" >
            <Columns>
                <asp:BoundField DataField="Id" HeaderText="ID" SortExpression="Id" />
                <asp:BoundField DataField="Category" HeaderText="Category" SortExpression="Category" />
                <asp:BoundField DataField="Title" HeaderText="Title" SortExpression="Title" />
                <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description"/>
                <asp:BoundField DataField="MinValue" HeaderText="Minimum Value" SortExpression="MinValue"/>
                <asp:BoundField DataField="MaxValue" HeaderText="Maximum Value" SortExpression="MaxValue"/>
                <asp:BoundField DataField="CollectionFrequency.Name" HeaderText="Collection Frequency" SortExpression="CollectionFrequency.Name"/>
                <asp:BoundField DataField="Source" HeaderText="Source" SortExpression="Source"/>
                <Rock:EditField OnClick="rGridMetric_Edit" />
                <Rock:EditValueField OnClick="rGridMetric_EditValue"/>
                <Rock:DeleteField OnClick="rGridMetric_Delete" />
            </Columns>
        </Rock:Grid>

    </asp:Panel>

    <asp:Panel ID="pnlMetricDetails" runat="server" Visible="false">

        <asp:HiddenField ID="hfIdMetric" runat="server" />
        <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert-message block-message error"/>

        <div class="row-fluid">

            <div class="span6">
                <fieldset>
                    <legend><asp:Literal ID="lAction" runat="server">Metric</asp:Literal></legend>
                    
                    <Rock:DataTextBox ID="tbCategory" runat="server" 
                        SourceTypeName="Rock.Core.Metric, Rock" PropertyName="Category" />
                    <Rock:DataTextBox ID="tbTitle" runat="server" 
                        SourceTypeName="Rock.Core.Metric, Rock" PropertyName="Title" />
                    <Rock:DataTextBox ID="tbSubtitle" runat="server" 
                        SourceTypeName="Rock.Core.Metric, Rock" PropertyName="Subtitle" />                   
                    <Rock:DataTextBox ID="tbDescription" runat="server" 
                        SourceTypeName="Rock.Core.Metric, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                </fieldset>
            </div>

            <div class="span6">
                <fieldset>
                    <legend>&nbsp;</legend>
                    <Rock:LabeledDropDownList ID="ddlCollectionFrequency" runat="server" 
                        LabelText="Collection Frequency" AutoPostBack="true" 
                        OnSelectedIndexChanged="ddlCategoryFilter_SelectedIndexChanged" />
                    <Rock:DataTextBox ID="tbMinValue" runat="server" LabelText="Minimum Value"
                        SourceTypeName="Rock.Core.Metric, Rock" PropertyName="MinValue" />
                    <Rock:DataTextBox ID="tbMaxValue" runat="server"  LabelText="Maximum Value"
                        SourceTypeName="Rock.Core.Metric, Rock" PropertyName="MaxValue" />                      
                    <Rock:DataTextBox ID="tbSource" runat="server" LabelText="Data Source"
                        SourceTypeName="Rock.Core.Metric, Rock" PropertyName="Source" />
                    <Rock:DataTextBox ID="tbSourceSQL" runat="server" LabelText="Source SQL"
                        SourceTypeName="Rock.Core.Metric, Rock" TextMode="MultiLine" Rows="3"
                        PropertyName="SourceSQL" />
                    <Rock:LabeledCheckBox ID="cbType" runat="server" LabelText="Allow Multiple Values" />                  
                </fieldset>
            </div>
        </div>

        <div class="actions">
            <asp:LinkButton ID="btnSaveMetric" runat="server" Text="Save" CssClass="btn primary" onclick="btnSaveMetric_Click" />
            <asp:LinkButton id="btnCancelMetric" runat="server" Text="Cancel" CssClass="btn secondary" CausesValidation="false" OnClick="btnCancelMetric_Click" />
        </div>

    </asp:Panel>

    <asp:Panel ID="pnlValueList" runat="server" Visible="false">
        
        <legend><asp:Literal ID="lMetric" runat="server">Metric Values</asp:Literal></legend>
        <Rock:Grid ID="rGridValue" runat="server" AllowSorting="true" EmptyText="No Metric Values Found">
            <Columns>
                <asp:BoundField DataField="Id" HeaderText="ID" SortExpression="Id" />
                <asp:BoundField DataField="Value" HeaderText="Value" SortExpression="Value" />
                <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description"/>
                <asp:BoundField DataField="xValue" HeaderText="X-Value" SortExpression="xValue"/>
                <asp:BoundField DataField="Label" HeaderText="Label" SortExpression="Label"/>
                <asp:BoundField DataField="isDateBased" HeaderText="IsDateBased" 
                    SortExpression="isDateBased"/>
                <Rock:EditField OnClick="rGridValue_Edit" />                
                <Rock:DeleteField OnClick="rGridValue_Delete" />
            </Columns>
        </Rock:Grid>

         <div class="actions">
            <asp:LinkButton ID="btnValueDone" runat="server" Text="Done" CssClass="btn close" onclick="btnValueDone_Click" />            
        </div>

    </asp:Panel>

    <Rock:ModalDialog id="modalValue" runat="server" Title="Metric Value" >
    <Content>
        <asp:HiddenField ID="hfIdValue" runat="server" />
        <asp:ValidationSummary ID="valSummaryValue" runat="server" HeaderText="Please Correct the Following" CssClass="alert-message block-message error"/>
        
        <legend><asp:Literal ID="lValue" runat="server">Metric Value</asp:Literal></legend>

        <fieldset>
            <div class="span6">
                <Rock:LabeledDropDownList ID="ddlMetricFilter" runat="server" LabelText="Metric" />
                <Rock:DataTextBox ID="tbValue" runat="server" LabelText="Value"
                    SourceTypeName="Rock.Core.MetricValue, Rock" PropertyName="Value" />
                <Rock:DataTextBox ID="tbValueDescription" runat="server"  LabelText="Description"
                    SourceTypeName="Rock.Core.MetricValue, Rock" PropertyName="Description" />
                <Rock:DataTextBox ID="tbXValue" runat="server" LabelText="X-Value"
                    SourceTypeName="Rock.Core.MetricValue, Rock" PropertyName="xValue" />
                <Rock:DataTextBox ID="tbLabel" runat="server" LabelText="Label"
                    SourceTypeName="Rock.Core.MetricValue, Rock" PropertyName="Label" />
                <Rock:LabeledCheckBox ID="cbIsDateBased" runat="server" LabelText="Is Date Based"
                    SourceTypeName="Rock.Core.MetricValue, Rock" PropertyName="isDateBased" />
                
            </div>
        </fieldset>                
        
        
    </Content>
    </Rock:ModalDialog>

</ContentTemplate>
</asp:UpdatePanel>
