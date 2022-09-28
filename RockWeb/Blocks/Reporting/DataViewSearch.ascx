<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DataViewSearch.ascx.cs" Inherits="RockWeb.Blocks.Reporting.DataViewSearch" %>

<div class="grid">
    <Rock:Grid ID="gGroups" runat="server" EmptyDataText="No DataViews Found">
        <Columns>
            <Rock:RockBoundField
                HeaderText="DataView"
                DataField="Structure"
                SortExpression="Structure" HtmlEncode="false" />
            <Rock:RockBoundField 
                HeaderText="Name"
                DataField="Name" 
                SortExpression="Name" />
        </Columns>
    </Rock:Grid>
</div>


