<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BenevolenceRequestList.ascx.cs" Inherits="RockWeb.Blocks.Finance.BenevolenceRequestList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlView" runat="server" CssClass="">
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-paste"></i>Benevolence Requests</h1>
                </div>
                <div class="panel-body">

                    <div class="grid grid-panel">
                        <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue">
                            <Rock:DateRangePicker ID="drpDate" runat="server" Label="Select from Date Range" />
                            <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" />
                            <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" />
                            <Rock:RockTextBox ID="tbGovernmentId" runat="server" Label="Government ID" />
                            <Rock:RockDropDownList ID="ddlCaseWorker" runat="server" Label="Case Worker" />
                            <Rock:RockDropDownList ID="ddlResult" runat="server" Label="Result" DataTextField="Value" DataValueField="Id" />
                            <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Request Status" DataTextField="Value" DataValueField="Id" />
                        </Rock:GridFilter>
                        <Rock:Grid ID="gList" runat="server" DisplayType="Full" AllowSorting="true" OnRowDataBound="gList_RowDataBound" OnRowSelected="gList_Edit">
                            <Columns>
                                <Rock:RockBoundField DataField="RequestDateTime" HeaderText="Date" DataFormatString="{0:d}" SortExpression="Date" />

                                <Rock:RockTemplateField SortExpression="Name" HeaderText="Name">
                                    <ItemTemplate>
                                        <asp:Literal ID="lName" runat="server" />
                                    </ItemTemplate>
                                </Rock:RockTemplateField>

                                <Rock:RockBoundField DataField="ConnectionStatusValue.Value" HeaderText="Connection Status" SortExpression="ConnectionStatus" ColumnPriority="DesktopSmall" />

                                <Rock:RockBoundField DataField="GovernmentId" HeaderText="Government ID" SortExpression="GovernmentId" ColumnPriority="DesktopLarge" />

                                <Rock:RockBoundField DataField="RequestText" HeaderText="Request" SortExpression="Request" />

                                <Rock:PersonField DataField="CaseWorkerPersonAlias.Person" SortExpression="Case Worker" HeaderText="Case Worker" ColumnPriority="Tablet" />

                                <Rock:RockBoundField DataField="ResultSummary" HeaderText="Result Summary" SortExpression="ResultSummary" ColumnPriority="DesktopLarge" />

                                <Rock:RockTemplateField SortExpression="ResultSpecifics" HeaderText="Result Specifics" ColumnPriority="DesktopLarge">
                                    <ItemTemplate>
                                        <asp:Literal ID="lResults" runat="server" />
                                    </ItemTemplate>
                                </Rock:RockTemplateField>

                                <Rock:RockTemplateField SortExpression="RequestStatus" HeaderText="Status">
                                    <ItemTemplate>
                                        <Rock:HighlightLabel ID="hlStatus" runat="server" />
                                    </ItemTemplate>
                                </Rock:RockTemplateField>

                                <Rock:CurrencyField DataField="TotalAmount" HeaderText="Total Amount" SortExpression="TotalAmount" />

                            </Columns>
                        </Rock:Grid>
                    </div>

                </div>
            </div>
        </asp:Panel>
        
        <div class="row">
            <div class="col-md-4 col-md-offset-8 margin-t-md">
                <asp:Panel ID="pnlSummary" runat="server" CssClass="panel panel-block">
                    <div class="panel-heading">
                        <h1 class="panel-title">Total Results</h1>
                    </div>
                    <div class="panel-body">
                        <asp:PlaceHolder ID="phSummary" runat="server" />
                    </div>
                </asp:Panel>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
