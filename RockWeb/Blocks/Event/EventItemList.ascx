<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EventItemList.ascx.cs" Inherits="RockWeb.Blocks.Event.CalendarItemList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" runat="server">

            <div id="pnlEventCalendarItems" runat="server">

                <div class="panel panel-block">

                    <div class="panel-heading clearfix">
                        <h1 class="panel-title pull-left">
                            <i class="fa fa-calendar-check-o"></i>
                            Event Items
                        </h1>
                    </div>

                    <div class="panel-body">
                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue">
                                <Rock:DateRangePicker ID="drpDate" runat="server" Label="Next Start Date Range" />
                                <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status">
                                    <asp:ListItem Text="All" Value="" />
                                    <asp:ListItem Text="Active" Value="Active" />
                                    <asp:ListItem Text="Inactive" Value="Inactive" />
                                </Rock:RockDropDownList>
                                <Rock:RockDropDownList ID="ddlApprovalStatus" runat="server" Label="Approval Status">
                                    <asp:ListItem Text="All" Value="" />
                                    <asp:ListItem Text="Approved" Value="Approved" />
                                    <asp:ListItem Text="Not Approved" Value="Not Approved" />
                                </Rock:RockDropDownList>
                                <Rock:RockCheckBoxList ID="cblCampus" runat="server" Label="Campuses" DataTextField="Name" DataValueField="Id" />
                                <Rock:RockCheckBoxList ID="cblAudience" runat="server" Label="Audiences" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal" />
                                <asp:PlaceHolder ID="phAttributeFilters" runat="server" />
                            </Rock:GridFilter>
                            <Rock:Grid ID="gEventCalendarItems" runat="server" DisplayType="Full" AllowSorting="true" OnRowSelected="gEventCalendarItems_Edit">
                                <Columns>
                                    <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="EventItem.Name" />
                                    <Rock:RockBoundField DataField="Date" HeaderText="Next Start Date" SortExpression="Date" />
                                    <Rock:RockBoundField DataField="Campus" HeaderText="Campuses" HtmlEncode="false" />
                                    <Rock:RockBoundField DataField="Calendar" HeaderText="Calendars" HtmlEncode="false" />
                                    <Rock:RockBoundField DataField="Audience" HeaderText="Audiences" HtmlEncode="false" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
