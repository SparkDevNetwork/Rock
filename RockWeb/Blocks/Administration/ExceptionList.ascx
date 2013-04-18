<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ExceptionList.ascx.cs" Inherits="RockWeb.Blocks.Administraton.ExceptionList" %>
<asp:UpdatePanel ID="upExceptionList" runat="server">
    <ContentTemplate>
        <Rock:GridFilter ID="fExceptionList" runat="server">
            <Rock:LabeledDropDownList ID="ddlSite" runat="server" LabelText="Site" />
            <Rock:PagePicker ID="ppPage" runat="server" LabelText="Page" />
            <Rock:PersonPicker ID="ppUser" runat="server" LabelText="User" />
            <Rock:LabeledTextBox ID="txtStatusCode" runat="server" LabelText="Status Code" />
            <Rock:DateTimePicker ID="dpStartDate" runat="server" LabelText="Start Date" />
            <Rock:DateTimePicker ID="dpEndDate" runat="server" LabelText="End Date" />
        </Rock:GridFilter>
        <Rock:Grid ID="gExceptionList" runat="server" AllowSorting="true" OnRowSelected="gExceptionList_RowSelected" EmptyDataText="No Exceptions Found">
            <Columns>
                <asp:BoundField DataField="LastDate" HeaderText="Last Date" DataFormatString="{0:d}" />
                <asp:BoundField DataField="SiteName" HeaderText="Site Name" />
                <asp:BoundField DataField="PageName" HeaderText="Page" />
                <asp:BoundField DataField="Description" HeaderText="Exception" />
                <asp:BoundField DataField="TotalCount" HeaderText="Total Count" />
                <asp:BoundField DataField="SubsetCount" HeaderText="7 Day Count" />
            </Columns>
        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>
