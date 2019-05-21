<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BenevolenceRequestList.ascx.cs" Inherits="RockWeb.Blocks.Finance.BenevolenceRequestList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <asp:Panel ID="pnlView" runat="server" CssClass="">
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-paste"></i> Benevolence Requests</h1>
                </div>
                <div class="panel-body">

                    <div class="grid grid-panel">
                        <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue">
                            <Rock:DateRangePicker ID="drpDate" runat="server" Label="Date Range" />
                            <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" />
                            <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" />
                            <Rock:RockTextBox ID="tbGovernmentId" runat="server" Label="Government ID" />
                            <Rock:RockDropDownList ID="ddlCaseWorker" runat="server" Label="Case Worker" EnhanceForLongLists="true" />
                            <Rock:DefinedValuePicker ID="dvpResult" runat="server" Label="Result" DataTextField="Value" DataValueField="Id" />
                            <Rock:DefinedValuePicker ID="dvpStatus" runat="server" Label="Request Status" DataTextField="Value" DataValueField="Id" />
                            <Rock:CampusPicker ID="cpCampus" runat="server" Label="Campus" />
                            <asp:PlaceHolder ID="phAttributeFilters" runat="server" />
                        </Rock:GridFilter>
                        <Rock:Grid ID="gList" runat="server" DisplayType="Full" AllowSorting="true" OnRowDataBound="gList_RowDataBound" OnRowSelected="gList_Edit" ExportSource="DataSource">
                            <Columns>
                                <Rock:RockBoundField DataField="RequestDateTime" HeaderText="Date" DataFormatString="{0:d}" SortExpression="RequestDateTime" />
                                <Rock:RockBoundField DataField="Campus.Name" HeaderText="Campus" SortExpression="Campus.Name" />
                                <Rock:RockTemplateField SortExpression="RequestedByPersonAlias.Person.LastName, RequestedByPersonAlias.Person.NickName, LastName, FirstName" HeaderText="Name">
                                    <ItemTemplate>
                                        <asp:Literal ID="lName" runat="server" />
                                    </ItemTemplate>
                                </Rock:RockTemplateField>

                                <Rock:DefinedValueField DataField="ConnectionStatusValueId" HeaderText="Connection Status" SortExpression="ConnectionStatusValue.Value" ColumnPriority="DesktopSmall" />

                                <Rock:RockBoundField DataField="GovernmentId" HeaderText="Government ID" SortExpression="GovernmentId" ColumnPriority="DesktopLarge" />

                                <Rock:RockBoundField DataField="RequestText" HeaderText="Request" SortExpression="RequestText" />

                                <Rock:PersonField DataField="CaseWorkerPersonAlias.Person" SortExpression="CaseWorkerPersonAlias.Person.LastName, CaseWorkerPersonAlias.Person.NickName" HeaderText="Case Worker" ColumnPriority="Tablet" />

                                <Rock:RockBoundField DataField="ResultSummary" HeaderText="Result Summary" SortExpression="ResultSummary" ColumnPriority="DesktopLarge" />

                                <Rock:RockTemplateField HeaderText="Result Specifics" ColumnPriority="DesktopLarge">
                                    <ItemTemplate>
                                        <asp:Literal ID="lResults" runat="server" />
                                    </ItemTemplate>
                                </Rock:RockTemplateField>

                                <Rock:RockTemplateField SortExpression="RequestStatusValue.Value" HeaderText="Status">
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
