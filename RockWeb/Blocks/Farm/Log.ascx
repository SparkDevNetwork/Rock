<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Log.ascx.cs" Inherits="RockWeb.Blocks.Farm.Log" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-network-wired"></i> 
                    Log
                </h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue" OnClearFilterClick="rFilter_ClearFilterClick" FieldLayout="Custom">
                        <div class="row">
                            <div class="col-lg-6">
                                <Rock:SlidingDateRangePicker runat="server" ID="sdrpDateRange" Label="Date" />
                                <Rock:RockTextBox ID="tbNodeName" runat="server" Label="Node Name" />
                                <Rock:RockDropDownList ID="ddlSeverity" runat="server" Label="Severity" />
                            </div>
                            <div class="col-lg-6">
                                <Rock:RockDropDownList ID="ddlEventType" runat="server" Label="Event Type" />
                                <Rock:RockTextBox ID="tbWriterNodeName" runat="server" Label="Writer Node Name" />
                                <Rock:RockTextBox ID="tbText" runat="server" Label="Details" />
                            </div>
                        </div>
                    </Rock:GridFilter>
                    <Rock:Grid ID="gLog" runat="server" AllowSorting="true">
                        <Columns>
                            <Rock:RockBoundField DataField="DateTime" HeaderText="Date" SortExpression="DateTime" />
                            <Rock:RockBoundField DataField="WriterNodeName" HeaderText="Writer Node Name" SortExpression="WriterNodeName" />
                            <Rock:RockBoundField DataField="NodeName" HeaderText="Node Name" SortExpression="NodeName" />
                            <Rock:RockLiteralField HeaderText="Severity" ID="lSeverity" SortExpression="Severity" OnDataBound="lSeverity_DataBound" />
                            <Rock:RockLiteralField HeaderText="Type" ID="lEventType" SortExpression="EventType" OnDataBound="lEventType_DataBound" />
                            <Rock:RockBoundField DataField="Text" HeaderText="Details" SortExpression="Text" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>