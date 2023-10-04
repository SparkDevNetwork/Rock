<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MinistrySafeRequestList.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.MinistrySafe.MinistrySafeRequestList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-file-text-o"></i>Requests</h1>
                </div>
                <div class="panel-body">
                    <div class="grid grid-panel">
                        <Rock:GridFilter ID="fRequest" runat="server">
                            <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" />
                            <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" />
                            <Rock:DateRangePicker ID="drpRequestDates" runat="server" Label="Requested Date Range" />
                            <Rock:DateRangePicker ID="drpResponseDates" runat="server" Label="Completed Date Range" />
                        </Rock:GridFilter>

                        <Rock:Grid ID="gRequest" runat="server" AllowSorting="true" PersonIdField="PersonId">
                            <Columns>
                                <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" HtmlEncode="false" ItemStyle-HorizontalAlign="left" />
                                <Rock:DateField DataField="RequestDate" HeaderText="Requested" SortExpression="RequestDate" />
                                <Rock:DateField DataField="ResponseDate" HeaderText="Completed" SortExpression="ResponseDate" />
                                <Rock:LinkButtonField HeaderText="Report" Text="<i class='fa fa-file-pdf-o fa-lg'></i>" OnClick="gRequest_Data" HeaderStyle-CssClass="" ItemStyle-CssClass="" ColumnPriority="DesktopLarge" />
                                <Rock:EditField OnClick="gRequest_ViewWorkflow" IconCssClass="fa fa-cogs" ToolTip="View Workflow" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
