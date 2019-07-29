<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ExceptionOccurrences.ascx.cs" Inherits="RockWeb.Blocks.Administration.ExceptionOccurrences" %>

<asp:UpdatePanel ID="upExceptionList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlExceptionOccurrences" CssClass="panel panel-block" runat="server">
            <asp:HiddenField ID="hfBaseExceptionID" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-bug"></i>
                    <asp:Literal ID="lDetailTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbBlockStatus" runat="server" />
                <asp:Panel ID="pnlList" runat="server">

                    <div class="row">
                        <div class="col-md-12">
                            <p>
                                Shows the group of exception log entries matching a specific type and partial description.
                            </p>
                            <asp:Literal ID="lblMainDetails" runat="server" />
                        </div>
                    </div>

                    <%-- Exceptions List --%>
                    <h4>Exception List</h4>

                    <div class="grid margin-t-md">
                        <Rock:GridFilter ID="fExceptionList" runat="server">
                            <Rock:RockDropDownList ID="ddlSite" runat="server" Label="Site" />
                            <Rock:PagePicker ID="ppPage" runat="server" Label="Page" />
                            <Rock:PersonPicker ID="ppUser" runat="server" Label="User" />
                            <Rock:SlidingDateRangePicker ID="sdpDateRange" runat="server" Label="Date Range" EnabledSlidingDateRangeTypes="Previous, Last, Current, DateRange" />
                        </Rock:GridFilter>
                        <Rock:Grid ID="gExceptionList" runat="server" AllowSorting="true" EmptyDataText="No Exceptions Found">
                            <Columns>
                                <Rock:RockBoundField DataField="Id" HeaderText="Id" SortExpression="Id" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Right" />
                                <Rock:DateTimeField DataField="CreatedDateTime" HeaderText="Date/Time" SortExpression="CreatedDateTime" HeaderStyle-HorizontalAlign="Center" />
                                <Rock:RockBoundField DataField="PageName" HeaderText="Page/URL" SortExpression="PageName" />
                                <Rock:RockBoundField DataField="FullName" HeaderText="Logged In User" SortExpression="FullName" />
                                <Rock:RockBoundField DataField="Description" HeaderText="Description" SortExpression="Description" HtmlEncode="false" ItemStyle-CssClass="wrap-contents" />
                                <Rock:LinkButtonField ID="lbShowDetail" Text="<i class='fa fa-file-alt'></i>" CssClass="btn btn-default btn-sm btn-square" OnClick="gExceptionList_ShowDetail" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </asp:Panel>
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
