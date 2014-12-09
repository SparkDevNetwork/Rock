<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ExceptionList.ascx.cs" Inherits="RockWeb.Blocks.Administraton.ExceptionList" %>

<script type="text/javascript">
    function confirmExceptionListClear() {
        return confirm("Are you sure that you want to clear all exceptions?");
    }
</script>

<asp:UpdatePanel ID="upExceptionList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlExceptionGroups" CssClass="panel panel-block" runat="server" Visible="true">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-bug"></i> Exceptions Types</h1>
            </div>
            <div class="panel-body">

                <div class="row">
                    <div class="col-md-9">
                        <p>
                            The list below displays all of the types of errors that have occurred in the system in order by the last time it occurred. Counts are shown
                            of how many times each exception has occurred.
                        </p>
                    </div>

                    <div class="col-md-3">
                        <p class="clearfix">
                            <asp:LinkButton ID="btnClearExceptions" runat="server" CssClass="btn btn-action btn-sm pull-right" OnClientClick="return confirmExceptionListClear();" OnClick="btnClearExceptions_Click" CausesValidation="false">
                            <i class="fa fa-repeat"></i> Clear All Exceptions
                            </asp:LinkButton>
                        </p>
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-12">
                        <Rock:LineChart ID="lcExceptions" runat="server" DataSourceUrl="~/api/ExceptionLogs/GetChartData" SeriesNameUrl="" Title="Exception Count" ChartHeight="280px" />
                    </div>
                </div>

                <div class="grid margin-t-md">
                    <Rock:GridFilter ID="fExceptionList" runat="server">
                        <Rock:RockDropDownList ID="ddlSite" runat="server" Label="Site" />
                        <Rock:PagePicker ID="ppPage" runat="server" Label="Page" />
                        <Rock:PersonPicker ID="ppUser" runat="server" Label="User" />
                        <Rock:RockTextBox ID="txtStatusCode" runat="server" Label="Status Code" />
                        <Rock:DatePicker ID="dpStartDate" runat="server" Label="Start Date" />
                        <Rock:DatePicker ID="dpEndDate" runat="server" Label="End Date" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gExceptionList" runat="server" AllowSorting="true" OnRowSelected="gExceptionList_RowSelected" EmptyDataText="No Exceptions Found">
                        <Columns>
                            <Rock:DateField DataField="LastExceptionDate" HeaderText="Last Date" SortExpression="LastExceptionDate" />
                            <Rock:RockBoundField DataField="SiteName" HeaderText="Site Name" SortExpression="SiteName" />
                            <Rock:RockBoundField DataField="PageName" HeaderText="Page" SortExpression="PageName" />
                            <Rock:RockBoundField DataField="Description" HeaderText="Exception" SortExpression="Description" />
                            <Rock:RockBoundField DataField="TotalCount" HeaderText="Total Count" SortExpression="TotalCount" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                            <Rock:RockBoundField DataField="SubsetCount" SortExpression="SubsetCount" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

        </asp:Panel>

        <asp:Panel ID="pnlExceptionOccurrences" CssClass="panel panel-block" runat="server" Visible="false">
            <asp:HiddenField ID="hfBaseExceptionID" runat="server" />
            
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-bug"></i> <asp:Literal ID="lDetailTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <div class="row">
                    <div class="col-md-12">
                        <p>
                            Below is a list of each occurence of this error.
                        </p>
                        <asp:Literal ID="lblMainDetails" runat="server" />
                    </div>
                </div>

                <h4>Exception List</h4>

                <div class="grid margin-t-md">
                    <Rock:Grid ID="gExceptionOccurrences" runat="server" AllowSorting="true">
                        <Columns>
                            <Rock:DateField DataField="CreatedDateTime" HeaderText="Date" SortExpression="CreatedDateTime" />
                            <Rock:TimeField DataField="CreatedDateTime" HeaderText="Time" SortExpression="CreatedDateTime" />
                            <Rock:RockBoundField DataField="FullName" HeaderText="Logged In User" SortExpression="FullName" />
                            <Rock:RockBoundField DataField="Description" HeaderText="Exception" SortExpression="Description" />
                        </Columns>
                    </Rock:Grid>
                </div>

                <div class="row">
                    <div class="col-md-1">
                        <asp:LinkButton ID="btnReturnToList" runat="server" CssClass="btn btn-action btn-sm" OnClick="btnReturnToList_Click" CausesValidation="false">
                            <i class="fa fa-arrow-left"></i> Return to List
                        </asp:LinkButton>
                    </div>
                </div>

            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
