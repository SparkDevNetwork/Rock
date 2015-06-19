<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CalendarItemList.ascx.cs" Inherits="RockWeb.Blocks.Event.CalendarItemList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" runat="server">

            <div id="pnlEventCalendarItems" runat="server">

                <div class="panel panel-block">

                    <div class="panel-heading clearfix">
                        <h1 class="panel-title pull-left">
                            <i class="fa fa-calendar-o"></i>
                            Calendar Items
                        </h1>

                        <div class="panel-labels">
                            <Rock:HighlightLabel ID="hlSyncStatus" runat="server" LabelType="Info" Visible="false" Text="<i class='fa fa-exchange'></i>" />
                            &nbsp;
                        </div>
                    </div>

                    <div class="panel-body">
                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue">
                                <Rock:DateRangePicker ID="drpDate" runat="server" Label="Date Range" />
                                <Rock:RockCheckBox ID="cbActive" runat="server" Label="Active" Text="Only Show Active Items" RepeatDirection="Horizontal" />
                                <Rock:RockCheckBoxList ID="cblCampus" runat="server" Label="Campuses" DataTextField="Name" DataValueField="Id" />
                                <Rock:RockCheckBoxList ID="cblAudience" runat="server" Label="Audiences" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal" />
                                <asp:PlaceHolder ID="phAttributeFilters" runat="server" />
                            </Rock:GridFilter>
                            <Rock:Grid ID="gEventCalendarItems" runat="server" DisplayType="Full" AllowSorting="false" OnRowSelected="gEventCalendarItems_Edit">
                                <Columns>
                                    <Rock:RockBoundField DataField="Date" HeaderText="Start Date" />
                                    <Rock:RockBoundField DataField="Name" HeaderText="Name"/>
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