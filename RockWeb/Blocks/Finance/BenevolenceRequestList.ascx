<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BenevolenceRequestList.ascx.cs" Inherits="RockWeb.Blocks.Finance.BenevolenceRequestList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="col-md-12">
            <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

                <div class="panel-heading">
                    <h1 class="panel-title">Benevolence Requests</h1>
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
                                <asp:BoundField DataField="RequestDateTime" HeaderText="Date" DataFormatString="{0:d}" SortExpression="Date" />

                                <asp:TemplateField SortExpression="Name" HeaderText="Name">
                                    <ItemTemplate>
                                        <asp:Literal ID="lName" runat="server" />
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:BoundField DataField="ConnectionStatusValue.Value" HeaderText="Connection Status" SortExpression="ConnectionStatus" />

                                <asp:BoundField DataField="GovernmentId" HeaderText="Government ID" SortExpression="GovernmentId" />

                                <asp:BoundField DataField="RequestText" HeaderText="Request" SortExpression="Request" />

                                <asp:TemplateField SortExpression="Case Worker" HeaderText="Case Worker">
                                    <ItemTemplate>
                                        <asp:Literal ID="lCaseWorker" runat="server" />
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:BoundField DataField="ResultSummary" HeaderText="Result Summary" SortExpression="ResultSummary" />

                                <asp:TemplateField SortExpression="ResultSpecifics" HeaderText="Result Specifics">
                                    <ItemTemplate>
                                        <asp:Literal ID="lResults" runat="server" />
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:BoundField DataField="TotalAmount" HeaderText="Total Amount" DataFormatString="{0:C}" SortExpression="TotalAmount" />

                            </Columns>
                        </Rock:Grid>
                    </div>

                </div>

            </asp:Panel>
        </div>
        <div class="col-md-4">
            <asp:Panel ID="pnlSummary" runat="server" CssClass="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title">Total Results</h1>
                </div>
                <div class="panel-body">
                    <asp:PlaceHolder ID="phSummary" runat="server" />
                </div>
            </asp:Panel>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
