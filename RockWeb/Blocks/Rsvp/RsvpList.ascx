<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RsvpList.ascx.cs" Inherits="RockWeb.Blocks.RSVP.RSVPList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" runat="server">

            <div id="pnlRSVPItems" runat="server">

                <div class="panel panel-block">

                    <div class="panel-heading">
                        <h1 class="panel-title pull-left">
                            <i class="fa fa-user-check"></i>
                            RSVP Occurrences
                        </h1>
                    </div>

                    <div class="panel-body">
                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue" OnApplyFilterClick="rFilter_ApplyFilterClick">
                                <Rock:DateRangePicker ID="drpDates" runat="server" Label="Date Range" />
                                <Rock:RockDropDownList ID="ddlLocation" runat="server" Label="Location" AppendDataBoundItems="true" DataValueField="Key" DataTextField="Value">
                                    <asp:ListItem Text="All" Value="" />
                                </Rock:RockDropDownList>
                                <Rock:RockDropDownList ID="ddlSchedule" runat="server" Label="Schedule" DataValueField="Key" DataTextField="Value"  />
                            </Rock:GridFilter>
                            <Rock:Grid ID="gRSVPItems" runat="server" DisplayType="Full" AllowSorting="true" OnRowSelected="gRSVPItems_RowSelected" DataKeyNames="Id" OnSorting="gRSVPItems_Sorting">
                                <Columns>
                                    <Rock:DateField DataField="OccurrenceDate" HeaderText="Date" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" SortExpression="OccurrenceDate" />
                                    <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" HtmlEncode="true" />
                                    <Rock:RockBoundField DataField="ScheduleName" HeaderText="Schedule" SortExpression="ScheduleName" HtmlEncode="false" />
                                    <Rock:RockBoundField DataField="LocationName" HeaderText="Location" SortExpression="LocationName" />
                                    <Rock:RockTemplateField>
                                        <ItemTemplate>
                                            <asp:HiddenField ID="hfOccurrenceDate" runat="server" Value='<%# Eval("OccurrenceDate") %>' />
                                            <asp:HiddenField ID="hfGroupId" runat="server" Value='<%# Eval("GroupId") %>' />
                                            <asp:HiddenField ID="hfScheduleId" runat="server" Value='<%# Eval("ScheduleId") %>' />
                                            <asp:HiddenField ID="hfLocationId" runat="server" Value='<%# Eval("LocationId") %>' />
                                            <div class="progress" style="width:145px;">
                                                <div class="progress-bar progress-bar-success" role="progressbar" style="width: <%# Eval("AcceptedPercentage") %>%"></div>
                                                <div class="progress-bar progress-bar-danger" role="progressbar" style="width: <%# Eval("DeclinedPercentage") %>%"></div>
                                            </div>
                                        </ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:RockBoundField DataField="InvitedCount" HeaderText="Invited" SortExpression="InvitedCount" />
                                    <Rock:RockBoundField DataField="AcceptedCount" HeaderText="Accepted" SortExpression="AcceptedCount" />
                                    <Rock:RockBoundField DataField="DeclinedCount" HeaderText="Declined" SortExpression="DeclinedCount" />
                                    <Rock:RockBoundField DataField="NoResponseCount" HeaderText="No Response" SortExpression="NoResponseCount" />
                                    <Rock:LinkButtonField CssClass="btn btn-default btn-sm" ID="btnDetails" Text="<i class='fa fa-user-check' title='Details'></i>" OnClick="btnDetails_Click" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
