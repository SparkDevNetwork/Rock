<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReportSearch.ascx.cs" Inherits="RockWeb.Blocks.Reporting.ReportSearch" %>

<div class="grid">
    <Rock:Grid ID="gGroups" runat="server" EmptyDataText="No Reports Found">
        <Columns>
            <Rock:RockBoundField
                HeaderText="Report"
                DataField="Structure"
                SortExpression="Structure" HtmlEncode="false" />
            <Rock:RockBoundField 
                HeaderText="Name"
                DataField="Name" 
                SortExpression="Name" />
        </Columns>
    </Rock:Grid>
</div>


