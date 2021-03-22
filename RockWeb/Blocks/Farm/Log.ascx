﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Log.ascx.cs" Inherits="RockWeb.Blocks.Farm.Log" %>

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
                                <Rock:RockTextBox ID="tbNodeName" runat="server" Label="Name" />
                                <Rock:RockDropDownList ID="ddlSeverity" runat="server" Label="Severity" />
                            </div>
                            <div class="col-lg-6">
                                <Rock:RockDropDownList ID="ddlEventType" runat="server" Label="Event Type" />
                                <Rock:RockTextBox ID="tbText" runat="server" Label="Text" />
                            </div>
                        </div>
                    </Rock:GridFilter>
                    <Rock:Grid ID="gLog" runat="server" AllowSorting="true">
                        <Columns>
                            <Rock:RockBoundField DataField="DateTime" HeaderText="Date" SortExpression="DateTime" />
                            <Rock:RockBoundField DataField="NodeName" HeaderText="Node" SortExpression="NodeName" />
                            <Rock:RockBoundField DataField="Severity" HeaderText="Severity" SortExpression="Severity" />
                            <Rock:RockBoundField DataField="EventType" HeaderText="Type" SortExpression="EventType" />
                            <Rock:RockBoundField DataField="Text" HeaderText="Text" SortExpression="Text" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>