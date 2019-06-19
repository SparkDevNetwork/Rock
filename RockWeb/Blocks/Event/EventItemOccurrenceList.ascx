﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EventItemOccurrenceList.ascx.cs" Inherits="RockWeb.Blocks.Event.EventItemOccurrenceList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" runat="server">

            <div id="pnlEventCalendarCampusItems" runat="server">

                <div class="panel panel-block">

                    <div class="panel-heading">
                        <h1 class="panel-title pull-left"><i class="fa fa-clock-o"></i> Event Occurrences</h1>
                    </div>

                    <div class="panel-body">
                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue">
                                <Rock:DateRangePicker ID="drpDate" runat="server" Label="Next Start Date Range" />
                                <Rock:RockCheckBoxList ID="cblCampus" runat="server" Label="Campuses" DataTextField="Name" DataValueField="Id" />
                                <Rock:RockTextBox ID="tbContact" runat="server" Label="Contact" />
                            </Rock:GridFilter>
                            <Rock:Grid ID="gCalendarItemOccurrenceList" runat="server" DisplayType="Full" AllowSorting="true" OnRowSelected="gCalendarItemOccurrenceList_RowSelected" RowItemText="Event Occurrence">
                                <Columns>
                                    <Rock:RockBoundField DataField="Campus" HeaderText="Campus" />
                                    <Rock:RockBoundField DataField="Date" HeaderText="Next Start Date" SortExpression="Date" />
                                    <Rock:RockBoundField DataField="Location" HeaderText="Location" />
                                    <asp:HyperLinkField HeaderText="Registration" DataTextField="RegistrationInstance" DataNavigateUrlFields="RegistrationInstanceId" />
                                    <asp:HyperLinkField HeaderText="Group" DataTextField="Group" DataNavigateUrlFields="GroupID" />
                                    <Rock:RockTemplateFieldUnselected HeaderText="Content Items" ColumnPriority="DesktopLarge">
                                        <ItemTemplate><%# Eval("ContentItems") %></ItemTemplate>
                                    </Rock:RockTemplateFieldUnselected>
                                    <Rock:RockBoundField DataField="Contact" HeaderText="Contact" />
                                    <Rock:RockBoundField DataField="Phone" HeaderText="Phone" ColumnPriority="Desktop" />
                                    <Rock:RockBoundField DataField="Email" HeaderText="Email" ColumnPriority="Desktop" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
