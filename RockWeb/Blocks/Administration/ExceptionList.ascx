<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ExceptionList.ascx.cs" Inherits="RockWeb.Blocks.Administraton.ExceptionList" %>

<script type="text/javascript">
    function confirmExceptionListClear() {
        return confirm("Are you sure that you want to clear all exceptions?");
    }
</script>

<asp:UpdatePanel ID="upExceptionList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlExceptionGroups" runat="server" CssClass="well" Visible="true">
            <div class="row-fluid">
                <div class="span12 clearExceptionsLink">
                    <asp:LinkButton ID="btnClearExceptions" runat="server" CssClass="btn" OnClientClick="return confirmExceptionListClear();" OnClick="btnClearExceptions_Click" CausesValidation="false">
                        <i class="icon-repeat"></i> Clear All Exceptions
                    </asp:LinkButton>
                </div>
            </div>
            <Rock:GridFilter ID="fExceptionList" runat="server">
                <Rock:LabeledDropDownList ID="ddlSite" runat="server" LabelText="Site" />
                <Rock:PagePicker ID="ppPage" runat="server" LabelText="Page" />
                <Rock:PersonPicker ID="ppUser" runat="server" LabelText="User" />
                <Rock:LabeledTextBox ID="txtStatusCode" runat="server" LabelText="Status Code" />
                <Rock:DatePicker ID="dpStartDate" runat="server"   LabelText="Start Date" />
                <Rock:DatePicker ID="dpEndDate" runat="server" LabelText="End Date" />
            </Rock:GridFilter>
            <Rock:Grid ID="gExceptionList" runat="server" AllowSorting="true" OnRowSelected="gExceptionList_RowSelected" EmptyDataText="No Exceptions Found">
                <Columns>
                    <Rock:DateField DataField="LastExceptionDate" HeaderText="Last Date" SortExpression="LastExceptionDate" />
                    <asp:BoundField DataField="SiteName" HeaderText="Site Name" SortExpression="SiteName" />
                    <asp:BoundField DataField="PageName" HeaderText="Page" SortExpression="PageName" />
                    <asp:BoundField DataField="Description" HeaderText="Exception" SortExpression="Description" />
                    <asp:BoundField DataField="TotalCount" HeaderText="Total Count" SortExpression="TotalCount" ItemStyle-HorizontalAlign="Right" />
                    <asp:BoundField DataField="SubsetCount" SortExpression="SubsetCount" ItemStyle-HorizontalAlign="Right" />
                </Columns>
            </Rock:Grid>
        </asp:Panel>
        <asp:Panel ID="pnlExceptionOccurrences" runat="server" CssClass="well" Visible="false">
            <asp:HiddenField ID="hfBaseExceptionID" runat="server" />
            <div class="banner">
                <h1><span class='first-word'>Exception </span> Occurrences</h1>
            </div>
            <asp:LinkButton ID="btnReturnToExceptionList" runat="server" CssClass="btn exceptionReturnToListButton" OnClick="btnReturnToExceptionList_Click"><i class="icon-angle-left"></i>&nbsp; Return to List</asp:LinkButton>
            <asp:Literal ID="lblMainDetails" runat="server" />
            <Rock:Grid ID="gExceptionOccurrences" runat="server" AllowSorting="true">
                <Columns>
                    <Rock:DateTimeField DataField="ExceptionDateTime" HeaderText="Date" SortExpression="ExceptionDateTime" />
                    <asp:BoundField DataField="FullName" HeaderText="Logged In User" SortExpression="FullName" />
                    <asp:BoundField DataField="Description" HeaderText="Exception" SortExpression="Description" />
                </Columns>
            </Rock:Grid>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
