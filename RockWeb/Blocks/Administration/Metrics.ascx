<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Metrics.ascx.cs" Inherits="RockWeb.Blocks.Administration.Metrics" %>

<asp:UpdatePanel ID="upPanel" runat="server">
<ContentTemplate>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert-message block-massage error"/>

    <div class="grid-filter">
        <fieldset>
            <legend>Filter Options</legend>
            <Rock:LabeledDropDownList ID="ddlCategoryFilter" runat="server" LabelText="Category" AutoPostBack="true" OnSelectedIndexChanged="ddlCategoryFilter_SelectedIndexChanged" />
        </fieldset>
    </div>

    <Rock:Grid ID="rGrid" runat="server" >
        <Columns>
            <asp:BoundField DataField="Id" HeaderText="Id" />
            <asp:BoundField DataField="Category" HeaderText="Category"  />
            <asp:BoundField DataField="Title" HeaderText="Title" />
            <asp:BoundField DataField="Subtitle" HeaderText="Subtitle" />
            <asp:BoundField DataField="Source" HeaderText="Source"/>
            <asp:BoundField DataField="MinValue" HeaderText="MinValue" />
            <asp:BoundField DataField="MaxValue" HeaderText="MaxValue" />
            <asp:BoundField DataField="LastCollected" HeaderText="Last Collected" />
            <Rock:EditField OnClick="rGrid_Edit" />
            <Rock:DeleteField OnClick="rGrid_Delete" />
        </Columns>
    </Rock:Grid>
     
    <Rock:ModalDialog id="modalDetails" runat="server" Title="Metric Details" >
    <Content>        
        <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert-message block-message error"/>
        <fieldset>
            <Rock:LabeledDropDownList ID="ddlType" runat="server" SourceTypeName="Rock.Core.Metric, Rock" PropertyName="Type" />
            <Rock:DataTextBox ID="tbTitle" runat="server" SourceTypeName="Rock.Core.Metric, Rock" PropertyName="Title" />
            <Rock:DataTextBox ID="tbSubtitle" runat="server" SourceTypeName="Rock.Core.Metric, Rock" PropertyName="Subtitle" />
            <Rock:DataTextBox ID="tbCategory" runat="server" SourceTypeName="Rock.Core.Metric, Rock" PropertyName="Category" />
            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Core.Metric, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
            <Rock:DataTextBox ID="tbMinValue" runat="server" SourceTypeName="Rock.Core.Metric, Rock" PropertyName="MinValue" />
            <Rock:DataTextBox ID="tbMaxValue" runat="server" SourceTypeName="Rock.Core.Metric, Rock" PropertyName="MaxValue" />
            <Rock:LabeledDropDownList ID="ddlCollectionFrequency" runat="server" SourceTypeName="Rock.Core.Metric, Rock" PropertyName="CollectionFrequency" />
            <Rock:DataTextBox ID="tbSource" runat="server" SourceTypeName="Rock.Core.Metric, Rock" PropertyName="Source" />
            <Rock:DataTextBox ID="tbSourceSQL" runat="server" SourceTypeName="Rock.Core.Metric, Rock" PropertyName="SourceSQL" TextMode="MultiLine" Rows="3"/>


        </fieldset>

        <asp:HiddenField ID="hfMetricId" runat="server" />
    </Content>
    </Rock:ModalDialog>

</ContentTemplate>
</asp:UpdatePanel>
